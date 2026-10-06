# ESCAPE VIGGO — CURRENT IMPLEMENTATION STATE

**Purpose:** fast orientation for coding agents. This file describes the current repository, not its full history.

**Last consolidated checkpoint:** C5 Viggo base model engineering completed locally, 2026-10-06.

## 1. Current Development Position

- M0–M19: engineering/acceptance criteria completed.
- M20: engineering implementation completed; human fun/balance judgement remains a playtest concern.
- M21: engineering implementation and Tier 3 checkpoint completed. Human listening/feel acceptance remains.
- M22 is partially authorized/implemented through **C5 Viggo base model only**.
- C0 Blender-to-Unity pipeline (`fd24a02`) is verified. C1–C4 electrician production
  passed its engineering checkpoint. C5 model/rig/presentation is implemented;
  targeted tests and the final local host/client check pass. Human visual review remains.
- Stop before C6 Viggo locomotion, other M22 production and M23 unless explicitly requested.

Recent local milestone commits:

- M18 — Five-Day Week: `bbb601d`
- M19 — Multiple Work Tasks: `2e54e57`
- M20 — Viggo Distractions: `885de29`

Nothing is assumed pushed unless Git says otherwise.

## 2. Proven Game Loop

The current playable game supports:

- local host/client multiplayer;
- private online Sessions/Relay multiplayer;
- one Viggo and multiple electricians;
- first-person movement, sprint, jump, crouch, and interaction;
- accelerated workdays;
- a five-day Monday–Friday match flow;
- cumulative week scoring/results;
- multiple electrician work tasks;
- Viggo stamina and forced `Javell` rest;
- battery theft, delayed discovery, and Rage;
- Viggo capture and electrician recovery;
- Viggo task interruption;
- Viggo power-cut distraction and electrician/automatic power restoration;
- synchronized day/week results.

The prototype remains visually greybox-oriented. M21 supplies neutral development sounds and optional visual edge feedback. Final art/voice recordings are not integrated.

## 3. Task System

Implemented task types include:

1. Simple Circuit.
2. Voltage Measurement.
3. Outlet Replacement.
4. Breaker Installation.

They use the common task/scoring architecture rather than independent duplicate systems.

Before adding or changing a task, inspect the current `WorkTask` hierarchy/framework, shared task UI/presentation patterns, scoring path, and current M19 implementations.
Do not assume the old Simple Circuit implementation is still the complete task architecture.

## 4. Viggo Systems

Current Viggo gameplay includes:

- distinct movement tuning;
- server-authoritative stamina;
- forced rest on exhaustion;
- `Javell` presentation event;
- battery discovery and Rage;
- capture;
- task-progress interruption;
- temporary power cut with cooldown/state validation.

The exact current class/prefab/configuration names for M20 systems must be taken from the repository, because M20 introduced new distraction and power-control assets after the older README architecture summary.

## 5. Authority Model

Server/host remains authoritative over important shared gameplay state, including:

- roles;
- match/week/day progression;
- authoritative time;
- task progress/completion;
- scores;
- battery state/carrier/discovery;
- stamina/rest and Rage;
- captures;
- Viggo distraction outcomes;
- power state;
- results.

Movement retains the established owner-authoritative presentation/movement model unless the repository currently says otherwise.
Important interactions use server-side validation.

Do not move authoritative outcomes into UI, audio, animation, or other presentation systems.

## 6. Known Core Architecture

Stable systems established before M18 include:

- `PlayerInputReader`, `PlayerMovement`, `PlayerLook`
- `PlayerInteraction`
- `NetworkPlayer`
- `PlayerScore`
- `LocalNetworkSession`
- `OnlineNetworkSession`
- `OnlineServices`
- `NetworkWorldSpawner`
- `NetworkWorkday`
- `ScoreService`
- `DailyResults`
- `WorkTask`
- `ViggoStamina`
- `ViggoRage`
- `PlayerCapture`
- `BatteryStation`
- gameplay presentation components for battery/rest/Rage/results

M18–M20 expanded the week system, task framework, distractions, power control, tuning, and validation. Inspect those current implementations before creating new systems.

## 7. Networking and Online Play

Networking stack:

- Netcode for GameObjects
- Unity Transport
- Unity Multiplayer Services
- Unity Authentication
- private Multiplayer Sessions / Relay

Current online flow supports private session creation and join code entry.
Local IP/port development networking remains supported separately.

