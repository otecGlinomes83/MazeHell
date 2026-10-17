using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using MazeHell.Configuration;

namespace MazeHell.Level
{
    public class LevelGeometryFactory
    {
        private const string QuarterRoomModuleId = "room_quarters";
        private const string GateRoomModuleId = "room_gate";
        private const int MaxPlacementAttemptsPerPosition = 64;
        private const float NavMeshSampleRadius = 2f;
        private const float NavMeshSnapHeight = 1f;
        private const float GridSampleInset = 1f;
        private const float PortCapThicknessMultiplier = 4f;
        private const float PortCapSizeMargin = 0.5f;
        private const float MazeLightHeightOffset = 0.5f;
        private const float GateGlowOffset = 2.5f;
        private const float GateGlowHeight = 3f;

        private LevelGenerationConfig _generationConfig;
        private LevelPresentationConfig _presentationConfig;

        public void Setup(LevelGenerationConfig generationConfig, LevelPresentationConfig presentationConfig)
        {
            if (generationConfig == null)
            {
                throw new ArgumentNullException(nameof(generationConfig));
            }

            if (presentationConfig == null)
            {
                throw new ArgumentNullException(nameof(presentationConfig));
            }

            _generationConfig = generationConfig;
            _presentationConfig = presentationConfig;
        }

        public MazeLayout PrepareLayout(int levelNumber, int seed)
        {
            ValidateBuildState(levelNumber);
            int side = ComputeSide(levelNumber);
            System.Random random = new System.Random(seed);
            float loopFraction = ComputeLoopFraction(random, side);
            return new MazeGenerator().Generate(side, seed, loopFraction, _generationConfig.DeadEndMinimum);
        }

        public GeneratedLevel Build(int levelNumber, int seed, Transform parent)
        {
            return Build(levelNumber, seed, parent, null);
        }

        public GeneratedLevel Build(int levelNumber, int seed, Transform parent, MazeLayout preparedLayout)
        {
            ValidateBuildState(levelNumber);

            int side = ComputeSide(levelNumber);
            System.Random random = new System.Random(seed);
            float loopFraction = ComputeLoopFraction(random, side);
            MazeLayout layout = preparedLayout;

            if (layout == null)
            {
                layout = new MazeGenerator().Generate(side, seed, loopFraction, _generationConfig.DeadEndMinimum);
            }

            BoundaryEndpoints endpoints = SelectBoundaryEndpoints(layout, side);
            layout = new MazeLayout(side, seed, endpoints.EntryCellIndex, endpoints.ExitCellIndex, layout.Cells);
            LevelAnchors anchors = ComputeAnchors(side, endpoints);

            GameObject root = CreateRoot(levelNumber, seed, parent);
            Transform geometryRoot = CreateGeometryRoot(root.transform);

            LevelGeometryBuilder builder = new LevelGeometryBuilder(_generationConfig.WallMaterial, _generationConfig.FloorMaterial, _generationConfig.CeilingMaterial);
            BuildMazeShell(geometryRoot, layout, builder, anchors, endpoints);
            BuildWestCorridor(geometryRoot, builder, anchors);
            BuildEastCorridor(geometryRoot, builder, anchors);
            BuildExitCorridor(geometryRoot, builder, anchors);
            BuildQuarterRoom(geometryRoot, builder, anchors);
            BuildGateRoom(geometryRoot, anchors);
            ExitGate exitGate = BuildExitGateActor(root.transform, anchors);
            BuildMazeLights(geometryRoot, anchors);

            MarkStaticTree(geometryRoot.gameObject);
            StaticBatchingUtility.Combine(geometryRoot.gameObject);

            NavMeshSurface navMeshSurface = BakeNavMesh(geometryRoot.gameObject);
            LevelResources resources = root.AddComponent<LevelResources>();
            resources.Setup(builder.GetRuntimeMeshes(), navMeshSurface.navMeshData);
            Vector3 navigationSpawn = SnapToNavMesh(anchors.SpawnPosition, seed);
            Vector3[] enemySpawnPositions = SampleReachablePositions(navigationSpawn, ComputeEnemyCount(levelNumber), _generationConfig.EnemyMinSpawnDistance, _generationConfig.EnemyMinSpacing, random, anchors, levelNumber, seed, "enemy");
            Vector3[] pickupPositions = SampleReachablePositions(navigationSpawn, _generationConfig.HealPickupCount, _generationConfig.PickupMinSpawnDistance, _generationConfig.EnemyMinSpacing, random, anchors, levelNumber, seed, "pickup");

            return new GeneratedLevel(root, geometryRoot, navigationSpawn, Quaternion.LookRotation(Vector3.right), layout, exitGate, anchors.CorridorStart, anchors.CorridorEnd, Vector3.right, enemySpawnPositions, pickupPositions, navMeshSurface);
        }

