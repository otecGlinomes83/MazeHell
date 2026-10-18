using MazeHell.Combat;
using MazeHell.Configuration;
using MazeHell.Input;
using MazeHell.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MazeHell.Tests
{
    public class CombatTests
    {
        private GameObject _root;
        private ShotgunConfig _config;
        private PlayerMovementConfig _movementConfig;
        private PlayerCombatConfig _combatConfig;
        private Shotgun _gun;
        private ShotNoise _noise;
        private Health _targetHealth;
        private DamageTarget _target;
        private int _noiseCount;
        private int _impactCount;
        private bool _wasVolley;

        [SetUp]
        public void SetUp()
        {
            _noiseCount = 0;
            _impactCount = 0;
            _wasVolley = false;
            _root = new GameObject("CombatTests");
            _root.transform.position = new Vector3(1000f, 1000f, 1000f);
            Camera camera = _root.AddComponent<Camera>();
            _config = ScriptableObject.CreateInstance<ShotgunConfig>();
            _movementConfig = ScriptableObject.CreateInstance<PlayerMovementConfig>();
            _combatConfig = ScriptableObject.CreateInstance<PlayerCombatConfig>();
            _noise = new ShotNoise();
            _noise.Emitted += OnNoise;
            _gun = _root.AddComponent<Shotgun>();
            _gun.Setup(_config, camera, _noise);
            _gun.Fired += OnFired;
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.transform.SetParent(_root.transform, false);
            target.transform.localPosition = Vector3.forward * 5f;
            target.transform.localScale = new Vector3(4f, 4f, 1f);
            _targetHealth = target.AddComponent<Health>();
            _targetHealth.Setup(100f);
            _target = target.AddComponent<DamageTarget>();
            _target.Setup(_targetHealth);
            _target.Impacted += OnImpacted;
            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            _noise.Emitted -= OnNoise;
            _gun.Fired -= OnFired;
            _target.Impacted -= OnImpacted;
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_movementConfig);
            Object.DestroyImmediate(_combatConfig);
        }

        [Test]
        public void ShotDealsPelletDamageButKnocksTargetOnlyOnce()
        {
            _gun.Tick(FireInput(), 0f);

            Assert.AreEqual(80f, _targetHealth.CurrentHealth);
            Assert.AreEqual(1, _impactCount);
            Assert.AreEqual(1, _noiseCount);
            Assert.AreEqual(1, _gun.Ammo);
            Assert.IsFalse(_wasVolley);
        }

        [Test]
        public void FastSecondClickCompletesFortyDamageVolleyAndReloads()
        {
            _gun.Tick(FireInput(), 0f);
            _gun.Tick(FireInput(), _config.MinShotInterval);

            Assert.AreEqual(60f, _targetHealth.CurrentHealth);
            Assert.IsTrue(_wasVolley);
            Assert.AreEqual(0, _gun.Ammo);
            Assert.AreEqual(ShotgunState.Reloading, _gun.State);
            _gun.Tick(new InputSnapshot(), _config.ReloadDuration);
            Assert.AreEqual(2, _gun.Ammo);
            Assert.AreEqual(ShotgunState.Ready, _gun.State);
        }

        [Test]
        public void BlockedOrTooFastClicksEmitNoExtraShotNoise()
        {
            InputSnapshot blocked = new InputSnapshot(Vector2.zero, Vector2.zero, false, false, true, true, false);
            _gun.Tick(blocked, 0f);
            Assert.AreEqual(0, _noiseCount);
            _gun.Tick(FireInput(), 0f);
            _gun.Tick(FireInput(), _config.MinShotInterval * 0.25f);
            Assert.AreEqual(1, _noiseCount);
            Assert.AreEqual(1, _gun.Ammo);
        }

        [Test]
        public void HoldingBlockOnCooldownDoesNotReduceSecondHit()
        {
            PlayerDefense defense = CreateDefense();
            defense.Tick(true, 0f);
            float stun = defense.ReceiveMelee(20f, Vector3.back);
            Assert.IsTrue(defense.TryGetComponent(out Health health));
            Assert.Greater(stun, 0f);
            Assert.AreEqual(94f, health.CurrentHealth);
            defense.ReceiveMelee(20f, Vector3.back);
            Assert.AreEqual(74f, health.CurrentHealth);
        }

        [Test]
        public void ZeroDamageBlockStillConsumesCooldownAndStuns()
        {
            SerializedObject config = new SerializedObject(_combatConfig);
            config.FindProperty("_blockDamageMultiplier").floatValue = 0f;
            config.ApplyModifiedPropertiesWithoutUndo();
            PlayerDefense defense = CreateDefense();
            defense.Tick(true, 0f);
            float stun = defense.ReceiveMelee(20f, Vector3.back);
            Assert.Greater(stun, 0f);
            Assert.Greater(defense.BlockCooldownRemaining, 0f);
        }

        [Test]
        public void ManualReloadTopsUpOneShell()
        {
            _gun.Tick(FireInput(), 0f);
            Assert.AreEqual(1, _gun.Ammo);
            InputSnapshot reload = new InputSnapshot(Vector2.zero, Vector2.zero, false, false, false, false, false, true);
            _gun.Tick(reload, _config.MinShotInterval);
            Assert.AreEqual(ShotgunState.Reloading, _gun.State);
            _gun.Tick(new InputSnapshot(), _config.ReloadDuration);
            Assert.AreEqual(2, _gun.Ammo);
            Assert.AreEqual(ShotgunState.Ready, _gun.State);
        }

        [Test]
        public void FireInterruptsManualReloadWithLoadedShell()
        {
            _gun.Tick(FireInput(), 0f);
            InputSnapshot reload = new InputSnapshot(Vector2.zero, Vector2.zero, false, false, false, false, false, true);
            _gun.Tick(reload, _config.MinShotInterval);
            _gun.Tick(new InputSnapshot(), _config.ReloadDuration * 0.5f);
            Assert.AreEqual(1, _gun.Ammo);
            _gun.Tick(FireInput(), 0f);
            Assert.AreEqual(0, _gun.Ammo);
            Assert.AreEqual(2, _noiseCount);
        }

        [Test]
        public void EmptyReloadRemainsEmptyUntilWholeCycleCompletes()
        {
            _gun.Tick(FireInput(), 0f);
            _gun.Tick(FireInput(), _config.MinShotInterval);
            _gun.Tick(new InputSnapshot(), _config.ReloadDuration * 0.5f);
            Assert.AreEqual(0, _gun.Ammo);
            Assert.AreEqual(ShotgunState.Reloading, _gun.State);
            _gun.Tick(new InputSnapshot(), _config.ReloadDuration * 0.5f);
            Assert.AreEqual(2, _gun.Ammo);
            Assert.AreEqual(ShotgunState.Ready, _gun.State);
        }

        private PlayerDefense CreateDefense()
        {
            GameObject player = new GameObject("Defense");
            player.transform.SetParent(_root.transform, false);
            CharacterController controller = player.AddComponent<CharacterController>();
            PlayerMotor motor = player.AddComponent<PlayerMotor>();
            motor.Setup(controller, _movementConfig);
            Health health = player.AddComponent<Health>();
            health.Setup(100f);
            PlayerDefense defense = player.AddComponent<PlayerDefense>();
            defense.Setup(_combatConfig, health, motor);
            return defense;
        }

        private InputSnapshot FireInput()
        {
            return new InputSnapshot(Vector2.zero, Vector2.zero, false, false, true, false, false);
        }

        private void OnNoise(Vector3 position, float radius)
        {
            _noiseCount += 1;
        }

        private void OnImpacted(Vector3 impulse)
        {
            _impactCount += 1;
        }

        private void OnFired(bool isVolley)
        {
            _wasVolley = isVolley;
        }
    }
}
