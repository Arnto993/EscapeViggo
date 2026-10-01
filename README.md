# Escape Viggo

Unity 6.3 LTS (**6000.3.25f1**), URP, Windows PC. M0–M16 provide the playable workday;
M17 adds private online Sessions/Relay, with separate-network acceptance pending.
M18–M20 add a five-day week, four electrical task types and two Viggo distractions;
human fun/balance review remains pending. Read [AGENTS.md](AGENTS.md),
[GAME_SPEC.md](GAME_SPEC.md), and [MILESTONES.md](MILESTONES.md) before changes.

## How to Playtest Escape Viggo

Use `Builds/Windows/EscapeViggo.exe`, or build with **Escape Viggo > Build >
Windows Development Player** in Unity 6000.3.25f1. The game opens directly at the
PrototypeWorksite connection menu. Online play uses an anonymous Unity account;
**Local Development** needs no services or accounts.

1. Launch the game. Select **Local Development**. Leave IP **127.0.0.1** and port **7777** for one-PC testing.
   Select **Guided: 5 min**, then **Start Host**. The host is Viggo.
2. Launch one or two more copies, use the same IP/port and choose **Start Client**.
   Clients become electricians. The day starts automatically five seconds after
   hosting; join promptly. The menu shows connection status and player count.
3. **WASD** moves, **mouse** looks, **Shift** sprints, **Space** jumps,
   held **Ctrl/C** crouches, and **E** interacts. **Esc** releases the mouse and
   opens the menu. Choose **Resume controls** after switching windows. Task UI
   owns the mouse until you complete or cancel it.
4. As electrician, find **SIMPLE CIRCUIT** on the south side of the main room
   (Task Location 2). Aim within three metres, press **E**, then click **1, 3, 2**.
   Wrong input resets progress. Completion awards **100** points and turns the
   panel green. This is the one real work task; other task markers are placeholders.
5. As Viggo, move until stamina runs out: **Javell.** and a rest countdown appear.
   You cannot move or capture during the default four-second rest.
6. Take an electrician to **Battery Station** in the electrical room. Hold **E**
   for three seconds while aiming within range. Release or look away to cancel.
   Only the carrier sees the carrying indicator. Theft alone does **not** start Rage.
7. Have Viggo return and press **E** at the station. Missing battery discovery
   starts the warning for everyone; it never names the thief. Viggo can now chase
   continuously. Use rooms and cover to hide.
8. As Viggo, aim at the carrier from clearly inside two metres and press **E**.
   The worker is disabled for three seconds, loses **25** points and respawns.
   Viggo gains **100** for this carrier capture; the battery is secured and Rage
   ends. After one task and one carrier capture, scores are **Viggo 100 / worker 75**.
9. At **15:00**, compare all windows: gameplay stops and the same final table
   shows player, role, score, tasks and captures, plus the battery outcome.
   Choose **Disconnect / return to start**. Disconnect all clients, restart the
   host, and reconnect clients to play another fresh day.