        private void ValidateBuildState(int levelNumber)
        {
            if (_generationConfig == null || _presentationConfig == null)
            {
                throw new InvalidOperationException($"{nameof(LevelGeometryFactory)}: call {nameof(Setup)} before {nameof(Build)}.");
            }

            if (levelNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(levelNumber), levelNumber, "Level number starts at 1.");
            }
        }

        private int ComputeSide(int levelNumber)
        {
            int side = _generationConfig.LogicalSideStart + (levelNumber - 1) * _generationConfig.LogicalSideGrowthPerLevel;
            return Mathf.Min(side, _generationConfig.LogicalSideCap);
        }

        private float ComputeLoopFraction(System.Random random, int side)
        {
            int desiredLoopCount = random.Next(_generationConfig.LoopEdgeCountMin, _generationConfig.LoopEdgeCountMax + 1);
            int bridgeEdgeCount = (side - 1) * (side - 1);
            return Mathf.Clamp01(desiredLoopCount / (float)bridgeEdgeCount);
        }

        private int ComputeEnemyCount(int levelNumber)
        {
            int enemyCount = _generationConfig.EnemyCountStart + (levelNumber - 1) * _generationConfig.EnemyCountGrowthPerLevel;
            return Mathf.Min(enemyCount, _generationConfig.EnemyCountCap);
        }

        private BoundaryEndpoints SelectBoundaryEndpoints(MazeLayout layout, int side)
        {
            BoundaryEndpoints endpoints = new BoundaryEndpoints();
            endpoints.EntryCellIndex = 0;
            endpoints.ExitCellIndex = layout.Size - 1;
            endpoints.GraphDistance = -1;

            for (int entryIndex = 0; entryIndex < layout.Size; entryIndex++)
            {
                if (layout.Cells[entryIndex].Coordinates.x > 0)
                {
                    continue;
                }

                for (int exitIndex = 0; exitIndex < layout.Size; exitIndex++)
                {
                    if (layout.Cells[exitIndex].Coordinates.x < side - 1)
                    {
                        continue;
                    }

                    int graphDistance = layout.GetDistance(entryIndex, exitIndex);

                    if (graphDistance > endpoints.GraphDistance)
                    {
                        endpoints.GraphDistance = graphDistance;
                        endpoints.EntryCellIndex = entryIndex;
                        endpoints.ExitCellIndex = exitIndex;
                    }
                }
            }

            return endpoints;
        }

