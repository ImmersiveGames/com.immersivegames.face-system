using System;
using System.Collections.Generic;
using UnityEngine;
using FrameResult = ImmersiveGames.FaceSystem.FaceAnimation.FrameResult;

namespace ImmersiveGames.FaceSystem
{
    public sealed class FaceController : MonoBehaviour
    {
        private static readonly int OffsetPropertyId = Shader.PropertyToID("_Offset");
        private static readonly int TilingPropertyId = Shader.PropertyToID("_Tiling");
        private static readonly int MainTexPropertyId = Shader.PropertyToID("_MainTex");

        [SerializeField]
        private bool debugLogging;

        [SerializeField]
        private List<TrackBinding> trackBindings = new List<TrackBinding>();

        [SerializeField]
        private FaceState defaultState;

        [SerializeField]
        private List<FaceState> states = new List<FaceState>();

        [SerializeField]
        private List<FaceAnimation> animations = new List<FaceAnimation>();

        private readonly List<FaceAnimationPlayer> activePlayers = new List<FaceAnimationPlayer>();
        private readonly Dictionary<string, FrameResult> resolvedFrames = new Dictionary<string, FrameResult>();
        private Dictionary<string, TrackBinding> bindingLookup;
        private HashSet<string> invalidBindingIds;
        private HashSet<string> invalidStateIds;
        private HashSet<string> invalidAnimationIds;
        private Dictionary<string, FaceAnimation> animationLookup;
        private Dictionary<FaceAnimation, float> intervalTimers;
        private Dictionary<string, FaceState> stateLookup;
        private FaceState currentState;

        private void OnEnable()
        {
            BuildLookups();
            InitializeDefaultState();
        }

        private void OnValidate()
        {
            BuildLookups();
            ValidateConfiguration();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            UpdateActivePlayers(deltaTime);
            UpdateIntervalAnimations(deltaTime);
            ResolveAndApplyTracks();
        }

        public bool SetState(string stateId)
        {
            if (string.IsNullOrWhiteSpace(stateId)
                || stateLookup == null
                || !stateLookup.TryGetValue(stateId, out FaceState state))
            {
                return false;
            }

            if (!CanApplyState(state, out string error))
            {
                if (debugLogging)
                {
                    Debug.LogWarning(error, this);
                }

                return false;
            }

            currentState = state;

            if (debugLogging)
            {
                Debug.Log($"Applying state: {stateId}", this);
            }

            StopAllAnimations();
            ResetIntervalTimers();
            ResolveAndApplyTracks();
            return true;
        }

        public bool PlayAnimation(string animationId)
        {
            if (string.IsNullOrWhiteSpace(animationId)
                || animationLookup == null
                || !animationLookup.TryGetValue(animationId, out FaceAnimation animation))
            {
                return false;
            }

            if (!CanPlayAnimation(animation, out string error))
            {
                if (debugLogging)
                {
                    Debug.LogWarning(error, this);
                }

                return false;
            }

            FaceAnimationPlayer player = new FaceAnimationPlayer();

            if (!player.Start(animation))
            {
                return false;
            }

            StopAnimationTracks(animation);

            if (debugLogging)
            {
                Debug.Log($"Playing animation: {animationId}", this);
            }

            activePlayers.Add(player);
            return true;
        }

        public void StopAnimation(string trackId)
        {
            if (string.IsNullOrWhiteSpace(trackId))
            {
                return;
            }

            for (int i = activePlayers.Count - 1; i >= 0; i--)
            {
                activePlayers[i].StopTrack(trackId);

                if (!activePlayers[i].IsPlaying)
                {
                    activePlayers.RemoveAt(i);
                }
            }
        }

        public void StopAllAnimations()
        {
            activePlayers.Clear();
        }

        private void BuildLookups()
        {
            BuildBindingLookup();
            BuildStateLookup();
            BuildAnimationLookup();
        }

