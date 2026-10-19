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
        [SerializeField] private VolumeSlider _musicSlider;
        [SerializeField] private VolumeSlider _sfxSlider;
        [SerializeField] private TMP_Text _musicText;
        [SerializeField] private TMP_Text _sfxText;
        [SerializeField] private TMP_Text _soundText;
        [SerializeField] private TMP_Text _sensitivityText;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _closeText;

        public VolumeSlider MusicSlider => _musicSlider;
        public VolumeSlider SfxSlider => _sfxSlider;
        public MenuClick Close => _close;
        public MenuClick Sound => _sound;
        public MenuClick Decrease => _decrease;
        public MenuClick Increase => _increase;

        public void Refresh(Localization localization, bool soundOn, float sensitivity, float musicVolume, float sfxVolume)
        {
            _musicText.text = localization.Get("settings_music");
            _sfxText.text = localization.Get("settings_sfx");
            _musicSlider.SetValueWithoutNotify(musicVolume);
            _sfxSlider.SetValueWithoutNotify(sfxVolume);
            _title.text = localization.Get("settings_title");
            _closeText.text = localization.Get("window_back");
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
