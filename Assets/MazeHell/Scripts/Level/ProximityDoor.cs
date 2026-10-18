using System;
using MazeHell.Configuration;
using UnityEngine;

namespace MazeHell.Level
{
    [DisallowMultipleComponent]
    public class ProximityDoor : MonoBehaviour
    {
        [SerializeField] private Transform _leaf;
        private DoorConfig _config;
        private Vector3 _closedPosition;
        private float _lift;
        private float _holdTimer;
        private bool _isOpening;

        public event Action<ProximityDoor> Opening;

        public void Setup(DoorConfig config)
        {
            if (config == null || _leaf == null)
            {
                throw new InvalidOperationException($"{name}: door config and leaf are required.");
            }

            _config = config;
            _closedPosition = _leaf.position;
        }

        public void Tick(Vector3 playerPosition, Vector3 playerVelocity, float bodyHeight, float deltaTime)
        {
            Vector3 offset = _closedPosition - playerPosition;
            offset.y = 0f;
            float distance = offset.magnitude;
            float approachSpeed = 0f;

            if (distance > 0.001f)
            {
                approachSpeed = Mathf.Max(0f, Vector3.Dot(playerVelocity, offset / distance));
            }

            float openDistance = Mathf.Max(_config.OpenDistance, approachSpeed * _config.AnticipationTime);

            if (distance <= openDistance)
            {
                _holdTimer = _config.HoldOpenDuration;
            }
            else
            {
                _holdTimer = Mathf.Max(0f, _holdTimer - deltaTime);
            }

            bool shouldOpen = distance <= openDistance || _holdTimer > 0f || distance <= _config.CloseDistance && _lift > 0f;

            if (shouldOpen && _isOpening == false)
            {
                Opening?.Invoke(this);
            }

            _isOpening = shouldOpen;
            float target = 0f;
            float speed = _config.LiftHeight / _config.CloseDuration;

            if (shouldOpen)
            {
                target = _config.LiftHeight;
                speed = target / _config.OpenDuration;

                if (approachSpeed > 0f)
                {
                    speed = Mathf.Max(speed, target * approachSpeed / Mathf.Max(distance - _config.CollisionMargin, 0.01f));
                }

                if (distance <= approachSpeed * deltaTime + _config.CollisionMargin)
                {
                    _lift = Mathf.Max(_lift, Mathf.Min(target, bodyHeight + _config.CollisionMargin));
                }
            }

            _lift = Mathf.MoveTowards(_lift, target, speed * deltaTime);
            _leaf.position = _closedPosition + Vector3.up * _lift;
        }
    }
}