        private void BuildBindingLookup()
        {
            bindingLookup = new Dictionary<string, TrackBinding>();
            invalidBindingIds = new HashSet<string>();

            if (trackBindings == null)
            {
                return;
            }

            for (int i = 0; i < trackBindings.Count; i++)
            {
                TrackBinding binding = trackBindings[i];

                if (binding == null)
                {
                    Debug.LogWarning($"Track Binding {i} is null.", this);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(binding.TrackId))
                {
                    Debug.LogWarning($"Track Binding {i} has an empty Track ID.", this);
                    continue;
                }

                if (invalidBindingIds.Contains(binding.TrackId))
                {
                    continue;
                }

                if (bindingLookup.ContainsKey(binding.TrackId))
                {
                    bindingLookup.Remove(binding.TrackId);
                    invalidBindingIds.Add(binding.TrackId);
                    Debug.LogWarning($"Duplicate Track Binding ID '{binding.TrackId}'.", this);
                    continue;
                }

                bindingLookup.Add(binding.TrackId, binding);

                if (!binding.IsValid(out string error))
                {
                    Debug.LogWarning(error, this);
                }
            }
        }
        private void BuildStateLookup()
        {
            stateLookup = new Dictionary<string, FaceState>();
            invalidStateIds = new HashSet<string>();

            if (states == null)
            {
                return;
            }

            for (int i = 0; i < states.Count; i++)
            {
                FaceState state = states[i];

                if (state == null || string.IsNullOrWhiteSpace(state.Id)
                    || invalidStateIds.Contains(state.Id))
                {
                    continue;
                }

                if (stateLookup.ContainsKey(state.Id))
                {
                    stateLookup.Remove(state.Id);
                    invalidStateIds.Add(state.Id);
                    Debug.LogWarning($"Duplicate Face State ID '{state.Id}'.", this);
                    continue;
                }

                stateLookup.Add(state.Id, state);
            }
        }
        private void BuildAnimationLookup()
        {
            animationLookup = new Dictionary<string, FaceAnimation>();
            intervalTimers = new Dictionary<FaceAnimation, float>();
            invalidAnimationIds = new HashSet<string>();

            if (animations == null)
            {
                return;
            }

            for (int i = 0; i < animations.Count; i++)
            {
                FaceAnimation animation = animations[i];

                if (animation == null || string.IsNullOrWhiteSpace(animation.Id)
                    || invalidAnimationIds.Contains(animation.Id))
                {
                    continue;
                }

                if (animationLookup.ContainsKey(animation.Id))
                {
                    animationLookup.Remove(animation.Id);
                    invalidAnimationIds.Add(animation.Id);
                    Debug.LogWarning($"Duplicate Face Animation ID '{animation.Id}'.", this);
                    continue;
                }

                animationLookup.Add(animation.Id, animation);

                if (animation.Mode == FaceAnimation.PlaybackMode.Interval)
                {
                    intervalTimers.Add(animation, 0f);
                }
            }
        }
        private void InitializeDefaultState()
        {
            currentState = null;

            if (defaultState == null)
            {
                Debug.LogError("Default Face State is required.", this);
                return;
            }

            if (!CanApplyState(defaultState, out string error))
            {
                Debug.LogError($"Default Face State is invalid: {error}", this);
                return;
            }

            currentState = defaultState;
            ResolveAndApplyTracks();
        }

        private bool CanApplyState(FaceState state, out string error)
        {
            error = null;

            if (state == null || string.IsNullOrWhiteSpace(state.Id))
            {
                error = "State is null or has an empty ID.";
                return false;
            }

            if (stateLookup == null
                || !stateLookup.TryGetValue(state.Id, out FaceState registeredState)
                || registeredState != state)
            {
                error = $"State '{state.Id}' is not uniquely registered in this controller.";
                return false;
            }

            if (state.SpriteLibrary == null || state.TrackStates == null || state.TrackStates.Count == 0)
            {
                error = $"State '{state.Id}' has no valid sprite library or track states.";
                return false;
            }

            HashSet<string> trackIds = new HashSet<string>();

            for (int i = 0; i < state.TrackStates.Count; i++)
            {
                FaceState.TrackState trackState = state.TrackStates[i];

                if (trackState == null || string.IsNullOrWhiteSpace(trackState.TrackId))
                {
                    error = $"State '{state.Id}' has an empty or null track state at index {i}.";
                    return false;
                }

                string trackId = trackState.TrackId;

                if (!trackIds.Add(trackId))
                {
                    error = $"State '{state.Id}' has duplicate Track ID '{trackId}'.";
                    return false;
                }

                if (bindingLookup == null
                    || !bindingLookup.TryGetValue(trackId, out TrackBinding binding)
                    || !binding.IsValid(out error))
                {
                    error = string.IsNullOrWhiteSpace(error)
                        ? $"State '{state.Id}' has no valid renderer binding for Track ID '{trackId}'."
                        : error;
                    return false;
                }

                if (!state.SpriteLibrary.TryGetTrack(trackId, out FaceSpriteLibrary.SpriteTrack spriteTrack)
                    || spriteTrack.Atlas == null
                    || spriteTrack.GridSize.x <= 0
                    || spriteTrack.GridSize.y <= 0
                    || !state.TryGetBaseCoordinate(trackId, out Vector2Int coordinate)
                    || !IsCoordinateInGrid(coordinate, spriteTrack.GridSize))
                {
                    error = $"State '{state.Id}' has an invalid default frame or atlas for Track ID '{trackId}'.";
                    return false;
                }
            }

            return true;
        }

