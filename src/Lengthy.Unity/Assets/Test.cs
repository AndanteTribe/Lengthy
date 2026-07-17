using Lengthy;
using UnityEngine;

namespace DefaultNamespace
{
    public class Test : MonoBehaviour
    {
        [SerializeField]
        private TextAsset _textAsset;

        private async void Start()
        {
            await LengthyViewLauncher.ShowLengthyViewAsync(_textAsset, title: "ssss");
        }
    }
}