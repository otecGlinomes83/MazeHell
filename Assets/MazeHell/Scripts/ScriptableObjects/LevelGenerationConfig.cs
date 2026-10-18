using System;
using MazeHell.Level;
using UnityEngine;

namespace MazeHell.Configuration
{
    [CreateAssetMenu(fileName = "LevelGenerationConfig", menuName = "MazeHell/Configurations/Level Generation")]
    public class LevelGenerationConfig : ScriptableObject
    {
        [Header("Seed")]
        [Min(0)]
        [SerializeField] private int _baseSeed = 1;

        [SerializeField] private bool _useFixedSeed = false;

        [Header("Room Grid")]
        [Range(4, 8)]
        [SerializeField] private int _roomGridSideStart = 4;

        [Range(0, 2)]
        [SerializeField] private int _roomGridSideGrowthPerLevel = 1;

        [Range(4, 8)]
        [SerializeField] private int _roomGridSideCap = 6;

        [Header("Graph Loops")]
        [Range(0, 64)]
        [SerializeField] private int _loopEdgeCountMin = 1;

        [Range(0, 64)]
        [SerializeField] private int _loopEdgeCountMax = 3;

        [Header("Dead Ends")]
        [Range(0, 128)]
        [SerializeField] private int _deadEndMinimum = 3;

        [Header("Difficulty")]
        [Range(0, 120)]
        [SerializeField] private int _enemyCountStart = 8;

        [Range(0, 40)]
        [SerializeField] private int _enemyCountGrowthPerLevel = 3;

        [Range(1, 200)]
        [SerializeField] private int _enemyCountCap = 40;

        [Header("Pickups")]
        [Range(0, 20)]
        [SerializeField] private int _healPickupCount = 3;

        [Min(1f)]
        [SerializeField] private float _healPickupHealth = 25f;

        [Min(0f)]
        [SerializeField] private float _healPickupBobAmplitude = 0.35f;

        [Min(0f)]
        [SerializeField] private float _healPickupBobSpeed = 2f;

        [Min(0f)]
        [SerializeField] private float _healPickupSpinDegreesPerSecond = 90f;

        [Header("Spawn Distances")]
        [Min(0f)]
        [SerializeField] private float _enemyMinSpawnDistance = 12f;

        [Min(0f)]
        [SerializeField] private float _enemyMinSpacing = 3f;

        [Min(0f)]
        [SerializeField] private float _pickupMinSpawnDistance = 8f;

        [Header("Level Prefabs")]
        [SerializeField] private LevelRoot _levelPrefab;

        [SerializeField] private GameObject _closedDoorPrefab;
        [SerializeField] private DoorConfig _doorConfig;

        [Header("Room Selection")]
        [SerializeField] private string[] _mainRoomIds = new string[] { "room_quarters", "room_ring", "room_comb" };
        [Min(0)] [SerializeField] private int _sideRoomCountMin = 2;
        [Min(0)] [SerializeField] private int _sideRoomCountMax = 4;

        [Header("Decoration")]
        [SerializeField] private GameObject[] _decorationPrefabs = new GameObject[0];
        [Min(0)] [SerializeField] private int _decorationsPerRoomMin = 3;
        [Min(0)] [SerializeField] private int _decorationsPerRoomMax = 6;
        [Min(0.5f)] [SerializeField] private float _decorationWallInset = 2f;
        [Min(0.5f)] [SerializeField] private float _decorationSpacing = 4f;
        [Min(0.5f)] [SerializeField] private float _doorwayClearance = 5f;

        [Header("Module Catalog")]
        [SerializeField] private ModuleCatalogEntry[] _moduleCatalog = new ModuleCatalogEntry[0];

        public int BaseSeed => _baseSeed;

        public bool UseFixedSeed => _useFixedSeed;

        public int RoomGridSideStart => _roomGridSideStart;

        public int RoomGridSideGrowthPerLevel => _roomGridSideGrowthPerLevel;

        public int RoomGridSideCap => _roomGridSideCap;

        public int LoopEdgeCountMin => _loopEdgeCountMin;

        public int LoopEdgeCountMax => _loopEdgeCountMax;

        public int DeadEndMinimum => _deadEndMinimum;

        public int EnemyCountStart => _enemyCountStart;

        public int EnemyCountGrowthPerLevel => _enemyCountGrowthPerLevel;

        public int EnemyCountCap => _enemyCountCap;

        public int HealPickupCount => _healPickupCount;

        public float HealPickupHealth => _healPickupHealth;

        public float HealPickupBobAmplitude => _healPickupBobAmplitude;

        public float HealPickupBobSpeed => _healPickupBobSpeed;

        public float HealPickupSpinDegreesPerSecond => _healPickupSpinDegreesPerSecond;

        public float EnemyMinSpawnDistance => _enemyMinSpawnDistance;

        public float EnemyMinSpacing => _enemyMinSpacing;

        public float PickupMinSpawnDistance => _pickupMinSpawnDistance;

        public ModuleCatalogEntry[] ModuleCatalog => _moduleCatalog;

        public LevelRoot LevelPrefab => _levelPrefab;

        public GameObject ClosedDoorPrefab => _closedDoorPrefab;
        public DoorConfig DoorConfig => _doorConfig;
        public string[] MainRoomIds => _mainRoomIds;
        public int SideRoomCountMin => _sideRoomCountMin;
        public int SideRoomCountMax => _sideRoomCountMax;
        public GameObject[] DecorationPrefabs => _decorationPrefabs;
        public int DecorationsPerRoomMin => _decorationsPerRoomMin;
        public int DecorationsPerRoomMax => _decorationsPerRoomMax;
        public float DecorationWallInset => _decorationWallInset;
        public float DecorationSpacing => _decorationSpacing;
        public float DoorwayClearance => _doorwayClearance;
    }

    [Serializable]
    public class ModuleCatalogEntry
    {
        [SerializeField] private string _moduleId;

        [SerializeField] private GameObject _prefab;

        [SerializeField] private Vector3 _footprintSize;

        public ModuleCatalogEntry()
        {
        }

        public ModuleCatalogEntry(string moduleId, GameObject prefab, Vector3 footprintSize)
        {
            _moduleId = moduleId;
            _prefab = prefab;
            _footprintSize = footprintSize;
        }

        public string ModuleId => _moduleId;

        public GameObject Prefab => _prefab;

        public Vector3 FootprintSize => _footprintSize;
    }
}
