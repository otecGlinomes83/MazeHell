using System;
using MazeHell.Audio;
using MazeHell.Combat;
using MazeHell.Configuration;
using MazeHell.Enemies;
using MazeHell.Input;
using MazeHell.Level;
using MazeHell.Player;
using MazeHell.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MazeHell.Session
{
    [DisallowMultipleComponent]
    public class GameComposition : MonoBehaviour
    {
        private const string UiClickCueId = "ui_click";

        [Header("Configurations")]
        [SerializeField] private PlayerMovementConfig _movementConfig;
        [SerializeField] private PlayerCombatConfig _combatConfig;
        [SerializeField] private ShotgunConfig _shotgunConfig;
        [SerializeField] private LevelGenerationConfig _generationConfig;
        [SerializeField] private LevelPresentationConfig _presentationConfig;
        [SerializeField] private CameraMotionConfig _cameraConfig;
        [SerializeField] private HudMotionConfig _hudConfig;
        [SerializeField] private AnimationLibrary _animations;
        [SerializeField] private AudioConfig _audioConfig;
        [SerializeField] private InterfaceConfig _interfaceConfig;
        [SerializeField] private LocalizationConfig _localizationConfig;

        [Header("Prefabs")]
        [SerializeField] private PlayerRig _playerPrefab;
        [SerializeField] private EnemyRig _enemyPrefab;
        [SerializeField] private HudView _hudPrefab;
        [SerializeField] private GameObject _healingPickupPrefab;

        [Header("Scene")]
        [SerializeField] private Transform _worldRoot;
        [SerializeField] private DesktopInput _input;
        [SerializeField] private Pauser _pauser;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource[] _sfxSources;

        private RecordStorage _records;
        private Localization _localization;
        private GameAudio _audio;
        private ShotNoise _noise;
        private LevelGeometryFactory _levelFactory;
        private PlayerFactory _playerFactory;
        private HudFactory _hudFactory;
        private EnemyFactory _enemyFactory;
        private PickupFactory _pickupFactory;
        private PlayerRig _player;
        private HudView _hud;
        private HudScreens _screens;
        private RunSession _session;
        private GameLoop _gameLoop;
        private float _sensitivity;
        private bool _isSubscribed;
        private bool _skipGameplayInput;

        public PlayerRig Player => _player;

        public RunSession Session => _session;

        public HudView Hud => _hud;

        private void Awake()
        {
            ValidateReferences();
            RenderSettings.fog = _presentationConfig.FogDensity > 0f;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = _presentationConfig.FogColor;
            RenderSettings.fogDensity = _presentationConfig.FogDensity;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = _presentationConfig.AmbientLightColor;
            UnityEngine.Rendering.SphericalHarmonicsL2 ambientProbe = new UnityEngine.Rendering.SphericalHarmonicsL2();
            ambientProbe.AddAmbientLight(_presentationConfig.AmbientLightColor);
            RenderSettings.ambientProbe = ambientProbe;
            RenderSettings.skybox = null;

            _records = new RecordStorage(_audioConfig.MasterVolume, _cameraConfig.MouseSensitivity);
            _localization = new Localization();
            _localization.Setup(_localizationConfig, _records.Language);

            _audio = gameObject.AddComponent<GameAudio>();
            _audio.Setup(_audioConfig, _musicSource, _sfxSources);
            _audio.SetVolume(_records.Volume);
            _audio.SetMusicVolume(_records.MusicVolume);
            _audio.SetSfxVolume(_records.SfxVolume);
            _audio.StartMusic();

            _noise = new ShotNoise();

            _levelFactory = new LevelGeometryFactory();
            _levelFactory.Setup(_generationConfig, _presentationConfig);

            _playerFactory = new PlayerFactory(_playerPrefab, _movementConfig, _combatConfig, _shotgunConfig, _cameraConfig, _audio, _noise);
            _hudFactory = new HudFactory(_hudPrefab, _interfaceConfig, _animations, _hudConfig, _movementConfig, _shotgunConfig, _cameraConfig);
            _enemyFactory = new EnemyFactory(_enemyPrefab, _audio, _audioConfig, _animations, _noise);
            _pickupFactory = new PickupFactory(_healingPickupPrefab, _generationConfig, _audio);

            _player = _playerFactory.Create(_worldRoot.position, Quaternion.identity, _worldRoot);
            _hud = _hudFactory.Create(_player, _localization, _records, _worldRoot);
            _screens = _hud.Screens;
            _sensitivity = Mathf.Clamp(_records.MouseSensitivity, _cameraConfig.MinMouseSensitivity, _cameraConfig.MaxMouseSensitivity);

            HudPresenter presenter = _hud.Presenter;
            _session = new RunSession(_levelFactory, _enemyFactory, _pickupFactory, _player, _hud, presenter, _audio, _localization, _generationConfig, _presentationConfig, _worldRoot);
            _gameLoop = new GameLoop(_player, presenter);

            Subscribe();
            _session.Start(1, ResolveRunSeed());

            if (_screens.IsNarrativeOpen)
            {
                _pauser.RequestPause();
            }

            SetCursorLocked(_pauser.IsPaused == false);
        }

        private void Update()
        {
            _audio.Tick(Time.unscaledDeltaTime);

            if (_session.State == RunState.Dead)
            {
                return;
            }

            if (_screens.IsNarrativeOpen)
            {
                _hud.Presenter.LateTick(Time.unscaledDeltaTime);

                if (_input.IsAnyKeyPressed())
                {
                    _screens.DismissMonologue();
                }

                return;
            }

            if (_skipGameplayInput)
            {
                _skipGameplayInput = false;
                return;
            }

            InputSnapshot input = _input.Poll();

            if (input.PausePressed)
            {
                if (_pauser.IsPaused && _screens.TryCloseTopPopup())
                {
                    return;
                }

                TogglePause();
                return;
            }

            if (_pauser.IsPaused)
            {
                return;
            }

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                _pauser.RequestPause();
                return;
            }

            float deltaTime = Time.deltaTime;
            _session.Tick(deltaTime);

            if (_session.State == RunState.Dead || _session.State == RunState.Generating)
            {
                return;
            }

            _gameLoop.Tick(input, deltaTime, _session.Enemies);
        }

        private void LateUpdate()
        {
            if (_pauser.IsPaused || _session.State == RunState.Dead)
            {
                return;
            }

            _gameLoop.LateTick(Time.deltaTime);
        }

        private void OnDisable()
        {
            SetCursorLocked(false);
            Unsubscribe();

            if (_session != null)
            {
                _session.Dispose();
            }
        }

        private void Subscribe()
        {
            if (_isSubscribed)
            {
                return;
            }

            _session.PlayerLevelStart += OnPlayerLevelStart;
            _session.Died += OnSessionDied;
            _screens.ResumeRequested += OnResumeRequested;
            _screens.RestartRequested += OnRestartRequested;
            _screens.MenuRequested += OnMenuRequested;
            _screens.VolumeChanged += OnVolumeChanged;
            _screens.MusicVolumeChanged += OnMusicVolumeChanged;
            _screens.SfxVolumeChanged += OnSfxVolumeChanged;
            _screens.SensitivityChanged += OnSensitivityChanged;
            _screens.NarrativeCompleted += OnNarrativeCompleted;
            _pauser.Changed += OnPauseChanged;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (_isSubscribed == false)
            {
                return;
            }

            _session.PlayerLevelStart -= OnPlayerLevelStart;
            _session.Died -= OnSessionDied;
            _screens.ResumeRequested -= OnResumeRequested;
            _screens.RestartRequested -= OnRestartRequested;
            _screens.MenuRequested -= OnMenuRequested;
            _screens.VolumeChanged -= OnVolumeChanged;
            _screens.MusicVolumeChanged -= OnMusicVolumeChanged;
            _screens.SfxVolumeChanged -= OnSfxVolumeChanged;
            _screens.SensitivityChanged -= OnSensitivityChanged;
            _screens.NarrativeCompleted -= OnNarrativeCompleted;
            _pauser.Changed -= OnPauseChanged;
            _isSubscribed = false;
        }

        private void TogglePause()
        {
            _audio.Play(UiClickCueId, Vector3.zero);

            if (_pauser.IsPaused)
            {
                _pauser.Resume();
                return;
            }

            _pauser.RequestPause();
        }

        private int ResolveRunSeed()
        {
            if (_generationConfig.UseFixedSeed)
            {
                return _generationConfig.BaseSeed;
            }

            return new System.Random(Environment.TickCount).Next();
        }

        private void OnPlayerLevelStart(GeneratedLevel level)
        {
            _player.Motor.Teleport(level.SpawnPosition, level.SpawnRotation);
            _playerFactory.RebindLook(_player, _sensitivity);
            _player.Feedback.ResetTravel();
            _player.CameraMotion.ResetMotion();
            _hud.Spring.ResetState();
        }

        private void OnSessionDied(RunSession session)
        {
            _audio.Heartbeat(0f, 0f);
            _records.TrySaveRecord(session.LevelNumber, session.Kills, session.RunTime);
            SetCursorLocked(false);
            _pauser.RequestPause();
            _screens.ShowPause(false);
            _screens.ShowDeath(session.LevelNumber, session.Kills, session.RunTime);
        }

        private void OnNarrativeCompleted(HudScreens screens)
        {
            _pauser.Resume();
        }

        private void OnResumeRequested(HudScreens screens)
        {
            _pauser.Resume();
        }

        private void OnRestartRequested(HudScreens screens)
        {
            _pauser.Resume();
            SceneManager.LoadScene(_interfaceConfig.GameSceneName);
        }

        private void OnMenuRequested(HudScreens screens)
        {
            _pauser.Resume();
            SceneManager.LoadScene(_interfaceConfig.MenuSceneName);
        }

        private void OnVolumeChanged(float volume)
        {
            _audio.SetVolume(volume);
        }

        private void OnMusicVolumeChanged(float volume)
        {
            _audio.SetMusicVolume(volume);
        }

        private void OnSfxVolumeChanged(float volume)
        {
            _audio.SetSfxVolume(volume);
        }

        private void OnSensitivityChanged(float sensitivity)
        {
            _sensitivity = sensitivity;
            _playerFactory.RebindLook(_player, _sensitivity);
        }

        private void OnPauseChanged(bool isPaused)
        {
            bool isDead = _session.State == RunState.Dead;
            _screens.ShowPause(isPaused && isDead == false && _screens.IsNarrativeOpen == false);
            SetCursorLocked(isPaused == false && isDead == false);

            if (isPaused == false && isDead == false)
            {
                _skipGameplayInput = true;
            }
        }

        private void SetCursorLocked(bool isLocked)
        {
            if (isLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                return;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ValidateReferences()
        {
            AssertReference(_movementConfig, nameof(_movementConfig));
            AssertReference(_combatConfig, nameof(_combatConfig));
            AssertReference(_shotgunConfig, nameof(_shotgunConfig));
            AssertReference(_generationConfig, nameof(_generationConfig));
            AssertReference(_presentationConfig, nameof(_presentationConfig));
            AssertReference(_cameraConfig, nameof(_cameraConfig));
            AssertReference(_hudConfig, nameof(_hudConfig));
            AssertReference(_animations, nameof(_animations));
            AssertReference(_audioConfig, nameof(_audioConfig));
            AssertReference(_interfaceConfig, nameof(_interfaceConfig));
            AssertReference(_localizationConfig, nameof(_localizationConfig));
            AssertReference(_playerPrefab, nameof(_playerPrefab));
            AssertReference(_enemyPrefab, nameof(_enemyPrefab));
            AssertReference(_hudPrefab, nameof(_hudPrefab));
            AssertReference(_healingPickupPrefab, nameof(_healingPickupPrefab));
            AssertReference(_worldRoot, nameof(_worldRoot));
            AssertReference(_input, nameof(_input));
            AssertReference(_pauser, nameof(_pauser));
            AssertReference(_musicSource, nameof(_musicSource));

            if (_sfxSources == null || _sfxSources.Length == 0)
            {
                throw new InvalidOperationException($"{name}: {nameof(_sfxSources)} pool is empty.");
            }
        }

        private void AssertReference(UnityEngine.Object reference, string fieldName)
        {
            if (reference == null)
            {
                throw new InvalidOperationException($"{name}: serialized reference '{fieldName}' is not assigned.");
            }
        }
    }
}