Known local multi-instance constraint:

- Multiple **online** processes on the same Windows account should use distinct `-ev-online-profile` values so Unity Authentication does not reuse the same cached player identity.
- Separate physical PCs do not require this argument.
- Multiple local/IP clients can run without the online-profile requirement.

Do not treat that same-PC authentication-profile issue as a gameplay player-capacity limitation.

## 8. Current Validation Baseline

M21 Tier 3 checkpoint passed:

- Unity compilation;
- asset/reference validation;
- Windows development build;
- **96 Edit Mode tests**;
- **15 Play Mode tests**;
- rendered baseline multiplayer regression, late observer, reconnect and host restart;
- complete three-player five-day weeks using direct networking;
- complete three-player five-day weeks using actual Unity Sessions/Relay;
- Friday reconnect coverage;
- exact audio/voice cue counts on both baseline peers and all three local/Relay week peers (no duplicate checkpoint cues or Friday result replay).

Relay validation above used three processes on one PC.
Human separate-PC Internet testing and human feel/readability/fun assessment are distinct acceptance activities where relevant.

Use targeted tests during intermediate work. Reserve the complete regression/build/Relay suite for milestone checkpoints or changes that can plausibly affect central networking/authority.

Targeted presentation tests also passed (5 Edit Mode + 2 Play Mode).
Validation uses an isolated copy under `Builds/M21Validation` to preserve the open
Unity editor session; source assets/scripts/metas matched byte-for-byte. Unity-generated
M21 assets/metas were copied back to source, and the validated development player
is available at `Builds/Windows/EscapeViggo.exe`. Detailed log locations are in the
M21 history checkpoint. Rendered Rage-edge presentation was inspected; listening,
rest/capture readability, final voice timing and mix quality require human review.

## 9. Known Human Acceptance State

The user has manually verified that the game generally functions correctly through M20.

Still difficult to judge without real multiplayer participants and non-greybox presentation:

- whether Viggo distractions are genuinely fun;
- overall comedic timing/game feel;
- task pacing with multiple human electricians;
- balance of scores, stamina, Rage, capture, distraction cooldowns, and power outages.

Do not “solve” M23 balance theoretically before multiplayer playtest evidence exists.

## 10. Art and Audio State

- Character and prop/environment art specifications exist as separate canonical project documents.
- They were intentionally left untouched during M18–M21 engineering work.
- Greybox/placeholder presentation remains valid until the relevant art/audio milestone.
- M21 supplies presentation hooks without requiring final approved recordings.
- M22 should consult the dedicated character/prop art specifications before replacing prototype visuals.

### C0 art pipeline (verified 2026-10-01)

- Blender **5.2.2 LTS** and Unity **6000.3.25f1** verified. `Tools/Invoke-Blender.ps1`
  discovers Blender via explicit path, `BLENDER_EXE`, PATH or the standard 5.2 install;
  runs background Python with factory settings, argument forwarding and exit propagation.
- `ArtSource/` separates editable sources from Unity exports, with reserved character,
  prop and environment directories. `Tools/Blender/` contains reusable static-mesh
  helpers and the rerunnable C0 generator; invocation is in its README.
- Only a test mesh was produced: `ArtSource/Tests/C0/TEST_C0_MetreMesh.blend` and
  `Assets/EscapeViggo/Art/Tests/C0/` (FBX, URP material, prefab, preview scene).
  Blender +Y forward / Z up becomes Unity +Z forward / Y up, with metre units,
  ground pivot and applied transforms. Existing gameplay assets were not replaced.
- Tier 2 scoped asset integration passed: Blender CLI, argument/error checks,
  repeated generation, saved-source reopen, FBX checks, Unity compilation/import,
  scale/orientation/mesh/material/reference validation, **2 targeted Edit Mode tests**,
  and Windows preview build with the FBX confirmed in packed build contents.
- Validation reused unchanged `Tools/Invoke-Unity.ps1` in `Builds/C0Validation`
  to preserve the open editor. C0 scripts/assets/metas match source byte-for-byte;
  original source GUIDs were preserved. Logs/tests are under that copy's `Logs/`;
  player: `Builds/C0Validation/Builds/C0Preview/C0Preview.exe`.
- Human Blender/Unity visual review remains: open the source and preview scene,
  inspect the blue 2 m test mesh, ground origin and raised +Z nose. At C0, rigging,
  skinning and animation were outside scope; see C1–C4 below. No core
  gameplay/network changes; no full-week/Relay regression was needed.
