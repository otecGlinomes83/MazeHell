using TMPro;
using UnityEngine;

namespace MazeHell.UI
{
    public class NarrativePopup : BaseWindow
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private TMP_Text _continueText;
        [Min(1f)] [SerializeField] private float _charactersPerSecond = 40f;
        private HealthPortrait _portrait;
        private float _revealedCharacters;

        public void Setup(string text, HealthPortrait portrait, string continueText)
        {
            _text.text = text;
            _continueText.text = continueText;
            _text.maxVisibleCharacters = 0;
            _revealedCharacters = 0f;
            _portrait = portrait;
            _portrait.SetSpeaking(true);
        }

        public void Tick(float deltaTime)
        {
            _revealedCharacters += Mathf.Max(0f, deltaTime) * _charactersPerSecond;
            _text.maxVisibleCharacters = Mathf.FloorToInt(_revealedCharacters);

            if (_revealedCharacters >= _text.text.Length && _portrait != null)
            {
                _portrait.SetSpeaking(false);
                _portrait = null;
            }
        }

        private void OnDisable()
        {
            if (_portrait != null)
            {
                _portrait.SetSpeaking(false);
                _portrait = null;
            }
        }
    }
}
