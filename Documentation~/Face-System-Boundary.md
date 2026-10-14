# Face System boundary

## Owns

- Reusable Unity runtime for selecting face states and playing face animations over explicitly configured renderer/material bindings.
- Face sprite-library, state, and animation authoring inspectors.
- The Built-in face-atlas shader and optional rendering-pipeline samples.
- Public compatibility and versioning for `com.immersivegames.face-system`.

## Does not own

- Character-specific atlases, state assets, animation assets, materials, prefabs, or scenes; those stay with each consumer.
- Framework lifecycle, scene composition, gameplay state, input, character identity, bootstrap, or service registration.
- A mandatory render-pipeline package. The URP Shader Graph is an optional sample.

## Assembly ownership

- `ImmersiveGames.FaceSystem.Runtime` uses Unity runtime types and has no dependency on UnityEditor or Immersive Framework packages.
- `ImmersiveGames.FaceSystem.Editor` is Editor-only and references the package runtime.
- The manual tester and its Inspector live in the `Face System Tester` sample assemblies, outside the runtime and editor product assemblies.

## Version status

`0.1.1` is a pre-stable candidate. The API and Unity `6000.5.0f1` consumer import require manual validation before a `1.0.0` stability decision.
