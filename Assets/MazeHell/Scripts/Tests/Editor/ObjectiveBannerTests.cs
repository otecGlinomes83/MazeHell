using MazeHell.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MazeHell.Tests
{
    public class ObjectiveBannerTests
    {
        private HudView _view;
        private ObjectiveBanner _banner;
        private Vector2 _basePosition;
        private float _baseAlpha;

        [SetUp]
        public void SetUp()
        {
            HudView prefab = AssetDatabase.LoadAssetAtPath<HudView>("Assets/MazeHell/Resources/Prefabs/UI/HUD.prefab");
            _view = Object.Instantiate(prefab);
            _basePosition = _view.ObjectiveText.rectTransform.anchoredPosition;
            _baseAlpha = _view.ObjectiveText.alpha;
            _banner = new ObjectiveBanner(_view.ObjectiveText);
            _banner.Reset("Find exit");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_view.gameObject);
        }

        [Test]
        public void PreviousObjectiveLeavesUpBeforeNewObjectiveDescends()
        {
            _banner.Change("Kill enemies: 8");
            _banner.Tick(0.1f);
            Assert.AreEqual("Find exit", _view.ObjectiveText.text);
            Assert.Greater(_view.ObjectiveText.rectTransform.anchoredPosition.y, _basePosition.y);
            Assert.Less(_view.ObjectiveText.alpha, _baseAlpha);
            _banner.Tick(0.3f);
            Assert.AreEqual("Kill enemies: 8", _view.ObjectiveText.text);
            Assert.Greater(_view.ObjectiveText.rectTransform.anchoredPosition.y, _basePosition.y);
            _banner.Tick(1f);
            Assert.AreEqual(_basePosition, _view.ObjectiveText.rectTransform.anchoredPosition);
            Assert.AreEqual(_baseAlpha, _view.ObjectiveText.alpha);
        }

        [Test]
        public void EnemyCountUpdatesDuringAndAfterTransitionWithoutRestartingIt()
        {
            _banner.Change("Kill enemies: 8");
            _banner.Tick(0.1f);
            _banner.UpdateText("Kill enemies: 7");
            Assert.AreEqual("Find exit", _view.ObjectiveText.text);
            _banner.Tick(1f);
            Assert.AreEqual("Kill enemies: 7", _view.ObjectiveText.text);
            _banner.UpdateText("Kill enemies: 6");
            Assert.AreEqual("Kill enemies: 6", _view.ObjectiveText.text);
            Assert.AreEqual(_basePosition, _view.ObjectiveText.rectTransform.anchoredPosition);
        }

        [Test]
        public void NewLevelCancelsPendingObjectiveAndRestoresPosition()
        {
            _banner.Change("Kill enemies: 8");
            _banner.Tick(0.2f);
            _banner.Reset("Find next exit");
            _banner.Tick(1f);
            Assert.AreEqual("Find next exit", _view.ObjectiveText.text);
            Assert.AreEqual(_basePosition, _view.ObjectiveText.rectTransform.anchoredPosition);
            Assert.AreEqual(_baseAlpha, _view.ObjectiveText.alpha);
        }
    }
}
