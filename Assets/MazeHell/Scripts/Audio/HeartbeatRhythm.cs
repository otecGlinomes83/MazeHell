using System;
using MazeHell.Configuration;
using UnityEngine;

namespace MazeHell.Audio
{
    public class HeartbeatRhythm
    {
        private AudioConfig _config;
        private float _timer;
        private float _intensity;
        private float _interval;
        private float _volume;
        private float _pitch;
        private bool _isActive;

        public float Intensity => _intensity;
        public float Interval => _interval;
        public float Volume => _volume;
        public float Pitch => _pitch;
        public bool IsActive => _isActive;

        public HeartbeatRhythm(AudioConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
        }

        public bool Tick(float healthFraction, float transitionProgress, float deltaTime)
        {
            float health = Mathf.Clamp01(healthFraction);
            float healthDanger = 0f;

            if (health > 0f && health < _config.HeartbeatHealthThreshold)
            {
                healthDanger = 1f - health / _config.HeartbeatHealthThreshold;
            }

            _intensity = Mathf.Max(healthDanger, Mathf.Clamp01(transitionProgress));
            bool wasActive = _isActive;
            _isActive = _intensity > 0f;
            _interval = Mathf.Lerp(_config.HeartbeatIntervalHealthy, _config.HeartbeatIntervalCritical, _intensity);
            _volume = Mathf.Lerp(_config.HeartbeatVolumeHealthy, _config.HeartbeatVolumeCritical, _intensity);
            _pitch = Mathf.Lerp(1f, _config.HeartbeatCriticalPitch, _intensity);

            if (_isActive == false)
            {
                _timer = 0f;
                return false;
            }

            if (wasActive == false)
            {
                _timer = _interval;
            }

            _timer += Mathf.Max(0f, deltaTime);

            if (_timer < _interval)
            {
                return false;
            }

            _timer = Mathf.Repeat(_timer, _interval);
            return true;
        }
    }
}
