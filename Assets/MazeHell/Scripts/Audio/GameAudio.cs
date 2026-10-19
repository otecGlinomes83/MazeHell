using System;
using System.Collections.Generic;
using MazeHell.Configuration;
using UnityEngine;

namespace MazeHell.Audio
{
    [DisallowMultipleComponent]
    public class GameAudio : MonoBehaviour
    {
        private const string HeartbeatCueId = "heartbeat";

        private const string FootstepCueId = "footstep";

        private const float MinimalFadeDuration = 0.01f;

        private const float BasePitch = 1f;
        private const float MinimumLinearVolume = 0.0001f;
        private const string MusicVolumeParameter = "MusicVolume";
        private const string SfxVolumeParameter = "SFXVolume";

        private AudioSource _heartbeatSource;
        private HeartbeatRhythm _heartbeat;
        private float _musicSetting = 1f;
        private float _sfxSetting = 1f;
        private bool _areMixerVolumesDirty;
        private MusicChannel _music;
        private AudioSource _secondMusicSource;

        private AudioConfig _config;

        private AudioSource _musicSource;

        private AudioSource[] _sfxSources;

        private AudioCueData[] _cues;

        private Dictionary<string, int> _cueIndexById = new Dictionary<string, int>();

        private int[] _sourceCueIndices;

        private float[] _sourceBaseVolumes;

        private int _sourceCount;

        private int _heartbeatCueIndex;

        private int _footstepCueIndex;

        private float _masterVolume;

        private float _transitionProgress;

        private float _stepDistanceAccumulator;

        private float _musicVolumeCurrent;

        private bool _isMusicPlaying;

        private bool _isSetupFinished;

        public void Setup(AudioConfig config, AudioSource musicSource, AudioSource[] sfxSources)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (musicSource == null)
            {
                throw new ArgumentNullException(nameof(musicSource));
            }

            if (sfxSources == null)
            {
                throw new ArgumentNullException(nameof(sfxSources));
            }

            if (sfxSources.Length == 0)
            {
                throw new ArgumentException($"{name}: sfx source pool must not be empty.", nameof(sfxSources));
            }

            foreach (AudioSource sfxSource in sfxSources)
            {
                if (sfxSource == null)
                {
                    throw new ArgumentException($"{name}: sfx source pool contains a null source.",
                        nameof(sfxSources));
                }
            }

            if (config.MusicLoopClip == null)
            {
                throw new InvalidOperationException($"{name}: audio config has no music loop clip assigned.");
            }

            AudioCueData[] cues = config.Cues;

            if (cues == null)
            {
                throw new InvalidOperationException($"{name}: audio config has no cues array.");
            }

            if (config.AudioMixer == null || config.MusicMixerGroup == null || config.SfxMixerGroup == null)
            {
                throw new InvalidOperationException($"{name}: audio mixer and Music/SFX groups must be assigned.");
            }

            _config = config;
            _musicSource = musicSource;
            _sfxSources = sfxSources;
            _cues = cues;
            _sourceCount = sfxSources.Length;
            _sourceCueIndices = new int[_sourceCount];
            _sourceBaseVolumes = new float[_sourceCount];

            for (int sourceIndex = 0; sourceIndex < _sourceCount; sourceIndex++)
            {
                _sourceCueIndices[sourceIndex] = -1;
                _sourceBaseVolumes[sourceIndex] = 0f;
            }

            BuildCueLookup(cues);

            _masterVolume = Mathf.Clamp01(config.MasterVolume);
            _transitionProgress = 0f;

            _stepDistanceAccumulator = 0f;
            _musicVolumeCurrent = 0f;
            _isMusicPlaying = false;
            _heartbeat = new HeartbeatRhythm(config);
            _musicSource.outputAudioMixerGroup = config.MusicMixerGroup;
            _musicSource.playOnAwake = false;

            if (_secondMusicSource == null)
            {
                _secondMusicSource = gameObject.AddComponent<AudioSource>();
            }

            _secondMusicSource.outputAudioMixerGroup = config.MusicMixerGroup;
            _music = new MusicChannel(_musicSource, _secondMusicSource, config.MusicFadeInDuration,
                config.MusicCrossfadeDuration);

            foreach (AudioSource source in _sfxSources)
            {
                source.outputAudioMixerGroup = config.SfxMixerGroup;
                source.playOnAwake = false;
            }

            if (_heartbeatSource == null)
            {
                _heartbeatSource = gameObject.AddComponent<AudioSource>();
            }

