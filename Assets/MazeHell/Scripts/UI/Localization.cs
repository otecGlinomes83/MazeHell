using System;
using System.Collections.Generic;
using MazeHell.Configuration;

namespace MazeHell.UI
{
    public class Localization
    {
        private Dictionary<string, LocalizationEntry> _entriesById = new Dictionary<string, LocalizationEntry>();

        private GameLanguage _language;

        private bool _isSetupFinished;

        public event Action<GameLanguage> Changed;

        public GameLanguage Language => _language;

        public void Setup(LocalizationConfig config, GameLanguage language)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            LocalizationEntry[] entries = config.Entries;

            if (entries == null)
            {
                throw new InvalidOperationException("Localization config has no entries array.");
            }

            foreach (LocalizationEntry entry in entries)
            {
                if (entry == null)
                {
                    throw new InvalidOperationException("Localization config contains a null entry.");
                }

                if (string.IsNullOrEmpty(entry.Key))
                {
                    throw new InvalidOperationException("Localization config contains an entry with an empty key.");
                }

                if (_entriesById.ContainsKey(entry.Key))
                {
                    throw new InvalidOperationException($"Localization config contains a duplicate key: {entry.Key}.");
                }

                _entriesById.Add(entry.Key, entry);
            }

            _language = language;
            _isSetupFinished = true;
        }

        public string Get(string key)
        {
            AssertSetup();

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Localization key must not be null or empty.", nameof(key));
            }

            if (_entriesById.TryGetValue(key, out LocalizationEntry entry) == false)
            {
                throw new KeyNotFoundException($"Localization key not found: {key}.");
            }

            return ReadLocalizedText(entry);
        }

        public void SetLanguage(GameLanguage language)
        {
            AssertSetup();

            if (language == _language)
            {
                return;
            }

            _language = language;

            Changed?.Invoke(language);
        }

        private string ReadLocalizedText(LocalizationEntry entry)
        {
            if (_language == GameLanguage.English)
            {
                if (string.IsNullOrEmpty(entry.English) == false)
                {
                    return entry.English;
                }

                return entry.Russian;
            }

            if (string.IsNullOrEmpty(entry.Russian) == false)
            {
                return entry.Russian;
            }

            return entry.English;
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException("Localization is not initialized, call Setup first.");
            }
        }
    }
}
