using System;
using MazeHell.Audio;
using MazeHell.Combat;
using MazeHell.Configuration;
using UnityEngine;

namespace MazeHell.Level
{
    public class PickupFactory
    {
        private GameObject _pickupPrefab;
        private LevelGenerationConfig _generation;
        private GameAudio _audio;

        public PickupFactory(GameObject pickupPrefab, LevelGenerationConfig generation, GameAudio audio)
        {
            if (pickupPrefab == null)
            {
                throw new ArgumentNullException(nameof(pickupPrefab), "Healing pickup prefab is required.");
            }

            if (generation == null)
            {
                throw new ArgumentNullException(nameof(generation), "Level generation config is required.");
            }

            if (audio == null)
            {
                throw new ArgumentNullException(nameof(audio), "Game audio is required.");
            }

            if (pickupPrefab.TryGetComponent<HealingPickup>(out _) == false ||
                pickupPrefab.TryGetComponent(out SphereCollider trigger) == false || trigger.isTrigger == false)
            {
                throw new ArgumentException("Healing pickup prefab must contain HealingPickup and a trigger SphereCollider.", nameof(pickupPrefab));
            }

            _pickupPrefab = pickupPrefab;
            _generation = generation;
            _audio = audio;
        }

        public HealingPickup Create(Vector3 position, Transform parent, Health playerHealth)
        {
            GameObject instance = UnityEngine.Object.Instantiate(_pickupPrefab, position, Quaternion.identity, parent);
            instance.name = "HealingPickup";
            instance.TryGetComponent(out HealingPickup pickup);
            pickup.Setup(playerHealth, _generation.HealPickupHealth, _audio, _generation.HealPickupBobAmplitude,
                _generation.HealPickupBobSpeed, _generation.HealPickupSpinDegreesPerSecond);
            return pickup;
        }
    }
}
