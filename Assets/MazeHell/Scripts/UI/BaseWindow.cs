using System;
using UnityEngine;

namespace MazeHell.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BaseWindow : MonoBehaviour
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private bool _blocksInput = true;
        [Min(0.01f)] [SerializeField] private float _showDuration = 0.16f;
        [Min(0.01f)] [SerializeField] private float _closeDuration = 0.12f;
        private CanvasGroup _group;
        private float _elapsed;
        private bool _isClosing;
        private bool _isClosed;

        public event Action<BaseWindow> CloseRequested;
        public event Action<BaseWindow> Closed;
        public CanvasGroup Group => _group;

        protected virtual void Awake()
        {
            if (TryGetComponent(out _group) == false)
            {
                throw new InvalidOperationException($"{name}: window requires CanvasGroup.");
            }
        }

        public void Show()
        {
            _elapsed = 0f;
            _isClosing = false;
            _isClosed = false;
            _group.alpha = 0f;
            _group.interactable = _blocksInput;
            _group.blocksRaycasts = _blocksInput;
        }

        public void RequestClose()
        {
            if (_isClosing == false && _isClosed == false)
            {
                CloseRequested?.Invoke(this);
            }
        }

        public void BeginClose()
        {
            if (_isClosing || _isClosed)
            {
                return;
            }

            _isClosing = true;
            _elapsed = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        private void Update()
        {
            if (_isClosed)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(_elapsed / _showDuration);

            if (_isClosing)
            {
                progress = 1f - Mathf.Clamp01(_elapsed / _closeDuration);
            }

            _group.alpha = progress;

            if (_content != null)
            {
                _content.localScale = Vector3.one * Mathf.Lerp(0.96f, 1f, progress);
            }

            if (_isClosing && progress <= 0f)
            {
                _isClosed = true;
                Closed?.Invoke(this);
            }
        }
    }
}
