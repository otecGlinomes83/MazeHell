using System;
using UnityEngine;

namespace MazeHell.Configuration
{
    [CreateAssetMenu(fileName = "CameraMotionConfig", menuName = "MazeHell/Configurations/Camera Motion")]
    public class CameraMotionConfig : ScriptableObject
    {
        [Header("Look")]
        [Range(0.005f, 1f)]
        [SerializeField] private float _mouseSensitivity = 0.12f;

        [Range(0.005f, 1f)]
        [SerializeField] private float _minMouseSensitivity = 0.02f;

        [Range(0.005f, 1f)]
        [SerializeField] private float _maxMouseSensitivity = 0.4f;

        [Range(1f, 89f)]
        [SerializeField] private float _pitchLimitDegrees = 85f;

        [Header("Field Of View")]
        [Range(60f, 120f)]
        [SerializeField] private float _baseFieldOfView = 90f;

        [Range(0f, 30f)]
        [SerializeField] private float _speedFieldOfViewBonus = 8f;

        [Min(1f)]
        [SerializeField] private float _speedFieldOfViewReferenceSpeed = 12f;

        [Min(0.01f)]
        [SerializeField] private float _fieldOfViewSmoothTime = 0.2f;

        [Header("Headbob")]
        [Min(0f)]
        [SerializeField] private float _bobVerticalAmplitude = 0.05f;

        [Min(0f)]
        [SerializeField] private float _bobLateralAmplitude = 0.03f;

        [Min(0.1f)]
        [SerializeField] private float _bobFrequency = 2.2f;

        [Header("Impulses")]
        [SerializeField] private CameraImpulseData _jumpImpulse = new CameraImpulseData();

        [SerializeField] private CameraImpulseData _landImpulse = new CameraImpulseData();

        [SerializeField] private CameraImpulseData _dashImpulse = new CameraImpulseData();

        [SerializeField] private CameraImpulseData _hitImpulse = new CameraImpulseData();

        [SerializeField] private CameraImpulseData _shotImpulse = new CameraImpulseData();

        [SerializeField] private CameraImpulseData _blockImpulse = new CameraImpulseData();

        [Min(1f)]
        [SerializeField] private float _volleyShotMultiplier = 2f;

        [Min(0.01f)]
        [SerializeField] private float _landingImpactReferenceSpeed = 7f;

        [Header("Gate Shake")]
        [Min(0.01f)] [SerializeField] private float _gateShakeDuration = 0.85f;
        [Min(0.1f)] [SerializeField] private float _gateShakeFrequency = 14f;
        [Min(0f)] [SerializeField] private float _gateShakePosition = 0.06f;
        [Min(0f)] [SerializeField] private float _gateShakeDegrees = 1.8f;

        [Header("Limits")]
        [Min(0f)]
        [SerializeField] private float _maxTotalPositionOffset = 0.3f;

        [Range(0f, 30f)]
        [SerializeField] private float _maxTotalRotationDegrees = 8f;

        public float MouseSensitivity => _mouseSensitivity;
        public float GateShakeDuration => _gateShakeDuration;
        public float GateShakeFrequency => _gateShakeFrequency;
        public float GateShakePosition => _gateShakePosition;
        public float GateShakeDegrees => _gateShakeDegrees;

        public float MinMouseSensitivity => _minMouseSensitivity;

        public float MaxMouseSensitivity => _maxMouseSensitivity;

        public float PitchLimitDegrees => _pitchLimitDegrees;

        public float BaseFieldOfView => _baseFieldOfView;

        public float SpeedFieldOfViewBonus => _speedFieldOfViewBonus;

        public float SpeedFieldOfViewReferenceSpeed => _speedFieldOfViewReferenceSpeed;

        public float FieldOfViewSmoothTime => _fieldOfViewSmoothTime;

        public float BobVerticalAmplitude => _bobVerticalAmplitude;

        public float BobLateralAmplitude => _bobLateralAmplitude;

        public float BobFrequency => _bobFrequency;

        public CameraImpulseData JumpImpulse => _jumpImpulse;

        public CameraImpulseData LandImpulse => _landImpulse;

        public CameraImpulseData DashImpulse => _dashImpulse;

        public CameraImpulseData HitImpulse => _hitImpulse;

        public CameraImpulseData ShotImpulse => _shotImpulse;

        public CameraImpulseData BlockImpulse => _blockImpulse;

        public float VolleyShotMultiplier => _volleyShotMultiplier;

        public float LandingImpactReferenceSpeed => _landingImpactReferenceSpeed;

        public float MaxTotalPositionOffset => _maxTotalPositionOffset;

        public float MaxTotalRotationDegrees => _maxTotalRotationDegrees;
    }

    [Serializable]
    public class CameraImpulseData
    {
        [Min(0f)]
        [SerializeField] private float _positionOffset = 0.1f;

        [Min(0f)]
        [SerializeField] private float _rotationDegrees = 2f;

        [Min(0.01f)]
        [SerializeField] private float _attackTime = 0.05f;

        [Min(0.01f)]
        [SerializeField] private float _returnTime = 0.3f;

        public float PositionOffset => _positionOffset;

        public float RotationDegrees => _rotationDegrees;

        public float AttackTime => _attackTime;

        public float ReturnTime => _returnTime;
    }
}
