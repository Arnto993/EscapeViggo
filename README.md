# Escape Viggo

Unity 6.3 LTS (**6000.3.25f1**), URP, Windows PC. M0–M14 provide one local multiplayer
workday, with validation and a local commit at each checkpoint. Read [AGENTS.md](AGENTS.md),
[GAME_SPEC.md](GAME_SPEC.md), and [MILESTONES.md](MILESTONES.md) before changes.

## Open and play

M9 stamina tuning is in `Assets/EscapeViggo/ScriptableObjects/Viggo.asset`.
During Workday, moving Viggo drains 20 stamina/second from 100; standing still
recovers 15/second. Walk/chase/crouch speeds are 3.5/5.5/1.8 metres/second.
The movement threshold and drain/recovery conditions are configurable. Only
Viggo's development panel shows the value. The server measures horizontal
movement and owns stamina; electricians keep the original controller tuning.
To manually verify M9, follow the host/client steps below, move Viggo for two
seconds and stop: its value should fall and then return to 100. Shift changes
chase speed. Observe the same movement from the client, whose movement and HUD
remain unchanged. Continue moving for approximately five seconds to exhaust
Viggo: **Resting** and **Javell.** appear. Movement/sprint/jump input must not
bypass the default four-second rest, and the client sees Viggo stop. Stamina
recovers to 100, then movement resumes. Repeat: one Javell event per rest appears
in each instance's log. `RestSeconds`, `RestRecoveryPerSecond`, `RefillAtRestEnd`
and `RestMovementMultiplier` configure this behavior. Capture eligibility is
disabled during rest.

M11 adds the single battery to **Battery Station** at (17, 0.5, 7.5), in the
electrical room. As an electrician, aim at the station within three metres and
hold E for three seconds. Release E, look away, step away or release the cursor
mid-hold to cancel. Completion removes the yellow battery and shows the carrier
indicator only to its electrician. Viggo cannot steal it and receives no thief
identity or theft alert. Two electricians competing must produce one carrier.
`Battery.asset` controls the theft duration. Carrier disconnect returns the same
battery to its station; original thief history is retained until the next theft.
After theft, verify there is no Rage warning. As Viggo, return to the station,
aim within three metres and press E to **Check battery station**. A missing
battery is discovered once and all players see the Rage banner. Checking a
present battery has no effect; electricians cannot perform discovery. Repeated
checks must not restart Rage. During Rage, Viggo moves at `Viggo.asset`'s
`RageSpeed` (default 8 m/s), stamina is suspended and rest is disabled, including
an already-running rest. No exact thief identity appears in Viggo's feedback.
Rage stops at EndOfDay or battery recovery; recovery/capture end conditions are
configurable.

M13 capture: as Viggo, aim at an electrician within two metres and press E.
This works during normal Workday or Rage, but never while resting. Capturing
awards 50 points, with another 50 for the carrier, and deducts 25 from the
electrician (negative scores are allowed). Values are in `Scoring.asset`.
The captured player cannot move/jump/interact for three seconds, then returns
to their electrician spawn with two seconds of protection. Timing/range are in
`Capture.asset`. Repeated E during capture must not duplicate rewards. A captured
carrier loses the battery: it is secured at the station for the rest of the day,
Rage ends by default, and original thief history remains intact. This is the
prototype's reversible recovery rule; battery passing and dropped pickups are
not implemented. Check that host/client scores agree (100/75 after one task and
one carrier capture), and that the electrician can move/work again afterward.

1. Open this repository in Unity Hub with 6000.3.25f1.
2. Wait for package import, then open
   `Assets/EscapeViggo/Scenes/PrototypeWorksite.unity`.
3. Enter Play mode. In the development panel, leave IP `127.0.0.1` and port
   `7777`, then click **Start Host**.
4. WASD moves, mouse looks, Left Shift sprints, Space jumps, and held Left Ctrl
   or C crouches. Escape releases the mouse. **Resume controls** captures it.
   Losing window focus releases controls; networking continues in the background.
5. Check the Console for errors.

## Full workday manual test (M1–M14)

A ready development build is at `Builds/Windows/EscapeViggo.exe` (ignored by Git).
To rebuild it, use **Escape Viggo > Build > Windows Development Player**.

1. Before Play mode, set `Workday.asset`'s `DayDurationMinutes` to **5** for this
   walkthrough (default 2). In the Editor, play the worksite and click
   **Start Host** at `127.0.0.1:7777`. The host supplies the day duration.
2. Launch `Builds/Windows/EscapeViggo.exe`. Leave the same address/port and click
   **Start Client**. Both windows should display **Players: 2**.
3. Confirm the host panel says **Viggo**, at the orange Viggo marker in the
   electrical room. The client should say **Electrician**, at the blue spawn
   area. Both scores start at 0. Observe the wider orange Viggo and blue worker.
