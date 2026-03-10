using UnityEditor;
using UnityEngine;

namespace LengthyDebug.Editor
{
    [CustomEditor(typeof(global::LengthyDebug.LengthyQuickTester))]
    public class LengthyQuickTesterEditor : UnityEditor.Editor
    {
        private const string SampleTextAssetPath = "Assets/LengthyDebug/LengthySampleText.txt";
        private const string DefaultStreamingAssetFileName = "Lengthy/sample.txt";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Play モード中にボタンを押すと、Lengthy の各 Show メソッドをその場で試せます。", MessageType.Info);

            DrawAutoSetupButton();
            DrawRuntimeButtons((global::LengthyDebug.LengthyQuickTester)target);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAutoSetupButton()
        {
            if (!GUILayout.Button("サンプル設定を自動入力"))
            {
                return;
            }

            var textProperty = serializedObject.FindProperty("_textAsset");
            var fileNameProperty = serializedObject.FindProperty("_streamingAssetFileName");
            var sampleTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(SampleTextAssetPath);

            if (sampleTextAsset != null)
            {
                textProperty.objectReferenceValue = sampleTextAsset;
            }

            fileNameProperty.stringValue = DefaultStreamingAssetFileName;
        }

        private void DrawRuntimeButtons(LengthyQuickTester tester)
        {
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Show(TextAsset) を実行"))
                {
                    tester.ShowFromTextAsset();
                }

                if (GUILayout.Button("Show(fileName) を実行"))
                {
                    tester.ShowFromFileName();
                }
            }

            if (GUILayout.Button("最後に開いた Lengthy を閉じる"))
            {
                tester.DestroyLastOpened();
            }
        }
    }
}
