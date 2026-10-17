using TMPro;
using UnityEngine;

namespace MazeHell.UI
{
    public class PauseWindow : BaseWindow
    {
        [SerializeField] private MenuClick _resume;
        [SerializeField] private MenuClick _restart;
        [SerializeField] private MenuClick _menu;
        [SerializeField] private MenuClick _settings;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _resumeText;
        [SerializeField] private TMP_Text _restartText;
        [SerializeField] private TMP_Text _menuText;
        [SerializeField] private TMP_Text _settingsText;

        public MenuClick Resume => _resume;
        public MenuClick Restart => _restart;
        public MenuClick Menu => _menu;
        public MenuClick Settings => _settings;

        public void Setup(Localization localization)
        {
            _title.text = localization.Get("pause_title");
            _resumeText.text = localization.Get("pause_resume");
            _restartText.text = localization.Get("pause_restart");
            _menuText.text = localization.Get("pause_menu");
            _settingsText.text = "НАСТРОЙКИ";
        }
    }
}