            _heartbeatSource.playOnAwake = false;
            _heartbeatSource.spatialBlend = 0f;
            _heartbeatSource.priority = 32;
            _heartbeatSource.outputAudioMixerGroup = config.SfxMixerGroup;
            _heartbeatSource.clip = _cues[_heartbeatCueIndex].Clip;
            _areMixerVolumesDirty = true;
            _isSetupFinished = true;
        }

        public void Tick(float deltaTime)
        {
            AssertSetup();

            ApplyMixerVolumes();

            if (_isMusicPlaying == false)
            {
                return;
            }

            float clampedDeltaTime = Mathf.Max(0f, deltaTime);
            float targetVolume = ComputeMusicTargetVolume();
            float fadeRate = 1f / Mathf.Max(MinimalFadeDuration, _config.MusicFadeInDuration);

            _musicVolumeCurrent = Mathf.MoveTowards(_musicVolumeCurrent, targetVolume, clampedDeltaTime * fadeRate);
            _music.Tick(clampedDeltaTime, _musicVolumeCurrent);
        }

        public void StartMusic()
        {
            AssertSetup();

            if (_isMusicPlaying)
            {
                return;
            }

            AudioClip[] playlist = _config.GameplayMusicClips;

            if (playlist != null && playlist.Length > 0)
            {
                _music.Play(playlist);
                _isMusicPlaying = true;
                return;
            }

            _music.Play(new AudioClip[] { _config.MusicLoopClip });
            _isMusicPlaying = true;
        }

        public void StartMenuMusic()
        {
            AssertSetup();

            if (_isMusicPlaying)
            {
                return;
            }

            AudioClip clip = _config.MenuMusicClip;

            if (clip == null)
            {
                clip = _config.MusicLoopClip;
            }

            _music.Play(new AudioClip[] { clip });
            _isMusicPlaying = true;
        }

        public void SetMusicVolume(float volume)
        {
            AssertSetup();
            _musicSetting = Mathf.Clamp01(volume);
            _areMixerVolumesDirty = true;
        }

        public void SetSfxVolume(float volume)
        {
            AssertSetup();
            _sfxSetting = Mathf.Clamp01(volume);
            _areMixerVolumesDirty = true;
        }

        private void ApplyMixerVolumes()
        {
            if (_areMixerVolumesDirty == false)
            {
                return;
            }

            float musicDecibels = Mathf.Log10(Mathf.Max(MinimumLinearVolume, _musicSetting)) * 20f;
            float sfxDecibels = Mathf.Log10(Mathf.Max(MinimumLinearVolume, _sfxSetting)) * 20f;

            if (_config.AudioMixer.SetFloat(MusicVolumeParameter, musicDecibels) == false ||
                _config.AudioMixer.SetFloat(SfxVolumeParameter, sfxDecibels) == false)
            {
                throw new InvalidOperationException($"{name}: mixer must expose MusicVolume and SFXVolume.");
            }

            _areMixerVolumesDirty = false;
        }

        public void Play(string cueId, Vector3 position)
        {
            AssertSetup();

            int cueIndex = ResolveCueIndex(cueId);

            PlayCue(cueIndex, position);
        }

        public void SetVolume(float volume)
        {
            AssertSetup();

            _masterVolume = Mathf.Clamp01(volume);
            _heartbeatSource.volume = _heartbeat.Volume * _config.SfxVolume * _masterVolume;

            for (int sourceIndex = 0; sourceIndex < _sourceCount; sourceIndex++)
            {
                _sfxSources[sourceIndex].volume = _sourceBaseVolumes[sourceIndex] * _masterVolume;
            }
        }

        public void SetTransition(float progress01)
        {
            AssertSetup();

            _transitionProgress = Mathf.Clamp01(progress01);
        }

        public void Heartbeat(float healthFraction, float deltaTime)
        {
            AssertSetup();
            bool shouldBeat = _heartbeat.Tick(healthFraction, _transitionProgress, deltaTime);
            _heartbeatSource.volume = _heartbeat.Volume * _config.SfxVolume * _masterVolume;
            _heartbeatSource.pitch = _heartbeat.Pitch;

            if (_heartbeat.IsActive == false)
            {
                _heartbeatSource.Stop();
                return;
            }

            if (shouldBeat)
            {
                _heartbeatSource.Play();
            }
        }

        public void Step(float distance)
        {
            AssertSetup();

            _stepDistanceAccumulator += Mathf.Max(0f, distance);

            if (_stepDistanceAccumulator < _config.FootstepDistance)
            {
                return;
            }

            _stepDistanceAccumulator -= _config.FootstepDistance;

            PlayCue(_footstepCueIndex, Vector3.zero);
        }

