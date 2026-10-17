using System;
using MazeHell.Audio;
using MazeHell.Combat;
using UnityEngine;

namespace MazeHell.Level
{
    [DisallowMultipleComponent]
    public class HealingPickup : MonoBehaviour
    {
        public event Action<HealingPickup> Collected;

        private const string CollectCueId = "heal";

        private Health _playerHealth;
        private GameAudio _audio;
        private float _healAmount;
        private float _bobAmplitude;
        private float _bobSpeed;
        private float _spinDegreesPerSecond;
        private float _bobTime;
        private Vector3 _basePosition;
        private bool _isConsumed;
        private bool _isSetupFinished;

        public void Setup(Health playerHealth, float healAmount, GameAudio audio, float bobAmplitude,
            float bobSpeed, float spinDegreesPerSecond)
        {
            if (playerHealth == null)
            {
                throw new ArgumentNullException(nameof(playerHealth), $"{name}: player health is required.");
            }

            if (float.IsFinite(healAmount) == false || healAmount <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(healAmount), $"{name}: heal amount must be positive, got {healAmount}.");
            }

            if (audio == null)
            {
                throw new ArgumentNullException(nameof(audio), $"{name}: game audio is required.");
            }

            _playerHealth = playerHealth;
            _healAmount = healAmount;
            _audio = audio;
            _bobAmplitude = Mathf.Max(0f, bobAmplitude);
            _bobSpeed = Mathf.Max(0f, bobSpeed);
            _spinDegreesPerSecond = Mathf.Max(0f, spinDegreesPerSecond);
            _bobTime = 0f;
            _basePosition = transform.position;
            _isConsumed = false;
            _isSetupFinished = true;
        }

        private void Update()
        {
            if (_isSetupFinished == false || _isConsumed)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            _bobTime += deltaTime;

            float bobOffset = Mathf.Sin(_bobTime * _bobSpeed) * _bobAmplitude;
            transform.position = _basePosition + Vector3.up * bobOffset;
            transform.Rotate(Vector3.up, _spinDegreesPerSecond * deltaTime, Space.World);
        }

        private void OnTriggerStay(Collider other)
        {
            if (_isSetupFinished == false || _isConsumed)
            {
                return;
            }

            if (other.TryGetComponent(out Health health) == false || health != _playerHealth)
            {
                return;
            }

            float healedAmount = _playerHealth.Heal(_healAmount);

            if (healedAmount <= 0f)
            {
                return;
            }

            _isConsumed = true;
            _audio.Play(CollectCueId, transform.position);
            Collected?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
