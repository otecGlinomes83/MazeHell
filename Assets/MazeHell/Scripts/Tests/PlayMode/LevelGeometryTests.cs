#if UNITY_EDITOR
using System.Collections;
using MazeHell.Configuration;
using MazeHell.Level;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace MazeHell.Tests
{
    public class LevelGeometryTests
    {
        private GeneratedLevel _level;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_level != null)
            {
                _level.NavMeshSurface.RemoveData();
                Object.Destroy(_level.Root);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator AuthoredModulesProduceReachableActorsAndUnlockableExit()
        {
            LevelGenerationConfig generation = AssetDatabase.LoadAssetAtPath<LevelGenerationConfig>(
                "Assets/MazeHell/Scriptables/Levels/LevelGeneration.asset");
            LevelPresentationConfig presentation = AssetDatabase.LoadAssetAtPath<LevelPresentationConfig>(
                "Assets/MazeHell/Scriptables/Levels/LevelPresentation.asset");
            LevelGeometryFactory factory = new LevelGeometryFactory();
            factory.Setup(generation, presentation);
            _level = factory.Build(1, 104729, null);
            yield return null;
            Assert.AreEqual(generation.EnemyCountStart, _level.EnemySpawnPositions.Length);
            Assert.AreEqual(generation.HealPickupCount, _level.PickupPositions.Length);
            Assert.AreEqual(generation.RoomGridSideStart, _level.Layout.Side);
            Assert.Greater(Vector3.Dot(_level.CorridorEnd - _level.CorridorStart,
                _level.CorridorDirection), 20f);
            NavMeshPath path = new NavMeshPath();

            for (int i = 0; i < _level.EnemySpawnPositions.Length; i++)
            {
                Assert.IsTrue(NavMesh.CalculatePath(_level.SpawnPosition, _level.EnemySpawnPositions[i],
                    NavMesh.AllAreas, path));
                Assert.AreEqual(NavMeshPathStatus.PathComplete, path.status);
            }

            Assert.IsFalse(_level.ExitGate.IsUnlocked);
            _level.ExitGate.SetRemainingEnemies(1);
            Assert.IsFalse(_level.ExitGate.IsUnlocked);
            _level.ExitGate.SetRemainingEnemies(0);
            Assert.IsFalse(_level.ExitGate.IsUnlocked);
            _level.ExitGate.TickPlayer(_level.ExitGate.transform.position - _level.ExitGate.transform.forward,
                _level.ExitGate.transform.forward * 20f, 1.8f, 0.1f);
            Assert.IsTrue(_level.ExitGate.IsUnlocked);
            _level.ExitGate.Tick(presentation.GateLiftDuration + 0.1f);
            Assert.IsTrue(NavMesh.SamplePosition(_level.CorridorEnd, out NavMeshHit end,
                2f, NavMesh.AllAreas));
            Assert.IsTrue(NavMesh.CalculatePath(_level.SpawnPosition, end.position, NavMesh.AllAreas, path));
            Assert.AreEqual(NavMeshPathStatus.PathComplete, path.status);
        }

        [UnityTest]
        public IEnumerator FastApproachClearsDoorBeforeContactAndKeepsItOpenAroundPlayer()
        {
            LevelGenerationConfig generation = AssetDatabase.LoadAssetAtPath<LevelGenerationConfig>(
                "Assets/MazeHell/Scriptables/Levels/LevelGeneration.asset");
            LevelPresentationConfig presentation = AssetDatabase.LoadAssetAtPath<LevelPresentationConfig>(
                "Assets/MazeHell/Scriptables/Levels/LevelPresentation.asset");
            LevelGeometryFactory factory = new LevelGeometryFactory();
            factory.Setup(generation, presentation);
            _level = factory.Build(1, 37, null);
            ProximityDoor selected = null;
            ProximityDoor[] doors = _level.Root.GetComponentsInChildren<ProximityDoor>();

            for (int doorIndex = 0; doorIndex < doors.Length; doorIndex++)
            {
                if (doors[doorIndex].transform.parent == _level.Root.transform && doors[doorIndex].name.StartsWith("door_wood"))
                {
                    selected = doors[doorIndex];
                    break;
                }
            }

            Assert.IsNotNull(selected);
            Vector3 closedPosition = selected.transform.position;
            Vector3 forward = selected.transform.forward;
            selected.Tick(closedPosition + forward * 0.6f, -forward * 60f, 1.8f, 0.016f);
            Assert.GreaterOrEqual(selected.transform.position.y - closedPosition.y, 2.3f);
            selected.Tick(closedPosition, Vector3.zero, 1.8f, 1f);
            Assert.Greater(selected.transform.position.y - closedPosition.y, 2.3f);
            selected.Tick(closedPosition + forward * 1000f, Vector3.zero, 1.8f, 3f);
            Assert.Less(Vector3.Distance(selected.transform.position, closedPosition), 0.01f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PrefabModulesRemainReachableAcrossSeedsAndLevelSizes()
        {
            LevelGenerationConfig generation = AssetDatabase.LoadAssetAtPath<LevelGenerationConfig>(
                "Assets/MazeHell/Scriptables/Levels/LevelGeneration.asset");
            LevelPresentationConfig presentation = AssetDatabase.LoadAssetAtPath<LevelPresentationConfig>(
                "Assets/MazeHell/Scriptables/Levels/LevelPresentation.asset");
            LevelGeometryFactory factory = new LevelGeometryFactory();
            factory.Setup(generation, presentation);
            int[] seeds = new int[] { 1, 104729, -987654 };

            for (int seedIndex = 0; seedIndex < seeds.Length; seedIndex++)
            {
                int levelNumber = seedIndex + 1;
                MazeLayout prepared = factory.PrepareLayout(levelNumber, seeds[seedIndex]);
                _level = factory.Build(levelNumber, seeds[seedIndex], null, prepared);
                yield return null;
                Assert.IsTrue(_level.Layout.IsConnected());
                LevelModule[] modules = _level.GeometryRoot.GetComponentsInChildren<LevelModule>();
                int roomCount = 0;
                NavMeshPath path = new NavMeshPath();

                for (int moduleIndex = 0; moduleIndex < modules.Length; moduleIndex++)
                {
                    LevelModule module = modules[moduleIndex];
                    Assert.AreEqual(Vector3.one, module.transform.localScale);

                    if (module.name.StartsWith("room_"))
                    {
                        roomCount++;
                    }

                    Transform port = module.GetPort(MazeDirection.South);
                    Vector3 interior = port.position - port.forward * 2f + Vector3.up;
                    Assert.IsTrue(NavMesh.SamplePosition(interior, out NavMeshHit hit, 2f, NavMesh.AllAreas), module.name);
                    Assert.IsTrue(NavMesh.CalculatePath(_level.SpawnPosition, hit.position, NavMesh.AllAreas, path), module.name);
                    Assert.AreEqual(NavMeshPathStatus.PathComplete, path.status, module.name);
                }

                Assert.GreaterOrEqual(roomCount, _level.Layout.Size + 2 + generation.SideRoomCountMin);
                Assert.LessOrEqual(roomCount, _level.Layout.Size + 2 + generation.SideRoomCountMax);
                MeshFilter[] meshes = _level.Root.GetComponentsInChildren<MeshFilter>();
                Assert.Greater(meshes.Length, 0);

                for (int meshIndex = 0; meshIndex < meshes.Length; meshIndex++)
                {
                    Assert.IsNotNull(meshes[meshIndex].sharedMesh);
                    Assert.IsNotEmpty(AssetDatabase.GetAssetPath(meshes[meshIndex].sharedMesh), meshes[meshIndex].name);
                }

                Assert.Greater(_level.Root.GetComponentsInChildren<Light>().Length, 0);
                Vector3 gatePosition = _level.ExitGate.transform.position;
                _level.ExitGate.SetRemainingEnemies(0);
                _level.ExitGate.TickPlayer(gatePosition - _level.ExitGate.transform.forward, Vector3.zero, 1.8f, 0.1f);
                _level.ExitGate.Tick(presentation.GateLiftDuration);
                Assert.Greater(_level.ExitGate.transform.position.y, gatePosition.y);
                _level.NavMeshSurface.RemoveData();
                Object.Destroy(_level.Root);
                _level = null;
                yield return null;
            }
        }
    }
}
#endif
