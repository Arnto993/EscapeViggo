# ESCAPE VIGGO — CODEX TASK TEMPLATE

Use this as a starting point for substantial Codex tasks.
Delete irrelevant lines instead of expanding the prompt unnecessarily.

## Milestone implementation

```text
Implement MXX only. Do not begin MXX+1.

Read first:
- AGENTS.md
- CURRENT_STATE.md
- the MXX section of MILESTONES.md

Then inspect only the existing code/assets/tests relevant to this milestone.
Read only relevant sections of GAME_SPEC.md or specialized art docs; do not reread completed milestone history unless a concrete dependency requires it.

Preserve existing multiplayer authority and working architecture.
Prefer the smallest reusable implementation that satisfies MXX.
Do not implement backlog/future features.

During implementation use targeted validation.
When MXX is ready for checkpoint, run the appropriate full milestone validation tier from AGENTS.md.

Update CURRENT_STATE.md with the new current architecture/status.
Add only a concise checkpoint entry to MILESTONE_HISTORY.md if useful.

Commit locally with a descriptive message if the task requests a commit.
Do not push.

Report:
- what changed;
- important files/systems;
- validation performed;
- what remains human/unverified;
- whether acceptance criteria are satisfied;
- commit hash if created.

Stop after MXX.
```

## Small bug/fix task

```text
Fix only this issue: <issue>.

Read AGENTS.md and CURRENT_STATE.md, then inspect the directly related implementation/tests.
Do not perform a general repository redesign or reread full milestone history.

Preserve current gameplay/network authority.
Make the smallest clear fix.
Run targeted compile/tests that demonstrate the fix and protect against regression.
Escalate to broader multiplayer regression only if the changed code can plausibly affect shared/networked behaviour.

Do not start unrelated milestone work.
Do not push.
```

## Art/presentation task

```text
Implement only: <art/presentation target>.

Read AGENTS.md and CURRENT_STATE.md.
Read the relevant specialized art specification sections for this asset/character only.
Inspect the current prefab/material/animation/presentation integration before changing it.

Keep gameplay state authoritative and separate from presentation.
Preserve interaction, collider, pivot, scale, rig, network, and prefab requirements.
Use targeted validation during iteration; run broader regression only when integration risk warrants it.

Do not modify unrelated art/design documents.
Do not begin the next milestone.
Do not push.
```