        private LevelAnchors ComputeAnchors(int side, BoundaryEndpoints endpoints)
        {
            ModuleCatalogEntry quarterRoom = GetModuleEntry(QuarterRoomModuleId);
            ModuleCatalogEntry gateRoom = GetModuleEntry(GateRoomModuleId);
            float quarterHalfWidth = quarterRoom.FootprintSize.x * 0.5f;
            float quarterHalfDepth = quarterRoom.FootprintSize.z * 0.5f;
            float gateHalfWidth = gateRoom.FootprintSize.x * 0.5f;
            float gateHalfDepth = gateRoom.FootprintSize.z * 0.5f;

            LevelAnchors anchors = new LevelAnchors();
            anchors.GridWidth = side * _generationConfig.CellSize;
            anchors.EntryAxisZ = (endpoints.EntryCellIndex / side + 0.5f) * _generationConfig.CellSize;
            anchors.ExitAxisZ = (endpoints.ExitCellIndex / side + 0.5f) * _generationConfig.CellSize;
            anchors.QuarterRoomCenterX = -(_generationConfig.ConnectorLength + quarterHalfWidth);
            anchors.QuarterRoomHalfWidth = quarterHalfWidth;
            anchors.QuarterRoomHalfDepth = quarterHalfDepth;
            anchors.GateRoomCenterX = anchors.GridWidth + _generationConfig.ConnectorLength + gateHalfDepth;
            anchors.GateRoomHalfWidth = gateHalfWidth;
            anchors.GateRoomHalfDepth = gateHalfDepth;
            anchors.GateOpeningX = anchors.GateRoomCenterX + gateHalfDepth;
            anchors.SpawnPosition = new Vector3(anchors.QuarterRoomCenterX, 0f, anchors.EntryAxisZ);
            anchors.CorridorStart = new Vector3(anchors.GateOpeningX, 0f, anchors.ExitAxisZ);
            anchors.CorridorEnd = anchors.CorridorStart + Vector3.right * (_presentationConfig.DarkCorridorLength - _presentationConfig.TransitionEndInset);
            return anchors;
        }

        private ModuleCatalogEntry GetModuleEntry(string moduleId)
        {
            ModuleCatalogEntry[] moduleCatalog = _generationConfig.ModuleCatalog;

            if (moduleCatalog == null)
            {
                throw new InvalidOperationException($"{nameof(LevelGenerationConfig)}: module catalog is empty, cannot resolve '{moduleId}'.");
            }

            for (int entryIndex = 0; entryIndex < moduleCatalog.Length; entryIndex++)
            {
                ModuleCatalogEntry entry = moduleCatalog[entryIndex];

                if (entry == null)
                {
                    continue;
                }

                if (entry.ModuleId == moduleId)
                {
                    if (entry.Prefab == null)
                    {
                        continue;
                    }

                    return entry;
                }
            }

            throw new InvalidOperationException($"{nameof(LevelGenerationConfig)}: module catalog has no usable entry '{moduleId}'.");
        }

        private GameObject CreateRoot(int levelNumber, int seed, Transform parent)
        {
            GameObject root = new GameObject($"Level_{levelNumber}_Seed_{seed}");

            if (parent == null)
            {
                return root;
            }

            root.transform.SetParent(parent, false);
            return root;
        }

        private Transform CreateGeometryRoot(Transform root)
        {
            GameObject geometryRoot = new GameObject("Geometry");
            geometryRoot.transform.SetParent(root, false);
            return geometryRoot.transform;
        }

