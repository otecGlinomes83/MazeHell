using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MazeHell.UI
{
    [DisallowMultipleComponent]
    public class MenuClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
        ISubmitHandler
    {
        private Graphic _graphic;

        private Color _normalColor;

        private Color _hoverColor;

        private bool _isSetupFinished;

        public event Action<MenuClick> Clicked;

        public void Setup(Graphic graphic, Color normalColor, Color hoverColor)
        {
            if (graphic == null)
            {
                throw new ArgumentNullException(nameof(graphic));
            }

            _graphic = graphic;
            _normalColor = normalColor;
            _hoverColor = hoverColor;

            graphic.color = normalColor;
            _isSetupFinished = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            RaiseClick();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            AssertSetup();

            _graphic.color = _hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            AssertSetup();

            _graphic.color = _normalColor;
        }

        public void OnSubmit(BaseEventData eventData)
        {
            RaiseClick();
        }

        private void RaiseClick()
        {
            AssertSetup();

            Clicked?.Invoke(this);
        }

        private void OnDisable()
        {
            if (_isSetupFinished == false)
            {
                return;
            }

            _graphic.color = _normalColor;
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: menu click is not initialized, call Setup first.");
            }
        }
    }
}
