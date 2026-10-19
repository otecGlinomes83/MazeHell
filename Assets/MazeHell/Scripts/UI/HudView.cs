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
        [SerializeField] private MinimapGraphic _minimap;
        [SerializeField] private TMP_Text _splashText;
        [SerializeField] private TMP_Text _objectiveText;
        [SerializeField] private RectTransform _popupRoot;
        [SerializeField] private CanvasGroup _hudGroup;
        [SerializeField] private PauseWindow _pauseWindowPrefab;
        [SerializeField] private DeathWindow _deathWindowPrefab;
        [SerializeField] private SettingsWindow _settingsWindowPrefab;
        [SerializeField] private NarrativePopup _narrativePrefab;

        public RectTransform PopupRoot => _popupRoot;
        public CanvasGroup HudGroup => _hudGroup;
        public PauseWindow PauseWindowPrefab => _pauseWindowPrefab;
        public DeathWindow DeathWindowPrefab => _deathWindowPrefab;
        public SettingsWindow SettingsWindowPrefab => _settingsWindowPrefab;
        public NarrativePopup NarrativePrefab => _narrativePrefab;

        public TMP_Text ObjectiveText => _objectiveText;

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
        public MinimapGraphic Minimap => _minimap;
        public TMP_Text SplashText => _splashText;
    }
}
