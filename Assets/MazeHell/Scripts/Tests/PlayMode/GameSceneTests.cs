#if UNITY_EDITOR
using System.Collections;
using MazeHell.Enemies;
using MazeHell.Level;
using MazeHell.Session;
using MazeHell.UI;
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
            Assert.IsNotNull(composition.Hud.PopupRoot.GetComponentInChildren<NarrativePopup>());
            Assert.IsNotNull(composition.Hud.ObjectiveText);
            Assert.IsFalse(string.IsNullOrEmpty(composition.Hud.ObjectiveText.text));
            Assert.IsTrue(composition.TryGetComponent(out Pauser pauser));
            Assert.IsTrue(pauser.IsPaused);
            Assert.IsNull(composition.Hud.Screens.ActivePause);
            Vector3 initialPosition = composition.Player.transform.position;
            float initialRunTime = composition.Session.RunTime;
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.AreEqual(initialPosition, composition.Player.transform.position);
            Assert.AreEqual(initialRunTime, composition.Session.RunTime);
            composition.Hud.Screens.DismissMonologue();
            float closeDeadline = Time.realtimeSinceStartup + 2f;

            while (composition.Hud.Screens.IsNarrativeOpen && Time.realtimeSinceStartup < closeDeadline)
            {
                yield return null;
            }

            Assert.IsFalse(composition.Hud.Screens.IsNarrativeOpen);
            Assert.IsFalse(pauser.IsPaused);
            Vector3 portraitPosition = composition.Hud.PortraitImage.transform.position;
            Vector3 crosshairPosition = composition.Hud.CrosshairRoot.position;
            composition.Player.CameraMotion.ReactDamage();
            composition.Player.CameraMotion.Tick(0f, true, 0.1f);
            composition.Hud.Spring.Tick(0.1f);
            Assert.Greater(Vector3.Distance(portraitPosition, composition.Hud.PortraitImage.transform.position), 0.001f);
            Assert.Less(Vector3.Distance(crosshairPosition, composition.Hud.CrosshairRoot.position), 0.001f);
            composition.Player.CameraMotion.ResetMotion();
            composition.Hud.Spring.ResetState();
            pauser.RequestPause();
            Assert.AreEqual(CursorLockMode.None, Cursor.lockState);
            Assert.IsTrue(Cursor.visible);
            Assert.IsNotNull(composition.Hud.Screens.ActivePause);
            EntityId firstPauseId = composition.Hud.Screens.ActivePause.GetEntityId();
            PointerEventData click = new PointerEventData(EventSystem.current);
            composition.Hud.Screens.ActivePause.Settings.OnPointerClick(click);
            Assert.IsNotNull(composition.Hud.Screens.ActiveSettings);
            Assert.IsFalse(composition.Hud.Screens.ActivePause.Group.interactable);
            composition.Hud.Screens.ActiveSettings.Close.OnPointerClick(click);
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.IsNull(composition.Hud.Screens.ActiveSettings);
            Assert.IsTrue(composition.Hud.Screens.ActivePause.Group.interactable);
            pauser.Resume();
            Assert.IsNull(composition.Hud.Screens.ActivePause);

            if (Application.isBatchMode == false)
            {
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
                Assert.AreNotEqual(firstPauseId, composition.Hud.Screens.ActivePause.GetEntityId());
                Assert.IsTrue(Cursor.visible);
                pauser.Resume();
                Assert.AreEqual(CursorLockMode.Locked, Cursor.lockState);
                Assert.IsFalse(Cursor.visible);
            }

            RunSession session = composition.Session;
            Assert.AreEqual(RunState.Fighting, session.State);
            Assert.AreEqual(1, session.LevelNumber);
            Assert.Greater(session.Enemies.Length, 0);
            Assert.IsFalse(session.CurrentLevel.ExitGate.IsUnlocked);
            Assert.IsNotNull(composition.Hud.WeaponImage.sprite);
            Assert.IsNotNull(composition.Hud.PortraitImage.sprite);
            Assert.AreSame(session.CurrentLevel.Map, composition.Hud.Minimap.Map);
            Assert.IsFalse(composition.Hud.CrosshairRoot.IsChildOf(composition.Hud.SpringRoot));
            Assert.IsFalse(composition.Hud.Minimap.transform.IsChildOf(composition.Hud.SpringRoot));
            Assert.Less(Vector3.Distance(composition.Player.transform.position, session.CurrentLevel.SpawnPosition), 1f);
            composition.Player.Health.ApplyDamage(50f);
            Assert.AreEqual(0.5f, composition.Hud.HealthFill.fillAmount, 0.001f);
            Assert.AreEqual(UnityEngine.UI.Image.FillMethod.Horizontal, composition.Hud.HealthFill.fillMethod);
            StringAssert.StartsWith("50 ", composition.Hud.HealthText.text);
            StringAssert.DoesNotContain("%", composition.Hud.HealthText.text);
            EnemyRig[] enemies = session.Enemies;
            string initialObjective = composition.Hud.ObjectiveText.text;
            Vector2 objectivePosition = composition.Hud.ObjectiveText.rectTransform.anchoredPosition;
            Assert.IsFalse(session.CurrentLevel.Map.IsRoomDiscovered(session.CurrentLevel.Map.ExitRoomIndex));
            enemies[0].Health.ApplyDamage(enemies[0].Health.MaxHealth);
            Assert.AreEqual(initialObjective, composition.Hud.ObjectiveText.text);
            Vector3 previousPosition = composition.Player.transform.position;
            Vector3 exitRoom = session.CurrentLevel.Map.Rooms[session.CurrentLevel.Map.ExitRoomIndex].center;
            composition.Player.Motor.Teleport(exitRoom, composition.Player.transform.rotation);
            composition.Hud.Presenter.LateTick(0.1f);
            Assert.AreEqual(initialObjective, composition.Hud.ObjectiveText.text);
            Assert.Greater(composition.Hud.ObjectiveText.rectTransform.anchoredPosition.y, objectivePosition.y);
            composition.Hud.Presenter.LateTick(1f);
            string expectedCount = (enemies.Length - 1).ToString();
            StringAssert.Contains(expectedCount, composition.Hud.ObjectiveText.text);
            Assert.AreNotEqual(initialObjective, composition.Hud.ObjectiveText.text);
            enemies[1].Health.ApplyDamage(enemies[1].Health.MaxHealth);
            expectedCount = (enemies.Length - 2).ToString();
            StringAssert.Contains(expectedCount, composition.Hud.ObjectiveText.text);
            Assert.AreEqual(objectivePosition, composition.Hud.ObjectiveText.rectTransform.anchoredPosition);
            composition.Player.Motor.Teleport(previousPosition, composition.Player.transform.rotation);


            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i].Health.ApplyDamage(enemies[i].Health.MaxHealth);
            }

            Assert.Less(Mathf.Abs(Vector3.Dot(enemies[0].Renderer.transform.forward, Vector3.up)), 0.01f);
            Assert.AreEqual(enemies.Length, session.Kills);
            Assert.AreEqual(0, session.RemainingEnemies);
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
            Assert.AreEqual(session.Enemies.Length, session.RemainingEnemies);
            Assert.AreNotSame(firstLevel.Map, session.CurrentLevel.Map);
            Assert.AreSame(session.CurrentLevel.Map, composition.Hud.Minimap.Map);
            Assert.AreEqual(session.CurrentLevel.Map.EntranceRoomIndex, session.CurrentLevel.Map.CurrentRoomIndex);
            Assert.IsFalse(session.CurrentLevel.Map.IsRoomDiscovered(session.CurrentLevel.Map.ExitRoomIndex));
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

        [UnityTest]
        public IEnumerator StartingAnotherRunSpawnsAnotherMonologue()
        {
            _scene = EditorSceneManager.LoadSceneInPlayMode("Assets/MazeHell/Scenes/Game.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            NarrativePopup firstPopup = null;

            foreach (GameObject root in _scene.GetRootGameObjects())
            {
                if (root.TryGetComponent(out GameComposition composition))
                {
                    firstPopup = composition.Hud.PopupRoot.GetComponentInChildren<NarrativePopup>();
                }
            }

            Assert.IsNotNull(firstPopup);
            EntityId firstIdentity = firstPopup.GetEntityId();
            _scene = EditorSceneManager.LoadSceneInPlayMode("Assets/MazeHell/Scenes/Game.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            NarrativePopup nextPopup = null;

            foreach (GameObject root in _scene.GetRootGameObjects())
            {
                if (root.TryGetComponent(out GameComposition composition))
                {
                    nextPopup = composition.Hud.PopupRoot.GetComponentInChildren<NarrativePopup>();
                }
            }

            Assert.IsNotNull(nextPopup);
            Assert.AreNotEqual(firstIdentity, nextPopup.GetEntityId());
        }
    }
}
#endif
