using System;
using UnityEngine;

namespace MazeHell.UI
{
    [DisallowMultipleComponent]
    public class SafeArea : MonoBehaviour
    {
        private RectTransform _safeRoot;

        private Rect _lastAppliedSafeArea;

        private int _lastAppliedScreenWidth;

        private int _lastAppliedScreenHeight;

        private bool _isSetupFinished;

        public void Setup(RectTransform safeRoot)
        {
            if (safeRoot == null)
            {
                throw new ArgumentNullException(nameof(safeRoot));
            }

            _safeRoot = safeRoot;
            _isSetupFinished = true;

            _lastAppliedSafeArea = new Rect(0f, 0f, 0f, 0f);
            _lastAppliedScreenWidth = 0;
            _lastAppliedScreenHeight = 0;

            ApplyIfChanged();
        }

        private void Update()
        {
            if (_isSetupFinished == false)
            {
                return;
            }

            ApplyIfChanged();
        }

        private void ApplyIfChanged()
        {
            Rect safeArea = Screen.safeArea;
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;

            if (safeArea == _lastAppliedSafeArea && screenWidth == _lastAppliedScreenWidth &&
                screenHeight == _lastAppliedScreenHeight)
            {
                return;
            }

            if (screenWidth <= 0 || screenHeight <= 0)
            {
                return;
            }

            float anchorMinX = safeArea.xMin / screenWidth;
            float anchorMinY = safeArea.yMin / screenHeight;
            float anchorMaxX = safeArea.xMax / screenWidth;
            float anchorMaxY = safeArea.yMax / screenHeight;

            _safeRoot.anchorMin = new Vector2(Mathf.Clamp01(anchorMinX), Mathf.Clamp01(anchorMinY));
            _safeRoot.anchorMax = new Vector2(Mathf.Clamp01(anchorMaxX), Mathf.Clamp01(anchorMaxY));

            _lastAppliedSafeArea = safeArea;
            _lastAppliedScreenWidth = screenWidth;
            _lastAppliedScreenHeight = screenHeight;
        }
    }
}
