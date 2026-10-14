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

`1.0.0` remains the stable baseline. The user confirmed its package import, authoring workflow, face states and animations, renderer bindings, and shader/sample paths in PEGA on Unity `6000.5.0f1` with URP `17.5.0`. This is manual consumer validation, not automated QA certification; later Unity versions have not been separately confirmed. Candidate `1.1.0-preview.1` adds identity and Inspector UX changes and is pending the same consumer revalidation.

The stable `1.0.0` contract includes its documented runtime API, serialized authoring assets, ID behavior, and shader property names `_MainTex`, `_Tiling`, and `_Offset`. The `1.1.0-preview.1` candidate preserves existing serialized keys, generates keys for new authored entries, and hides those keys behind names and selectors in the Inspector. State and Animation string APIs remain available for compatibility; new code can pass their exact asset references. Generated keys are package-owned values, distinct from Unity `.meta` GUIDs and from Immersive Framework identity types.
