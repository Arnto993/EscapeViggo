# ESCAPE VIGGO — DEVELOPMENT MILESTONES

This roadmap defines the intended development order.

Do not skip forward unless explicitly requested.

Each milestone should leave the project in a usable state.

---

# ROADMAP OVERVIEW

## Phase A — Foundation

- M0 — Repository and Project Health
- M1 — Prototype Worksite
- M2 — First-Person Controller
- M3 — Local Multiplayer Foundation

## Phase B — Core Match

- M4 — Roles and Player Spawning
- M5 — Workday Clock and Match State
- M6 — Interaction Framework
- M7 — First Electrical Task
- M8 — Scoring

## Phase C — Viggo

- M9 — Viggo Movement and Stamina
- M10 — Viggo Rest
- M11 — Battery Theft
- M12 — Battery Discovery and Rage
- M13 — Capture System

## Phase D — Complete Vertical Slice

- M14 — End-of-Day Flow
- M15 — Prototype UI and Feedback
- M16 — Local Multiplayer Playtest Build
- M17 — Online Session and Relay

## Phase E — First Proper Game Loop

- M18 — Five-Day Week
- M19 — Multiple Work Tasks
- M20 — Viggo Distraction Expansion
- M21 — Audio and Game Feel
- M22 — First Art Pass
- M23 — Balance and Playtesting

Do not begin Phase E until the vertical slice is fun enough to justify expansion.

---

# M0 — REPOSITORY AND PROJECT HEALTH

## Goal

Create a stable, agent-friendly Unity project foundation.

## Requirements

Confirm:

- Unity 6.3 LTS project.
- Universal Render Pipeline.
- Git repository.
- Unity-appropriate `.gitignore`.
- `GAME_SPEC.md`.
- `AGENTS.md`.
- `MILESTONES.md`.

Create clean project structure.

Suggested:

`Assets/EscapeViggo/Scenes`

`Assets/EscapeViggo/Scripts`

`Assets/EscapeViggo/Prefabs`

`Assets/EscapeViggo/ScriptableObjects`

`Assets/EscapeViggo/UI`

`Assets/EscapeViggo/Materials`

`Assets/EscapeViggo/Audio`

`Assets/EscapeViggo/Tests`

Ensure required basic packages are available.

Do NOT configure cloud multiplayer yet.

## Deliverable

A clean Unity project that opens and compiles without errors.

## Acceptance Criteria

- Unity project opens successfully.
- No C# compiler errors.
- Folder structure exists.
- Documentation exists in repository root.
- Git ignores Unity-generated folders.
- Existing sample/demo content that is not needed has been removed only when safe.
- No gameplay has been unnecessarily implemented.

## Manual Test

Open the project in Unity.

Confirm Console contains no compile errors.

## M0 Status — 2026-09-29

Complete, as confirmed by the user when requesting M1–M3. The original M0
automated evidence and manual verification procedure are retained below.

- Renamed `GAME_SPECS.md` to the required `GAME_SPEC.md` without changing its content.
- Created `Assets/EscapeViggo` folders with Git placeholders and Unity-generated metadata.
- Added `.slnx` to `.gitignore` and untracked the generated solution, retaining the local file.
- Removed unreferenced template welcome/readme assets. Retained `SampleScene`,
  referenced URP settings/profiles, and registered Input Actions.
- Added `README.md` with setup, package inventory, and validation instructions.
- Confirmed Unity `6000.3.25f1`, URP `17.3.0`, Input System `1.20.0`, and Test
  Framework `1.6.0`; package manifest and lock versions agree. No packages were changed.
- Unity completed batch import and script compilation with no C# compiler errors
  and logged successful exit code `0` in `Logs/M0-validation.log` (ignored by Git).
  The initial sandbox attempt could not connect to licensing; the successful
  retry ran outside the sandbox. Licensing/token diagnostics and a shutdown
  `Curl error 42` remain in the log; this is not a claim of a completely clean Console.
- Unity serialized `overrideShaderVariantLimit: 0` in `ShaderGraphSettings.asset`.
  This editor-written field was retained; editor version, pipeline assignments,
  rendering assets, input configuration, and all other project settings are unchanged.
- Repository checks cover generated-file exclusions, required folders/documents,
  asset metadata, and preserved source references. No project automated tests exist yet.

Remaining manual check: open with Unity `6000.3.25f1`, let import finish, open
`Assets/Scenes/SampleScene.unity`, and confirm the Console has no compile errors.
Enter Play mode, confirm the camera renders the skybox without runtime errors,
then exit Play mode. Check the new folders and Package Manager for resolution
errors. Headless validation did not verify rendering or interactive Play mode.

---

# M1 — PROTOTYPE WORKSITE

## Goal

Create a simple greybox environment for testing all future systems.

## Requirements

Create:

**PrototypeWorksite**

Use Unity primitives.

Required spaces:

- Electrician spawn area.
- Viggo spawn area.
- Main workspace.
- Corridor.
- Electrical room.
- Storage room.
- Small office.
- Battery station.
- At least five marked task positions.
- Multiple obstacles and line-of-sight breaks.

Use simple materials to distinguish important areas.

No final art.

## Design Requirements

Map should support:

- Running loops.
- Chases.
- Hiding behind geometry.
- Multiple paths.
- Easy navigation during development.

## Deliverable

A playable greybox scene.

## Acceptance Criteria

- Player-sized capsule can traverse map.
- No obvious holes in floor.
- Key areas are labelled or visually identifiable.
- Spawn locations exist.
- Battery station exists.
- Five task locations exist.
- Scene is saved correctly.
- Project still compiles.

---

## M1 Status — 2026-09-29

