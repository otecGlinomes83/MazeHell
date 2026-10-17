#if UNITY_INCLUDE_TESTS
using System;
using NUnit.Framework;
using MazeHell.Combat;
using UnityEngine;

namespace MazeHell.Tests
{
    public class HealthTests
    {
        private const float MaxHealth = 100f;

        private GameObject _gameObject;
        private Health _health;
        private int _changedCount;
        private int _diedCount;
        private bool _wasDeadWhenChanged;

        [SetUp]
        public void SetUp()
        {
            _changedCount = 0;
            _diedCount = 0;
            _wasDeadWhenChanged = false;
            _gameObject = new GameObject("HealthTests");
            _health = _gameObject.AddComponent<Health>();

            _health.Setup(MaxHealth);
            _health.Changed += OnHealthChanged;
            _health.Died += OnHealthDied;
        }

        [TearDown]
        public void TearDown()
        {
            _health.Changed -= OnHealthChanged;
            _health.Died -= OnHealthDied;

            GameObject.DestroyImmediate(_gameObject);
        }

        [Test]
        public void SetupInitializesCurrentHealthToMax()
        {
            Assert.AreEqual(MaxHealth, _health.CurrentHealth);
            Assert.AreEqual(MaxHealth, _health.MaxHealth);
            Assert.IsFalse(_health.IsDead);
        }

        [Test]
        public void SetupRejectsNonPositiveMaxHealth()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _health.Setup(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => _health.Setup(-5f));
        }

        [Test]
        public void SetupReinitializesState()
        {
            _health.ApplyDamage(40f);
            _health.ResetHealth();

            _health.Setup(60f);

            Assert.AreEqual(60f, _health.CurrentHealth);
            Assert.AreEqual(60f, _health.MaxHealth);
            Assert.IsFalse(_health.IsDead);
        }

        [Test]
        public void ApplyDamageReducesCurrentHealthAndReturnsActual()
        {
            float actualDamage = _health.ApplyDamage(30f);

            Assert.AreEqual(30f, actualDamage);
            Assert.AreEqual(70f, _health.CurrentHealth);
            Assert.IsFalse(_health.IsDead);
            Assert.AreEqual(1, _changedCount);
            Assert.AreEqual(0, _diedCount);
        }

        [Test]
        public void ApplyDamageClampsToRemainingHealth()
        {
            float actualDamage = _health.ApplyDamage(250f);

            Assert.AreEqual(MaxHealth, actualDamage);
            Assert.AreEqual(0f, _health.CurrentHealth);
            Assert.IsTrue(_health.IsDead);
            Assert.AreEqual(1, _diedCount);
        }

        [Test]
        public void ZeroDamageChangesNothing()
        {
            float actualDamage = _health.ApplyDamage(0f);

            Assert.AreEqual(0f, actualDamage);
            Assert.AreEqual(MaxHealth, _health.CurrentHealth);
            Assert.AreEqual(0, _changedCount);
            Assert.AreEqual(0, _diedCount);
        }

        [Test]
        public void ApplyDamageAfterDeathChangesNothing()
        {
            _health.ApplyDamage(MaxHealth);

            float actualDamage = _health.ApplyDamage(20f);

            Assert.AreEqual(0f, actualDamage);
            Assert.AreEqual(0f, _health.CurrentHealth);
            Assert.AreEqual(1, _changedCount);
            Assert.AreEqual(1, _diedCount);
        }

        [Test]
        public void DeathFiresDiedExactlyOnce()
        {
            _health.ApplyDamage(MaxHealth);
            _health.ApplyDamage(50f);
            _health.Heal(10f);

            Assert.AreEqual(1, _diedCount);
        }

        [Test]
        public void DiedFiresOncePerDeathAfterReset()
        {
            _health.ApplyDamage(MaxHealth);
            _health.ResetHealth();
            _health.ApplyDamage(MaxHealth);

            Assert.AreEqual(2, _diedCount);
        }

        [Test]
        public void HealOnDeadIsIgnored()
        {
            _health.ApplyDamage(MaxHealth);

            float actualHeal = _health.Heal(10f);

            Assert.AreEqual(0f, actualHeal);
            Assert.AreEqual(0f, _health.CurrentHealth);
            Assert.IsTrue(_health.IsDead);
            Assert.AreEqual(1, _changedCount);
        }

        [Test]
        public void HealClampsAtMaxHealth()
        {
            _health.ApplyDamage(30f);

            float actualHeal = _health.Heal(50f);

            Assert.AreEqual(30f, actualHeal);
            Assert.AreEqual(MaxHealth, _health.CurrentHealth);
            Assert.IsFalse(_health.IsDead);
            Assert.AreEqual(2, _changedCount);
        }

        [Test]
        public void NegativeAmountsAreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _health.ApplyDamage(-1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => _health.Heal(-1f));
        }

        [Test]
        public void ApplyDamageBeforeSetupFails()
        {
            Health freshHealth = new GameObject("HealthFresh").AddComponent<Health>();

            Assert.Throws<InvalidOperationException>(() => freshHealth.ApplyDamage(1f));

            GameObject.DestroyImmediate(freshHealth.gameObject);
        }

        [Test]
        public void ResetHealthRestoresAndRevives()
        {
            _health.ApplyDamage(MaxHealth);
            _health.ResetHealth();

            Assert.AreEqual(MaxHealth, _health.CurrentHealth);
            Assert.IsFalse(_health.IsDead);
            Assert.AreEqual(2, _changedCount);

            float actualDamage = _health.ApplyDamage(10f);

            Assert.AreEqual(10f, actualDamage);
        }

        [Test]
        public void ResetHealthOnFullHealthFiresNothing()
        {
            _health.ResetHealth();

            Assert.AreEqual(0, _changedCount);
            Assert.AreEqual(0, _diedCount);
        }

        [Test]
        public void LethalChangeExposesCommittedDeathState()
        {
            _health.ApplyDamage(MaxHealth);

            Assert.IsTrue(_wasDeadWhenChanged);
            Assert.AreEqual(1, _diedCount);
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void NonFiniteValuesAreRejected(float amount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _health.Setup(amount));
            Assert.Throws<ArgumentOutOfRangeException>(() => _health.ApplyDamage(amount));
            Assert.Throws<ArgumentOutOfRangeException>(() => _health.Heal(amount));
            Assert.AreEqual(MaxHealth, _health.CurrentHealth);
        }

        private void OnHealthChanged(Health health)
        {
            _changedCount += 1;
            _wasDeadWhenChanged = health.IsDead;
        }

        private void OnHealthDied(Health health)
        {
            _diedCount += 1;
        }
    }
}
#endif
