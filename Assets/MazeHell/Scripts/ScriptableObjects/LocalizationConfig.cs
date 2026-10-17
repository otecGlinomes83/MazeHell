using System;
using UnityEngine;

namespace MazeHell.Configuration
{
    public enum GameLanguage
    {
        Russian = 0,
        English = 1
    }

    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "MazeHell/Configurations/Localization")]
    public class LocalizationConfig : ScriptableObject
    {
        [SerializeField] private GameLanguage _defaultLanguage = GameLanguage.Russian;

        [SerializeField] private LocalizationEntry[] _entries = new LocalizationEntry[]
        {
            new LocalizationEntry("menu_title", "MAZEHELL", "MAZEHELL"),
            new LocalizationEntry("menu_start", "НАЧАТЬ", "START"),
            new LocalizationEntry("menu_record", "РЕКОРД", "BEST"),
            new LocalizationEntry("menu_leaderboard", "ЛИДЕРБОРД", "LEADERBOARD"),
            new LocalizationEntry("leaderboard_unavailable", "ЛИДЕРБОРД НЕДОСТУПЕН", "LEADERBOARD UNAVAILABLE"),
            new LocalizationEntry("menu_language", "ЯЗЫК", "LANGUAGE"),
            new LocalizationEntry("menu_sound", "ЗВУК", "SOUND"),
            new LocalizationEntry("menu_quit", "ВЫХОД", "QUIT"),
            new LocalizationEntry("pause_title", "ПАУЗА", "PAUSED"),
            new LocalizationEntry("pause_resume", "ПРОДОЛЖИТЬ", "RESUME"),
            new LocalizationEntry("pause_restart", "ЗАНОВО", "RESTART"),
            new LocalizationEntry("pause_sound", "ЗВУК", "SOUND"),
            new LocalizationEntry("pause_sensitivity", "ЧУВСТВИТЕЛЬНОСТЬ", "SENSITIVITY"),
            new LocalizationEntry("pause_menu", "В МЕНЮ", "MAIN MENU"),
            new LocalizationEntry("death_title", "ТЫ УМЕР", "YOU DIED"),
            new LocalizationEntry("death_level", "УРОВЕНЬ", "LEVEL"),
            new LocalizationEntry("death_kills", "УБИЙСТВА", "KILLS"),
            new LocalizationEntry("death_time", "ВРЕМЯ", "TIME"),
            new LocalizationEntry("death_best", "РЕКОРД", "BEST"),
            new LocalizationEntry("death_retry", "ЕЩЁ РАЗ", "RETRY"),
            new LocalizationEntry("death_menu", "В МЕНЮ", "MAIN MENU"),
            new LocalizationEntry("level_splash_format", "УРОВЕНЬ {0}", "LEVEL {0}"),
            new LocalizationEntry("language_value", "РУС", "ENG"),
            new LocalizationEntry("sound_on", "ЗВУК: ВКЛ", "SOUND: ON"),
            new LocalizationEntry("sound_off", "ЗВУК: ВЫКЛ", "SOUND: OFF"),
            new LocalizationEntry("menu_record_format", "РЕКОРД: УР. {0} · {1} УБ.", "BEST: LV. {0} · {1} KILLS"),
            new LocalizationEntry("menu_record_none", "РЕКОРД: —", "BEST: —"),
            new LocalizationEntry("menu_controls_hint",
                "WASD — ДВИЖЕНИЕ · ЛКМ — ВЫСТРЕЛ · ПКМ — БЛОК · SPACE — ПРЫЖОК · SHIFT — РЫВОК",
                "WASD - MOVE · LMB - SHOOT · RMB - BLOCK · SPACE - JUMP · SHIFT - DASH"),
            new LocalizationEntry("hud_sensitivity_format", "ЧУВСТВИТЕЛЬНОСТЬ: {0}", "SENSITIVITY: {0}"),
            new LocalizationEntry("hud_enemy_format", "ОСТАЛОСЬ: {0}", "REMAINING: {0}"),
            new LocalizationEntry("death_level_format", "УРОВЕНЬ: {0}", "LEVEL: {0}"),
            new LocalizationEntry("death_kills_format", "УБИЙСТВА: {0}", "KILLS: {0}"),
            new LocalizationEntry("death_time_format", "ВРЕМЯ: {0} С", "TIME: {0} S"),
            new LocalizationEntry("death_best_format", "РЕКОРД: УР. {0} · {1} УБ.", "BEST: LV. {0} · {1} KILLS")
        };

        public GameLanguage DefaultLanguage => _defaultLanguage;

        public LocalizationEntry[] Entries => _entries;
    }

    [Serializable]
    public class LocalizationEntry
    {
        [SerializeField] private string _key;

        [SerializeField] private string _russian;

        [SerializeField] private string _english;

        public LocalizationEntry()
        {
        }

        public LocalizationEntry(string key, string russian, string english)
        {
            _key = key;
            _russian = russian;
            _english = english;
        }

        public string Key => _key;

        public string Russian => _russian;

        public string English => _english;
    }
}
