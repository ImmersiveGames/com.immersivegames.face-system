# Immersive Face System

Reusable Unity package for authoring and applying atlas-based facial expressions to voxel models.

## Status

Version `1.1.0` is the current stable release. The user confirmed that the current package
works in the PEGA consumer after the candidate updates. This is manual consumer validation;
Unity automated QA was not run for this release.

## Install

Add the package through Unity Package Manager with this Git URL to follow the latest commit
on the repository's default branch (`main`):

```text
https://github.com/ImmersiveGames/com.immersivegames.face-system.git
```

For a reproducible install of this stable release, pin `#1.1.0` explicitly.

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
