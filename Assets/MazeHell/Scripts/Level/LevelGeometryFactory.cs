using System;
using System.Collections.Generic;
using MazeHell.Configuration;
using UnityEngine;
using UnityEngine.AI;

namespace MazeHell.Level
{
    public class LevelGeometryFactory
    {
        private const int PlacementAttemptsPerSpawn = 128;
        private const float NavMeshSampleRadius = 2f;
        private const float SpawnInset = 2f;
        private const float PortTolerance = 0.01f;

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
            int loopCount = random.Next(_generationConfig.LoopEdgeCountMin, _generationConfig.LoopEdgeCountMax + 1);
            float loopFraction = Mathf.Clamp01(loopCount / (float)((side - 1) * (side - 1)));
            MazeLayout layout = new MazeGenerator().Generate(side, seed, loopFraction, _generationConfig.DeadEndMinimum);
            return SelectBoundaryEndpoints(layout);
        }

        public GeneratedLevel Build(int levelNumber, int seed, Transform parent)
        {
            return Build(levelNumber, seed, parent, null);
        }

        public GeneratedLevel Build(int levelNumber, int seed, Transform parent, MazeLayout preparedLayout)
        {
            ValidateBuildState(levelNumber);
            MazeLayout layout = preparedLayout;

            if (layout == null)
            {
                layout = PrepareLayout(levelNumber, seed);
            }

            if (layout.Seed != seed || layout.Side != ComputeSide(levelNumber) || layout.IsConnected() == false)
            {
                throw new ArgumentException("Prepared layout does not match the requested level.", nameof(preparedLayout));
            }

            if (preparedLayout != null)
            {
                layout = SelectBoundaryEndpoints(layout);
            }
            LevelRoot root = UnityEngine.Object.Instantiate(_generationConfig.LevelPrefab, Vector3.zero, Quaternion.identity, parent);
            root.name = $"Level_{levelNumber}_Seed_{seed}";

            try
            {
                return PopulateLevel(root, layout, levelNumber, seed);
            }
            catch
            {
                root.gameObject.SetActive(false);

                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(root.gameObject);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(root.gameObject);
                }

                throw;
            }
        }

        private GeneratedLevel PopulateLevel(LevelRoot root, MazeLayout layout, int levelNumber, int seed)
        {
            if (root.GeometryRoot == null || root.Navigation == null || root.Resources == null)
            {
                throw new InvalidOperationException("LevelRoot prefab has unassigned geometry, navigation or resource bindings.");
            }

            System.Random random = new System.Random(seed);
            ModuleCatalogEntry straight = GetModuleEntry("corridor_straight");
            List<ProximityDoor> doors = new List<ProximityDoor>();
            ModuleCatalogEntry[] roomEntries = new ModuleCatalogEntry[_generationConfig.MainRoomIds.Length];

            for (int roomIndex = 0; roomIndex < roomEntries.Length; roomIndex++)
            {
                roomEntries[roomIndex] = GetModuleEntry(_generationConfig.MainRoomIds[roomIndex]);
            }
            float pitch = ValidateGridModules(roomEntries, straight);
            List<Bounds> spawnAreas = new List<Bounds>();
            LevelModule[] rooms = PlaceGridRooms(root.GeometryRoot, layout, roomEntries, pitch, random, spawnAreas);
            ConnectGridRooms(root.GeometryRoot, root.transform, layout, rooms, straight, doors);

            Transform entryPort = rooms[layout.StartCellIndex].GetPort(MazeDirection.West);
            LevelModule entryCorridor = Attach(straight, entryPort, MazeDirection.South, root.GeometryRoot);
            ModuleCatalogEntry entranceEntry = GetModuleEntry("room_gate");
            LevelModule entrance = Attach(entranceEntry, entryCorridor.GetPort(MazeDirection.North), MazeDirection.South, root.GeometryRoot);
            AddSpawnArea(spawnAreas, entrance, entranceEntry);
            PlacePassageDoor(entryPort, root.transform, doors);
            PlacePassageDoor(entrance.GetPort(MazeDirection.South), root.transform, doors);
            GameObject entranceGate = UnityEngine.Object.Instantiate(_presentationConfig.GateLeafPrefab,
                entrance.GetPort(MazeDirection.North).position, entrance.GetPort(MazeDirection.North).rotation, root.transform);
            if (entranceGate.TryGetComponent(out ProximityDoor entranceDoor) == false)
            {
                throw new InvalidOperationException("Gate prefab must contain ProximityDoor for the entrance.");
            }

            entranceDoor.Setup(_generationConfig.DoorConfig);
            doors.Add(entranceDoor);
            ModuleCatalogEntry black = new ModuleCatalogEntry("black_corridor", _presentationConfig.BlackCorridorPrefab, straight.FootprintSize);
            PlaceExitCorridor(root.GeometryRoot, black, entrance.TransitionAnchor);
            Vector3 entranceSpawn = entrance.TransitionAnchor.position +
                entrance.TransitionAnchor.forward * _presentationConfig.EntranceSpawnInset;
            HashSet<int> branchCells = PlaceSideRooms(root, layout, rooms, straight, doors, spawnAreas, random);
            Transform exitPort = rooms[layout.ExitCellIndex].GetPort(MazeDirection.East);
            LevelModule exitConnector = Attach(straight, exitPort, MazeDirection.South, root.GeometryRoot);
            ModuleCatalogEntry gateEntry = GetModuleEntry("room_gate");
            LevelModule gateRoom = Attach(gateEntry, exitConnector.GetPort(MazeDirection.North), MazeDirection.South, root.GeometryRoot);
            AddSpawnArea(spawnAreas, gateRoom, gateEntry);
            CloseGridPorts(root.GeometryRoot, layout, rooms, branchCells);
            PlacePassageDoor(exitPort, root.transform, doors);
            PlacePassageDoor(gateRoom.GetPort(MazeDirection.South), root.transform, doors);

            Transform gatePort = gateRoom.GetPort(MazeDirection.North);
            GameObject gateObject = UnityEngine.Object.Instantiate(_presentationConfig.GateLeafPrefab, gatePort.position, gatePort.rotation, root.transform);

            if (gateObject.TryGetComponent(out ExitGate exitGate) == false)
            {
                throw new InvalidOperationException("Exit gate prefab must contain ExitGate.");
            }

            exitGate.Setup(_presentationConfig);
            Transform corridorStart = gateRoom.TransitionAnchor;

            if (corridorStart == null)
            {
                throw new InvalidOperationException("Gate room prefab must have a transition anchor.");
            }

            Vector3 corridorEnd = PlaceExitCorridor(root.GeometryRoot, black, corridorStart);
            Physics.SyncTransforms();
            ApplyWallTint(root.GeometryRoot, levelNumber);
            root.Navigation.BuildNavMesh();
            root.Resources.Setup(root.Navigation.navMeshData);
            Vector3 spawn = SnapToNavMesh(entranceSpawn, seed);
            PlaceDecorations(root.GeometryRoot, new System.Random(seed));
            Physics.SyncTransforms();
            Vector3[] enemies = SampleSpawnPositions(spawn, ComputeEnemyCount(levelNumber), _generationConfig.EnemyMinSpawnDistance,
                _generationConfig.EnemyMinSpacing, spawnAreas, random, seed);
            Vector3[] pickups = SampleSpawnPositions(spawn, _generationConfig.HealPickupCount, _generationConfig.PickupMinSpawnDistance,
                _generationConfig.EnemyMinSpacing, spawnAreas, random, seed);
            return new GeneratedLevel(root.gameObject, root.GeometryRoot, spawn, Quaternion.LookRotation(-entrance.TransitionAnchor.forward),
                layout, exitGate, corridorStart.position, corridorEnd, corridorStart.forward, straight.FootprintSize.x * 0.5f,
                enemies, pickups, root.Navigation, doors.ToArray());
        }

        private void ApplyWallTint(Transform root, int levelNumber)
        {
            float progress = Mathf.Clamp01((levelNumber - 1) * _presentationConfig.WallReddeningPerLevel);
            Color tint = Color.Lerp(_presentationConfig.WallTintStart, _presentationConfig.WallTintDeep, progress);
            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;

                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material material = materials[materialIndex];

                    if (material == null || material.shader.name != "MazeHell/Environment Unlit")
                    {
                        continue;
                    }

                    properties.Clear();
                    renderer.GetPropertyBlock(properties, materialIndex);
                    properties.SetColor("_BaseColor", tint);
                    properties.SetFloat("_Saturation", _presentationConfig.WallSaturation);
                    properties.SetFloat("_Brightness", _presentationConfig.WallBrightness);
                    float corruption = Mathf.Clamp01(_presentationConfig.WallCorruptionStart +
                        (levelNumber - 1) * _presentationConfig.WallCorruptionPerLevel);
                    properties.SetFloat("_GrimeAmount", corruption);
                    properties.SetFloat("_GrimeScale", _presentationConfig.WallCorruptionTileSize);
                    renderer.SetPropertyBlock(properties, materialIndex);
                }
            }
        }

        private LevelModule[] PlaceGridRooms(Transform parent, MazeLayout layout, ModuleCatalogEntry[] entries, float pitch,
            System.Random random, List<Bounds> spawnAreas)
        {
            LevelModule[] rooms = new LevelModule[layout.Size];
            int variantOffset = random.Next(entries.Length);

            for (int cellIndex = 0; cellIndex < layout.Size; cellIndex++)
            {
                int variant = random.Next(entries.Length);

                if (cellIndex < entries.Length)
                {
                    variant = (cellIndex + variantOffset) % entries.Length;
                }

                ModuleCatalogEntry entry = entries[variant];
                MazeCell cell = layout.Cells[cellIndex];
                Vector3 position = new Vector3(cell.Coordinates.x * pitch, 0f, cell.Coordinates.y * pitch);
                rooms[cellIndex] = InstantiateModule(entry, position, Quaternion.identity, parent);
                AddSpawnArea(spawnAreas, rooms[cellIndex], entry);
            }

            return rooms;
        }

        private void ConnectGridRooms(Transform parent, Transform doorParent, MazeLayout layout, LevelModule[] rooms, ModuleCatalogEntry straight, List<ProximityDoor> doors)
        {
            for (int cellIndex = 0; cellIndex < layout.Size; cellIndex++)
            {
                for (int directionIndex = 0; directionIndex < 2; directionIndex++)
                {
                    MazeDirection direction = (MazeDirection)directionIndex;

                    if (layout.TryGetNeighborIndex(cellIndex, direction, out int neighborIndex) == false)
                    {
                        continue;
                    }

                    LevelModule corridor = Attach(straight, rooms[cellIndex].GetPort(direction), MazeDirection.South, parent);
                    Transform corridorEnd = corridor.GetPort(MazeDirection.North);
                    Transform roomEnd = rooms[neighborIndex].GetPort(MazeDirections.Opposite(direction));
                    PlacePassageDoor(rooms[cellIndex].GetPort(direction), doorParent, doors);
                    PlacePassageDoor(roomEnd, doorParent, doors);

                    if (Vector3.Distance(corridorEnd.position, roomEnd.position) > PortTolerance ||
                        Vector3.Dot(corridorEnd.forward, roomEnd.forward) > -1f + PortTolerance)
                    {
                        throw new InvalidOperationException($"Prefab ports do not meet between rooms {cellIndex} and {neighborIndex}.");
                    }
                }
            }
        }

        private void CloseGridPorts(Transform parent, MazeLayout layout, LevelModule[] rooms, HashSet<int> branchCells)
        {
            for (int cellIndex = 0; cellIndex < layout.Size; cellIndex++)
            {
                for (int directionIndex = 0; directionIndex < MazeDirections.Count; directionIndex++)
                {
                    MazeDirection direction = (MazeDirection)directionIndex;

                    if (layout.Cells[cellIndex].HasPassage(direction) ||
                        (cellIndex == layout.StartCellIndex && direction == MazeDirection.West) ||
                        (cellIndex == layout.ExitCellIndex && direction == MazeDirection.East) ||
                        (branchCells.Contains(cellIndex) && direction == MazeDirection.North))
                    {
                        continue;
                    }

                    PlaceClosedDoor(rooms[cellIndex].GetPort(direction), parent);
                }
            }
        }

        private void CloseUnusedPorts(LevelModule room, MazeDirection connectedDirection)
        {
            for (int directionIndex = 0; directionIndex < MazeDirections.Count; directionIndex++)
            {
                MazeDirection direction = (MazeDirection)directionIndex;

                if (direction != connectedDirection)
                {
                    PlaceClosedDoor(room.GetPort(direction), room.transform);
                }
            }
        }

        private void PlacePassageDoor(Transform port, Transform parent, List<ProximityDoor> doors)
        {
            GameObject instance = UnityEngine.Object.Instantiate(_generationConfig.ClosedDoorPrefab, port.position, port.rotation, parent);

            if (instance.TryGetComponent(out ProximityDoor door) == false)
            {
                throw new InvalidOperationException("Passage door prefab must contain ProximityDoor.");
            }

            door.Setup(_generationConfig.DoorConfig);
            doors.Add(door);
        }

        private HashSet<int> PlaceSideRooms(LevelRoot root, MazeLayout layout, LevelModule[] rooms, ModuleCatalogEntry straight,
            List<ProximityDoor> doors, List<Bounds> spawnAreas, System.Random random)
        {
            HashSet<int> branchCells = new HashSet<int>();
            List<int> columns = new List<int>();

            for (int column = 0; column < layout.Side; column++)
            {
                columns.Add(column);
            }

            int minimum = Mathf.Min(_generationConfig.SideRoomCountMin, layout.Side);
            int maximum = Mathf.Clamp(_generationConfig.SideRoomCountMax, minimum, layout.Side);
            int count = random.Next(minimum, maximum + 1);

            for (int branchIndex = 0; branchIndex < count; branchIndex++)
            {
                int columnIndex = random.Next(columns.Count);
                int cellIndex = (layout.Side - 1) * layout.Side + columns[columnIndex];
                columns.RemoveAt(columnIndex);
                branchCells.Add(cellIndex);
                Transform port = rooms[cellIndex].GetPort(MazeDirection.North);
                LevelModule corridor = Attach(straight, port, MazeDirection.South, root.GeometryRoot);
                LevelModule corner = Attach(GetModuleEntry("corridor_corner"), corridor.GetPort(MazeDirection.North), MazeDirection.South, root.GeometryRoot);
                string moduleId = "room_mini_cover";

                if (branchIndex % 2 == 1)
                {
                    moduleId = "room_mini_pillars";
                }

                ModuleCatalogEntry entry = GetModuleEntry(moduleId);
                LevelModule branch = Attach(entry, corner.GetPort(MazeDirection.West), MazeDirection.East, root.GeometryRoot);
                CloseUnusedPorts(branch, MazeDirection.East);
                PlacePassageDoor(port, root.transform, doors);
                PlacePassageDoor(branch.GetPort(MazeDirection.East), root.transform, doors);
                AddSpawnArea(spawnAreas, branch, entry);
            }

            return branchCells;
        }

        private void PlaceDecorations(Transform root, System.Random random)
        {
            GameObject[] prefabs = _generationConfig.DecorationPrefabs;

            if (prefabs.Length == 0)
            {
                return;
            }

            LevelModule[] modules = root.GetComponentsInChildren<LevelModule>();
            List<Vector3> placedPositions = new List<Vector3>();

            for (int moduleIndex = 0; moduleIndex < modules.Length; moduleIndex++)
            {
                Transform[] points = modules[moduleIndex].DecorationPoints;
                List<int> availablePoints = new List<int>(points.Length);

                for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
                {
                    availablePoints.Add(pointIndex);
                }

                int minimum = _generationConfig.DecorationsPerRoomMin;
                int count = random.Next(minimum, Mathf.Max(minimum, _generationConfig.DecorationsPerRoomMax) + 1);
                int placed = 0;

                while (availablePoints.Count > 0 && placed < count)
                {
                    int index = random.Next(availablePoints.Count);
                    Transform point = points[availablePoints[index]];
                    availablePoints.RemoveAt(index);

                    if (HasNearbyPosition(point.position, placedPositions, _generationConfig.DecorationSpacing) ||
                        modules[moduleIndex].IsNearDoorway(point.position, _generationConfig.DoorwayClearance) ||
                        Physics.CheckBox(point.position + Vector3.up * 1.4f,
                            new Vector3(_generationConfig.DecorationWallInset * 0.5f, 1.3f, _generationConfig.DecorationWallInset * 0.5f), point.rotation,
                            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    {
                        continue;
                    }

                    GameObject prefab = prefabs[random.Next(prefabs.Length)];

                    if (prefab == null)
                    {
                        throw new InvalidOperationException("Decoration catalog contains an unassigned prefab.");
                    }

                    UnityEngine.Object.Instantiate(prefab, point.position, point.rotation, root);
                    placedPositions.Add(point.position);
                    placed++;
                }
            }
        }

        private void PlaceClosedDoor(Transform port, Transform parent)
        {
            UnityEngine.Object.Instantiate(_generationConfig.ClosedDoorPrefab, port.position, port.rotation, parent);
        }

        private Vector3 PlaceExitCorridor(Transform parent, ModuleCatalogEntry straight, Transform start)
        {
            LevelModule prefab = GetModule(straight);
            float length = Vector3.Distance(prefab.GetPort(MazeDirection.South).localPosition, prefab.GetPort(MazeDirection.North).localPosition);
            int count = Mathf.Max(1, Mathf.CeilToInt(_presentationConfig.DarkCorridorLength / length));
            Transform outlet = start;

            for (int corridorIndex = 0; corridorIndex < count; corridorIndex++)
            {
                LevelModule corridor = Attach(straight, outlet, MazeDirection.South, parent);
                outlet = corridor.GetPort(MazeDirection.North);
            }

            PlaceClosedDoor(outlet, parent);
            float inset = Mathf.Clamp(_presentationConfig.TransitionEndInset, SpawnInset, length * 0.5f);
            return outlet.position - outlet.forward * inset;
        }

        private LevelModule Attach(ModuleCatalogEntry entry, Transform outlet, MazeDirection inletDirection, Transform parent)
        {
            Transform inlet = GetModule(entry).GetPort(inletDirection);
            Quaternion rotation = outlet.rotation * Quaternion.Euler(0f, 180f, 0f) * Quaternion.Inverse(inlet.localRotation);
            Vector3 position = outlet.position - rotation * inlet.localPosition;
            return InstantiateModule(entry, position, rotation, parent);
        }

        private LevelModule InstantiateModule(ModuleCatalogEntry entry, Vector3 position, Quaternion rotation, Transform parent)
        {
            LevelModule prefab = GetModule(entry);
            return UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
        }

        private LevelModule GetModule(ModuleCatalogEntry entry)
        {
            if (entry.Prefab.TryGetComponent(out LevelModule module) == false)
            {
                throw new InvalidOperationException($"{entry.ModuleId}: prefab must contain LevelModule.");
            }

            if (module.transform.localScale != Vector3.one)
            {
                throw new InvalidOperationException($"{entry.ModuleId}: module prefab must have unit scale.");
            }

            return module;
        }

        private float ValidateGridModules(ModuleCatalogEntry[] rooms, ModuleCatalogEntry straight)
        {
            LevelModule corridor = GetModule(straight);
            float length = Vector3.Distance(corridor.GetPort(MazeDirection.North).localPosition, corridor.GetPort(MazeDirection.South).localPosition);
            float roomWidth = rooms[0].FootprintSize.x;

            for (int roomIndex = 0; roomIndex < rooms.Length; roomIndex++)
            {
                ModuleCatalogEntry entry = rooms[roomIndex];
                LevelModule module = GetModule(entry);

                if (Mathf.Abs(entry.FootprintSize.x - roomWidth) > PortTolerance ||
                    Mathf.Abs(entry.FootprintSize.z - roomWidth) > PortTolerance)
                {
                    throw new InvalidOperationException($"{entry.ModuleId}: main grid room footprints must match.");
                }

                for (int directionIndex = 0; directionIndex < MazeDirections.Count; directionIndex++)
                {
                    MazeDirection direction = (MazeDirection)directionIndex;
                    Vector2Int delta = MazeDirections.Delta(direction);
                    Vector3 expected = new Vector3(delta.x, 0f, delta.y) * (roomWidth * 0.5f);
                    Transform port = module.GetPort(direction);

                    if (port.parent != module.transform || Vector3.Distance(port.localPosition, expected) > PortTolerance)
                    {
                        throw new InvalidOperationException($"{entry.ModuleId}: {direction} port must match the grid footprint.");
                    }
                }
            }

            return roomWidth + length;
        }

        private void AddSpawnArea(List<Bounds> areas, LevelModule module, ModuleCatalogEntry entry)
        {
            Vector3 size = entry.FootprintSize - new Vector3(SpawnInset * 2f, 0f, SpawnInset * 2f);
            areas.Add(new Bounds(module.transform.position, size));
        }

        private Vector3 SnapToNavMesh(Vector3 position, int seed)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, NavMeshSampleRadius * 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            throw new InvalidOperationException($"Player spawn is not on NavMesh (seed {seed}).");
        }

        private Vector3[] SampleSpawnPositions(Vector3 origin, int count, float minimumDistance, float minimumSpacing,
            List<Bounds> areas, System.Random random, int seed)
        {
            List<Vector3> positions = new List<Vector3>(count);
            NavMeshPath path = new NavMeshPath();
            int attemptLimit = PlacementAttemptsPerSpawn * count;

            for (int attempt = 0; attempt < attemptLimit && positions.Count < count; attempt++)
            {
                Bounds area = areas[random.Next(areas.Count)];
                Vector3 candidate = new Vector3(Mathf.Lerp(area.min.x, area.max.x, (float)random.NextDouble()), 1f,
                    Mathf.Lerp(area.min.z, area.max.z, (float)random.NextDouble()));

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, NavMeshSampleRadius, NavMesh.AllAreas) == false ||
                    Vector3.Distance(hit.position, origin) < minimumDistance || HasNearbyPosition(hit.position, positions, minimumSpacing))
                {
                    continue;
                }

                if (NavMesh.CalculatePath(origin, hit.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    positions.Add(hit.position);
                }
            }

            if (positions.Count != count)
            {
                throw new InvalidOperationException($"Only {positions.Count} of {count} reachable spawn positions found (seed {seed}).");
            }

            return positions.ToArray();
        }

        private bool HasNearbyPosition(Vector3 candidate, List<Vector3> positions, float minimumSpacing)
        {
            for (int positionIndex = 0; positionIndex < positions.Count; positionIndex++)
            {
                if (Vector3.Distance(candidate, positions[positionIndex]) < minimumSpacing)
                {
                    return true;
                }
            }

            return false;
        }

        private MazeLayout SelectBoundaryEndpoints(MazeLayout layout)
        {
            int entry = 0;
            int exit = layout.Size - 1;
            int longestDistance = -1;

            for (int entryRow = 0; entryRow < layout.Side; entryRow++)
            {
                int entryIndex = entryRow * layout.Side;

                for (int exitRow = 0; exitRow < layout.Side; exitRow++)
                {
                    int exitIndex = exitRow * layout.Side + layout.Side - 1;
                    int distance = layout.GetDistance(entryIndex, exitIndex);

                    if (distance > longestDistance)
                    {
                        longestDistance = distance;
                        entry = entryIndex;
                        exit = exitIndex;
                    }
                }
            }

            return new MazeLayout(layout.Side, layout.Seed, entry, exit, layout.Cells);
        }

        private ModuleCatalogEntry GetModuleEntry(string moduleId)
        {
            ModuleCatalogEntry[] entries = _generationConfig.ModuleCatalog;

            for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++)
            {
                ModuleCatalogEntry entry = entries[entryIndex];

                if (entry != null && entry.ModuleId == moduleId && entry.Prefab != null)
                {
                    return entry;
                }
            }

            throw new InvalidOperationException($"Module catalog has no usable entry '{moduleId}'.");
        }

        private int ComputeSide(int levelNumber)
        {
            int side = _generationConfig.RoomGridSideStart + (levelNumber - 1) * _generationConfig.RoomGridSideGrowthPerLevel;
            return Mathf.Min(side, _generationConfig.RoomGridSideCap);
        }

        private int ComputeEnemyCount(int levelNumber)
        {
            int count = _generationConfig.EnemyCountStart + (levelNumber - 1) * _generationConfig.EnemyCountGrowthPerLevel;
            return Mathf.Min(count, _generationConfig.EnemyCountCap);
        }

        private void ValidateBuildState(int levelNumber)
        {
            if (_generationConfig == null || _presentationConfig == null)
            {
                throw new InvalidOperationException("Call Setup before building a level.");
            }

            if (levelNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(levelNumber));
            }

            if (_generationConfig.LevelPrefab == null || _generationConfig.ClosedDoorPrefab == null ||
                _presentationConfig.GateLeafPrefab == null || _generationConfig.ModuleCatalog == null || _generationConfig.DoorConfig == null || _presentationConfig.BlackCorridorPrefab == null)
            {
                throw new InvalidOperationException("Level root, closed door, exit gate and module catalog prefabs must be assigned.");
            }

            if (_generationConfig.RoomGridSideStart < MazeLayout.MinimumSide ||
                _generationConfig.RoomGridSideCap < _generationConfig.RoomGridSideStart ||
                _generationConfig.LoopEdgeCountMin > _generationConfig.LoopEdgeCountMax)
            {
                throw new InvalidOperationException("Invalid room grid or loop settings.");
            }
        }
    }
}
