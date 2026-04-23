using UnityEngine;
using UnityEngine.UIElements;

namespace DefaultNamespace
{
    public class Test : MonoBehaviour
    {
        [SerializeField]
        private UIDocument _uiDocument;
        [SerializeField]
        private StyleSheet _lengthyUss;
        [SerializeField]
        private TextAsset _textAsset;

        private void Start()
        {
            var root = _uiDocument.rootVisualElement;
            root.styleSheets.Add(_lengthyUss);
            root.Add(new Lengthy.LengthyView(_textAsset, title: "利用規約など"));
        }
    }
}