using System;
using UnityEngine.UI;

namespace MazeHell.UI
{
    public class VolumeSlider : Slider
    {
        public event Action<float> Changed;

        protected override void Set(float input, bool sendCallback = true)
        {
            float previousValue = value;
            base.Set(input, false);

            if (sendCallback && value != previousValue)
            {
                Changed?.Invoke(value);
            }
        }
    }
}
