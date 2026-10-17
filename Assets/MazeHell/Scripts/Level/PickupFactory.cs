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

            _pickupPrefab = pickupPrefab;
            _generation = generation;
            _audio = audio;
        }

        public HealingPickup Create(Vector3 position, Transform parent, Health playerHealth)
        {
            GameObject instance = UnityEngine.Object.Instantiate(_pickupPrefab, position, Quaternion.identity, parent);
            instance.name = "HealingPickup";
            EnsureTrigger(instance);

            HealingPickup pickup = instance.AddComponent<HealingPickup>();
            pickup.Setup(playerHealth, _generation.HealPickupHealth, _audio, _generation.HealPickupBobAmplitude,
                _generation.HealPickupBobSpeed, _generation.HealPickupSpinDegreesPerSecond);
            return pickup;
        }

        private void EnsureTrigger(GameObject instance)
        {
            if (instance.TryGetComponent(out SphereCollider sphereCollider))
            {
                sphereCollider.isTrigger = true;
                sphereCollider.radius = _generation.HealPickupRadius;
                return;
            }

            SphereCollider trigger = instance.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = _generation.HealPickupRadius;
        }
    }
}
