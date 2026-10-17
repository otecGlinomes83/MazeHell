using System;
using MazeHell.Combat;
using MazeHell.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace MazeHell.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public class HealthPortrait : MonoBehaviour
    {
        private Image _image;

        private Health _health;

        private AnimationLibrary _sprites;

        private HudMotionConfig _config;

        private FaceState _currentState;

        private FaceState _displayedState;

        private float _previousHealthFraction;

        private float _hitOverrideTimer;

        private float _frameTimer;

        private int _frameIndex;

        private bool _isSetupFinished;

        private bool _isSubscribed;

        public void Setup(Image image, Health health, AnimationLibrary sprites, HudMotionConfig config)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (health == null)
            {
                throw new ArgumentNullException(nameof(health));
            }

            if (sprites == null)
            {
                throw new ArgumentNullException(nameof(sprites));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Unsubscribe();

            _image = image;
            _health = health;
            _sprites = sprites;
            _config = config;

            _previousHealthFraction = ReadHealthFraction(health);
            _currentState = ResolveState(_previousHealthFraction);
            _displayedState = _currentState;
            _hitOverrideTimer = 0f;
            _frameTimer = 0f;
            _frameIndex = 0;
            _isSetupFinished = true;

            if (isActiveAndEnabled)
            {
                Subscribe();
            }

            ApplyFirstFrame();
        }

        public void Tick(float deltaTime)
        {
            AssertSetup();

            float clampedDeltaTime = Mathf.Max(0f, deltaTime);

            if (_hitOverrideTimer > 0f)
            {
                _hitOverrideTimer = Mathf.Max(0f, _hitOverrideTimer - clampedDeltaTime);
                AnimateState(FaceState.Hit, clampedDeltaTime);

                if (_hitOverrideTimer == 0f)
                {
                    SwitchState(_currentState);
                }

                return;
            }

            AnimateState(_displayedState, clampedDeltaTime);
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnHealthChanged(Health health)
        {
            float healthFraction = ReadHealthFraction(health);
            FaceState resolvedState = ResolveState(healthFraction);

            if (healthFraction < _previousHealthFraction)
            {
                SwitchState(FaceState.Hit);
                _hitOverrideTimer = _config.HitFaceOverrideDuration;
            }
            else if (_displayedState != FaceState.Hit)
            {
                SwitchState(resolvedState);
            }

            _previousHealthFraction = healthFraction;
            _currentState = resolvedState;
        }

        private void Subscribe()
        {
            if (_isSetupFinished == false)
            {
                return;
            }

            if (_isSubscribed)
            {
                return;
            }

            _health.Changed += OnHealthChanged;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (_isSubscribed == false)
            {
                return;
            }

            _health.Changed -= OnHealthChanged;
            _isSubscribed = false;
        }

        private void SwitchState(FaceState state)
        {
            _displayedState = state;
            _frameIndex = 0;
            _frameTimer = 0f;

            ApplyFirstFrame();
        }

        private void AnimateState(FaceState state, float deltaTime)
        {
            FaceTierData tierData = ResolveTierData(state);
            Sprite[] frames = tierData.Frames;

            if (frames == null)
            {
                return;
            }

            if (frames.Length == 0)
            {
                return;
            }

            _frameTimer += Mathf.Min(deltaTime, 2f);
            float frameInterval = ResolveFrameDuration(tierData);

            while (_frameTimer >= frameInterval)
            {
                _frameTimer -= frameInterval;
                _frameIndex = (_frameIndex + 1) % frames.Length;
                frameInterval = ResolveFrameDuration(tierData);
            }

            _image.sprite = frames[_frameIndex];
        }

        private float ResolveFrameDuration(FaceTierData tier)
        {
            if (tier.FrameDurations != null && _frameIndex < tier.FrameDurations.Length)
            {
                return Mathf.Max(0.01f, tier.FrameDurations[_frameIndex]);
            }

            return Mathf.Max(0.01f, _sprites.FaceSet.LoopFrameInterval);
        }

        private void ApplyFirstFrame()
        {
            FaceTierData tierData = ResolveTierData(_displayedState);
            Sprite[] frames = tierData.Frames;

            if (frames == null)
            {
                return;
            }

            if (frames.Length == 0)
            {
                return;
            }

            _image.sprite = frames[0];
        }

        private FaceTierData ResolveTierData(FaceState state)
        {
            FaceSpriteSetData faceSet = _sprites.FaceSet;

            switch (state)
            {
                case FaceState.Scratched:
                    return faceSet.Scratched;

                case FaceState.Bleeding:
                    return faceSet.Bleeding;

                case FaceState.Critical:
                    return faceSet.Critical;

                case FaceState.NearDeath:
                    return faceSet.NearDeath;

                case FaceState.Hit:
                    return faceSet.Hit;

                default:
                    return faceSet.Healthy;
            }
        }

        private FaceState ResolveState(float healthFraction)
        {
            if (healthFraction > _config.ScratchedHealthFraction)
            {
                return FaceState.Healthy;
            }

            if (healthFraction > _config.BleedingHealthFraction)
            {
                return FaceState.Scratched;
            }

            if (healthFraction > _config.CriticalHealthFraction)
            {
                return FaceState.Bleeding;
            }

            if (healthFraction > _config.NearDeathHealthFraction)
            {
                return FaceState.Critical;
            }

            return FaceState.NearDeath;
        }

        private static float ReadHealthFraction(Health health)
        {
            if (health.MaxHealth <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(health.CurrentHealth / health.MaxHealth);
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: health portrait is not initialized, call Setup first.");
            }
        }
    }
}
