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

M0–M9 are complete. The user authorized M9–M14 sequentially with a healthy local
commit per checkpoint. M10 — Viggo Rest is next. Stop after M14; do not begin M15.
