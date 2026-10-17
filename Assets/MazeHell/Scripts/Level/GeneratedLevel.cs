using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace MazeHell.Level
{
    public class GeneratedLevel
    {
        private GameObject _root;
        private Transform _geometryRoot;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private MazeLayout _layout;
        private ExitGate _exitGate;
        private Vector3 _corridorStart;
        private Vector3 _corridorEnd;
        private Vector3 _corridorDirection;
        private float _corridorHalfWidth;
        private Vector3[] _enemySpawnPositions;
        private Vector3[] _pickupPositions;
        private NavMeshSurface _navMeshSurface;
        private ProximityDoor[] _doors;

        public GeneratedLevel(GameObject root, Transform geometryRoot, Vector3 spawnPosition, Quaternion spawnRotation, MazeLayout layout, ExitGate exitGate, Vector3 corridorStart, Vector3 corridorEnd, Vector3 corridorDirection, float corridorHalfWidth, Vector3[] enemySpawnPositions, Vector3[] pickupPositions, NavMeshSurface navMeshSurface, ProximityDoor[] doors)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (geometryRoot == null)
            {
                throw new ArgumentNullException(nameof(geometryRoot));
            }

            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            if (exitGate == null)
            {
                throw new ArgumentNullException(nameof(exitGate));
            }

            if (enemySpawnPositions == null)
            {
                throw new ArgumentNullException(nameof(enemySpawnPositions));
            }

            if (pickupPositions == null)
            {
                throw new ArgumentNullException(nameof(pickupPositions));
            }

            if (navMeshSurface == null)
            {
                throw new ArgumentNullException(nameof(navMeshSurface));
            }

            _root = root;
            _geometryRoot = geometryRoot;
            _spawnPosition = spawnPosition;
            _spawnRotation = spawnRotation;
            _layout = layout;
            _exitGate = exitGate;
            _corridorStart = corridorStart;
            _corridorEnd = corridorEnd;
            _corridorDirection = corridorDirection;
            _corridorHalfWidth = corridorHalfWidth;
            _enemySpawnPositions = enemySpawnPositions;
            _pickupPositions = pickupPositions;
            _navMeshSurface = navMeshSurface;
            _doors = doors;
        }

        public void TickDoors(Vector3 position, Vector3 velocity, float bodyHeight, float deltaTime)
        {
            for (int doorIndex = 0; doorIndex < _doors.Length; doorIndex++)
            {
                _doors[doorIndex].Tick(position, velocity, bodyHeight, deltaTime);
            }

            _exitGate.TickPlayer(position, velocity, bodyHeight, deltaTime);
        }

        public GameObject Root => _root;

        public Transform GeometryRoot => _geometryRoot;

        public Vector3 SpawnPosition => _spawnPosition;

        public Quaternion SpawnRotation => _spawnRotation;

        public MazeLayout Layout => _layout;

        public ExitGate ExitGate => _exitGate;

        public Vector3 CorridorStart => _corridorStart;

        public Vector3 CorridorEnd => _corridorEnd;

        public Vector3 CorridorDirection => _corridorDirection;

        public float CorridorHalfWidth => _corridorHalfWidth;

        public Vector3[] EnemySpawnPositions => _enemySpawnPositions;

        public Vector3[] PickupPositions => _pickupPositions;

        public NavMeshSurface NavMeshSurface => _navMeshSurface;
    }
}
