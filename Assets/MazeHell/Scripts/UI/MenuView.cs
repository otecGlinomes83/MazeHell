using TMPro;
using UnityEngine;

namespace MazeHell.UI
{
    [DisallowMultipleComponent]
    public class MenuView : MonoBehaviour
    {
        [SerializeField] private RectTransform _safeRoot;
        [SerializeField] private RectTransform _popupRoot;
        [SerializeField] private CanvasGroup _menuGroup;
        [SerializeField] private SettingsWindow _settingsWindowPrefab;
        [SerializeField] private MenuClick _startButton;
        [SerializeField] private MenuClick _leaderboardButton;
        [SerializeField] private MenuClick _languageButton;
        [SerializeField] private MenuClick _soundButton;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _startText;
        [SerializeField] private TMP_Text _leaderboardText;
        [SerializeField] private TMP_Text _languageText;
        [SerializeField] private TMP_Text _soundText;
        [SerializeField] private TMP_Text _recordText;
        [SerializeField] private TMP_Text _statusText;

        public RectTransform PopupRoot => _popupRoot;
        public CanvasGroup MenuGroup => _menuGroup;
        public SettingsWindow SettingsWindowPrefab => _settingsWindowPrefab;

        public RectTransform SafeRoot => _safeRoot;
        public MenuClick StartButton => _startButton;
        public MenuClick LeaderboardButton => _leaderboardButton;
        public MenuClick LanguageButton => _languageButton;
        public MenuClick SoundButton => _soundButton;
        public TMP_Text Title => _title;
        public TMP_Text StartText => _startText;
        public TMP_Text LeaderboardText => _leaderboardText;
        public TMP_Text LanguageText => _languageText;
        public TMP_Text SoundText => _soundText;
        public TMP_Text RecordText => _recordText;
        public TMP_Text StatusText => _statusText;
    }
}
