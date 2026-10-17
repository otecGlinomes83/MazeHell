using TMPro;
using UnityEngine;

namespace MazeHell.UI
{
    public class SettingsWindow : BaseWindow
    {
        [SerializeField] private MenuClick _close;
        [SerializeField] private MenuClick _sound;
        [SerializeField] private MenuClick _decrease;
        [SerializeField] private MenuClick _increase;
        [SerializeField] private TMP_Text _soundText;
        [SerializeField] private TMP_Text _sensitivityText;

        public MenuClick Close => _close;
        public MenuClick Sound => _sound;
        public MenuClick Decrease => _decrease;
        public MenuClick Increase => _increase;

        public void Refresh(Localization localization, bool soundOn, float sensitivity)
        {
            string soundKey = "sound_off";

            if (soundOn)
            {
                soundKey = "sound_on";
            }

            _soundText.text = localization.Get(soundKey);
            _sensitivityText.text = string.Format(localization.Get("hud_sensitivity_format"), sensitivity.ToString("F2"));
        }
    }
}