        private void BuildMazeShell(Transform geometryRoot, MazeLayout layout, LevelGeometryBuilder builder, LevelAnchors anchors, BoundaryEndpoints endpoints)
        {
            float cellSize = _generationConfig.CellSize;
            float wallThickness = _generationConfig.WallThickness;
            float height = _generationConfig.CeilingHeight;
            float floorThickness = _generationConfig.FloorThickness;
            int side = layout.Side;

            builder.CreateFloorSlab(geometryRoot, "MazeFloor", new Vector3(anchors.GridWidth * 0.5f, -floorThickness * 0.5f, anchors.GridWidth * 0.5f), new Vector3(anchors.GridWidth, floorThickness, anchors.GridWidth), cellSize);
            builder.CreateCeilingSlab(geometryRoot, "MazeCeiling", new Vector3(anchors.GridWidth * 0.5f, height + floorThickness * 0.5f, anchors.GridWidth * 0.5f), new Vector3(anchors.GridWidth, floorThickness, anchors.GridWidth), cellSize);

            for (int cellIndex = 0; cellIndex < layout.Size; cellIndex++)
            {
                MazeCell cell = layout.Cells[cellIndex];
                int cellCoordinateX = cell.Coordinates.x;
                int cellCoordinateY = cell.Coordinates.y;

                if (cellCoordinateY == 0 && cell.HasPassage(MazeDirection.South) == false)
                {
                    Vector3 center = new Vector3(cellCoordinateX * cellSize + cellSize * 0.5f, height * 0.5f, 0f);
                    builder.CreateBox(geometryRoot, $"MazeWall_S_{cellIndex}", center, new Vector3(cellSize + wallThickness, height, wallThickness));
                }

                if (cellCoordinateY == side - 1 && cell.HasPassage(MazeDirection.North) == false)
                {
                    Vector3 center = new Vector3(cellCoordinateX * cellSize + cellSize * 0.5f, height * 0.5f, anchors.GridWidth);
                    builder.CreateBox(geometryRoot, $"MazeWall_N_{cellIndex}", center, new Vector3(cellSize + wallThickness, height, wallThickness));
                }

                if (cellCoordinateX == 0 && cell.HasPassage(MazeDirection.West) == false && cellIndex == endpoints.EntryCellIndex == false)
                {
                    Vector3 center = new Vector3(0f, height * 0.5f, cellCoordinateY * cellSize + cellSize * 0.5f);
                    builder.CreateBox(geometryRoot, $"MazeWall_W_{cellIndex}", center, new Vector3(wallThickness, height, cellSize + wallThickness));
                }

                if (cell.HasPassage(MazeDirection.East) == false)
                {
                    if (cellCoordinateX < side - 1)
                    {
                        Vector3 center = new Vector3((cellCoordinateX + 1) * cellSize, height * 0.5f, cellCoordinateY * cellSize + cellSize * 0.5f);
                        builder.CreateBox(geometryRoot, $"MazeWall_E_{cellIndex}", center, new Vector3(wallThickness, height, cellSize + wallThickness));
                    }
                    else if (cellIndex == endpoints.ExitCellIndex == false)
                    {
                        Vector3 center = new Vector3(anchors.GridWidth, height * 0.5f, cellCoordinateY * cellSize + cellSize * 0.5f);
                        builder.CreateBox(geometryRoot, $"MazeWall_E_{cellIndex}", center, new Vector3(wallThickness, height, cellSize + wallThickness));
                    }
                }

                if (cellCoordinateY < side - 1 && cell.HasPassage(MazeDirection.North) == false)
                {
                    Vector3 center = new Vector3(cellCoordinateX * cellSize + cellSize * 0.5f, height * 0.5f, (cellCoordinateY + 1) * cellSize);
                    builder.CreateBox(geometryRoot, $"MazeWall_N_{cellIndex}", center, new Vector3(cellSize + wallThickness, height, wallThickness));
                }
            }
        }

        private void BuildWestCorridor(Transform geometryRoot, LevelGeometryBuilder builder, LevelAnchors anchors)
        {
            float wallThickness = _generationConfig.WallThickness;
            float xBegin = -_generationConfig.ConnectorLength - wallThickness;
            float length = _generationConfig.ConnectorLength + wallThickness * 2f;

            if (TryCreateConnector(geometryRoot, "WestCorridor", -_generationConfig.ConnectorLength * 0.5f, anchors.EntryAxisZ) == false)
            {
                builder.CreateCorridorShell(geometryRoot, "WestCorridor", xBegin, length, anchors.EntryAxisZ,
                    _generationConfig.CorridorWidth, _generationConfig.CorridorHeight, _generationConfig.FloorThickness,
                    wallThickness, _generationConfig.CellSize, false);
            }
            BuildGridJunction(geometryRoot, builder, anchors, "WestJunction", 0f, anchors.EntryAxisZ);
        }

        private void BuildEastCorridor(Transform geometryRoot, LevelGeometryBuilder builder, LevelAnchors anchors)
        {
            float wallThickness = _generationConfig.WallThickness;
            float xBegin = anchors.GridWidth - wallThickness;
            float length = _generationConfig.ConnectorLength + wallThickness * 2f;

            if (TryCreateConnector(geometryRoot, "EastCorridor", anchors.GridWidth + _generationConfig.ConnectorLength * 0.5f,
                anchors.ExitAxisZ) == false)
            {
                builder.CreateCorridorShell(geometryRoot, "EastCorridor", xBegin, length, anchors.ExitAxisZ,
                    _generationConfig.CorridorWidth, _generationConfig.CorridorHeight, _generationConfig.FloorThickness,
                    wallThickness, _generationConfig.CellSize, false);
            }
            BuildGridJunction(geometryRoot, builder, anchors, "EastJunction", anchors.GridWidth, anchors.ExitAxisZ);
        }

