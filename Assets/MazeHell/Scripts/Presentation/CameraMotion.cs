using System;
using MazeHell.Configuration;
using UnityEngine;

namespace MazeHell.Presentation
{
    [DisallowMultipleComponent]
    public class CameraMotion : MonoBehaviour
    {
        private const int ImpulseCapacity = 8;
        private const float ImpulseFinishThreshold = 0.02f;
        private const float ImpulseMinimalDuration = 0.01f;
        private const float SpeedReferenceFloor = 0.01f;
        private const float FieldOfViewMinimalSmoothTime = 0.01f;
        private const float BobWeightMinimalRate = 1f;

        private struct ImpulseState
        {
            public Vector3 PositionTarget;

            public Vector3 RotationTarget;

            public float ElapsedTime;

            public float AttackTime;

            public float ReturnTime;

            public bool IsActive;
        }

        private Transform _motionPivot;

        private Camera _camera;

        private CameraMotionConfig _config;

        private Vector3 _pivotOriginalLocalPosition;

        private Quaternion _pivotOriginalLocalRotation;

        private ImpulseState[] _impulses = new ImpulseState[ImpulseCapacity];

        private int _impulseCursor;

        private float _bobPhase;

        private float _bobWeight;

        private Vector3 _bobOffset;

        private float _currentFieldOfView;

        private float _fieldOfViewVelocity;

        private Vector3 _positionOffset;

        private Vector3 _rotationOffset;

        private bool _isSetupFinished;
        private bool _isGateShaking;
        private float _gateShakeElapsed;

        public Vector3 PositionOffset => _positionOffset;

        public Vector3 RotationOffset => _rotationOffset;

        public void Setup(Transform motionPivot, Camera camera, CameraMotionConfig config)
        {
            if (motionPivot == null)
            {
                throw new ArgumentNullException(nameof(motionPivot));
            }

            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _motionPivot = motionPivot;
            _camera = camera;
            _config = config;

            _pivotOriginalLocalPosition = motionPivot.localPosition;
            _pivotOriginalLocalRotation = motionPivot.localRotation;

            _currentFieldOfView = camera.fieldOfView;
            _impulseCursor = 0;
            _bobPhase = 0f;
            _bobWeight = 0f;
            _fieldOfViewVelocity = 0f;
            _positionOffset = Vector3.zero;
            _rotationOffset = Vector3.zero;
            _isSetupFinished = true;
        }

        public void Tick(float horizontalSpeed, bool isGrounded, float deltaTime)
        {
            AssertSetup();

            float clampedDeltaTime = Mathf.Max(0f, deltaTime);
            float clampedSpeed = Mathf.Max(0f, horizontalSpeed);

            UpdateBob(clampedSpeed, isGrounded, clampedDeltaTime);
            UpdateImpulses(clampedDeltaTime);
            UpdateGateShake(clampedDeltaTime);
            ApplyPivot();
            UpdateFieldOfView(clampedSpeed, clampedDeltaTime);
        }

        public void ReactJump()
        {
            AssertSetup();

            CameraImpulseData jumpImpulse = _config.JumpImpulse;
            AddImpulse(new Vector3(0f, 1f, 0f) * jumpImpulse.PositionOffset,
                new Vector3(-1f, 0f, 0f) * jumpImpulse.RotationDegrees, jumpImpulse);
        }

        public void ReactLanding(float impact)
        {
            AssertSetup();

            CameraImpulseData landImpulse = _config.LandImpulse;
            float clampedImpact = Mathf.Clamp01(Mathf.Max(0f, impact) /
                Mathf.Max(SpeedReferenceFloor, _config.LandingImpactReferenceSpeed));
            AddImpulse(new Vector3(0f, -1f, 0f) * (landImpulse.PositionOffset * clampedImpact),
                new Vector3(1f, 0f, 0f) * (landImpulse.RotationDegrees * clampedImpact), landImpulse);
        }

        public void ReactDash()
        {
            AssertSetup();

            CameraImpulseData dashImpulse = _config.DashImpulse;
            AddImpulse(new Vector3(0f, 1f, 0f) * dashImpulse.PositionOffset,
                new Vector3(-1f, 0f, 0f) * dashImpulse.RotationDegrees, dashImpulse);
        }

