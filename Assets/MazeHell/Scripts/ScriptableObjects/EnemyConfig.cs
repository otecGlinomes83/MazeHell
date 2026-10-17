using System;
using UnityEngine;

namespace MazeHell.Configuration
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "MazeHell/Configurations/Enemy")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Health")]
        [Range(1f, 500f)]
        [SerializeField] private float _maxHealth = 30f;

        [Range(0f, 100f)]
        [SerializeField] private float _healOnKill = 10f;

        [Header("Movement")]
        [Min(0.5f)]
        [SerializeField] private float _moveSpeed = 6.2f;

        [Min(0.01f)]
        [SerializeField] private float _pathRefreshInterval = 0.25f;

        [Header("Growth")]
        [SerializeField] private EnemyGrowthData _growth = new EnemyGrowthData();

        [Header("Perception")]
        [SerializeField] private EnemyPerceptionData _perception = new EnemyPerceptionData();

        [Header("Attack")]
        [SerializeField] private EnemyAttackData _attack = new EnemyAttackData();

        [Header("Flinch")]
        [Range(0f, 1f)]
        [SerializeField] private float _flinchTime = 0.2f;

        [Header("Body")]
        [SerializeField] private EnemyBodyData _body = new EnemyBodyData();

        public float MaxHealth => _maxHealth;

        public float HealOnKill => _healOnKill;

        public float MoveSpeed => _moveSpeed;

        public float PathRefreshInterval => _pathRefreshInterval;

        public EnemyGrowthData Growth => _growth;

        public EnemyPerceptionData Perception => _perception;

        public EnemyAttackData Attack => _attack;

        public float FlinchTime => _flinchTime;

        public EnemyBodyData Body => _body;
    }

    [Serializable]
    public class EnemyGrowthData
    {
        [Min(0f)]
        [SerializeField] private float _healthPerLevel = 2f;

        [Min(1f)]
        [SerializeField] private float _healthCap = 50f;

        [Range(0f, 100f)]
        [SerializeField] private float _speedGrowthPercentPerLevel = 1.5f;

        [Range(0f, 100f)]
        [SerializeField] private float _speedGrowthCapPercent = 25f;

        public float HealthPerLevel => _healthPerLevel;

        public float HealthCap => _healthCap;

        public float SpeedGrowthPercentPerLevel => _speedGrowthPercentPerLevel;

        public float SpeedGrowthCapPercent => _speedGrowthCapPercent;
    }

    [Serializable]
    public class EnemyPerceptionData
    {
        [Min(0.5f)]
        [SerializeField] private float _visionRange = 12f;

        [Range(10f, 360f)]
        [SerializeField] private float _visionFovDegrees = 90f;

        [Min(0f)]
        [SerializeField] private float _hearingRadius = 25f;

        [Min(0.05f)]
        [SerializeField] private float _loseSightGraceTime = 1f;

        [Min(0.1f)]
        [SerializeField] private float _searchWaitDuration = 2.5f;

        [Min(0.05f)]
        [SerializeField] private float _returnToleranceDistance = 0.5f;

        [Range(0.01f, 0.5f)]
        [SerializeField] private float _perceptionInterval = 0.1f;

        [SerializeField] private LayerMask _perceptionMask = ~(1 << 9);

        public float VisionRange => _visionRange;

        public float VisionFovDegrees => _visionFovDegrees;

        public float HearingRadius => _hearingRadius;

        public float LoseSightGraceTime => _loseSightGraceTime;

        public float SearchWaitDuration => _searchWaitDuration;

        public float ReturnToleranceDistance => _returnToleranceDistance;

        public float PerceptionInterval => _perceptionInterval;

        public LayerMask PerceptionMask => _perceptionMask;
    }

    [Serializable]
    public class EnemyAttackData
    {
        [Min(0.5f)]
        [SerializeField] private float _attackRange = 2.5f;

        [Range(0.05f, 2f)]
        [SerializeField] private float _windupDuration = 0.38f;

        [Range(0f, 2f)]
        [SerializeField] private float _windupProtectedFinalTime = 0.15f;

        [Min(0.5f)]
        [SerializeField] private float _lungeDistance = 3.5f;

        [Range(0.05f, 1f)]
        [SerializeField] private float _lungeDuration = 0.3f;

        [Min(0.1f)]
        [SerializeField] private float _attackCooldown = 0.9f;

        [Min(0f)]
        [SerializeField] private float _damageMin = 15f;

        [Min(0f)]
        [SerializeField] private float _damageMax = 18f;

        [SerializeField] private LayerMask _lungeCollisionMask = 1;

        [Min(0f)]
        [SerializeField] private float _collisionSkin = 0.02f;

        [Min(0.1f)]
        [SerializeField] private float _hitDistance = 0.85f;

        public float AttackRange => _attackRange;

        public float WindupDuration => _windupDuration;

        public float WindupProtectedFinalTime => _windupProtectedFinalTime;

        public float LungeDistance => _lungeDistance;

        public float LungeDuration => _lungeDuration;

        public float AttackCooldown => _attackCooldown;

        public float DamageMin => _damageMin;

        public float DamageMax => _damageMax;

        public LayerMask LungeCollisionMask => _lungeCollisionMask;

        public float CollisionSkin => _collisionSkin;

        public float HitDistance => _hitDistance;
    }

    [Serializable]
    public class EnemyBodyData
    {
        [Min(0.1f)]
        [SerializeField] private float _bodyRadius = 0.45f;

        [Min(0.1f)]
        [SerializeField] private float _bodyHeight = 1.8f;

        [Min(0.1f)]
        [SerializeField] private float _agentRadius = 0.4f;

        [Min(0.1f)]
        [SerializeField] private float _agentHeight = 1.8f;

        [Min(0f)]
        [SerializeField] private float _agentAcceleration = 20f;

        [Min(0f)]
        [SerializeField] private float _agentAngularSpeed = 540f;

        [Min(0f)]
        [SerializeField] private float _agentStoppingDistance = 2.2f;

        public float BodyRadius => _bodyRadius;

        public float BodyHeight => _bodyHeight;

        public float AgentRadius => _agentRadius;

        public float AgentHeight => _agentHeight;

        public float AgentAcceleration => _agentAcceleration;

        public float AgentAngularSpeed => _agentAngularSpeed;

        public float AgentStoppingDistance => _agentStoppingDistance;
    }
}
