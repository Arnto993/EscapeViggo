# Escape Viggo

Unity 6.3 LTS (**6000.3.25f1**), URP, Windows PC. M1–M5 are implemented.
M6 is the next checkpoint. Read [AGENTS.md](AGENTS.md),
[GAME_SPEC.md](GAME_SPEC.md), and [MILESTONES.md](MILESTONES.md) before changes.

## Open and play

1. Open this repository in Unity Hub with 6000.3.25f1.
2. Wait for package import, then open
   `Assets/EscapeViggo/Scenes/PrototypeWorksite.unity`.
3. Enter Play mode. In the development panel, leave IP `127.0.0.1` and port
   `7777`, then click **Start Host**.
4. WASD moves, mouse looks, Left Shift sprints, Space jumps, and held Left Ctrl
   or C crouches. Escape releases the mouse. **Resume controls** captures it.
   Losing window focus releases controls; networking continues in the background.
5. Check the Console for errors.

## Host and client manual test

A ready development build is at `Builds/Windows/EscapeViggo.exe` (ignored by Git).
To rebuild it, use **Escape Viggo > Build > Windows Development Player**.

1. In the Editor, play the worksite and click **Start Host** at `127.0.0.1:7777`.
2. Launch `Builds/Windows/EscapeViggo.exe`. Leave the same address/port and click
   **Start Client**. Both windows should display **Players: 2**.
3. Press Escape to switch windows. Resume controls in the window you want to
   move. Move each player separately into the main work area. Confirm the other
   window shows that movement and its own player stays still.
4. In both windows test sprint, jump, crouch and mouse look. The other capsule
   should move, rotate, jump and shrink when crouching. Each window should keep
   its own camera; there should be no multiple AudioListener warnings.
5. Walk through the three corridor doors and through the electrical/storage/
   office connecting doors. Run around central machinery and behind cover.
   Find both spawn markers, the orange battery station table and five yellow
   task panels. These are placeholders with no interactions.
6. Crouch through the low duct near the south of the main room. Release crouch
   while underneath: remain crouched until the ceiling is clear.
7. Escape in the client, click **Disconnect**, then **Start Client** again.
   The host should stay usable; the departed capsule disappears and a new one
   spawns on rejoin. Then disconnect the host, confirm the client returns to an
   offline state, start a new host, and reconnect the client.
8. Check the Editor Console and player logs for errors. Review mouse sensitivity
   using the panel slider, or change the shared tuning asset outside Play mode.

Alternatively, launch the executable twice and choose Host in one window and
Client in the other. Extra clients use the same address/port. The host is always
Viggo (wide orange body, Viggo spawn); up to seven clients are electricians
(blue body, electrician spawn area). Check the local role in both panels and
observe the other player's silhouette. Disconnect/rejoin retains this policy.
CLI shortcuts are `-ev-host`, `-ev-client`,
`-ev-address 127.0.0.1`, and `-ev-port 7777` in development builds.

## Assets and architecture

The development panel shows synchronized phase and time. A new host begins at
07:00 in Preparing, enters Workday after five seconds and reaches EndOfDay at
15:00 after two real minutes. Change `DayDurationMinutes` and `PreparingSeconds`
in `Assets/EscapeViggo/ScriptableObjects/Workday.asset` outside Play mode.
Rejoining a running host retains its time; restarting the host starts a new day.
Movement remains available at EndOfDay; later gameplay must check the phase.

- Scene: `Assets/EscapeViggo/Scenes/PrototypeWorksite.unity` is the build scene.
  A continuous foundation, three corridor connections, interconnecting rooms,
  cover, labels and a low duct provide traversal and hiding tests.
- Local prefab: `Assets/EscapeViggo/Prefabs/Player.prefab`.
  `PlayerInputReader`, `PlayerMovement`, and `PlayerLook` separate input,
  CharacterController physics, and camera responsibility.
- Network prefab: `Assets/EscapeViggo/Prefabs/NetworkPlayer.prefab`, with
  NetworkObject, owner-authoritative NetworkTransform, and NetworkPlayer.
  Local components start disabled and are enabled only for the owner.
- Tuning: `Assets/EscapeViggo/ScriptableObjects/PlayerMovement.asset`.
  Bindings: `Assets/EscapeViggo/UI/PlayerControls.inputactions`.
- Networking: `LocalNetworkSession` owns transport lifecycle, server-approved
  role spawn slots and connection status. The development panel only sends
  connection intentions and adjusts local mouse sensitivity.
- Registry: `Assets/DefaultNetworkPrefabs.asset` (NGO's standard generated path).
  Scene and prefab construction uses Unity Editor APIs, with Unity-generated GUIDs.

Movement is owner-authoritative for this local prototype. Important future
gameplay outcomes still require server validation. Remote CharacterControllers
are disabled; players currently pass through each other. Crouch synchronization
is presentation state. Role assignment is server-owned. No prediction, cloud sessions, Relay, or
gameplay rewards are included. The panel and smoke harness operate only in the
Editor/development builds; use a development build for this checkpoint.

## Packages

| Dependency | Version |
| --- | --- |
| Universal Render Pipeline | 17.3.0 |
| Input System | 1.20.0 |
| Unity Test Framework | 1.6.0 |
| Netcode for GameObjects | 2.13.3 |
| Unity Transport | 2.7.4 |

Unity's Package Manager reported NGO 2.13.2/2.13.3 compatible with this editor
and Transport 2.7.4 recommended. NGO 2.13.3 is the unsuffixed
[official package release](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/releases/tag/v2.13.3);
the chosen version resolves and passes the project build and two-process tests.
Package Manager generated the lockfile; no experimental
version suffixes or alternative networking frameworks were added.

## Automated validation

Close the project in Unity before running batch editor commands from the root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -Method EscapeViggo.Editor.NetworkSetup.Validate -LogName M3-validation
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -TestPlatform PlayMode -LogName M3-playmode
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -Method EscapeViggo.Editor.NetworkSetup.Build -LogName M3-build
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Test-LocalMultiplayer.ps1
```

The editor validator checks saved scene/prefab references, required locations,
4,697 floor samples and capsule routes. Seven Play Mode tests use the real
CharacterController against the worksite, including binding simulation, speed,
jump/landing, collision, headroom, look and room traversal.

The multiplayer script launches two separate player processes on port 17777,
moves both owners, compares their replicated positions/rotations/crouch states,
checks local-only components and camera/audio counts, disconnects and reconnects
the client, then restarts the host. A timeout, exception, nonzero exit or state
mismatch fails the run. It only terminates processes it launched.
Add `-Graphics` to exercise rendering and save camera renders (excluding IMGUI).
Logs, XML test results and screenshots are under ignored `Logs/`; builds are
under ignored `Builds/`. The PowerShell execution-policy override applies only
to that command process.

The **Escape Viggo > Validation** menus provide the same asset checks in Unity.
The **Setup** menus explicitly reconstruct M1, M2 or M3 assets and replace their
scene/prefab content. Use these only when intending to reset that checkpoint;
normal gameplay does not need any setup script. To reconstruct everything, run
M1, then M2, then M3. Existing tuning/material/input assets are preserved.

`Assets/Scenes/SampleScene.unity` and the template Input Actions remain for
reference; existing URP assignments are preserved. M0 history and each milestone's
acceptance evidence are recorded in `MILESTONES.md`.
