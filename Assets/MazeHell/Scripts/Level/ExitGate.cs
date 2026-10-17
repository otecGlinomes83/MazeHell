using System;
using MazeHell.Configuration;
using UnityEngine;

namespace MazeHell.Level
{
    public class ExitGate : MonoBehaviour
    {
        private Transform _leaf;
        private Light _glow;
        private LevelPresentationConfig _presentationConfig;
        private Vector3 _leafClosedPosition;
        private float _liftProgress;
        private bool _isUnlocked;
        private bool _isSetupFinished;

        public bool IsUnlocked => _isUnlocked;

        public void Setup(Transform leaf, Light glow, LevelPresentationConfig presentationConfig)
        {
            if (leaf == null)
            {
                throw new ArgumentNullException(nameof(leaf));
            }

            if (glow == null)
            {
                throw new ArgumentNullException(nameof(glow));
            }

            if (presentationConfig == null)
            {
                throw new ArgumentNullException(nameof(presentationConfig));
            }

            _leaf = leaf;
            _glow = glow;
            _presentationConfig = presentationConfig;
            _leafClosedPosition = leaf.position;
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

            if (remainingEnemies > 0 || _isUnlocked)
            {
                return;
            }

            _isUnlocked = true;
            _glow.enabled = true;
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
