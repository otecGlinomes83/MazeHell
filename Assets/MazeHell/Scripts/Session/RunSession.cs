using System;
using Unity.AI.Navigation;
using MazeHell.Audio;
using MazeHell.Configuration;
using MazeHell.Enemies;
using MazeHell.Level;
using MazeHell.Player;
using MazeHell.UI;
using UnityEngine;

namespace MazeHell.Session
{
    public class RunSession
    {
        public event Action<GeneratedLevel> PlayerLevelStart;

        public event Action<RunSession> Died;

        private const string DoorOpenCueId = "door_open";
        private const string GateOpenCueId = "gate_open";
        private const float CorridorLateralTolerance = 1f;
        private const float FadeAlphaEpsilon = 0.001f;

        private LevelGeometryFactory _levelFactory;
        private EnemyFactory _enemyFactory;
        private PickupFactory _pickupFactory;
        private PlayerRig _player;
        private HudView _hud;
        private HudPresenter _presenter;
        private GameAudio _audio;
        private Localization _localization;
        private LevelGenerationConfig _generation;
        private LevelPresentationConfig _presentation;
        private Transform _levelParent;

        private EnemyRegistry _registry;
        private System.Random _seedRandom;
        private GeneratedLevel _currentLevel;
        private MazeLayout _pendingLayout;
        private ExitGate _exitGate;
        private EnemyRig[] _enemies;
        private RunState _state;
        private int _levelNumber;
        private int _kills;
        private int _pendingSeed;
        private float _runTime;
        private float _transitionProgress;
        private float _generationWait;
        private bool _isStarted;

        public RunState State => _state;

        public int LevelNumber => _levelNumber;

        public int Kills => _kills;

        public float RunTime => _runTime;

        public GeneratedLevel CurrentLevel => _currentLevel;

        public EnemyRig[] Enemies => _enemies;

        public float TransitionProgress => _transitionProgress;

        public RunSession(LevelGeometryFactory levelFactory, EnemyFactory enemyFactory, PickupFactory pickupFactory,
            PlayerRig player, HudView hud, HudPresenter presenter, GameAudio audio, Localization localization,
            LevelGenerationConfig generation, LevelPresentationConfig presentation, Transform levelParent)
        {
            if (levelFactory == null)
            {
                throw new ArgumentNullException(nameof(levelFactory), "Level geometry factory is required.");
            }

            if (enemyFactory == null)
            {
                throw new ArgumentNullException(nameof(enemyFactory), "Enemy factory is required.");
            }

            if (pickupFactory == null)
            {
                throw new ArgumentNullException(nameof(pickupFactory), "Pickup factory is required.");
            }

            if (player == null)
            {
                throw new ArgumentNullException(nameof(player), "Player rig is required.");
            }

            if (hud == null)
            {
                throw new ArgumentNullException(nameof(hud), "Hud view is required.");
            }

            if (hud.ScreenFade == null)
            {
                throw new ArgumentNullException(nameof(hud), "Hud view has no screen fade binding.");
            }

            if (presenter == null)
            {
                throw new ArgumentNullException(nameof(presenter), "Hud presenter is required.");
            }

            if (audio == null)
            {
                throw new ArgumentNullException(nameof(audio), "Game audio is required.");
            }

            if (localization == null)
            {
                throw new ArgumentNullException(nameof(localization), "Localization is required.");
            }

            if (generation == null)
            {
                throw new ArgumentNullException(nameof(generation), "Level generation config is required.");
            }

            if (presentation == null)
            {
                throw new ArgumentNullException(nameof(presentation), "Level presentation config is required.");
            }

            if (levelParent == null)
            {
                throw new ArgumentNullException(nameof(levelParent), "Level parent is required.");
            }

            _levelFactory = levelFactory;
            _enemyFactory = enemyFactory;
            _pickupFactory = pickupFactory;
            _player = player;
            _hud = hud;
            _presenter = presenter;
            _audio = audio;
            _localization = localization;
            _generation = generation;
            _presentation = presentation;
            _levelParent = levelParent;
            _registry = new EnemyRegistry();
            _enemies = new EnemyRig[0];
            _state = RunState.Generating;

            _registry.CountChanged += OnRegistryCountChanged;
            _registry.EnemyKilled += OnEnemyKilled;
        }