        private bool TryCreateConnector(Transform parent, string objectName, float centerX, float centerZ)
        {
            ModuleCatalogEntry[] entries = _generationConfig.ModuleCatalog;

            for (int i = 0; i < entries.Length; i++)
            {
                ModuleCatalogEntry entry = entries[i];

                if (entry == null)
                {
                    continue;
                }

                if (entry.ModuleId != "corridor_straight" || entry.Prefab == null ||
                    Mathf.Abs(entry.FootprintSize.z - _generationConfig.ConnectorLength) > Mathf.Epsilon)
                {
                    continue;
                }

                GameObject instance = UnityEngine.Object.Instantiate(entry.Prefab, parent);
                instance.name = objectName;
                instance.transform.localPosition = new Vector3(centerX, 0f, centerZ);
                instance.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                return true;
            }

            return false;
        }

        private void BuildGridJunction(Transform geometryRoot, LevelGeometryBuilder builder, LevelAnchors anchors, string namePrefix, float junctionX, float axisZ)
        {
            float cellSize = _generationConfig.CellSize;
            float wallThickness = _generationConfig.WallThickness;
            float corridorWidth = _generationConfig.CorridorWidth;
            float corridorHeight = _generationConfig.CorridorHeight;
            float mazeHeight = _generationConfig.CeilingHeight;
            float flankWidth = (corridorWidth - cellSize) * 0.5f;

            if (flankWidth > 0f)
            {
                float flankOffsetZ = cellSize * 0.5f + flankWidth * 0.5f;

                builder.CreateBox(geometryRoot, $"{namePrefix}_CapSouth", new Vector3(junctionX, corridorHeight * 0.5f, axisZ - flankOffsetZ), new Vector3(wallThickness * 2f, corridorHeight, flankWidth));
                builder.CreateBox(geometryRoot, $"{namePrefix}_CapNorth", new Vector3(junctionX, corridorHeight * 0.5f, axisZ + flankOffsetZ), new Vector3(wallThickness * 2f, corridorHeight, flankWidth));
            }

            if (corridorHeight > mazeHeight)
            {
                float lintelHeight = corridorHeight - mazeHeight;
                builder.CreateBox(geometryRoot, $"{namePrefix}_Lintel", new Vector3(junctionX, mazeHeight + lintelHeight * 0.5f, axisZ), new Vector3(wallThickness, lintelHeight, corridorWidth));
            }
        }

        private void BuildExitCorridor(Transform geometryRoot, LevelGeometryBuilder builder, LevelAnchors anchors)
        {
            Material blackMaterial = _generationConfig.ExitCorridorMaterial;

            if (blackMaterial == null)
            {
                throw new InvalidOperationException($"{nameof(LevelGenerationConfig)}: exit corridor material is not assigned.");
            }

            float wallThickness = _generationConfig.WallThickness;
            float floorThickness = _generationConfig.FloorThickness;
            float width = _generationConfig.GateOpeningWidth;
            float height = _generationConfig.RoomHeight;
            float length = _presentationConfig.DarkCorridorLength;
            float xBegin = anchors.GateOpeningX;
            float centerLineX = xBegin + length * 0.5f;

            builder.CreateSlabWithMaterial(geometryRoot, "ExitCorridor_Floor", new Vector3(centerLineX, -floorThickness * 0.5f, anchors.ExitAxisZ), new Vector3(length, floorThickness, width), _generationConfig.CellSize, blackMaterial);
            builder.CreateSlabWithMaterial(geometryRoot, "ExitCorridor_Ceiling", new Vector3(centerLineX, height + floorThickness * 0.5f, anchors.ExitAxisZ), new Vector3(length, floorThickness, width + wallThickness * 2f), _generationConfig.CellSize, blackMaterial);
            builder.CreateBoxWithMaterial(geometryRoot, "ExitCorridor_WallSouth", new Vector3(centerLineX, height * 0.5f, anchors.ExitAxisZ - (width * 0.5f + wallThickness * 0.5f)), new Vector3(length, height, wallThickness), blackMaterial);
            builder.CreateBoxWithMaterial(geometryRoot, "ExitCorridor_WallNorth", new Vector3(centerLineX, height * 0.5f, anchors.ExitAxisZ + (width * 0.5f + wallThickness * 0.5f)), new Vector3(length, height, wallThickness), blackMaterial);
            builder.CreateBoxWithMaterial(geometryRoot, "ExitCorridor_EndCap", new Vector3(xBegin + length + wallThickness * 0.5f, height * 0.5f, anchors.ExitAxisZ), new Vector3(wallThickness, height, width + wallThickness * 2f), blackMaterial);
        }

