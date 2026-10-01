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
