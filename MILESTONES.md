# ESCAPE VIGGO — DEVELOPMENT ROADMAP

This file defines development order, active milestone requirements, and concise status.
Detailed historical evidence belongs in `MILESTONE_HISTORY.md`.

Do not implement a milestone merely because it appears here. Follow the user's explicit request.
Do not automatically continue into the next milestone.

## Roadmap

### Phase A — Foundation
- M0 — Repository and Project Health — **Complete**
- M1 — Prototype Worksite — **Complete**
- M2 — First-Person Controller — **Complete**
- M3 — Local Multiplayer Foundation — **Complete**

### Phase B — Core Match
- M4 — Roles and Player Spawning — **Complete**
- M5 — Workday Clock and Match State — **Complete**
- M6 — Interaction Framework — **Complete**
- M7 — First Electrical Task — **Complete**
- M8 — Scoring — **Complete**

### Phase C — Viggo
- M9 — Viggo Movement and Stamina — **Complete**
- M10 — Viggo Rest — **Complete**
- M11 — Battery Theft — **Complete**
- M12 — Battery Discovery and Rage — **Complete**
- M13 — Capture System — **Complete**

### Phase D — Complete Vertical Slice
- M14 — End-of-Day Flow — **Complete**
- M15 — Prototype UI and Feedback — **Complete**
- M16 — Local Multiplayer Playtest Build — **Complete**
- M17 — Online Session and Relay — **Engineering complete; separate-PC human Internet check remains useful**

### Phase E — First Proper Game Loop
- M18 — Five-Day Week — **Complete** — `bbb601d`
- M19 — Multiple Work Tasks — **Complete** — `2e54e57`
- M20 — Viggo Distraction Expansion — **Engineering complete; human fun/balance judgement pending** — `885de29`
- M21 — Audio and Game Feel — **Engineering complete; human audio/feel review pending**
- M22 — First Art Pass — **C1–C4 electrician checkpoint; remaining art not started**
- M23 — Balance and Playtesting — **Not started**

---

# M21 — AUDIO AND GAME FEEL

**Status (2026-10-01):** Engineering complete and committed locally. Reusable cue
configuration/emitter, all presentation hooks, neutral placeholders and empty
Viggo voice slots are wired. Tier 3 passed: compilation/assets, 96 Edit Mode + 15
Play Mode tests, Windows development build, rendered baseline multiplayer and
three-player local/Relay five-day weeks with Friday reconnect and cue-count checks.
Human listening, readability, timing and mix acceptance remain pending. See M22 for the subsequent art scope.

## Goal

Transform the functional prototype into a readable, comedic game experience through presentation without changing authoritative gameplay rules unnecessarily.

## Scope

Add or improve presentation for:

- electrician and Viggo footsteps;
- worksite ambience;
- tool/task interaction sounds;
- battery theft/discovery/recovery feedback;
- task completion/failure feedback;
- Viggo exhaustion/rest;
- Rage transition and Rage presentation;
- capture feedback;
- power-cut/restoration feedback where appropriate;
- end-of-day/day-transition/week-result feedback;
- reusable Viggo voice hooks.

Signature voice event:

**`Javell.`**

Final voice recordings or other final audio assets should only be integrated when supplied/approved. Placeholder or neutral development audio is acceptable.

## Architecture Requirements

- Presentation reacts to gameplay state/events; it does not own authoritative state.
- Reuse existing presentation/event boundaries where possible.
- Avoid embedding audio logic directly into unrelated gameplay systems when a small presentation component/event subscriber is cleaner.
- Avoid one-off duplicated audio infrastructure for every mechanic.
- Audio must not be required for gameplay correctness.
- Multiplayer clients should receive appropriate presentation without creating duplicate authoritative outcomes.
- Important audio volumes/settings should be configurable where practical.

## Acceptance Criteria

- Major existing gameplay events have clear audio/presentation feedback.
- Viggo exhaustion/rest and Rage transitions are immediately understandable without reading debug text.
- Task/battery/capture/power feedback is distinguishable enough for normal play.
- Audio/presentation does not change authoritative gameplay outcomes.
- Multiplayer does not produce obvious unintended duplicate event playback caused by replicated state/event handling.
- The game still functions correctly with final voice assets absent.
- Relevant targeted tests/validation pass.
- Human review confirms the feedback is understandable and not obviously overwhelming.

## Validation Guidance

During implementation, prefer targeted presentation/event tests and compile checks.
Run full multiplayer regression near the M21 checkpoint, not after every individual sound hook.
Relay regression is required only at checkpoint or if M21 changes shared networking/session/event-delivery behaviour in a way that could regress online play.

