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

        [Header("Wall Progression")]
        [SerializeField] private Color _wallTintStart = new Color(1f, 0.64f, 0.52f, 1f);
        [SerializeField] private Color _wallTintDeep = new Color(1.25f, 0.18f, 0.12f, 1f);
        [Range(0.001f, 1f)] [SerializeField] private float _wallReddeningPerLevel = 0.08f;
        [Range(0f, 1f)] [SerializeField] private float _wallSaturation = 0.55f;
        [Range(0f, 3f)] [SerializeField] private float _wallBrightness = 1.4f;
        [Range(0f, 1f)] [SerializeField] private float _wallCorruptionStart = 0.03f;
        [Range(0f, 1f)] [SerializeField] private float _wallCorruptionPerLevel = 0.09f;
        [Min(0.1f)] [SerializeField] private float _wallCorruptionTileSize = 8f;

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
        [SerializeField] private string _firstLevelMonologueKey = "intro_monologue";
        [SerializeField] private string _objectiveKey = "level_objective";

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
        public Color WallTintStart => _wallTintStart;
        public Color WallTintDeep => _wallTintDeep;
        public float WallReddeningPerLevel => _wallReddeningPerLevel;
        public float WallSaturation => _wallSaturation;
        public float WallBrightness => _wallBrightness;
        public float WallCorruptionStart => _wallCorruptionStart;
        public float WallCorruptionPerLevel => _wallCorruptionPerLevel;
        public float WallCorruptionTileSize => _wallCorruptionTileSize;

        public GameObject GateLeafPrefab => _gateLeafPrefab;

        public float GateLiftHeight => _gateLiftHeight;

        public float GateLiftDuration => _gateLiftDuration;
        public float GateActivationDistance => _gateActivationDistance;
        public bool ExitRequiresAllKills => _exitRequiresAllKills;
        public GameObject BlackCorridorPrefab => _blackCorridorPrefab;
        public float EntranceSpawnInset => _entranceSpawnInset;
        public string FirstLevelMonologueKey => _firstLevelMonologueKey;
        public string ObjectiveKey => _objectiveKey;

        public float DarkCorridorLength => _darkCorridorLength;

        public float TransitionEndInset => _transitionEndInset;

        public float LevelSplashDuration => _levelSplashDuration;

        public float ScreenFadeDuration => _screenFadeDuration;
    }
}
