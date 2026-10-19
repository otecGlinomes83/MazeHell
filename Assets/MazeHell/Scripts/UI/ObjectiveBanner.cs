using System;
using TMPro;
using UnityEngine;

namespace MazeHell.UI
{
    public class ObjectiveBanner
    {
        private const float TransitionDuration = 0.6f;
        private const float HalfProgress = 0.5f;
        private const float TravelDistance = 110f;

        private TMP_Text _text;
        private Vector2 _basePosition;
        private float _baseAlpha;
        private string _pendingText;
        private float _progress;
        private bool _isTransitioning;

        public ObjectiveBanner(TMP_Text text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            _text = text;
            _basePosition = text.rectTransform.anchoredPosition;
            _baseAlpha = text.alpha;
        }

        public void Reset(string text)
        {
            _isTransitioning = false;
            _progress = 0f;
            _pendingText = text;
            _text.text = text;
            _text.rectTransform.anchoredPosition = _basePosition;
            _text.alpha = _baseAlpha;
        }

        public void Change(string text)
        {
            _pendingText = text;
            _progress = 0f;
            _isTransitioning = true;
        }

        public void UpdateText(string text)
        {
            _pendingText = text;

            if (_isTransitioning == false || _progress >= HalfProgress)
            {
                _text.text = text;
            }
        }

        public void Tick(float deltaTime)
        {
            if (_isTransitioning == false)
            {
                return;
            }

            _progress = Mathf.Clamp01(_progress + Mathf.Max(0f, deltaTime) / TransitionDuration);
            float hiddenFraction;

            if (_progress < HalfProgress)
            {
                hiddenFraction = Mathf.SmoothStep(0f, 1f, _progress / HalfProgress);
            }
            else
            {
                _text.text = _pendingText;
                hiddenFraction = 1f - Mathf.SmoothStep(0f, 1f, (_progress - HalfProgress) / HalfProgress);
            }

            _text.rectTransform.anchoredPosition = _basePosition + Vector2.up * TravelDistance * hiddenFraction;
            _text.alpha = _baseAlpha * (1f - hiddenFraction);

            if (_progress >= 1f)
            {
                _isTransitioning = false;
            }
        }
    }
}
