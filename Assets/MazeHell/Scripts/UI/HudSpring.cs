using System;
using MazeHell.Configuration;
using MazeHell.Presentation;
using MazeHell.Player;
using UnityEngine;

namespace MazeHell.UI
{
    [DisallowMultipleComponent]
    public class HudSpring : MonoBehaviour
    {
        private const int MaxSubsteps = 16;

        private const float CriticalDampingTolerance = 0.0001f;

        private const float MinimalOmega = 0.01f;
        private const float StatusMaxDisplacement = 2f;
        private const float StatusMaxAngle = 0.06f;

        [SerializeField] private RectTransform _statusRoot;

        private Vector2 _statusBaseAnchoredPosition;
        private Vector3 _statusBaseLocalEulerAngles;

        private struct SpringState
        {
            public Vector3 Position;

            public Vector3 Velocity;
        }

        private RectTransform _hudRoot;

        private RectTransform _weaponRoot;

        private CameraMotion _motion;

        private HudMotionConfig _config;

        private Vector2 _hudBaseAnchoredPosition;

        private Vector2 _weaponBaseAnchoredPosition;

        private Vector3 _hudBaseLocalEulerAngles;

        private Vector3 _weaponBaseLocalEulerAngles;

        private SpringState _hudSpring;

        private SpringState _weaponSpring;

        private bool _isWeaponChildOfHud;

        private bool _isSetupFinished;
        private PlayerMotor _motor;
        private Vector3 _previousVelocity;
        private SpringState _movementSpring;
        private Vector3 _hudBaseLocalScale;
        private Vector3 _weaponBaseLocalScale;
        private float _dashZoom;

        public void BindMotor(PlayerMotor motor)
        {
            _motor = motor;
            _previousVelocity = motor.Velocity;
        }

