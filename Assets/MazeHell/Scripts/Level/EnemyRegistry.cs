using System;
using System.Collections.Generic;
using MazeHell.Combat;

namespace MazeHell.Level
{
    public class EnemyRegistry
    {
        public event Action<int> CountChanged;

        public event Action<float> EnemyKilled;

        private Dictionary<Health, float> _healOnKillByHealth = new Dictionary<Health, float>();
        private int _remainingCount;
        private bool _isSpawnFinished;
        private bool _isSetupFinished;

        public int RemainingCount => _remainingCount;

        public void Setup()
        {
            Clear();

            _isSetupFinished = true;
        }

        public void BeginSpawning()
        {
            AssertSetup();

            _isSpawnFinished = false;
        }

        public void Register(Health health, float healOnKill)
        {
            AssertSetup();

            if (health == null)
            {
                throw new ArgumentNullException(nameof(health), "Registered enemy health is required.");
            }

            if (health.IsDead)
            {
                throw new InvalidOperationException($"{health.name}: dead enemies cannot be registered.");
            }

            if (float.IsFinite(healOnKill) == false || healOnKill < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(healOnKill), $"{health.name}: heal on kill must be finite and non-negative, got {healOnKill}.");
            }

            if (_healOnKillByHealth.ContainsKey(health))
            {
                throw new ArgumentException($"{health.name}: enemy is already registered.", nameof(health));
            }

            _healOnKillByHealth.Add(health, healOnKill);
            _remainingCount += 1;

            health.Died += OnEnemyDied;
        }

        public void FinishSpawning()
        {
            AssertSetup();

            _isSpawnFinished = true;

            CountChanged?.Invoke(_remainingCount);
        }

        public void Clear()
        {
            foreach (Health health in _healOnKillByHealth.Keys)
            {
                health.Died -= OnEnemyDied;
            }

            _healOnKillByHealth.Clear();
            _remainingCount = 0;
            _isSpawnFinished = false;
        }

        private void OnEnemyDied(Health health)
        {
            if (_healOnKillByHealth.Remove(health, out float healOnKill) == false)
            {
                return;
            }

            health.Died -= OnEnemyDied;
            _remainingCount -= 1;

            if (_isSpawnFinished == false)
            {
                return;
            }

            EnemyKilled?.Invoke(healOnKill);
            CountChanged?.Invoke(_remainingCount);
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException("Enemy registry is not initialized, call Setup first.");
            }
        }
    }
}
