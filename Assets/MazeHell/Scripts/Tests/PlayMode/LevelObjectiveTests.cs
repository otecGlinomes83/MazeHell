#if UNITY_EDITOR
using System.Collections;
using MazeHell.Enemies;
using MazeHell.Level;
using MazeHell.Session;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MazeHell.Tests
{
    public class LevelObjectiveTests
    {
        private Scene _scene;
        private GameComposition _composition;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _scene = EditorSceneManager.LoadSceneInPlayMode("Assets/MazeHell/Scenes/Game.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            foreach (GameObject root in _scene.GetRootGameObjects())
            {
                if (root.TryGetComponent(out GameComposition composition))
                {
                    _composition = composition;
                }
            }

            Assert.IsNotNull(_composition);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Scene blank = SceneManager.CreateScene("ObjectiveTestCleanup");
            SceneManager.SetActiveScene(blank);

            if (_scene.IsValid() && _scene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(_scene);
            }
        }

        [UnityTest]
        public IEnumerator KillsBeforeFindingExitCountTowardsTheGoalAndDoorRemainsLocked()
        {
            string searchObjective = _composition.Hud.ObjectiveText.text;
            EnemyRig[] enemies = _composition.Session.Enemies;
            int earlierKills = enemies.Length - 3;

            for (int enemyIndex = 0; enemyIndex < earlierKills; enemyIndex++)
            {
                Kill(enemies[enemyIndex]);
            }

            _composition.Hud.Presenter.LateTick(1f);
            Assert.AreEqual(searchObjective, _composition.Hud.ObjectiveText.text);
            Assert.AreEqual(3, _composition.Session.RemainingEnemies);
            EnterExitRoom();
            StringAssert.EndsWith(": 3", _composition.Hud.ObjectiveText.text);
            ExitGate gate = _composition.Session.CurrentLevel.ExitGate;
            gate.TickPlayer(gate.transform.position, Vector3.zero, 1.8f, 0.1f);
            Assert.IsFalse(gate.IsUnlocked);

            for (int enemyIndex = earlierKills; enemyIndex < enemies.Length; enemyIndex++)
            {
                Kill(enemies[enemyIndex]);
                _composition.Hud.Presenter.LateTick(1f);
            }

            Assert.AreEqual(0, _composition.Session.RemainingEnemies);
            StringAssert.DoesNotContain(": 0", _composition.Hud.ObjectiveText.text);
            StringAssert.DoesNotContain(": 3", _composition.Hud.ObjectiveText.text);
            Assert.AreNotEqual(searchObjective, _composition.Hud.ObjectiveText.text);
            gate.TickPlayer(gate.transform.position, Vector3.zero, 1.8f, 0.1f);
            Assert.IsTrue(gate.IsUnlocked);
            Assert.AreEqual(enemies.Length, _composition.Session.Kills);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ClearingEverythingBeforeFindingExitShowsCompletedGoalImmediately()
        {
            EnemyRig[] enemies = _composition.Session.Enemies;

            foreach (EnemyRig enemy in enemies)
            {
                Kill(enemy);
            }

            string searchObjective = _composition.Hud.ObjectiveText.text;
            EnterExitRoom();
            Assert.AreNotEqual(searchObjective, _composition.Hud.ObjectiveText.text);
            StringAssert.DoesNotContain(": 0", _composition.Hud.ObjectiveText.text);
            ExitGate gate = _composition.Session.CurrentLevel.ExitGate;
            gate.TickPlayer(gate.transform.position, Vector3.zero, 1.8f, 0.1f);
            Assert.IsTrue(gate.IsUnlocked);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FinalKillsDuringObjectiveAnimationCannotRestoreAnOldEnemyCount()
        {
            EnemyRig[] enemies = _composition.Session.Enemies;
            int earlierKills = enemies.Length - 3;

            for (int enemyIndex = 0; enemyIndex < earlierKills; enemyIndex++)
            {
                Kill(enemies[enemyIndex]);
            }

            GeneratedLevel level = _composition.Session.CurrentLevel;
            Vector3 center = level.Map.Rooms[level.Map.ExitRoomIndex].center;
            _composition.Player.Motor.Teleport(center, _composition.Player.transform.rotation);
            Vector2 objectivePosition = _composition.Hud.ObjectiveText.rectTransform.anchoredPosition;
            _composition.Hud.Presenter.LateTick(0.1f);

            for (int enemyIndex = earlierKills; enemyIndex < enemies.Length; enemyIndex++)
            {
                Kill(enemies[enemyIndex]);
            }

            _composition.Hud.Presenter.LateTick(0.3f);
            _composition.Hud.Presenter.LateTick(0.3f);
            Assert.AreEqual(0, _composition.Session.RemainingEnemies);
            StringAssert.DoesNotContain(": 3", _composition.Hud.ObjectiveText.text);
            StringAssert.DoesNotContain(": 0", _composition.Hud.ObjectiveText.text);
            Assert.AreEqual(objectivePosition, _composition.Hud.ObjectiveText.rectTransform.anchoredPosition);
            Assert.Greater(_composition.Hud.ObjectiveText.alpha, 0.99f);
            yield return null;
        }

        private void EnterExitRoom()
        {
            GeneratedLevel level = _composition.Session.CurrentLevel;
            Vector3 center = level.Map.Rooms[level.Map.ExitRoomIndex].center;
            _composition.Player.Motor.Teleport(center, _composition.Player.transform.rotation);
            _composition.Hud.Presenter.LateTick(1f);
        }

        private void Kill(EnemyRig enemy)
        {
            enemy.DamageTarget.Receive(enemy.Health.MaxHealth, Vector3.forward, 0f);
            Assert.IsTrue(enemy.Health.IsDead);
        }
    }
}
#endif
