using ImmersiveGames.FaceSystem;
using UnityEditor;
using UnityEngine;

namespace ImmersiveGames.FaceSystem.Editor
{
    [CustomEditor(typeof(FaceController))]
    public sealed class FaceControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            SerializedProperty defaultStateProperty = serializedObject.FindProperty("defaultState");
            SerializedProperty statesProperty = serializedObject.FindProperty("states");
            FaceState defaultState = defaultStateProperty != null
                ? defaultStateProperty.objectReferenceValue as FaceState
                : null;

            if (defaultState == null)
            {
                EditorGUILayout.HelpBox(
                    "A Default State is required and must also be registered in States.",
                    MessageType.Error);
            }
            else if (string.IsNullOrWhiteSpace(defaultState.Id))
            {
                EditorGUILayout.HelpBox("Default State has an empty stable ID.", MessageType.Error);
            }
            else if (!IsRegistered(statesProperty, defaultState))
            {
                EditorGUILayout.HelpBox(
                    $"Default State '{defaultState.name}' is not registered in States.",
                    MessageType.Error);
            }

            EditorGUILayout.HelpBox(
                "State and animation IDs are stable identifiers. Renaming an asset does not change its ID.",
                MessageType.Info);
        }

        private static bool IsRegistered(SerializedProperty statesProperty, FaceState candidate)
        {
            if (statesProperty == null || !statesProperty.isArray || candidate == null)
            {
                return false;
            }

            for (int i = 0; i < statesProperty.arraySize; i++)
            {
                if (statesProperty.GetArrayElementAtIndex(i).objectReferenceValue == candidate)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
