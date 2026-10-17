using System;
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

        [Header("Logical Grid")]
        [Range(8, 64)]
        [SerializeField] private int _logicalSideStart = 16;

        [Range(0, 8)]
        [SerializeField] private int _logicalSideGrowthPerLevel = 2;

        [Range(8, 128)]
        [SerializeField] private int _logicalSideCap = 36;

        [Header("Physical Scale")]
        [Min(0.5f)]
        [SerializeField] private float _cellSize = 4f;

        [Min(0.1f)]
        [SerializeField] private float _wallThickness = 0.4f;

        [Min(1f)]
        [SerializeField] private float _ceilingHeight = 4.5f;

        [Header("Graph Loops")]
        [Range(0, 64)]
        [SerializeField] private int _loopEdgeCountMin = 8;

        [Range(0, 64)]
        [SerializeField] private int _loopEdgeCountMax = 14;

        [Header("Dead Ends")]
        [Range(0, 128)]
        [SerializeField] private int _deadEndMinimum = 10;

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

        [Min(0.1f)]
        [SerializeField] private float _healPickupRadius = 1.2f;

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

        [Header("Connectors And Rooms")]
        [Min(1f)]
        [SerializeField] private float _connectorLength = 24f;

        [Min(1f)]
        [SerializeField] private float _corridorWidth = 6f;

        [Min(1f)]
        [SerializeField] private float _corridorHeight = 6f;

        [Min(0.05f)]
        [SerializeField] private float _floorThickness = 0.2f;

        [Min(1f)]
        [SerializeField] private float _roomDoorWidth = 6f;

        [Min(1f)]
        [SerializeField] private float _roomDoorHeight = 4.5f;

        [Min(1f)]
        [SerializeField] private float _gateOpeningWidth = 10f;

        [Min(1f)]
        [SerializeField] private float _roomHeight = 8f;

        [Header("Module Catalog")]
        [SerializeField] private ModuleCatalogEntry[] _moduleCatalog = new ModuleCatalogEntry[0];

        [Header("Materials")]
        [SerializeField] private Material _wallMaterial;

        [SerializeField] private Material _floorMaterial;

        [SerializeField] private Material _ceilingMaterial;

        [SerializeField] private Material _exitCorridorMaterial;

        public int BaseSeed => _baseSeed;

        public bool UseFixedSeed => _useFixedSeed;

        public int LogicalSideStart => _logicalSideStart;

        public int LogicalSideGrowthPerLevel => _logicalSideGrowthPerLevel;

        public int LogicalSideCap => _logicalSideCap;

        public float CellSize => _cellSize;

        public float WallThickness => _wallThickness;

        public float CeilingHeight => _ceilingHeight;

        public int LoopEdgeCountMin => _loopEdgeCountMin;

        public int LoopEdgeCountMax => _loopEdgeCountMax;

        public int DeadEndMinimum => _deadEndMinimum;

        public int EnemyCountStart => _enemyCountStart;

        public int EnemyCountGrowthPerLevel => _enemyCountGrowthPerLevel;

        public int EnemyCountCap => _enemyCountCap;

        public int HealPickupCount => _healPickupCount;

        public float HealPickupHealth => _healPickupHealth;

        public float HealPickupRadius => _healPickupRadius;

        public float HealPickupBobAmplitude => _healPickupBobAmplitude;

        public float HealPickupBobSpeed => _healPickupBobSpeed;

        public float HealPickupSpinDegreesPerSecond => _healPickupSpinDegreesPerSecond;

        public float EnemyMinSpawnDistance => _enemyMinSpawnDistance;

        public float EnemyMinSpacing => _enemyMinSpacing;

        public float PickupMinSpawnDistance => _pickupMinSpawnDistance;

        public ModuleCatalogEntry[] ModuleCatalog => _moduleCatalog;

        public Material WallMaterial => _wallMaterial;

        public Material FloorMaterial => _floorMaterial;

        public Material CeilingMaterial => _ceilingMaterial;

        public Material ExitCorridorMaterial => _exitCorridorMaterial;

        public float ConnectorLength => _connectorLength;

        public float CorridorWidth => _corridorWidth;

        public float CorridorHeight => _corridorHeight;

        public float FloorThickness => _floorThickness;

        public float RoomDoorWidth => _roomDoorWidth;

        public float RoomDoorHeight => _roomDoorHeight;

        public float GateOpeningWidth => _gateOpeningWidth;

        public float RoomHeight => _roomHeight;
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
