# ESCAPE VIGGO — CURRENT IMPLEMENTATION STATE

**Purpose:** fast orientation for coding agents. This file describes the current repository, not its full history.

**Last consolidated checkpoint:** M20 implemented locally, 2026-10-01.

## 1. Current Development Position

- M0–M19: engineering/acceptance criteria completed.
- M20: engineering implementation completed; human fun/balance judgement remains a playtest concern.
- Next planned milestone: **M21 — Audio and Game Feel**.
- M21 is not authorized merely because it is listed here; follow the user's current request.
- Do not begin M22 or M23 unless explicitly requested.

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

The prototype remains visually greybox-oriented. Final art/audio are not yet integrated.

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

M18–M20 completion reported passing:

- Unity compilation;
- asset/reference validation;
- Windows development build;
- **91 Edit Mode tests**;
- **13 Play Mode tests**;
- baseline multiplayer regression;
- complete three-player five-day weeks using direct networking;
- complete three-player five-day weeks using actual Unity Sessions/Relay;
- Friday reconnect coverage.

Relay validation above used three processes on one PC.
Human separate-PC Internet testing and human feel/readability/fun assessment are distinct acceptance activities where relevant.

Use targeted tests during intermediate work. Reserve the complete regression/build/Relay suite for milestone checkpoints or changes that can plausibly affect central networking/authority.

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
- They were intentionally left untouched during M18–M20 engineering work.
- Greybox/placeholder presentation remains valid until the relevant art/audio milestone.
- M21 should create/use presentation hooks without requiring final approved recordings.
- M22 should consult the dedicated character/prop art specifications before replacing prototype visuals.

## 11. Immediate Next Milestones

### M21 — Audio and Game Feel

Focus on presentation: footsteps, ambience, tool/battery/task feedback, Viggo exhaustion, Rage transition, capture, end-of-day sound, and proper voice hooks.
Gameplay state must remain authoritative and separate from audio/presentation.

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
