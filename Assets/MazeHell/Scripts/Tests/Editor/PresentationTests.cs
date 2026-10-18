using MazeHell.Combat;
using MazeHell.Configuration;
using MazeHell.Presentation;
using MazeHell.Input;
using MazeHell.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MazeHell.Tests
{
    public class PresentationTests
    {
        private GameObject _root;
        private CameraMotionConfig _cameraConfig;
        private HudMotionConfig _hudConfig;
        private CameraMotion _motion;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("PresentationTests");
            Camera camera = _root.AddComponent<Camera>();
            _cameraConfig = ScriptableObject.CreateInstance<CameraMotionConfig>();
            _hudConfig = ScriptableObject.CreateInstance<HudMotionConfig>();
            GameObject pivot = new GameObject("MotionPivot");
            pivot.transform.SetParent(_root.transform);
            _motion = _root.AddComponent<CameraMotion>();
            _motion.Setup(pivot.transform, camera, _cameraConfig);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_cameraConfig);
            Object.DestroyImmediate(_hudConfig);
        }

        [Test]
        public void CameraImpulseSurvivesFirstShortFrame()
        {
            _motion.ReactDamage();
            _motion.Tick(0f, true, 0.0001f);
            _motion.Tick(0f, true, 0.1f);

            Assert.Greater(_motion.PositionOffset.sqrMagnitude, 0.00001f);
        }

        [TestCase(20f, 0.1f)]
        [TestCase(20f, 2f)]
        [TestCase(3f, 0.82f)]
        public void HudSpringRemainsFiniteWithLongFrames(float frequency, float damping)
        {
            SerializedObject settings = new SerializedObject(_hudConfig);
            settings.FindProperty("_springFrequency").floatValue = frequency;
            settings.FindProperty("_springDampingRatio").floatValue = damping;
            settings.ApplyModifiedPropertiesWithoutUndo();
            RectTransform hud = CreateRect("HUD");
            RectTransform weapon = CreateRect("Weapon");
            RectTransform crosshair = CreateRect("Crosshair");
            HudSpring spring = hud.gameObject.AddComponent<HudSpring>();
            spring.Setup(hud, weapon, crosshair, _motion, _hudConfig);

            for (int i = 0; i < 150; i++)
            {
                _motion.ReactBlock();
                _motion.Tick(0f, true, 0.2f);
                spring.Tick(0.5f);
                Vector2 offset = hud.anchoredPosition;
                Assert.IsFalse(float.IsNaN(offset.x) || float.IsInfinity(offset.x));
                Assert.IsFalse(float.IsNaN(offset.y) || float.IsInfinity(offset.y));
                Assert.LessOrEqual(offset.magnitude, _hudConfig.MaxPixelDisplacement * Mathf.Sqrt(2f) + 0.001f);
            }
        }

        [Test]
        public void GateShakeMovesCameraThenSettles()
        {
            _motion.ReactGateOpen();
            _motion.Tick(0f, true, 0.016f);
            Assert.Greater(_motion.RotationOffset.sqrMagnitude, 0.001f);
            _motion.Tick(0f, true, _cameraConfig.GateShakeDuration + 0.1f);
            Assert.Less(_motion.RotationOffset.sqrMagnitude, 0.00001f);
        }

        [Test]
        public void PortraitSubscribesWhenSetupRunsOnActiveObject()
        {
            AnimationLibrary sprites = AssetDatabase.LoadAssetAtPath<AnimationLibrary>(
                "Assets/MazeHell/Scriptables/UI/AnimationLibrary.asset");
            Health health = _root.AddComponent<Health>();
            health.Setup(100f);
            RectTransform portraitRoot = CreateRect("Portrait");
            Image image = portraitRoot.gameObject.AddComponent<Image>();
            HealthPortrait portrait = portraitRoot.gameObject.AddComponent<HealthPortrait>();
            portrait.Setup(image, health, sprites, _hudConfig);
            health.ApplyDamage(60f);
            Assert.AreEqual(sprites.FaceSet.Hit.Frames[0], image.sprite);
            health.Heal(30f);
            portrait.Tick(_hudConfig.HitFaceOverrideDuration + 0.1f);
            Assert.AreEqual(sprites.FaceSet.Scratched.Frames[0], image.sprite);
        }

        [Test]
        public void WeaponShowsVolleyBeforeAutomaticReload()
        {
            AnimationLibrary sprites = AssetDatabase.LoadAssetAtPath<AnimationLibrary>(
                "Assets/MazeHell/Scriptables/UI/AnimationLibrary.asset");
            ShotgunConfig config = ScriptableObject.CreateInstance<ShotgunConfig>();
            Shotgun gun = _root.AddComponent<Shotgun>();
            gun.Setup(config, _root.GetComponent<Camera>(), new ShotNoise());
            RectTransform weaponRoot = CreateRect("WeaponAnimation");
            Image image = weaponRoot.gameObject.AddComponent<Image>();
            WeaponSpriteAnimator animator = weaponRoot.gameObject.AddComponent<WeaponSpriteAnimator>();
            animator.Setup(image, gun, sprites, config);
            InputSnapshot fire = new InputSnapshot(Vector2.zero, Vector2.zero, false, false, true, false, false);
            gun.Tick(fire, 0f);
            animator.Tick(0.01f);
            Assert.AreEqual(sprites.WeaponSet.ShotFrames[0], image.sprite);
            gun.Tick(fire, config.MinShotInterval + 0.01f);
            animator.Tick(0.01f);
            Assert.Greater(gun.ReloadRemaining, 0f);
            Assert.AreEqual(sprites.WeaponSet.VolleyFrames[0], image.sprite);
            Object.DestroyImmediate(config);
        }

        private RectTransform CreateRect(string name)
        {
            GameObject child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(_root.transform);
            return (RectTransform)child.transform;
        }
    }
}
