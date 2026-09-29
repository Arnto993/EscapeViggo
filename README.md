# Escape Viggo

Unity project for a first-person multiplayer party game on Windows PC.

Read [AGENTS.md](AGENTS.md), [GAME_SPEC.md](GAME_SPEC.md), and
[MILESTONES.md](MILESTONES.md) before making changes.

## Open the project

1. In Unity Hub, add this repository's root folder.
2. Open with **Unity 6.3 LTS, 6000.3.25f1**, matching
   `ProjectSettings/ProjectVersion.txt`.
3. Let package resolution, asset import, and compilation finish.
4. Open `Assets/EscapeViggo/Scenes/PrototypeWorksite.unity` and check the Console for errors.

## M0 foundation

The existing rendering and input setup is preserved:

| Dependency | Installed version |
| --- | --- |
| Universal Render Pipeline | 17.3.0 |
| Input System | 1.20.0 |
| Unity Test Framework | 1.6.0 |

The Input System is the active input backend. Graphics and quality settings
use the existing URP assets under `Assets/Settings`. Asset serialization is
Force Text, with Visible Meta Files enabled.

Project-owned assets belong under `Assets/EscapeViggo`: `Art`, `Audio`,
`Materials`, `Prefabs`, `Scenes`, `ScriptableObjects`, `Scripts`, `Tests`, and
`UI`. Empty folders have `.gitkeep` files so they survive a fresh checkout;
Unity generates their `.meta` files, which must be versioned.

The template welcome/readme assets have been removed after checking that no
retained asset references them. `SampleScene` remains the only build scene
and provides a rendering smoke test until M1. Keep its volume profile:
the scene and URP pipeline assets reference it. The template Input Actions
asset is also retained because it is registered in Editor Build Settings.

No gameplay or cloud multiplayer is configured by M0. Netcode for GameObjects
and Unity Transport belong to M3; Multiplayer Services/session/Relay setup
belongs to M17. Existing template packages are retained to avoid unrelated
package changes. There are no project automated tests yet.

## Validation

With this project closed in Unity, run from the repository root in PowerShell:

```powershell
$unityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.3.25f1\Editor\Unity.exe'
New-Item -ItemType Directory -Force -Path Logs | Out-Null
$unityCheck = Start-Process -FilePath $unityEditor -ArgumentList @(
    '-batchmode', '-nographics', '-quit',
    '-projectPath', ('"{0}"' -f $PWD.Path),
    '-logFile', ('"{0}"' -f (Join-Path $PWD.Path 'Logs/M0-validation.log'))
) -WindowStyle Hidden -PassThru
$null = $unityCheck.Handle
$unityCheck.WaitForExit()
$unityCheck.ExitCode
```

Expect exit code `0`; inspect `Logs/M0-validation.log` for compiler, package, and
import errors. See Unity's [command-line reference](https://docs.unity3d.com/6000.3/Documentation/Manual/EditorCommandLineArguments.html).
Batch mode checks import/compilation; `-nographics` does not validate rendering.

Manual check: open `SampleScene`, confirm the Console has no compile errors,
enter Play mode, verify the camera renders the skybox without errors, and
exit Play mode. No player or worksite geometry is expected at M0. Confirm the
`Assets/EscapeViggo` folders are present and Package Manager has no resolution
errors.

Git should track `Assets` (including `.meta` files), `Packages`,
`ProjectSettings`, and root documentation. Generated Unity folders, builds,
user/IDE settings, and generated solution/project files are ignored.

Milestone validation status is recorded in `MILESTONES.md`.

## M1 worksite

`PrototypeWorksite` is now the build scene. The template `SampleScene` is
retained for reference. The main area connects to a corridor at three points;
the eastern electrical, storage and office rooms also connect to each other.
Blue/orange pads mark future worker/Viggo spawns. Yellow panels mark five future
tasks; the orange table is a battery station placeholder. None has gameplay yet.

Run `Escape Viggo > Validation > Validate M1 Worksite` in Unity, or close Unity
and run `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1
-LogName M1-validation` from the repository root. Logs stay ignored.
`Escape Viggo > Setup > Create M1 Worksite (replaces scene)` explicitly rebuilds
the greybox; it replaces scene edits, so use it only when intending to reset it.

Manual M1 check: open the worksite, inspect the labelled rooms and connecting
doorways in Scene view, then enter Play mode to inspect the overview camera.
The M2 controller now replaces the overview camera.

## M2 controller

Enter Play mode in `PrototypeWorksite`: WASD moves, mouse looks, Left Shift
sprints, Space jumps, and held Left Ctrl or C crouches. Escape releases the
cursor; click **Resume controls** to capture it again. Losing window focus also
releases controls. Tuning (including sensitivity) lives in
`Assets/EscapeViggo/ScriptableObjects/PlayerMovement.asset`; bindings live in
`Assets/EscapeViggo/UI/PlayerControls.inputactions`.

`Player.prefab` separates input, CharacterController movement and camera look.
The low duct near the south of the main room tests crouching and headroom.
Manually walk through all rooms, run both loops, jump, crouch through the duct,
release crouch under it (you should stay crouched), then exit and stand.

Run all Play Mode tests in Test Runner, or close Unity and run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -TestPlatform PlayMode -LogName M2-playmode
```

The tests drive the actual CharacterController against the saved worksite and
exercise the Input Actions with a synthetic keyboard.