Stop after M21 unless the user explicitly requests M22.

---

# M22 — FIRST ART PASS

**Scoped progress (2026-10-01):** C0 verified at `fd24a02`; C1–C4 now implement
the reusable base, Humanoid rig, minimum in-place locomotion and first electrician.
Tier 3 engineering passed (102 Edit Mode, 18 Play Mode, build and baseline multiplayer).
Human visual acceptance remains. This does not complete M22: C5/Viggo, variants,
props, environment and UI art remain unstarted. Stop before C5.

## Goal

Replace the most important prototype visuals while preserving gameplay readability and the established stylized low-poly direction.

## Required Context

Before implementation, read the relevant sections of:

- `CHARACTER_ART_SPECS.md`
- `ART_DIRECTION_PROPS.md`
- `GAME_SPEC.md` art/gameplay-readability sections
- `CURRENT_STATE.md`

Do not reread unrelated historical milestone material by default.

## Priority

1. Viggo character.
2. Electrician character(s).
3. Viggo battery and drill.
4. Gameplay-critical task props.
5. Key worksite/environment kit.
6. UI visual pass.

## Requirements

- Preserve strong silhouettes and gameplay readability.
- Keep real-world approximate Unity scale.
- Prefer reusable modular assets.
- Preserve or improve existing colliders/interactions rather than binding gameplay to decorative mesh complexity.
- Character rigs/animations must follow the dedicated character specification.
- Keep gameplay/presentation separation intact.
- Do not block gameplay on completion of every decorative asset.

## Acceptance Criteria

- Viggo is clearly recognizable at gameplay distance.
- Electricians are visually distinct from Viggo and readable in multiplayer movement.
- Battery/drill and interactive task props are immediately identifiable.
- Replaced visuals retain correct scale, pivots/orientation, references, and required interaction/collision behaviour.
- The core prototype remains fully playable after the art replacement.
- Visual style is coherent enough that the game no longer reads primarily as greybox in its key gameplay areas.

Stop after M22 unless the user explicitly requests M23.

---

# M23 — BALANCE AND PLAYTESTING

## Goal

Tune Escape Viggo from actual multiplayer evidence.

## Measure

At minimum, collect or observe:

- tasks completed per day;
- task-type completion/failure patterns;
- Viggo capture count;
- battery theft frequency and success;
- time from theft to discovery;
- Rage duration;
- electrician and Viggo scores;
- Viggo distraction usage/effectiveness;
- power-cut frequency and recovery time;
- idle/dead time;
- whether players understand objectives/feedback;
- whether playing Viggo is enjoyable;
- whether electricians have meaningful choices.

## Tune

Potential tuning includes:

- day length;
- task difficulty/duration/rewards;
- Viggo movement;
- stamina and recovery;
- rest duration;
- Rage speed/behaviour;
- theft duration;
- capture range/penalties/rewards;
- distraction cooldown/effect;
- power-cut duration/restoration;
- score values.

## Rules

- Do not balance solely from theory.
- Preserve raw playtest observations before changing several variables at once.
- Prefer small tuning batches so cause/effect remains understandable.
- Do not redesign core systems merely to chase one unusual playtest outcome.

## Acceptance Direction

M23 is not “complete” because automated tests pass. Completion requires real multiplayer sessions providing enough evidence that the core loop is understandable, reasonably paced, and enjoyable enough to justify further expansion.

---

# POST-PROTOTYPE BACKLOG

Not approved implementation work unless explicitly requested:

- battery passing;
- multiple batteries;
- proximity voice chat;
- more maps;
- Hurtigbåtterminal;
- construction site;
- industrial building;
- random daily events;
- lunch/coffee/overtime systems;
- expanded tool/tool-theft mechanics;
- work quota;
- apprentice/foreman/safety-inspector roles;
- security cameras/evidence systems;
- character customization;
- achievements;
- Steam integration;
- public matchmaking;
- progression/statistics systems;
- additional Viggo abilities.

---

# MILESTONE WORKFLOW

For milestone work:

1. Read `AGENTS.md` and `CURRENT_STATE.md`.
2. Read only the requested milestone section here.
3. Read only relevant sections of `GAME_SPEC.md`/specialized specs.
4. Inspect current implementation and tests.
5. Implement the minimum coherent solution.
6. Use the validation tier defined in `AGENTS.md`.
7. At milestone checkpoint, update `CURRENT_STATE.md` and add a concise historical entry to `MILESTONE_HISTORY.md` if useful.
8. Report clearly and stop before the next milestone.