For a quick launch of three interactive windows from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Start-LocalPlaytest.ps1 -Players 3 -Minutes 5
```

Use `-Players 2` for two windows or `-Minutes 2` for a quick day. The launcher
uses ordinary gameplay, not the automated test driver. Close each window with
**Quit game**. Do not start another host on the same port while one is running.
The menu presets and `-ev-day-minutes 2` override only the next host's runtime
configuration; **Asset tuning** uses `Workday.asset`. No asset editing is needed.

For another PC on the same LAN, enter the host's LAN IP in both host and client
menus and use the same port. For separate networks, use the online flow below.
A failed connection times out with retry guidance. A reconnect gets a new player
ID and score zero; completed tasks remain completed until the host restarts.
Results remain frozen even after reconnect. Host disconnect ends that session.

Human review: check HUD readability at **1280x720** and **1920x1080**, actual
button clicks, mouse resume after task cancel/completion or window focus loss,
and whether the objectives/warnings make sense to someone new. In another day,
leave a task open or Rage active at 15:00; both must stop. New mechanics, final
art/audio and M18 are outside this build.

## Online play setup (M17)

The existing EscapeViggo Cloud Project was successfully used for real anonymous
authentication and Sessions/Relay connections. **No manual linking is required
for this checkout.** The SDK uses the `production` environment, configurable on
**Local Network > Online Network Session** in `PrototypeWorksite`.

If another checkout loses its project link or online access is refused:

1. Open Unity **6000.3.25f1**, sign in with the Unity account that owns/accesses
   the project, then open **Edit > Project Settings > Services**.
2. Check the organization and linked project are **arnto993 / EscapeViggo**.
   If unlinked, choose **Use an existing Unity Cloud Project**, select that
   organization and **EscapeViggo**, then **Link project ID**. Do not create a
   replacement project or paste credentials into game scripts.
3. Open **Unity Dashboard** from the Services settings (or
   [Unity Cloud Dashboard](https://cloud.unity.com/)), select the same organization
   and project, then **Gaming Services > Multiplayer**. If onboarding is shown,
   click **Set up / Get started** for **Relay** and **Lobby**. Sessions uses both
   through the unified SDK; no standalone packages are needed.
4. Under **Gaming Services > Authentication**, complete **Get started** if shown.
   Anonymous sign-in does not require an external identity provider or player
   registration. Select **production** in the environment selector; if using a
   different environment, set the scene component to that exact environment name.
5. Let Package Manager finish, check the Console, then choose **Escape Viggo >
   Validation > Validate M17 Online Sessions** and **Escape Viggo > Build > Windows
   Development Player**. Distribute the entire `Builds/Windows` folder to both PCs.

Unity UI labels may vary by Dashboard layout. The repository stores only its
existing non-secret project association; anonymous credentials are managed by
Unity Authentication in each player's local storage.

## Host and join online

1. PC A: launch `EscapeViggo.exe`, choose **Quick: 2 min** or **Guided: 5 min**, then
   **Host Online Game**. Services initialize and authenticate automatically.
2. Wait for **Connected online**. Read the six-character **Join Code** at the top
   right, or press **Esc > Copy join code** and share it privately.
3. PC B on a different connection (for example mobile hotspot): launch the same
   build, enter the code and click **Join Online Game**. Input accepts lowercase
   and trims surrounding whitespace. No IP address or port forwarding is needed.
4. The host is Viggo; joining players are electricians. Capacity follows the
   saved spawn slots (currently eight total). The five-second preparation period
   starts when the host spawns, so join promptly. Existing late joining works.
5. Follow steps 3–9 in **How to Playtest Escape Viggo** above. Compare movement,
   task completion, scores, theft, delayed discovery/Rage, capture/respawn and
   the same frozen results at **15:00** on both PCs.
6. Choose **Disconnect / return to start**. Verify the overview/menu returns and
   a new host/join attempt works. Also force-close PC A: PC B must report host
   loss, release the mouse and recover to the connection menu. No host migration.
7. Try an empty code, a mistyped code, an ended session's code and a full session.
   Verify useful feedback and retry. Disconnect before switching to **Local
   Development**, then verify `127.0.0.1:7777` still works without cloud access.

Architecture: `OnlineServices` owns UGS initialization and anonymous sign-in;
`OnlineNetworkSession` creates/joins private Multiplayer Sessions and uses
`WithRelayNetwork()` plus DTLS network options. The SDK's NGO handler starts and
stops networking. `LocalNetworkSession` retains approval, spawn slots and world
callbacks. Gameplay authority is unchanged. Host leave deletes the session;
clients leave membership. Join code is the **Session code**, not a Relay allocation
code. UI reads connection state and never assigns gameplay outcomes.

Only one create/join/leave operation can run at once. A 60-second connection
deadline requests cleanup. MPS create/join APIs have no cancellation token: a
pending SDK request must settle before retry/local fallback becomes available,
so a late response cannot start a second game. Failure feedback remains visible
during cleanup. Closing the application also ends pending work. After shutdown,
a 0.25-second transport reuse delay prevents the immediate local reconnect race
found by the headless observer test.

For automated **real** cloud testing with two isolated authentication profiles:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Test-OnlineMultiplayer.ps1 -Graphics
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Test-OnlineMultiplayer.ps1 -Graphics -Recovery -KillHost
```

