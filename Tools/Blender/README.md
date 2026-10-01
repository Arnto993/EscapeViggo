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
The C0 commands above produce only the static fixture. Character commands follow.
# C1-C4 electrician production

The C0 static profile remains unchanged. Character work uses its metre scene/reset,
transform and CLI helpers with a separate skinned FBX profile: **no baked space
transforms**, no leaf bones, explicit baked actions at 30 fps. Unity bakes axis
conversion and explicitly maps the standard Humanoid names.

```powershell
# Rebuild authored geometry/rig/actions (overwrites the two named source files).
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Blender.ps1 -Script Tools/Blender/create_electrician.py
# Preserve manual Blender edits; reopen and export the saved electrician instead.
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Blender.ps1 -Script Tools/Blender/export_character.py
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Blender.ps1 -Script Tools/Blender/validate_character.py
# Run with the project closed, or use an isolated copy as described above.
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -Method EscapeViggo.Editor.ElectricianArtPipeline.Prepare -LogName C4Prepare
```

Editable sources:
- `ArtSource/Characters/BaseHuman/SRC_BaseHuman.blend`: reusable T-pose body/rig.
- `ArtSource/Characters/Electrician/SRC_Electrician.blend`: canonical workwear,
  skinning, materials and Idle/Walk/Run actions. Clear the active action to inspect
  the rest T-pose. All geometry remains editable; no subdivision or dense textures.
- `ArtSource/Characters/Electrician/Review/`: front, side, back and three-quarter
  PNGs of the actual model, rendered at idle frame 1. Regeneration updates these.

Rig: Hips > Spine > Chest > Neck > Head; left/right shoulder, upper arm, lower arm,
hand; hips > upper leg > lower leg > foot > toes. 21 bones, including all 17
required bones. No finger/facial controls. Elbows, knees and torso use blended
weights; rigid accessories follow their owning bones. Blender +Y forward/Z up,
Unity +Z forward/Y up; ground origin, applied transforms, height 1.8355 m.

Runtime: `Assets/EscapeViggo/Art/Characters/Electrician/` contains the Humanoid FBX,
URP materials, controller and `PF_Electrician.prefab`. The three source actions
become looping Humanoid clips in the FBX, shared through a normalized Speed blend
(0 idle, 1 walk, 2 run/sprint). The controller and all clips disable/bake root
motion. These are minimum pipeline-validation locomotion, not the final library.

`ElectricianPresentation` reads the existing player transform, role, ownership,
crouch state and movement settings. Owner sees shadows only; remote players see
the mesh. Viggo retains the existing placeholder. Teleport-sized deltas do not
produce animation speed spikes. Minimal crouch scales the visual height; dedicated
crouch/jump/task animation and stride/foot-placement polish are later work.

`Prepare` updates presentation children on Player/NetworkPlayer with Unity prefab
APIs. Existing Body references, collider, movement and networking components remain.
Unity owns all GUIDs. Re-run Prepare after model changes; don't replace gameplay
prefabs with a raw model. The development multiplayer smoke asserts character
role visibility, Humanoid validity and owner/remote shadow modes.

`Logs/C4Blender.json` reports geometry/material/weight/rig semantic checksum.
Repeated generation is semantically deterministic; FBX timestamps and Blender
internal vertex ordering are not promised byte-identical. No C5 content is made.

Human review: compare the four renders to the canonical reference, then open the
prefab/model in Unity. Host and join two players; observe the electrician idle,
walk and sprint from the other window, turn/crouch/jump and reconnect. Confirm no
local face/body obstruction, readable silhouette at 5-15 m, and acceptable limb
bending/foot sliding. Visual identity and animation feel need human sign-off.
