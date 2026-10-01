# C0 Blender → Unity pipeline

Requires Blender **5.2 LTS**, Unity **6000.3.25f1**, and Windows PowerShell.
Run from the repository root. Close this project's Unity editor before batch runs,
or copy `Assets`, `Packages`, `ProjectSettings` and `Tools` to an isolated project
and invoke that copy's unchanged `Tools/Invoke-Unity.ps1`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Blender.ps1 -Version
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Blender.ps1 -Script Tools/Blender/validate_c0.py
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -Method EscapeViggo.Editor.C0ArtPipeline.BuildPreview -LogName C0Build
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -TestPlatform EditMode -TestFilter EscapeViggo.Tests.C0ArtPipelineTests -LogName C0Tests
```

Executable lookup: `-BlenderPath`, `BLENDER_EXE`, `blender.exe` on PATH, then
`%ProgramFiles%/Blender Foundation/Blender 5.2/blender.exe`. Wrong versions are
rejected; select an explicit override. No local executable path is stored in
project configuration. Blender's exit code is returned; Python exceptions exit 1.
`-Version` runs the CLI smoke test. Background runs use factory settings and ignored
`Logs/BlenderUser` cache data. Use a child PowerShell process as above because the
wrapper exits. For multiple script arguments, use a PowerShell array:

```powershell
& ./Tools/Invoke-Blender.ps1 -Script ./Tools/Blender/validate_c0.py -ScriptArguments @('--project-root', 'D:/Projects/Escape Viggo')
```

`pipeline.py` supplies scene reset, transform application and selected static-mesh
FBX export helpers. The generator recreates only the named C0 source/export files,
checks geometry, reopens its saved source and checks binary FBX output. Reruns
preserve geometry, names and materials; Blender/FBX metadata may change binary
hashes. Do not hand-edit the generated fixture.

Export uses metre units, FBX unit scaling, -Z forward / Y up and baked static axis
conversion. This profile is **only validated for static meshes**; rigging and
animation require later validation. Settings follow the
[Blender FBX API](https://docs.blender.org/api/main/bpy.ops.export_scene.html) and
[Unity model importer](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/ModelImporter.html).

Unity assets live in `Assets/EscapeViggo/Art/Tests/C0`: FBX model, explicit URP
material, prefab and preview scene. `C0ArtPipeline.Prepare` remaps the fixture's
material to URP Lit and validates scale, axes, pivot, mesh, transforms and references.
Unity generates all `.meta` GUIDs. `BuildPreview` saves the isolated scene, builds
a separate Windows development player and checks the FBX is in packed build assets.
Gameplay scenes, build scene lists and production import defaults are untouched.

Outputs (relative to the project being validated): `Logs/C0Blender.json`,
`Logs/C0Build.log`, `Logs/C0Tests.xml`, `Builds/C0Preview/C0Preview.exe`.
Caches, logs and players are ignored; source `.blend`, FBX, material, prefab, preview
scene and Unity-generated metas are tracked.

Manual review: open the `.blend` to inspect ground origin and applied transforms.
Open `C0Preview.unity` or run its player: inspect the blue 2 m tall mesh with its
raised nose pointing +Z. This is pipeline review, not final art acceptance.
Stop at C0; no character, prop or environment production is included.
