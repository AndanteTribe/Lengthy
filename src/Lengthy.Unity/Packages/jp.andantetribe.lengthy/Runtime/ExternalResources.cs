#nullable enable

using UnityEngine;
using UnityEngine.UIElements;

namespace Lengthy
{
    internal sealed class ExternalResources: ScriptableObject
    {
        [SerializeField]
        private StyleSheet? _styleSheet;

        public const string RootPath = "Packages/jp.andantetribe.lengthy/ExternalResources";

#if !UNITY_EDITOR
        private static ExternalResources s_instance;

        private void OnEnable()
        {
            s_instance = this;
        }
#endif

        public static StyleSheet LoadStyleSheet()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RootPath + "/LengthyDocument.uxml").CloneTree().styleSheets[0];
#else
            return s_instance._styleSheet;
#endif
        }
    }
}