Acceptance criteria satisfied by Unity editor validation. Created the saved
primitive worksite with labelled rooms, two connected loops, cover, spawn
markers, battery station marker and five future task locations. A 1.8 m tall,
0.7 m wide capsule sweep traverses both loops and all rooms; 4,697 foundation
samples found no holes. Required objects and missing scripts were checked.
Unity batch compilation/validation exited 0 (`Logs/M1-validation.log`).
Interactive rendering and movement feel still need the manual walkthrough.
M0 is treated as complete following the user's confirmation for this task.

---

# M2 — FIRST-PERSON CONTROLLER

## Goal

Create reliable local first-person movement.

## Required Features

- WASD.
- Mouse look.
- Sprint.
- Jump.
- Crouch.
- Ground detection.
- Configurable sensitivity.
- Configurable movement values.

Use Unity Input System.

## Requirements

Movement should live in focused components.

Camera only responds to the local player's input.

Do not add networking yet unless required by existing architecture.

## Deliverable

One controllable first-person player in PrototypeWorksite.

## Acceptance Criteria

Player can:

- Walk.
- Look.
- Sprint.
- Jump.
- Crouch.
- Navigate prototype map.

No continuous console errors.

Input values are not scattered as hard-coded keys throughout gameplay scripts.

---

## M2 Status — 2026-09-29

Acceptance criteria satisfied by asset validation and seven passing Play Mode
tests (`Logs/M2-playmode.xml`). Player prefab and scene are wired; Input System
bindings, walk/sprint/diagonal speed, jump/landing/no air jump, collision, crouch
headroom, mouse sensitivity/pitch clamp and actual room-loop traversal passed.
Unity compilation and asset validation exited 0 (`Logs/M2-setup.log`). No
gameplay exceptions were reported by the tests. Interactive mouse feel/rendering
remain a manual review; the headless input test explicitly ignores editor focus
and restores the settings afterwards. No networking was added at this checkpoint.

---

# M3 — LOCAL MULTIPLAYER FOUNDATION

## Goal

Prove that multiple players can exist in the same networked game.

## Technology

Use:

**Netcode for GameObjects**

Use Unity Transport.

Cloud Relay is NOT required yet.

## Required Features

Create a basic development connection flow:

- Start Host.
- Start Client.

Network player prefab must include required NGO components.

Players should spawn when connected.

## Movement

Each player controls only their own character.

Other players' movement must synchronize.

Remote cameras and audio listeners must not interfere with the local player.

## Deliverable

At least two local game instances can connect and move independently.

## Acceptance Criteria

- Host starts successfully.
- Client connects.
- Both players spawn.
- Host controls only host character.
- Client controls only client character.
- Movement is visible on both instances.
- Disconnect does not permanently break project state.
- No critical network errors.

## Manual Test

Launch host and client.

Walk both players to the same room.

Move them independently.

Confirm each instance observes the other's movement.

---

## M3 Status — 2026-09-29

Acceptance criteria satisfied. Unity compiled the project and built a Windows
development player successfully (`Logs/M3-build.log`). Scene/prefab validators
pass and all seven controller Play Mode tests pass (`Logs/M3-playmode.xml`).

The two-process standalone test verifies host startup, client connection, both
spawns, independent owner movement, disabled remote input/cameras/listeners,
matching replicated position/yaw/crouch, client disconnect/reconnect, and host
shutdown/restart. Both processes exit successfully with no gameplay exceptions
or critical network errors. A graphics-enabled run also passes; camera renders
were inspected. Capture initially failed with hidden-window screen capture;
the harness now uses a URP render texture. Development labels face the local
camera so they are readable from either approach.

Added NGO 2.13.3 (Package Manager compatible) and Unity Transport 2.7.4
(recommended). Lockfile generated by Unity. Movement is owner-authoritative;
server-approved neutral spawn slots introduce no roles. Remote controllers do
not collide with players. Connection UI and automation are development-only.
Manual review of mouse feel, UI clicks and the full host/client walkthrough in
README.md remains useful; automated rendering captures the camera, not IMGUI.
Unity licensing/graphics diagnostics may appear in logs despite successful
builds; no C# errors or failing tests remain.

M4 has not been started. This task stops at M3.

---

# M4 — ROLES AND PLAYER SPAWNING

## Goal

Introduce Viggo and electrician roles.

## Required Features

Authoritative role enum/state.

Example:

- Electrician
- Viggo

Server assigns roles.

For prototype:

- First/selected player may become Viggo.
- Others become electricians.

Implementation should be easy to replace with random/rotating selection later.

## Role Presentation

Use placeholders.

Example:

- Viggo = large/wide character.
- Electrician = normal capsule.

Role must be visually recognizable during development.

## Spawn Behaviour

- Viggo uses Viggo spawn.
- Electricians use electrician spawn positions.

## Acceptance Criteria

- Exactly one connected player becomes Viggo.
- Remaining players become electricians.
- Role assignment is synchronized.
- Correct spawn location is used.
- Local player can determine their own role.
- Remote players display correct role.
- Client cannot simply assign itself Viggo.

---

## M4 Status — 2026-09-29

Acceptance criteria satisfied. The listen-server host is Viggo; all joining and
rejoining clients are electricians. Roles are server-written NetworkVariables.
Role spawn slots use the existing worksite areas, and orange/wide versus blue
capsules distinguish roles without changing the M2 movement collider or tuning.
The local development panel displays the player's role.

Unity asset validation/compilation and Windows build passed. Four Edit Mode role
tests and all seven M2 Play Mode tests passed. The graphics-enabled two-process
test checked role replication, server-only write permissions, spawn positions,
silhouettes, movement, camera/input ownership, client reconnect and host restart
(`Logs/M4-*.log`, `Logs/Multiplayer-20260929-201556`). Interactive UI/mouse feel
remains a manual review. No critical networking errors were reported.

---

# M5 — WORKDAY CLOCK AND MATCH STATE

