using System.Collections;
using MazeHell.Configuration;
using MazeHell.Input;
using MazeHell.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MazeHell.Tests
{
    public class PlayerMotorTests
    {
        private GameObject _floor;
        private GameObject _player;
        private GameObject _wall;
        private PlayerMovementConfig _config;
        private PlayerMotor _motor;
        private int _dashCount;
        private int _jumpCount;
        private float _previousCaptureDelta;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _dashCount = 0;
            _jumpCount = 0;
            _previousCaptureDelta = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 120f;
            _floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _floor.transform.position = new Vector3(0f, -0.5f, 0f);
            _floor.transform.localScale = new Vector3(80f, 1f, 80f);
            _player = new GameObject("PlayerMotorTests");
            _player.transform.position = new Vector3(0f, 0.05f, 0f);
            CharacterController controller = _player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.radius = 0.35f;
            controller.skinWidth = 0.04f;
            _config = ScriptableObject.CreateInstance<PlayerMovementConfig>();
            _motor = _player.AddComponent<PlayerMotor>();
            _motor.Setup(controller, _config);
            _motor.Dashed += OnDashed;
            _motor.Jumped += OnJumped;
            Physics.SyncTransforms();

            for (int i = 0; i < 8; i++)
            {
                _motor.Tick(new InputSnapshot(), Time.deltaTime);
                yield return null;
            }
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _motor.Dashed -= OnDashed;
            _motor.Jumped -= OnJumped;
            Time.captureDeltaTime = _previousCaptureDelta;
            Object.Destroy(_player);
            Object.Destroy(_floor);
            Object.Destroy(_config);

            if (_wall != null)
            {
                Object.Destroy(_wall);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator JumpRisesClearOfGroundProbe()
        {
            Assert.IsTrue(_motor.IsGrounded);
            float startingHeight = _player.transform.position.y;
            _motor.Tick(new InputSnapshot(Vector2.zero, Vector2.zero, true, false, false, false, false), Time.deltaTime);
            float elapsed = 0f;
            float maximumHeight = startingHeight;

            while (elapsed < 0.3f)
            {
                yield return null;
                _motor.Tick(new InputSnapshot(), Time.deltaTime);
                maximumHeight = Mathf.Max(maximumHeight, _player.transform.position.y);
                elapsed += Time.deltaTime;
            }

            Assert.Greater(maximumHeight - startingHeight, 0.4f);
        }

        [UnityTest]
        public IEnumerator DashCannotCrossWallOrRestartDuringCooldown()
        {
            _wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _wall.transform.position = new Vector3(0f, 1.5f, 3f);
            _wall.transform.localScale = new Vector3(8f, 3f, 0.5f);
            Physics.SyncTransforms();
            InputSnapshot dash = new InputSnapshot(Vector2.up, Vector2.zero, false, true, false, false, false);
            _motor.Tick(dash, Time.deltaTime);
            float elapsed = 0f;

            while (elapsed < 0.35f)
            {
                yield return null;
                _motor.Tick(new InputSnapshot(), Time.deltaTime);
                elapsed += Time.deltaTime;
            }

            _motor.Tick(dash, Time.deltaTime);
            Assert.AreEqual(1, _dashCount);
            Assert.Less(_player.transform.position.z, 2.5f);
            Assert.Greater(_motor.DashCooldownRemaining, 0f);
        }

        [UnityTest]
        public IEnumerator BufferedRepeatedJumpsGainSpeed()
        {
            float elapsed = 0f;
            InputSnapshot running = new InputSnapshot(Vector2.up, Vector2.zero, false, false, false, false, false);

            while (elapsed < 0.3f)
            {
                _motor.Tick(running, Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            InputSnapshot hopping = new InputSnapshot(Vector2.up, Vector2.zero, true, false, false, false, false);
            elapsed = 0f;
            float maximumSpeed = 0f;

            while (elapsed < 2f)
            {
                _motor.Tick(hopping, Time.deltaTime);
                maximumSpeed = Mathf.Max(maximumSpeed, _motor.HorizontalSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.GreaterOrEqual(_jumpCount, 3);
            Assert.Greater(maximumSpeed, _config.BaseSpeed + _config.BhopSpeedGain * 0.9f);
        }

        private void OnJumped(float speed)
        {
            _jumpCount += 1;
        }

        [UnityTest]
        public IEnumerator RepeatedJumpAgainstWallCannotClimbAboveJumpHeight()
        {
            _wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _wall.transform.position = new Vector3(0f, 3f, 1f);
            _wall.transform.localScale = new Vector3(8f, 6f, 0.5f);
            Physics.SyncTransforms();
            float maximum = 0f;
            float elapsed = 0f;
            InputSnapshot jumping = new InputSnapshot(Vector2.up, Vector2.zero, true, false, false, false, false);

            while (elapsed < 3f)
            {
                _motor.Tick(jumping, Time.deltaTime);
                maximum = Mathf.Max(maximum, _player.transform.position.y);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.Less(maximum, _config.JumpHeight + 0.3f);
        }

        [UnityTest]
        public IEnumerator DiagonalMovementExceedsCardinalSpeed()
        {
            float elapsed = 0f;
            InputSnapshot diagonal = new InputSnapshot(Vector2.one, Vector2.zero, false, false, false, false, false);

            while (elapsed < 0.8f)
            {
                _motor.Tick(diagonal, Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.Greater(_motor.HorizontalSpeed, _config.BaseSpeed * 1.3f);
            Assert.AreEqual(0f, _player.GetComponent<CharacterController>().stepOffset);
        }

        private void OnDashed(float speed)
        {
            _dashCount += 1;
        }
    }
}
