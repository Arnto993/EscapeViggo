# ESCAPE VIGGO — CODEX PROJECT INSTRUCTIONS

This file contains persistent instructions for coding agents working in this repository.
Keep this file short, stable, and operational. Do not turn it into milestone history.

## Work Scope / Token Discipline

Choose the smallest appropriate execution mode.

### FAST PATCH
Use for small visual, material, text, parameter, prefab or isolated asset changes.

Rules:
- Do NOT reread GAME_SPEC.md or large project documentation.
- Read only the directly affected files and any immediately relevant rule.
- Do NOT run full EditMode/PlayMode suites.
- Do NOT run Windows builds.
- Do NOT run multiplayer, Relay or full-week regression.
- Do NOT regenerate unrelated review artifacts.
- Do NOT update documentation unless project state materially changes.
- Make the smallest source-of-truth change.
- Run one or a few targeted sanity checks.
- Commit and stop.

Target: minimal tool calls and minimal context.

### NORMAL IMPLEMENTATION
Use for contained feature/art/animation work.
Read AGENTS.md, CURRENT_STATE.md and only relevant specification sections.
Use targeted validation while iterating.
Do not run broad regression unless affected systems justify it.

### CHECKPOINT
Use only for milestone completion, core gameplay changes, networking/authority
changes, or when explicitly requested.
Broader validation/builds are allowed here.

## 1. Project

- Project: **Escape Viggo**
- Engine: **Unity 6.3 LTS**
- Rendering: **Universal Render Pipeline**
- Language: **C#**
- Architecture: **GameObject / MonoBehaviour**
- Networking: **Netcode for GameObjects**
- Online services: **Unity Multiplayer Services / Sessions / Relay**
- Input: **Unity Input System**
- Primary platform: **Windows PC**
- Target match size: approximately **5–8 players**

Do not change the Unity editor version, render pipeline, networking framework, or core platform direction unless the user explicitly requests it.

## 2. Read Context Efficiently

Before a meaningful change, read in this order:

1. `AGENTS.md`.
2. `CURRENT_STATE.md`.
3. The active milestone or explicitly requested section of `MILESTONES.md`.
4. Relevant existing code, prefabs, scenes, configuration, and tests.

Then read **only the relevant sections** of other documentation when needed:

- `GAME_SPEC.md` — canonical game design and gameplay intent.
- `CHARACTER_ART_SPECS.md` — character modelling, rigging, animation, and character art work.
- `ART_DIRECTION_PROPS.md` — prop and environment art work.
- `README.md` — player/testing instructions and operational setup when relevant.
- `MILESTONE_HISTORY.md` — historical evidence only when debugging a regression or investigating why an old decision exists.

Do **not** reread complete large design/history documents by default.
Search or open the specific heading relevant to the task.

If `CURRENT_STATE.md` conflicts with the repository, the repository is the source of truth. Correct `CURRENT_STATE.md` as part of the task when appropriate.

## 3. Scope

Implement only:

1. The user's explicit request.
2. The active milestone if the user requested milestone work.
3. Small supporting changes required to make that work correct.

Do not automatically implement later milestones, backlog items, or adjacent features.
Stop when the requested scope is complete.

For an unspecified small decision, choose the simplest reversible option and make it configurable when useful.
Do not silently invent major design decisions.

## 4. Preserve Working Architecture

Prefer:

- small changes;
- local changes;
- clear responsibilities;
- composition over inheritance;
- reusable existing systems;
- configurable gameplay tuning;
- reversible implementation choices.

Do not broadly refactor working systems unless:

- the current architecture blocks the requested feature;
- there is a demonstrated technical problem; or
- the user explicitly requests the refactor.

A new feature is not permission to redesign unrelated code.

Avoid giant managers, unnecessary singletons, deep inheritance, static mutable gameplay state, and abstractions created only for imagined future needs.

## 5. Multiplayer Authority

Important gameplay state remains host/server authoritative.

The authority owns, where applicable:

- role assignment;
- match/week/day state;
- authoritative time;
- task state and completion;
- scoring;
- battery state, carrier, theft, and discovery;
- Viggo stamina/rest and Rage state;
- captures;
- Viggo distractions and power state;
- day/week results.

Clients may own local input and presentation.
Clients must not award themselves authoritative outcomes.
Never trust a client-provided score or completion result.
Prefer client intentions followed by server-side validation.

Do not redesign the project as an MMO, competitive shooter, lockstep simulation, or dedicated-server game. This is a small party game using a listen-server model.

## 6. Unity and Assets

Use official Unity packages where practical. Do not add third-party packages without a clear requirement.

Do not casually hand-edit large Unity scene/prefab YAML files.
Prefer Unity editor operations or focused editor automation when practical.
Never invent GUIDs or arbitrarily regenerate existing `.meta` files.

Do not edit or commit generated Unity directories such as:

