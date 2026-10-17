using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MazeHell.UI
{
    [DisallowMultipleComponent]
    public class HudView : MonoBehaviour
    {
        [SerializeField] private RectTransform _safeRoot;
        [SerializeField] private RectTransform _springRoot;
        [SerializeField] private RectTransform _weaponRoot;
        [SerializeField] private RectTransform _crosshairRoot;
        [SerializeField] private GameObject _hudPanel;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _deathPanel;
        [SerializeField] private GameObject _splashPanel;
        [SerializeField] private HudSpring _spring;
        [SerializeField] private HealthPortrait _portrait;
        [SerializeField] private WeaponSpriteAnimator _weaponAnimator;
        [SerializeField] private Image _portraitImage;
        [SerializeField] private Image _weaponImage;
        [SerializeField] private Image _barrelLeft;
        [SerializeField] private Image _barrelRight;
        [SerializeField] private Image _dashImage;
        [SerializeField] private Image _healthFill;
        [SerializeField] private Image _damageOverlay;
        [SerializeField] private Image _screenFade;
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private TMP_Text _enemyText;
        [SerializeField] private TMP_Text _splashText;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private TMP_Text _pauseTitle;
        [SerializeField] private TMP_Text _deathTitle;
        [SerializeField] private TMP_Text _resumeText;
        [SerializeField] private TMP_Text _restartText;
        [SerializeField] private TMP_Text _pauseMenuText;
        [SerializeField] private TMP_Text _soundText;
        [SerializeField] private TMP_Text _sensitivityText;
        [SerializeField] private TMP_Text _retryText;
        [SerializeField] private TMP_Text _deathMenuText;
        [SerializeField] private MenuClick _resumeButton;
        [SerializeField] private MenuClick _restartButton;
        [SerializeField] private MenuClick _pauseMenuButton;
        [SerializeField] private MenuClick _soundButton;
        [SerializeField] private MenuClick _sensitivityDownButton;
        [SerializeField] private MenuClick _sensitivityUpButton;
        [SerializeField] private MenuClick _retryButton;
        [SerializeField] private MenuClick _deathMenuButton;

        private HudPresenter _presenter;
        private HudScreens _screens;

        public HudPresenter Presenter => _presenter;
        public HudScreens Screens => _screens;

        public void SetupPresentation(HudPresenter presenter, HudScreens screens)
        {
            _presenter = presenter;
            _screens = screens;
        }

        public RectTransform SafeRoot => _safeRoot;
        public RectTransform SpringRoot => _springRoot;
        public RectTransform WeaponRoot => _weaponRoot;
        public RectTransform CrosshairRoot => _crosshairRoot;
        public GameObject HudPanel => _hudPanel;
        public GameObject PausePanel => _pausePanel;
        public GameObject DeathPanel => _deathPanel;
        public GameObject SplashPanel => _splashPanel;
        public HudSpring Spring => _spring;
        public HealthPortrait Portrait => _portrait;
        public WeaponSpriteAnimator WeaponAnimator => _weaponAnimator;
        public Image PortraitImage => _portraitImage;
        public Image WeaponImage => _weaponImage;
        public Image BarrelLeft => _barrelLeft;
        public Image BarrelRight => _barrelRight;
        public Image DashImage => _dashImage;
        public Image HealthFill => _healthFill;
        public Image DamageOverlay => _damageOverlay;
        public Image ScreenFade => _screenFade;
        public TMP_Text HealthText => _healthText;
        public TMP_Text EnemyText => _enemyText;
        public TMP_Text SplashText => _splashText;
        public TMP_Text ResultText => _resultText;
        public TMP_Text PauseTitle => _pauseTitle;
        public TMP_Text DeathTitle => _deathTitle;
        public TMP_Text ResumeText => _resumeText;
        public TMP_Text RestartText => _restartText;
        public TMP_Text PauseMenuText => _pauseMenuText;
        public TMP_Text SoundText => _soundText;
        public TMP_Text SensitivityText => _sensitivityText;
        public TMP_Text RetryText => _retryText;
        public TMP_Text DeathMenuText => _deathMenuText;
        public MenuClick ResumeButton => _resumeButton;
        public MenuClick RestartButton => _restartButton;
        public MenuClick PauseMenuButton => _pauseMenuButton;
        public MenuClick SoundButton => _soundButton;
        public MenuClick SensitivityDownButton => _sensitivityDownButton;
        public MenuClick SensitivityUpButton => _sensitivityUpButton;
        public MenuClick RetryButton => _retryButton;
        public MenuClick DeathMenuButton => _deathMenuButton;
    }
}