        private void BuildQuarterRoom(Transform geometryRoot, LevelGeometryBuilder builder, LevelAnchors anchors)
        {
            ModuleCatalogEntry quarterRoom = GetModuleEntry(QuarterRoomModuleId);
            GameObject roomInstance = UnityEngine.Object.Instantiate(quarterRoom.Prefab, geometryRoot);
            roomInstance.name = "RoomQuartersArena";
            roomInstance.transform.position = new Vector3(anchors.QuarterRoomCenterX, 0f, anchors.EntryAxisZ);
            roomInstance.transform.rotation = Quaternion.identity;

            float capThickness = _generationConfig.WallThickness * PortCapThicknessMultiplier;
            float capWidth = _generationConfig.RoomDoorWidth + PortCapSizeMargin;
            float capHeight = _generationConfig.RoomDoorHeight + PortCapSizeMargin;
            float capCenterY = _generationConfig.RoomDoorHeight * 0.5f;

            builder.CreateBox(roomInstance.transform, "PortCapNorth", new Vector3(0f, capCenterY, anchors.QuarterRoomHalfDepth), new Vector3(capWidth, capHeight, capThickness));
            builder.CreateBox(roomInstance.transform, "PortCapSouth", new Vector3(0f, capCenterY, -anchors.QuarterRoomHalfDepth), new Vector3(capWidth, capHeight, capThickness));
            builder.CreateBox(roomInstance.transform, "PortCapWest", new Vector3(-anchors.QuarterRoomHalfWidth, capCenterY, 0f), new Vector3(capThickness, capHeight, capWidth));
        }

        private void BuildGateRoom(Transform geometryRoot, LevelAnchors anchors)
        {
            ModuleCatalogEntry gateRoom = GetModuleEntry(GateRoomModuleId);
            GameObject roomInstance = UnityEngine.Object.Instantiate(gateRoom.Prefab, geometryRoot);
            roomInstance.name = "RoomGate";
            roomInstance.transform.position = new Vector3(anchors.GateRoomCenterX, 0f, anchors.ExitAxisZ);
            roomInstance.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }

        private ExitGate BuildExitGateActor(Transform root, LevelAnchors anchors)
        {
            GameObject gateLeafPrefab = _presentationConfig.GateLeafPrefab;

            if (gateLeafPrefab == null)
            {
                throw new InvalidOperationException($"{nameof(LevelPresentationConfig)}: gate leaf prefab is not assigned.");
            }

            GameObject gateActor = new GameObject("ExitGateActor");
            gateActor.transform.SetParent(root, false);
            gateActor.transform.position = new Vector3(anchors.GateOpeningX, 0f, anchors.ExitAxisZ);
            gateActor.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            GameObject leafInstance = UnityEngine.Object.Instantiate(gateLeafPrefab, gateActor.transform);
            leafInstance.name = "GateLeaf";
            leafInstance.transform.localPosition = Vector3.zero;

            GameObject glowObject = new GameObject("GateGlow");
            glowObject.transform.SetParent(gateActor.transform, false);
            Vector3 glowWorldPosition = new Vector3(anchors.GateOpeningX - GateGlowOffset, GateGlowHeight, anchors.ExitAxisZ);
            glowObject.transform.localPosition = gateActor.transform.InverseTransformPoint(glowWorldPosition);

            Light glow = glowObject.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = _presentationConfig.ExitGlowColor;
            glow.intensity = _presentationConfig.ExitGlowIntensity;
            glow.range = _presentationConfig.LightRange;
            glow.shadows = LightShadows.None;
            glow.enabled = false;

            ExitGate exitGate = gateActor.AddComponent<ExitGate>();
            exitGate.Setup(leafInstance.transform, glow, _presentationConfig);
            return exitGate;
        }

