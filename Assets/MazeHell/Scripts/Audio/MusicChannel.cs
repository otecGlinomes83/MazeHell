using System;
using UnityEngine;

namespace MazeHell.Audio
{
    public class MusicChannel
    {
        private AudioSource[] _sources;
        private AudioClip[] _tracks;
        private float _fadeInDuration;
        private float _crossfadeDuration;
        private float _fadeProgress;
        private int _activeIndex;
        private int _trackIndex;
        private bool _isCrossfading;

        public MusicChannel(AudioSource firstSource, AudioSource secondSource, float fadeInDuration,
            float crossfadeDuration)
        {
            if (firstSource == null || secondSource == null)
            {
                throw new ArgumentNullException(nameof(firstSource));
            }

            _sources = new AudioSource[] { firstSource, secondSource };
            _fadeInDuration = Mathf.Max(0.01f, fadeInDuration);
            _crossfadeDuration = Mathf.Max(0.01f, crossfadeDuration);

            foreach (AudioSource source in _sources)
            {
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.ignoreListenerPause = false;
            }
        }

        public void Play(AudioClip[] tracks)
        {
            if (tracks == null || tracks.Length == 0)
            {
                throw new ArgumentException("Music playlist must contain tracks.", nameof(tracks));
            }

            foreach (AudioClip clip in tracks)
            {
                if (clip == null)
                {
                    throw new ArgumentException("Music playlist contains a missing clip.", nameof(tracks));
                }
            }

            if (_tracks == tracks)
            {
                return;
            }

            Stop();
            _tracks = tracks;
            _activeIndex = 0;
            _trackIndex = 0;
            StartTrack();
        }

        public void Tick(float deltaTime, float volume)
        {
            if (_tracks == null || AudioListener.pause)
            {
                return;
            }

            AudioSource activeSource = _sources[_activeIndex];

            if (_tracks.Length > 1 && _fadeProgress >= 1f)
            {
                float remaining = 0f;

                if (activeSource.isPlaying)
                {
                    remaining = activeSource.clip.length - activeSource.time;
                }

                if (remaining <= _crossfadeDuration)
                {
                    _trackIndex = (_trackIndex + 1) % _tracks.Length;
                    _activeIndex = 1 - _activeIndex;
                    _isCrossfading = true;
                    StartTrack();
                }
            }

            float duration = _fadeInDuration;

            if (_isCrossfading)
            {
                duration = _crossfadeDuration;
            }

            _fadeProgress = Mathf.MoveTowards(_fadeProgress, 1f, Mathf.Max(0f, deltaTime) / duration);
            _sources[_activeIndex].volume = Mathf.Clamp01(volume) * _fadeProgress;
            AudioSource outgoingSource = _sources[1 - _activeIndex];

            if (_isCrossfading)
            {
                outgoingSource.volume = Mathf.Clamp01(volume) * (1f - _fadeProgress);

                if (_fadeProgress >= 1f)
                {
                    outgoingSource.Stop();
                    outgoingSource.clip = null;
                    _isCrossfading = false;
                }
            }
        }

        public void Stop()
        {
            foreach (AudioSource source in _sources)
            {
                source.Stop();
                source.clip = null;
                source.volume = 0f;
            }

            _tracks = null;
            _isCrossfading = false;
        }

        private void StartTrack()
        {
            AudioSource source = _sources[_activeIndex];
            source.Stop();
            source.clip = _tracks[_trackIndex];
            source.loop = _tracks.Length == 1;
            source.volume = 0f;
            _fadeProgress = 0f;
            source.Play();
        }
    }
}
