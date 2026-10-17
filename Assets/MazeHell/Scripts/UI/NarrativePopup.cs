using TMPro;
using UnityEngine;

namespace MazeHell.UI
{
    public class NarrativePopup : BaseWindow
    {
        [SerializeField] private TMP_Text _text;

        public void Setup(string text)
        {
            _text.text = text;
        }
    }
}