## Goal

Create authoritative workday progression.

## Required Features

Create match state.

At minimum:

- Preparing
- Workday
- EndOfDay

Create accelerated clock:

07:00 → 15:00

Expose configurable:

`DayDurationMinutes`

Development default:

Approximately 2–5 minutes.

## Authority

Host/server owns time.

Clients display synchronized time.

## Deliverable

All players observe the same workday time.

## Acceptance Criteria

- Match begins at 07:00.
- Clock advances.
- Clock reaches 15:00.
- Match transitions to EndOfDay.
- Client clocks do not independently decide match completion.
- Day duration can be changed without editing core timer logic.
- Late-joining client receives correct current time/state if late joining is currently supported.

---

## M5 Status — 2026-09-29

Acceptance criteria satisfied. A registered network prefab holds an atomic,
server-written phase/time snapshot. Preparing lasts five seconds, followed by a
configurable two-minute day from 07:00 to 15:00. Clients only display snapshots;
they cannot advance the phase. Disconnect/reconnect receives the ongoing clock.
Restarting the host creates a fresh day. No five-day/results flow was added.

Unity compilation, scene/prefab validation and Windows build passed. All 14
Edit Mode tests (including timer boundaries and configurable duration) and seven
controller Play Mode tests passed. Two standalone processes observed Preparing
07:00, matching ongoing time after reconnect, then EndOfDay 15:00, with the M3/M4
regression checks passing (`Logs/M5-*`, `Logs/Multiplayer-20260929-202024`).

---

# M6 — INTERACTION FRAMEWORK

## Goal

Create one reusable interaction system.

## Required Features

Local player can look at nearby interactable object.

When valid:

Display simple interaction prompt.

Press/hold:

**E**

Interaction code should support future use by:

- Tasks.
- Battery.
- Battery station.
- Viggo mechanics.

## Requirements

Interaction range configurable.

Server validates important network interactions.

## Deliverable

Development test interactable.

Example:

Cube labelled:

**TEST INTERACTION**

Interacting produces visible/logged confirmation.

## Acceptance Criteria

- Only object being aimed at is selected.
- Out-of-range object cannot be interacted with.
- Prompt disappears appropriately.
- Interaction input comes only from local player.
- Network-important interactions have a path for server validation.

---

## M6 Status — 2026-09-29

Acceptance criteria satisfied. `PlayerInteraction` selects from the owner's
camera, shows/hides the E prompt, and sends an owner-only RPC. Server validation
uses the server-observed player position, configured range, line of sight and
aimed target. `NetworkInteractable` is the common extension point. A development
cube near the electrician spawn displays a replicated confirmation count.

Unity compilation, asset validation and Windows build passed. All 14 Edit Mode
and 10 Play Mode tests passed, including E input, aim/range/wall rejection.
The two-process run rejected a forged distant client request, exercised the
local prompt and valid interaction, confirmed the count on both instances, and
passed M3–M5 regressions (`Logs/M6-*`, `Logs/Multiplayer-20260929-202612`).
Movement remains owner-authoritative as in M3; this is not an anti-teleport system.

---

# M7 — FIRST ELECTRICAL TASK

## Goal

Implement first productive electrician activity.

## Task

**Simple Circuit Task**

Prototype design:

Three switches/connections/buttons must be activated in correct order.

Example:

1 → 3 → 2

Sequence may be configurable.

## Flow

Electrician:

1. Approaches task.
2. Interacts.
3. Task UI appears.
4. Completes sequence.
5. Task becomes complete.
6. UI closes.
7. Completion synchronizes.

Viggo should not complete electrician tasks.

## Authority

Server owns:

- Task state.
- Completion.

Client handles:

- Local UI input.

## Deliverable

At least one functioning networked work task.

## Acceptance Criteria

- Electrician can begin task.
- Viggo cannot legitimately complete task.
- Incorrect action behaves consistently.
- Correct sequence completes task.
- Task completion is visible to all clients.
- Same task cannot repeatedly reward players unless explicitly reset.
- Task architecture allows additional instances.

---

## M7 Status — 2026-09-29

Acceptance criteria satisfied. Task Location 2 hosts the Simple Circuit Task
(default 1 > 3 > 2, editable on its prefab). Electricians open its UI through
the M6 interaction path. The server owns worker, progress, input revision and
completion. Wrong input resets progress with feedback. Replayed revisions and
repeated completion are rejected. Cancel, distance loss, disconnect and the end
of the workday release an unfinished task. Completion persists for the session.
The task UI releases/restores local controls; shared labels show completion.

Unity compilation, full asset validation and Windows build passed. All 20 Edit
Mode and 11 Play Mode tests passed (`Logs/M7-fixed-*`). The graphics-enabled
two-process run verified Viggo rejection, electrician UI opening/closing, wrong
input, duplicate requests, matching completion/worker, and all M3–M6 checks
through EndOfDay (`Logs/Multiplayer-20260929-220446`).

Initial task integration testing caught downward rays hitting the local
CharacterController. The common query now ignores only the requesting player's
collider, uses a reusable hit buffer and still rejects occlusion; a regression
test covers this. Earlier failed diagnostic runs are retained in ignored Logs.
No scoring exists at this checkpoint; M8 adds rewards to the one-shot completion.

---

# M8 — SCORING

## Goal

Add authoritative scoring.

## Required Features

Each player has synchronized score.

Electrician receives points for completing task.

Create central configurable values.

Do not hard-code reward inside task implementation if avoidable.

## Development UI

Display local player's score.

Optional temporary scoreboard.

## Acceptance Criteria

- Task completion awards correct player.
- Score synchronized.
- Client cannot directly set authoritative score.
- Score does not duplicate because of repeated network messages.
- Reward value is configurable.
- Viggo can later receive score through same scoring architecture.

