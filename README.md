# Immersive Face System

Reusable Unity package for authoring and applying atlas-based facial expressions to voxel models.

## Status

Version `1.0.0` is the stable baseline, manually validated in the PEGA consumer on Unity
`6000.5.0f1` with URP `17.5.0`. The current `1.1.0-preview.1` candidate adds generated,
hidden authoring keys and asset-reference runtime APIs. Reimport and validate this candidate
in PEGA before treating it as stable.

## Install

Add the package through Unity Package Manager with this Git URL to follow the latest commit
on the repository's default branch (`main`):

```text
https://github.com/ImmersiveGames/com.immersivegames.face-system.git
```

For a reproducible install of the previous stable baseline, pin `#1.0.0` explicitly.

The package does not require the Immersive Framework or URP. It supports Unity `6000.5`
and later versions only where compatibility has been confirmed by the consuming project.

## Start here

Follow the [Face System User Guide](Documentation~/face_system_user_manual.md) to create
libraries, states, animations, controller bindings, and materials.

The package includes two optional samples:

| Sample | Use |
| --- | --- |
| Face System Tester | Add Play Mode Inspector controls for registered states and animations. |
| Universal Render Pipeline Shader | Create an atlas material for projects configured with URP and Shader Graph. |

Samples are imported separately from Package Manager. The guide explains both sample
workflows and the Built-in Render Pipeline shader option.

## Package ownership

The package core contains reusable runtime/editor code and the Built-in face-atlas shader;
the URP Shader Graph is an optional sample. Character-specific atlases, state and animation
assets, materials, prefabs, and scenes belong to the consuming project.

## References

- [User Guide](Documentation~/face_system_user_manual.md)
- [Package boundary](Documentation~/Face-System-Boundary.md)
- [Technical specification](Documentation~/face_system_spec.md)
- [Architecture reference](Documentation~/FACESYSTEM_ARCHITECTURE_V2.md)
- [Changelog](CHANGELOG.md)

## License

Licensed under the [MIT License](LICENSE.md).