- `ArtReferences/` and supplied art/design documents remain untouched.

### C1–C4 electrician pipeline (2026-10-01)

- Editable sources: `ArtSource/Characters/BaseHuman/SRC_BaseHuman.blend` and
  `ArtSource/Characters/Electrician/SRC_Electrician.blend`. Canonical reference files
  currently have `.png.png` suffixes; the technical spec is `CHARACTER_ARTS_SPECS.md`.
- One 1.8355 m faceted electrician: 4,380 triangles, 21-bone Humanoid rig, all 17
  required bones plus shoulders/toes; blended torso/elbow/knee weights. No finger
  or facial rig. Yellow helmet/vest, blue workwear, glasses/hair, gloves, boots/belt.
- `Tools/Blender/create_electrician.py` regenerates the named sources and four review
  renders. `export_character.py` preserves manual source edits; `validate_character.py`
  checks saved sources/rig/weights/transforms/loops. The C0 static export is unchanged;
  `pipeline.py` adds a separate skinned export profile. See `Tools/Blender/README.md`.
- Unity: `Assets/EscapeViggo/Art/Characters/Electrician/` contains FBX with valid
  Humanoid Avatar and looping Idle/Walk/Run clips, URP materials, Speed controller,
  and `PF_Electrician.prefab`. All locomotion is in place; root motion is disabled.
- `ElectricianArtPipeline.Prepare` uses Unity prefab APIs to wire presentation into
  Player/NetworkPlayer. `ElectricianPresentation` reads existing movement/role/owner
  state: owner shadows only, remote visible; C5 adds Viggo to the same component. Existing
  colliders, Body references, movement and network authority remain unchanged.
- Front/side/back/three-quarter renders: `ArtSource/Characters/Electrician/Review/`.
  C3/C4 leg-twist fix: anatomical sides were reversed for the chosen forward axis,
  producing an incompatible Humanoid reference pose and inward shin twist. Generator
  now uses Left=-X / Right=+X and explicit sagittal leg axes in both source rigs.
  Mesh and physical skinning are unchanged. `RefreshRigReferencePose` rebuilds the
  reference skeleton/mapping without changing gameplay prefabs or materials.
  Tier 1: Blender checks, 5 targeted Edit Mode + 4 Play Mode tests, clean Humanoid
  import and actual runtime pose renders. Side/roll/bend-plane regressions now cover
  the defect missed by earlier checks. See `Review/RigValidation.md`, `BlenderRig/`
  and `UnityRig/` below the electrician source directory for axes and pose evidence.
  The earlier torso-cap adjustment did not solve the animation-driven issue.
  The current C5 validation rebuild includes these fixes.
  Human visual identity/readability/deformation/animation-feel acceptance remains.
  Crouch currently scales visual height; full crouch/jump/task animation and final
  stride/foot-placement polish are outside the minimum C3 locomotion scope.
- Checkpoint validation uses `Builds/C4Validation` to preserve the open editor.
  Tier 3 passed: compilation/asset checks, **102 Edit Mode + 18 Play Mode tests**,
  Windows development build, rendered baseline host/client/late-observer regression,
  reconnect and host restart. Character role/ownership checks passed on all peers.
  Playback and retargeting onto a proportion-changed test skeleton passed without
  root displacement. Repeated Blender geometry/material/weight/rig checksums match.
  `Assets/EscapeViggo` matches the validated copy byte-for-byte; Unity-generated build
  settings outside that directory were not copied back. Current player is at
  `Builds/Windows/EscapeViggo.exe`; detailed evidence paths are in the history.
  Full Relay/five-day regression was not rerun: gameplay/network authority is unchanged.
  This C1–C4 checkpoint did not include C5 assets; see the current C5 state below.

### C5 Viggo base model (checkpoint completed 2026-10-06)

- Source: `ArtSource/Characters/Viggo/SRC_Viggo.blend`; Unity exports/materials/prefab:
  `Assets/EscapeViggo/Art/Characters/Viggo/` (`Models/Viggo.fbx`, `PF_Viggo.prefab`).
- 1.66 m, 5,474 triangles, one faceted skinned mesh. Independently shaped jacket/belly,
  short legs, broad hips/arms, geometric grey hair/beard, orange/charcoal workwear.
  BaseHuman supplies the logical rig, not a uniformly scaled body mesh.
