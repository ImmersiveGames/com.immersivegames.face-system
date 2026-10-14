using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ImmersiveGames.FaceSystem;

namespace ImmersiveGames.FaceSystem.Editor
{
    internal static class FaceEditorUtility
    {
        public static string CreateId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string CreateUniqueId(SerializedProperty items, string idFieldName)
        {
            HashSet<string> usedIds = new HashSet<string>(StringComparer.Ordinal);

            if (items != null && items.isArray)
            {
                for (int i = 0; i < items.arraySize; i++)
                {
                    SerializedProperty id = items.GetArrayElementAtIndex(i).FindPropertyRelative(idFieldName);
                    if (id != null && !string.IsNullOrWhiteSpace(id.stringValue))
                    {
                        usedIds.Add(id.stringValue);
                    }
                }
            }

            string generatedId;
            do
            {
                generatedId = CreateId();
            }
            while (!usedIds.Add(generatedId));

            return generatedId;
        }

        public static void EnsureId(SerializedProperty property)
        {
            if (property != null && string.IsNullOrWhiteSpace(property.stringValue))
            {
                property.stringValue = CreateId();
            }
        }

        public static void EnsureAssetId(UnityEngine.Object asset)
        {
            if (asset == null)
            {
                return;
            }

            SerializedObject serializedAsset = new SerializedObject(asset);
            EnsureId(serializedAsset.FindProperty("id"));
            serializedAsset.ApplyModifiedProperties();
        }

        public static int EnsureUniqueIds(SerializedProperty items, string idFieldName)
        {
            if (items == null || !items.isArray)
            {
                return 0;
            }

            HashSet<string> usedIds = new HashSet<string>(StringComparer.Ordinal);
            int repairedDuplicates = 0;

            for (int i = 0; i < items.arraySize; i++)
            {
                SerializedProperty id = items.GetArrayElementAtIndex(i).FindPropertyRelative(idFieldName);

                if (id == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(id.stringValue))
                {
                    id.stringValue = CreateId();
                    usedIds.Add(id.stringValue);
                }
                else if (!usedIds.Add(id.stringValue))
                {
                    id.stringValue = CreateId();
                    usedIds.Add(id.stringValue);
                    repairedDuplicates++;
                }
            }

            return repairedDuplicates;
        }

        public static int EnsureUniqueAssetIds(SerializedProperty assets)
        {
            if (assets == null || !assets.isArray)
            {
                return 0;
            }

            HashSet<int> seenAssetIds = new HashSet<int>();
            HashSet<string> usedIds = new HashSet<string>(StringComparer.Ordinal);
            int repairedDuplicates = 0;

            for (int i = 0; i < assets.arraySize; i++)
            {
                UnityEngine.Object asset = assets.GetArrayElementAtIndex(i).objectReferenceValue;

                if (asset == null || !seenAssetIds.Add(asset.GetInstanceID()))
                {
                    continue;
                }

                SerializedObject serializedAsset = new SerializedObject(asset);
                SerializedProperty id = serializedAsset.FindProperty("id");

                if (id == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(id.stringValue))
                {
                    id.stringValue = CreateId();
                    usedIds.Add(id.stringValue);
                    serializedAsset.ApplyModifiedProperties();
                }
                else if (!usedIds.Add(id.stringValue))
                {
                    id.stringValue = CreateId();
                    usedIds.Add(id.stringValue);
                    serializedAsset.ApplyModifiedProperties();
                    repairedDuplicates++;
                }
            }

            return repairedDuplicates;
        }

        public static void EnsureDisplayName(SerializedProperty property, string currentId, string fallback)
        {
            if (property == null || !string.IsNullOrWhiteSpace(property.stringValue))
            {
                return;
            }

            property.stringValue = string.IsNullOrWhiteSpace(currentId) ? fallback : currentId;
        }

        public static string[] GetTrackIds(FaceSpriteLibrary library)
        {
            if (library == null)
            {
                return Array.Empty<string>();
            }

            List<string> ids = new List<string>();

            for (int i = 0; i < library.Tracks.Count; i++)
            {
                FaceSpriteLibrary.SpriteTrack track = library.Tracks[i];

                if (track != null && !string.IsNullOrWhiteSpace(track.TrackId))
                {
                    ids.Add(track.TrackId);
                }
            }

            return ids.ToArray();
        }