4. The host starts in **Preparing / 07:00**, then **Workday** after five seconds.
   A joining client receives current time. Both clocks should advance together.
5. Press Escape to switch windows and **Resume controls** to move. Walk, sprint,
   jump, crouch and look in each window; verify only its own player responds and
   the other window observes the movement without camera/audio conflicts.
6. As electrician, approach the **TEST INTERACTION** cube just northeast of the
   spawn (at -12, 1, -10). Look within three metres and press E. Both instances
   should show its confirmation count increase. Look away or step out of range:
   the prompt disappears.
7. Walk east along the south side of the main room to **SIMPLE CIRCUIT**, at
   Task Location 2 (-5, 0.7, -13). Bring Viggo here too using the corridor doors.
   Viggo should get no task prompt or UI when aiming at it and pressing E.
8. As electrician, aim at the panel within three metres and press E during
   Workday. The task UI opens and movement/look stop. Click **2** first: it
   reports wrong input and resets progress. Then click **1**, **3**, **2**.
9. The UI closes, the panel turns green and both instances show the same
   completing player ID. The electrician's score becomes **100**; Viggo stays
   at **0**. Both scoreboards must agree. Repeated E presses must not add points.
10. Optionally launch a third client now. It should receive an electrician
    role, the completed panel, the existing player's 100 points and current time.
11. Move Viggo continuously for about five seconds, using Shift to chase.
    Stamina falls from 100 to 0, then **Resting / Javell.** appears once. Hold
    movement/sprint/jump: Viggo stays stopped for four seconds, recovers to 100
    and can move again. Electricians remain mobile. Repeat to check one new
    Javell event per rest. Brief movement followed by standing still also
    recovers stamina without exhaustion.
12. Take the electrician to **Battery Station** in the electrical room
    (17, 0.5, 7.5). Before theft, have Viggo check it with E: no Rage. As the
    electrician, start holding E within three metres, then release, look away,
    or leave range before three seconds: theft cancels. Hold for the full three
    seconds. The yellow battery disappears and only its carrier gets the
    carrier indicator. With two electricians, competing holds must yield one
    carrier. Viggo cannot steal the battery.
13. Verify theft alone produces **no Rage warning** and identifies no thief to
    Viggo. As Viggo, return, aim at the station and press E to check it. All
    windows now show Rage; repeated checks must not restart it. Electricians
    cannot initiate discovery. Chase for more than five seconds: Viggo moves
    faster and never enters normal rest during Rage.
14. As Viggo, aim at the carrier outside two metres and press E: no capture.
    Move clearly inside two metres (about 1.5 m) and press E. The electrician
    is disabled for three seconds, loses 25 points, then returns to their spawn
    with two seconds of protection. Viggo receives 100 points (50 capture +
    50 carrier bonus); both scoreboards show **Viggo 100 / electrician 75**.
    Repeated E while captured must not duplicate points. The battery becomes
    **Secured** at its station and Rage ends. The worker can move/interact
    again. Electricians cannot capture either role.
15. Wait for **15:00**. Every window enters **EndOfDay**, releases the cursor,
    blocks movement and interactions, ends Rage and opens results. Compare
    player identifiers, roles, scores, tasks completed (**1** for the worker),
    captures (**1** for Viggo), and battery outcome (**Secured**). Repeated
    interaction or task input must not change these results.
16. Disconnect/rejoin one client after results appear: the same frozen table
    reappears, including its old identifier. A new join does not add a results
    row. Stop/restart the host and reconnect to begin a fresh day.
17. In a fresh session, leave a circuit task unfinished when 15:00 arrives:
    its UI closes, the task releases its worker, and no points are awarded.
    In another fresh session, steal/discover the battery and avoid capture
    until 15:00: Rage must stop. Restore the default two-minute tuning afterward.

During Workday, disconnect/rejoin also remains supported:
a rejoining player gets a new client ID and fresh score; completed world
tasks persist until host restart. A new host session resets tasks and scores.
Check the Editor Console and player logs for errors.

To check task cancellation, restart the host, begin a task,
click Cancel or disconnect mid-task, and confirm another electrician can begin.
The four other task locations remain placeholders; the battery station is interactive.
The low service duct near (-5, -6) still supports the M2 crouch/headroom test.

Alternatively, launch the executable twice and choose Host in one window and
Client in the other. Extra clients use the same address/port. The host is always
Viggo (wide orange body, Viggo spawn); up to seven clients are electricians
(blue body, electrician spawn area). Check the local role in both panels and
observe the other player's silhouette. Disconnect/rejoin retains this policy.
CLI shortcuts are `-ev-host`, `-ev-client`,
`-ev-address 127.0.0.1`, and `-ev-port 7777` in development builds.