        private bool CanPlayAnimation(FaceAnimation animation, out string error)
        {
            error = null;

            if (animation == null || string.IsNullOrWhiteSpace(animation.Id))
            {
                error = "Animation is null or has an empty ID.";
                return false;
            }

            if (!Enum.IsDefined(typeof(FaceAnimation.PlaybackMode), animation.Mode))
            {
                error = $"Animation '{animation.Id}' has an invalid playback mode.";
                return false;
            }

            if ((animation.Mode == FaceAnimation.PlaybackMode.Interval
                    && !FaceAnimation.IsFinitePositive(animation.Interval))
                || (animation.Mode != FaceAnimation.PlaybackMode.Interval && animation.Interval != 0f))
            {
                error = $"Animation '{animation.Id}' has an invalid interval configuration.";
                return false;
            }

            IReadOnlyList<FaceAnimation.AnimationTrack> tracks = animation.Tracks;

            if (tracks == null || tracks.Count == 0)
            {
                error = $"Animation '{animation.Id}' has no tracks.";
                return false;
            }

            HashSet<string> trackIds = new HashSet<string>();
            bool hasEnabledTrack = false;

            for (int i = 0; i < tracks.Count; i++)
            {
                FaceAnimation.AnimationTrack track = tracks[i];

                if (track == null)
                {
                    error = $"Animation '{animation.Id}' has a null track at index {i}.";
                    return false;
                }

                if (!track.Enabled)
                {
                    continue;
                }

                hasEnabledTrack = true;

                if (string.IsNullOrWhiteSpace(track.TrackId) || !trackIds.Add(track.TrackId))
                {
                    error = $"Animation '{animation.Id}' has an empty or duplicate Track ID.";
                    return false;
                }

                if (bindingLookup == null
                    || !bindingLookup.TryGetValue(track.TrackId, out TrackBinding binding)
                    || !binding.IsValid(out error))
                {
                    error = string.IsNullOrWhiteSpace(error)
                        ? $"Animation '{animation.Id}' has no valid renderer binding for Track ID '{track.TrackId}'."
                        : error;
                    return false;
                }

                if (track.FrameCount == 0)
                {
                    error = $"Animation '{animation.Id}' Track ID '{track.TrackId}' has no frames.";
                    return false;
                }

                for (int frameIndex = 0; frameIndex < track.FrameCount; frameIndex++)
                {
                    if (!track.TryGetFrame(frameIndex, out FaceAnimation.FrameResult frame)
                        || !FaceAnimation.IsFinitePositive(frame.Duration))
                    {
                        error = $"Animation '{animation.Id}' Track ID '{track.TrackId}' has an invalid frame at index {frameIndex}.";
                        return false;
                    }

                    FaceSpriteLibrary library;
                    string spriteId;

                    if (frame.DataType == FaceAnimation.FrameDataType.StateFrame)
                    {
                        if (animation.TemplateState == null
                            || currentState == null
                            || !currentState.TryGetCoordinate(track.TrackId, frame.FrameId, out Vector2Int coordinate)
                            || currentState.SpriteLibrary == null
                            || !currentState.SpriteLibrary.TryGetTrack(track.TrackId, out FaceSpriteLibrary.SpriteTrack stateTrack)
                            || stateTrack.Atlas == null
                            || stateTrack.GridSize.x <= 0
                            || stateTrack.GridSize.y <= 0
                            || !IsCoordinateInGrid(coordinate, stateTrack.GridSize))
                        {
                            error = $"Animation '{animation.Id}' Track ID '{track.TrackId}' cannot resolve Frame ID '{frame.FrameId}' in the active state.";
                            return false;
                        }

                        if (!animation.TemplateState.TryGetTrackState(track.TrackId, out _))
                        {
                            error = $"Animation '{animation.Id}' Track ID '{track.TrackId}' is missing from its Template State.";
                            return false;
                        }

                        continue;
                    }

                    if (frame.DataType != FaceAnimation.FrameDataType.SpriteFrame)
                    {
                        error = $"Animation '{animation.Id}' Track ID '{track.TrackId}' has an unsupported frame source.";
                        return false;
                    }

                    library = animation.SpriteLibrary;
                    spriteId = frame.SpriteId;

                    if (library == null
                        || !library.TryGetCoordinate(track.TrackId, spriteId, out Vector2Int spriteCoordinate)
                        || !library.TryGetTrack(track.TrackId, out FaceSpriteLibrary.SpriteTrack spriteTrack)
                        || spriteTrack.Atlas == null
                        || spriteTrack.GridSize.x <= 0
                        || spriteTrack.GridSize.y <= 0
                        || !IsCoordinateInGrid(spriteCoordinate, spriteTrack.GridSize))
                    {
                        error = $"Animation '{animation.Id}' Track ID '{track.TrackId}' cannot resolve Sprite ID '{spriteId}'.";
                        return false;
                    }
                }
            }

            if (!hasEnabledTrack)
            {
                error = $"Animation '{animation.Id}' has no enabled tracks.";
                return false;
            }

            return true;
        }