---

## M8 Status — 2026-09-29

Acceptance criteria satisfied. Each network player has a server-written score.
`ScoreService` listens to server task completion, validates the actual electrician
and Workday phase, reads `Scoring.asset` (default 100), and records the source
event once. Clients have no score-setting RPC. The underlying award path is
role-independent for later server-validated events; no later mechanics were added.
The development HUD shows local role/score/time and a small shared scoreboard.

Unity compilation, full M1–M8 scene/prefab validation and the Windows development
build passed. All 26 Edit Mode and 11 Play Mode tests passed (`Logs/M8-*`),
including the original seven controller tests and the downward-ray regression.
The graphics-enabled host/client run passed movement, role/spawn/ownership,
reconnect, timer, distant-request rejection, task UI, Viggo rejection, wrong
sequence, duplicate input/completion, exact score recipient, duplicate-award
rejection, EndOfDay scoring rejection and host restart. A third client joined
after completion and received the existing roles, completed task, 100/0 scores,
current time and EndOfDay (`Logs/Multiplayer-20260929-221317`). Host/client task
camera renders were inspected; both show the green completed panel and worker 2.

No C# compiler warnings/errors or critical runtime/network errors occurred in
the successful checkpoint runs. Unity licensing/environment diagnostics can
remain in editor logs. Human mouse feel and IMGUI clicks still merit the exact
README walkthrough; automated camera captures exclude IMGUI. Scores belong to
the current network player, so disconnect/rejoin creates a new ID with score 0;
completed world tasks persist until host restart. No session identity persistence
or results screen was added. Unrelated Unity build serialization changes were
discarded; Unity version, packages and render-pipeline assignments are unchanged.

**Stopped after M8. M9 has not been started.**

---

# M9 — VIGGO MOVEMENT AND STAMINA

## Goal

Give Viggo his defining mobility limitation.

## Required Features

Only Viggo uses Viggo stamina system.

Parameters:

- Maximum stamina.
- Drain rate.
- Recovery rate.
- Movement threshold/conditions.
- Viggo speed.

Stamina should decrease during meaningful movement/chasing.

Electricians do not use this stamina system.

## Development UI

Viggo sees stamina bar/value.

## Acceptance Criteria

- Viggo stamina decreases.
- Electrician stamina unaffected.
- Stamina values are configurable.
- Stamina state is appropriate for authoritative gameplay.
- Other clients see movement behaviour consistently.

---

## M9 Status — 2026-09-29

Acceptance criteria satisfied. `ViggoStamina` composes the existing controller;
`Viggo.asset` centralizes speeds, capacity, drain/recovery rates and movement
conditions. Server-observed horizontal movement drives a server-written stamina
value, and only Viggo sees its development HUD. Electrician tuning is unchanged.
No rest or Rage mechanics were added at this checkpoint.

Unity compilation, M1–M9 asset validation and Windows development build passed.
All 32 Edit Mode and 11 Play Mode tests passed (`Logs/M9-*`). The graphics-enabled
host/client run plus late observer passed stamina authority, drain, recovery,
electrician exclusion, synchronized movement and all existing M3–M8 scenarios,
including scoring, EndOfDay and restart (`Logs/Multiplayer-20260929-223152`).
The harness's day and EndOfDay wait were extended to fit the new scenario.
No compiler errors/warnings or critical runtime errors occurred. Movement remains
owner-authoritative as in M3; no anti-teleport system was introduced. Human input
feel/IMGUI review remains the README manual check. Unity-generated unrelated
build serialization changes were discarded; packages/editor/pipeline unchanged.

---
# M10 — VIGGO REST

## Goal

Implement forced exhaustion break.

## Trigger

Viggo stamina reaches zero.

## Behaviour

Viggo enters:

**Resting**

During Resting:

- Movement disabled or heavily restricted.
- Catching disabled.
- Stamina recovers.
- Placeholder "Javell" feedback triggers once.

Placeholder feedback may be:

- UI text.
- Debug text.
- Temporary audio.

## Acceptance Criteria

- Zero stamina reliably triggers rest.
- Viggo cannot simply bypass rest through input.
- Rest ends correctly.
- Stamina restored according to configuration.
- Rest behaviour synchronizes.
- "Javell" event fires once per rest, not every frame.

---

## M10 Status — 2026-09-29

Acceptance criteria satisfied. Server-owned atomic stamina/rest state enters
Resting on exhaustion. Default rest disables horizontal movement and jumping,
gates capture eligibility, restores stamina over four seconds and then releases
control. Duration, recovery, final refill and restricted speed are configurable.
A monotonically increasing rest counter drives a separate presentation event:
exactly one Javell message per rest, with no replay on late join.

Compilation, M1–M10 asset validation and Windows build passed. All 34 Edit Mode
and 11 Play Mode cases passed (`Logs/M10-*`). Graphics-enabled host/client plus
late observer passed exhaustion, continued movement/sprint/jump rejection,
recovery, capture gating, one Javell event per instance and state replication,
plus all M3–M9 regression scenarios (`Logs/Multiplayer-20260929-223814`).
No compiler warnings/errors or critical gameplay/network errors occurred.
Human input feel and IMGUI rendering remain manual README checks. No final audio,
capture mechanic, Rage or battery feature was added at this checkpoint.

---
# M11 — BATTERY THEFT

## Goal

Implement Escape Viggo's central risk mechanic.

## Required Objects

- Battery station.
- Battery.
- Battery interaction.

## Flow

Electrician:

1. Approaches battery.
2. Holds Interact.
3. Theft progress runs.
4. Theft completes.
5. Battery becomes carried by electrician.

Viggo cannot steal his own battery.

## Important

Viggo must NOT automatically enter Rage Mode yet.

Theft remains undiscovered.

## Required State

Track:

