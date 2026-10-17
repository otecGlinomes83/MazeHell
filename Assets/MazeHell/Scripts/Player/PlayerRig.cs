using MazeHell.Combat;
using MazeHell.Presentation;
using UnityEngine;

namespace MazeHell.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerMotor), typeof(PlayerLook))]
    [RequireComponent(typeof(Health), typeof(Shotgun), typeof(PlayerDefense))]
    [RequireComponent(typeof(CameraMotion))]
    [DisallowMultipleComponent]
    public class PlayerRig : MonoBehaviour
    {
        [SerializeField] private CharacterController _controller;
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private PlayerLook _look;
        [SerializeField] private Health _health;
        [SerializeField] private Shotgun _shotgun;
        [SerializeField] private PlayerDefense _defense;
        [SerializeField] private CameraMotion _cameraMotion;
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _lookPivot;
        [SerializeField] private Transform _motionPivot;
        [SerializeField] private Light _muzzleLight;

        private MuzzleFlash _muzzleFlash;
        private PlayerFeedback _feedback;

        public MuzzleFlash MuzzleFlash => _muzzleFlash;
        public PlayerFeedback Feedback => _feedback;

        public void SetupFeedback(MuzzleFlash muzzleFlash, PlayerFeedback feedback)
        {
            _muzzleFlash = muzzleFlash;
            _feedback = feedback;
        }

        public CharacterController Controller => _controller;
        public PlayerMotor Motor => _motor;
        public PlayerLook Look => _look;
        public Health Health => _health;
        public Shotgun Shotgun => _shotgun;
        public PlayerDefense Defense => _defense;
        public CameraMotion CameraMotion => _cameraMotion;
        public Camera Camera => _camera;
        public Transform LookPivot => _lookPivot;
        public Transform MotionPivot => _motionPivot;
        public Light MuzzleLight => _muzzleLight;
    }
}
