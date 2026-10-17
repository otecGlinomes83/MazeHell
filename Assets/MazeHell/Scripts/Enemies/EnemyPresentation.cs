using System;
using MazeHell.Audio;
using MazeHell.Configuration;
using MazeHell.Combat;
using UnityEngine;

namespace MazeHell.Enemies
{
    [DisallowMultipleComponent]
    public class EnemyPresentation : MonoBehaviour
    {
        private EnemyBrain _brain;
        private EnemyLocomotion _locomotion;
        private DamageTarget _damageTarget;
        private GameAudio _audio;
        private AudioConfig _config;
        private EnemyState _previousState;
        private Vector3 _previousPosition;
        private float _stepDistance;
        private bool _isSetupFinished;
        private bool _isSubscribed;

        public void Setup(EnemyBrain brain, EnemyLocomotion locomotion, DamageTarget damageTarget,
            GameAudio audio, AudioConfig config)
        {
            if (brain == null || locomotion == null || damageTarget == null || audio == null || config == null)
            {
                throw new ArgumentException($"{name}: enemy presentation requires all dependencies.");
            }

            Unsubscribe();

            _brain = brain;
            _locomotion = locomotion;
            _damageTarget = damageTarget;
            _audio = audio;
            _config = config;
            _previousState = brain.State;
            _previousPosition = transform.position;
            _stepDistance = 0f;
            _isSetupFinished = true;
            Subscribe();
        }

        public void Tick()
        {
            if (_isSetupFinished == false)
            {
                return;
            }

            EnemyState state = _brain.State;

            if (state != _previousState)
            {
                PlayStateCue(state);
                _previousState = state;
            }

            Vector3 position = transform.position;

            if (state != EnemyState.Dead && _locomotion.IsMoving)
            {
                _stepDistance += Vector3.Distance(position, _previousPosition);

                if (_stepDistance >= _config.FootstepDistance)
                {
                    _stepDistance -= _config.FootstepDistance;
                    _audio.Play("imp_footstep", position);
                }
            }

            _previousPosition = position;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_isSetupFinished == false || _isSubscribed || isActiveAndEnabled == false)
            {
                return;
            }

            _damageTarget.Impacted += OnImpacted;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (_isSubscribed == false)
            {
                return;
            }

            _damageTarget.Impacted -= OnImpacted;
            _isSubscribed = false;
        }

        private void OnImpacted(Vector3 impulse)
        {
            if (_brain.State != EnemyState.Dead)
            {
                _audio.Play("imp_pain", transform.position);
            }
        }

        private void PlayStateCue(EnemyState state)
        {
            switch (state)
            {
                case EnemyState.Chasing:
                    if (_previousState == EnemyState.Idle || _previousState == EnemyState.Investigating ||
                        _previousState == EnemyState.Searching || _previousState == EnemyState.Returning)
                    {
                        _audio.Play("imp_alert", transform.position);
                    }

                    break;
                case EnemyState.Windup:
                    _audio.Play("imp_windup", transform.position);
                    break;
                case EnemyState.Lunging:
                    _audio.Play("imp_lunge", transform.position);
                    break;
                case EnemyState.Dead:
                    _audio.Play("imp_death", transform.position);
                    break;
            }
        }
    }
}
