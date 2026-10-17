using System;
using MazeHell.Configuration;
using UnityEngine;

namespace MazeHell.Level
{
    public class ExitGate : MonoBehaviour
    {
        [SerializeField] private Transform _leaf;
        private LevelPresentationConfig _presentationConfig;
        private Vector3 _leafClosedPosition;
        private float _liftProgress;
        private bool _isUnlocked;
        private bool _isSetupFinished;
        private bool _enemiesCleared;

        public bool IsUnlocked => _isUnlocked;

        public void Setup(LevelPresentationConfig presentationConfig)
        {
            if (_leaf == null)
            {
                throw new InvalidOperationException($"{name}: gate leaf must be assigned in the prefab.");
            }

            if (presentationConfig == null)
            {
                throw new ArgumentNullException(nameof(presentationConfig));
            }

            _presentationConfig = presentationConfig;
            _leafClosedPosition = _leaf.position;
            _liftProgress = 0f;
            _isUnlocked = false;
            _isSetupFinished = true;
        }

        public void SetRemainingEnemies(int remainingEnemies)
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: {nameof(SetRemainingEnemies)} called before {nameof(Setup)}.");
            }

            if (remainingEnemies < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(remainingEnemies), remainingEnemies, "Remaining enemies must not be negative.");
            }

            _enemiesCleared = remainingEnemies == 0;
        }

        public void TickPlayer(Vector3 position, Vector3 velocity, float bodyHeight, float deltaTime)
        {
            Vector3 offset = position - _leafClosedPosition;
            offset.y = 0f;

            if ((_presentationConfig.ExitRequiresAllKills == false || _enemiesCleared) &&
                offset.magnitude <= _presentationConfig.GateActivationDistance)
            {
                _isUnlocked = true;
            }

            if (_isUnlocked && offset.magnitude <= velocity.magnitude * deltaTime + 0.6f)
            {
                float clearance = Mathf.Min(_presentationConfig.GateLiftHeight, bodyHeight + 0.5f);
                _liftProgress = Mathf.Max(_liftProgress, clearance / Mathf.Max(0.01f, _presentationConfig.GateLiftHeight));
            }

            Tick(deltaTime);
        }

        public void Tick(float deltaTime)
        {
            if (_isSetupFinished == false)
            {
                return;
            }

            if (_isUnlocked == false)
            {
                return;
            }

            if (_liftProgress >= 1f)
            {
                return;
            }

            _liftProgress = Mathf.Min(1f, _liftProgress + deltaTime / _presentationConfig.GateLiftDuration);
            float liftedHeight = _presentationConfig.GateLiftHeight * _liftProgress;
            _leaf.position = _leafClosedPosition + Vector3.up * liftedHeight;
        }
    }
}
