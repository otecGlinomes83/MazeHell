using UnityEngine;

namespace MazeHell.Configuration
{
    [CreateAssetMenu(fileName = "HudMotionConfig", menuName = "MazeHell/Configurations/Hud Motion")]
    public class HudMotionConfig : ScriptableObject
    {
        [Header("Spring")]
        [Range(0.1f, 20f)]
        [SerializeField] private float _springFrequency = 3f;

        [Range(0.1f, 2f)]
        [SerializeField] private float _springDampingRatio = 0.7f;

        [Range(0.001f, 0.1f)]
        [SerializeField] private float _maxSimulationStep = 0.033f;

        [Header("Camera To Screen Gain")]
        [Min(0f)]
        [SerializeField] private float _positionGainPixelsPerMeter = 10f;

        [Min(0f)]
        [SerializeField] private float _angularGainPixelsPerDegree = 1.5f;

        [Header("Limits")]
        [Range(0f, 200f)]
        [SerializeField] private float _maxPixelDisplacement = 24f;

        [Range(0f, 20f)]
        [SerializeField] private float _maxPixelAngle = 3f;

        [Min(0f)]
        [SerializeField] private float _overscanMarginPixels = 32f;

        [Header("Status Panel")]
        [Range(0f, 1f)]
        [SerializeField] private float _statusMotionMultiplier = 0.06f;

        public float StatusMotionMultiplier => _statusMotionMultiplier;

        [Header("Weapon")]
        [Range(0f, 10f)]
        [SerializeField] private float _weaponGainMultiplier = 3f;

        [Range(0f, 300f)]
        [SerializeField] private float _weaponMaxPixelDisplacement = 60f;

        [Header("Movement Inertia")]
        [Min(0f)] [SerializeField] private float _dashDisplacementPixels = 32f;
        [Min(0f)] [SerializeField] private float _dashForwardDisplacementPixels = 16f;
        [Range(0f, 0.3f)] [SerializeField] private float _dashScaleBoost = 0.08f;
        [Min(0.1f)] [SerializeField] private float _dashZoomResponse = 12f;
        [Min(0f)] [SerializeField] private float _jumpLagPixelsPerSpeed = 2.5f;
        [Min(0f)] [SerializeField] private float _inertiaPixelsPerAcceleration = 0.15f;
        [Min(0f)] [SerializeField] private float _movementMaxDisplacement = 42f;

        public float DashDisplacementPixels => _dashDisplacementPixels;
        public float DashForwardDisplacementPixels => _dashForwardDisplacementPixels;
        public float DashScaleBoost => _dashScaleBoost;
        public float DashZoomResponse => _dashZoomResponse;
        public float JumpLagPixelsPerSpeed => _jumpLagPixelsPerSpeed;
        public float InertiaPixelsPerAcceleration => _inertiaPixelsPerAcceleration;
        public float MovementMaxDisplacement => _movementMaxDisplacement;

        [Header("Face")]
        [Min(0.05f)]
        [SerializeField] private float _hitFaceOverrideDuration = 0.9f;

        [Range(0.01f, 1f)]
        [SerializeField] private float _scratchedHealthFraction = 0.75f;

        [Range(0.01f, 1f)]
        [SerializeField] private float _bleedingHealthFraction = 0.5f;

        [Range(0.01f, 1f)]
        [SerializeField] private float _criticalHealthFraction = 0.25f;

        [Range(0f, 1f)]
        [SerializeField] private float _nearDeathHealthFraction = 0.1f;

        public float SpringFrequency => _springFrequency;

        public float SpringDampingRatio => _springDampingRatio;

        public float MaxSimulationStep => _maxSimulationStep;

        public float PositionGainPixelsPerMeter => _positionGainPixelsPerMeter;

        public float AngularGainPixelsPerDegree => _angularGainPixelsPerDegree;

        public float MaxPixelDisplacement => _maxPixelDisplacement;

        public float MaxPixelAngle => _maxPixelAngle;

        public float OverscanMarginPixels => _overscanMarginPixels;

        public float WeaponGainMultiplier => _weaponGainMultiplier;

        public float WeaponMaxPixelDisplacement => _weaponMaxPixelDisplacement;

        public float HitFaceOverrideDuration => _hitFaceOverrideDuration;

        public float ScratchedHealthFraction => _scratchedHealthFraction;

        public float BleedingHealthFraction => _bleedingHealthFraction;

        public float CriticalHealthFraction => _criticalHealthFraction;

        public float NearDeathHealthFraction => _nearDeathHealthFraction;
    }
}
