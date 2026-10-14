using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ImmersiveGames.FaceSystem
{
    [CreateAssetMenu(menuName = "Face System/Face Animation")]
    public sealed partial class FaceAnimation : ScriptableObject
    {
        [SerializeField]
        private string id;

        [SerializeField]
        private PlaybackMode playbackMode;

        [SerializeField]
        [Min(0f)]
        private float interval;

        [SerializeField]
        [FormerlySerializedAs("referenceState")]
        private FaceState templateState;

        [SerializeField]
        private FaceSpriteLibrary spriteLibrary;

        [SerializeField]
        private List<AnimationTrack> tracks = new List<AnimationTrack>();

        public string Id => id;
        public PlaybackMode Mode => playbackMode;
        public float Interval => interval;
        public FaceState TemplateState => templateState;
        public FaceSpriteLibrary SpriteLibrary => spriteLibrary;
        public IReadOnlyList<AnimationTrack> Tracks => tracks;

        internal static bool IsFinitePositive(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private void OnValidate()
        {
            List<string> errors = new List<string>();

            ValidatePlayback(errors);
            ValidateTracks(errors);

            if (errors.Count > 0)
            {
                Debug.LogWarning($"{name} validation warnings:\n- {string.Join("\n- ", errors)}", this);
            }
        }

        private void ValidatePlayback(List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                errors.Add("ID is invalid.");
            }

            if (!Enum.IsDefined(typeof(PlaybackMode), playbackMode))
            {
                errors.Add("Playback mode is invalid.");
            }

            if (playbackMode == PlaybackMode.Interval && !IsFinitePositive(interval))
            {
                errors.Add("Interval playback requires a finite interval > 0.");
            }
            else if (playbackMode != PlaybackMode.Interval && interval != 0f)
            {
                errors.Add("Interval must be zero unless Interval playback is selected.");
            }
        }

        private void ValidateTracks(List<string> errors)
        {
            if (tracks == null || tracks.Count == 0)
            {
                errors.Add("Animation Track list is empty.");
                return;
            }

            HashSet<string> trackIds = new HashSet<string>();

            for (int i = 0; i < tracks.Count; i++)
            {
                AnimationTrack track = tracks[i];
                string label = $"Animation Track {i}";

                if (track == null)
                {
                    errors.Add($"{label} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(track.TrackId))
                {
                    errors.Add($"{label} has an empty Track ID.");
                }
                else
                {
                    label = $"Animation Track '{track.TrackId}'";

                    if (!trackIds.Add(track.TrackId))
                    {
                        errors.Add($"Duplicate Track ID '{track.TrackId}'.");
                    }
                }

                if (track.Enabled)
                {
                    track.Validate(errors, label, templateState, spriteLibrary);
                }
            }
        }
    }

}
