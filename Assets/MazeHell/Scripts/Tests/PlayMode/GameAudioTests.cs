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
        public IEnumerator GameplayPlaylistCrossfadesAndWrapsWithoutRestartingOnPause()
        {
            yield return null;
            AudioClip first = AudioClip.Create("First", 44100 * 3, 1, 44100, false);
            AudioClip second = AudioClip.Create("Second", 44100 * 3, 1, 44100, false);
            AudioSource otherMusic = _root.AddComponent<AudioSource>();
            MusicChannel channel = new MusicChannel(_music, otherMusic, 0.1f, 0.5f);
            AudioClip[] playlist = new AudioClip[] { first, second };

            try
            {
                channel.Play(playlist);
                channel.Tick(0.1f, 1f);
                Assert.AreSame(first, _music.clip);
                Assert.IsFalse(_music.loop);
                _music.time = 2.75f;
                AudioListener.pause = true;
                channel.Tick(1f, 1f);
                Assert.IsNull(otherMusic.clip);
                AudioListener.pause = false;
                channel.Tick(0.25f, 1f);
                Assert.AreSame(first, _music.clip);
                Assert.AreSame(second, otherMusic.clip);
                Assert.IsFalse(otherMusic.loop);
                Assert.AreEqual(0.5f, _music.volume, 0.01f);
                Assert.AreEqual(0.5f, otherMusic.volume, 0.01f);
                channel.Tick(0.25f, 1f);
                Assert.IsNull(_music.clip);
                Assert.AreEqual(1f, otherMusic.volume, 0.01f);
                channel.Play(playlist);
                Assert.AreSame(second, otherMusic.clip);
                otherMusic.time = 2.75f;
                channel.Tick(0.5f, 1f);
                Assert.AreSame(first, _music.clip);
                Assert.IsNull(otherMusic.clip);
            }
            finally
            {
                AudioListener.pause = false;
                channel.Stop();
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [UnityTest]
        public IEnumerator SceneMusicUsesAssignedMenuAndGameplayTracks()
        {
            yield return null;
            _audio.StartMenuMusic();
            Assert.AreSame(_config.MenuMusicClip, _music.clip);
            Assert.AreEqual("Infernal Stone Hall", _music.clip.name);
            Assert.IsTrue(_music.loop);
            Assert.AreEqual(2, _config.GameplayMusicClips.Length);
            Assert.AreEqual("Reload Riff", _config.GameplayMusicClips[0].name);
            Assert.AreEqual("Combat Riff", _config.GameplayMusicClips[1].name);
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