        public static string[] GetTrackLabels(FaceSpriteLibrary library)
        {
            if (library == null)
            {
                return Array.Empty<string>();
            }

            List<string> labels = new List<string>();

            for (int i = 0; i < library.Tracks.Count; i++)
            {
                FaceSpriteLibrary.SpriteTrack track = library.Tracks[i];

                if (track != null && !string.IsNullOrWhiteSpace(track.TrackId))
                {
                    labels.Add(GetTrackLabel(track, i));
                }
            }

            return labels.ToArray();
        }

        public static string[] GetTrackLabels(FaceState state)
        {
            if (state == null)
            {
                return Array.Empty<string>();
            }

            List<string> labels = new List<string>();

            for (int i = 0; i < state.TrackStates.Count; i++)
            {
                FaceState.TrackState trackState = state.TrackStates[i];

                if (trackState == null || string.IsNullOrWhiteSpace(trackState.TrackId))
                {
                    continue;
                }

                string label = state.SpriteLibrary != null
                    && state.SpriteLibrary.TryGetTrack(trackState.TrackId, out FaceSpriteLibrary.SpriteTrack track)
                    ? GetTrackLabel(track, i)
                    : $"Track {i + 1}";
                labels.Add(label);
            }

            return labels.ToArray();
        }

        public static string GetTrackLabel(FaceSpriteLibrary library, string trackId)
        {
            if (library != null)
            {
                for (int i = 0; i < library.Tracks.Count; i++)
                {
                    FaceSpriteLibrary.SpriteTrack track = library.Tracks[i];

                    if (track != null && track.TrackId == trackId)
                    {
                        return GetTrackLabel(track, i);
                    }
                }
            }

            return "Unassigned Track";
        }

        private static string GetTrackLabel(FaceSpriteLibrary.SpriteTrack track, int index)
        {
            string name = string.IsNullOrWhiteSpace(track.DisplayName)
                ? $"Track {index + 1}"
                : track.DisplayName;
            string atlasName = track.Atlas != null ? $", {track.Atlas.name}" : string.Empty;
            return $"Track {index + 1}: {name}{atlasName}";
        }

        public static string[] GetTrackIds(FaceState state)
        {
            if (state == null)
            {
                return Array.Empty<string>();
            }

            List<string> ids = new List<string>();

            for (int i = 0; i < state.TrackStates.Count; i++)
            {
                FaceState.TrackState track = state.TrackStates[i];

                if (track != null && !string.IsNullOrWhiteSpace(track.TrackId))
                {
                    ids.Add(track.TrackId);
                }
            }

            return ids.ToArray();
        }

        public static string[] GetSpriteIds(FaceSpriteLibrary library, string trackId)
        {
            if (library == null || !library.TryGetTrack(trackId, out FaceSpriteLibrary.SpriteTrack track))
            {
                return Array.Empty<string>();
            }

            List<string> ids = new List<string>();

            for (int i = 0; i < track.Sprites.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(track.Sprites[i].Id))
                {
                    ids.Add(track.Sprites[i].Id);
                }
            }

            return ids.ToArray();
        }

        public static string[] GetSpriteLabels(FaceSpriteLibrary library, string trackId)
        {
            if (library == null || !library.TryGetTrack(trackId, out FaceSpriteLibrary.SpriteTrack track))
            {
                return Array.Empty<string>();
            }

            List<string> labels = new List<string>();

            for (int i = 0; i < track.Sprites.Count; i++)
            {
                FaceSpriteLibrary.SpriteEntry sprite = track.Sprites[i];

                if (!string.IsNullOrWhiteSpace(sprite.Id))
                {
                    string name = string.IsNullOrWhiteSpace(sprite.DisplayName)
                        ? $"Sprite {i + 1}"
                        : sprite.DisplayName;
                    Vector2Int cell = sprite.AtlasCoordinate;
                    labels.Add($"Sprite {i + 1}: {name} (cell {cell.x}, {cell.y})");
                }
            }

            return labels.ToArray();
        }

        public static string[] GetStateFrameIds(FaceState state, string trackId)
        {
            if (state == null || !state.TryGetTrackState(trackId, out FaceState.TrackState trackState))
            {
                return Array.Empty<string>();
            }

            List<string> ids = new List<string>();

            for (int i = 0; i < trackState.Frames.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(trackState.Frames[i].FrameId))
                {
                    ids.Add(trackState.Frames[i].FrameId);
                }
            }

