#if UNITY_EDITOR
using System.Collections;
using MazeHell.Enemies;
using MazeHell.Level;
using MazeHell.Session;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
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
            Assert.IsTrue(composition.TryGetComponent(out Pauser pauser));
            pauser.RequestPause();
            Assert.AreEqual(CursorLockMode.None, Cursor.lockState);
            Assert.IsTrue(Cursor.visible);
            Assert.IsNotNull(composition.Hud.Screens.ActivePause);
            int firstPauseId = composition.Hud.Screens.ActivePause.GetInstanceID();
            PointerEventData click = new PointerEventData(EventSystem.current);
            composition.Hud.Screens.ActivePause.Settings.OnPointerClick(click);
            Assert.IsNotNull(composition.Hud.Screens.ActiveSettings);
            Assert.IsFalse(composition.Hud.Screens.ActivePause.Group.interactable);
            composition.Hud.Screens.ActiveSettings.Close.OnPointerClick(click);
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.IsNull(composition.Hud.Screens.ActiveSettings);
            Assert.IsTrue(composition.Hud.Screens.ActivePause.Group.interactable);
            pauser.Resume();
            Assert.AreEqual(CursorLockMode.Locked, Cursor.lockState);
            Assert.IsFalse(Cursor.visible);
            Assert.IsNull(composition.Hud.Screens.ActivePause);
            yield return null;
            yield return null;
            Cursor.lockState = CursorLockMode.None;
            yield return null;
            yield return null;
            Assert.IsTrue(pauser.IsPaused);
            Assert.IsNotNull(composition.Hud.Screens.ActivePause);
            Assert.AreNotEqual(firstPauseId, composition.Hud.Screens.ActivePause.GetInstanceID());
            Assert.IsTrue(Cursor.visible);
            pauser.Resume();
            Assert.AreEqual(CursorLockMode.Locked, Cursor.lockState);
            Assert.IsFalse(Cursor.visible);
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

            Assert.Less(Mathf.Abs(Vector3.Dot(enemies[0].Renderer.transform.forward, Vector3.up)), 0.01f);
            Assert.AreEqual(enemies.Length, session.Kills);
            Assert.Greater(composition.Player.Health.CurrentHealth, 50f);
            Assert.IsFalse(session.CurrentLevel.ExitGate.IsUnlocked);
            ExitGate gate = session.CurrentLevel.ExitGate;
            gate.TickPlayer(gate.transform.position - gate.transform.forward, Vector3.zero, 1.8f, 0.1f);
            Assert.IsTrue(gate.IsUnlocked);
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
            Assert.IsNotNull(composition.Hud.Screens.ActiveDeath);
            Assert.AreEqual(CursorLockMode.None, Cursor.lockState);
            Assert.IsTrue(Cursor.visible);
            yield return null;
        }
    }
}
#endif
