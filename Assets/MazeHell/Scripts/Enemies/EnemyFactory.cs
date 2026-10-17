using System;
using MazeHell.Audio;
using MazeHell.Combat;
using MazeHell.Configuration;
using MazeHell.Player;
using UnityEngine;

namespace MazeHell.Enemies
{
    public class EnemyFactory
    {
        private EnemyRig _enemyPrefab;
        private EnemyConfig _config;
        private GameAudio _audio;
        private AudioConfig _audioConfig;
        private AnimationLibrary _animations;
        private ShotNoise _noise;

        public EnemyFactory(EnemyRig enemyPrefab, EnemyConfig config, GameAudio audio, AudioConfig audioConfig,
            AnimationLibrary animations, ShotNoise noise)
        {
            if (enemyPrefab == null)
            {
                throw new ArgumentNullException(nameof(enemyPrefab), "Enemy prefab is required.");
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "Enemy config is required.");
            }

            if (audio == null)
            {
                throw new ArgumentNullException(nameof(audio), "Game audio is required.");
            }

            if (audioConfig == null)
            {
                throw new ArgumentNullException(nameof(audioConfig), "Audio config is required.");
            }

            if (animations == null)
            {
                throw new ArgumentNullException(nameof(animations), "Animation library is required.");
            }

            if (noise == null)
            {
                throw new ArgumentNullException(nameof(noise), "Shot noise is required.");
            }

            _enemyPrefab = enemyPrefab;
            _config = config;
            _audio = audio;
            _audioConfig = audioConfig;
            _animations = animations;
            _noise = noise;
        }

        public float HealOnKill => _config.HealOnKill;

        public EnemyRig Create(Vector3 position, Quaternion rotation, int level, Transform parent, PlayerRig player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player), "Player rig is required.");
            }

            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level number starts at 1.");
            }

            EnemyRig enemy = UnityEngine.Object.Instantiate(_enemyPrefab, position, rotation, parent);
            enemy.name = $"Enemy_{level}_{position.x:F0}_{position.z:F0}";

            float maxHealth = Mathf.Min(_config.MaxHealth + (level - 1) * _config.Growth.HealthPerLevel, _config.Growth.HealthCap);
            float speedGrowthPercent = Mathf.Min((level - 1) * _config.Growth.SpeedGrowthPercentPerLevel, _config.Growth.SpeedGrowthCapPercent);
            float moveSpeedScale = 1f + speedGrowthPercent / 100f;

            enemy.Collider.height = _config.Body.BodyHeight;
            enemy.Collider.radius = _config.Body.BodyRadius;
            enemy.Collider.center = Vector3.up * (_config.Body.BodyHeight * 0.5f);
            enemy.Health.Setup(maxHealth);
            enemy.DamageTarget.Setup(enemy.Health);
            enemy.Perception.Setup(player.transform, _noise, _config);
            enemy.Locomotion.Setup(enemy.Agent, _config, moveSpeedScale);
            enemy.Animator.Setup(enemy.Renderer, enemy.transform, player.Camera, _animations, _config);
            enemy.Brain.Setup(_config, enemy.Perception, enemy.Locomotion, enemy.Animator, enemy.Health,
                enemy.DamageTarget, player.Defense, player.transform, enemy.Collider);
            enemy.Presentation.Setup(enemy.Brain, enemy.Locomotion, enemy.DamageTarget, _audio, _audioConfig);

            return enemy;
        }
    }
}
