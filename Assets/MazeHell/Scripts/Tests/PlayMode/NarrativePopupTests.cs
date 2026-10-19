#if UNITY_EDITOR
using MazeHell.Combat;
using MazeHell.Configuration;
using MazeHell.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MazeHell.Tests
{
    public class NarrativePopupTests
    {
        private NarrativePopup _popup;
        private HealthPortrait _portrait;
        private Image _image;
        private AnimationLibrary _sprites;
        private HudMotionConfig _config;
        private Sprite[] _speakingFrames;
        private float _frameDuration;

        [SetUp]
        public void SetUp()
        {
            HudView hudPrefab = AssetDatabase.LoadAssetAtPath<HudView>("Assets/MazeHell/Resources/Prefabs/UI/HUD.prefab");
            _portrait = Object.Instantiate(hudPrefab.Portrait);
            Assert.IsTrue(_portrait.TryGetComponent(out _image));
            Health health = _portrait.gameObject.AddComponent<Health>();
            health.Setup(100f);
            _sprites = AssetDatabase.LoadAssetAtPath<AnimationLibrary>("Assets/MazeHell/Scriptables/UI/AnimationLibrary.asset");
            _config = ScriptableObject.CreateInstance<HudMotionConfig>();
            _portrait.Setup(_image, health, _sprites, _config);

            SerializedObject portraitSettings = new SerializedObject(_portrait);
            SerializedProperty frames = portraitSettings.FindProperty("_speakingFrames");
            _speakingFrames = new Sprite[frames.arraySize];

            for (int i = 0; i < frames.arraySize; i++)
            {
                _speakingFrames[i] = (Sprite)frames.GetArrayElementAtIndex(i).objectReferenceValue;
            }

            _frameDuration = portraitSettings.FindProperty("_speakingFrameDuration").floatValue;
            NarrativePopup popupPrefab = AssetDatabase.LoadAssetAtPath<NarrativePopup>(
                "Assets/MazeHell/Resources/Prefabs/UI/Windows/NarrativePopup.prefab");
            _popup = Object.Instantiate(popupPrefab);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_popup.gameObject);
            Object.DestroyImmediate(_portrait.gameObject);
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void PortraitKeepsSpeakingAfterTextFinishesUntilPopupIsDisabled()
        {
            Assert.Greater(_speakingFrames.Length, 1);
            Assert.AreNotEqual(_speakingFrames[0], _speakingFrames[1]);
            _popup.Setup("Intro", _portrait, "Continue");
            _popup.Tick(60f);

            for (int i = 0; i < _speakingFrames.Length * 5; i++)
            {
                _popup.Tick(_frameDuration);
                _portrait.Tick(_frameDuration);
                Assert.AreEqual(_speakingFrames[(i + 1) % _speakingFrames.Length], _image.sprite);
            }

            _portrait.Tick(_frameDuration);
            Assert.AreEqual(_speakingFrames[1], _image.sprite);
            _popup.gameObject.SetActive(false);
            Assert.AreEqual(_sprites.FaceSet.Healthy.Frames[0], _image.sprite);
        }

        [Test]
        public void DisablingPopupBeforeTextFinishesStopsSpeaking()
        {
            _popup.Setup("Intro", _portrait, "Continue");
            _portrait.Tick(_frameDuration);
            Assert.AreEqual(_speakingFrames[1], _image.sprite);
            _popup.gameObject.SetActive(false);
            Assert.AreEqual(_sprites.FaceSet.Healthy.Frames[0], _image.sprite);
            _portrait.Tick(0f);
            Assert.AreEqual(_sprites.FaceSet.Healthy.Frames[0], _image.sprite);
        }
    }
}
#endif
