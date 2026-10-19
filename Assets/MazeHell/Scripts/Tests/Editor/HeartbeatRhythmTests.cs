using MazeHell.Audio;
using MazeHell.Configuration;
using NUnit.Framework;
using UnityEngine;

namespace MazeHell.Tests
{
    public class HeartbeatRhythmTests
    {
        private AudioConfig _config;
        private HeartbeatRhythm _rhythm;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<AudioConfig>();
            _rhythm = new HeartbeatRhythm(_config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [TestCase(1f)]
        [TestCase(0.5f)]
        [TestCase(0.3f)]
        [TestCase(0f)]
        public void HealthyOrDeadPlayerHasNoHeartbeatOutsideTransition(float health)
        {
            Assert.IsFalse(_rhythm.Tick(health, 0f, 10f));
            Assert.IsFalse(_rhythm.IsActive);
        }

        [Test]
        public void FallingHealthBelowThresholdMakesHeartbeatLouderAndFaster()
        {
            Assert.IsTrue(_rhythm.Tick(0.29f, 0f, 0f));
            float interval = _rhythm.Interval;
            float volume = _rhythm.Volume;
            float pitch = _rhythm.Pitch;
            _rhythm.Tick(0.05f, 0f, 0f);
            Assert.Less(_rhythm.Interval, interval);
            Assert.Greater(_rhythm.Volume, volume);
            Assert.Greater(_rhythm.Pitch, pitch);
            Assert.IsFalse(_rhythm.Tick(0.7f, 0f, 0.5f));
            Assert.IsFalse(_rhythm.IsActive);
        }

        [Test]
        public void FinalCorridorActivatesAndAcceleratesHeartbeatAtFullHealth()
        {
            Assert.IsTrue(_rhythm.Tick(1f, 0.1f, 0f));
            float interval = _rhythm.Interval;
            float volume = _rhythm.Volume;
            _rhythm.Tick(1f, 1f, 0f);
            Assert.Less(_rhythm.Interval, interval);
            Assert.Greater(_rhythm.Volume, volume);
            Assert.AreEqual(1f, _rhythm.Intensity);
            Assert.IsFalse(_rhythm.Tick(1f, 0f, 1f));
        }
    }
}
