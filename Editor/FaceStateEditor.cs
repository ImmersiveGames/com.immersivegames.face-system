using UnityEditor;
using UnityEngine;
using ImmersiveGames.FaceSystem;

namespace ImmersiveGames.FaceSystem.Editor
{
    [CustomEditor(typeof(FaceState))]
    public sealed class FaceStateEditor : UnityEditor.Editor
    {
        private const float PreviewSize = 52f;

        private SerializedProperty idProperty;
        private SerializedProperty spriteLibraryProperty;
        private SerializedProperty trackStatesProperty;

        private void OnEnable()
        {
            idProperty = serializedObject.FindProperty("id");
            spriteLibraryProperty = serializedObject.FindProperty("spriteLibrary");
            trackStatesProperty = serializedObject.FindProperty("trackStates");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            FaceEditorUtility.EnsureId(idProperty);
            int repairedFrameKeys = EnsureFrameKeys();

            EditorGUILayout.HelpBox(
                "This asset has an internal generated identity. Use the asset reference when selecting a state in code.",
                MessageType.Info);
            if (repairedFrameKeys > 0)
            {
                EditorGUILayout.HelpBox(
                    $"Generated new keys for {repairedFrameKeys} duplicate frames. Existing animation links still resolve to the first frame; select the intended named frame where needed.",
                    MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sprite Library", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(spriteLibraryProperty);
            FaceSpriteLibrary library = spriteLibraryProperty.objectReferenceValue as FaceSpriteLibrary;

            if (library == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a FaceSpriteLibrary to select named tracks and sprites.",
                    MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Track States", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Each Track State maps named frames to sprites and selects an explicit default frame.",
                MessageType.Info);

            for (int i = 0; i < trackStatesProperty.arraySize; i++)
            {
                if (DrawTrackState(trackStatesProperty.GetArrayElementAtIndex(i), i, library))
                {
                    trackStatesProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            if (GUILayout.Button("Add Track State"))
            {
                AddTrackState(library);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private int EnsureFrameKeys()
        {
            int repairedFrameKeys = 0;

            for (int i = 0; i < trackStatesProperty.arraySize; i++)
            {
                SerializedProperty trackState = trackStatesProperty.GetArrayElementAtIndex(i);
                SerializedProperty frames = trackState.FindPropertyRelative("frames");

                for (int frameIndex = 0; frameIndex < frames.arraySize; frameIndex++)
                {
                    SerializedProperty frame = frames.GetArrayElementAtIndex(frameIndex);
                    SerializedProperty frameId = frame.FindPropertyRelative("frameId");
                    SerializedProperty displayName = frame.FindPropertyRelative("displayName");
                    string previousId = frameId.stringValue;
                    FaceEditorUtility.EnsureDisplayName(
                        displayName,
                        previousId,
                        $"Frame {frameIndex + 1}");
                    FaceEditorUtility.EnsureId(frameId);
                }

                repairedFrameKeys += FaceEditorUtility.EnsureUniqueIds(frames, "frameId");
            }

            return repairedFrameKeys;
        }

        private static bool DrawTrackState(
            SerializedProperty trackState,
            int index,
            FaceSpriteLibrary library)
        {
            SerializedProperty trackIdProperty = trackState.FindPropertyRelative("trackId");
            SerializedProperty defaultFrameIdProperty = trackState.FindPropertyRelative("defaultFrameId");
            SerializedProperty framesProperty = trackState.FindPropertyRelative("frames");

            for (int i = 0; i < framesProperty.arraySize; i++)
            {
                SerializedProperty frame = framesProperty.GetArrayElementAtIndex(i);
                SerializedProperty frameId = frame.FindPropertyRelative("frameId");
                SerializedProperty displayName = frame.FindPropertyRelative("displayName");
                string previousId = frameId.stringValue;
                FaceEditorUtility.EnsureId(frameId);
                FaceEditorUtility.EnsureDisplayName(displayName, previousId, $"Frame {i + 1}");
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            string title = FaceEditorUtility.GetTrackLabel(library, trackIdProperty.stringValue);
            trackState.isExpanded = EditorGUILayout.Foldout(trackState.isExpanded, title, true);

            if (trackState.isExpanded)
            {
                FaceEditorUtility.DrawIdPopup(
                    trackIdProperty,
                    "Track",
                    FaceEditorUtility.GetTrackIds(library),
                    FaceEditorUtility.GetTrackLabels(library));

                string trackId = trackIdProperty.stringValue;
                DrawDefaultFrame(defaultFrameIdProperty, framesProperty);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Frame Mappings", EditorStyles.boldLabel);

                string[] spriteIds = FaceEditorUtility.GetSpriteIds(library, trackId);
                string[] spriteLabels = FaceEditorUtility.GetSpriteLabels(library, trackId);

                for (int i = 0; i < framesProperty.arraySize; i++)
                {
                    if (DrawFrame(
                        framesProperty.GetArrayElementAtIndex(i),
                        i,
                        library,
                        trackId,
                        spriteIds,
                        spriteLabels))
                    {
                        string removedFrameId = framesProperty
                            .GetArrayElementAtIndex(i)
                            .FindPropertyRelative("frameId")
                            .stringValue;
                        framesProperty.DeleteArrayElementAtIndex(i);

                        if (defaultFrameIdProperty.stringValue == removedFrameId)
                        {
                            defaultFrameIdProperty.stringValue = string.Empty;
                        }

                        break;
                    }
                }

                if (GUILayout.Button("Add Frame Mapping"))
                {
                    AddFrame(framesProperty, spriteIds);
                }

                if (FaceEditorUtility.DrawRemoveButton("Remove Track State"))
                {
                    EditorGUILayout.EndVertical();
                    return true;
                }
            }

            EditorGUILayout.EndVertical();
            return false;
        }

        private static void DrawDefaultFrame(
            SerializedProperty defaultFrameIdProperty,
            SerializedProperty framesProperty)
        {
            string[] frameIds = GetFrameIds(framesProperty);
            string[] frameLabels = GetFrameLabels(framesProperty);
            FaceEditorUtility.DrawIdPopup(
                defaultFrameIdProperty,
                "Default Frame",
                frameIds,
                frameLabels);

            if (string.IsNullOrWhiteSpace(defaultFrameIdProperty.stringValue))
            {
                EditorGUILayout.HelpBox("A Default Frame selection is required.", MessageType.Error);
            }
            else if (System.Array.IndexOf(frameIds, defaultFrameIdProperty.stringValue) < 0)
            {
                EditorGUILayout.HelpBox(
                    "The selected default frame is missing from this track's mappings.",
                    MessageType.Error);
            }
        }

        private static bool DrawFrame(
            SerializedProperty frameProperty,
            int index,
            FaceSpriteLibrary library,
            string trackId,
            string[] spriteIds,
            string[] spriteLabels)
        {
            SerializedProperty frameIdProperty = frameProperty.FindPropertyRelative("frameId");
            SerializedProperty spriteIdProperty = frameProperty.FindPropertyRelative("spriteId");
            SerializedProperty displayNameProperty = frameProperty.FindPropertyRelative("displayName");
            string previousId = frameIdProperty.stringValue;
            FaceEditorUtility.EnsureId(frameIdProperty);
            FaceEditorUtility.EnsureDisplayName(displayNameProperty, previousId, $"Frame {index + 1}");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(displayNameProperty.stringValue, EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();

            Rect previewRect = GUILayoutUtility.GetRect(
                PreviewSize,
                PreviewSize,
                GUILayout.Width(PreviewSize),
                GUILayout.Height(PreviewSize));
            FaceEditorUtility.DrawSpritePreview(
                previewRect,
                library,
                trackId,
                spriteIdProperty.stringValue);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("Frame Name"));
            FaceEditorUtility.DrawIdPopup(spriteIdProperty, "Sprite", spriteIds, spriteLabels);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            bool remove = FaceEditorUtility.DrawRemoveButton("Remove Mapping");
            EditorGUILayout.EndVertical();
            return remove;
        }

        private void AddTrackState(FaceSpriteLibrary library)
        {
            int index = trackStatesProperty.arraySize;
            trackStatesProperty.InsertArrayElementAtIndex(index);
            SerializedProperty trackState = trackStatesProperty.GetArrayElementAtIndex(index);
            string[] trackIds = FaceEditorUtility.GetTrackIds(library);

            trackState.FindPropertyRelative("trackId").stringValue =
                trackIds.Length > 0 ? trackIds[0] : string.Empty;
            trackState.FindPropertyRelative("defaultFrameId").stringValue = string.Empty;
            trackState.FindPropertyRelative("frames").ClearArray();
            trackState.isExpanded = true;
        }

        private static void AddFrame(SerializedProperty framesProperty, string[] spriteIds)
        {
            int index = framesProperty.arraySize;
            framesProperty.InsertArrayElementAtIndex(index);
            SerializedProperty frame = framesProperty.GetArrayElementAtIndex(index);
            frame.FindPropertyRelative("frameId").stringValue =
                FaceEditorUtility.CreateUniqueId(framesProperty, "frameId");
            frame.FindPropertyRelative("displayName").stringValue = $"Frame {index + 1}";
            frame.FindPropertyRelative("spriteId").stringValue =
                spriteIds.Length > 0 ? spriteIds[0] : string.Empty;
        }

        private static string[] GetFrameIds(SerializedProperty framesProperty)
        {
            System.Collections.Generic.List<string> ids = new System.Collections.Generic.List<string>();

            for (int i = 0; i < framesProperty.arraySize; i++)
            {
                string id = framesProperty
                    .GetArrayElementAtIndex(i)
                    .FindPropertyRelative("frameId")
                    .stringValue;

                if (!string.IsNullOrWhiteSpace(id))
                {
                    ids.Add(id);
                }
            }

            return ids.ToArray();
        }

        private static string[] GetFrameLabels(SerializedProperty framesProperty)
        {
            string[] labels = new string[framesProperty.arraySize];

            for (int i = 0; i < framesProperty.arraySize; i++)
            {
                SerializedProperty frame = framesProperty.GetArrayElementAtIndex(i);
                SerializedProperty displayName = frame.FindPropertyRelative("displayName");
                string name = string.IsNullOrWhiteSpace(displayName.stringValue)
                    ? $"Frame {i + 1}"
                    : displayName.stringValue;
                labels[i] = $"Frame {i + 1}: {name}";
            }

            return labels;
        }
    }

}
