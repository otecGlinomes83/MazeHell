using UnityEngine;

namespace MazeHell.Configuration
{
    [CreateAssetMenu(fileName = "Doors", menuName = "MazeHell/Configurations/Doors")]
    public class DoorConfig : ScriptableObject
    {
        [Min(1f)] [SerializeField] private float _openDistance = 8f;
        [Min(1f)] [SerializeField] private float _closeDistance = 12f;
        [Min(0.01f)] [SerializeField] private float _openDuration = 0.65f;
        [Min(0.01f)] [SerializeField] private float _closeDuration = 1.2f;
        [Min(0f)] [SerializeField] private float _holdOpenDuration = 0.6f;
        [Min(0f)] [SerializeField] private float _anticipationTime = 0.18f;
        [Min(0f)] [SerializeField] private float _collisionMargin = 0.5f;
        [Min(0.1f)] [SerializeField] private float _liftHeight = 5f;

        public float OpenDistance => _openDistance;
        public float CloseDistance => _closeDistance;
        public float OpenDuration => _openDuration;
        public float CloseDuration => _closeDuration;
        public float HoldOpenDuration => _holdOpenDuration;
        public float AnticipationTime => _anticipationTime;
        public float CollisionMargin => _collisionMargin;
        public float LiftHeight => _liftHeight;
    }
}
