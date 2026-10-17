using System;
using MazeHell.Combat;
using MazeHell.Configuration;
using UnityEngine;

namespace MazeHell.Player
{
    [DisallowMultipleComponent]
    public class MuzzleFlash : MonoBehaviour
    {
        private Light _light;
        private Shotgun _shotgun;
        private ShotgunConfig _config;
        private float _remaining;
        private bool _isSubscribed;
        private bool _isSetupFinished;

        public void Setup(Light light, Shotgun shotgun, ShotgunConfig config)
        {
            if (light == null)
            {
                throw new ArgumentNullException(nameof(light), $"{name}: muzzle light is required.");
            }

            if (shotgun == null)
            {
                throw new ArgumentNullException(nameof(shotgun), $"{name}: shotgun is required.");
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), $"{name}: shotgun config is required.");
            }

            Unsubscribe();

            _light = light;
            _shotgun = shotgun;
            _config = config;
            _remaining = 0f;
            _light.enabled = false;
            _light.color = config.MuzzleFlashColor;
            _light.intensity = 0f;
            _isSetupFinished = true;
            Subscribe();
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

            _shotgun.Fired += OnFired;
            _isSubscribed = true;
        }

        public void Tick(float deltaTime)
        {
            if (_isSetupFinished == false || _remaining <= 0f)
            {
                return;
            }

            _remaining = Mathf.Max(0f, _remaining - deltaTime);
            float flashFraction = _remaining / Mathf.Max(0.01f, _config.MuzzleFlashDuration);
            _light.intensity = _config.MuzzleFlashIntensity * flashFraction;

            if (_remaining <= 0f)
            {
                _light.enabled = false;
                _light.intensity = 0f;
            }
        }

        private void OnFired(bool isVolley)
        {
            _remaining = _config.MuzzleFlashDuration;
            _light.color = _config.MuzzleFlashColor;
            _light.range = _config.MuzzleFlashRange;
            _light.intensity = _config.MuzzleFlashIntensity;
            _light.enabled = true;
        }

        private void OnDisable()
        {
            Unsubscribe();

            if (_isSetupFinished == false)
            {
                return;
            }

            _remaining = 0f;
            _light.enabled = false;
            _light.intensity = 0f;
        }

        private void Unsubscribe()
        {
            if (_isSubscribed == false)
            {
                return;
            }

            _shotgun.Fired -= OnFired;
            _isSubscribed = false;
        }
    }
}
