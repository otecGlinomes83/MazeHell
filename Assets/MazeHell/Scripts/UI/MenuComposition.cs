using System;
using MazeHell.Audio;
using MazeHell.Configuration;
using MazeHell.Session;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MazeHell.UI
{
    [DisallowMultipleComponent]
    public class MenuComposition : MonoBehaviour
    {
        private const string AudioClickCueId = "ui_click";

        private const string MenuTitleKey = "menu_title";

        private const string MenuStartKey = "menu_start";

        private const string MenuLeaderboardKey = "menu_leaderboard";

        private const string LeaderboardUnavailableKey = "leaderboard_unavailable";

        private const string MenuLanguageKey = "menu_language";

        private const string LanguageValueKey = "language_value";

        private const string MenuRecordFormatKey = "menu_record_format";

        private const string MenuRecordNoneKey = "menu_record_none";

        private const string SoundOnKey = "sound_on";

        private const string SoundOffKey = "sound_off";

        private const string MenuControlsHintKey = "menu_controls_hint";

        [SerializeField] private MenuView _view;

        [SerializeField] private InterfaceConfig _interface;

        [SerializeField] private LocalizationConfig _localizationConfig;

        [SerializeField] private AudioConfig _audioConfig;

        [SerializeField] private CameraMotionConfig _cameraConfig;

        [SerializeField] private GameAudio _audio;

        [SerializeField] private AudioSource _musicSource;

        [SerializeField] private AudioSource[] _sfxSources;

        private RecordStorage _records;

        private Localization _localization;

        private bool _isFirstUserClickHandled;

        private bool _isSoundOn;

        private bool _isStatusHintVisible;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ValidateRefs();

            _records = new RecordStorage(_audioConfig.MasterVolume, _cameraConfig.MouseSensitivity);

            _localization = new Localization();
            _localization.Setup(_localizationConfig, _records.Language);

            _audio.Setup(_audioConfig, _musicSource, _sfxSources);
            _audio.SetVolume(_records.Volume);

            SetupSafeArea();
            SetupButton(_view.StartButton);
            SetupButton(_view.LeaderboardButton);
            SetupButton(_view.LanguageButton);
            SetupButton(_view.SoundButton);

            _view.StartButton.Clicked += OnStartClicked;
            _view.LeaderboardButton.Clicked += OnLeaderboardClicked;
            _view.LanguageButton.Clicked += OnLanguageClicked;
            _view.SoundButton.Clicked += OnSoundClicked;

            _isSoundOn = _records.Volume > 0f;
            _isStatusHintVisible = true;

            RefreshTexts();
        }

        private void Update()
        {
            _audio.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            if (_view == null)
            {
                return;
            }

            _view.StartButton.Clicked -= OnStartClicked;
            _view.LeaderboardButton.Clicked -= OnLeaderboardClicked;
            _view.LanguageButton.Clicked -= OnLanguageClicked;
            _view.SoundButton.Clicked -= OnSoundClicked;
        }

        private void OnStartClicked(MenuClick clicked)
        {
            HandleFirstUserClick();
            _audio.Play(AudioClickCueId, Vector3.zero);

            SceneManager.LoadScene(_interface.GameSceneName);
        }

        private void OnLeaderboardClicked(MenuClick clicked)
        {
            HandleFirstUserClick();
            _audio.Play(AudioClickCueId, Vector3.zero);

            _isStatusHintVisible = false;
            _view.StatusText.text = _localization.Get(LeaderboardUnavailableKey);
        }

        private void OnLanguageClicked(MenuClick clicked)
        {
            HandleFirstUserClick();
            _audio.Play(AudioClickCueId, Vector3.zero);

            GameLanguage nextLanguage = GameLanguage.Russian;

            if (_localization.Language == GameLanguage.Russian)
            {
                nextLanguage = GameLanguage.English;
            }

            _records.SetLanguage(nextLanguage);
            _localization.SetLanguage(nextLanguage);

            RefreshTexts();
        }

        private void OnSoundClicked(MenuClick clicked)
        {
            HandleFirstUserClick();

            float newVolume = 0f;

            if (_isSoundOn == false)
            {
                newVolume = _interface.DefaultVolume;
            }

            _isSoundOn = newVolume > 0f;
            _records.SetVolume(newVolume);
            _audio.SetVolume(newVolume);
            _audio.Play(AudioClickCueId, Vector3.zero);

            RefreshTexts();
        }

        private void HandleFirstUserClick()
        {
            if (_isFirstUserClickHandled)
            {
                return;
            }

            _isFirstUserClickHandled = true;
            _audio.StartMusic();
        }

        private void SetupSafeArea()
        {
            if (_view.SafeRoot.TryGetComponent(out SafeArea safeArea) == false)
            {
                throw new InvalidOperationException(
                    $"{name}: menu safe root {_view.SafeRoot.name} has no SafeArea component.");
            }

            safeArea.Setup(_view.SafeRoot);
        }

        private void SetupButton(MenuClick button)
        {
            if (button.TryGetComponent(out Graphic graphic) == false)
            {
                throw new InvalidOperationException($"{name}: menu button {button.name} has no graphic.");
            }

            button.Setup(graphic, _interface.ButtonNormalColor, _interface.ButtonHoverColor);
        }

        private void RefreshTexts()
        {
            _view.Title.text = _localization.Get(MenuTitleKey);
            _view.StartText.text = _localization.Get(MenuStartKey);
            _view.LeaderboardText.text = _localization.Get(MenuLeaderboardKey);
            _view.LanguageText.text = _localization.Get(MenuLanguageKey) + ": " +
                _localization.Get(LanguageValueKey);

            RefreshSoundText();
            RefreshRecordText();
            RefreshStatusText();
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

        private void RefreshRecordText()
        {
            if (_records.BestLevel <= 0)
            {
                _view.RecordText.text = _localization.Get(MenuRecordNoneKey);

                return;
            }

            _view.RecordText.text = string.Format(_localization.Get(MenuRecordFormatKey), _records.BestLevel,
                _records.BestKills);
        }

        private void RefreshStatusText()
        {
            if (_isStatusHintVisible == false)
            {
                return;
            }

            _view.StatusText.text = _localization.Get(MenuControlsHintKey);
        }

        private void ValidateRefs()
        {
            if (_view == null)
            {
                throw new InvalidOperationException($"{name}: menu view is not assigned.");
            }

            if (_interface == null)
            {
                throw new InvalidOperationException($"{name}: interface config is not assigned.");
            }

            if (_localizationConfig == null)
            {
                throw new InvalidOperationException($"{name}: localization config is not assigned.");
            }

            if (_audioConfig == null)
            {
                throw new InvalidOperationException($"{name}: audio config is not assigned.");
            }

            if (_cameraConfig == null)
            {
                throw new InvalidOperationException($"{name}: camera motion config is not assigned.");
            }

            if (_audio == null)
            {
                throw new InvalidOperationException($"{name}: game audio is not assigned.");
            }

            if (_musicSource == null)
            {
                throw new InvalidOperationException($"{name}: music source is not assigned.");
            }

            if (_sfxSources == null || _sfxSources.Length == 0)
            {
                throw new InvalidOperationException($"{name}: sfx sources are not assigned.");
            }

            if (_view.SafeRoot == null || _view.StartButton == null || _view.LeaderboardButton == null ||
                _view.LanguageButton == null || _view.SoundButton == null || _view.Title == null ||
                _view.StartText == null || _view.LeaderboardText == null || _view.LanguageText == null ||
                _view.SoundText == null || _view.RecordText == null || _view.StatusText == null)
            {
                throw new InvalidOperationException($"{name}: menu view has unassigned bindings.");
            }
        }
    }
}
