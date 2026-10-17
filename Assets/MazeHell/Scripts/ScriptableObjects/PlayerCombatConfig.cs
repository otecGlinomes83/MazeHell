using UnityEngine;

namespace MazeHell.Configuration
{
    [CreateAssetMenu(fileName = "PlayerCombatConfig", menuName = "MazeHell/Configurations/Player Combat")]
    public class PlayerCombatConfig : ScriptableObject
    {
        [Header("Health")]
        [Range(1f, 999f)]
        [SerializeField] private float _maxHealth = 100f;

        [Min(0f)]
        [SerializeField] private float _hitKnockbackDistance = 0.4f;

        [Header("Block")]
        [Range(0f, 1f)]
        [SerializeField] private float _blockDamageMultiplier = 0.3f;

        [Min(0f)]
        [SerializeField] private float _blockCooldown = 2f;

        [Min(0f)]
        [SerializeField] private float _blockKnockbackDistance = 1.5f;

        [Min(0f)]
        [SerializeField] private float _blockStunTime = 0.6f;

        public float MaxHealth => _maxHealth;

        public float HitKnockbackDistance => _hitKnockbackDistance;

        public float BlockDamageMultiplier => _blockDamageMultiplier;

        public float BlockCooldown => _blockCooldown;

        public float BlockKnockbackDistance => _blockKnockbackDistance;

        public float BlockStunTime => _blockStunTime;
    }
}
