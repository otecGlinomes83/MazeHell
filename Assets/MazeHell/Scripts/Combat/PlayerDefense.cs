using System;
using MazeHell.Configuration;
using MazeHell.Player;
using UnityEngine;

namespace MazeHell.Combat
{
    [DisallowMultipleComponent]
    public class PlayerDefense : MonoBehaviour
    {
        public event Action<float> Damaged;

        public event Action<float> Blocked;

        private const float MinDirectionSqrMagnitude = 0.0001f;

        private PlayerCombatConfig _config;
        private Health _health;
        private PlayerMotor _motor;
        private bool _isSetupFinished;

        private bool _isBlocking;
        private float _blockCooldownTimer;

        public bool IsBlocking => _isBlocking;

        public float BlockCooldownRemaining => _blockCooldownTimer;

        public void Setup(PlayerCombatConfig config, Health health, PlayerMotor motor)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), $"{name}: combat config is required.");
            }

            if (health == null)
            {
                throw new ArgumentNullException(nameof(health), $"{name}: health is required.");
            }

            if (motor == null)
            {
                throw new ArgumentNullException(nameof(motor), $"{name}: player motor is required.");
            }

            _config = config;
            _health = health;
            _motor = motor;
            _isSetupFinished = true;

            _isBlocking = false;
            _blockCooldownTimer = 0f;
        }

        public void Tick(bool isBlocking, float deltaTime)
        {
            AssertSetup();

            if (float.IsFinite(deltaTime) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), $"{name}: delta time must be finite, got {deltaTime}.");
            }

            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), $"{name}: delta time must be non-negative, got {deltaTime}.");
            }

            _isBlocking = isBlocking;
            _blockCooldownTimer = Mathf.Max(0f, _blockCooldownTimer - deltaTime);
        }

        public float ReceiveMelee(float damage, Vector3 awayFromAttacker)
        {
            AssertSetup();

            if (float.IsFinite(damage) == false || damage < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), $"{name}: damage must be finite and non-negative, got {damage}.");
            }

            if (IsFinite(awayFromAttacker) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(awayFromAttacker), $"{name}: knockback direction must be finite, got {awayFromAttacker}.");
            }

            if (damage <= 0f || _health.IsDead || _motor.IsInvulnerable)
            {
                return 0f;
            }

            if (_isBlocking && _blockCooldownTimer <= 0f)
            {
                return Block(damage, awayFromAttacker);
            }

            float actualDamage = _health.ApplyDamage(damage);

            if (actualDamage > 0f)
            {
                _motor.AddKnockback(awayFromAttacker, _config.HitKnockbackDistance);
                Damaged?.Invoke(actualDamage);
            }

            return 0f;
        }

        private float Block(float damage, Vector3 awayFromAttacker)
        {
            float actualDamage = _health.ApplyDamage(damage * _config.BlockDamageMultiplier);

            _blockCooldownTimer = _config.BlockCooldown;

            ApplyBlockKnockback(awayFromAttacker);

            float stunDuration = _config.BlockStunTime;

            if (actualDamage > 0f)
            {
                Damaged?.Invoke(actualDamage);
            }

            Blocked?.Invoke(stunDuration);

            return stunDuration;
        }

        private void ApplyBlockKnockback(Vector3 awayFromAttacker)
        {
            Vector3 direction = awayFromAttacker;
            direction.y = 0f;

            if (direction.sqrMagnitude < MinDirectionSqrMagnitude)
            {
                return;
            }

            _motor.AddKnockback(direction, _config.BlockKnockbackDistance);
        }

        private bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: player defense is not initialized, call Setup first.");
            }
        }
    }
}