        public void Setup(RectTransform hudRoot, RectTransform weaponRoot, RectTransform crosshairRoot,
            CameraMotion motion, HudMotionConfig config)
        {
            if (hudRoot == null)
            {
                throw new ArgumentNullException(nameof(hudRoot));
            }

            if (weaponRoot == null)
            {
                throw new ArgumentNullException(nameof(weaponRoot));
            }

            if (crosshairRoot == null)
            {
                throw new ArgumentNullException(nameof(crosshairRoot));
            }

            if (motion == null)
            {
                throw new ArgumentNullException(nameof(motion));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (crosshairRoot.IsChildOf(hudRoot))
            {
                Vector2 anchoredPosition = crosshairRoot.anchoredPosition;
                crosshairRoot.SetParent(hudRoot.parent, false);
                crosshairRoot.anchoredPosition = anchoredPosition;
            }

            _hudRoot = hudRoot;
            _weaponRoot = weaponRoot;
            _motion = motion;
            _config = config;

            if (_statusRoot != null)
            {
                _statusBaseAnchoredPosition = _statusRoot.anchoredPosition;
                _statusBaseLocalEulerAngles = _statusRoot.localEulerAngles;
            }

            _hudBaseAnchoredPosition = hudRoot.anchoredPosition;
            _weaponBaseAnchoredPosition = weaponRoot.anchoredPosition;
            _hudBaseLocalScale = hudRoot.localScale;
            _weaponBaseLocalScale = weaponRoot.localScale;

            _hudBaseLocalEulerAngles = hudRoot.localEulerAngles;
            _weaponBaseLocalEulerAngles = weaponRoot.localEulerAngles;

            _isWeaponChildOfHud = weaponRoot.IsChildOf(hudRoot);

            _hudSpring = new SpringState();
            _weaponSpring = new SpringState();
            _isSetupFinished = true;

            ApplyAll();
        }

        public void Tick(float deltaTime)
        {
            AssertSetup();

            float clampedDeltaTime = Mathf.Clamp(deltaTime, 0f, _config.MaxSimulationStep * MaxSubsteps);

            Vector3 motionPosition = new Vector3(_motion.PositionOffset.x, _motion.PositionOffset.y, 0f);
            Vector3 motionRotation = _motion.RotationOffset;

            Vector3 hudTarget = BuildTarget(motionPosition, motionRotation, 1f, _config.MaxPixelDisplacement,
                _config.MaxPixelAngle);
            Vector3 weaponTarget = BuildTarget(motionPosition, motionRotation, _config.WeaponGainMultiplier,
                _config.WeaponMaxPixelDisplacement, _config.MaxPixelAngle);

            if (_isWeaponChildOfHud)
            {
                weaponTarget -= _hudSpring.Position;
            }

            float omega = Mathf.Max(MinimalOmega, _config.SpringFrequency) * Mathf.PI * 2f;
            float dampingRatio = _config.SpringDampingRatio;
            _hudSpring = Integrate(_hudSpring, hudTarget, omega, dampingRatio, clampedDeltaTime);
            _weaponSpring = Integrate(_weaponSpring, weaponTarget, omega, dampingRatio, clampedDeltaTime);
            UpdateMovement(clampedDeltaTime, omega, dampingRatio);

            ApplyAll();
        }

        public void ResetState()
        {
            AssertSetup();

            _hudSpring = new SpringState();
            _weaponSpring = new SpringState();
            _movementSpring = new SpringState();
            _previousVelocity = Vector3.zero;
            _dashZoom = 0f;

            ApplyAll();
        }

        private Vector3 BuildTarget(Vector3 motionPosition, Vector3 motionRotation, float gain,
            float maxPixelDisplacement, float maxPixelAngle)
        {
            float positionGain = _config.PositionGainPixelsPerMeter * gain;
            float angularGain = _config.AngularGainPixelsPerDegree * gain;

            float targetX = motionPosition.x * positionGain - motionRotation.y * angularGain;
            float targetY = motionPosition.y * positionGain + motionRotation.x * angularGain;
            float targetRoll = Mathf.Clamp(motionRotation.z, -maxPixelAngle, maxPixelAngle);

            return new Vector3(
                Mathf.Clamp(targetX, -maxPixelDisplacement, maxPixelDisplacement),
                Mathf.Clamp(targetY, -maxPixelDisplacement, maxPixelDisplacement),
                targetRoll);
        }

        private SpringState Integrate(SpringState spring, Vector3 target, float omega, float dampingRatio,
            float deltaTime)
        {
            Vector3 offset = spring.Position - target;
            Vector3 velocity = spring.Velocity;
            float damping = Mathf.Max(0f, dampingRatio);

            if (Mathf.Abs(damping - 1f) < CriticalDampingTolerance)
            {
                float decay = Mathf.Exp(-omega * deltaTime);
                Vector3 coefficient = velocity + omega * offset;
                spring.Position = target + (offset + coefficient * deltaTime) * decay;
                spring.Velocity = (velocity - omega * coefficient * deltaTime) * decay;
                return spring;
            }

            if (damping < 1f)
            {
                float dampedFrequency = omega * Mathf.Sqrt(1f - damping * damping);
                float decay = Mathf.Exp(-damping * omega * deltaTime);
                float cosine = Mathf.Cos(dampedFrequency * deltaTime);
                float sine = Mathf.Sin(dampedFrequency * deltaTime);
                Vector3 coefficient = (velocity + damping * omega * offset) / dampedFrequency;
                spring.Position = target + decay * (offset * cosine + coefficient * sine);
                spring.Velocity = decay * (velocity * cosine -
                    (damping * omega * velocity + omega * omega * offset) * sine / dampedFrequency);
                return spring;
            }

            float discriminant = Mathf.Sqrt(damping * damping - 1f);
            float firstRoot = -omega * (damping - discriminant);
            float secondRoot = -omega * (damping + discriminant);
            Vector3 firstCoefficient = (velocity - secondRoot * offset) / (firstRoot - secondRoot);
            Vector3 secondCoefficient = offset - firstCoefficient;
            float firstDecay = Mathf.Exp(firstRoot * deltaTime);
            float secondDecay = Mathf.Exp(secondRoot * deltaTime);
            spring.Position = target + firstCoefficient * firstDecay + secondCoefficient * secondDecay;
            spring.Velocity = firstRoot * firstCoefficient * firstDecay + secondRoot * secondCoefficient * secondDecay;
            return spring;
        }

        private void ApplyAll()
        {
            SpringState combined = _hudSpring;
            combined.Position += _movementSpring.Position;

            if (_statusRoot != null)
            {
                SpringState status = combined;
                status.Position *= _config.StatusMotionMultiplier;
                ApplyRoot(_statusRoot, _statusBaseAnchoredPosition, _statusBaseLocalEulerAngles, status,
                    StatusMaxDisplacement, StatusMaxAngle);
            }

            ApplyRoot(_hudRoot, _hudBaseAnchoredPosition, _hudBaseLocalEulerAngles, combined,
                _config.MaxPixelDisplacement + _config.MovementMaxDisplacement, _config.MaxPixelAngle);
            ApplyRoot(_weaponRoot, _weaponBaseAnchoredPosition, _weaponBaseLocalEulerAngles, _weaponSpring,
                _config.WeaponMaxPixelDisplacement, _config.MaxPixelAngle);
            float zoom = 1f + _dashZoom;
            _hudRoot.localScale = _hudBaseLocalScale * zoom;

            if (_isWeaponChildOfHud == false)
            {
                _weaponRoot.localScale = _weaponBaseLocalScale * zoom;
            }
        }

        private void UpdateMovement(float deltaTime, float omega, float damping)
        {
            if (_motor == null || deltaTime <= 0f)
            {
                return;
            }

            Vector3 localVelocity = _motor.transform.InverseTransformDirection(_motor.Velocity);
            Vector3 acceleration = _motor.transform.InverseTransformDirection((_motor.Velocity - _previousVelocity) / deltaTime);
            _previousVelocity = _motor.Velocity;
            Vector3 target = new Vector3(-acceleration.x, -acceleration.z, 0f) * _config.InertiaPixelsPerAcceleration;
            target.y -= localVelocity.y * _config.JumpLagPixelsPerSpeed;
            float targetZoom = 0f;

            if (_motor.IsDashing)
            {
                Vector3 direction = _motor.transform.InverseTransformDirection(_motor.DashDirection);
                target += new Vector3(direction.x * _config.DashDisplacementPixels,
                    direction.z * _config.DashForwardDisplacementPixels, 0f);
                targetZoom = _config.DashScaleBoost;
            }

            target = Vector3.ClampMagnitude(target, _config.MovementMaxDisplacement);
            _movementSpring = Integrate(_movementSpring, target, omega, damping, deltaTime);
            _dashZoom = Mathf.Lerp(_dashZoom, targetZoom, 1f - Mathf.Exp(-_config.DashZoomResponse * deltaTime));
        }

        private static void ApplyRoot(RectTransform root, Vector2 baseAnchoredPosition,
            Vector3 baseLocalEulerAngles, SpringState spring, float maxPixelDisplacement, float maxPixelAngle)
        {
            float clampedX = Mathf.Clamp(spring.Position.x, -maxPixelDisplacement, maxPixelDisplacement);
            float clampedY = Mathf.Clamp(spring.Position.y, -maxPixelDisplacement, maxPixelDisplacement);
            float clampedRoll = Mathf.Clamp(spring.Position.z, -maxPixelAngle, maxPixelAngle);

            root.anchoredPosition = baseAnchoredPosition + new Vector2(clampedX, clampedY);
            root.localEulerAngles = baseLocalEulerAngles + new Vector3(0f, 0f, clampedRoll);
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: hud spring is not initialized, call Setup first.");
            }
        }
    }
}
