using System;
using MazeHell.Combat;
using MazeHell.Configuration;
using UnityEngine;

namespace MazeHell.Enemies
{
    [DisallowMultipleComponent]
    public class EnemyPerception : MonoBehaviour
    {
        private const float EyeHeightFraction = 0.5f;
        private const float ConeHalfScale = 0.5f;

        private EnemyConfig _config;
        private Transform _targetTransform;
        private ShotNoise _noise;
        private bool _isSetupFinished;

        private float _perceptionTimer;
        private bool _isVisible;
        private Vector3 _noisePosition;
        private bool _hasFreshNoise;
        private bool _isSubscribed;

        public bool IsTargetVisible => _isVisible;

        public void Setup(Transform targetTransform, ShotNoise noise, EnemyConfig config)
        {
            if (targetTransform == null)
            {
                throw new ArgumentNullException(nameof(targetTransform), $"{name}: target transform is required.");
            }

            if (noise == null)
            {
                throw new ArgumentNullException(nameof(noise), $"{name}: shot noise is required.");
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), $"{name}: enemy config is required.");
            }

            if (config.Perception.PerceptionMask.value == 0)
            {
                throw new ArgumentException($"{name}: perception mask must not be empty.", nameof(config));
            }

            UnsubscribeFromNoise();

            _targetTransform = targetTransform;
            _noise = noise;
            _config = config;
            _isSetupFinished = true;

            _perceptionTimer = 0f;
            _isVisible = false;
            _hasFreshNoise = false;
            _noisePosition = Vector3.zero;

            SubscribeToNoise();
        }

        public void Tick(float deltaTime)
        {
            AssertSetup();

            _perceptionTimer -= deltaTime;

            if (_perceptionTimer > 0f)
            {
                return;
            }

            _perceptionTimer = _config.Perception.PerceptionInterval;
            _isVisible = ComputeVisibility();
        }

        public bool TryConsumeNoise(out Vector3 position)
        {
            AssertSetup();

            if (_hasFreshNoise == false)
            {
                position = Vector3.zero;

                return false;
            }

            _hasFreshNoise = false;
            position = _noisePosition;

            return true;
        }

        private void OnNoiseEmitted(Vector3 position, float radius)
        {
            if (_isSetupFinished == false)
            {
                return;
            }

            float hearRadius = Mathf.Min(_config.Perception.HearingRadius, radius);
            Vector3 offset = position - transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude > hearRadius * hearRadius)
            {
                return;
            }

            _noisePosition = position;
            _hasFreshNoise = true;
        }

        private void OnEnable()
        {
            if (_isSetupFinished)
            {
                SubscribeToNoise();
            }
        }

        private void SubscribeToNoise()
        {
            if (_isSubscribed || _noise == null)
            {
                return;
            }

            _noise.Emitted += OnNoiseEmitted;
            _isSubscribed = true;
        }

        private void UnsubscribeFromNoise()
        {
            if (_isSubscribed == false)
            {
                return;
            }

            _noise.Emitted -= OnNoiseEmitted;
            _isSubscribed = false;
        }

        private bool ComputeVisibility()
        {
            EnemyPerceptionData perception = _config.Perception;
            float chestHeight = _config.Body.BodyHeight * EyeHeightFraction;
            Vector3 eyePosition = transform.position + Vector3.up * chestHeight;
            Vector3 targetChest = _targetTransform.position + Vector3.up * chestHeight;
            Vector3 flatOffset = targetChest - eyePosition;
            flatOffset.y = 0f;
            float flatDistance = flatOffset.magnitude;

            if (flatDistance > perception.VisionRange)
            {
                return false;
            }

            if (flatDistance > 0f)
            {
                Vector3 flatDirection = flatOffset / flatDistance;
                Vector3 forward = transform.forward;
                forward.y = 0f;

                if (Vector3.Dot(forward.normalized, flatDirection) < Mathf.Cos(perception.VisionFovDegrees * ConeHalfScale * Mathf.Deg2Rad))
                {
                    return false;
                }
            }

            Vector3 lineOfSight = targetChest - eyePosition;
            float sightDistance = lineOfSight.magnitude;

            if (sightDistance <= 0f)
            {
                return true;
            }

            if (Physics.Raycast(eyePosition, lineOfSight, out RaycastHit hit, sightDistance, perception.PerceptionMask, QueryTriggerInteraction.Ignore) == false)
            {
                return true;
            }

            return hit.transform.IsChildOf(_targetTransform);
        }

        private void OnDisable()
        {
            UnsubscribeFromNoise();
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: enemy perception is not initialized, call Setup first.");
            }
        }
    }
}
