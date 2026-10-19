using System;
using MazeHell.Combat;
using MazeHell.Configuration;
using UnityEngine;

namespace MazeHell.Enemies
{
    [DisallowMultipleComponent]
    public class EnemyBrain : MonoBehaviour
    {
        private const float MinDirectionSqrMagnitude = 0.0001f;
        private const float ArrivalStoppingScale = 0.5f;
        private const float ChestHeightFraction = 0.5f;

        private EnemyConfig _config;
        private EnemyPerception _perception;
        private EnemyLocomotion _locomotion;
        private EnemySpriteAnimator _animator;
        private Health _health;
        private DamageTarget _damageTarget;
        private PlayerDefense _playerDefense;
        private Transform _targetTransform;
        private Collider _hitCollider;
        private bool _isSetupFinished;
        private bool _isSubscribed;

        private EnemyState _state;
        private Vector3 _homePosition;
        private float _homeYaw;
        private Vector3 _investigationPoint;
        private Vector3 _lastSeenPosition;
        private float _loseSightTimer;
        private float _stateTimer;
        private float _attackCooldownTimer;
        private float _windupElapsed;
        private float _lungeElapsed;
        private float _lungeDistanceTraveled;
        private Vector3 _lungeDirection;
        private bool _hasHitThisLunge;

        public EnemyState State => _state;

