using System;
using MazeHell.Combat;
using MazeHell.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace MazeHell.UI
{
    [DisallowMultipleComponent]
    public class WeaponSpriteAnimator : MonoBehaviour
    {
        private Image _image;
        private Shotgun _gun;
        private WeaponSpriteSetData _weaponSet;
        private ShotgunConfig _config;
        private bool _isSetupFinished;

        private float _fireHoldTimer;
        private float _fireFrameTimer;
        private int _fireFrameIndex;
        private Sprite[] _fireFrames;
        private Sprite _currentSprite;

        public void Setup(Image image, Shotgun gun, AnimationLibrary library, ShotgunConfig config)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), $"{name}: weapon image is required.");
            }

            if (gun == null)
            {
                throw new ArgumentNullException(nameof(gun), $"{name}: shotgun is required.");
            }

            if (library == null)
            {
                throw new ArgumentNullException(nameof(library), $"{name}: animation library is required.");
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), $"{name}: shotgun config is required.");
            }

            UnsubscribeFromGun();

            _image = image;
            _gun = gun;
            _weaponSet = library.WeaponSet;
            _config = config;
            _isSetupFinished = true;

            _fireHoldTimer = 0f;
            _fireFrameTimer = 0f;
            _fireFrameIndex = 0;
            _fireFrames = null;
            _currentSprite = null;

            if (isActiveAndEnabled)
            {
                SubscribeToGun();
            }
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

            if (_gun.IsBlocking)
            {
                ShowSprite(_weaponSet.BlockSprite);

                return;
            }

            if (_fireHoldTimer > 0f)
            {
                UpdateFireHold(deltaTime);

                return;
            }

            if (_gun.ReloadRemaining > 0f)
            {
                ShowReloadFrame();

                return;
            }

            ShowSprite(_weaponSet.IdleSprite);
        }

        private void UpdateFireHold(float deltaTime)
        {
            _fireHoldTimer -= deltaTime;

            if (_fireFrames == null || _fireFrames.Length == 0)
            {
                ShowSprite(_weaponSet.IdleSprite);

                return;
            }

            _fireFrameTimer += deltaTime;

            if (_fireFrameTimer >= _weaponSet.FrameInterval && _fireFrameIndex < _fireFrames.Length - 1)
            {
                _fireFrameTimer -= _weaponSet.FrameInterval;
                _fireFrameIndex += 1;
            }

            ShowSprite(_fireFrames[_fireFrameIndex]);
        }

        private void ShowReloadFrame()
        {
            Sprite[] reloadFrames = _weaponSet.ReloadFrames;

            if (reloadFrames == null || reloadFrames.Length == 0)
            {
                ShowSprite(_weaponSet.EmptySprite);

                return;
            }

            float reloadProgress = 1f - _gun.ReloadRemaining / _config.ReloadDuration;
            int midpoint = reloadFrames.Length / 2;

            if (reloadProgress < _config.ReloadBreakProgress)
            {
                int firstPhaseLength = Mathf.Max(1, midpoint);
                float firstPhaseProgress = reloadProgress / _config.ReloadBreakProgress;
                int firstFrameIndex = Mathf.Clamp(Mathf.FloorToInt(firstPhaseProgress * firstPhaseLength), 0, firstPhaseLength - 1);

                ShowSprite(reloadFrames[firstFrameIndex]);

                return;
            }

            int secondPhaseLength = Mathf.Max(1, reloadFrames.Length - midpoint);
            float secondPhaseProgress = (reloadProgress - _config.ReloadBreakProgress) / (1f - _config.ReloadBreakProgress);
            int secondFrameIndex = Mathf.Clamp(Mathf.FloorToInt(secondPhaseProgress * secondPhaseLength), 0, secondPhaseLength - 1);

            ShowSprite(reloadFrames[Mathf.Min(midpoint + secondFrameIndex, reloadFrames.Length - 1)]);
        }

        private void ShowSprite(Sprite sprite)
        {
            if (_currentSprite == sprite)
            {
                return;
            }

            _currentSprite = sprite;
            _image.sprite = sprite;
        }

        private void OnGunChanged(Shotgun gun)
        {
            _currentSprite = null;
        }

        private void OnGunFired(bool isVolley)
        {
            if (isVolley)
            {
                _fireHoldTimer = _config.VolleySpriteHoldTime;
                _fireFrames = _weaponSet.VolleyFrames;
            }
            else
            {
                _fireHoldTimer = _config.ShotSpriteHoldTime;
                _fireFrames = _weaponSet.ShotFrames;
            }

            _fireFrameTimer = 0f;
            _fireFrameIndex = 0;
        }

        private void OnEnable()
        {
            SubscribeToGun();
        }

        private void SubscribeToGun()
        {
            if (_isSetupFinished == false)
            {
                return;
            }

            UnsubscribeFromGun();
            _gun.Changed += OnGunChanged;
            _gun.Fired += OnGunFired;
        }

        private void OnDisable()
        {
            UnsubscribeFromGun();
        }

        private void UnsubscribeFromGun()
        {
            if (_gun == null)
            {
                return;
            }

            _gun.Changed -= OnGunChanged;
            _gun.Fired -= OnGunFired;
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: weapon sprite animator is not initialized, call Setup first.");
            }
        }
    }
}
