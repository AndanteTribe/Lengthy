using Lengthy;
using UnityEngine;
using UnityEngine.UIElements;

namespace DefaultNamespace
{
    public class Test : MonoBehaviour
    {
        [SerializeField]
        private TextAsset _textAsset;

        [SerializeField]
        private StyleSheet _styleSheet;

        private async void Start()
        {
            var root = FindFirstObjectByType<UIDocument>().rootVisualElement;

            await LengthyView.ShowAsync(_textAsset, root, _styleSheet, title: "Test Title");
        }
    }
}