        public void ReactDamage()
        {
            AssertSetup();

            CameraImpulseData hitImpulse = _config.HitImpulse;
            AddImpulse(new Vector3(0f, 0f, -1f) * hitImpulse.PositionOffset,
                new Vector3(-1f, 0f, 0f) * hitImpulse.RotationDegrees, hitImpulse);
        }

        public void ReactShot(bool isVolley)
        {
            AssertSetup();

            CameraImpulseData shotImpulse = _config.ShotImpulse;
            float impulseScale = 1f;

            if (isVolley)
            {
                impulseScale = _config.VolleyShotMultiplier;
            }

            AddImpulse(new Vector3(0f, 0f, -1f) * (shotImpulse.PositionOffset * impulseScale),
                new Vector3(-1f, 0f, 0f) * (shotImpulse.RotationDegrees * impulseScale), shotImpulse);
        }

        public void ReactBlock()
        {
            AssertSetup();

            CameraImpulseData blockImpulse = _config.BlockImpulse;
            AddImpulse(new Vector3(0f, 0f, -1f) * blockImpulse.PositionOffset,
                new Vector3(0f, 0f, 1f) * blockImpulse.RotationDegrees, blockImpulse);
        }

        public void ReactGateOpen()
        {
            AssertSetup();
            _gateShakeElapsed = 0f;
            _isGateShaking = true;
        }

        private void UpdateGateShake(float deltaTime)
        {
            if (_isGateShaking == false)
            {
                return;
            }

            _gateShakeElapsed += deltaTime;
            float progress = Mathf.Clamp01(_gateShakeElapsed / _config.GateShakeDuration);
            float envelope = (1f - progress) * (1f - progress);
            float phase = _gateShakeElapsed * _config.GateShakeFrequency * Mathf.PI * 2f;
            Vector3 oscillation = new Vector3(Mathf.Sin(phase), Mathf.Sin(phase * 1.37f), Mathf.Sin(phase * 0.73f));
            _positionOffset = ClampComponentwise(_positionOffset + oscillation * (_config.GateShakePosition * envelope),
                _config.MaxTotalPositionOffset);
            _rotationOffset = ClampComponentwise(_rotationOffset + oscillation * (_config.GateShakeDegrees * envelope),
                _config.MaxTotalRotationDegrees);
            _isGateShaking = progress < 1f;
        }

        public void ResetMotion()
        {
            AssertSetup();

            for (int impulseIndex = 0; impulseIndex < ImpulseCapacity; impulseIndex++)
            {
                _impulses[impulseIndex].IsActive = false;
            }

            _impulseCursor = 0;
            _isGateShaking = false;
            _bobPhase = 0f;
            _bobWeight = 0f;
            _bobOffset = Vector3.zero;
            _positionOffset = Vector3.zero;
            _rotationOffset = Vector3.zero;

            RestoreOriginalPose();
        }

        private void OnDisable()
        {
            if (_isSetupFinished == false)
            {
                return;
            }

            RestoreOriginalPose();
        }

        private void RestoreOriginalPose()
        {
            _motionPivot.localPosition = _pivotOriginalLocalPosition;
            _motionPivot.localRotation = _pivotOriginalLocalRotation;
            _camera.fieldOfView = _currentFieldOfView;
        }

        private void UpdateBob(float horizontalSpeed, bool isGrounded, float deltaTime)
        {
            float referenceSpeed = Mathf.Max(SpeedReferenceFloor, _config.SpeedFieldOfViewReferenceSpeed);
            float speedFraction = Mathf.Clamp01(horizontalSpeed / referenceSpeed);

            float targetWeight = 0f;

            if (isGrounded && speedFraction > 0f)
            {
                targetWeight = 1f;
            }

            float bobWeightRate = Mathf.Max(BobWeightMinimalRate, _config.BobFrequency);
            _bobWeight = Mathf.MoveTowards(_bobWeight, targetWeight, deltaTime * bobWeightRate);

            if (isGrounded)
            {
                _bobPhase += deltaTime * _config.BobFrequency * (horizontalSpeed / referenceSpeed);
            }

            float bobAngle = _bobPhase * Mathf.PI * 2f;
            float bobX = Mathf.Sin(bobAngle) * _config.BobLateralAmplitude * speedFraction * _bobWeight;
            float bobY = Mathf.Sin(bobAngle * 2f) * _config.BobVerticalAmplitude * speedFraction * _bobWeight;

            _bobOffset = new Vector3(bobX, bobY, 0f);
        }

