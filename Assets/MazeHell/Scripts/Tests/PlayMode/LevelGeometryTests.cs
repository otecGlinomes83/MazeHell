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
            Assert.Greater(_level.CorridorStart.x, generation.LogicalSideStart * generation.CellSize);
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
            Assert.IsTrue(_level.ExitGate.IsUnlocked);
            _level.ExitGate.Tick(presentation.GateLiftDuration + 0.1f);
            Assert.IsTrue(NavMesh.SamplePosition(_level.CorridorEnd, out NavMeshHit end,
                2f, NavMesh.AllAreas));
            Assert.IsTrue(NavMesh.CalculatePath(_level.SpawnPosition, end.position, NavMesh.AllAreas, path));
            Assert.AreEqual(NavMeshPathStatus.PathComplete, path.status);
        }
    }
}
#endif