        private static bool IsCoordinateInGrid(Vector2Int coordinate, Vector2Int gridSize)
        {
            return coordinate.x >= 0
                && coordinate.y >= 0
                && coordinate.x < gridSize.x
                && coordinate.y < gridSize.y;
        }
        private void ResolveAndApplyTracks()
        {
            resolvedFrames.Clear();

            if (currentState != null)
            {
                IReadOnlyList<FaceState.TrackState> stateTracks = currentState.TrackStates;

                for (int i = 0; i < stateTracks.Count; i++)
                {
                    FaceState.TrackState trackState = stateTracks[i];

                    if (trackState == null
                        || !currentState.TryGetBaseCoordinate(trackState.TrackId, out Vector2Int coordinate))
                    {
                        if (debugLogging && trackState != null)
                        {
                            Debug.LogWarning(
                                $"Could not resolve default frame '{trackState.DefaultFrameId}' "
                                + $"for track '{trackState.TrackId}' in state '{currentState.Id}'.",
                                this);
                        }

                        continue;
                    }

                    currentState.TryGetSpriteId(
                        trackState.TrackId,
                        trackState.DefaultFrameId,
                        out string spriteId);

                    resolvedFrames[trackState.TrackId] = new FrameResult
                    {
                        TrackId = trackState.TrackId,
                        Coordinate = coordinate,
                        GridSize = currentState.SpriteLibrary.TryGetTrack(
                            trackState.TrackId,
                            out FaceSpriteLibrary.SpriteTrack spriteTrack)
                                ? spriteTrack.GridSize
                                : Vector2Int.zero,
                        Atlas = spriteTrack != null ? spriteTrack.Atlas : null
                    };

                    if (debugLogging)
                    {
                        Debug.Log(
                            $"Resolved track '{trackState.TrackId}': default frame "
                            + $"'{trackState.DefaultFrameId}', sprite '{spriteId}', coordinate {coordinate}.",
                            this);
                    }
                }
            }

            for (int i = 0; i < activePlayers.Count; i++)
            {
                activePlayers[i].CollectFrames(currentState, resolvedFrames);
            }

            foreach (KeyValuePair<string, FrameResult> pair in resolvedFrames)
            {
                ApplyTrack(pair.Key, pair.Value);
            }
        }