        private void OnDisable()
        {
            if (_isSetupFinished == false)
            {
                return;
            }

            StopAllAudio();
        }

        private void StopAllAudio()
        {
            _isMusicPlaying = false;
            _music.Stop();
            _heartbeatSource.Stop();

            for (int sourceIndex = 0; sourceIndex < _sourceCount; sourceIndex++)
            {
                _sfxSources[sourceIndex].Stop();
                _sourceCueIndices[sourceIndex] = -1;
                _sourceBaseVolumes[sourceIndex] = 0f;
            }

        }

        private void BuildCueLookup(AudioCueData[] cues)
        {
            _cueIndexById.Clear();
            int cueCount = cues.Length;

            for (int cueIndex = 0; cueIndex < cueCount; cueIndex++)
            {
                AudioCueData cue = cues[cueIndex];

                if (string.IsNullOrEmpty(cue.CueId))
                {
                    throw new InvalidOperationException(
                        $"{name}: audio config cue at index {cueIndex} has an empty id.");
                }

                if (cue.Clip == null)
                {
                    throw new InvalidOperationException(
                        $"{name}: audio cue '{cue.CueId}' has no clip assigned.");
                }

                if (_cueIndexById.ContainsKey(cue.CueId))
                {
                    throw new InvalidOperationException(
                        $"{name}: audio config contains a duplicate cue id: {cue.CueId}.");
                }

                _cueIndexById.Add(cue.CueId, cueIndex);
            }

            if (_cueIndexById.ContainsKey(HeartbeatCueId) == false)
            {
                throw new InvalidOperationException($"{name}: audio config has no '{HeartbeatCueId}' cue.");
            }

            if (_cueIndexById.ContainsKey(FootstepCueId) == false)
            {
                throw new InvalidOperationException($"{name}: audio config has no '{FootstepCueId}' cue.");
            }

            _heartbeatCueIndex = _cueIndexById[HeartbeatCueId];
            _footstepCueIndex = _cueIndexById[FootstepCueId];
        }

        private int ResolveCueIndex(string cueId)
        {
            if (string.IsNullOrEmpty(cueId))
            {
                throw new ArgumentException("Audio cue id must not be null or empty.", nameof(cueId));
            }

            if (_cueIndexById.TryGetValue(cueId, out int cueIndex) == false)
            {
                throw new InvalidOperationException($"{name}: audio cue '{cueId}' is not present in audio config.");
            }

            return cueIndex;
        }

        private void PlayCue(int cueIndex, Vector3 position)
        {
            AudioCueData cue = _cues[cueIndex];
            int activeVoices = 0;

            for (int sourceIndex = 0; sourceIndex < _sourceCount; sourceIndex++)
            {
                if (_sourceCueIndices[sourceIndex] == cueIndex && _sfxSources[sourceIndex].isPlaying)
                {
                    activeVoices++;
                }
            }

            if (activeVoices >= _config.MaxVoicesPerCue)
            {
                return;
            }

            int availableSource = -1;

            for (int sourceIndex = 0; sourceIndex < _sourceCount; sourceIndex++)
            {
                if (_sfxSources[sourceIndex].isPlaying)
                {
                    continue;
                }

                availableSource = sourceIndex;
                break;
            }

            if (availableSource < 0)
            {
                if (cue.IsCritical == false)
                {
                    return;
                }

                availableSource = 0;
            }

            PlayOnSource(availableSource, cueIndex, cue, position);
        }

        private void PlayOnSource(int sourceIndex, int cueIndex, AudioCueData cue, Vector3 position)
        {
            _sourceCueIndices[sourceIndex] = cueIndex;
            _sourceBaseVolumes[sourceIndex] = cue.Volume * _config.SfxVolume;

            AudioSource source = _sfxSources[sourceIndex];
            float pitchVariation = cue.PitchVariation;

            source.spatialBlend = cue.SpatialBlend;
            source.minDistance = cue.MinDistance;
            source.maxDistance = cue.MaxDistance;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.ignoreListenerPause = cue.IgnoreListenerPause;
            source.clip = cue.Clip;
            source.volume = _sourceBaseVolumes[sourceIndex] * _masterVolume;
            source.pitch = BasePitch + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            source.transform.position = position;
            source.Play();
        }

        private float ComputeMusicTargetVolume()
        {
            float musicDuck = Mathf.Lerp(1f, _config.HeartbeatMusicDuck, _heartbeat.Intensity);
            return _masterVolume * _config.MusicVolume * musicDuck;
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: game audio is not initialized, call Setup first.");
            }
        }
    }
}
