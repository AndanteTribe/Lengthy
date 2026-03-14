#nullable enable

using UnityEngine;
using UnityEngine.UIElements;
using Lengthy;

namespace LengthyDebug
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Lengthy/Quick Tester")]
    public class LengthyQuickTester : MonoBehaviour
    {
        [SerializeField]
        private LengthyPresenter? _presenterPrefab;

        [SerializeField]
        private TextAsset? _textAsset;

        [SerializeField]
        private string _title = "Lengthy Quick Test";

        [SerializeField]
        private PanelSettings? _panelSettings;

        [SerializeField]
        private StyleSheet? _styleSheet;

        [SerializeField, HideInInspector]
        private LengthyPresenter? _lastOpened;

        /// <summary>
        /// Minimal example flow:
        /// Instantiate presenter -> optional Configure -> TryShow.
        /// </summary>
        public LengthyPresenter? ShowFromTextAsset()
        {
            if (!ValidateSetup())
            {
                return null;
            }

            CloseLastOpened();

            var presenter = Instantiate(_presenterPrefab!);
            ConfigurePresenter(presenter);

            if (!presenter.TryShow(_textAsset!, out var error, _title))
            {
                Debug.LogWarning($"LengthyQuickTester: Show failed. {error}", this);
                Destroy(presenter.gameObject);
                return null;
            }

            _lastOpened = presenter;
            Debug.Log($"LengthyQuickTester: Show succeeded. source={_textAsset!.name}, instance={presenter.name}", this);
            return presenter;
        }

        private void ConfigurePresenter(LengthyPresenter presenter)
        {
            if (_panelSettings != null && _styleSheet != null)
            {
                presenter.Configure(_panelSettings, _styleSheet);
                return;
            }

            if (_panelSettings != null)
            {
                presenter.Configure(_panelSettings);
            }

            if (_styleSheet != null)
            {
                presenter.Configure(_styleSheet);
            }
        }

        public void CloseLastOpened()
        {
            if (_lastOpened == null)
            {
                return;
            }

            var instance = _lastOpened;
            _lastOpened = null;

            if (instance != null)
            {
                instance.Hide();
            }
        }

        private bool ValidateSetup()
        {
            if (_presenterPrefab == null)
            {
                Debug.LogWarning("LengthyQuickTester: Assign Presenter Prefab.", this);
                return false;
            }

            if (_textAsset == null)
            {
                Debug.LogWarning("LengthyQuickTester: Assign Text Asset.", this);
                return false;
            }

            return true;
        }
    }
}