        private void ApplyTrack(string trackId, FrameResult frame)
        {
            if (bindingLookup == null
                || !bindingLookup.TryGetValue(trackId, out TrackBinding binding))
            {
                if (debugLogging)
                {
                    Debug.LogWarning($"No Track Binding was found for Track ID '{trackId}'.", this);
                }

                return;
            }

            bool applied = binding.Apply(frame.Coordinate, frame.GridSize, frame.Atlas);

            if (debugLogging)
            {
                Debug.Log(
                    applied
                        ? $"Applied track '{trackId}' to renderer '{binding.RendererName}', "
                            + $"material index {binding.MaterialIndex}, coordinate {frame.Coordinate}, "
                            + $"grid {frame.GridSize}."
                        : $"Failed to apply track '{trackId}' to its binding.",
                    this);
            }
        }

        private void UpdateActivePlayers(float deltaTime)
        {
            for (int i = activePlayers.Count - 1; i >= 0; i--)
            {
                activePlayers[i].Update(deltaTime);

                if (!activePlayers[i].IsPlaying)
                {
                    activePlayers.RemoveAt(i);
                }
            }
        }

        private void UpdateIntervalAnimations(float deltaTime)
        {
            if (animations == null || intervalTimers == null)
            {
                return;
            }

            for (int i = 0; i < animations.Count; i++)
            {
                FaceAnimation animation = animations[i];

                if (animation == null
                    || animation.Mode != FaceAnimation.PlaybackMode.Interval
                    || !FaceAnimation.IsFinitePositive(animation.Interval)
                    || !intervalTimers.TryGetValue(animation, out float timer))
                {
                    continue;
                }

                timer += deltaTime;

                if (timer < animation.Interval)
                {
                    intervalTimers[animation] = timer;
                    continue;
                }

                intervalTimers[animation] = 0f;

                if (!IsAnyTrackPlaying(animation)
                    && !PlayAnimation(animation.Id)
                    && debugLogging)
                {
                    Debug.LogWarning($"Interval animation '{animation.Id}' could not start.", this);
                }
            }
        }

