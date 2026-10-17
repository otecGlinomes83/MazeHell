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

        private float _heartbeatTimer;

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
            _heartbeatTimer = 0f;
            _stepDistanceAccumulator = 0f;
            _musicVolumeCurrent = 0f;
            _isMusicPlaying = false;
            _isSetupFinished = true;
        }

        public void Tick(float deltaTime)
        {
            AssertSetup();

            if (_isMusicPlaying == false)
            {
                return;
            }

            float clampedDeltaTime = Mathf.Max(0f, deltaTime);
            float targetVolume = ComputeMusicTargetVolume();
            float fadeRate = 1f / Mathf.Max(MinimalFadeDuration, _config.MusicFadeInDuration);

            _musicVolumeCurrent = Mathf.MoveTowards(_musicVolumeCurrent, targetVolume, clampedDeltaTime * fadeRate);
            _musicSource.volume = _musicVolumeCurrent;
        }

        public void StartMusic()
        {
            AssertSetup();

            if (_isMusicPlaying)
            {
                return;
            }

            _isMusicPlaying = true;
            _musicSource.loop = true;
            _musicSource.clip = _config.MusicLoopClip;
            _musicSource.volume = 0f;
            _musicVolumeCurrent = 0f;
            _musicSource.Play();
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

            float clampedFraction = Mathf.Clamp01(healthFraction);
            float clampedDeltaTime = Mathf.Max(0f, deltaTime);

            bool isHeartActive = clampedFraction < 1f || _transitionProgress > 0f;

            if (isHeartActive == false)
            {
                _heartbeatTimer = 0f;

                return;
            }

            float healthyInterval = _config.HeartbeatIntervalHealthy;
            float criticalInterval = _config.HeartbeatIntervalCritical;
            float healthInterval = Mathf.Lerp(healthyInterval, criticalInterval, 1f - clampedFraction);
            float interval = Mathf.Lerp(healthInterval, criticalInterval, _transitionProgress);

            _heartbeatTimer += clampedDeltaTime;

            if (_heartbeatTimer >= interval)
            {
                _heartbeatTimer = Mathf.Max(0f, _heartbeatTimer - interval);

                PlayCue(_heartbeatCueIndex, Vector3.zero);
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
            _musicSource.Stop();

            for (int sourceIndex = 0; sourceIndex < _sourceCount; sourceIndex++)
            {
                _sfxSources[sourceIndex].Stop();
                _sourceCueIndices[sourceIndex] = -1;
                _sourceBaseVolumes[sourceIndex] = 0f;
            }

            _heartbeatTimer = 0f;
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

            for (int sourceIndex = 0; sourceIndex < _sourceCount; sourceIndex++)
            {
                if (_sfxSources[sourceIndex].isPlaying)
                {
                    continue;
                }

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

                return;
            }
        }

        private float ComputeMusicTargetVolume()
        {
            return _masterVolume * _config.MusicVolume * (1f - _transitionProgress);
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
