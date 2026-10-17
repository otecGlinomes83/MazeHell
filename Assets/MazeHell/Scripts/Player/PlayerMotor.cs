using System;
using MazeHell.Configuration;
using MazeHell.Input;
using UnityEngine;

namespace MazeHell.Player
{
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public class PlayerMotor : MonoBehaviour
    {
        public event Action<float> Jumped;

        public event Action<float> Landed;

        public event Action<float> Dashed;

        private const float MinWishDirectionSqrMagnitude = 0.0001f;

        private CharacterController _controller;
        private PlayerMovementConfig _config;
        private bool _isSetupFinished;

        private Vector3 _moveVelocity;
        private Vector3 _knockbackVelocity;
        private float _verticalSpeed;
        private bool _wasGrounded;

        private float _jumpBufferTimer;
        private float _coyoteTimer;
        private float _timeSinceLanded;

        private bool _isDashing;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private float _dashInvulnerabilityTimer;
        private float _dashSpeed;
        private Vector3 _dashDirection;

        private Vector3 _actualVelocity;
        private float _actualHorizontalSpeed;
        private bool _isGrounded;
        private bool _isInvulnerable;
        private float _dashCooldownRemaining;

        public Vector3 Velocity => _actualVelocity;

        public float HorizontalSpeed => _actualHorizontalSpeed;

        public bool IsGrounded => _isGrounded;

        public bool IsDashing => _isDashing;

        public bool IsInvulnerable => _isInvulnerable;

        public float DashCooldownRemaining => _dashCooldownRemaining;

        public Vector3 DashDirection => _dashDirection;
        public float BodyHeight => _controller.height;

        public void Setup(CharacterController controller, PlayerMovementConfig config)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller), $"{name}: character controller is required.");
            }

            if (controller.gameObject != gameObject)
            {
                throw new ArgumentException($"{name}: character controller must sit on the motor GameObject.", nameof(controller));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), $"{name}: movement config is required.");
            }

            _controller = controller;
            _config = config;
            _controller.height = config.BodyHeight;
            _controller.radius = config.BodyRadius;
            _controller.center = Vector3.up * (config.BodyHeight * 0.5f);
            _controller.stepOffset = config.StepOffset;
            _controller.skinWidth = config.SkinWidth;
            _controller.slopeLimit = config.SlopeLimit;
            _controller.minMoveDistance = config.MinimumMoveDistance;
            _isSetupFinished = true;

            ResetMotionState();
        }

        public void Tick(InputSnapshot input, float deltaTime)
        {
            AssertSetup();

            if (float.IsFinite(deltaTime) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), $"{name}: delta time must be finite, got {deltaTime}.");
            }

            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), $"{name}: delta time must be non-negative, got {deltaTime}.");
            }

            UpdateTimers(deltaTime);

            Vector3 wishDirection = ComputeWishDirection(input.Move);
            float wishMagnitude = wishDirection.magnitude;
            float wishSpeed = _config.BaseSpeed * wishMagnitude;

            if (wishMagnitude > 0f)
            {
                wishDirection /= wishMagnitude;
            }

            if (input.IsBlocking)
            {
                wishSpeed *= _config.BlockSpeedMultiplier;
            }

            if (input.JumpPressed)
            {
                _jumpBufferTimer = _config.JumpBufferTime;
            }

            bool isJumpFrame = TryConsumeJump();

            if (input.DashPressed && _isDashing == false && _dashCooldownTimer <= 0f)
            {
                StartDash(wishDirection);
            }

            if (_isDashing)
            {
                _moveVelocity.x = _dashDirection.x * _dashSpeed;
                _moveVelocity.z = _dashDirection.z * _dashSpeed;
            }
            else if (_wasGrounded)
            {
                bool skipFriction = isJumpFrame || _timeSinceLanded <= _config.BhopGraceTime;

                if (skipFriction == false)
                {
                    ApplyFriction(deltaTime);
                }

                ApplyGroundAcceleration(wishDirection, wishSpeed, deltaTime);
            }
            else
            {
                ApplyAirAcceleration(wishDirection, wishSpeed, deltaTime);
            }

            if (_isDashing == false)
            {
                ApplySoftSpeedCap(deltaTime);
            }

            if (isJumpFrame == false)
            {
                if (_wasGrounded)
                {
                    _verticalSpeed = -_config.Gravity * deltaTime;
                }
                else
                {
                    _verticalSpeed -= _config.Gravity * deltaTime;
                }
            }

            _knockbackVelocity = Vector3.MoveTowards(_knockbackVelocity, Vector3.zero, _config.KnockbackDecay * deltaTime);

            MoveAndResolveState(deltaTime);
        }

        public void AddImpulse(Vector3 impulse)
        {
            AssertSetup();

            if (IsFinite(impulse) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(impulse), $"{name}: impulse must be finite, got {impulse}.");
            }

            _knockbackVelocity += impulse;
        }

        public void AddKnockback(Vector3 direction, float distance)
        {
            AssertSetup();

            if (float.IsFinite(distance) == false || distance < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(distance));
            }

            direction.y = 0f;

            if (direction.sqrMagnitude <= MinWishDirectionSqrMagnitude || distance == 0f)
            {
                return;
            }

            float speed = Mathf.Sqrt(2f * _config.KnockbackDecay * distance);
            AddImpulse(direction.normalized * speed);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            AssertSetup();

            if (IsFinite(position) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(position), $"{name}: teleport position must be finite, got {position}.");
            }

            _controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            _controller.enabled = true;

            ResetMotionState();
        }

        private void UpdateTimers(float deltaTime)
        {
            _jumpBufferTimer = Mathf.Max(0f, _jumpBufferTimer - deltaTime);
            _coyoteTimer = Mathf.Max(0f, _coyoteTimer - deltaTime);
            _dashTimer = Mathf.Max(0f, _dashTimer - deltaTime);
            _dashCooldownTimer = Mathf.Max(0f, _dashCooldownTimer - deltaTime);
            _dashInvulnerabilityTimer = Mathf.Max(0f, _dashInvulnerabilityTimer - deltaTime);
            _timeSinceLanded += deltaTime;
            _isDashing = _dashTimer > 0f;

            if (_wasGrounded && _verticalSpeed <= 0f)
            {
                _coyoteTimer = _config.CoyoteTime;
            }
        }

        private bool TryConsumeJump()
        {
            if (_jumpBufferTimer <= 0f || _coyoteTimer <= 0f)
            {
                return false;
            }

            if (_wasGrounded && _timeSinceLanded <= _config.BhopGraceTime)
            {
                float speed = HorizontalSpeedOf(_moveVelocity);

                if (speed > 0f)
                {
                    float boostedScale = (speed + _config.BhopSpeedGain) / speed;
                    _moveVelocity.x *= boostedScale;
                    _moveVelocity.z *= boostedScale;
                }
            }

            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
            _verticalSpeed = Mathf.Sqrt(2f * _config.Gravity * _config.JumpHeight);
            _wasGrounded = false;
            _isGrounded = false;

            Jumped?.Invoke(_verticalSpeed);

            return true;
        }

        private void StartDash(Vector3 wishDirection)
        {
            _dashDirection = ResolveDashDirection(wishDirection);
            _dashSpeed = Mathf.Max(HorizontalSpeedOf(_moveVelocity), _config.BaseSpeed * _config.DashSpeedMultiplier);
            _dashTimer = _config.DashDuration;
            _dashCooldownTimer = _config.DashCooldown;
            _dashInvulnerabilityTimer = _config.DashInvulnerabilityTime;
            _isDashing = true;

            Dashed?.Invoke(_dashSpeed);
        }

        private Vector3 ResolveDashDirection(Vector3 wishDirection)
        {
            if (wishDirection.sqrMagnitude > MinWishDirectionSqrMagnitude)
            {
                return wishDirection;
            }

            Vector3 fallback = transform.forward;
            fallback.y = 0f;

            return fallback.normalized;
        }

        private Vector3 ComputeWishDirection(Vector2 moveInput)
        {
            Vector2 clampedInput = moveInput;

            if (_config.NormalizeDiagonalMovement && clampedInput.sqrMagnitude > 1f)
            {
                clampedInput.Normalize();
            }

            Vector3 wishDirection = transform.right * clampedInput.x + transform.forward * clampedInput.y;
            wishDirection.y = 0f;

            if (wishDirection.sqrMagnitude < MinWishDirectionSqrMagnitude)
            {
                return Vector3.zero;
            }

            return wishDirection;
        }

        private void ApplyFriction(float deltaTime)
        {
            float speed = HorizontalSpeedOf(_moveVelocity);

            if (speed <= 0f)
            {
                return;
            }

            float drop = speed * _config.GroundFriction * deltaTime;
            float newSpeed = Mathf.Max(0f, speed - drop);
            float scale = newSpeed / speed;

            _moveVelocity.x *= scale;
            _moveVelocity.z *= scale;
        }

        private void ApplyGroundAcceleration(Vector3 wishDirection, float wishSpeed, float deltaTime)
        {
            if (wishDirection.sqrMagnitude == 0f)
            {
                return;
            }

            float currentSpeedAlongWish = _moveVelocity.x * wishDirection.x + _moveVelocity.z * wishDirection.z;
            float addSpeed = wishSpeed - currentSpeedAlongWish;

            if (addSpeed <= 0f)
            {
                return;
            }

            float inputScale = wishSpeed / Mathf.Max(0.01f, _config.BaseSpeed);
            _moveVelocity += wishDirection * Mathf.Min(_config.GroundAcceleration * inputScale * deltaTime, addSpeed);
        }

        private void ApplyAirAcceleration(Vector3 wishDirection, float wishSpeed, float deltaTime)
        {
            if (wishDirection.sqrMagnitude == 0f)
            {
                return;
            }

            float airWishSpeed = Mathf.Min(wishSpeed, _config.AirWishSpeed);
            float currentSpeedAlongWish = _moveVelocity.x * wishDirection.x + _moveVelocity.z * wishDirection.z;
            float addSpeed = airWishSpeed - currentSpeedAlongWish;

            if (addSpeed <= 0f)
            {
                return;
            }

            _moveVelocity += wishDirection * Mathf.Min(_config.AirAcceleration * wishSpeed * deltaTime, addSpeed);
        }

        private void ApplySoftSpeedCap(float deltaTime)
        {
            float speed = HorizontalSpeedOf(_moveVelocity);

            if (speed <= _config.SoftSpeedCap)
            {
                return;
            }

            float cappedSpeed = Mathf.MoveTowards(speed, _config.SoftSpeedCap, _config.CapDecay * deltaTime);
            float scale = cappedSpeed / speed;

            _moveVelocity.x *= scale;
            _moveVelocity.z *= scale;
        }

        private void MoveAndResolveState(float deltaTime)
        {
            Vector3 displacement = _moveVelocity + _knockbackVelocity;
            displacement.y += _verticalSpeed;

            float fallSpeed = Mathf.Max(0f, -_verticalSpeed);

            CollisionFlags collisionFlags = _controller.Move(displacement * deltaTime);
            bool hitGround = _verticalSpeed <= 0f && ProbeGround();

            if ((collisionFlags & CollisionFlags.CollidedAbove) != 0 && _verticalSpeed > 0f)
            {
                _verticalSpeed = 0f;
            }

            if (hitGround)
            {
                if (_verticalSpeed < 0f)
                {
                    _verticalSpeed = 0f;
                }

                if (_wasGrounded == false)
                {
                    _timeSinceLanded = 0f;

                    Landed?.Invoke(fallSpeed);
                }
            }

            _isGrounded = hitGround;
            _wasGrounded = hitGround;

            _actualVelocity = _controller.velocity;
            _actualHorizontalSpeed = HorizontalSpeedOf(_actualVelocity);
            _isDashing = _dashTimer > 0f;
            _isInvulnerable = _dashInvulnerabilityTimer > 0f;
            _dashCooldownRemaining = _dashCooldownTimer;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (Vector3.Angle(hit.normal, Vector3.up) <= _config.SlopeLimit)
            {
                return;
            }

            Vector3 normal = hit.normal;
            normal.y = 0f;

            if (normal.sqrMagnitude <= MinWishDirectionSqrMagnitude)
            {
                return;
            }

            normal.Normalize();
            float movementIntoWall = Vector3.Dot(_moveVelocity, normal);
            float knockbackIntoWall = Vector3.Dot(_knockbackVelocity, normal);

            if (movementIntoWall < 0f)
            {
                _moveVelocity -= normal * movementIntoWall;
            }

            if (knockbackIntoWall < 0f)
            {
                _knockbackVelocity -= normal * knockbackIntoWall;
            }
        }

        private bool ProbeGround()
        {
            Vector3 bottom = transform.position + _controller.center + Vector3.down * (_controller.height * 0.5f);
            Vector3 origin = bottom + Vector3.up * _config.GroundedProbeSkin;
            float distance = _config.GroundedProbeSkin + _config.GroundedProbeDistance;

            return Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore) && Vector3.Angle(hit.normal, Vector3.up) <= _config.SlopeLimit;
        }

        private float HorizontalSpeedOf(Vector3 velocity)
        {
            return Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
        }

        private bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
        }

        private void ResetMotionState()
        {
            _moveVelocity = Vector3.zero;
            _knockbackVelocity = Vector3.zero;
            _verticalSpeed = 0f;
            _wasGrounded = false;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
            _timeSinceLanded = 0f;
            _isDashing = false;
            _dashTimer = 0f;
            _dashCooldownTimer = 0f;
            _dashInvulnerabilityTimer = 0f;
            _dashSpeed = 0f;
            _dashDirection = Vector3.zero;
            _actualVelocity = Vector3.zero;
            _actualHorizontalSpeed = 0f;
            _isGrounded = false;
            _isInvulnerable = false;
            _dashCooldownRemaining = 0f;
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: player motor is not initialized, call Setup first.");
            }
        }
    }
}
