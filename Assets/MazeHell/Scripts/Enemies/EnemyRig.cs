using MazeHell.Combat;
using MazeHell.Configuration;
using UnityEngine;
using UnityEngine.AI;

namespace MazeHell.Enemies
{
    [DisallowMultipleComponent]
    public class EnemyRig : MonoBehaviour
    {
        [Tooltip("Health, damage, movement and level growth settings for this enemy prefab.")]
        [SerializeField] private EnemyConfig _config;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private CapsuleCollider _collider;
        [SerializeField] private Health _health;
        [SerializeField] private DamageTarget _damageTarget;
        [SerializeField] private EnemyPerception _perception;
        [SerializeField] private EnemyLocomotion _locomotion;
        [SerializeField] private EnemyBrain _brain;
        [SerializeField] private EnemySpriteAnimator _animator;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private EnemyPresentation _presentation;

        public EnemyConfig Config => _config;
        public NavMeshAgent Agent => _agent;
        public CapsuleCollider Collider => _collider;
        public Health Health => _health;
        public DamageTarget DamageTarget => _damageTarget;
        public EnemyPerception Perception => _perception;
        public EnemyLocomotion Locomotion => _locomotion;
        public EnemyBrain Brain => _brain;
        public EnemySpriteAnimator Animator => _animator;
        public SpriteRenderer Renderer => _renderer;
        public EnemyPresentation Presentation => _presentation;
    }
}
