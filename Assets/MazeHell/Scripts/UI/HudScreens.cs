using System;
using MazeHell.Configuration;
using MazeHell.Session;
using UnityEngine;
using UnityEngine.UI;

namespace MazeHell.UI
{
    [DisallowMultipleComponent]
    public class HudScreens : MonoBehaviour
    {
        private HudView _view;
        private Localization _localization;
        private RecordStorage _records;
        private InterfaceConfig _interface;
        private CameraMotionConfig _camera;
        private UiSpawner _spawner;
        private MenuPopupStack _popups;
        private PauseWindow _pause;
        private DeathWindow _death;
        private SettingsWindow _settings;
        private NarrativePopup _narrative;
        private float _narrativeRemaining;
        private float _sensitivity;
        private bool _soundOn;
        private bool _resumeAfterClose;

        public event Action<HudScreens> ResumeRequested;
        public event Action<HudScreens> RestartRequested;
        public event Action<HudScreens> MenuRequested;
        public event Action<float> VolumeChanged;
        public event Action<float> SensitivityChanged;
        public PauseWindow ActivePause => _pause;
        public DeathWindow ActiveDeath => _death;
        public SettingsWindow ActiveSettings => _settings;

        public void Setup(HudView view, Localization localization, RecordStorage records, InterfaceConfig ui, CameraMotionConfig camera)
        {
            if (view == null || localization == null || records == null || ui == null || camera == null)
            {
                throw new ArgumentNullException(nameof(view), "HUD screen dependencies must be assigned.");
            }

            _view = view;
            _localization = localization;
            _records = records;
            _interface = ui;
            _camera = camera;
            _soundOn = records.Volume > 0f;
            _sensitivity = Mathf.Clamp(records.MouseSensitivity, camera.MinMouseSensitivity, camera.MaxMouseSensitivity);
            _spawner = new UiSpawner(view.PopupRoot);
            _popups = new MenuPopupStack(view.HudGroup);
        }

        public void ShowPause(bool visible)
        {
            if (visible == false)
            {
                ReleaseSettings();
                ReleasePause();
                return;
            }

            if (_pause != null || _death != null)
            {
                return;
            }

            _pause = _spawner.Spawn(_view.PauseWindowPrefab);
            _pause.Setup(_localization);
            SetupButton(_pause.Resume);
            SetupButton(_pause.Restart);
            SetupButton(_pause.Menu);
            SetupButton(_pause.Settings);
            _pause.Resume.Clicked += OnResume;
            _pause.Restart.Clicked += OnRestart;
            _pause.Menu.Clicked += OnMenu;
            _pause.Settings.Clicked += OnSettings;
            _pause.CloseRequested += OnPauseCloseRequested;
            _pause.Closed += OnPauseClosed;
            _pause.Show();
            _popups.Push(_pause);
        }

        public void ShowDeath(int level, int kills, float runTime)
        {
            ReleaseSettings();
            ReleasePause();
            ReleaseNarrative();
            _view.HudPanel.SetActive(false);
            _death = _spawner.Spawn(_view.DeathWindowPrefab);
            string result = string.Format(_localization.Get("death_level_format"), level) + "\n" +
                string.Format(_localization.Get("death_kills_format"), kills) + "\n" +
                string.Format(_localization.Get("death_time_format"), runTime.ToString("0"));
            _death.Setup(_localization, result);
            SetupButton(_death.Retry);
            SetupButton(_death.Menu);
            _death.Retry.Clicked += OnRestart;
            _death.Menu.Clicked += OnMenu;
            _death.Show();
            _popups.Push(_death);
        }

        public void ShowMonologue(string text, float duration)
        {
            ReleaseNarrative();

            if (duration <= 0f)
            {
                return;
            }

            _narrative = _spawner.Spawn(_view.NarrativePrefab);
            _narrative.Setup(text);
            _narrativeRemaining = duration;
            _narrative.Closed += OnNarrativeClosed;
            _narrative.Show();
        }

        public void Tick(float deltaTime)
        {
            if (_narrative == null)
            {
                return;
            }

            _narrativeRemaining -= deltaTime;

            if (_narrativeRemaining <= 0f)
            {
                _narrative.BeginClose();
            }
        }

        public bool TryCloseTopPopup()
        {
            if (_settings == null)
            {
                return false;
            }

            _settings.BeginClose();
            return true;
        }

        private void OnResume(MenuClick button)
        {
            _resumeAfterClose = true;
            _pause.BeginClose();
        }

        private void OnPauseCloseRequested(BaseWindow window)
        {
            _resumeAfterClose = true;
            window.BeginClose();
        }

