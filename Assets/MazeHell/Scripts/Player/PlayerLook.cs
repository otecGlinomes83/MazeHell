using System;
using UnityEngine;

namespace MazeHell.Player
{
    [DisallowMultipleComponent]
    public class PlayerLook : MonoBehaviour
    {
        private const float MaxPitchLimitDegrees = 90f;

        private Transform _yawRoot;
        private Transform _pitchPivot;
        private float _sensitivity;
        private float _pitchLimit;
        private float _yaw;
        private float _pitch;
        private bool _isSetupFinished;

        public void Setup(Transform yawRoot, Transform pitchPivot, float sensitivity, float pitchLimit)
        {
            if (yawRoot == null)
            {
                throw new ArgumentNullException(nameof(yawRoot), $"{name}: yaw root is required.");
            }

            if (pitchPivot == null)
            {
                throw new ArgumentNullException(nameof(pitchPivot), $"{name}: pitch pivot is required.");
            }

            if (float.IsFinite(sensitivity) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(sensitivity), $"{name}: sensitivity must be finite, got {sensitivity}.");
            }

            if (float.IsFinite(pitchLimit) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(pitchLimit), $"{name}: pitch limit must be finite, got {pitchLimit}.");
            }

            if (sensitivity < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(sensitivity), $"{name}: sensitivity must be non-negative, got {sensitivity}.");
            }

            if (pitchLimit <= 0f || pitchLimit >= MaxPitchLimitDegrees)
            {
                throw new ArgumentOutOfRangeException(nameof(pitchLimit), $"{name}: pitch limit must be within (0, {MaxPitchLimitDegrees}) degrees, got {pitchLimit}.");
            }

            _yawRoot = yawRoot;
            _pitchPivot = pitchPivot;
            _sensitivity = sensitivity;
            _pitchLimit = pitchLimit;
            _isSetupFinished = true;

            SyncAnglesFromTransforms();
        }

        public void Tick(Vector2 mouseDelta)
        {
            AssertSetup();

            if (IsFinite(mouseDelta) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(mouseDelta), $"{name}: mouse delta must be finite, got {mouseDelta}.");
            }

            _yaw += mouseDelta.x * _sensitivity;
            _pitch -= mouseDelta.y * _sensitivity;
            _pitch = Mathf.Clamp(_pitch, -_pitchLimit, _pitchLimit);

            ApplyRotation();
        }

        private void SyncAnglesFromTransforms()
        {
            _yaw = _yawRoot.eulerAngles.y;

            float localPitch = _pitchPivot.localEulerAngles.x;

            if (localPitch > 180f)
            {
                localPitch -= 360f;
            }

            _pitch = Mathf.Clamp(localPitch, -_pitchLimit, _pitchLimit);

            ApplyRotation();
        }

        private void ApplyRotation()
        {
            _yawRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);
            _pitchPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private bool IsFinite(Vector2 value)
        {
            return float.IsFinite(value.x) && float.IsFinite(value.y);
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: player look is not initialized, call Setup first.");
            }
        }
    }
}