The full-loop test reuses the gameplay assertions from local testing. A shared
file barrier only aligns movement baseline sampling between processes; all
gameplay travels through real Sessions/DTLS Relay. `-Recovery` tests graceful
host leave; adding `-KillHost` tests abrupt process loss, online retry and switching
to direct transport. `-EndDayStress` leaves a task and Rage active at cutoff.
Logs are under ignored `Logs/Online-*`. These are **one-PC tests**, not proof of
two PCs on separate networks. Separate-network play and human UI/input review
remain the final M17 acceptance check.

For two interactive online copies on the same Windows account, start each with
a distinct `-ev-online-profile` value (for example `host` and `friend`); otherwise
Unity Authentication may reuse the same cached player identity. Distinct PCs
need no special arguments. The local launcher remains unchanged.

Packages: unified Multiplayer Services **2.3.3**, with Package Manager-resolved
Authentication **3.7.4** and Core **1.18.0**. NGO **2.13.3** and Transport **2.7.4**
are unchanged. No deprecated standalone Lobby/Relay SDKs were added. Unity's
[session networking guide](https://docs.unity.com/en-us/mps-sdk/manage-session-network-connection)
and [SDK installation guide](https://docs.unity.com/en-us/mps-sdk/install-and-upgrade)
describe the supported integration.

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

1. In the Editor, play the worksite, select **Guided: 5 min**, and click
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
   at **0**. Check both HUD scores or enable Development diagnostics in the menu. Repeated E presses must not add points.
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
    50 carrier bonus); the respective HUD scores show **Viggo 100 / electrician 75**.
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
    until 15:00: Rage must stop. Select **Quick: 2 min** for subsequent short tests.

During Workday, disconnect/rejoin also remains supported:
a rejoining player gets a new client ID and fresh score; completed world
tasks persist until the next workday. A new host session resets tasks and scores.
Check the Editor Console and player logs for errors.

To check task cancellation, restart the host, begin a task,
click Cancel or disconnect mid-task, and confirm another electrician can begin.
All five task locations and the battery station are interactive (see M19 below).
The low service duct near (-5, -6) still supports the M2 crouch/headroom test.

Alternatively, launch the executable twice and choose Host in one window and
Client in the other. Extra clients use the same address/port. The host starts as
Viggo on Monday (wide orange body, Viggo spawn); clients start as electricians
(blue body, electrician spawn area). Roles rotate daily. Check the local role in
both panels and observe the other player's silhouette.
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
HUD and optional development diagnostics read synchronized player scores.

The development panel shows synchronized phase and time. A new host begins at
07:00 in Preparing, enters Workday after five seconds and reaches EndOfDay at
15:00 after the selected preset duration (default Guided: five real minutes).
Select Quick for two minutes or Asset tuning to use `DayDurationMinutes` from
`Assets/EscapeViggo/ScriptableObjects/Workday.asset`; `PreparingSeconds` remains
configurable there outside Play mode.
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
is presentation state. M17 adds private cloud sessions and Relay; gameplay
authority is unchanged. The online connection menu is available in player builds.
Direct-local controls, diagnostic UI, test cube and smoke fixtures remain
Editor/development-only. No prediction or M18 gameplay is included.

## Packages

| Dependency | Version |
| --- | --- |
| Universal Render Pipeline | 17.3.0 |
| Input System | 1.20.0 |
| Unity Test Framework | 1.6.0 |
| Netcode for GameObjects | 2.13.3 |
| Unity Transport | 2.7.4 |
| Multiplayer Services | 2.3.3 |
| Authentication (transitive) | 3.7.4 |
| Services Core (transitive) | 1.18.0 |

Unity's Package Manager reported NGO 2.13.2/2.13.3 compatible with this editor
and Transport 2.7.4 recommended. NGO 2.13.3 is the unsuffixed
[official package release](https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/releases/tag/v2.13.3);
the chosen version resolves and passes the project build and two-process tests.
Package Manager generated the lockfile; no experimental
version suffixes or alternative networking frameworks were added.

## Automated validation

Close the project in Unity before running batch editor commands from the root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -Method EscapeViggo.Editor.OnlineSessionSetup.Validate -LogName M17-validation
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -TestPlatform EditMode -LogName M17-editmode
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -TestPlatform PlayMode -LogName M17-playmode
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -Method EscapeViggo.Editor.NetworkSetup.Build -LogName M17-build
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Test-LocalMultiplayer.ps1 -Graphics -Observer
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Test-LocalMultiplayer.ps1 -Graphics -Observer -EndDayStress
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Test-LocalMultiplayer.ps1 -ConnectionOnly
```

The editor validator checks saved scene/prefab references, required locations,
4,697 floor samples and capsule routes. The 53 Edit Mode cases cover role policy,
clock boundaries, task sequencing/replays, score deduplication/overflow, stamina,
rest, battery/discovery transitions and capture eligibility/battery consequences.
Thirteen Play Mode tests cover the original seven controller checks plus aiming,
range, walls, invalid directions, ignoring the player's own colliders and capture
trigger selection and task-modal resume/release.

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
M1, M2, M3, then Apply M4, M5, M6, M7, M8, M10, M11, M12, M13, M14 and M15 in order. Earlier setup menus do not
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
walkthrough above.

M16 validation adds seven duration-input cases and one modal-control regression:
`Logs/M16-editmode.xml` **53/53**, `Logs/M16-playmode.xml` **13/13**. The final
Windows build and full asset validation are in `Logs/M16-final-build.log`.
The three-process full workday passed in `Logs/Multiplayer-20260930-122230-17777`.
The final connection test passed in `Logs/Multiplayer-20260930-122535-18777`:
unreachable host, timeout guidance retained through disconnect callbacks,
retry, selected runtime duration, results, preset change on restart and cursor
cleanup. The launcher passed PowerShell syntax validation; actual mouse clicks
and window arrangement are part of human review.

`-ConnectionOnly` exercises connection recovery, three rapid reconnects and runtime day presets without
the full gameplay fixture. Normal full-loop and `-EndDayStress` runs retain all
prior authority/state regression checks. Hidden framebuffer captures are detected
as unavailable; only valid camera renders count as render evidence. The normal
launcher never enables test fixtures or edits assets. This records the M16 checkpoint.

The final two-process full-loop stress test also passed in
`Logs/Multiplayer-20260930-122535-19777`: task UI and Rage were active at 15:00,
then stopped; both processes agreed on frozen results and restart succeeded.
M15/M16 engineering work is finished. Human UI/usability acceptance remains the
walkthrough above. M17 validation is recorded in `MILESTONES.md`.

## Five-day work week (M18)

The host starts as Viggo on Monday. At 15:00, daily results freeze and weekly
scores/statistics accumulate. After a minimum three-second review, only the host
can click **Continue**. Tuesday through Friday reuse the same connected players,
world objects and local/Relay session. Friday has final week results and no Day 6.
Viggo rotates to the least-used connected player, excluding yesterday's Viggo
when possible. A departing client Viggo is replaced by the server during gameplay.

Daily tasks, battery ownership/discovery, Rage, captures, stamina/rest, scores and
transient feedback reset. Weekly totals and completed-day rows remain independent.
Results identify network client IDs: reconnect creates a new ID (as before), not
an account identity. Earlier completed-day totals remain listed under the old ID;
unfinished-day scores for a disconnected ID are not restored.

For a human five-day check, build the Windows development player, then run:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Start-LocalPlaytest.ps1 -Players 3 -DebugWeek
```

Alternatively choose **Debug week: 30 sec** before hosting online. Keep Guided or
Asset tuning for normal play. Each day, verify the weekday and 07:00, one Viggo,
role-specific HUD/visuals and spawn positions. Complete work, steal the battery,
have Viggo inspect it, and try a capture. At 15:00 compare all three results,
then use Continue on the host. Verify clean task/battery/rest/capture state and
persistent week totals on the next day. Repeat through Friday; confirm final
score, tasks, captures, days as Viggo and battery statistics, with no Continue.
Reconnect a client at Friday and compare frozen results. Repeat with online join
codes; use two separate Internet connections for the remaining M17 human check.

Engineering commands (run sequentially for Unity editor operations):

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Invoke-Unity.ps1 -Method EscapeViggo.Editor.WeekSetup.Validate -LogName M18-validation
powershell -ExecutionPolicy Bypass -File Tools/Test-WorkWeek.ps1
powershell -ExecutionPolicy Bypass -File Tools/Test-WorkWeek.ps1 -Online -Port 24778
```

The week harness drives actual task/theft/discovery requests. Its even-day fixture
extends capture/rest durations in memory only, so those states cross the cutoff.
The ordinary playtest launcher does not enable fixtures or automated input.

## Electrical tasks (M19)

All five existing task locations now contain real work. Simple Circuit stays in
the main room (location 2). Locations 1 and 3 alternate Voltage Measurement and
Outlet Replacement on successive days. Location 4 offers Breaker Installation;
location 5 is another rotating outlet/voltage station. Each ElectricalWorkTask
prefab has a small **Daily Types** array; the server chooses its weekday entry.

- Simple Circuit: press the displayed three-element sequence.
- Voltage Measurement: choose AC voltage, place black on N and red on L, then
  compare the displayed 230 V reading with the work order.
- Outlet Replacement: remove the faceplate; disconnect all three conductors in
  any order; select each labelled wire and its matching L/N/PE terminal; fit the
  outlet and replace the faceplate. Both labels and colours identify wires.
- Breaker Installation: match the 16 A/slot 2 work order, attach supply and load,
  switch on and confirm the indicator.

Default rewards are 100/120/150/180 respectively in `Scoring.asset`. All rewards
come through the existing server ScoreService. One player can own one active task;
other workers cannot take it over. Cancel, range loss, capture, disconnect and
15:00 release unfinished work. Completion stays shared until the next day.

Human check: in the three-window Debug Week, complete a different task in each
window/day as roles rotate. Try a wrong mode/terminal/rating, Cancel and reopen,
and two workers using the same station. Compare the score recipient and amount.
Leave an open task at 15:00; verify the modal closes, next day starts clean, and
the alternate task types appear. Repeat the five-day procedure above with Relay.
`Test-WorkWeek.ps1` automatically exercises the same task rules, cancellation,
contention, duplicate requests, scoring, unfinished cutoff and weekday reset.

## Viggo distractions (M20)

Before theft, Viggo can aim at an active task station and press E to interrupt
meaningful progress. Work closes and resets, the worker can retry after 2 seconds,
and Viggo receives 15 points once. The interruption has an 8-second cooldown and
requires proximity, line of sight, an active Workday and a non-resting Viggo.
Starting an untouched task does not provide interruption points.

The labelled POWER CONTROL at (-1, 0.8, -10) interrupts all active electrical work
for 6 seconds. Viggo receives 25 points only if at least one task lost progress.
Electricians can restore power there after a 2-second safety delay. A 20-second
Viggo cooldown leaves a recovery window; repeated requests cannot extend an outage.
The HUD shows outages and Viggo cooldowns. No final animation/audio is required:
WorkDistractions exposes WorkInterrupted, PowerCut and PowerRestored events.
Distractions.asset and Scoring.asset hold tuning. EndOfDay restores power; the next
day clears cooldowns and transfers eligibility with the Viggo role.

For the complete human acceptance check, use three people/windows and two-minute
days so there is time to read each task:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/Start-LocalPlaytest.ps1 -Players 3 -Minutes 2
```

1. Monday: complete one correct circuit input, then have Viggo approach the station
   and press E. Verify closed UI, lost progress, +15 once, two-second retry and
   cooldown rejection. Reopen and complete the task. Try an empty power cut: no points.
2. Tuesday: verify roles rotate and scores/cooldowns reset. Start work, make progress,
   then cut power. Verify +25 once and blocked tasks. The other electrician restores
   at POWER CONTROL after two seconds; finish the interrupted task.
3. Wednesday: repeat the power cut, but let it restore automatically. Complete Voltage
   Measurement and Outlet Replacement; deliberately try a wrong input and retry.
4. Thursday: complete Breaker Installation. Steal the battery, delay Viggo's inspection,
   then inspect and capture the carrier. Exercise normal stamina exhaustion/Javell rest.
5. Friday: leave work open and cut power close to 15:00. Confirm cleanup and matching
   daily/final totals in all windows, including distraction points. Verify no Continue
   or sixth day. Reconnect a client and compare the frozen results again.

At every daily result, compare scores, then only the host clicks Continue. Check
07:00, rotated spawns, fresh tasks/battery/rest and preserved weekly totals. Repeat
using Host Online and the join code, ideally on separate PCs/Internet connections.
Judge whether normal Viggo play is entertaining, whether electricians have enough
recovery time, and whether prompts/modal controls are readable at 720p and 1080p.
Automated state checks cannot establish those human acceptance criteria.

## Audio and game feel (M21)

`Assets/EscapeViggo/ScriptableObjects/GameFeel.asset` configures master, effects,
ambience and voice volumes, cue clips/variation/range/cooldown, footstep stride,
crouch loudness, and optional screen-edge opacity (zero disables the edges).
The checked-in sounds under `Assets/EscapeViggo/Audio/Development` are neutral,
generated placeholders, not final recordings. Replace clips in the settings asset
with approved assets. `VoiceJavell`, `VoiceRage`, `VoiceCapture` and
`VoiceBatteryRecovered` are intentionally empty; gameplay works with every clip
removed or master volume at zero. Javell uses the existing rest-start event.

Use **Escape Viggo > Setup > Apply M21 Audio and Game Feel** to wire missing
components; rerunning preserves the settings asset and assigned clips. The
validation menu checks every cue and all affected prefabs. Audio is entirely
local presentation of existing replicated state/events; there are no audio RPCs.
Nearby player/task/battery effects are spatial; ambience, Rage alerts, power and
day/week transitions are shared non-spatial cues. Rest has a slow blue edge,
Rage a steady red edge, and capture an amber edge. Existing gameplay HUD remains.

Human listening check: launch a host and two clients with two-minute days using
the command above. Listen to one window/device at a time on a shared PC.
Walk/sprint/crouch and jump as both roles; nearby footsteps should differ and stop
in the air. Exhaust Viggo, then steal/discover/recover the battery and capture a
worker. Check blue rest, red Rage, amber capture, and the matching sound changes.
Try correct/wrong task inputs, completion, interruption, power cut/restoration,
and daily/Friday results. Reconnect during Rage/rest and at Friday results:
ongoing loops resume, old one-shots do not replay. Repeat muted. Judge audibility,
spatial range, timing and overlapping cues with actual players before accepting
the mix; final voice timing needs approved recordings.
