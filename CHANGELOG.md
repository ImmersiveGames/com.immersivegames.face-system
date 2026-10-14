# Changelog

All notable package changes are recorded here.

## [1.1.0-preview.1] - 2026-10-14

- Added `SetState(FaceState)` and `PlayAnimation(FaceAnimation)` so gameplay code can use authored asset references; string overloads remain compatible.
- Added generated internal keys for new states, animations, tracks, sprites, and frames; the Inspector now edits friendly names and hides raw keys.
- Repaired duplicate generated keys in their authoring scope with a visible warning while preserving the first entry's links.
- Preserved existing serialized keys and used their former values as initial display names, avoiding remapping consumer assets.
- Replaced obsolete `GetInstanceID()` use with `GetEntityId()` when de-duplicating asset references in the Editor.
- Added named track lookup and a display-name overload for stopping a track animation.
- Updated installation guidance to use the untagged Git URL, which follows the default branch.
- Pending Unity import and PEGA consumer validation for this candidate.

## [1.0.0] - 2026-10-14

- Promoted the documented Face System package contract to stable after the PEGA consumer confirmed import, authoring, runtime expressions, renderer bindings, and shader/sample workflows.
- Confirmed the identifier model: State and Animation IDs are explicit string selectors; Track, Sprite, and Frame IDs are authoring keys. These IDs are not Unity-generated asset GUIDs.
- Kept the material shader contract on `_MainTex`, `_Tiling`, and `_Offset`.

## [0.1.2] - 2026-10-14 (pre-stable candidate)

- Consolidated the package entry point and user guide.
- Added step-by-step import and usage instructions for the Face System Tester and URP Shader Graph samples.
- Documented the Built-in shader option and the shared `_MainTex`, `_Tiling`, and `_Offset` material contract.
- Recorded the PEGA consumer's report that package import and visible face behavior work; full `1.0.0` acceptance remains pending.

## [0.1.1] - 2026-10-14 (pre-stable candidate)

- Fixed Editor inspectors to explicitly inherit from UnityEditor.Editor inside namespaces named Editor.
- Applied the same fix to the optional Face System Tester sample editor.
- Unity 6000.5.0f1 consumer import validation remains pending; this release is pre-stable.

## [0.1.0] - 2026-10-14 (pre-stable candidate)

- Introduced the standalone `com.immersivegames.face-system` UPM package.
- Separated runtime, editor authoring, and optional samples.
- Preserved Unity asset metadata for migrated scripts and shaders.
- Stabilized state and animation identity, controller behavior, and the `_MainTex` shader contract.
- Unity `6000.5.0f1` consumer import validation remains pending; this release is pre-stable.
