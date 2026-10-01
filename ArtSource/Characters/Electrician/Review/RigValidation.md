# C3/C4 Humanoid leg basis correction

The source rig had **reversed anatomical side names**: Left was on +X, although
Blender +Y exports as Unity +Z forward. Both leg rolls were already equal (0 degrees);
there was no isolated right-shin 90-degree roll or negative scale. Unity interpreted
the named body backwards and built an incompatible Humanoid reference pose. Its
retargeting introduced about **83 degrees of inward axial shin twist on both legs**,
while compensating the feet. The earlier lower-torso mesh adjustment did not fix this.

## Source correction

- Generator uses Left = -X, Right = +X consistently for bones, skin-group names and
  animation channels in both BaseHuman and Electrician.
- Bone names and parent relationships remain the same; they now identify the correct
  anatomical sides. No negative-scale mirroring, mesh edits, weight redistribution,
  runtime correction rotations or gameplay changes were used.
- Explicit roll construction: leg local X is the hinge axis; local Z faces forward.
  Foot local Y runs toward the toes and local Z faces up. Object transforms are identity.
- Comparing old/new source geometry and weights by physical joint position confirmed
  identical vertex positions, topology, materials and physical skinning.
- Avatar reference data is rebuilt from the source bind transforms with all 21
  named bones explicitly mapped. Unweighted toe endpoints are retained.

## Local axes (Blender armature space)

These vectors apply to **both sides** and both source files. Local Y follows each
bone. Mirroring changes joint X positions, not the hinge/bend plane.

| Bone | Local X | Local Y | Local Z |
| --- | --- | --- | --- |
| UpperLeg | (1, 0, 0) | (0, 0.030755, -0.999527) | (0, 0.999527, 0.030755) |
| LowerLeg | (1, 0, 0) | (0, -0.030755, -0.999527) | (0, 0.999527, -0.030755) |
| Foot | (1, 0, 0) | (0, 0.953583, -0.301131) | (0, 0.301131, 0.953583) |

See [actual rest axes](BlenderRig/LegAxes.png) (RGB = local XYZ) and
[full measured vectors](BlenderRig/LegAxes.json). Knee centres are 12 mm forward
of hips/ankles; both chains remain in the sagittal plane.

## Targeted validation

- Blender saved-source, hierarchy, skinning, transforms and loop checks pass.
  New regression checks anatomical side, local hinge axis, left/right roll agreement,
  forward knee plane and BaseHuman/Electrician compatibility.
- Unity's Configure Avatar editor machinery (`SampleBindPose`, `MakePoseValid`,
  `GetPoseError`) was invoked in an isolated editor: corrected pose error is zero,
  and enforcing T-pose changes none of the six upper-leg/lower-leg/foot rotations.
- The Play Mode regression failed on the old rig: kneecap front vectors had sideways
  components +/-0.9921. After correction, the maximum sideways component across
  idle/walk/run is below 0.000002; idle is (0, 0, 1) on both knees. Feet also remain
  in the forward plane. Deep run flexion is permitted within that plane.
- Disabling Animator freezes its last evaluated pose; it does **not** automatically
  restore the bind pose. The test explicitly restores the recorded bind transforms
  for the disabled-Animator comparison. That restoration exists only in test code.
- Targeted tests: 5 C4 Edit Mode and 4 C4 Play Mode, including the existing shared
  animation retarget onto a proportion-changed skeleton. No broad regression/build.
- Logs: `Builds/C4Validation/Logs/C4Rig*.log/xml`; Blender comparison/diagnostics:
  `Logs/C4RigFix/`, source validation `Logs/C4SourceValidation.json`.

## Review images

`BlenderRig/` contains neutral T-pose, idle, walk and run front/three-quarter views.
`UnityRig/` contains actual Play Mode bind and Animator-driven front/three-quarter
views, with the animated views held at quarter-cycle for repeatable capture.
Both knees/feet were inspected; the inward-twisted shin protrusion is absent.

Existing character art polish outside the leg-twist issue is unchanged. Review the
corrected Unity project; the previously built Windows executable is not rebuilt.
C5/Viggo remains unstarted.
