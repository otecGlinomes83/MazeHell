using UnityEngine;

namespace MazeHell.Configuration
{
    [CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "MazeHell/Configurations/Player Movement")]
    public class PlayerMovementConfig : ScriptableObject
    {
        [Header("Body")]
        [Min(0.1f)]
        [SerializeField] private float _bodyHeight = 1.8f;

        [Min(0.05f)]
        [SerializeField] private float _bodyRadius = 0.35f;

        [Min(0f)]
        [SerializeField] private float _eyeHeight = 1.62f;

        [Min(0f)]
        [SerializeField] private float _stepOffset = 0f;

        [Min(0.001f)]
        [SerializeField] private float _skinWidth = 0.04f;

        [Range(0f, 90f)]
        [SerializeField] private float _slopeLimit = 5f;

        [Header("Ground Movement")]
        [SerializeField] private bool _normalizeDiagonalMovement = false;
        [Min(0.5f)]
        [SerializeField] private float _baseSpeed = 7f;

        [Min(0f)]
        [SerializeField] private float _groundAcceleration = 60f;

        [Min(0f)]
        [SerializeField] private float _groundFriction = 8f;

        [Header("Air Control")]
        [Min(0f)]
        [SerializeField] private float _airAcceleration = 12f;

        [Min(0.1f)]
        [SerializeField] private float _airWishSpeed = 1f;

        [Header("Jump")]
        [Min(1f)]
        [SerializeField] private float _gravity = 20f;

        [Range(0.2f, 3f)]
        [SerializeField] private float _jumpHeight = 1f;

        [Range(0f, 0.5f)]
        [SerializeField] private float _jumpBufferTime = 0.15f;

        [Range(0f, 0.5f)]
        [SerializeField] private float _coyoteTime = 0.15f;

        [Range(0f, 0.5f)]
        [SerializeField] private float _bhopGraceTime = 0.15f;

        [Min(0f)]
        [SerializeField] private float _bhopSpeedGain = 0.6f;

        [Header("Speed Cap")]
        [Min(1f)]
        [SerializeField] private float _softSpeedCap = 12f;

        [Min(0f)]
        [SerializeField] private float _capDecay = 4f;

        [Header("Dash")]
        [Min(1f)]
        [SerializeField] private float _dashSpeedMultiplier = 3f;

        [Range(0.05f, 1f)]
        [SerializeField] private float _dashDuration = 0.18f;

        [Min(0f)]
        [SerializeField] private float _dashCooldown = 1.5f;

        [Range(0f, 1f)]
        [SerializeField] private float _dashInvulnerabilityTime = 0.2f;

        [Min(0f)]
        [SerializeField] private float _minimumMoveDistance = 0f;

        [Header("Ground Probe")]
        [Min(0.01f)]
        [SerializeField] private float _groundedProbeDistance = 0.25f;

        [Range(0.005f, 0.2f)]
        [SerializeField] private float _groundedProbeSkin = 0.08f;

        [Header("Knockback")]
        [Min(0f)]
        [SerializeField] private float _knockbackDecay = 8f;

        [Header("Block Stance")]
        [Range(0.1f, 1f)]
        [SerializeField] private float _blockSpeedMultiplier = 0.6f;

        public float BodyHeight => _bodyHeight;

        public float BodyRadius => _bodyRadius;

        public float EyeHeight => _eyeHeight;

        public float StepOffset => _stepOffset;

        public float SkinWidth => _skinWidth;

        public float SlopeLimit => _slopeLimit;

        public float BaseSpeed => _baseSpeed;

        public bool NormalizeDiagonalMovement => _normalizeDiagonalMovement;

        public float GroundAcceleration => _groundAcceleration;

        public float GroundFriction => _groundFriction;

        public float AirAcceleration => _airAcceleration;

        public float AirWishSpeed => _airWishSpeed;

        public float Gravity => _gravity;

        public float JumpHeight => _jumpHeight;

        public float JumpBufferTime => _jumpBufferTime;

        public float CoyoteTime => _coyoteTime;

        public float BhopGraceTime => _bhopGraceTime;

        public float BhopSpeedGain => _bhopSpeedGain;

        public float SoftSpeedCap => _softSpeedCap;

        public float CapDecay => _capDecay;

        public float DashSpeedMultiplier => _dashSpeedMultiplier;

        public float DashDuration => _dashDuration;

        public float DashCooldown => _dashCooldown;

        public float DashInvulnerabilityTime => _dashInvulnerabilityTime;

        public float MinimumMoveDistance => _minimumMoveDistance;

        public float GroundedProbeDistance => _groundedProbeDistance;

        public float GroundedProbeSkin => _groundedProbeSkin;

        public float KnockbackDecay => _knockbackDecay;

        public float BlockSpeedMultiplier => _blockSpeedMultiplier;
    }
}