- Battery state.
- Current carrier.
- Original thief.
- Whether theft has been discovered.

## Acceptance Criteria

- Electrician can steal battery.
- Theft takes configurable time.
- Leaving/cancelling behaves correctly.
- Exactly one authoritative battery exists.
- All clients agree who carries battery.
- Original thief is recorded.
- Viggo receives no automatic exact thief information.
- Rage does NOT begin merely because theft succeeded.

---

## M11 Status — 2026-09-29

Acceptance criteria satisfied. One registered BatteryStation prefab replaces the
existing station marker at runtime. Shared interaction now supports hold pulses
and cancellation. Server-owned state records progress, actor, carrier, original
thief and discovery separately. Default theft is three seconds. Release, lost
valid targeting/range, heartbeat timeout, EndOfDay and disconnect cancel attempts.
Only electricians can steal; successful theft stays undiscovered, with no Viggo
identity alert or Rage. Carrier feedback is local to its electrician. A disconnected
carrier returns the same battery to the station; no new battery is instantiated.

Compilation, saved asset/reference validation and Windows build passed. All 36
Edit Mode and 11 Play Mode tests passed (`Logs/M11-*`). Graphics-enabled host and
two electricians passed theft prompt, duration, release/timeout/range cancellation,
Viggo rejection, competing requests, unique carrier/original thief, undiscovered
replication and all M3–M10 regressions (`Logs/Multiplayer-20260929-224644`).
The first asset check exposed a validator error: a runtime NetworkObject accessor
was used on an unspawned prefab. Replacing it with GetComponent fixed validation;
the saved scene and registry each already contained exactly one battery.
No compiler errors/warnings or critical runtime/network errors occurred. Human
hold-input/IMGUI feel remains the README manual check. No discovery/Rage yet.

---
# M12 — BATTERY DISCOVERY AND RAGE

## Goal

Create the key gameplay phase change.

## Discovery

Viggo returns to battery station.

Viggo interacts/checks.

If battery is missing:

Activate Rage Mode.

## Rage Behaviour

During Rage:

- Viggo normal stamina limitation suspended.
- Forced rest disabled.
- Viggo movement becomes faster.
- Global Rage state synchronized.
- Electricians receive simple warning.
- Viggo receives clear feedback.

Prototype presentation:

- UI banner.
- Screen message.
- Optional placeholder sound.

## Acceptance Criteria

- Theft alone does not trigger Rage.
- Viggo inspection triggers discovery.
- Discovery only happens once per theft event.
- Rage begins for all clients.
- Viggo no longer enters normal stamina-rest loop.
- Viggo movement reflects Rage tuning.
- Electricians receive Rage notification.
- Host/server owns Rage state.

---

## M12 Status — 2026-09-29

Acceptance criteria satisfied. Viggo's station inspection uses the shared
range/aim-validated interaction. Only a missing, undiscovered battery triggers
one discovery and one server-owned Rage sequence. Original thief and carrier
remain separate. ViggoRage lives with the workday; RagePresentation shows role-
appropriate generic banners without thief identity. Rage speed is configurable,
normal stamina is suspended and existing/new forced rest is suppressed. EndOfDay
ends Rage; recovery/capture end conditions are configurable for the prototype.

Compilation, M1–M12 asset validation and Windows build passed. All 38 Edit Mode
and 11 Play Mode tests passed (`Logs/M12-*`). Host/client plus the competing
observer passed present-battery inspection, undiscovered theft, electrician
rejection, repeated inspection, one Rage event, synchronized discovery/Rage,
six seconds of faster pursuit without stamina loss/rest and Rage shutdown at
15:00 (`Logs/Multiplayer-20260929-225401`), together with M3–M11 regressions.
No compiler warnings/errors or critical runtime/network failures occurred.
Human banner presentation/input feel remains the README walkthrough. No final
art/audio or capture implementation was added at this checkpoint.

---
# M13 — CAPTURE SYSTEM

## Goal

Allow Viggo to catch electricians.

## Flow

1. Viggo approaches electrician.
2. Viggo uses capture interaction.
3. Server validates:
   - Roles.
   - Distance.
   - Match state.
4. Electrician is captured.

## Prototype Consequence

Captured electrician:

- Loses configurable points.
- Drops battery if carrying it.
- Is disabled for several seconds.
- Respawns at electrician spawn.

Viggo:

- Receives configurable points.

Additional reward if captured electrician was battery carrier.

## Acceptance Criteria

- Viggo can capture valid nearby electrician.
- Electrician cannot capture another electrician.
- Client cannot fake distant capture.
- Score updates correctly.
- Carrier drops/loses battery correctly.
- Captured player returns to game.
- No permanent elimination.
- Capture state synchronizes.

---

## M13 Status — 2026-09-29

Acceptance criteria satisfied. PlayerCapture uses shared aimed interaction with
a trigger selection collider; physics movement remains unchanged. The server
validates roles, Workday, rest/capture state, configured distance and line of
sight. It locks capture before awarding points, blocks movement/jump/interactions,
then orders the owner to teleport to its assigned spawn after the timeout.
Respawn protection prevents immediate recapture. ScoreService deduplicates
capture reward, carrier bonus and penalty; captured players return to gameplay.

Prototype battery decision: capturing its carrier removes ownership and secures
the same battery at the station until the day ends, preserving original thief
and discovery history. Rage ends according to its recovery/capture configuration.
This implements the allowed loses/recovered outcome without dropped pickups or
passing. Defaults: range 2 m, timeout 3 s, protection 2 s, reward 50, carrier bonus
50 and penalty 25. Scores may go negative.

