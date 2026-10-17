using System;
using MazeHell.Audio;
using MazeHell.Combat;
using UnityEngine;

namespace MazeHell.Player
{
    [DisallowMultipleComponent]
    public class PlayerFeedback : MonoBehaviour
    {
        private const string JumpCueId = "jump";
        private const string LandCueId = "land";
        private const string DashCueId = "dash";
        private const string HurtCueId = "player_hit";
        private const string BlockCueId = "block";
        private const string ShotCueId = "shotgun_single";
        private const string VolleyCueId = "shotgun_volley";
        private const string ReloadCueId = "shotgun_reload";
        private const float GroundedFootstepSpeed = 0.5f;

        private PlayerRig _player;
        private GameAudio _audio;
        private Vector3 _previousPosition;
        private bool _isSubscribed;
        private bool _isSetupFinished;

        public void Setup(PlayerRig player, GameAudio audio)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player), $"{name}: player rig is required.");
            }

            if (audio == null)
            {
                throw new ArgumentNullException(nameof(audio), $"{name}: game audio is required.");
            }

            Unsubscribe();

            _player = player;
            _audio = audio;
            ResetTravel();
            _isSetupFinished = true;
            Subscribe();
        }

        public void ResetTravel()
        {
            _previousPosition = transform.position;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_isSetupFinished == false || _isSubscribed)
            {
                return;
            }

            _player.Motor.Jumped += OnJumped;
            _player.Motor.Landed += OnLanded;
            _player.Motor.Dashed += OnDashed;
            _player.Defense.Damaged += OnDamaged;
            _player.Defense.Blocked += OnBlocked;
            _player.Shotgun.Fired += OnFired;
            _player.Shotgun.ReloadStarted += OnReloadStarted;
            _isSubscribed = true;
        }

        public void Tick()
        {
            if (_isSetupFinished == false)
            {
                return;
            }

            AccumulateFootsteps();
        }

        private void AccumulateFootsteps()
        {
            Vector3 position = transform.position;
            Vector3 travel = position - _previousPosition;
            travel.y = 0f;
            _previousPosition = position;

            if (_player.Motor.IsGrounded == false)
            {
                return;
            }

            if (_player.Motor.HorizontalSpeed < GroundedFootstepSpeed)
            {
                return;
            }

            _audio.Step(travel.magnitude);
        }

        private void OnJumped(float verticalSpeed)
        {
            _player.CameraMotion.ReactJump();
            _audio.Play(JumpCueId, Vector3.zero);
        }

        private void OnLanded(float fallSpeed)
        {
            _player.CameraMotion.ReactLanding(fallSpeed);
            _audio.Play(LandCueId, Vector3.zero);
        }

        private void OnDashed(float dashSpeed)
        {
            _player.CameraMotion.ReactDash();
            _audio.Play(DashCueId, Vector3.zero);
        }

        private void OnDamaged(float damage)
        {
            _player.CameraMotion.ReactDamage();
            _audio.Play(HurtCueId, Vector3.zero);
        }

        private void OnBlocked(float stunDuration)
        {
            _player.CameraMotion.ReactBlock();
            _audio.Play(BlockCueId, Vector3.zero);
        }

        private void OnFired(bool isVolley)
        {
            _player.CameraMotion.ReactShot(isVolley);
            if (isVolley)
            {
                _audio.Play(VolleyCueId, Vector3.zero);
            }
            else
            {
                _audio.Play(ShotCueId, Vector3.zero);
            }
        }

        private void OnReloadStarted(Shotgun shotgun)
        {
            _audio.Play(ReloadCueId, Vector3.zero);
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (_isSubscribed == false)
            {
                return;
            }

            _player.Motor.Jumped -= OnJumped;
            _player.Motor.Landed -= OnLanded;
            _player.Motor.Dashed -= OnDashed;
            _player.Defense.Damaged -= OnDamaged;
            _player.Defense.Blocked -= OnBlocked;
            _player.Shotgun.Fired -= OnFired;
            _player.Shotgun.ReloadStarted -= OnReloadStarted;
            _isSubscribed = false;
        }
    }
}