- `Library/`
- `Temp/`
- `Logs/`
- `obj/`

Keep source assets and runtime Unity assets separated according to the established repository structure.

## 7. Gameplay and Presentation Separation

Gameplay state owns outcomes. Presentation reads state and reacts to it.

Examples:

- Rage state decides whether Viggo is raging; audio/animation/UI present it.
- Task state decides completion; sounds and effects react to completion.
- Capture state decides a capture; presentation shows feedback.

Animation, UI, VFX, and audio must not become the authoritative owner of gameplay state.

## 8. Configuration

Do not scatter important tuning values through code.
Reuse established configuration assets and ScriptableObjects where appropriate.

Typical configurable values include movement, stamina, rest, Rage, interactions, capture, task rewards, distraction timing, power timing, day duration, and audio/presentation tuning.

ScriptableObjects are configuration assets, not authoritative mutable multiplayer state.

## 9. Testing Strategy

Use the **lowest validation tier that gives credible evidence for the current change**.
Escalate when risk or scope requires it.

### Tier 1 — Targeted change

Use for small/local implementation work.

- Compile affected code.
- Run directly relevant Edit Mode/Play Mode tests.
- Validate directly affected assets/references when applicable.

Do not run the complete multiplayer/Relay suite for an isolated presentation, documentation, or local UI change unless there is a concrete reason.

### Tier 2 — Feature integration

Use when a feature crosses gameplay systems, networking, scenes, prefabs, or shared state.

- Compile.
- Run relevant Edit Mode and Play Mode suites.
- Run focused multiplayer validation when network behaviour is affected.
- Validate scene/prefab integration.

### Tier 3 — Milestone/checkpoint regression

Use when completing a milestone or changing central networking/authority/session architecture.

- Full compilation and asset validation.
- Full relevant Edit Mode and Play Mode suites.
- Windows development build when applicable.
- Baseline multiplayer regression.
- Relay/online regression when the milestone affects or could regress online multiplayer.

Do not repeatedly run expensive full-week + Relay validation during every intermediate edit. Run it at meaningful checkpoints.

Never claim a test passed if it was not run successfully.
Human fun, readability, feel, and balance must be reported as human acceptance items when automation cannot establish them.

## 10. Code Quality

Prefer readable modern C# supported by the current Unity version.

Conventions:

- Classes/methods/public properties: `PascalCase`
- Private fields: `_camelCase`
- Local variables: `camelCase`
- Interfaces: `IName`

MonoBehaviours should have focused responsibilities.
Reuse the existing interaction/task/scoring/presentation architecture instead of duplicating systems per feature.

Avoid obvious performance problems such as repeated global searches every frame, unnecessary allocations in `Update`, excessive physics queries, and unnecessary per-frame network traffic.
Do not prematurely optimize beyond demonstrated needs.

## 11. Git Discipline

Before major work, inspect Git state.

- Do not delete or revert unrelated user changes.
- Keep changes logically scoped.
- Do not rewrite history unless explicitly requested.
- Do not push unless the user explicitly asks.
- Commit only when the user/task requests a commit.
- Use concise descriptive commit messages.

Art/design documents supplied by the user must not be modified unless the current task explicitly requires it.

Never write credentials, secrets, or tokens into source files.

## 12. Documentation Discipline

Keep active operational documentation concise.

### `CURRENT_STATE.md`

Update it after meaningful architecture or milestone changes.
Replace obsolete information instead of appending a chronological diary.
It should remain a fast map of the current repository.

### `MILESTONES.md`

Keep roadmap, active requirements, acceptance criteria, and short status summaries.
Do not store long test transcripts or debugging narratives here.

### `MILESTONE_HISTORY.md`

Store historical checkpoint notes only when worth preserving.
Agents do not need to read it by default.

### `README.md`

Keep player-facing setup, build, playtest, and operational instructions that humans actually need.
Do not duplicate internal milestone history there.

## 13. Definition of Done

A requested feature is done only when, as applicable:

- code compiles;
- required assets/references exist;
- it is wired into the relevant scene/prefab;
- it can actually be exercised;
- multiplayer behaviour is verified when relevant;
- relevant tests pass;
- no known critical errors were introduced.

For a milestone, its acceptance criteria must also be satisfied.
If human acceptance remains, say so explicitly instead of claiming full completion.

## 14. Completion Report

At the end of meaningful work, report concisely:

1. What changed.
2. Important files/systems changed.
3. Validation performed and validation tier used.
4. Anything not verified.
5. Exact manual test the user should perform, if useful.
6. Whether the requested scope/milestone acceptance criteria are satisfied.
7. The local commit hash, if a commit was requested and created.

Do not automatically begin the next milestone.

## 15. Core Engineering Principle

When choosing between a large sophisticated solution and a smaller clear solution that satisfies the current task, choose the smaller clear solution.

Build Escape Viggo one reliable system at a time.
