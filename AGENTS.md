# ESCAPE VIGGO — CODEX PROJECT INSTRUCTIONS

This file contains persistent instructions for any coding agent working in this repository.

Read this file before making changes.

Also read:

- `GAME_SPEC.md`
- `MILESTONES.md`

The three documents have different purposes:

- `GAME_SPEC.md` defines the game.
- `MILESTONES.md` defines what should be built and in what order.
- `AGENTS.md` defines how you must work.

---

# 1. Project

Project name:

**Escape Viggo**

Engine:

**Unity 6.3 LTS**

Rendering:

**Universal Render Pipeline**

Language:

**C#**

Primary platform:

**Windows PC**

Architecture:

**GameObject / MonoBehaviour**

Networking:

**Netcode for GameObjects**

Online session services:

**Unity Multiplayer Services**

Input:

**Unity Input System**

---

# 2. Primary Rule

Build the game incrementally.

Do NOT attempt to implement the entire GAME_SPEC in one task.

Only implement:

1. The user's explicit request.
2. The currently active milestone in `MILESTONES.md`.
3. Small supporting changes required to make that milestone work.

Do not implement later milestones simply because they appear easy.

---

# 3. Before Changing Code

Before making meaningful changes:

1. Read `GAME_SPEC.md`.
2. Read `MILESTONES.md`.
3. Inspect the existing project.
4. Identify the currently relevant milestone.
5. Inspect related existing code before creating new systems.
6. Reuse working architecture where reasonable.

Do not assume that the repository still matches an earlier prompt.

The repository is the current source of truth for implementation state.

---

# 4. Preserve Working Behaviour

Do not rewrite working systems unnecessarily.

Prefer:

- Small changes.
- Local changes.
- Clear changes.
- Reversible changes.

Avoid broad refactors unless:

- The current architecture prevents the requested feature.
- There is a demonstrated technical problem.
- The user explicitly requests the refactor.

A new feature is not permission to redesign unrelated code.

---

# 5. Keep the Project Buildable

The Unity project should remain compilable after every meaningful implementation step.

Before considering a task complete:

- Resolve C# compiler errors caused by your changes.
- Resolve missing references caused by your changes.
- Run relevant automated tests when available.
- Check for obvious Unity console errors if tooling permits it.

Do not knowingly finish a task with a broken compilation state.

---

# 6. Unity Version

Do NOT:

- Upgrade Unity.
- Downgrade Unity.
- Change the project's Unity editor version.
- Migrate the project to another render pipeline.

The project targets:

**Unity 6.3 LTS**

---

# 7. Package Management

Do not add third-party packages without a clear requirement.

Prefer official Unity packages.

Current intended technologies include:

- Universal Render Pipeline.
- Unity Input System.
- Netcode for GameObjects.
- Unity Transport.
- Unity Multiplayer Services.
- Unity Test Framework.
- Multiplayer testing tools where useful.

When installing Unity packages:

- Prefer versions compatible with the current Unity editor.
- Prefer released/stable versions shown as compatible by Unity Package Manager.
- Do not deliberately choose experimental or pre-release versions unless required and explicitly justified.
- Do not manually edit package lock data unless necessary.

Do not replace Netcode for GameObjects with:

- Mirror.
- FishNet.
- Photon.
- Netcode for Entities.
- A custom networking framework.

unless the user explicitly changes the technical direction.

---

# 8. Networking Authority

Escape Viggo uses host/server-authoritative gameplay for important state.

The authority must control:

- Role assignment.
- Match state.
- Workday timer.
- Task completion.
- Scoring.
- Battery state.
- Battery carrier.
- Battery discovery.
- Viggo Rage Mode.
- Captures.
- End-of-day state.

Clients may own local input.

Clients must not independently award themselves gameplay outcomes.

Never trust a client-provided score value.

Prefer client requests followed by server-side validation.

---

# 9. Networking Philosophy

This is an approximately 5–8 player party game.

Do not design networking as if this were:

- A 128-player shooter.
- An MMO.
- A deterministic lockstep simulation.
- A dedicated esports server.

Priorities are:

1. Correct shared state.
2. Reasonable responsiveness.
3. Simple architecture.
4. Easy multiplayer testing.
5. Security against obvious client authority mistakes.

Do not prematurely optimize networking.

---

# 10. Host Architecture

