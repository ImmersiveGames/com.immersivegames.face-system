# Immersive Face System

Reusable Unity package for authoring and applying atlas-based facial expressions to voxel models.

## Status

Version `1.1.0` is the current stable code baseline. The Project Owner confirmed
that the package works in PEGA; this is manual consumer validation, not a Unity
automated QA result. Signed OpenUPM distribution for this version is being
prepared. Do not change a consumer to OpenUPM until the package is listed there.

## Install

After the signed `1.1.0` package is listed, add the OpenUPM registry to
`Packages/manifest.json` with scope `com.immersivegames`, then declare the
minimum stable SemVer:

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": ["com.immersivegames"]
    }
  ],
  "dependencies": {
    "com.immersivegames.face-system": "1.1.0"
  }
}
```

Unity records the exact resolved version and source in
`Packages/packages-lock.json`. Use Package Manager to update the package; do not
pin a Git tag or edit the generated lock file by hand. The lockfile retains the
current resolution until the project is updated.

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
