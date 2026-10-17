using TMPro;
using UnityEngine;

namespace MazeHell.UI
{
    public class DeathWindow : BaseWindow
    {
        [SerializeField] private MenuClick _retry;
        [SerializeField] private MenuClick _menu;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _result;
        [SerializeField] private TMP_Text _retryText;
        [SerializeField] private TMP_Text _menuText;

        public MenuClick Retry => _retry;
        public MenuClick Menu => _menu;

        public void Setup(Localization localization, string result)
        {
            _title.text = localization.Get("death_title");
            _result.text = result;
            _retryText.text = localization.Get("death_retry");
            _menuText.text = localization.Get("death_menu");
        }
    }
}
