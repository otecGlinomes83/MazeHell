using MazeHell.Session;
using MazeHell.UI;
using NUnit.Framework;
using UnityEngine;

namespace MazeHell.Tests
{
    public class AudioSettingsTests
    {
        [Test]
        public void VolumeSlidersExposeIndependentValuesAndSilentRefresh()
        {
            GameObject root = new GameObject("VolumeSlider", typeof(RectTransform));

            try
            {
                VolumeSlider slider = root.AddComponent<VolumeSlider>();
                int changes = 0;
                slider.Changed += OnChanged;
                slider.SetValueWithoutNotify(0.25f);
                Assert.AreEqual(0, changes);
                slider.value = 0.75f;
                Assert.AreEqual(1, changes);
                Assert.AreEqual(0.75f, slider.value);

                void OnChanged(float value)
                {
                    changes++;
                }
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SavedCategoryVolumesRemainIndependentOfMasterMute()
        {
            RecordStorage original = new RecordStorage(1f, 0.12f);
            float music = original.MusicVolume;
            float sfx = original.SfxVolume;
            float master = original.Volume;

            try
            {
                original.SetMusicVolume(0.25f);
                original.SetSfxVolume(0.75f);
                original.SetVolume(0f);
                RecordStorage restored = new RecordStorage(1f, 0.12f);
                Assert.AreEqual(0.25f, restored.MusicVolume);
                Assert.AreEqual(0.75f, restored.SfxVolume);
                Assert.AreEqual(0f, restored.Volume);
                restored.SetMusicVolume(2f);
                restored.SetSfxVolume(-1f);
                Assert.AreEqual(1f, restored.MusicVolume);
                Assert.AreEqual(0f, restored.SfxVolume);
            }
            finally
            {
                original.SetMusicVolume(music);
                original.SetSfxVolume(sfx);
                original.SetVolume(master);
            }
        }
    }
}
