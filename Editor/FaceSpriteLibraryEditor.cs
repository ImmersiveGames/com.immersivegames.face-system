using UnityEditor;
using UnityEngine;
using ImmersiveGames.FaceSystem;

namespace ImmersiveGames.FaceSystem.Editor
{
    [CustomEditor(typeof(FaceSpriteLibrary))]
    public sealed class FaceSpriteLibraryEditor : UnityEditor.Editor
    {
        private const float PreviewSize = 56f;
        private SerializedProperty tracksProperty;

        private void OnEnable()
        {
            tracksProperty = serializedObject.FindProperty("tracks");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            int repairedDuplicateKeys = EnsureTrackKeys();

            EditorGUILayout.LabelField("Face Sprite Library", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Each named track owns an atlas, grid size, and sprite list. Internal keys are generated automatically.",
                MessageType.Info);
            if (repairedDuplicateKeys > 0)
            {
                EditorGUILayout.HelpBox(
                    $"Generated new keys for {repairedDuplicateKeys} duplicate entries. Existing links still resolve to the first entry; select the intended named entry where needed.",
                    MessageType.Warning);
            }

            for (int i = 0; i < tracksProperty.arraySize; i++)
            {
                if (DrawTrack(tracksProperty.GetArrayElementAtIndex(i), i))
                {
                    tracksProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            if (GUILayout.Button("Add Track"))
            {
                AddTrack();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private int EnsureTrackKeys()
        {
            int repairedDuplicateKeys = 0;

            for (int i = 0; i < tracksProperty.arraySize; i++)
            {
                SerializedProperty track = tracksProperty.GetArrayElementAtIndex(i);
                SerializedProperty trackId = track.FindPropertyRelative("trackId");
                SerializedProperty trackName = track.FindPropertyRelative("displayName");
                string previousTrackId = trackId.stringValue;
                FaceEditorUtility.EnsureDisplayName(trackName, previousTrackId, $"Track {i + 1}");
                FaceEditorUtility.EnsureId(trackId);

                SerializedProperty sprites = track.FindPropertyRelative("sprites");
                for (int spriteIndex = 0; spriteIndex < sprites.arraySize; spriteIndex++)
                {
                    SerializedProperty sprite = sprites.GetArrayElementAtIndex(spriteIndex);
                    SerializedProperty spriteId = sprite.FindPropertyRelative("id");
                    SerializedProperty spriteName = sprite.FindPropertyRelative("displayName");
                    string previousSpriteId = spriteId.stringValue;
                    FaceEditorUtility.EnsureDisplayName(
                        spriteName,
                        previousSpriteId,
                        $"Sprite {spriteIndex + 1}");
                    FaceEditorUtility.EnsureId(spriteId);
                }

                repairedDuplicateKeys += FaceEditorUtility.EnsureUniqueIds(sprites, "id");
            }

            repairedDuplicateKeys += FaceEditorUtility.EnsureUniqueIds(tracksProperty, "trackId");
            return repairedDuplicateKeys;
        }

        private static bool DrawTrack(SerializedProperty trackProperty, int index)
        {
            SerializedProperty trackIdProperty = trackProperty.FindPropertyRelative("trackId");
            SerializedProperty displayNameProperty = trackProperty.FindPropertyRelative("displayName");
            SerializedProperty atlasProperty = trackProperty.FindPropertyRelative("atlas");
            SerializedProperty gridSizeProperty = trackProperty.FindPropertyRelative("gridSize");
            SerializedProperty spritesProperty = trackProperty.FindPropertyRelative("sprites");
            string previousId = trackIdProperty.stringValue;
            FaceEditorUtility.EnsureId(trackIdProperty);
            FaceEditorUtility.EnsureDisplayName(displayNameProperty, previousId, $"Track {index + 1}");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            string title = displayNameProperty.stringValue;
            trackProperty.isExpanded = EditorGUILayout.Foldout(trackProperty.isExpanded, title, true);

            if (trackProperty.isExpanded)
            {
                EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("Track Name"));
                EditorGUILayout.PropertyField(atlasProperty);
                EditorGUILayout.PropertyField(gridSizeProperty);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Sprite Entries", EditorStyles.boldLabel);

                for (int i = 0; i < spritesProperty.arraySize; i++)
                {
                    if (DrawSpriteEntry(
                        spritesProperty.GetArrayElementAtIndex(i),
                        i,
                        atlasProperty.objectReferenceValue as Texture2D,
                        gridSizeProperty.vector2IntValue))
                    {
                        spritesProperty.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }

                if (GUILayout.Button("Add Sprite Entry"))
                {
                    AddSprite(spritesProperty);
                }

                if (FaceEditorUtility.DrawRemoveButton("Remove Track"))
                {
                    EditorGUILayout.EndVertical();
                    return true;
                }
            }

            EditorGUILayout.EndVertical();
            return false;
        }

        private static bool DrawSpriteEntry(
            SerializedProperty spriteProperty,
            int index,
            Texture2D atlas,
            Vector2Int gridSize)
        {
            SerializedProperty idProperty = spriteProperty.FindPropertyRelative("id");
            SerializedProperty displayNameProperty = spriteProperty.FindPropertyRelative("displayName");
            SerializedProperty coordinateProperty = spriteProperty.FindPropertyRelative("atlasCoordinate");
            string previousId = idProperty.stringValue;
            FaceEditorUtility.EnsureId(idProperty);
            FaceEditorUtility.EnsureDisplayName(displayNameProperty, previousId, $"Sprite {index + 1}");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(displayNameProperty.stringValue, EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();

            Rect previewRect = GUILayoutUtility.GetRect(
                PreviewSize,
                PreviewSize,
                GUILayout.Width(PreviewSize),
                GUILayout.Height(PreviewSize));
            FaceEditorUtility.DrawTexturePreview(
                previewRect,
                atlas,
                gridSize,
                coordinateProperty.vector2IntValue);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("Sprite Name"));
            EditorGUILayout.PropertyField(coordinateProperty, new GUIContent("Atlas Coordinate"));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            bool remove = FaceEditorUtility.DrawRemoveButton("Remove Sprite");
            EditorGUILayout.EndVertical();
            return remove;
        }

        private void AddTrack()
        {
            int index = tracksProperty.arraySize;
            tracksProperty.InsertArrayElementAtIndex(index);
            SerializedProperty track = tracksProperty.GetArrayElementAtIndex(index);
            track.FindPropertyRelative("trackId").stringValue =
                FaceEditorUtility.CreateUniqueId(tracksProperty, "trackId");
            track.FindPropertyRelative("displayName").stringValue = $"Track {index + 1}";
            track.FindPropertyRelative("atlas").objectReferenceValue = null;
            track.FindPropertyRelative("gridSize").vector2IntValue = new Vector2Int(4, 4);
            track.FindPropertyRelative("sprites").ClearArray();
            track.isExpanded = true;
        }

        private static void AddSprite(SerializedProperty spritesProperty)
        {
            int index = spritesProperty.arraySize;
            spritesProperty.InsertArrayElementAtIndex(index);
            SerializedProperty sprite = spritesProperty.GetArrayElementAtIndex(index);
            sprite.FindPropertyRelative("id").stringValue =
                FaceEditorUtility.CreateUniqueId(spritesProperty, "id");
            sprite.FindPropertyRelative("displayName").stringValue = $"Sprite {index + 1}";
            sprite.FindPropertyRelative("atlasCoordinate").vector2IntValue = Vector2Int.zero;
        }
    }

}