        private void BuildMazeLights(Transform geometryRoot, LevelAnchors anchors)
        {
            int lightCount = _presentationConfig.MaxSimultaneousLights;

            if (lightCount == 0)
            {
                return;
            }

            int gridLightCount = Mathf.Max(1, lightCount - 4);
            int columns = Mathf.CeilToInt(Mathf.Sqrt(gridLightCount));
            int rows = Mathf.CeilToInt(gridLightCount / (float)columns);

            for (int lightIndex = 0; lightIndex < lightCount; lightIndex++)
            {
                int gridIndex = Mathf.Max(0, lightIndex - 4);
                int column = gridIndex % columns;
                int row = gridIndex / columns;
                float positionX = (column + 0.5f) / columns * anchors.GridWidth;
                float positionZ = (row + 0.5f) / rows * anchors.GridWidth;
                float height = _generationConfig.CeilingHeight - MazeLightHeightOffset;

                switch (lightIndex)
                {
                    case 0:
                        positionX = anchors.QuarterRoomCenterX;
                        positionZ = anchors.EntryAxisZ;
                        height = _generationConfig.RoomHeight - MazeLightHeightOffset;
                        break;
                    case 1:
                        positionX = anchors.GateRoomCenterX;
                        positionZ = anchors.ExitAxisZ;
                        height = _generationConfig.RoomHeight - MazeLightHeightOffset;
                        break;
                    case 2:
                        positionX = -_generationConfig.ConnectorLength * 0.5f;
                        positionZ = anchors.EntryAxisZ;
                        height = _generationConfig.CorridorHeight - MazeLightHeightOffset;
                        break;
                    case 3:
                        positionX = anchors.GridWidth + _generationConfig.ConnectorLength * 0.5f;
                        positionZ = anchors.ExitAxisZ;
                        height = _generationConfig.CorridorHeight - MazeLightHeightOffset;
                        break;
                }

                GameObject lightObject = new GameObject($"MazeLight_{lightIndex}");
                lightObject.transform.SetParent(geometryRoot, false);
                lightObject.transform.position = new Vector3(positionX, height, positionZ);

                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = _presentationConfig.LightColor;
                light.intensity = _presentationConfig.LightIntensity;
                light.range = _presentationConfig.LightRange;
                light.shadows = LightShadows.None;
            }
        }

        private void MarkStaticTree(GameObject rootObject)
        {
            rootObject.isStatic = true;
            Transform rootTransform = rootObject.transform;

            for (int childIndex = 0; childIndex < rootTransform.childCount; childIndex++)
            {
                MarkStaticTree(rootTransform.GetChild(childIndex).gameObject);
            }
        }

        private NavMeshSurface BakeNavMesh(GameObject geometryRoot)
        {
            NavMeshSurface navMeshSurface = geometryRoot.AddComponent<NavMeshSurface>();
            navMeshSurface.collectObjects = CollectObjects.Children;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            navMeshSurface.BuildNavMesh();
            return navMeshSurface;
        }

        private Vector3 SnapToNavMesh(Vector3 position, int seed)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(position, out hit, NavMeshSampleRadius * 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            throw new InvalidOperationException($"{nameof(LevelGeometryFactory)}: player spawn is not on NavMesh (seed {seed}).");
        }