- 22 bones: corrected 21-bone Humanoid hierarchy plus `Belly` under `Spine`.
  Left=-X / Right=+X; matching sagittal knee hinges, forward toes, no negative scale.
  Source is T-pose. `ViggoArtPipeline.Prepare` refreshes the Avatar reference and
  matches C4's relaxed reference arm rotations; Viggo joint positions/leg bases remain
  its own. This prevents the shared Idle clip from incorrectly raising Viggo's arms.
- `create_viggo.py` generates the source/FBX/reviews; `validate_viggo.py` checks saved
  topology, normalized weights, hierarchy, scale/pivot/transforms and deterministic
  semantics. `export_character.py --character Viggo` preserves manual source edits.
- Valid Unity Humanoid, URP material remaps and shared electrician controller. No
  Viggo clips, root motion, physics, gameplay tuning or authority changes. Existing
  presentation selects the role mesh, hides the capsule and retains owner shadows.
- Targeted Tier 2: source reopen, repeat-generation checksum, export-only, Unity
  import/references, 10 Edit Mode and 6 graphics-enabled Play Mode tests pass.
  Knee/foot axial twist, lowered idle arms, all shared clips and zero root displacement
  are covered. No Humanoid/animation import warnings. Windows development build and
  rendered host/client, late observer, reconnect and host restart passed with both
  role meshes/owner shadows checked. Player: `Builds/Windows/EscapeViggo.exe`.
  Full five-day/Relay regression was not rerun; gameplay/network authority is unchanged.
- Review: `ArtSource/Characters/Viggo/Review/` contains front/side/back/three-quarter,
  face closeup, true-scale electrician comparison, `UnityRig/` playback images and
  `SourceValidation.json`. Detailed checkpoint evidence is in `Review/Validation.md`.
- Human approval is still required for proportions, belly size/shape, face, height,
  silhouette, low-poly style and reference similarity. Face/hair/beard and sleeve
  joins are deliberately geometric; shared stride/arm swing is not final Viggo style.
  C6 is unstarted; no final locomotion, Javell, discovery, Rage or capture animation.

### M21 presentation architecture

- `Scripts/Presentation/GameFeelSettings.cs` and `ScriptableObjects/GameFeel.asset`:
  master/effects/ambience/voice volumes, per-cue clips/range/cooldown/variation,
  footsteps and optional edge opacity. Configuration only.
- `AudioEmitter`: bounded local source pool, two looping channels, separate voice
  channel, null-safe clips, cooldowns and despawn/disable cleanup. No audio RPCs.
- `PlayerAudioFeedback`: existing rest-start/Javell boundary, rest loop/recovery,
  capture sequence, local/replicated movement footsteps and owner-only rest/Rage/capture edges.
- `TaskAudioFeedback`: existing `WorkTask.StateChanged`, revision-gated tool,
  wrong-input and completion cues. `BatteryAudioFeedback` observes battery state.
- `WorksiteAudioFeedback`: power/interruption events plus existing day/week/Rage
  state; worksite hum and Rage loops. Late joins seed snapshots without historical
  one-shots; active loops resume. Gameplay authority and rules are unchanged.
- `Editor/AudioSetup.cs`: idempotent setup and chained asset validation for all
  affected prefabs; Windows builds include M21 validation.
- `Audio/Development`: generated neutral placeholder WAVs. Four assignable Viggo
  voice slots remain empty, including `VoiceJavell`.
- `PresentationTests`, `AudioEmitterTests`, `Tools/Test-AudioFeedback.ps1` cover
  transition/cadence/lifecycle behavior and development multiplayer cue counts.
- README contains audio configuration and the human listening checklist.

## 11. Immediate Next Milestones

### M21 — Audio and Game Feel (engineering complete; human review pending)

All requested presentation hooks are wired with neutral placeholders or empty
voice slots. Human audibility, timing, spatial range and overlapping-cue judgement
remain acceptance items; automated state/event checks cannot establish them.

### M22 — First Art Pass

Priority: Viggo, electricians, battery/drill, task props, environment, UI.
Read the dedicated art specs when this work is authorized.

### M23 — Balance and Playtesting

Instrument/measure and tune from actual multiplayer playtests rather than theory alone.

## 12. Updating This File

After a meaningful architecture/milestone change:

- replace stale facts;
- add only information that helps the next agent orient quickly;
- keep historical debugging narratives out;
- keep this file concise enough to read at the start of every task.