Initial architecture should support a listen server:

- One player is host/server.
- That same player is also a local client.
- Other players connect as clients.

Dedicated servers are not part of the prototype.

Do not introduce dedicated-server infrastructure unless a later milestone requests it.

---

# 11. Multiplayer Development Order

Networking should be implemented progressively:

1. Local host/client networking.
2. Player spawning.
3. Movement synchronization.
4. Authoritative shared gameplay systems.
5. Local multiplayer testing.
6. Session/join-code flow.
7. Relay/internet connectivity.

Do not start with matchmaking.

---

# 12. C# Style

Prefer readable modern C# supported by Unity.

Naming:

- Classes: `PascalCase`
- Public properties: `PascalCase`
- Methods: `PascalCase`
- Private fields: `_camelCase`
- Local variables: `camelCase`
- Interfaces: `IName`
- Constants: follow established project convention

Use meaningful names.

Avoid abbreviations unless universally obvious.

Good:

`WorkdayClock`

`BatteryState`

`ViggoStamina`

Bad:

`WDC`

`BatMgr`

`StuffController`

---

# 13. Components

MonoBehaviours should have focused responsibilities.

Prefer:

`PlayerMovement`

`PlayerInteraction`

`ViggoStamina`

`BatteryStation`

`ScoreService`

over:

`EverythingManager`

Do not create a single giant `GameManager` containing unrelated gameplay.

A small coordinator responsible for match state is acceptable.

---

# 14. Composition Over Inheritance

Prefer composing GameObjects from focused components.

Avoid deep gameplay inheritance trees.

For example, do not create:

`Entity -> Person -> Worker -> Electrician -> PlayerElectrician`

unless there is a genuine need.

Use role/state/components where simpler.

---

# 15. Global State

Avoid unnecessary:

- Static mutable state.
- Global service locators.
- Singleton proliferation.

A small number of explicit persistent services may be appropriate.

Networking systems should integrate cleanly with `NetworkManager`.

---

# 16. ScriptableObjects

Use ScriptableObjects when they improve editable configuration.

Good candidates:

- Movement tuning.
- Viggo tuning.
- Match timing.
- Score values.
- Task definitions.
- Audio definitions.

Do not move runtime mutable network state into ScriptableObjects.

ScriptableObjects are configuration assets, not authoritative multiplayer state.

---

# 17. Hard-Coded Values

Do not scatter gameplay constants throughout code.

Important tuning should be adjustable.

Examples:

- Movement speed.
- Day duration.
- Stamina.
- Rest duration.
- Rage speed.
- Task rewards.
- Battery rewards.
- Capture range.
- Capture penalty.
- Interaction duration.

Use serialized configuration or appropriate ScriptableObjects.

---

# 18. Input

Use Unity's Input System.

Gameplay code should respond to actions rather than directly checking arbitrary hard-coded keys throughout multiple scripts.

Initial controls:

- WASD movement.
- Mouse look.
- Shift sprint.
- Space jump.
- Ctrl/C crouch.
- E interact.

The exact bindings may later change.

---

# 19. Interaction System

Prefer one reusable player interaction mechanism.

The player should not contain separate raycast code for:

- Battery.
- Tasks.
- Doors.
- Viggo interactions.

Use a small reusable abstraction such as an interactable interface or component where appropriate.

Do not build a complex interaction framework before it is needed.

---

# 20. Game State

Prefer explicit match phases/states.

Avoid large collections of loosely related booleans.

The current game phase should have one clear authoritative source.

Potential states include:

- Lobby
- Preparing
- Workday
- EndOfDay
- Results

Rage Mode may be represented as an independent modifier/state when appropriate.

---

# 21. UI Separation

Do not let UI own important gameplay state.

UI should:

- Read gameplay state.
- Present gameplay state.
- Send allowed player intentions.

UI should not:

- Determine scores.
- Advance the authoritative timer.
- Decide who has the battery.
- Decide if Rage Mode starts.

---

# 22. Presentation Separation

Whenever practical, separate:

**Game state**

from:

**Presentation**

Examples:

Rage state decides that Viggo is raging.

Separate presentation may:

- Play sound.
- Show UI.
- Trigger animation.
- Add camera effect.

This allows placeholder presentation during prototype development.

---

# 23. Scenes

Keep the number of scenes small during early development.

Expected early scenes:

