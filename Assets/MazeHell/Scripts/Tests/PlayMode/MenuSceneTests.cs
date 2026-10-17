#if UNITY_EDITOR
using System.Collections;
using MazeHell.Configuration;
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
    public class MenuSceneTests
    {
        private RecordStorage _records;
        private GameLanguage _language;
        private float _volume;
        private Scene _menu;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _records = new RecordStorage(1f, 0.12f);
            _language = _records.Language;
            _volume = _records.Volume;
            _menu = EditorSceneManager.LoadSceneInPlayMode("Assets/MazeHell/Scenes/Menu.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            RecordStorage restored = new RecordStorage(1f, 0.12f);
            restored.SetLanguage(_language);
            restored.SetVolume(_volume);
            Scene blank = SceneManager.CreateScene("MenuTestCleanup");
            SceneManager.SetActiveScene(blank);
            yield return SceneManager.UnloadSceneAsync(_menu);
        }

        [UnityTest]
        public IEnumerator SerializedMenuSupportsLanguageSoundAndLeaderboardActions()
        {
            MenuView view = null;
            GameObject[] roots = _menu.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].TryGetComponent(out MenuView candidate))
                {
                    view = candidate;
                    break;
                }
            }

            Assert.IsNotNull(view);
            Assert.AreEqual("MAZEHELL", view.Title.text);
            Assert.IsFalse(string.IsNullOrEmpty(view.StatusText.text));
            PointerEventData click = new PointerEventData(EventSystem.current);
            string language = view.LanguageText.text;
            view.LanguageButton.OnPointerClick(click);
            Assert.AreNotEqual(language, view.LanguageText.text);
            view.LanguageButton.OnPointerClick(click);
            Assert.AreEqual(language, view.LanguageText.text);
            string sound = view.SoundText.text;
            view.SoundButton.OnPointerClick(click);
            Assert.AreNotEqual(sound, view.SoundText.text);
            view.LeaderboardButton.OnPointerClick(click);
            Assert.IsFalse(string.IsNullOrEmpty(view.StatusText.text));
            Assert.AreEqual(CursorLockMode.None, Cursor.lockState);
            yield return null;
        }
    }
}
#endif
