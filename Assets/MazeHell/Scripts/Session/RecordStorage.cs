using System;
using MazeHell.Configuration;
using UnityEngine;

namespace MazeHell.Session
{
    public class RecordStorage
    {
        private const string RecordLevelKey = "MazeHell.Record.Level";

        private const string RecordKillsKey = "MazeHell.Record.Kills";

        private const string RecordTimeKey = "MazeHell.Record.Time";

        private const string LanguageKey = "MazeHell.Language";

        private const string VolumeKey = "MazeHell.Volume";

        private const string MouseSensitivityKey = "MazeHell.MouseSensitivity";

        private int _bestLevel;

        private int _bestKills;

        private float _bestTime;

        private GameLanguage _language;

        private float _volume;

        private float _mouseSensitivity;

        public RecordStorage(float defaultVolume, float defaultMouseSensitivity)
        {
            _bestLevel = PlayerPrefs.GetInt(RecordLevelKey, 0);
            _bestKills = PlayerPrefs.GetInt(RecordKillsKey, 0);
            _bestTime = PlayerPrefs.GetFloat(RecordTimeKey, 0f);

            int storedLanguage = PlayerPrefs.GetInt(LanguageKey, (int)GameLanguage.Russian);

            if (storedLanguage == (int)GameLanguage.Russian || storedLanguage == (int)GameLanguage.English)
            {
                _language = (GameLanguage)storedLanguage;
            }
            else
            {
                _language = GameLanguage.Russian;
            }

            _volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, Mathf.Clamp01(defaultVolume)));
            _mouseSensitivity = PlayerPrefs.GetFloat(MouseSensitivityKey, defaultMouseSensitivity);

            if (float.IsNaN(_mouseSensitivity) || _mouseSensitivity <= 0f)
            {
                _mouseSensitivity = defaultMouseSensitivity;
            }
        }

        public int BestLevel => _bestLevel;

        public int BestKills => _bestKills;

        public float BestTime => _bestTime;

        public GameLanguage Language => _language;

        public float Volume => _volume;

        public float MouseSensitivity => _mouseSensitivity;

        public bool TrySaveRecord(int level, int kills, float time)
        {
            if (level < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level), $"Record level must be non-negative, got {level}.");
            }

            if (kills < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(kills), $"Record kills must be non-negative, got {kills}.");
            }

            if (float.IsNaN(time) || float.IsInfinity(time) || time < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(time), $"Record time must be non-negative, got {time}.");
            }

            if (IsNewRecord(level, kills, time) == false)
            {
                return false;
            }

            _bestLevel = level;
            _bestKills = kills;
            _bestTime = time;

            PlayerPrefs.SetInt(RecordLevelKey, level);
            PlayerPrefs.SetInt(RecordKillsKey, kills);
            PlayerPrefs.SetFloat(RecordTimeKey, time);
            PlayerPrefs.Save();

            return true;
        }

        public void SetLanguage(GameLanguage language)
        {
            if (language == _language)
            {
                return;
            }

            _language = language;

            PlayerPrefs.SetInt(LanguageKey, (int)language);
            PlayerPrefs.Save();
        }

        public void SetVolume(float volume)
        {
            float clampedVolume = Mathf.Clamp01(volume);

            if (clampedVolume == _volume)
            {
                return;
            }

            _volume = clampedVolume;

            PlayerPrefs.SetFloat(VolumeKey, clampedVolume);
            PlayerPrefs.Save();
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            if (float.IsNaN(sensitivity) || float.IsInfinity(sensitivity) || sensitivity <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(sensitivity),
                    $"Mouse sensitivity must be positive, got {sensitivity}.");
            }

            if (sensitivity == _mouseSensitivity)
            {
                return;
            }

            _mouseSensitivity = sensitivity;

            PlayerPrefs.SetFloat(MouseSensitivityKey, sensitivity);
            PlayerPrefs.Save();
        }

        private bool IsNewRecord(int level, int kills, float time)
        {
            if (level > _bestLevel)
            {
                return true;
            }

            if (level < _bestLevel)
            {
                return false;
            }

            if (kills > _bestKills)
            {
                return true;
            }

            if (kills < _bestKills)
            {
                return false;
            }

            return time > _bestTime;
        }
    }
}