## Assets and architecture

Near the electrician spawn, look at the yellow **TEST INTERACTION** cube within
three metres and press **E**. Its confirmation count must increase in both
windows. Look away, step outside range, or release the cursor: the prompt should
disappear. Range is serialized on `NetworkPlayer.prefab`'s `PlayerInteraction`.
The common interaction path checks range and line of sight on the server.

Task Location 2, near the south edge of the main room at (-5, 0.7, -13), is now
**SIMPLE CIRCUIT**. During Workday, approach as an electrician, aim and press E.
Click the displayed three-element sequence (default **1 > 3 > 2**). A wrong
button resets progress. Cancel returns to movement. Completion closes the UI,
turns the panel green, and shows the same worker ID in both windows. Viggo has
no task prompt and cannot complete it. A completed task stays completed until
the host restarts. Configure the sequence on `SimpleCircuitTask.prefab`.
Completion awards `Scoring.asset`'s `CircuitTaskPoints` (default 100). The local
HUD and small multiplayer scoreboard read synchronized player scores.

The development panel shows synchronized phase and time. A new host begins at
07:00 in Preparing, enters Workday after five seconds and reaches EndOfDay at
15:00 after two real minutes. Change `DayDurationMinutes` and `PreparingSeconds`
in `Assets/EscapeViggo/ScriptableObjects/Workday.asset` outside Play mode.
Rejoining a running host retains its time; restarting the host starts a new day.
At EndOfDay movement, tasks, theft and capture stop, and Rage ends. Score and
time configuration are read by the host. Results contain players connected at
the cutoff and remain unchanged by later disconnects/joins; there is no persistent
account identity or five-day week.

- Scene: `Assets/EscapeViggo/Scenes/PrototypeWorksite.unity` is the build scene.
  A continuous foundation, three corridor connections, interconnecting rooms,
  cover, labels and a low duct provide traversal and hiding tests.
- Local prefab: `Assets/EscapeViggo/Prefabs/Player.prefab`.
  `PlayerInputReader`, `PlayerMovement`, and `PlayerLook` separate input,
  CharacterController physics, and camera responsibility.
- Network prefab: `Assets/EscapeViggo/Prefabs/NetworkPlayer.prefab`, with
  NetworkObject, owner-authoritative NetworkTransform, and NetworkPlayer.
  Local movement components start disabled and are enabled only for the owner.
  `PlayerInteraction` gates camera/input by ownership. `PlayerScore` has a
  server-written NetworkVariable and no public score setter/RPC.
- Tuning: `Assets/EscapeViggo/ScriptableObjects/PlayerMovement.asset`.
  Bindings: `Assets/EscapeViggo/UI/PlayerControls.inputactions`.
- Networking: `LocalNetworkSession` owns transport lifecycle, server-approved
  role spawn slots and connection status. `NetworkWorldSpawner` creates
  registered interactive prefabs at saved markers on the server.
- `Workday.prefab` holds `NetworkWorkday`, `ScoreService`, `ViggoRage`, and
  `DailyResults` with separate Rage/results presentation. The clock publishes
  an atomic phase/time snapshot. Scoring listens to server task completion and
  captures and deduplicates by authoritative source object and event occurrence.
  The server finalizes results once at EndOfDay; server-written result rows and
  a summary synchronize to existing and late-joining clients.
- `ViggoStamina`, `PlayerCapture`, and `BatteryStation` own server-written
  mobility, capture and battery state. `ViggoRestFeedback`, `RagePresentation`,
  and `BatteryPresentation` read that state. `Viggo.asset`, `Capture.asset`,
  `Battery.asset`, and `Scoring.asset` centralize tuning. Carrier capture secures
  the same battery for the day; passing and dropped pickups are deferred.
- `WorkTask` owns worker/progress/completion; `SimpleCircuitTask` validates the
  configured sequence and input revision. `SimpleCircuitTaskPanel` handles UI.
  New task prefabs can reuse the interaction, task and scoring path; additional
  instances can be registered in the worksite's `NetworkWorldSpawner` entries.
