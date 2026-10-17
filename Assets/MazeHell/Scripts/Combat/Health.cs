using System;
using UnityEngine;

namespace MazeHell.Combat
{
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour
    {
        public event Action<Health> Changed;

        public event Action<Health> Died;

        private float _maxHealth;
        private float _currentHealth;
        private bool _isDead;
        private bool _isSetupFinished;

        public float MaxHealth => _maxHealth;

        public float CurrentHealth => _currentHealth;

        public bool IsDead => _isDead;

        public void Setup(float maxHealth)
        {
            if (float.IsNaN(maxHealth) || float.IsInfinity(maxHealth) || maxHealth <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHealth), $"{name}: max health must be positive, got {maxHealth}.");
            }

            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
            _isDead = false;
            _isSetupFinished = true;

            Changed?.Invoke(this);
        }

        public float ApplyDamage(float amount)
        {
            AssertSetup();

            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), $"{name}: damage amount must be non-negative, got {amount}.");
            }

            if (_isDead)
            {
                return 0f;
            }

            float actualDamage = Mathf.Min(amount, _currentHealth);

            if (actualDamage <= 0f)
            {
                return 0f;
            }

            _currentHealth -= actualDamage;

            bool hasDied = _currentHealth <= 0f;

            if (hasDied)
            {
                _currentHealth = 0f;
                _isDead = true;
            }

            Changed?.Invoke(this);

            if (hasDied)
            {
                Died?.Invoke(this);
            }

            return actualDamage;
        }

        public float Heal(float amount)
        {
            AssertSetup();

            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), $"{name}: heal amount must be non-negative, got {amount}.");
            }

            if (_isDead)
            {
                return 0f;
            }

            float missingHealth = _maxHealth - _currentHealth;
            float actualHeal = Mathf.Min(amount, missingHealth);

            if (actualHeal <= 0f)
            {
                return 0f;
            }

            _currentHealth += actualHeal;

            Changed?.Invoke(this);

            return actualHeal;
        }

        public void ResetHealth()
        {
            AssertSetup();

            if (_currentHealth == _maxHealth && _isDead == false)
            {
                return;
            }

            _currentHealth = _maxHealth;
            _isDead = false;

            Changed?.Invoke(this);
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: health is not initialized, call Setup first.");
            }
        }
    }
}
