using System;
using System.Collections.Generic;
using UnityEngine;

namespace MazeHell.UI
{
    public class MenuPopupStack
    {
        private List<BaseWindow> _windows = new List<BaseWindow>();
        private CanvasGroup _screen;

        public MenuPopupStack(CanvasGroup screen)
        {
            _screen = screen;
        }

        public void Push(BaseWindow window)
        {
            if (_windows.Contains(window))
            {
                throw new InvalidOperationException("Popup is already open.");
            }

            if (_windows.Count > 0)
            {
                SetInteraction(_windows[_windows.Count - 1].Group, false);
            }

            SetInteraction(_screen, false);
            _windows.Add(window);
        }

        public void Pop(BaseWindow window)
        {
            if (_windows.Count == 0 || _windows[_windows.Count - 1] != window)
            {
                throw new InvalidOperationException("Only the top popup can be released.");
            }

            _windows.RemoveAt(_windows.Count - 1);

            if (_windows.Count > 0)
            {
                SetInteraction(_windows[_windows.Count - 1].Group, true);
                return;
            }

            SetInteraction(_screen, true);
        }

        private void SetInteraction(CanvasGroup group, bool enabled)
        {
            if (group == null)
            {
                return;
            }

            group.interactable = enabled;
            group.blocksRaycasts = enabled;
        }
    }
}