Compilation, M1–M13 asset validation and Windows build passed. All 46 Edit Mode
and 12 Play Mode cases passed (`Logs/M13-*`). Host/client plus observer passed
range (including 2.5 m inside interaction range), role and duplicate rejection,
100/75 synchronized scores after task/carrier capture, battery recovery/Rage end,
forced timeout, owner respawn and resumed movement, plus M3–M12 regressions
(`Logs/Multiplayer-20260929-230754`). The first test attempted capture at exactly
2 m and failed its prompt check; the successful case now uses 1.5 m, while exact
boundary rules remain unit-tested. The harness now reports host loss explicitly
instead of dereferencing the despawned clock after an aborted test. No gameplay
range was relaxed. Earlier diagnostic logs remain ignored.

No compiler warnings/errors or critical errors occurred in the passing run.
Movement remains owner-authoritative with server-observed range checks, as in
M3; this is not an anti-teleport system. Manual UI/input review remains in README.

---
# M14 — END-OF-DAY FLOW

## Goal

Complete one full workday.

## At 15:00

- Active gameplay stops.
- Task interactions stop.
- Rage ends.
- Capture stops.
- Daily results are calculated.
- End-of-day screen opens.

## Screen

Display at minimum:

- Player.
- Role.
- Score.
- Tasks completed.
- Captures.
- Battery outcome.

## Acceptance Criteria

- Every client transitions at same authoritative time.
- Players cannot continue scoring after end.
- Results agree between host/client.
- End-of-day screen opens.
- One complete prototype workday can be played from beginning to end.

---

## M14 Status — 2026-09-29

Acceptance criteria satisfied. The authoritative EndOfDay transition now cancels
unfinished tasks/theft, ends Rage, clears captures, blocks gameplay input and
rejects further scoring. `DailyResults` finalizes once and publishes server-owned
rows (player ID, role, score, completed tasks, captures) plus battery outcome.
The results panel opens on every client. Rows describe players connected at the
cutoff and remain frozen across later disconnects/joins; there is no persistent
identity or five-day week. Restarting the host starts a fresh day.

Compilation, M1–M14 scene/prefab/configuration validation and Windows development
build passed. All 46 Edit Mode and 12 Play Mode cases passed (`Logs/M14-*`).
The graphics-enabled host/client plus second electrician run passed the entire
loop: ownership/roles/movement, work and score, stamina/rest/Javell, theft and
contention, delayed discovery/Rage/continuous pursuit, capture/reward/penalty,
battery recovery/respawn, authoritative 15:00, matching results and host restart
(`Logs/Multiplayer-20260929-232125`). Negative role/range/replay requests and
post-cutoff interaction/scoring requests were rejected. The observer rejoined
after EndOfDay and received the same frozen results.

A second complete run left a real task unfinished and Rage active until 15:00;
both stopped and all clients still agreed on results
(`Logs/Multiplayer-20260929-232358`, `-EndDayStress`). Its runtime-only fixture
tuning is isolated behind explicit development smoke arguments. Final results
were Viggo 100 points / 1 capture, worker 75 points / 1 completed task, second
worker 0 points; battery Secured. Repeated finalization did not add rows.

No C# compiler warnings/errors or gameplay/network exceptions occurred in the
successful runs. Baseline Unity licensing/shutdown allocation diagnostics remain
in logs. Human mouse feel and IMGUI clicks/rendering remain manual README checks;
automated checks exercise UI state and camera captures exclude IMGUI. Movement
retains the M3 owner-authoritative model with server-observed interaction checks.
Unrelated Unity build serialization changes were discarded; editor version,
packages and render-pipeline assignments are unchanged.

M9–M14 are complete. M15 was not started.

---

# M15 — PROTOTYPE UI AND FEEDBACK

## Goal

Make the prototype understandable without developer knowledge.

## Required HUD

All players:

- Workday time.
- Interaction prompt.
- Score.

Electricians:

- Relevant task UI.
- Battery carried indicator.
- Rage warning.

Viggo:

- Stamina.
- Rest state.
- Rage state.

## Feedback

Create basic feedback for:

- Task completion.
- Battery stolen.
- Battery discovered.
- Rage activated.
- Capture.
- Workday ending.

No final art required.

## Acceptance Criteria

A new tester can understand the basic state of the game without watching the Unity Console.

---

## M15 Status — 2026-09-30

Implemented and engineering validation passed; checkpoint commit `5f86d2a`.
The local-owner HUD presents role, ID, score, phase/time, objectives, interaction,
stamina/rest/Javell, private carrier status, theft progress/cancellation, Rage,
capture timeout, task completion and score changes. Diagnostics are optional.
Task UI and scrollable frozen results use a shared 1280x720 layout. Existing rest
events and server authority remain intact; results still use M14 frozen data.

Unity compilation, M1–M15 saved asset/reference validation and Windows build
passed (`Logs/M15-*`). All 46 Edit Mode and 12 Play Mode tests passed. The
three-process full-loop run `Logs/Multiplayer-20260930-121318` passed ownership,
private carrier feedback, cancellation, Rage, existing gameplay regressions,
reconnect, frozen results and host restart. Camera renders were inspected.
Hidden-window framebuffer screenshots were black and do not establish IMGUI
visual correctness. New-tester comprehension, readability and real mouse/button
feel remain human acceptance checks in README, not claimed by automation.

---

# M16 — LOCAL MULTIPLAYER PLAYTEST BUILD

## Goal

Create the first true playable version of Escape Viggo.

This is the most important early milestone.

## Required Complete Loop

Players can:

1. Join.
2. Spawn.
3. Receive roles.
4. Work.
5. Score.
6. Distract/interact.
7. Steal battery.
8. Hide.
9. Trigger Viggo Rage.
10. Get chased.
11. Get caught.
12. Reach 15:00.
13. View results.

## Testing

Test at minimum:

- 1 host + 1 client.
- Preferably 1 Viggo + 2 or more electricians.

Run several complete short workdays.

