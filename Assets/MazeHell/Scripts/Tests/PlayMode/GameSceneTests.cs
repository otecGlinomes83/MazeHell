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
    public class GameSceneTests
    {
        private Scene _scene;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Scene blank = SceneManager.CreateScene("GameTestCleanup");
            SceneManager.SetActiveScene(blank);

            if (_scene.IsValid() && _scene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(_scene);
            }
        }

        [UnityTest]
        public IEnumerator CompleteRunCycleKeepsHealthUnlocksExitAndRebuildsNextLevel()
        {
            _scene = EditorSceneManager.LoadSceneInPlayMode("Assets/MazeHell/Scenes/Game.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            GameComposition composition = null;
            GameObject[] roots = _scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].TryGetComponent(out GameComposition candidate))
                {
                    composition = candidate;
                }
            }

            Assert.IsNotNull(composition);
            RunSession session = composition.Session;
            Assert.AreEqual(RunState.Fighting, session.State);
            Assert.AreEqual(1, session.LevelNumber);
            Assert.Greater(session.Enemies.Length, 0);
            Assert.IsFalse(session.CurrentLevel.ExitGate.IsUnlocked);
            Assert.IsNotNull(composition.Hud.WeaponImage.sprite);
            Assert.IsNotNull(composition.Hud.PortraitImage.sprite);
            Assert.Less(Vector3.Distance(composition.Player.transform.position, session.CurrentLevel.SpawnPosition), 1f);
            composition.Player.Health.ApplyDamage(50f);
            EnemyRig[] enemies = session.Enemies;

            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i].Health.ApplyDamage(enemies[i].Health.MaxHealth);
            }

            Assert.Less(enemies[0].Renderer.transform.localPosition.y, 0.1f);
            Assert.AreEqual(enemies.Length, session.Kills);
            Assert.Greater(composition.Player.Health.CurrentHealth, 50f);
            Assert.IsTrue(session.CurrentLevel.ExitGate.IsUnlocked);
            float health = composition.Player.Health.CurrentHealth;
            GeneratedLevel firstLevel = session.CurrentLevel;
            composition.Player.Motor.Teleport(firstLevel.CorridorStart + firstLevel.CorridorDirection,
                firstLevel.SpawnRotation);
            session.Tick(0.02f);
            Assert.AreEqual(RunState.Transition, session.State);
            composition.Player.Motor.Teleport(firstLevel.CorridorEnd + firstLevel.CorridorDirection * 0.05f,
                firstLevel.SpawnRotation);
            session.Tick(0.02f);
            Assert.AreEqual(RunState.Generating, session.State);
            session.Tick(10f);
            Assert.AreEqual(2, session.LevelNumber);
            Assert.AreEqual(RunState.Fighting, session.State);
            Assert.IsFalse(firstLevel.Root.activeSelf);
            Assert.AreEqual(health, composition.Player.Health.CurrentHealth);
            Assert.IsFalse(session.CurrentLevel.ExitGate.IsUnlocked);
            Assert.Greater(session.Enemies.Length, enemies.Length);
            composition.Player.Health.ApplyDamage(composition.Player.Health.MaxHealth);
            session.Tick(0.02f);
            Assert.AreEqual(RunState.Dead, session.State);
            Assert.IsTrue(composition.Hud.DeathPanel.activeSelf);
            Assert.AreEqual(CursorLockMode.None, Cursor.lockState);
            yield return null;
        }
    }
}
#endif
