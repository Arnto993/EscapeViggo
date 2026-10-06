# C5 Viggo base model — engineering review

Source: `../SRC_Viggo.blend`. Runtime: `Assets/EscapeViggo/Art/Characters/Viggo/`.
Height **1.66 m**, **5,474 triangles**, **22 bones**, one combined skinned mesh.

The jacket uses elliptical section rings with a forward-swelling belly distinct
from the smaller chest. Legs and arms have bend rings and blended weights.
Boots/gloves and face/hair/beard use simple faceted masses. All object transforms
are applied, pivot is at ground origin, and there are no negative scales.
The electrician mesh was not scaled into Viggo. Only its established logical
BaseHuman hierarchy and shared validation clips are reused.

## Rig and reference compatibility

The 21 core bones retain C4's anatomical Left=-X / Right=+X convention with
matching sagittal leg hinges and forward toes. `Belly`, parented to `Spine`, is
the only extra bone; it has normalized blended skin weights and no simulation.
It is excluded from Humanoid mapping. No finger/facial/cloth rig was added.

Viggo's saved source and mesh bind pose are T-pose. The existing C4 FBX stores
relaxed Idle arm rotations in its Avatar reference; sharing its clips with a
raw T-pose Avatar initially raised Viggo's arms. C5 now copies only the shared
arm reference rotations into its Avatar import description, retaining Viggo's
joint positions and leg bases. This is an import calibration, not a runtime
pose override. The arm regression checks both downward upper arms and hands
below elbows at idle, in addition to the existing leg tests.

## Completed targeted checks

- Blender saved-source reopen, valid nondegenerate/manifold component topology,
  normalized weights, hierarchy, transforms, metre scale, origin and axes.
- Repeat generation/export: matching geometry/material/weight/rig semantic SHA-256
  `1894dae22f4c15351b635443cb569992b85cc2490c80f1c57454db171f642419`.
  Binary timestamps are not a determinism promise. See `SourceValidation.json`.
- Export-only from saved source via the existing skinned FBX profile.
- Unity compilation, valid Humanoid Avatar, explicit mapping, material/bone/prefab
  references and existing player collider/Body references. No Humanoid/animation
  import warnings in the final import/test runs.
- 10 targeted Edit Mode tests and 6 graphics-enabled Play Mode tests passed.
  All three shared clips play without moving the root. Idle arms lower correctly.
  Maximum knee-front sideways component: 0.000658; foot sideways: 0.000644
  (test tolerance 0.05). Both knees remain forward through deep run flexion.
- Unity captured actual Animator-driven Idle/Walk/Run at quarter cycle, plus
  the unanimated bind pose. See `UnityRig/`.

Windows development build passed. On 2026-10-06 the rendered local host/client
checkpoint passed, including late observer, reconnect, host restart, role mesh
selection, owner shadows and hidden capsules. The existing baseline gameplay
checks also passed. Evidence: `Logs/Multiplayer-20261006-135152-17855/`.
Gameplay/network authority was unchanged; full five-day and Relay suites were
not rerun. Separate-PC Internet testing and human visual approval remain unverified.

Other logs are under project `Logs/`:
`C5ArmReference.log`, `C5EditMode.log/xml`, `C5PlayMode.log/xml`, `C5Build.log`.
This satisfies C5 engineering acceptance at scoped Tier 2 presentation integration,
not full M22 or human visual acceptance. The local Windows player is
`Builds/Windows/EscapeViggo.exe`. Existing electrician material edits were preserved;
incidental Unity-generated build settings were restored and excluded from C5.

## Human review gate

Review `Front.png`, `Side.png`, `Back.png`, `ThreeQuarter.png`, `FaceCloseup.png`
and `ViggoVsElectrician.png`. The comparison uses the unchanged canonical
electrician Blender source at true relative metre scale; it does not overwrite
the user's Unity electrician material edits.

Human approval remains required for body proportions, belly size/shape, face,
height, silhouette, low-poly style and similarity to both approved Viggo references.
The facial masses and sleeve joins are intentionally visibly geometric and need
that review. Shared electrician stride/arm swing is only an import/retarget smoke
test and is not Viggo movement personality. No final Viggo animations are authored.

Manual check: host and join two windows; view Viggo from the electrician window
at idle, walk and run, then crouch/turn and reconnect. Check orange/black silhouette
at 5–15 m, forward knees/feet, no visible placeholder, and owner shadows with no
local face obstruction. Judge appearance against the references separately from
the automated engineering result. Stop before C6.
