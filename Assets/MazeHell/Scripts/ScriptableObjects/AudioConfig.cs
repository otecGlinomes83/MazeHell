using System;
using UnityEngine;
using UnityEngine.Audio;

namespace MazeHell.Configuration
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "MazeHell/Configurations/Audio")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Volumes")]
        [Range(0f, 1f)]
        [SerializeField] private float _masterVolume = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float _musicVolume = 0.7f;

        [Range(0f, 1f)]
        [SerializeField] private float _sfxVolume = 0.9f;

        [Header("Mixer")]
        [SerializeField] private AudioMixer _audioMixer;

        [Header("Music")]
        [SerializeField] private AudioClip _musicLoopClip;

        [Min(0f)]
        [SerializeField] private float _musicFadeInDuration = 2f;

        [Header("Pools")]
        [Range(1, 64)]
        [SerializeField] private int _sfxPoolCapacity = 16;

        [Range(1, 16)]
        [SerializeField] private int _maxVoicesPerCue = 3;

        [Header("Heartbeat")]
        [Min(0.1f)]
        [SerializeField] private float _heartbeatIntervalHealthy = 1.1f;

        [Min(0.1f)]
        [SerializeField] private float _heartbeatIntervalCritical = 0.45f;

        [Header("Footsteps")]
        [Min(0.1f)]
        [SerializeField] private float _footstepDistance = 2.2f;

        [Header("Cues")]
        [SerializeField] private AudioCueData[] _cues = new AudioCueData[]
        {
            new AudioCueData("shotgun_single"),
            new AudioCueData("shotgun_volley"),
            new AudioCueData("shotgun_reload"),
            new AudioCueData("footstep"),
            new AudioCueData("heartbeat"),
            new AudioCueData("jump"),
            new AudioCueData("land"),
            new AudioCueData("dash"),
            new AudioCueData("block"),
            new AudioCueData("player_hit"),
            new AudioCueData("imp_footstep"),
            new AudioCueData("imp_alert"),
            new AudioCueData("imp_windup"),
            new AudioCueData("imp_lunge"),
            new AudioCueData("imp_pain"),
            new AudioCueData("imp_death"),
            new AudioCueData("heal"),
            new AudioCueData("gate_horn"),
            new AudioCueData("gate_move"),
            new AudioCueData("door_open"),
            new AudioCueData("gate_open"),
            new AudioCueData("ui_click")
        };

        public float MasterVolume => _masterVolume;

        public float MusicVolume => _musicVolume;

        public float SfxVolume => _sfxVolume;

        public AudioMixer AudioMixer => _audioMixer;

        public AudioClip MusicLoopClip => _musicLoopClip;

        public float MusicFadeInDuration => _musicFadeInDuration;

        public int SfxPoolCapacity => _sfxPoolCapacity;

        public int MaxVoicesPerCue => _maxVoicesPerCue;

        public float HeartbeatIntervalHealthy => _heartbeatIntervalHealthy;

        public float HeartbeatIntervalCritical => _heartbeatIntervalCritical;

        public float FootstepDistance => _footstepDistance;

        public AudioCueData[] Cues => _cues;
    }

    [Serializable]
    public class AudioCueData
    {
        [SerializeField] private string _cueId;

        [SerializeField] private AudioClip _clip;

        [Range(0f, 1f)]
        [SerializeField] private float _volume = 1f;

        [Range(0f, 0.5f)]
        [SerializeField] private float _pitchVariation = 0.05f;

        [Range(0f, 1f)]
        [SerializeField] private float _spatialBlend;

        [Min(0.1f)]
        [SerializeField] private float _minDistance = 3f;

        [Min(1f)]
        [SerializeField] private float _maxDistance = 40f;

        [SerializeField] private bool _ignoreListenerPause;
        [SerializeField] private bool _isCritical;

        public AudioCueData()
        {
        }

        public AudioCueData(string cueId)
        {
            _cueId = cueId;
        }

        public string CueId => _cueId;

        public AudioClip Clip => _clip;

        public float Volume => _volume;

        public float PitchVariation => _pitchVariation;

        public float SpatialBlend => _spatialBlend;

        public float MinDistance => _minDistance;

        public float MaxDistance => _maxDistance;

        public bool IgnoreListenerPause => _ignoreListenerPause;
        public bool IsCritical => _isCritical;
    }
}