        private void OnPauseClosed(BaseWindow window)
        {
            bool resume = _resumeAfterClose;
            ReleasePause();

            if (resume)
            {
                ResumeRequested?.Invoke(this);
            }
        }

        private void OnRestart(MenuClick button)
        {
            RestartRequested?.Invoke(this);
        }

        private void OnMenu(MenuClick button)
        {
            MenuRequested?.Invoke(this);
        }

        private void OnSettings(MenuClick button)
        {
            if (_settings != null)
            {
                return;
            }

            _settings = _spawner.Spawn(_view.SettingsWindowPrefab);
            SetupButton(_settings.Close);
            SetupButton(_settings.Sound);
            SetupButton(_settings.Decrease);
            SetupButton(_settings.Increase);
            _settings.Close.Clicked += OnSettingsClose;
            _settings.Sound.Clicked += OnSound;
            _settings.Decrease.Clicked += OnDecrease;
            _settings.Increase.Clicked += OnIncrease;
            _settings.Closed += OnSettingsClosed;
            _settings.CloseRequested += OnSettingsCloseRequested;
            _settings.Refresh(_localization, _soundOn, _sensitivity);
            _settings.Show();
            _popups.Push(_settings);
        }

        private void OnSettingsClose(MenuClick button)
        {
            _settings.BeginClose();
        }

        private void OnSettingsCloseRequested(BaseWindow window)
        {
            window.BeginClose();
        }

        private void OnSettingsClosed(BaseWindow window)
        {
            ReleaseSettings();
        }

        private void OnSound(MenuClick button)
        {
            float volume = 0f;

            if (_soundOn == false)
            {
                volume = _interface.DefaultVolume;
            }

            _soundOn = volume > 0f;
            _records.SetVolume(volume);
            VolumeChanged?.Invoke(volume);
            _settings.Refresh(_localization, _soundOn, _sensitivity);
        }

        private void OnDecrease(MenuClick button)
        {
            ChangeSensitivity(-_interface.SensitivityStep);
        }

        private void OnIncrease(MenuClick button)
        {
            ChangeSensitivity(_interface.SensitivityStep);
        }

        private void ChangeSensitivity(float delta)
        {
            _sensitivity = Mathf.Clamp(_sensitivity + delta, _camera.MinMouseSensitivity, _camera.MaxMouseSensitivity);
            _records.SetMouseSensitivity(_sensitivity);
            SensitivityChanged?.Invoke(_sensitivity);
            _settings.Refresh(_localization, _soundOn, _sensitivity);
        }

        private void SetupButton(MenuClick button)
        {
            if (button == null || button.TryGetComponent(out Graphic graphic) == false)
            {
                throw new InvalidOperationException("Window prefab has an unassigned button graphic.");
            }

            button.Setup(graphic, _interface.ButtonNormalColor, _interface.ButtonHoverColor);
        }

        private void ReleasePause()
        {
            if (_pause == null)
            {
                return;
            }

            _pause.Resume.Clicked -= OnResume;
            _pause.Restart.Clicked -= OnRestart;
            _pause.Menu.Clicked -= OnMenu;
            _pause.Settings.Clicked -= OnSettings;
            _pause.CloseRequested -= OnPauseCloseRequested;
            _pause.Closed -= OnPauseClosed;
            _popups.Pop(_pause);
            _spawner.Release(_pause);
            _pause = null;
            _resumeAfterClose = false;
        }

        private void ReleaseSettings()
        {
            if (_settings == null)
            {
                return;
            }

            _settings.Close.Clicked -= OnSettingsClose;
            _settings.Sound.Clicked -= OnSound;
            _settings.Decrease.Clicked -= OnDecrease;
            _settings.Increase.Clicked -= OnIncrease;
            _settings.Closed -= OnSettingsClosed;
            _settings.CloseRequested -= OnSettingsCloseRequested;
            _popups.Pop(_settings);
            _spawner.Release(_settings);
            _settings = null;
        }

        private void OnNarrativeClosed(BaseWindow window)
        {
            ReleaseNarrative();
        }

        private void ReleaseNarrative()
        {
            if (_narrative == null)
            {
                return;
            }

            _narrative.Closed -= OnNarrativeClosed;
            _spawner.Release(_narrative);
            _narrative = null;
        }

        private void OnDisable()
        {
            ReleaseSettings();
            ReleasePause();
            ReleaseNarrative();

            if (_death != null)
            {
                _death.Retry.Clicked -= OnRestart;
                _death.Menu.Clicked -= OnMenu;
                _popups.Pop(_death);
                _spawner.Release(_death);
                _death = null;
            }
        }
    }
}