        public void Setup(EnemyConfig config, EnemyPerception perception, EnemyLocomotion locomotion, EnemySpriteAnimator animator,
            Health health, DamageTarget damageTarget, PlayerDefense playerDefense, Transform targetTransform, Collider hitCollider)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), $"{name}: enemy config is required.");
            }

            if (config.Attack.LungeCollisionMask.value == 0)
            {
                throw new ArgumentException($"{name}: lunge collision mask must not be empty.", nameof(config));
            }

            if (perception == null)
            {
                throw new ArgumentNullException(nameof(perception), $"{name}: perception is required.");
            }

            if (perception.gameObject != gameObject)
            {
                throw new ArgumentException($"{name}: perception must sit on the brain GameObject.", nameof(perception));
            }

            if (locomotion == null)
            {
                throw new ArgumentNullException(nameof(locomotion), $"{name}: locomotion is required.");
            }

            if (locomotion.gameObject != gameObject)
            {
                throw new ArgumentException($"{name}: locomotion must sit on the brain GameObject.", nameof(locomotion));
            }

            if (animator == null)
            {
                throw new ArgumentNullException(nameof(animator), $"{name}: sprite animator is required.");
            }

            if (health == null)
            {
                throw new ArgumentNullException(nameof(health), $"{name}: health is required.");
            }

            if (health.gameObject != gameObject)
            {
                throw new ArgumentException($"{name}: health must sit on the brain GameObject.", nameof(health));
            }

            if (damageTarget == null)
            {
                throw new ArgumentNullException(nameof(damageTarget), $"{name}: damage target is required.");
            }

            if (damageTarget.gameObject != gameObject)
            {
                throw new ArgumentException($"{name}: damage target must sit on the brain GameObject.", nameof(damageTarget));
            }

            if (playerDefense == null)
            {
                throw new ArgumentNullException(nameof(playerDefense), $"{name}: player defense is required.");
            }

            if (targetTransform == null)
            {
                throw new ArgumentNullException(nameof(targetTransform), $"{name}: target transform is required.");
            }

            if (hitCollider == null)
            {
                throw new ArgumentNullException(nameof(hitCollider), $"{name}: hit collider is required.");
            }

            if (hitCollider.transform.IsChildOf(transform) == false)
            {
                throw new ArgumentException($"{name}: hit collider must belong to the enemy hierarchy.", nameof(hitCollider));
            }

            UnsubscribeFromDependencies();

            _config = config;
            _perception = perception;
            _locomotion = locomotion;
            _animator = animator;
            _health = health;
            _damageTarget = damageTarget;
            _playerDefense = playerDefense;
            _targetTransform = targetTransform;
            _hitCollider = hitCollider;
            _isSetupFinished = true;

            _state = EnemyState.Idle;
            _homePosition = transform.position;
            _homeYaw = transform.eulerAngles.y;
            _investigationPoint = _homePosition;
            _lastSeenPosition = _homePosition;
            _loseSightTimer = 0f;
            _stateTimer = 0f;
            _attackCooldownTimer = 0f;
            _windupElapsed = 0f;
            _lungeElapsed = 0f;
            _lungeDistanceTraveled = 0f;
            _lungeDirection = transform.forward;
            _hasHitThisLunge = false;

            SubscribeToDependencies();
        }

        public void Tick(float deltaTime)
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

            if (_state == EnemyState.Dead)
            {
                return;
            }

            _attackCooldownTimer = Mathf.Max(0f, _attackCooldownTimer - deltaTime);
            _perception.Tick(deltaTime);
            _locomotion.Tick(deltaTime);
            UpdateVisionKnowledge(deltaTime);

            switch (_state)
            {
                case EnemyState.Idle:
                    UpdateIdle();
                    break;
                case EnemyState.Investigating:
                    UpdateInvestigating();
                    break;
                case EnemyState.Searching:
                    UpdateSearching(deltaTime);
                    break;
                case EnemyState.Chasing:
                    UpdateChasing();
                    break;
                case EnemyState.Windup:
                    UpdateWindup(deltaTime);
                    break;
                case EnemyState.Lunging:
                    UpdateLunging(deltaTime);
                    break;
                case EnemyState.Recovering:
                    UpdateRecovering(deltaTime);
                    break;
                case EnemyState.Stunned:
                    UpdateStunned(deltaTime);
                    break;
                case EnemyState.Returning:
                    UpdateReturning();
                    break;
            }
        }

        private void UpdateVisionKnowledge(float deltaTime)
        {
            if (_perception.IsTargetVisible)
            {
                _lastSeenPosition = _targetTransform.position;
                _loseSightTimer = _config.Perception.LoseSightGraceTime;

                if (_state == EnemyState.Idle || _state == EnemyState.Investigating || _state == EnemyState.Searching || _state == EnemyState.Returning)
                {
                    Transition(EnemyState.Chasing);
                }

                return;
            }

            _loseSightTimer = Mathf.Max(0f, _loseSightTimer - deltaTime);

            if (_loseSightTimer > 0f && _perception.HasTargetLineOfSight)
            {
                _lastSeenPosition = _targetTransform.position;
            }
        }

        private void UpdateIdle()
        {
            HandleNoise();

            if (_state != EnemyState.Idle)
            {
                return;
            }

            _animator.PlayIdle();
        }

        private void UpdateInvestigating()
        {
            HandleNoise();

            if (_state != EnemyState.Investigating)
            {
                return;
            }

            float toleranceSqr = _config.Perception.ReturnToleranceDistance * _config.Perception.ReturnToleranceDistance;

            if (FlatSqrDistanceTo(_investigationPoint) <= toleranceSqr)
            {
                _locomotion.Stop();
                _stateTimer = _config.Perception.SearchWaitDuration;

                Transition(EnemyState.Searching);

                return;
            }

            PlayMovementAnimation();
            _locomotion.SetDestination(_investigationPoint, _config.Perception.ReturnToleranceDistance * ArrivalStoppingScale);
        }

        private void UpdateSearching(float deltaTime)
        {
            HandleNoise();

            if (_state != EnemyState.Searching)
            {
                return;
            }

            _animator.PlayIdle();
            _stateTimer -= deltaTime;

            if (_stateTimer <= 0f)
            {
                Transition(EnemyState.Returning);
            }
        }

        private void UpdateChasing()
        {
            bool isTargetTracked = _perception.IsTargetVisible ||
                (_loseSightTimer > 0f && _perception.HasTargetLineOfSight);
            float arrivalStoppingDistance = _config.Perception.ReturnToleranceDistance * ArrivalStoppingScale;

            if (isTargetTracked)
            {
                _locomotion.SetDestination(_targetTransform.position, _config.Body.AgentStoppingDistance);
            }
            else
            {
                _locomotion.SetDestination(_lastSeenPosition, arrivalStoppingDistance);
            }

            PlayMovementAnimation();

            float attackRangeSqr = _config.Attack.AttackRange * _config.Attack.AttackRange;

            if (isTargetTracked && _attackCooldownTimer <= 0f && FlatSqrDistanceTo(_targetTransform.position) <= attackRangeSqr)
            {
                _locomotion.Stop();
                _windupElapsed = 0f;

                Transition(EnemyState.Windup);

                return;
            }

            if (_loseSightTimer > 0f)
            {
                return;
            }

            float toleranceSqr = _config.Perception.ReturnToleranceDistance * _config.Perception.ReturnToleranceDistance;
            bool reachedLastSeen = FlatSqrDistanceTo(_lastSeenPosition) <= toleranceSqr;

            if (reachedLastSeen)
            {
                _locomotion.Stop();
                _stateTimer = _config.Perception.SearchWaitDuration;

                Transition(EnemyState.Searching);

                return;
            }

            if (_loseSightTimer <= 0f)
            {
                _investigationPoint = _lastSeenPosition;

                Transition(EnemyState.Investigating);
            }
        }

        private void UpdateWindup(float deltaTime)
        {
            _animator.PlayWindup();

            Vector3 toTarget = _targetTransform.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > MinDirectionSqrMagnitude)
            {
                _locomotion.FaceDirection(toTarget);
            }

            _windupElapsed += deltaTime;

            if (_windupElapsed >= _config.Attack.WindupDuration)
            {
                StartLunge(toTarget);
            }
        }

        private void StartLunge(Vector3 toTarget)
        {
            if (toTarget.sqrMagnitude > MinDirectionSqrMagnitude)
            {
                _lungeDirection = toTarget.normalized;
            }
            else
            {
                Vector3 fallback = transform.forward;
                fallback.y = 0f;
                _lungeDirection = fallback.normalized;
            }

            _locomotion.FaceDirection(_lungeDirection);
            _lungeElapsed = 0f;
            _lungeDistanceTraveled = 0f;
            _hasHitThisLunge = false;
            _attackCooldownTimer = _config.Attack.AttackCooldown;

            Transition(EnemyState.Lunging);
        }

        private void UpdateLunging(float deltaTime)
        {
            _animator.PlayLunge();

            float remainingTime = _config.Attack.LungeDuration - _lungeElapsed;
            float stepTime = Mathf.Min(deltaTime, remainingTime);
            float lungeSpeed = _config.Attack.LungeDistance / _config.Attack.LungeDuration;
            float stepDistance = Mathf.Min(lungeSpeed * stepTime, _config.Attack.LungeDistance - _lungeDistanceTraveled);
            Vector3 displacement = ClipLungeDelta(_lungeDirection * stepDistance);

            _locomotion.Move(displacement);
            _lungeDistanceTraveled += displacement.magnitude;
            _lungeElapsed = Mathf.Min(_lungeElapsed + deltaTime, _config.Attack.LungeDuration);

            float hitDistanceSqr = _config.Attack.HitDistance * _config.Attack.HitDistance;

            if (_hasHitThisLunge == false && FlatSqrDistanceTo(_targetTransform.position) <= hitDistanceSqr && HasLineOfSightToTarget())
            {
                DeliverHit();
            }

            if (_state != EnemyState.Lunging)
            {
                return;
            }

            if (_lungeElapsed >= _config.Attack.LungeDuration)
            {
                _locomotion.Stop();
                _stateTimer = _config.Attack.AttackCooldown;

                Transition(EnemyState.Recovering);
            }
        }

        private Vector3 ClipLungeDelta(Vector3 displacement)
        {
            float distance = displacement.magnitude;

            if (distance <= 0f)
            {
                return Vector3.zero;
            }

            Vector3 direction = displacement / distance;
            EnemyBodyData body = _config.Body;
            float skin = _config.Attack.CollisionSkin;
            Vector3 bottom = transform.position + Vector3.up * (body.BodyRadius + skin);
            Vector3 top = transform.position + Vector3.up * (body.BodyHeight - body.BodyRadius + skin);

            if (Physics.CapsuleCast(bottom, top, body.BodyRadius, direction, out RaycastHit hit, distance, _config.Attack.LungeCollisionMask, QueryTriggerInteraction.Ignore) == false)
            {
                return displacement;
            }

            float clippedDistance = Mathf.Max(0f, hit.distance - skin);

            return direction * clippedDistance;
        }

        private bool HasLineOfSightToTarget()
        {
            float chestHeight = _config.Body.BodyHeight * ChestHeightFraction;
            Vector3 eyePosition = transform.position + Vector3.up * chestHeight;
            Vector3 targetChest = _targetTransform.position + Vector3.up * chestHeight;
            Vector3 toChest = targetChest - eyePosition;
            float distance = toChest.magnitude;

            if (distance <= 0f)
            {
                return true;
            }

            if (Physics.Raycast(eyePosition, toChest, out RaycastHit hit, distance, _config.Perception.PerceptionMask, QueryTriggerInteraction.Ignore) == false)
            {
                return true;
            }

            return hit.transform.IsChildOf(_targetTransform);
        }

        private void DeliverHit()
        {
            _hasHitThisLunge = true;

            Vector3 toTarget = _targetTransform.position - transform.position;
            toTarget.y = 0f;

            Vector3 awayDirection;

            if (toTarget.sqrMagnitude > MinDirectionSqrMagnitude)
            {
                awayDirection = toTarget.normalized;
            }
            else
            {
                awayDirection = _lungeDirection;
            }

            float damage = UnityEngine.Random.Range(_config.Attack.DamageMin, _config.Attack.DamageMax);
            float stunDuration = _playerDefense.ReceiveMelee(damage, awayDirection);

            if (stunDuration > 0f)
            {
                _locomotion.Stop();
                _stateTimer = stunDuration;

                Transition(EnemyState.Stunned);
            }
        }

        private void UpdateRecovering(float deltaTime)
        {
            HandleNoise();

            if (_state != EnemyState.Recovering)
            {
                return;
            }

            _animator.PlayIdle();
            _stateTimer -= deltaTime;

            if (_stateTimer <= 0f)
            {
                Transition(EnemyState.Chasing);
            }
        }

        private void UpdateStunned(float deltaTime)
        {
            _animator.PlayPain();
            _stateTimer -= deltaTime;

            if (_stateTimer <= 0f)
            {
                Transition(EnemyState.Chasing);
            }
        }

        private void UpdateReturning()
        {
            HandleNoise();

            if (_state != EnemyState.Returning)
            {
                return;
            }

            float toleranceSqr = _config.Perception.ReturnToleranceDistance * _config.Perception.ReturnToleranceDistance;

            if (FlatSqrDistanceTo(_homePosition) <= toleranceSqr)
            {
                _locomotion.Stop();
                _locomotion.FaceDirection(Quaternion.Euler(0f, _homeYaw, 0f) * Vector3.forward);

                Transition(EnemyState.Idle);

                return;
            }

            PlayMovementAnimation();
            _locomotion.SetDestination(_homePosition, _config.Perception.ReturnToleranceDistance * ArrivalStoppingScale);
        }

        private void PlayMovementAnimation()
        {
            if (_locomotion.IsMoving)
            {
                _animator.PlayWalk();

                return;
            }

            _animator.PlayIdle();
        }

        private void HandleNoise()
        {
            if (_state == EnemyState.Chasing || _state == EnemyState.Windup || _state == EnemyState.Lunging || _state == EnemyState.Stunned || _state == EnemyState.Dead)
            {
                return;
            }

            if (_perception.TryConsumeNoise(out Vector3 position) == false)
            {
                return;
            }

            _investigationPoint = position;

            if (_state != EnemyState.Investigating)
            {
                Transition(EnemyState.Investigating);
            }
        }

        private void OnImpacted(Vector3 impulse)
        {
            if (_state == EnemyState.Dead)
            {
                return;
            }

            float protectedWindupStart = _config.Attack.WindupDuration - _config.Attack.WindupProtectedFinalTime;

            if (_state == EnemyState.Windup && _windupElapsed >= protectedWindupStart)
            {
                return;
            }

            _locomotion.ApplyKnockback(impulse);
            _animator.PlayPain();

            if (_state == EnemyState.Windup)
            {
                Transition(EnemyState.Chasing);
            }
        }

        private void OnDied(Health health)
        {
            Transition(EnemyState.Dead);
            _locomotion.Stop();
            _locomotion.Disable();
            _hitCollider.enabled = false;

            _animator.PlayCorpse();
        }

        private void OnEnable()
        {
            if (_isSetupFinished)
            {
                SubscribeToDependencies();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromDependencies();
        }

        private void SubscribeToDependencies()
        {
            if (_isSubscribed || _health == null || _damageTarget == null)
            {
                return;
            }

            _health.Died += OnDied;
            _damageTarget.Impacted += OnImpacted;
            _isSubscribed = true;
        }

        private void UnsubscribeFromDependencies()
        {
            if (_isSubscribed == false)
            {
                return;
            }

            _health.Died -= OnDied;
            _damageTarget.Impacted -= OnImpacted;
            _isSubscribed = false;
        }

        private float FlatSqrDistanceTo(Vector3 position)
        {
            Vector3 offset = position - transform.position;
            offset.y = 0f;

            return offset.sqrMagnitude;
        }

        private void Transition(EnemyState state)
        {
            _state = state;
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: enemy brain is not initialized, call Setup first.");
            }
        }
    }
}