- Registry: `Assets/DefaultNetworkPrefabs.asset` (NGO's standard generated path).
  Scene and prefab construction uses Unity Editor APIs, with Unity-generated GUIDs.

Movement is owner-authoritative for this local prototype. Interaction validates
server-observed position, range and line of sight; this is not anti-teleport
protection. Role, phase, task outcomes, stamina/rest, battery, Rage, captures,
scores and results are server-owned. Remote CharacterControllers
are disabled; players currently pass through each other. Crouch synchronization
is presentation state. No prediction, cloud sessions, Relay or M15+ gameplay
is included. The connection panel, scoreboard, test cube and smoke harness operate only in the
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
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -Method EscapeViggo.Editor.ResultsSetup.Validate -LogName M14-validation
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -TestPlatform EditMode -LogName M14-editmode
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -TestPlatform PlayMode -LogName M14-playmode
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -Method EscapeViggo.Editor.NetworkSetup.Build -LogName M14-build
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Test-LocalMultiplayer.ps1 -Graphics -Observer
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Test-LocalMultiplayer.ps1 -Graphics -Observer -EndDayStress
```

The editor validator checks saved scene/prefab references, required locations,
4,697 floor samples and capsule routes. The 46 Edit Mode cases cover role policy,
clock boundaries, task sequencing/replays, score deduplication/overflow, stamina,
rest, battery/discovery transitions and capture eligibility/battery consequences.
Twelve Play Mode tests cover the original seven controller checks plus aiming,
range, walls, invalid directions, ignoring the player's own colliders and capture
trigger selection.

The multiplayer script launches two separate player processes on port 17777,
moves both owners, compares their replicated positions/rotations/crouch states,
checks local-only components and camera/audio counts, disconnects and reconnects
the client, completes the circuit with deliberate invalid/duplicate requests,
checks stamina/rest/Javell, theft/cancellation/contention, discovery/Rage,
capture/reward/penalty/respawn, EndOfDay blocking and matching frozen results,
then restarts the host. The smoke-only day lasts
120 seconds after preparation. `-Observer` launches a third client after task
completion and verifies late-join roles, task state, scores and clock, then
disconnects/rejoins after EndOfDay to verify the frozen results. `-EndDayStress`
uses runtime-only test tuning and resets the test task to leave an unfinished
interaction and Rage active at 15:00; both must stop. These fixtures run only
with explicit development smoke arguments and never save asset tuning.
A timeout, exception, nonzero exit or state
mismatch fails the run. It only terminates processes it launched.
Add `-Graphics` to exercise rendering and save camera renders (excluding IMGUI).
Logs, XML test results and screenshots are under ignored `Logs/`; builds are
under ignored `Builds/`. The PowerShell execution-policy override applies only
to that command process.

The **Escape Viggo > Validation** menus provide the same asset checks in Unity.
The **Setup** menus reconstruct or apply milestone assets and can replace
scene/prefab content. Use these only when intending to reset that checkpoint;
normal gameplay does not need any setup script. To reconstruct everything, run
M1, M2, M3, then Apply M4, M5, M6, M7, M8, M10, M11, M12, M13 and M14 in order. Earlier setup menus do not
restore later integrations. Existing tuning/material/input assets are preserved.

`Assets/Scenes/SampleScene.unity` and the template Input Actions remain for
reference; existing URP assignments are preserved. M0 history and each milestone's
acceptance evidence are recorded in `MILESTONES.md`.

M14 validation: `Logs/M14-editmode.xml` (46/46), `Logs/M14-playmode.xml` (12/12),
`Logs/M14-build.log`, and the graphics-enabled three-process full-loop run in
`Logs/Multiplayer-20260929-232125`. Host/client/observer received identical
results: Viggo 100 points / 1 capture, worker 75 points / 1 task, second worker
0 points; battery Secured. Post-cutoff requests and reconnect did not change
the table. Human mouse feel and IMGUI clicks remain a manual review; the harness
exercises the same interaction/RPC and UI state paths but does not automate
screen clicks. Camera captures exclude IMGUI. Unity licensing and engine shutdown
allocation diagnostics remain in logs, including baseline runs; successful runs
have no C# warnings/errors or gameplay/network exceptions.

The additional `-EndDayStress` run in `Logs/Multiplayer-20260929-232358` passed
with an unfinished task and Rage active at the cutoff, plus the same full-loop,
negative-request and late-results-join checks. Across M9–M14 checkpoints, the
Edit/Play suites executed 300 passing cases (58 cases in the final suite), and
seven successful three-process multiplayer runs covered the growing loop.

M15 adds a local role/score/clock HUD, objectives and interaction feedback,
private battery carry/progress/cancellation, stamina/rest/Javell, Rage warning,
capture timeout, task/score notifications and readable frozen results. Escape
releases controls and exposes the connection panel; diagnostics are optional.
Task and results UI use a shared 1280x720 layout. Results include a disconnect
button. All gameplay state still comes from the server.

M15 engineering checks passed: 46 Edit Mode, 12 Play Mode, asset validation,
Windows build and three-process full workday (`Logs/Multiplayer-20260930-121318`).
The harness also verifies owner-only HUD, carrier secrecy, cancellation feedback
and Rage warning. Camera renders work; attempted hidden-window UI screenshots
are black and cannot establish visual correctness. Review the HUD, task buttons,
results, readability at 1280x720/1920x1080 and mouse capture manually using the
walkthrough above. M16 is next; M17 is outside this request.
