using System;
using UnityEngine;

namespace MazeHell.Combat
{
    [DisallowMultipleComponent]
    public class DamageTarget : MonoBehaviour
    {
        public event Action<Vector3> Impacted;

        private Health _health;
        private bool _isSetupFinished;

        public Health Health => _health;

        public void Setup(Health health)
        {
            if (health == null)
            {
                throw new ArgumentNullException(nameof(health), $"{name}: health is required.");
            }

            _health = health;
            _isSetupFinished = true;
        }

        public float Receive(float damage, Vector3 direction, float knockbackSpeed)
        {
            AssertSetup();

            if (float.IsFinite(damage) == false || damage < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), $"{name}: damage must be finite and non-negative, got {damage}.");
            }

            if (float.IsFinite(knockbackSpeed) == false || knockbackSpeed < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(knockbackSpeed), $"{name}: knockback speed must be finite and non-negative, got {knockbackSpeed}.");
            }

            if (IsFinite(direction) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(direction), $"{name}: impact direction must be finite, got {direction}.");
            }

            float actualDamage = _health.ApplyDamage(damage);

            if (actualDamage <= 0f)
            {
                return 0f;
            }

            if (knockbackSpeed > 0f)
            {
                Impacted?.Invoke(direction.normalized * knockbackSpeed);
            }

            return actualDamage;
        }

        private bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: damage target is not initialized, call Setup first.");
            }
        }
    }
}