        private Vector3[] SampleReachablePositions(Vector3 origin, int targetCount, float minDistanceFromOrigin, float minSpacing, System.Random random, LevelAnchors anchors, int levelNumber, int seed, string purpose)
        {
            List<Vector3> acceptedPositions = new List<Vector3>(targetCount);
            NavMeshPath path = new NavMeshPath();
            int attemptLimit = MaxPlacementAttemptsPerPosition * targetCount;
            int attempts = 0;

            while (acceptedPositions.Count < targetCount)
            {
                if (attempts >= attemptLimit)
                {
                    throw new InvalidOperationException($"{nameof(LevelGeometryFactory)}: placed {acceptedPositions.Count} of {targetCount} {purpose} positions after {attempts} attempts (level {levelNumber}, seed {seed}).");
                }

                attempts += 1;
                Vector3 candidate = SampleGridPoint(random, anchors);

                if (acceptedPositions.Count == 0)
                {
                    candidate = SampleRoomPoint(random, anchors.QuarterRoomCenterX, anchors.EntryAxisZ,
                        anchors.QuarterRoomHalfWidth, anchors.QuarterRoomHalfDepth);
                }
                else if (purpose == "enemy" && acceptedPositions.Count == 1)
                {
                    candidate = SampleRoomPoint(random, anchors.GateRoomCenterX, anchors.ExitAxisZ,
                        anchors.GateRoomHalfDepth, anchors.GateRoomHalfWidth);
                }
                NavMeshHit hit;

                if (NavMesh.SamplePosition(candidate, out hit, NavMeshSampleRadius, NavMesh.AllAreas) == false)
                {
                    continue;
                }

                if (IsPositionAcceptable(hit.position, origin, minDistanceFromOrigin, minSpacing, acceptedPositions) == false)
                {
                    continue;
                }

                if (NavMesh.CalculatePath(origin, hit.position, NavMesh.AllAreas, path) == false)
                {
                    continue;
                }

                if (path.status == NavMeshPathStatus.PathComplete == false)
                {
                    continue;
                }

                acceptedPositions.Add(hit.position);
            }

            return acceptedPositions.ToArray();
        }

        private bool IsPositionAcceptable(Vector3 position, Vector3 origin, float minDistanceFromOrigin, float minSpacing, List<Vector3> acceptedPositions)
        {
            if (Vector3.Distance(position, origin) < minDistanceFromOrigin)
            {
                return false;
            }

            for (int positionIndex = 0; positionIndex < acceptedPositions.Count; positionIndex++)
            {
                if (Vector3.Distance(position, acceptedPositions[positionIndex]) < minSpacing)
                {
                    return false;
                }
            }

            return true;
        }

        private Vector3 SampleRoomPoint(System.Random random, float centerX, float centerZ, float halfWidth, float halfDepth)
        {
            float positionX = centerX + ((float)random.NextDouble() * 2f - 1f) * (halfWidth - GridSampleInset);
            float positionZ = centerZ + ((float)random.NextDouble() * 2f - 1f) * (halfDepth - GridSampleInset);
            return new Vector3(positionX, NavMeshSnapHeight, positionZ);
        }

        private Vector3 SampleGridPoint(System.Random random, LevelAnchors anchors)
        {
            float spanX = anchors.GridWidth - GridSampleInset * 2f;
            float spanZ = anchors.GridWidth - GridSampleInset * 2f;
            float positionX = (float)random.NextDouble() * spanX + GridSampleInset;
            float positionZ = (float)random.NextDouble() * spanZ + GridSampleInset;

            return new Vector3(positionX, NavMeshSnapHeight, positionZ);
        }

        private struct BoundaryEndpoints
        {
            public int EntryCellIndex;
            public int ExitCellIndex;
            public int GraphDistance;
        }

        private struct LevelAnchors
        {
            public float GridWidth;
            public float EntryAxisZ;
            public float ExitAxisZ;
            public float QuarterRoomCenterX;
            public float QuarterRoomHalfWidth;
            public float QuarterRoomHalfDepth;
            public float GateRoomCenterX;
            public float GateRoomHalfWidth;
            public float GateRoomHalfDepth;
            public float GateOpeningX;
            public Vector3 SpawnPosition;
            public Vector3 CorridorStart;
            public Vector3 CorridorEnd;
        }
    }
}