## Bug Priority

Fix:

- Disconnects.
- Desynchronization.
- Duplicate scores.
- Broken battery ownership.
- Broken task state.
- Incorrect role authority.
- Timer disagreement.
- Softlocks.

Do not prioritize visual polish over these issues.

## Acceptance Criteria

The game is genuinely playable locally from start to finish.

At this point:

**The core prototype of Escape Viggo exists.**

Do NOT immediately expand scope.

Playtest it first.

---

## M16 Status — 2026-09-30

Implemented and engineering validation passed; checkpoint commit `b8e3d0e`.
The connection menu supports Host/Client, IP/port, state/count, resume,
sensitivity, disconnect and quit. Hosts select two-minute, five-minute (default)
or asset-configured days. `-ev-day-minutes` is a runtime-only development override.
`Tools/Start-LocalPlaytest.ps1` launches 2–8 normal interactive game windows.
README includes **How to Playtest Escape Viggo**. No new gameplay mechanics.

Connection attempts have a bounded timeout and retry guidance. A review found
disconnect callbacks could overwrite failure guidance; final code preserves it
through cleanup. The ordinary launcher never enables automated test fixtures.

Validation:

- Compilation, M1–M16 saved scene/prefab/reference validation and Windows
  development build passed (`Logs/M16-final-build.log`).
- All 53 Edit Mode and 13 Play Mode cases passed (`Logs/M16-*.xml`), including
  invalid/locale-dependent duration input and modal resume/release regression.
- Three processes completed the full workday, late join, battery contention,
  capture, matching frozen results, reconnect and host restart
  (`Logs/Multiplayer-20260930-122230-17777`).
- The final build completed the two-process full loop with unfinished task and
  active Rage at 15:00; both stopped, results agreed and restart passed
  (`Logs/Multiplayer-20260930-122535-19777`, EndDayStress).
- A separate final two-process run passed unreachable-host timeout, retained
  retry guidance, retry connection, selected duration/results, preset change on
  restart and cursor cleanup (`Logs/Multiplayer-20260930-122535-18777`).
- Launcher PowerShell syntax passed. Camera renders were inspected. Hidden UI
  captures are black and explicitly reported unavailable. Interactive launcher
  window arrangement, IMGUI clicks/readability at 720p/1080p, input feel and
  new-tester comprehension still require human judgement.

No C# compiler warnings/errors or critical gameplay/network errors were found.
Baseline Unity licensing/shutdown diagnostics remain. Automated checks establish
local start-to-finish gameplay; human usability acceptance remains pending.
The pre-existing five-minute Workday.asset edit remains outside these commits.
Unrelated Unity-written build settings were restored. No editor, package,
pipeline or scene architecture changes. Stopped after M16; no M17 work.

---

# M17 — ONLINE SESSION AND RELAY

## Goal

Allow players on separate networks to join easily.

## Technology

Use Unity Multiplayer Services with Netcode for GameObjects.

Use the current Unity-supported session/Relay flow.

## Required Flow

Host:

1. Selects Host/Create Game.
2. Creates session.
3. Receives join code or equivalent private-session identifier.

Client:

1. Opens Join.
2. Enters code.
3. Connects.

## Not Required

- Public matchmaking.
- Server browser.
- Ranked matchmaking.
- Dedicated server.

## Acceptance Criteria

- Two PCs on separate connections can join.
- Role assignment works.
- Core gameplay remains synchronized.
- Session failures show useful feedback.
- Leaving returns cleanly to menu.

---

## M17 Status — 2026-09-30

Implemented and engineering validation passed; separate-network acceptance
remains pending. The existing Cloud
Project works: no manual organization/project linking or Dashboard setup was
required. Unity enabled its project-local Services setting on package import.
Added Package Manager-recommended Multiplayer Services 2.3.3; Authentication
3.7.4 and Core 1.18.0 resolved transitively. NGO 2.13.3, Transport 2.7.4, Unity
6000.3.25f1 and URP remain unchanged. No standalone Lobby/Relay SDKs were added.

`OnlineServices` isolates initialization/anonymous authentication and retry.
`OnlineNetworkSession` uses private Sessions, Session join codes, DTLS and the
SDK's built-in NGO handler. Capacity follows the eight saved spawn slots.
The menu supports host, trimmed/case-normalized code join, copy code, connection
feedback and development-only direct IP fallback. Host leave deletes the session;
host loss returns clients to the menu without migration. Gameplay authority,
spawn callbacks and the M0–M16 loop are retained. SDK requests are serialized
through cleanup, including late responses after a timeout/cancel.

Validation evidence (ignored local logs):

- M1–M17 saved scene/prefab validation and Windows development build passed
  (`Logs/M17-retry-build.log`), without C# compiler errors/warnings.
- 71 Edit Mode tests passed (`Logs/M17-final-editmode.xml`), including readiness,
  anonymous authentication, retry, code validation, duplicate request protection,
  state transitions, timeout feedback and wrapped session-full/not-found errors.
- All 13 Play Mode tests passed (`Logs/M17-final-playmode.xml`).
- Real Sessions/DTLS Relay full loop passed with two processes on one PC
  (`Logs/Online-20260930-173014`): roles, movement, reconnect, work, scores,
  theft, discovery/Rage, capture, EndOfDay, matching results and host restart.
- Real abrupt host-process loss, menu/cursor recovery, online rehosting and direct
  fallback passed (`Logs/Online-20260930-172810`). Graceful host leave and the same
  recovery path passed (`Logs/Online-20260930-221555`). The latter includes a
  nonfatal cleanup warning when the host deletes the session during client leave.
- Local unavailable-host timeout, retained retry guidance, retry, selected day
  duration, results, preset restart and cursor cleanup passed
  (`Logs/Multiplayer-20260930-221245-20777`). The expanded headless test also
  passed three consecutive immediate disconnect/reconnect cycles after adding
  a 0.25-second transport reuse delay (`Logs/Multiplayer-20260930-222050-22777`).
