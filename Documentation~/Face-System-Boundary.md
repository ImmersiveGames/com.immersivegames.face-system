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

`1.0.0` is stable. The user confirmed the package import, authoring workflow, face states and animations, renderer bindings, and shader/sample paths in PEGA on Unity `6000.5.0f1` with URP `17.5.0`. This is manual consumer validation, not automated QA certification; later Unity versions have not been separately confirmed.

The stable contract includes the documented runtime API, serialized authoring assets, ID behavior, and shader property names `_MainTex`, `_Tiling`, and `_Offset`. State and Animation IDs are explicit strings used by the runtime string APIs; Track, Sprite, and Frame IDs are authored keys that connect package data. The package validates uniqueness within each applicable scope, but does not auto-generate these IDs. Unity `.meta` GUIDs are separate asset identifiers.
