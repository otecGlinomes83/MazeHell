using TMPro;
using UnityEngine;

namespace MazeHell.Configuration
{
    [CreateAssetMenu(fileName = "InterfaceConfig", menuName = "MazeHell/Configurations/Interface")]
    public class InterfaceConfig : ScriptableObject
    {
        [Header("Font")]
        [SerializeField] private TMP_FontAsset _fontAsset;

        [Header("Canvas")]
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);

        [Range(0f, 1f)]
        [SerializeField] private float _matchWidthOrHeight = 0.5f;

        [Header("Colors")]
        [SerializeField] private Color _textColor = new Color(0.85f, 0.8f, 0.75f, 1f);

        [SerializeField] private Color _accentColor = new Color(1f, 0.45f, 0.15f, 1f);

        [SerializeField] private Color _dangerColor = new Color(0.85f, 0.1f, 0.05f, 1f);

        [Header("Layout")]
        [Min(0f)]
        [SerializeField] private float _hudMarginPixels = 48f;

        [Min(0f)]
        [SerializeField] private float _crosshairSizePixels = 32f;

        [Header("Font Sizes")]
        [Min(1f)]
        [SerializeField] private float _splashFontSize = 72f;

        [Min(1f)]
        [SerializeField] private float _menuFontSize = 36f;

        [Min(1f)]
        [SerializeField] private float _buttonFontSize = 32f;

        [Min(1f)]
        [SerializeField] private float _hudFontSize = 28f;

        [Header("Icons")]
        [SerializeField] private Sprite _crosshairSprite;

        [SerializeField] private Sprite _barrelFullSprite;

        [SerializeField] private Sprite _barrelEmptySprite;

        [SerializeField] private Sprite _dashPipSprite;

        [SerializeField] private Sprite _skullSprite;

        [SerializeField] private Sprite _menuBackgroundSprite;

        [Header("Scenes")]
        [SerializeField] private string _menuSceneName = "Menu";

        [SerializeField] private string _gameSceneName = "Game";

        [Header("Buttons")]
        [SerializeField] private Color _buttonNormalColor = new Color(0.07f, 0.025f, 0.018f, 0.95f);

        [SerializeField] private Color _buttonHoverColor = new Color(0.25f, 0.065f, 0.025f, 1f);

        [Header("Presentation Tuning")]
        [Range(0.001f, 0.2f)]
        [SerializeField] private float _sensitivityStep = 0.02f;

        [Range(0.01f, 2f)]
        [SerializeField] private float _damageFlashDuration = 0.25f;

        [Range(0f, 1f)]
        [SerializeField] private float _maxVignetteAlpha = 0.3f;

        [Range(0f, 1f)]
        [SerializeField] private float _defaultVolume = 0.7f;

        public TMP_FontAsset FontAsset => _fontAsset;

        public Vector2 ReferenceResolution => _referenceResolution;

        public float MatchWidthOrHeight => _matchWidthOrHeight;

        public Color TextColor => _textColor;

        public Color AccentColor => _accentColor;

        public Color DangerColor => _dangerColor;

        public float HudMarginPixels => _hudMarginPixels;

        public float CrosshairSizePixels => _crosshairSizePixels;

        public float SplashFontSize => _splashFontSize;

        public float MenuFontSize => _menuFontSize;

        public float ButtonFontSize => _buttonFontSize;

        public float HudFontSize => _hudFontSize;

        public Sprite CrosshairSprite => _crosshairSprite;

        public Sprite BarrelFullSprite => _barrelFullSprite;

        public Sprite BarrelEmptySprite => _barrelEmptySprite;

        public Sprite DashPipSprite => _dashPipSprite;

        public Sprite SkullSprite => _skullSprite;

        public Sprite MenuBackgroundSprite => _menuBackgroundSprite;

        public string MenuSceneName => _menuSceneName;

        public string GameSceneName => _gameSceneName;

        public Color ButtonNormalColor => _buttonNormalColor;

        public Color ButtonHoverColor => _buttonHoverColor;

        public float SensitivityStep => _sensitivityStep;

        public float DamageFlashDuration => _damageFlashDuration;

        public float MaxVignetteAlpha => _maxVignetteAlpha;

        public float DefaultVolume => _defaultVolume;
    }
}