        public void Start(int levelNumber, int runSeed)
        {
            if (_isStarted)
            {
                throw new InvalidOperationException($"{nameof(RunSession)}: {nameof(Start)} must be called once.");
            }

            if (levelNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(levelNumber), levelNumber, "Level number starts at 1.");
            }

            _seedRandom = new System.Random(runSeed);
            BuildLevel(levelNumber, _seedRandom.Next());
            _state = RunState.Fighting;
            _isStarted = true;
        }

        public void Tick(float deltaTime)
        {
            AssertStarted();
            _currentLevel.TickDoors(_player.transform.position, _player.Motor.Velocity,
                _player.Motor.BodyHeight, deltaTime);

            if (_state == RunState.Dead)
            {
                return;
            }

            if (_state == RunState.Generating)
            {
                _generationWait -= deltaTime;

                if (_generationWait <= 0f)
                {
                    CompleteGeneration();
                }

                return;
            }

            _runTime += deltaTime;

            if (_player.Health.IsDead)
            {
                EnterDead();
                return;
            }

            _audio.Heartbeat(HealthFraction(), deltaTime);

            if (_state == RunState.Fighting)
            {
                UpdateFighting(deltaTime);
                return;
            }

            UpdateTransition(deltaTime);
        }

        private void UpdateFighting(float deltaTime)
        {
            if (_exitGate.IsUnlocked && IsPlayerInDarkCorridor())
            {
                EnterTransition();
            }
        }

        private void UpdateTransition(float deltaTime)
        {
            _transitionProgress = ComputeCorridorProgress();
            _audio.SetTransition(_transitionProgress);
            float remainingDistance = Vector3.Distance(_currentLevel.CorridorStart, _currentLevel.CorridorEnd) * (1f - _transitionProgress);
            float fadeDistance = Mathf.Max(0.01f, _player.Motor.HorizontalSpeed * _presentation.ScreenFadeDuration);
            ApplyFade(1f - Mathf.Clamp01(remainingDistance / fadeDistance));

            if (_transitionProgress >= 1f)
            {
                EnterGenerating();
            }
        }

        private void EnterTransition()
        {
            _state = RunState.Transition;
            _transitionProgress = 0f;
            _pendingSeed = _seedRandom.Next();
            PrecomputeNextLevel();
        }

        private void EnterGenerating()
        {
            _state = RunState.Generating;
            _generationWait = _presentation.ScreenFadeDuration;
            ApplyFade(1f);
        }

        private void CompleteGeneration()
        {
            _audio.SetTransition(0f);
            DestroyCurrentLevel();
            BuildLevel(_levelNumber + 1, _pendingSeed);
            _transitionProgress = 0f;
            ApplyFade(0f);
            _state = RunState.Fighting;
        }

        private void EnterDead()
        {
            _state = RunState.Dead;
            _audio.SetTransition(0f);
            ApplyFade(0f);
            Died?.Invoke(this);
        }

        private void BuildLevel(int levelNumber, int seed)
        {
            _levelNumber = levelNumber;
            _currentLevel = _levelFactory.Build(levelNumber, seed, _levelParent, _pendingLayout);
            _pendingLayout = null;
            _exitGate = _currentLevel.ExitGate;
            _exitGate.Opened += OnExitGateOpened;

            foreach (ProximityDoor door in _currentLevel.Root.GetComponentsInChildren<ProximityDoor>())
            {
                door.Opening += OnDoorOpening;
            }

            foreach (DecorationBillboard decoration in _currentLevel.GeometryRoot.GetComponentsInChildren<DecorationBillboard>())
            {
                decoration.Setup(_player.Camera);
            }
            _presenter.ShowMap(_currentLevel.Map);
            _presenter.ShowLevel(levelNumber, _localization, _presentation);

            SpawnEnemies();
            SpawnPickups();

            PlayerLevelStart?.Invoke(_currentLevel);
        }

        private void SpawnEnemies()
        {
            _registry.Setup();
            _registry.BeginSpawning();

            Vector3[] spawnPositions = _currentLevel.EnemySpawnPositions;
            EnemyRig[] enemies = new EnemyRig[spawnPositions.Length];

            for (int spawnIndex = 0; spawnIndex < spawnPositions.Length; spawnIndex++)
            {
                EnemyRig enemy = _enemyFactory.Create(spawnPositions[spawnIndex], Quaternion.identity, _levelNumber, _currentLevel.Root.transform, _player);
                enemies[spawnIndex] = enemy;
                _registry.Register(enemy.Health, _enemyFactory.HealOnKill);
            }

            _enemies = enemies;
            _registry.FinishSpawning();
        }

