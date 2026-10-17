using System;
using UnityEngine;

namespace MazeHell.Level
{
    [DisallowMultipleComponent]
    public class LevelModule : MonoBehaviour
    {
        [SerializeField] private Transform _northPort;
        [SerializeField] private Transform _eastPort;
        [SerializeField] private Transform _southPort;
        [SerializeField] private Transform _westPort;
        [SerializeField] private Transform _transitionAnchor;
        [SerializeField] private Transform[] _decorationPoints = new Transform[0];

        public Transform TransitionAnchor => _transitionAnchor;
        public Transform[] DecorationPoints => _decorationPoints;

        public bool IsNearDoorway(Vector3 position, float distance)
        {
            return (_northPort != null && Vector3.Distance(position, _northPort.position) < distance) ||
                (_eastPort != null && Vector3.Distance(position, _eastPort.position) < distance) ||
                (_southPort != null && Vector3.Distance(position, _southPort.position) < distance) ||
                (_westPort != null && Vector3.Distance(position, _westPort.position) < distance);
        }

        public Transform GetPort(MazeDirection direction)
        {
            Transform port;

            switch (direction)
            {
                case MazeDirection.North:
                    port = _northPort;
                    break;
                case MazeDirection.East:
                    port = _eastPort;
                    break;
                case MazeDirection.South:
                    port = _southPort;
                    break;
                case MazeDirection.West:
                    port = _westPort;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }

            if (port == null)
            {
                throw new InvalidOperationException($"{name}: no {direction} connection port assigned.");
            }

            if (port.parent != transform)
            {
                throw new InvalidOperationException($"{name}: connection ports must be direct children of the module root.");
            }

            return port;
        }
    }
}
