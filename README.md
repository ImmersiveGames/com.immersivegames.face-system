# Immersive Face System

Reusable Unity package for authoring and applying facial expressions to voxel models.

## Status

Version `0.1.1` is a pre-stable candidate. The public contract is being stabilized; Unity `6000.5.0f1` import and consumer validation are pending. Do not treat this version as a stable `1.x` compatibility promise.

## Installation

Install through Unity Package Manager using the immutable package tag:

```text
https://github.com/ImmersiveGames/com.immersivegames.face-system.git#0.1.1
```

The package has no dependency on the Immersive Framework or its technical packages. It supports Unity `6000.5` and later versions only where compatibility has been confirmed by the consuming project.

## Package contents

- Runtime: face sprite libraries, states, animations, and an explicitly configured `FaceController`.
- Editor: inspectors and authoring validation.
- Built-in shader: face atlas material support.
- Optional samples: `Face System Tester` and a URP Shader Graph example. URP is not a required package dependency.
- Documentation: installation, authoring, bindings, shader properties, and migration notes.

Character-specific atlases, state assets, animation assets, materials, prefabs, and scenes belong to the consuming project and are not included in this package.

## Documentation

See [`Documentation~`](Documentation~) in the package source for the user manual, contract notes, architecture, and expression-system reference.

## License

Licensed under the [MIT License](LICENSE.md).
