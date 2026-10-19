#if UNITY_EDITOR
using System.Collections;
using MazeHell.Audio;
using MazeHell.Configuration;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace MazeHell.Tests
{
    public class GameAudioTests
    {
        private GameObject _root;
        private AudioConfig _config;
        private GameAudio _audio;
        private AudioSource _music;
        private AudioSource _sfx;

        [SetUp]
        public void SetUp()
        {
            _config = AssetDatabase.LoadAssetAtPath<AudioConfig>("Assets/MazeHell/Scriptables/Audio/Audio.asset");
            _root = new GameObject("GameAudioTests");
            _root.AddComponent<AudioListener>();
            _music = _root.AddComponent<AudioSource>();
            _sfx = _root.AddComponent<AudioSource>();
            _audio = _root.AddComponent<GameAudio>();
            _audio.Setup(_config, _music, new AudioSource[] { _sfx });
        }

        [TearDown]
        public void TearDown()
        {
            _config.AudioMixer.SetFloat("MusicVolume", 0f);
            _config.AudioMixer.SetFloat("SFXVolume", 0f);
            Object.DestroyImmediate(_root);
        }

        [UnityTest]
        public IEnumerator MixerRoutesAndControlsMusicAndEffectsIndependently()
        {
            yield return null;
            _audio.SetMusicVolume(0f);
            _audio.SetSfxVolume(0.75f);
            _audio.Tick(0f);
            Assert.AreSame(_config.MusicMixerGroup, _music.outputAudioMixerGroup);
            Assert.AreSame(_config.SfxMixerGroup, _sfx.outputAudioMixerGroup);
            Assert.IsTrue(_config.AudioMixer.GetFloat("MusicVolume", out float musicDecibels));
            Assert.IsTrue(_config.AudioMixer.GetFloat("SFXVolume", out float sfxDecibels));
            Assert.AreEqual(-80f, musicDecibels, 0.01f);
            Assert.AreEqual(Mathf.Log10(0.75f) * 20f, sfxDecibels, 0.01f);
            _audio.SetSfxVolume(0f);
            _audio.SetMusicVolume(0.5f);
            _audio.Tick(0f);
            _config.AudioMixer.GetFloat("SFXVolume", out sfxDecibels);
            _config.AudioMixer.GetFloat("MusicVolume", out musicDecibels);
            Assert.AreEqual(-80f, sfxDecibels, 0.01f);
            Assert.AreEqual(Mathf.Log10(0.5f) * 20f, musicDecibels, 0.01f);
        }

        [UnityTest]
        public IEnumerator CriticalHeartbeatHasDedicatedVoiceAndDucksMusic()
        {
            yield return null;
            _audio.StartMusic();
            _audio.Heartbeat(1f, 0f);
            _audio.Tick(10f);
            float normalMusicVolume = _music.volume;
            AudioSource[] sources = _root.GetComponents<AudioSource>();
            AudioSource heartbeat = sources[sources.Length - 1];
            Assert.AreNotSame(_sfx, heartbeat);
            Assert.AreNotSame(_music, heartbeat);
            Assert.AreSame(_config.SfxMixerGroup, heartbeat.outputAudioMixerGroup);
            _audio.Heartbeat(0.29f, 0f);
            float quietVolume = heartbeat.volume;
            float slowPitch = heartbeat.pitch;
            _audio.Heartbeat(0.01f, 0f);
            _audio.Tick(10f);
            Assert.Greater(heartbeat.volume, quietVolume);
            Assert.Greater(heartbeat.pitch, slowPitch);
            Assert.Less(_music.volume, normalMusicVolume);
            _audio.Heartbeat(1f, 0f);
            _audio.Tick(10f);
            Assert.AreEqual(normalMusicVolume, _music.volume, 0.001f);
            Assert.IsFalse(heartbeat.isPlaying);
            _audio.SetTransition(1f);
            _audio.Heartbeat(1f, 0f);
            _audio.Tick(10f);
            Assert.Less(_music.volume, normalMusicVolume);
        }
    }
}
#endif
