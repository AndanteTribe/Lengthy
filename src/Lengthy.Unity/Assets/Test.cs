using Lengthy;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;

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

        private CancellationTokenSource _cancellationTokenSource;

        private async void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            // どこかでキャンセルするのを待つ
            await LengthyViewLauncher.ShowLengthyViewAsync(_uiDocument.rootVisualElement, _lengthyUss, _textAsset,
                _cancellationTokenSource.Token, title: "Hi");
        }
    }
}