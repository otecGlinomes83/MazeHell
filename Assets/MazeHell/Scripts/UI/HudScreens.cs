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
        private const string PauseTitleKey = "pause_title";

        private const string ResumeKey = "pause_resume";

        private const string RestartKey = "pause_restart";

        private const string PauseMenuKey = "pause_menu";

        private const string DeathTitleKey = "death_title";

        private const string RetryKey = "death_retry";

        private const string DeathMenuKey = "death_menu";

        private const string SoundOnKey = "sound_on";

        private const string SoundOffKey = "sound_off";

        private const string SensitivityFormatKey = "hud_sensitivity_format";

        private const string DeathLevelFormatKey = "death_level_format";

        private const string DeathKillsFormatKey = "death_kills_format";

        private const string DeathTimeFormatKey = "death_time_format";

        private const string DeathBestFormatKey = "death_best_format";

        private const string SecondsFormat = "0";

        private const string LineSeparator = "\n";

        [SerializeField] private HudView _view;

        private Localization _localization;

        private RecordStorage _records;

        private InterfaceConfig _interface;

        private CameraMotionConfig _cameraConfig;

        private float _sensitivity;

        private bool _isSoundOn;

        private bool _isSetupFinished;

        public event Action<HudScreens> ResumeRequested;

        public event Action<HudScreens> RestartRequested;

        public event Action<HudScreens> MenuRequested;

        public event Action<float> VolumeChanged;

        public event Action<float> SensitivityChanged;

        public void Setup(HudView view, Localization localization, RecordStorage records, InterfaceConfig ui,
            CameraMotionConfig camera)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (localization == null)
            {
                throw new ArgumentNullException(nameof(localization));
            }

            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            if (ui == null)
            {
                throw new ArgumentNullException(nameof(ui));
            }

            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            if (view.HudPanel == null || view.PausePanel == null || view.DeathPanel == null ||
                view.PauseTitle == null || view.DeathTitle == null || view.ResumeText == null ||
                view.RestartText == null || view.PauseMenuText == null || view.SoundText == null ||
                view.SensitivityText == null || view.RetryText == null || view.DeathMenuText == null ||
                view.ResultText == null || view.ResumeButton == null || view.RestartButton == null ||
                view.PauseMenuButton == null || view.SoundButton == null || view.SensitivityDownButton == null ||
                view.SensitivityUpButton == null || view.RetryButton == null || view.DeathMenuButton == null)
            {
                throw new InvalidOperationException($"{name}: hud view has unassigned screen bindings.");
            }

            UnsubscribeButtons();

            _view = view;
            _localization = localization;
            _records = records;
            _interface = ui;
            _cameraConfig = camera;

            _isSoundOn = records.Volume > 0f;
            _sensitivity = Mathf.Clamp(records.MouseSensitivity, camera.MinMouseSensitivity,
                camera.MaxMouseSensitivity);

            SetupButtons();

            _view.ResumeButton.Clicked += OnResumeClicked;
            _view.RestartButton.Clicked += OnRestartClicked;
            _view.PauseMenuButton.Clicked += OnMenuClicked;
            _view.SoundButton.Clicked += OnSoundClicked;
            _view.SensitivityDownButton.Clicked += OnSensitivityDownClicked;
            _view.SensitivityUpButton.Clicked += OnSensitivityUpClicked;
            _view.RetryButton.Clicked += OnRestartClicked;
            _view.DeathMenuButton.Clicked += OnMenuClicked;

            _isSetupFinished = true;

            RefreshTexts();
        }

        public void ShowPause(bool isVisible)
        {
            AssertSetup();

            _view.PausePanel.SetActive(isVisible);

            if (isVisible)
            {
                RefreshTexts();
            }
        }

        public void ShowDeath(int level, int kills, float runTime)
        {
            AssertSetup();

            _view.HudPanel.SetActive(false);
            _view.PausePanel.SetActive(false);
            _view.DeathPanel.SetActive(true);

            RefreshTexts();
            _view.ResultText.text = BuildResultText(level, kills, runTime);
        }

        public void HideScreens()
        {
            AssertSetup();

            _view.DeathPanel.SetActive(false);
            _view.PausePanel.SetActive(false);
            _view.HudPanel.SetActive(true);
        }

        private void OnResumeClicked(MenuClick clicked)
        {
            AssertSetup();

            ResumeRequested?.Invoke(this);
        }

        private void OnRestartClicked(MenuClick clicked)
        {
            AssertSetup();

            RestartRequested?.Invoke(this);
        }

        private void OnMenuClicked(MenuClick clicked)
        {
            AssertSetup();

            MenuRequested?.Invoke(this);
        }

        private void OnSoundClicked(MenuClick clicked)
        {
            AssertSetup();

            float newVolume = 0f;

            if (_isSoundOn == false)
            {
                newVolume = _interface.DefaultVolume;
            }

            _isSoundOn = newVolume > 0f;
            _records.SetVolume(newVolume);
            VolumeChanged?.Invoke(newVolume);

            RefreshSoundText();
        }

        private void OnSensitivityDownClicked(MenuClick clicked)
        {
            AssertSetup();

            ChangeSensitivity(-_interface.SensitivityStep);
        }

        private void OnSensitivityUpClicked(MenuClick clicked)
        {
            AssertSetup();

            ChangeSensitivity(_interface.SensitivityStep);
        }

        private void ChangeSensitivity(float delta)
        {
            float clampedSensitivity = Mathf.Clamp(_sensitivity + delta, _cameraConfig.MinMouseSensitivity,
                _cameraConfig.MaxMouseSensitivity);

            if (clampedSensitivity == _sensitivity)
            {
                return;
            }

            _sensitivity = clampedSensitivity;
            _records.SetMouseSensitivity(_sensitivity);
            SensitivityChanged?.Invoke(_sensitivity);

            RefreshSensitivityText();
        }

        private void SetupButtons()
        {
            SetupButton(_view.ResumeButton);
            SetupButton(_view.RestartButton);
            SetupButton(_view.PauseMenuButton);
            SetupButton(_view.SoundButton);
            SetupButton(_view.SensitivityDownButton);
            SetupButton(_view.SensitivityUpButton);
            SetupButton(_view.RetryButton);
            SetupButton(_view.DeathMenuButton);
        }

        private void SetupButton(MenuClick button)
        {
            if (button.TryGetComponent(out Graphic graphic) == false)
            {
                throw new InvalidOperationException($"{name}: hud button {button.name} has no graphic.");
            }

            button.Setup(graphic, _interface.ButtonNormalColor, _interface.ButtonHoverColor);
        }

        private string BuildResultText(int level, int kills, float runTime)
        {
            string levelLine = string.Format(_localization.Get(DeathLevelFormatKey), level);
            string killsLine = string.Format(_localization.Get(DeathKillsFormatKey), kills);
            string timeLine = string.Format(_localization.Get(DeathTimeFormatKey),
                runTime.ToString(SecondsFormat));
            string bestLine = string.Format(_localization.Get(DeathBestFormatKey), _records.BestLevel,
                _records.BestKills);

            return levelLine + LineSeparator + killsLine + LineSeparator + timeLine + LineSeparator + bestLine;
        }

        private void RefreshTexts()
        {
            _view.PauseTitle.text = _localization.Get(PauseTitleKey);
            _view.ResumeText.text = _localization.Get(ResumeKey);
            _view.RestartText.text = _localization.Get(RestartKey);
            _view.PauseMenuText.text = _localization.Get(PauseMenuKey);
            _view.DeathTitle.text = _localization.Get(DeathTitleKey);
            _view.RetryText.text = _localization.Get(RetryKey);
            _view.DeathMenuText.text = _localization.Get(DeathMenuKey);

            RefreshSoundText();
            RefreshSensitivityText();
        }

        private void RefreshSoundText()
        {
            if (_isSoundOn)
            {
                _view.SoundText.text = _localization.Get(SoundOnKey);

                return;
            }

            _view.SoundText.text = _localization.Get(SoundOffKey);
        }

        private void RefreshSensitivityText()
        {
            string sensitivityValue = _sensitivity.ToString("F2");

            _view.SensitivityText.text = string.Format(_localization.Get(SensitivityFormatKey), sensitivityValue);
        }

        private void UnsubscribeButtons()
        {
            if (_view == null)
            {
                return;
            }

            if (_view.ResumeButton != null)
            {
                _view.ResumeButton.Clicked -= OnResumeClicked;
            }

            if (_view.RestartButton != null)
            {
                _view.RestartButton.Clicked -= OnRestartClicked;
            }

            if (_view.PauseMenuButton != null)
            {
                _view.PauseMenuButton.Clicked -= OnMenuClicked;
            }

            if (_view.SoundButton != null)
            {
                _view.SoundButton.Clicked -= OnSoundClicked;
            }

            if (_view.SensitivityDownButton != null)
            {
                _view.SensitivityDownButton.Clicked -= OnSensitivityDownClicked;
            }

            if (_view.SensitivityUpButton != null)
            {
                _view.SensitivityUpButton.Clicked -= OnSensitivityUpClicked;
            }

            if (_view.RetryButton != null)
            {
                _view.RetryButton.Clicked -= OnRestartClicked;
            }

            if (_view.DeathMenuButton != null)
            {
                _view.DeathMenuButton.Clicked -= OnMenuClicked;
            }
        }

        private void OnDisable()
        {
            if (_isSetupFinished == false)
            {
                return;
            }

            UnsubscribeButtons();
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: hud screens is not initialized, call Setup first.");
            }
        }
    }
}
