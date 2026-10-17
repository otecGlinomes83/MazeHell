using System;
using UnityEngine;

namespace MazeHell.Session
{
    [DisallowMultipleComponent]
    public class Pauser : MonoBehaviour
    {
        private const float PausedTimeScale = 0f;
        private const float NormalTimeScale = 1f;

        public event Action<bool> Changed;

        private bool _isPaused;

        public bool IsPaused => _isPaused;

        public void RequestPause()
        {
            if (_isPaused)
            {
                return;
            }

            _isPaused = true;
            Time.timeScale = PausedTimeScale;
            AudioListener.pause = true;

            Changed?.Invoke(true);
        }

        public void Resume()
        {
            if (_isPaused == false)
            {
                return;
            }

            _isPaused = false;
            Time.timeScale = NormalTimeScale;
            AudioListener.pause = false;

            Changed?.Invoke(false);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                return;
            }

            RequestPause();
        }

        private void OnDisable()
        {
            if (_isPaused == false)
            {
                return;
            }

            _isPaused = false;
            Time.timeScale = NormalTimeScale;
            AudioListener.pause = false;

            Changed?.Invoke(false);
        }
    }
}
