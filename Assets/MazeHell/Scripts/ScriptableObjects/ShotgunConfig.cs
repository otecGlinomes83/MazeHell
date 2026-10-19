using UnityEngine;

namespace MazeHell.Configuration
{
    [CreateAssetMenu(fileName = "ShotgunConfig", menuName = "MazeHell/Configurations/Shotgun")]
    public class ShotgunConfig : ScriptableObject
    {
        [Header("Barrels")]
        [Range(1, 4)]
        [SerializeField] private int _barrelCount = 2;

        [Header("Pellets")]
        [Range(1, 32)]
        [SerializeField] private int _pelletCount = 8;

        [Min(0f)]
        [SerializeField] private float _pelletDamage = 2.5f;

        [Range(0f, 45f)]
        [SerializeField] private float _spreadAngle = 5f;

        [Min(1f)]
        [SerializeField] private float _maxRange = 25f;

        [SerializeField] private LayerMask _hitMask = -1;

        [Header("Timing")]
        [Min(0.01f)]
        [SerializeField] private float _minShotInterval = 0.12f;

        [Min(0.01f)]
        [SerializeField] private float _volleyWindow = 0.25f;

        [Min(0.01f)]
        [SerializeField] private float _reloadDuration = 0.6f;

        [Range(0.05f, 0.95f)]
        [SerializeField] private float _reloadBreakProgress = 0.4f;

        [Header("Sprite Animation")]
        [Min(0f)]
        [SerializeField] private float _shotSpriteHoldTime = 0.08f;

        [Min(0f)]
        [SerializeField] private float _volleySpriteHoldTime = 0.12f;

        [Header("Shot Noise")]
        [Min(0f)]
        [SerializeField] private float _noiseRadius = 40f;

        [Header("Muzzle Flash")]
        [Min(0f)]
        [SerializeField] private float _muzzleFlashIntensity = 45f;

        [Min(0.01f)]
        [SerializeField] private float _muzzleFlashDuration = 0.06f;

        [Min(0.1f)]
        [SerializeField] private float _muzzleFlashRange = 8f;

        [SerializeField] private Color _muzzleFlashColor = new Color(1f, 0.72f, 0.4f, 1f);

        [Header("Enemy Knockback")]
        [Min(0f)]
        [SerializeField] private float _enemyKnockbackSpeed = 5f;

        [Min(1f)]
        [SerializeField] private float _volleyKnockbackMultiplier = 2f;

        public int BarrelCount => _barrelCount;

        public int PelletCount => _pelletCount;

        public float PelletDamage => _pelletDamage;

        public float SpreadAngle => _spreadAngle;

        public float MaxRange => _maxRange;

        public LayerMask HitMask => _hitMask;

        public float MinShotInterval => _minShotInterval;

        public float VolleyWindow => _volleyWindow;

        public float ReloadDuration => _reloadDuration;

        public float ReloadBreakProgress => _reloadBreakProgress;

        public float ShotSpriteHoldTime => _shotSpriteHoldTime;

        public float VolleySpriteHoldTime => _volleySpriteHoldTime;

        public float NoiseRadius => _noiseRadius;

        public float MuzzleFlashIntensity => _muzzleFlashIntensity;

        public float MuzzleFlashDuration => _muzzleFlashDuration;

        public float MuzzleFlashRange => _muzzleFlashRange;

        public Color MuzzleFlashColor => _muzzleFlashColor;

        public float EnemyKnockbackSpeed => _enemyKnockbackSpeed;

        public float VolleyKnockbackMultiplier => _volleyKnockbackMultiplier;
    }
}