        private void SpawnPickups()
        {
            Vector3[] pickupPositions = _currentLevel.PickupPositions;

            for (int pickupIndex = 0; pickupIndex < pickupPositions.Length; pickupIndex++)
            {
                _pickupFactory.Create(pickupPositions[pickupIndex], _currentLevel.Root.transform, _player.Health);
            }
        }

        private void DestroyCurrentLevel()
        {
            if (_currentLevel == null)
            {
                return;
            }

            _currentLevel.NavMeshSurface.RemoveData();
            _exitGate.Opened -= OnExitGateOpened;

            foreach (ProximityDoor door in _currentLevel.Root.GetComponentsInChildren<ProximityDoor>())
            {
                door.Opening -= OnDoorOpening;
            }
            _currentLevel.Root.SetActive(false);
            UnityEngine.Object.Destroy(_currentLevel.Root);
            _currentLevel = null;
            _exitGate = null;
            _enemies = new EnemyRig[0];
        }

        public void Dispose()
        {
            _registry.CountChanged -= OnRegistryCountChanged;
            _registry.EnemyKilled -= OnEnemyKilled;
            _registry.Clear();
            DestroyCurrentLevel();
        }

        private void PrecomputeNextLevel()
        {
            _pendingLayout = _levelFactory.PrepareLayout(_levelNumber + 1, _pendingSeed);
        }

        private bool IsPlayerInDarkCorridor()
        {
            Vector3 offset = _player.transform.position - _currentLevel.CorridorStart;
            float alongDistance = Vector3.Dot(offset, _currentLevel.CorridorDirection);

            if (alongDistance < 0f)
            {
                return false;
            }

            Vector3 lateral = offset - _currentLevel.CorridorDirection * alongDistance;
            float lateralHalfWidth = _currentLevel.CorridorHalfWidth + CorridorLateralTolerance;

            return lateral.sqrMagnitude <= lateralHalfWidth * lateralHalfWidth;
        }

        private float ComputeCorridorProgress()
        {
            Vector3 corridorSpan = _currentLevel.CorridorEnd - _currentLevel.CorridorStart;
            float totalLength = Vector3.Dot(corridorSpan, _currentLevel.CorridorDirection);

            if (totalLength <= 0f)
            {
                return 1f;
            }

            Vector3 offset = _player.transform.position - _currentLevel.CorridorStart;
            float alongDistance = Vector3.Dot(offset, _currentLevel.CorridorDirection);

            return Mathf.Clamp01(alongDistance / totalLength);
        }

        private float HealthFraction()
        {
            float maxHealth = _player.Health.MaxHealth;

            if (maxHealth <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(_player.Health.CurrentHealth / maxHealth);
        }

        private void ApplyFade(float progress)
        {
            Color fadeColor = Color.black;
            fadeColor.a = Mathf.SmoothStep(0f, 1f, progress);
            _hud.ScreenFade.color = fadeColor;
            _hud.ScreenFade.gameObject.SetActive(fadeColor.a > FadeAlphaEpsilon);
        }

        private void OnRegistryCountChanged(int remainingCount)
        {
            _exitGate.SetRemainingEnemies(remainingCount);
            _presenter.SetEnemyCount(remainingCount);
        }

        private void OnDoorOpening(ProximityDoor door)
        {
            _audio.Play(DoorOpenCueId, door.transform.position);
        }

        private void OnExitGateOpened(ExitGate gate)
        {
            _audio.Play(GateOpenCueId, Vector3.zero);
            _player.CameraMotion.ReactGateOpen();
        }

        private void OnEnemyKilled(float healOnKill)
        {
            _kills += 1;
            _player.Health.Heal(healOnKill);
        }

        private void AssertStarted()
        {
            if (_isStarted == false)
            {
                throw new InvalidOperationException($"{nameof(RunSession)}: call {nameof(Start)} before {nameof(Tick)}.");
            }
        }
    }
}