            return ids.ToArray();
        }

        public static string[] GetStateFrameLabels(FaceState state, string trackId)
        {
            if (state == null || !state.TryGetTrackState(trackId, out FaceState.TrackState trackState))
            {
                return Array.Empty<string>();
            }

            List<string> labels = new List<string>();

            for (int i = 0; i < trackState.Frames.Count; i++)
            {
                FaceState.StateFrame frame = trackState.Frames[i];

                if (string.IsNullOrWhiteSpace(frame.FrameId))
                {
                    continue;
                }

                string name = string.IsNullOrWhiteSpace(frame.DisplayName)
                    ? $"Frame {i + 1}"
                    : frame.DisplayName;
                if (state.SpriteLibrary != null
                    && state.SpriteLibrary.TryGetCoordinate(trackId, frame.SpriteId, out Vector2Int cell))
                {
                    name += $" (cell {cell.x}, {cell.y})";
                }

                labels.Add($"Frame {i + 1}: {name}");
            }

            return labels.ToArray();
        }

        public static void DrawIdPopup(
            SerializedProperty property,
            string label,
            string[] ids,
            string[] displayLabels)
        {
            string currentValue = property.stringValue;

            if (ids == null || ids.Length == 0)
            {
                EditorGUILayout.HelpBox($"No values are available for {label}.", MessageType.Warning);
                return;
            }

            if (displayLabels == null || displayLabels.Length != ids.Length)
            {
                displayLabels = ids;
            }

            int currentIndex = Array.IndexOf(ids, currentValue);
            string[] options = displayLabels;

            if (currentIndex < 0)
            {
                options = new string[ids.Length + 1];
                options[0] = string.IsNullOrWhiteSpace(currentValue)
                    ? "<Select>"
                    : "<Missing selection>";
                displayLabels.CopyTo(options, 1);
                currentIndex = 0;
            }

            int selectedIndex = EditorGUILayout.Popup(label, currentIndex, options);

            if (selectedIndex > 0 || options.Length == ids.Length)
            {
                int idIndex = options.Length == ids.Length ? selectedIndex : selectedIndex - 1;
                if (idIndex >= 0 && idIndex < ids.Length)
                {
                    property.stringValue = ids[idIndex];
                }
            }
        }

        public static void DrawStatePreview(Rect rect, FaceState state, string trackId, string frameId)
        {
            DrawPreviewBackground(rect);

            if (state != null
                && state.SpriteLibrary != null
                && state.TryGetCoordinate(trackId, frameId, out Vector2Int coordinate))
            {
                DrawAtlasPreview(rect, state.SpriteLibrary, trackId, coordinate);
            }
        }

        public static void DrawSpritePreview(Rect rect, FaceSpriteLibrary library, string trackId, string spriteId)
        {
            DrawPreviewBackground(rect);

            if (library != null && library.TryGetCoordinate(trackId, spriteId, out Vector2Int coordinate))
            {
                DrawAtlasPreview(rect, library, trackId, coordinate);
            }
        }

        public static void DrawTexturePreview(
            Rect rect,
            Texture2D atlas,
            Vector2Int gridSize,
            Vector2Int coordinate)
        {
            DrawPreviewBackground(rect);

            if (atlas == null || gridSize.x <= 0 || gridSize.y <= 0)
            {
                return;
            }

            GUI.DrawTextureWithTexCoords(rect, atlas, GetUvRect(gridSize, coordinate), true);
        }

        public static bool DrawRemoveButton(string label = "Remove")
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool clicked = GUILayout.Button(label, GUILayout.Width(110f));
            EditorGUILayout.EndHorizontal();
            return clicked;
        }

        private static void DrawAtlasPreview(
            Rect rect,
            FaceSpriteLibrary library,
            string trackId,
            Vector2Int coordinate)
        {
            if (library != null && library.TryGetTrack(trackId, out FaceSpriteLibrary.SpriteTrack track))
            {
                DrawTexturePreview(rect, track.Atlas, track.GridSize, coordinate);
            }
        }

        private static void DrawPreviewBackground(Rect rect)
        {
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f));
        }

        private static Rect GetUvRect(Vector2Int gridSize, Vector2Int coordinate)
        {
            float tileWidth = 1f / gridSize.x;
            float tileHeight = 1f / gridSize.y;

            return new Rect(
                coordinate.x * tileWidth,
                1f - ((coordinate.y + 1) * tileHeight),
                tileWidth,
                tileHeight);
        }
    }

}