        private bool IsAnyTrackPlaying(FaceAnimation animation)
        {
            IReadOnlyList<FaceAnimation.AnimationTrack> tracks = animation.Tracks;

            if (tracks == null)
            {
                return false;
            }

            for (int i = 0; i < tracks.Count; i++)
            {
                FaceAnimation.AnimationTrack track = tracks[i];

                if (track != null && track.Enabled && IsTrackPlaying(track.TrackId))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsTrackPlaying(string trackId)
        {
            for (int i = 0; i < activePlayers.Count; i++)
            {
                if (activePlayers[i].IsTrackPlaying(trackId))
                {
                    return true;
                }
            }

            return false;
        }

        private void StopAnimationTracks(FaceAnimation animation)
        {
            IReadOnlyList<FaceAnimation.AnimationTrack> tracks = animation.Tracks;

            for (int i = 0; i < tracks.Count; i++)
            {
                FaceAnimation.AnimationTrack track = tracks[i];

                if (track != null && track.Enabled)
                {
                    StopAnimation(track.TrackId);
                }
            }
        }

        private void ResetIntervalTimers()
        {
            if (intervalTimers == null)
            {
                return;
            }

            List<FaceAnimation> keys = new List<FaceAnimation>(intervalTimers.Keys);

            for (int i = 0; i < keys.Count; i++)
            {
                intervalTimers[keys[i]] = 0f;
            }
        }

        private void ValidateConfiguration()
        {
    #if UNITY_EDITOR
            List<string> errors = new List<string>();
            ValidateBindings(errors);
            ValidateDefaultState(errors);
            ValidateAssetList(states, "state", state => state != null ? state.Id : null, errors);
            ValidateAssetList(animations, "animation", animation => animation != null ? animation.Id : null, errors);

            if (errors.Count > 0)
            {
                Debug.LogWarning($"{name} validation warnings:\n- {string.Join("\n- ", errors)}", this);
            }
    #endif
        }

        private void ValidateDefaultState(List<string> errors)
        {
            if (defaultState == null)
            {
                errors.Add("Default State is required.");
                return;
            }

            if (!CanApplyState(defaultState, out string error))
            {
                errors.Add($"Default State is invalid: {error}");
            }
        }
        private void ValidateBindings(List<string> errors)
        {
            if (trackBindings == null || trackBindings.Count == 0)
            {
                errors.Add("Track Binding list is empty.");
                return;
            }

            HashSet<string> ids = new HashSet<string>();

            for (int i = 0; i < trackBindings.Count; i++)
            {
                TrackBinding binding = trackBindings[i];

                if (binding == null)
                {
                    errors.Add($"Track Binding {i} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(binding.TrackId))
                {
                    errors.Add($"Track Binding {i} has an empty Track ID.");
                }
                else if (!ids.Add(binding.TrackId))
                {
                    errors.Add($"Duplicate Track Binding ID '{binding.TrackId}'.");
                }

                binding.Validate(errors);
            }
        }

        private static void ValidateAssetList<T>(
            List<T> assets,
            string label,
            Func<T, string> getId,
            List<string> errors)
        {
            if (assets == null)
            {
                errors.Add($"{label} list is null.");
                return;
            }

            HashSet<string> ids = new HashSet<string>();

            for (int i = 0; i < assets.Count; i++)
            {
                string id = getId(assets[i]);

                if (string.IsNullOrWhiteSpace(id))
                {
                    errors.Add($"{label} entry {i} is null or has an invalid ID.");
                }
                else if (!ids.Add(id))
                {
                    errors.Add($"Duplicate {label} ID '{id}'.");
                }
            }
        }

        [Serializable]
        public sealed class TrackBinding
        {
            [SerializeField]
            private string trackId;

            [SerializeField]
            private Renderer targetRenderer;

            [SerializeField]
            [Min(0)]
            private int materialIndex;

            [NonSerialized]
            private MaterialPropertyBlock propertyBlock;

            public string TrackId => trackId;
            public string RendererName => targetRenderer != null ? targetRenderer.name : "<missing>";
            public int MaterialIndex => materialIndex;

            internal bool IsValid(out string error)
            {
                error = null;

                if (targetRenderer == null)
                {
                    error = $"Track Binding '{trackId}' is missing its Renderer.";
                    return false;
                }

                Material[] materials = targetRenderer.sharedMaterials;

                if (materials == null || materialIndex < 0 || materialIndex >= materials.Length)
                {
                    error = $"Track Binding '{trackId}' has invalid Material Index {materialIndex} for Renderer '{targetRenderer.name}'.";
                    return false;
                }

                Material material = materials[materialIndex];

                if (material == null)
                {
                    error = $"Track Binding '{trackId}' has no Material at index {materialIndex} on Renderer '{targetRenderer.name}'.";
                    return false;
                }

                if (!material.HasProperty(MainTexPropertyId)
                    || !material.HasProperty(TilingPropertyId)
                    || !material.HasProperty(OffsetPropertyId))
                {
                    error = $"Track Binding '{trackId}' Material '{material.name}' must expose _MainTex, _Tiling, and _Offset.";
                    return false;
                }

                return true;
            }

            public bool Apply(Vector2Int coordinate, Vector2Int gridSize, Texture atlas)
            {
                if (!IsValid(out _)
                    || atlas == null
                    || gridSize.x <= 0
                    || gridSize.y <= 0
                    || !IsCoordinateInGrid(coordinate, gridSize))
                {
                    return false;
                }

                propertyBlock ??= new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(propertyBlock, materialIndex);
                propertyBlock.SetTexture(MainTexPropertyId, atlas);
                propertyBlock.SetVector(
                    TilingPropertyId,
                    new Vector2(1f / gridSize.x, 1f / gridSize.y));
                propertyBlock.SetVector(
                    OffsetPropertyId,
                    new Vector2(
                        (float)coordinate.x / gridSize.x,
                        (float)(gridSize.y - 1 - coordinate.y) / gridSize.y));
                targetRenderer.SetPropertyBlock(propertyBlock, materialIndex);
                return true;
            }

            public void Validate(List<string> errors)
            {
                if (!IsValid(out string error))
                {
                    errors.Add(error);
                }
            }
        }
    }

}