        private void UpdateImpulses(float deltaTime)
        {
            Vector3 impulsePosition = Vector3.zero;
            Vector3 impulseRotation = Vector3.zero;

            for (int impulseIndex = 0; impulseIndex < ImpulseCapacity; impulseIndex++)
            {
                ImpulseState impulse = _impulses[impulseIndex];

                if (impulse.IsActive == false)
                {
                    continue;
                }

                impulse.ElapsedTime += deltaTime;

                float envelope = EvaluateImpulseEnvelope(impulse);

                if (impulse.ElapsedTime >= impulse.AttackTime && envelope <= ImpulseFinishThreshold)
                {
                    impulse.IsActive = false;
                    _impulses[impulseIndex] = impulse;
                    continue;
                }

                impulsePosition += impulse.PositionTarget * envelope;
                impulseRotation += impulse.RotationTarget * envelope;

                _impulses[impulseIndex] = impulse;
            }

            Vector3 totalPosition = impulsePosition + _bobOffset;
            _positionOffset = ClampComponentwise(totalPosition, _config.MaxTotalPositionOffset);
            _rotationOffset = ClampComponentwise(impulseRotation, _config.MaxTotalRotationDegrees);
        }

        private static float EvaluateImpulseEnvelope(ImpulseState impulse)
        {
            if (impulse.ElapsedTime < impulse.AttackTime)
            {
                float attackProgress = impulse.ElapsedTime / impulse.AttackTime;
                return Mathf.SmoothStep(0f, 1f, attackProgress);
            }

            float returnProgress = (impulse.ElapsedTime - impulse.AttackTime) / impulse.ReturnTime;
            return (1f + returnProgress) * Mathf.Exp(-returnProgress);
        }

        private static Vector3 ClampComponentwise(Vector3 value, float limit)
        {
            return new Vector3(
                Mathf.Clamp(value.x, -limit, limit),
                Mathf.Clamp(value.y, -limit, limit),
                Mathf.Clamp(value.z, -limit, limit));
        }

        private void ApplyPivot()
        {
            _motionPivot.localPosition = _pivotOriginalLocalPosition + _positionOffset;
            _motionPivot.localRotation = _pivotOriginalLocalRotation * Quaternion.Euler(_rotationOffset);
        }

        private void UpdateFieldOfView(float horizontalSpeed, float deltaTime)
        {
            float referenceSpeed = Mathf.Max(SpeedReferenceFloor, _config.SpeedFieldOfViewReferenceSpeed);
            float speedFraction = Mathf.Clamp01(horizontalSpeed / referenceSpeed);
            float targetFieldOfView = _config.BaseFieldOfView + _config.SpeedFieldOfViewBonus * speedFraction;

            _currentFieldOfView = Mathf.SmoothDamp(_currentFieldOfView, targetFieldOfView, ref _fieldOfViewVelocity,
                Mathf.Max(FieldOfViewMinimalSmoothTime, _config.FieldOfViewSmoothTime), Mathf.Infinity, deltaTime);

            _camera.fieldOfView = _currentFieldOfView;
        }

        private void AddImpulse(Vector3 positionTarget, Vector3 rotationTarget, CameraImpulseData impulseData)
        {
            ImpulseState impulse = new ImpulseState();
            impulse.PositionTarget = positionTarget;
            impulse.RotationTarget = rotationTarget;
            impulse.ElapsedTime = 0f;
            impulse.AttackTime = Mathf.Max(ImpulseMinimalDuration, impulseData.AttackTime);
            impulse.ReturnTime = Mathf.Max(ImpulseMinimalDuration, impulseData.ReturnTime);
            impulse.IsActive = true;

            _impulses[_impulseCursor] = impulse;
            _impulseCursor = (_impulseCursor + 1) % ImpulseCapacity;
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: camera motion is not initialized, call Setup first.");
            }
        }
    }
}
