using System;
using MazeHell.Configuration;
using UnityEngine;
using UnityEngine.AI;

namespace MazeHell.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    [DisallowMultipleComponent]
    public class EnemyLocomotion : MonoBehaviour
    {
        private const float MovementSpeedEpsilon = 0.1f;

        private NavMeshAgent _agent;
        private EnemyConfig _config;
        private bool _isSetupFinished;

        private float _pathRefreshTimer;
        private Vector3 _desiredDestination;
        private float _desiredStoppingDistance;
        private bool _hasDestination;

        private Vector3 _knockbackVelocity;
        private float _knockbackDuration;
        private float _knockbackTimer;
        private bool _isMoving;

        public bool IsMoving => _isMoving;

        public void Setup(NavMeshAgent agent, EnemyConfig config, float moveSpeedScale)
        {
            if (agent == null)
            {
                throw new ArgumentNullException(nameof(agent), $"{name}: nav mesh agent is required.");
            }

            if (agent.gameObject != gameObject)
            {
                throw new ArgumentException($"{name}: nav mesh agent must sit on the locomotion GameObject.", nameof(agent));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), $"{name}: enemy config is required.");
            }

            if (float.IsFinite(moveSpeedScale) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(moveSpeedScale), $"{name}: move speed scale must be finite, got {moveSpeedScale}.");
            }

            if (moveSpeedScale < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(moveSpeedScale), $"{name}: move speed scale must be non-negative, got {moveSpeedScale}.");
            }

            _agent = agent;
            _config = config;
            _isSetupFinished = true;

            _agent.speed = config.MoveSpeed * moveSpeedScale;
            _agent.acceleration = config.Body.AgentAcceleration;
            _agent.angularSpeed = config.Body.AgentAngularSpeed;
            _agent.radius = config.Body.AgentRadius;
            _agent.height = config.Body.AgentHeight;
            _agent.updateRotation = true;

            _pathRefreshTimer = 0f;
            _desiredDestination = Vector3.zero;
            _desiredStoppingDistance = 0f;
            _hasDestination = false;
            _knockbackVelocity = Vector3.zero;
            _knockbackDuration = 0f;
            _knockbackTimer = 0f;
            _isMoving = false;
        }

        public void Tick(float deltaTime)
        {
            AssertSetup();

            _pathRefreshTimer -= deltaTime;

            if (_hasDestination && _pathRefreshTimer <= 0f)
            {
                ApplyDestination();
            }

            if (_knockbackTimer > 0f)
            {
                _knockbackTimer = Mathf.Max(0f, _knockbackTimer - deltaTime);
                float decayFraction = _knockbackTimer / _knockbackDuration;

                _agent.Move(_knockbackVelocity * (decayFraction * deltaTime));
            }

            _isMoving = _agent.velocity.sqrMagnitude > MovementSpeedEpsilon * MovementSpeedEpsilon;
        }

        public void SetDestination(Vector3 position, float stoppingDistance)
        {
            AssertSetup();

            if (float.IsFinite(stoppingDistance) == false || stoppingDistance < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(stoppingDistance), $"{name}: stopping distance must be finite and non-negative, got {stoppingDistance}.");
            }

            _desiredDestination = position;
            _desiredStoppingDistance = stoppingDistance;
            _hasDestination = true;

            if (_pathRefreshTimer <= 0f)
            {
                ApplyDestination();
            }
        }

        private void ApplyDestination()
        {
            _pathRefreshTimer = _config.PathRefreshInterval;
            _agent.stoppingDistance = _desiredStoppingDistance;
            _agent.updateRotation = true;
            _agent.isStopped = false;
            _agent.SetDestination(_desiredDestination);
        }

        public void Stop()
        {
            AssertSetup();

            _hasDestination = false;
            _agent.isStopped = true;
            _agent.updateRotation = false;
        }

        public void Move(Vector3 delta)
        {
            AssertSetup();

            _agent.Move(delta);
        }

        public void FaceDirection(Vector3 direction)
        {
            AssertSetup();

            Vector3 flatDirection = direction;
            flatDirection.y = 0f;

            if (flatDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
        }

        public void ApplyKnockback(Vector3 impulse)
        {
            AssertSetup();

            if (float.IsFinite(impulse.x) == false || float.IsFinite(impulse.y) == false || float.IsFinite(impulse.z) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(impulse), $"{name}: knockback impulse must be finite, got {impulse}.");
            }

            Vector3 flatImpulse = impulse;
            flatImpulse.y = 0f;

            _knockbackVelocity += flatImpulse;
            _knockbackDuration = _config.FlinchTime;
            _knockbackTimer = _knockbackDuration;
        }

        public void Disable()
        {
            AssertSetup();

            _agent.enabled = false;
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: enemy locomotion is not initialized, call Setup first.");
            }
        }
    }
}