- `Bootstrap`
- `MainMenu`
- `PrototypeWorksite`

Do not create one scene per system.

---

# 24. Scene and Prefab Editing

Unity scene and prefab files are serialized assets.

Do not casually make large hand-written YAML modifications.

Prefer:

- Unity editor operations when available.
- Editor scripts for repeatable setup.
- Prefab construction through Unity APIs where practical.

If text modification of Unity YAML is necessary:

- Make the smallest possible change.
- Preserve file structure.
- Preserve references.
- Never invent GUIDs.

---

# 25. Meta Files

Never arbitrarily delete or regenerate `.meta` files for existing assets.

Do not manually invent asset GUIDs.

When moving assets:

- Preserve their associated `.meta` files.

Broken GUID references can corrupt scenes and prefabs.

---

# 26. Generated Unity Directories

Never treat these as source code:

- `Library/`
- `Temp/`
- `Logs/`
- `obj/`

Do not edit them.

Do not commit them.

Do not depend on their contents.

---

# 27. Assets Structure

Prefer a clean structure similar to:

`Assets/EscapeViggo/Art`

`Assets/EscapeViggo/Audio`

`Assets/EscapeViggo/Materials`

`Assets/EscapeViggo/Prefabs`

`Assets/EscapeViggo/Scenes`

`Assets/EscapeViggo/Scripts`

`Assets/EscapeViggo/ScriptableObjects`

`Assets/EscapeViggo/UI`

`Assets/EscapeViggo/Tests`

Inside `Scripts`, organize by feature rather than creating dozens of tiny technical folders.

Potential feature areas:

- Core
- Networking
- Player
- Interaction
- Tasks
- Battery
- Viggo
- Scoring
- UI

Follow the existing project structure if it has already stabilized.

---

# 28. Namespaces

Use a project namespace.

Preferred root:

`EscapeViggo`

Examples:

`EscapeViggo.Player`

`EscapeViggo.Tasks`

`EscapeViggo.Networking`

Do not create excessive namespace nesting.

---

# 29. Assembly Definitions

Assembly definitions may be introduced when useful for:

- Runtime/editor separation.
- Tests.
- Compilation boundaries.

Do not create many assembly definitions without a practical benefit.

Editor-only code must not leak into runtime builds.

---

# 30. Editor Scripts

Editor automation is encouraged when it makes agent-driven development safer.

Possible uses:

- Construct prototype scenes.
- Create configuration assets.
- Validate project setup.
- Generate development prefabs.
- Verify required components.

Place editor-only code inside `Editor` folders.

Do not require editor scripts for normal runtime gameplay.

---

# 31. Testing

Write automated tests for deterministic gameplay logic where practical.

Good test targets:

- Workday time conversion.
- Score calculations.
- Battery state transitions.
- Match phase transitions.
- Stamina logic that can be isolated from frame/input dependencies.
- Task state rules.

Do not create meaningless tests that merely duplicate implementation.

Gameplay requiring full networking or physics may need integration/manual tests.

---

# 32. Multiplayer Testing

Every networked milestone should include a manual verification path.

Document how to verify:

- Start host.
- Start client.
- Observe synchronization.
- Trigger the new mechanic.
- Confirm both instances agree.

When automated multiplayer tests are practical, add them.

Do not claim multiplayer functionality is verified solely because one local player works.

---

# 33. Debug Tools

Development-only debug tools are encouraged.

Examples:

- Short workday duration.
- Force Rage Mode.
- Refill/deplete stamina.
- Award points.
- Reset battery.
- Display current network state.

Debug functionality must be clearly separated from normal production behaviour.

Do not ship debug UI accidentally.

---

# 34. Logging

Use useful logs during development.

Include context.

Good:

`Battery stolen by client 3`

Bad:

`working`

Avoid console spam every frame.

Remove or reduce noisy temporary logs after diagnosing issues.

---

# 35. Error Handling

Fail clearly.

If a required reference is missing:

- Produce a useful error.
- Avoid silent undefined behaviour.

Do not wrap large sections in empty catch blocks.

---

# 36. Performance

Do not optimize prematurely.

However:

Avoid obvious problems such as:

- `FindObjectOfType` repeatedly every frame.
- Unbounded allocations in `Update`.
- Sending network messages every frame when not needed.
- Excessive physics queries.
- Recreating UI repeatedly.

