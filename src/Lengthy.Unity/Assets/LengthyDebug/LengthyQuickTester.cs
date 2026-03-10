using UnityEngine;

namespace LengthyDebug
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Lengthy/Quick Tester")]
    public class LengthyQuickTester : MonoBehaviour
    {
        [SerializeField]
        private Lengthy _target;

        [SerializeField]
        private TextAsset _textAsset;

        [SerializeField]
        private string _streamingAssetFileName = "Lengthy/sample.txt";

        [SerializeField]
        private string _title = "Lengthy Quick Test";

        [SerializeField]
        private bool _destroyPreviousInstance = true;

        [SerializeField, HideInInspector]
        private Lengthy _lastOpened;

        public Lengthy ShowFromTextAsset()
        {
            if (!ValidateTarget())
            {
                return null;
            }

            if (_destroyPreviousInstance)
            {
                DestroyLastOpened();
            }

            _lastOpened = _target.Show(_textAsset, _title);
            LogResult("Show(TextAsset)", _textAsset != null ? _textAsset.name : "null", _lastOpened);
            return _lastOpened;
        }

        public Lengthy ShowFromFileName()
        {
            if (!ValidateTarget())
            {
                return null;
            }

            if (_destroyPreviousInstance)
            {
                DestroyLastOpened();
            }

            _lastOpened = _target.Show(_streamingAssetFileName, _title);
            LogResult("Show(fileName)", _streamingAssetFileName, _lastOpened);
            return _lastOpened;
        }

        public void DestroyLastOpened()
        {
            if (_lastOpened == null)
            {
                return;
            }

            var instance = _lastOpened;
            _lastOpened = null;

            // 単一インスタンス運用では Lengthy 自体は残し、表示だけ閉じる。
            if (_target != null && instance == _target)
            {
                _target.Hide();
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(instance.gameObject);
                return;
            }

            DestroyImmediate(instance.gameObject);
        }

        [ContextMenu("Lengthy/Test Show(TextAsset)")]
        private void ContextShowFromTextAsset()
        {
            if (CanRunContextAction())
            {
                ShowFromTextAsset();
            }
        }

        [ContextMenu("Lengthy/Test Show(fileName)")]
        private void ContextShowFromFileName()
        {
            if (CanRunContextAction())
            {
                ShowFromFileName();
            }
        }

        [ContextMenu("Lengthy/Destroy Last Opened")]
        private void ContextDestroyLastOpened()
        {
            DestroyLastOpened();
        }

        private void Reset()
        {
            _target = GetComponent<Lengthy>();
        }

        private void OnValidate()
        {
            if (_target == null)
            {
                _target = GetComponent<Lengthy>();
            }
        }

        private bool ValidateTarget()
        {
            if (_target == null)
            {
                _target = GetComponent<Lengthy>();
            }

            if (_target != null)
            {
                return true;
            }

            Debug.LogWarning("LengthyQuickTester: Target に Lengthy コンポーネントを割り当ててください。", this);
            return false;
        }

        private bool CanRunContextAction()
        {
            if (Application.isPlaying)
            {
                return true;
            }

            Debug.LogWarning("LengthyQuickTester: Play モード中に実行してください。", this);
            return false;
        }

        private void LogResult(string label, string source, Lengthy instance)
        {
            if (instance == null)
            {
                Debug.LogWarning($"LengthyQuickTester: {label} に失敗しました。source={source}", this);
                return;
            }

            Debug.Log($"LengthyQuickTester: {label} を実行しました。source={source}, instance={instance.name}", this);
        }
    }
}
