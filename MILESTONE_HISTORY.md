# ESCAPE VIGGO — MILESTONE HISTORY

This file preserves concise historical checkpoint information.
It is **not required reading for normal Codex tasks**.
Read it only when investigating a regression, an old design/implementation decision, or historical validation evidence.

The repository and `CURRENT_STATE.md` describe the current implementation.

## M0–M14 — Core Prototype

Established the Unity/URP project, greybox worksite, first-person controller, local NGO multiplayer, roles/spawning, authoritative workday, shared interaction framework, first electrical task, authoritative scoring, Viggo stamina/rest, battery theft/discovery/Rage, capture, and end-of-day results.

Key architectural principles established during these milestones:

- listen-server multiplayer;
- owner-local input/movement presentation with authoritative shared gameplay outcomes;
- server-validated interactions;
- common task/scoring architecture;
- gameplay/presentation separation;
- configurable tuning assets;
- automated Edit Mode/Play Mode plus multi-process regression coverage.

Detailed logs from these milestones should remain in Git history and ignored validation logs rather than being copied into active instructions.

## M15 — Prototype UI and Feedback

Checkpoint commit: `5f86d2a`

Added player-facing prototype HUD/feedback for role, score, clock, objectives, interaction, stamina/rest/Javell, battery state, Rage, capture, tasks, and results.
Engineering validation passed; human readability/usability remained a manual judgement.

## M16 — Local Multiplayer Playtest Build

Checkpoint commit: `b8e3d0e`

Added the practical connection/playtest flow, runtime day presets, recovery guidance, and local multi-window launcher.
Established a complete locally playable workday build and full-loop regression harness.

## M17 — Online Sessions and Relay

Checkpoint commit: `e79080f`

Added Unity Multiplayer Services private Sessions/Relay with anonymous authentication while preserving direct/local development networking.
Real Relay gameplay was validated between multiple processes.
Graceful and abrupt host-loss recovery were exercised.

Known operational note:
Multiple online processes on one Windows account should use distinct `-ev-online-profile` values to avoid cached Unity Authentication identity reuse. Separate PCs do not need this argument.

Human two-PC/separate-network acceptance remains useful even though real Relay was exercised on one PC.

## M18 — Five-Day Week

Checkpoint commit: `bbb601d`

Acceptance criteria satisfied.
Expanded the match from one workday to a Monday–Friday loop with cumulative scoring/results and correct reset/carry-over behaviour between days.

## M19 — Multiple Work Tasks

Checkpoint commit: `2e54e57`

Acceptance criteria satisfied.
Added multiple electrician task types using the shared task/scoring architecture:

- Simple Circuit
- Voltage Measurement
- Outlet Replacement
- Breaker Installation

## M20 — Viggo Distraction Expansion

Checkpoint commit: `885de29`

Engineering implementation completed.
Added Viggo task interruption and temporary power-cut gameplay with server validation, cooldown/state handling, score-safety, and electrician recovery/restoration behaviour.

Validation reported at the M20 checkpoint:

- Unity compilation;
- asset/reference validation;
- Windows build;
- 91 Edit Mode tests;
- 13 Play Mode tests;
- baseline multiplayer regression;
- complete three-player five-day direct-networking week;
- complete three-player five-day actual Sessions/Relay week;
- Friday reconnect coverage.

Human fun/balance judgement remains intentionally separate from engineering completion.

## Historical Documentation Rule

Do not append long raw logs, command transcripts, or every fixed diagnostic race here.
Preserve only historical facts that could help future maintenance.
Git history and ignored validation logs remain the detailed evidence source.

## M21 — Audio and Game Feel — 2026-10-01

Engineering complete; human audio/game-feel acceptance remains pending. Added a
shared GameFeel settings asset, bounded local emitters, player/task/battery/worksite
presentation adapters, 23 neutral generated placeholder clips and four empty
Viggo voice slots (including Javell). Seven existing prefabs were wired using Unity
editor automation. No authoritative gameplay rules, network variables, RPCs,
frameworks or M22 art were changed.

Validation used an isolated project at `Builds/M21Validation` because the source
project was open in Unity. Source assets/scripts/metas matched byte-for-byte;
Unity-generated source assets/metas and the validated Windows player were copied
back. Sandbox licensing stalled initially; normal licensing access resolved it.

- Targeted: 5 presentation Edit Mode tests and 2 emitter lifecycle Play Mode tests passed.
- Tier 3: full compilation/chained asset validation, 96 Edit Mode tests, 15 Play Mode
  tests and Windows development build passed. Logs/XML: `Builds/M21Validation/Logs/`
  (`M21Setup`, `M21TargetedEdit`, `M21TargetedPlay`, `M21FullEdit`, `M21FullPlay`, `M21Build`).
- Rendered local baseline, late observer, reconnect and host restart passed:
  `Multiplayer-20261001-120840-27777`. Rage-edge screenshot inspected.
- Three-player five-day local week passed: `Week-20261001-121104-28777`.
- Three-player actual Sessions/Relay five-day week passed: `Week-20261001-120843-29777`.
  Both week runs matched daily/weekly results and passed Friday reconnect.
- `Tools/Test-AudioFeedback.ps1` passed on both baseline peers and every local/Relay
  week peer: exact task, battery, rest/Javell, Rage, capture, power, day and week cue
  counts. Each week peer requested WeekResults once, including the reconnecting peer.
  Tests verify cue delivery, not subjective audibility. A Friday late-join guard
  suppresses replay even when result lists become ready after initial variables.