Optimize only when architecture or profiling gives a reason.

---

# 37. Physics

Use physics consistently.

Do not mix unrelated movement approaches without reason.

Character movement should initially use one clear implementation.

Do not introduce advanced custom physics unless normal Unity systems fail to meet the game's needs.

---

# 38. Placeholder Assets

Use placeholders freely.

Prototype visuals may use:

- Capsules.
- Cubes.
- Unity primitives.
- Simple materials.
- Text labels.

Do not search for or import arbitrary copyrighted assets to make the prototype prettier.

Gameplay progress is more important.

---

# 39. Viggo Placeholder

Until a real character is provided:

Represent Viggo with a clearly different placeholder.

Examples:

- Larger capsule.
- Wider scale.
- Distinct material.
- Floating "VIGGO" label during development.

Do not attempt to generate a realistic likeness from code.

---

# 40. Audio Placeholders

Gameplay must function with no final voice/audio assets.

If required:

- Use temporary neutral audio.
- Use visible debug text.
- Expose audio hooks for later clips.

Do not block functionality on voice files.

---

# 41. Documentation

When adding a major system, update relevant project documentation if its usage is not obvious.

Keep documentation concise and useful.

Do not generate large redundant design documents inside the repository.

`GAME_SPEC.md` remains the primary design source.

---

# 42. MILESTONES.md

`MILESTONES.md` is the development roadmap.

Do not mark a milestone complete merely because code exists.

A milestone is complete only when its acceptance criteria are satisfied.

If tooling prevents verification:

- State what was implemented.
- State what remains unverified.
- Do not pretend it passed.

---

# 43. Active Milestone

Unless the user explicitly names another target, work on the earliest milestone in `MILESTONES.md` whose acceptance criteria have not yet been met.

Do not automatically start the next milestone in the same task unless explicitly requested.

---

# 44. Scope Creep

Do not implement ideas listed under future possibilities simply because they appear in `GAME_SPEC.md`.

Examples of prohibited premature work:

- Voice chat.
- Steam integration.
- Character cosmetics.
- Multiple maps.
- Complex inventory.
- Matchmaking.
- Dedicated servers.

unless the relevant milestone or user explicitly requests them.

---

# 45. New Design Decisions

When a small unspecified decision is necessary:

1. Choose the simplest reversible option.
2. Make it configurable if appropriate.
3. Document the assumption briefly.

When a major design decision is necessary:

Do not silently invent it.

Surface the decision to the user when possible.

---

# 46. User Intent

The user is intentionally using Codex to perform a large portion of implementation.

Therefore optimize the repository for:

- Agent readability.
- Predictable structure.
- Easy validation.
- Small independent systems.
- Clear names.
- Repeatable setup.

Avoid clever code that is difficult for future agents or humans to understand.

---

# 47. Git Discipline

Keep changes logically scoped.

Before major changes, inspect Git state.

Do not delete unrelated user changes.

Do not revert changes you did not create unless explicitly requested.

Avoid committing generated Unity directories.

If instructed to commit:

- Use descriptive commit messages.
- Keep commits coherent.

Do not rewrite Git history unless explicitly instructed.

---

# 48. Do Not Touch

Do not modify:

- User documents unrelated to the task.
- IDE configuration unnecessarily.
- Machine-wide configuration.
- Unity installation.
- Credentials.
- Secrets.
- External accounts.

Never put secret tokens or credentials into source files.

---

# 49. Completion Report

At the end of a meaningful Codex task, summarize:

1. What was implemented.
2. Important files created or changed.
3. Tests/checks performed.
4. Anything that could not be verified.
5. The exact manual Unity test the user should perform.
6. Whether the active milestone's acceptance criteria are satisfied.

Keep this report concise.

---

# 50. Definition of Done

A feature is not done merely because scripts were created.

For a feature to be considered done:

- Code compiles.
- Required assets/references exist.
- It is wired into the relevant scene/prefab.
- It can actually be exercised.
- Multiplayer behaviour is verified when relevant.
- Relevant tests pass.
- There are no known critical errors introduced by the change.

---

# 51. Most Important Engineering Principle

When choosing between:

**A large sophisticated solution**

and:

**A smaller clear solution that satisfies the current milestone**

choose the smaller clear solution.

Build Escape Viggo one reliable system at a time.