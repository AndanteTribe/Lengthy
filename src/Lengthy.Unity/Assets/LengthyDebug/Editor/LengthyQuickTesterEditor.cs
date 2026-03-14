#nullable enable

using UnityEditor;
using UnityEngine;

namespace LengthyDebug.Editor
{
    [CustomEditor(typeof(LengthyQuickTester))]
    public class LengthyQuickTesterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Usage Example", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This sample performs: Instantiate presenter -> optional Configure(panel/style) -> TryShow(text, out error, title).\n" +
                "Use Hide Last Presenter to close the currently opened sample instance.",
                MessageType.Info);

            DrawRuntimeButtons((LengthyQuickTester)target);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRuntimeButtons(LengthyQuickTester tester)
        {
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Show With TryShow"))
                {
                    tester.ShowFromTextAsset();
                }

                if (GUILayout.Button("Hide Last Presenter"))
                {
                    tester.CloseLastOpened();
                }
            }
        }
    }
}