- Human acceptance: follow README's M21 listening checklist with real players;
  judge mix, spatial range, exhaustion/Rage/capture readability, timing and clutter.
  Approved voice recordings are absent by design. Separate-PC Internet listening
  was not performed. Stop before M22.

## C1-C4 � Electrician character pipeline checkpoint (2026-10-01)

Scope: verified C0 `fd24a02` extended only through C4. No C5/Viggo production,
variants, environment/prop art or gameplay authority changes. Reference/spec files
were preserved. Repository filenames are `CHARACTER_ARTS_SPECS.md` and the supplied
reference PNGs have doubled `.png.png` extensions.

Outputs: editable BaseHuman/Electrician blends, 4,380-triangle / 1.8355 m canonical
faceted electrician, 21-bone rig (all 17 required plus shoulders/toes), minimum
Idle/Walk/Run actions at 30 fps, Humanoid FBX/Avatar, nine URP colour materials,
Speed blend controller and presentation prefab. Player/NetworkPlayer prefab changes
only add the presentation component/child. Existing colliders, Body references,
movement, cameras, role state and network components are preserved. Smoke harness
adds read-only character assertions; no gameplay networking changes.

Tier 3 evidence:
- Blender CLI generation, both saved-source reopens, bone hierarchy/weights/applied
  transforms, binary FBX, exact action endpoint poses and in-place roots passed.
  Reports: `Logs/C4Blender.json`, `Logs/C4BlenderFirst.json`,
  `Logs/C4SourceValidation.json`. Repeated semantic SHA-256:
  `4a850934e981304a616f4fe836d344e405fb12dc4d502b62a061f85020f67a5f`.
  Checksum canonicalizes geometry/materials/weights/rig rather than timestamps/IDs.
- Isolated Unity project: `Builds/C4Validation`. Licensing service required the
  normal unsandboxed batch process. `Logs/C4PrepareFinal.log` confirms valid Humanoid,
  three looping clips, bone/mesh/URP references and presentation-only prefab.
- `Logs/C4EditModeFinal.xml`: **102/102 passed**. An initial test incorrectly assumed
  one network collider; corrected to preserve the existing interaction trigger and
  assert no colliders on the new visual. Metre scale, ground pivot, +Z feet verified.
- `Logs/C4PlayModeFinal.xml`: **18/18 passed**, including actual idle/walk/run Animator
  playback, no root displacement, owner shadow-only visibility, and shared walk
  retargeted onto a temporary Humanoid skeleton with altered leg proportions.
- `Logs/C4Build.log`: compilation, existing M1-M21 asset validation chain and Windows
  development build passed. Validated player copied to `Builds/Windows/`.
- `Logs/Multiplayer-20261001-135932-17844/` within the isolated project: rendered
  host/client, late observer, reconnect and host restart passed. `EV_CHARACTER`
  verifies valid in-place Humanoid, role visibility and local/remote shadow modes.
  Existing movement, roles, workday, interaction/tasks/scoring, stamina/rest,
  battery/Rage, capture/respawn and frozen-results checks all passed.
- Source `Assets/EscapeViggo` matches the validated copy byte-for-byte, including
  original GUIDs and the new Unity-generated metas. Unrelated automatic URP build
  serialization under `Assets/Settings` remains confined to the validation copy.
- Full five-day/Relay regression not rerun: no material gameplay/network change.

Human review artifacts: `ArtSource/Characters/Electrician/Review/{Front,Side,Back,ThreeQuarter}.png`.
Shoulder openings and knee-pad intersections found in first renders were corrected.
Engineering acceptance is satisfied; visual identity, gameplay-distance readability,
deformation and animation feel require human sign-off. C3 deliberately has minimum
locomotion only: crouch scales visual height; contextual crouch/jump/task animation
and final stride/foot-placement polish remain outside this checkpoint. C5 is unstarted.

## C3/C4 Humanoid leg-twist correction

Manual inspection exposed an animation-only defect missed by the original validity
and coarse playback tests. Both source rolls were equal and object transforms were
positive; the generator had reversed Left/Right anatomy for +Y Blender / +Z Unity
forward. Unity's inferred reference pose faced backward, then Humanoid introduced
approximately 83-degree inward shin twist while compensating the feet. The previous
mesh-cap edit was not the root-cause fix.

Corrected paired-bone/group/channel anatomy in BaseHuman and Electrician, explicitly
constructed sagittal leg/foot roll, and rebuilt the imported reference skeleton and
complete mapping. Geometry and weights at physical joint positions are unchanged.
No runtime rotations, gameplay/presentation logic changes or C5 work. Existing user
material edits were preserved outside the commit.

Tier 1: Blender source/loop/weight/axis checks, 5 C4 Edit Mode and 4 C4 Play Mode tests
passed. The new test reproduced the defect on the old rig and measures all sampled
idle/walk/run knee and foot directions. Sideways knee component dropped from 0.9921
to below 0.000002. Configure Avatar's editor pose validator/enforcer reports zero
pose error and no enforced leg rotations. Final import has no animation/retargeting
warnings; existing proportion-change retarget test passes. Runtime front/three-quarter
captures and full axis/root-cause evidence: `ArtSource/Characters/Electrician/Review/RigValidation.md`.
Logs: `Builds/C4Validation/Logs/C4RigEditFinal.xml`, `C4RigRenderedFinal.xml`,
`C4RigCleanImport.log`, `C4RigFinalDiagnosis.log`; Blender checks in `Logs/C4RigFix/`.
Windows player and multiplayer regressions were not rebuilt/rerun for this scoped fix.
