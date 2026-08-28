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
            var view = new LengthyView(_textAsset, title: "ssss");
            await view.ShowAsync(root);

            await view.ShowAsync(root, _styleSheet);
        }
    }
}
