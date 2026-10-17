using UnityEngine;

namespace MazeHell.Configuration
{
    [CreateAssetMenu(fileName = "LevelPresentationConfig", menuName = "MazeHell/Configurations/Level Presentation")]
    public class LevelPresentationConfig : ScriptableObject
    {
        [Header("Fog")]
        [SerializeField] private Color _fogColor = new Color(0.07f, 0.02f, 0.02f, 1f);

        [Range(0f, 0.3f)]
        [SerializeField] private float _fogDensity = 0f;

        [Header("Lighting")]
        [SerializeField] private Color _ambientLightColor = Color.white;

        [Header("Exit Gate")]
        [SerializeField] private GameObject _gateLeafPrefab;

        [Min(0f)]
        [SerializeField] private float _gateLiftHeight = 4f;

        [Min(0.1f)]
        [SerializeField] private float _gateLiftDuration = 1.5f;
        [Min(0.1f)] [SerializeField] private float _gateActivationDistance = 2.2f;
        [SerializeField] private bool _exitRequiresAllKills = false;

        [Header("Transition Corridor")]
        [Min(1f)]
        [SerializeField] private float _darkCorridorLength = 84f;
        [SerializeField] private GameObject _blackCorridorPrefab;
        [Min(1f)] [SerializeField] private float _entranceSpawnInset = 8f;

        [Header("Objective")]
        [TextArea(3, 6)] [SerializeField] private string _firstLevelMonologue = "Я спущусь в самый центр ада.\nНужно найти проход глубже.\nБесы на пути меня не остановят.";
        [TextArea(1, 3)] [SerializeField] private string _objectiveText = "НАЙДИ ВЫХОД В СЛЕДУЮЩИЙ КРУГ";
        [Min(0f)] [SerializeField] private float _monologueDuration = 10f;

        [Min(0.1f)]
        [SerializeField] private float _transitionEndInset = 1f;

        [Header("Splash And Fade")]
        [Min(0.1f)]
        [SerializeField] private float _levelSplashDuration = 2f;

        [Min(0.05f)]
        [SerializeField] private float _screenFadeDuration = 0.8f;

        public Color FogColor => _fogColor;

        public float FogDensity => _fogDensity;

        public Color AmbientLightColor => _ambientLightColor;

        public GameObject GateLeafPrefab => _gateLeafPrefab;

        public float GateLiftHeight => _gateLiftHeight;

        public float GateLiftDuration => _gateLiftDuration;
        public float GateActivationDistance => _gateActivationDistance;
        public bool ExitRequiresAllKills => _exitRequiresAllKills;
        public GameObject BlackCorridorPrefab => _blackCorridorPrefab;
        public float EntranceSpawnInset => _entranceSpawnInset;
        public string FirstLevelMonologue => _firstLevelMonologue;
        public string ObjectiveText => _objectiveText;
        public float MonologueDuration => _monologueDuration;

        public float DarkCorridorLength => _darkCorridorLength;

        public float TransitionEndInset => _transitionEndInset;

        public float LevelSplashDuration => _levelSplashDuration;

        public float ScreenFadeDuration => _screenFadeDuration;
    }
}
