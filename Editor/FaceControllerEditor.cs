using System.Collections.Generic;
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
            DrawPropertiesExcluding(serializedObject, "m_Script", "trackBindings");
            DrawTrackBindings(
                serializedObject.FindProperty("trackBindings"),
                serializedObject.FindProperty("states"));

            SerializedProperty defaultStateForIdentity = serializedObject.FindProperty("defaultState");
            FaceEditorUtility.EnsureAssetId(
                defaultStateForIdentity != null ? defaultStateForIdentity.objectReferenceValue : null);
            int repairedStateIds = FaceEditorUtility.EnsureUniqueAssetIds(
                serializedObject.FindProperty("states"));
            int repairedAnimationIds = FaceEditorUtility.EnsureUniqueAssetIds(
                serializedObject.FindProperty("animations"));
            serializedObject.ApplyModifiedProperties();

            if (repairedStateIds + repairedAnimationIds > 0)
            {
                EditorGUILayout.HelpBox(
                    "Duplicate State or Animation keys were regenerated. Use asset references in code so copied assets remain unambiguous.",
                    MessageType.Warning);
            }

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
                EditorGUILayout.HelpBox("Default State identity is not initialized. Reopen its asset Inspector.", MessageType.Error);
            }
            else if (!IsRegistered(statesProperty, defaultState))
            {
                EditorGUILayout.HelpBox(
                    $"Default State '{defaultState.name}' is not registered in States.",
                    MessageType.Error);
            }

            EditorGUILayout.HelpBox(
                "Track bindings select named tracks. Their internal keys are generated and hidden.",
                MessageType.Info);
        }

        private static void DrawTrackBindings(
            SerializedProperty bindingsProperty,
            SerializedProperty statesProperty)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Track Bindings", EditorStyles.boldLabel);

            if (bindingsProperty == null || !bindingsProperty.isArray)
            {
                return;
            }

            GetTrackOptions(statesProperty, out string[] trackIds, out string[] trackLabels);

            for (int i = 0; i < bindingsProperty.arraySize; i++)
            {
                SerializedProperty binding = bindingsProperty.GetArrayElementAtIndex(i);
                SerializedProperty trackId = binding.FindPropertyRelative("trackId");
                SerializedProperty targetRenderer = binding.FindPropertyRelative("targetRenderer");
                SerializedProperty materialIndex = binding.FindPropertyRelative("materialIndex");

                int labelIndex = System.Array.IndexOf(trackIds, trackId.stringValue);
                string title = labelIndex >= 0
                    ? trackLabels[labelIndex]
                    : $"Track Binding {i + 1}";
                binding.isExpanded = EditorGUILayout.Foldout(binding.isExpanded, title, true);

                if (!binding.isExpanded)
                {
                    continue;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                FaceEditorUtility.DrawIdPopup(trackId, "Track", trackIds, trackLabels);
                EditorGUILayout.PropertyField(targetRenderer);
                EditorGUILayout.PropertyField(materialIndex);

                if (FaceEditorUtility.DrawRemoveButton("Remove Binding"))
                {
                    EditorGUILayout.EndVertical();
                    bindingsProperty.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Track Binding"))
            {
                int index = bindingsProperty.arraySize;
                bindingsProperty.InsertArrayElementAtIndex(index);
                SerializedProperty binding = bindingsProperty.GetArrayElementAtIndex(index);
                binding.FindPropertyRelative("trackId").stringValue =
                    trackIds.Length > 0 ? trackIds[0] : string.Empty;
                binding.FindPropertyRelative("targetRenderer").objectReferenceValue = null;
                binding.FindPropertyRelative("materialIndex").intValue = 0;
                binding.isExpanded = true;
            }
        }

        private static void GetTrackOptions(
            SerializedProperty statesProperty,
            out string[] ids,
            out string[] labels)
        {
            List<string> idList = new List<string>();
            List<string> labelList = new List<string>();

            if (statesProperty != null && statesProperty.isArray)
            {
                for (int i = 0; i < statesProperty.arraySize; i++)
                {
                    FaceState state = statesProperty
                        .GetArrayElementAtIndex(i)
                        .objectReferenceValue as FaceState;
                    FaceSpriteLibrary library = state != null ? state.SpriteLibrary : null;

                    if (library == null)
                    {
                        continue;
                    }

                    for (int trackIndex = 0; trackIndex < library.Tracks.Count; trackIndex++)
                    {
                        FaceSpriteLibrary.SpriteTrack track = library.Tracks[trackIndex];

                        if (track != null
                            && !string.IsNullOrWhiteSpace(track.TrackId)
                            && !idList.Contains(track.TrackId))
                        {
                            idList.Add(track.TrackId);
                            labelList.Add(FaceEditorUtility.GetTrackLabel(library, track.TrackId));
                        }
                    }
                }
            }

            ids = idList.ToArray();
            labels = labelList.ToArray();
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