- Final three-process local full workday passed, including battery contention,
  observer reconnect to frozen results and host restart
  (`Logs/Multiplayer-20260930-222054-23777`).
- Online launcher PowerShell syntax passed. README includes exact host/join,
  optional project-link repair, separate-network gameplay and host-loss checks.

An initial Relay smoke run exposed movement-baseline sampling that assumed local
connection timing. A test-only shared-file barrier now aligns baseline sampling;
gameplay assertions still use real Relay traffic. The first compile also exposed
a missing Services environments assembly reference, which was corrected.
Local observer tests exposed a transport reuse race on immediate reconnect.
A one-frame wait was insufficient; a 0.25-second restart gate passed both rapid
headless reconnects and the complete three-player results/reconnect regression.
No gameplay timing or authority rules were changed to satisfy these tests.

Real Relay was tested, not mocked. Two PCs on separate connections and human
UI/readability/input review are still unverified, so M17 acceptance is **not yet
fully satisfied**. No credentials were added, no cloud project was created, and
the existing Workday.asset user edit stays outside the commit. No M18 work.

---

# M18 — FIVE-DAY WORK WEEK

## Goal

Expand one workday into the complete match structure.

## Required Flow

Monday  
→ Results  
→ Tuesday  
→ Results  
→ Wednesday  
→ Results  
→ Thursday  
→ Results  
→ Friday  
→ Final Results

## Requirements

- Scores accumulate.
- Day resets relevant gameplay state.
- Battery resets.
- Tasks reset.
- Match timer resets.
- Roles may initially remain same unless changed by design decision.

## Acceptance Criteria

Five short debug days can run consecutively without reloading project manually.

---

# M19 — MULTIPLE WORK TASKS

## Goal

Make electrician gameplay varied.

Add additional task types one at a time.

Suggested order:

1. Voltage measurement.
2. Outlet replacement.
3. Circuit breaker installation.
4. Cable routing.

Each new task must use the common task/scoring architecture.

Do not copy entire task systems for each minigame.

## Acceptance Criteria

At least three distinct task types function in multiplayer.

---

# M20 — VIGGO DISTRACTION EXPANSION

## Goal

Give Viggo interesting things to do before battery theft.

Potential mechanics:

- Interrupt task.
- Move a required tool.
- Turn power off.
- Call electrician away.
- Create pointless work order.

Choose mechanics based on playtesting.

Do not implement all ideas automatically.

## Success Criterion

Playing Viggo before Rage Mode is entertaining rather than merely waiting for battery theft.

---

# M21 — AUDIO AND GAME FEEL

## Goal

Transform functional mechanics into a comedic game experience.

Add:

- Footsteps.
- Worksite ambience.
- Tool sounds.
- Battery sounds.
- Task completion.
- Viggo exhaustion.
- Rage transition.
- Capture feedback.
- End-of-day sound.

Add proper Viggo voice hooks.

Signature:

**"Javell."**

Final recorded assets should only be integrated when supplied/approved.

---

# M22 — FIRST ART PASS

## Goal

Replace the most important prototype visuals.

Priority:

1. Viggo character.
2. Electrician character.
3. Battery/drill.
4. Task props.
5. Worksite environment.
6. UI.

Maintain clear silhouettes.

Do not sacrifice gameplay readability for realism.

---

# M23 — BALANCE AND PLAYTESTING

## Goal

Tune Escape Viggo based on actual multiplayer sessions.

Measure:

- Tasks completed per day.
- Viggo capture count.
- Battery theft frequency.
- Theft success.
- Time until discovery.
- Rage duration.
- Average scores.
- Time players spend idle.
- Whether playing Viggo is enjoyable.
- Whether electricians have meaningful choices.

Tune:

- Day length.
- Task rewards.
- Viggo movement.
- Stamina.
- Rest duration.
- Rage speed.
- Theft duration.
- Capture range.
- Score rewards.

Do not balance solely from theory.

---

# POST-PROTOTYPE BACKLOG

Only consider after the core game proves fun.

Potential additions:

- Battery passing.
- Multiple batteries.
- Proximity voice chat.
- More maps.
- Hurtigbåtterminal.
- Construction site.
- Industrial building.
- Random daily events.
- Lunch break.
- Coffee system.
- Overtime.
- Tools.
- Tool theft.
- Work quota.
- Apprentice.
- Safety inspector.
- Security cameras.
- Footprints.
- Social accusation mechanics.
- Character customization.
- Achievements.
- Steam integration.
- Public matchmaking.
- Statistics.
- Progression.
- Additional Viggo abilities.

Items in this backlog are NOT approved implementation work merely because they are listed here.

---

# CODEX MILESTONE WORKFLOW

For each milestone:

## Step 1 — Inspect

Read:

- `AGENTS.md`
- `GAME_SPEC.md`
- This milestone

Inspect relevant current code.

## Step 2 — Plan

Determine the minimum architecture necessary.

Do not implement future milestone features.

## Step 3 — Implement

Make small coherent changes.

## Step 4 — Validate

- Compile.
- Run relevant tests.
- Test required scene/prefab integration.
- Test networking when applicable.

## Step 5 — Report

State:

- What changed.
- What was tested.
- What remains unverified.
- Whether all acceptance criteria are met.

## Step 6 — Stop

Do NOT automatically begin the next milestone.

Wait for the next user instruction.

---

# CURRENT TARGET

M0–M16 are the stable baseline for the user's M17 request. M17 implements private
online Sessions/Relay while preserving the direct local development path.
Two-PC separate-network acceptance and human UI/input review remain pending.
Stop after M17. M18 is not authorized.
