using UnityEngine;

namespace MazeHell.Configuration
{
    [CreateAssetMenu(fileName = "LevelPresentationConfig", menuName = "MazeHell/Configurations/Level Presentation")]
    public class LevelPresentationConfig : ScriptableObject
    {
        [Header("Fog")]
        [SerializeField] private Color _fogColor = new Color(0.07f, 0.02f, 0.02f, 1f);

        [Range(0f, 0.3f)]
        [SerializeField] private float _fogDensity = 0.045f;

        [Header("Lighting")]
        [SerializeField] private Color _ambientLightColor = new Color(0.12f, 0.04f, 0.04f, 1f);

        [SerializeField] private Color _lightColor = Color.white;

        [Min(0f)]
        [SerializeField] private float _lightIntensity = 30f;

        [Min(0f)]
        [SerializeField] private float _lightRange = 12f;

        [Range(0, 32)]
        [SerializeField] private int _maxSimultaneousLights = 8;

        [Header("Exit Gate")]
        [SerializeField] private GameObject _gateLeafPrefab;

        [Min(0f)]
        [SerializeField] private float _gateLiftHeight = 4f;

        [Min(0.1f)]
        [SerializeField] private float _gateLiftDuration = 1.5f;

        [SerializeField] private Color _exitGlowColor = new Color(1f, 0.35f, 0.1f, 1f);

        [Range(0f, 8f)]
        [SerializeField] private float _exitGlowIntensity = 2.5f;

        [Header("Dark Corridor")]
        [Min(1f)]
        [SerializeField] private float _darkCorridorLength = 84f;

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

        public Color LightColor => _lightColor;

        public float LightIntensity => _lightIntensity;

        public float LightRange => _lightRange;

        public int MaxSimultaneousLights => _maxSimultaneousLights;

        public GameObject GateLeafPrefab => _gateLeafPrefab;

        public float GateLiftHeight => _gateLiftHeight;

        public float GateLiftDuration => _gateLiftDuration;

        public Color ExitGlowColor => _exitGlowColor;

        public float ExitGlowIntensity => _exitGlowIntensity;

        public float DarkCorridorLength => _darkCorridorLength;

        public float TransitionEndInset => _transitionEndInset;

        public float LevelSplashDuration => _levelSplashDuration;

        public float ScreenFadeDuration => _screenFadeDuration;
    }
}
