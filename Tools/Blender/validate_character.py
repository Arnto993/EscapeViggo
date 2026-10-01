"""Read-only source integrity and loop checks; never saves/regenerates sources."""
import bpy, json, math
from mathutils import Vector
from pathlib import Path
root=Path(__file__).resolve().parents[2]
required={'Hips','Spine','Chest','Neck','Head'}
required.update(side+part for side in ['Left','Right'] for part in ['UpperArm','LowerArm','Hand','UpperLeg','LowerLeg','Foot'])
report={}
reference_legs=None
for folder in ['BaseHuman','Electrician']:
    bpy.ops.wm.open_mainfile(filepath=str(root/f'ArtSource/Characters/{folder}/SRC_{folder}.blend'))
    rig=bpy.data.objects['EV_Humanoid']; mesh=bpy.data.objects[folder]
    assert required.issubset(rig.data.bones.keys())
    assert len(rig.data.bones)==21
    assert rig.data.bones['Hips'].parent is None
    # +Y forward / Z up: anatomical left is -X. Reflection is in positions,
    # not negative object scale or a quarter-turn around the shin's local Y.
    axes={}
    for side,sign in [('Left',-1),('Right',1)]:
        for part in ['UpperLeg','LowerLeg','Foot']:
            b=rig.data.bones[side+part]
            assert b.head_local.x*sign>0, f'{b.name}: wrong anatomical side'
            assert abs(b.head_local.x-b.tail_local.x)<.00001, f'{b.name}: non-sagittal chain'
            basis=b.matrix_local.to_3x3()
            assert (basis.col[0]-Vector((1,0,0))).length<.00001, f'{b.name}: hinge/roll mismatch'
            assert basis.determinant()>.9999
            axes[b.name]={axis:[round(c,6) for c in basis.col[i]] for i,axis in enumerate(['X','Y','Z'])}
        knee=rig.data.bones[side+'LowerLeg'].head_local
        assert knee.y>rig.data.bones[side+'UpperLeg'].head_local.y
        assert knee.y>rig.data.bones[side+'Foot'].head_local.y, 'Knee bend must point forward'
    for part in ['UpperLeg','LowerLeg','Foot']:
        left=rig.data.bones['Left'+part].matrix_local.to_quaternion()
        right=rig.data.bones['Right'+part].matrix_local.to_quaternion()
        assert left.rotation_difference(right).angle<.0001, part+' roll mismatch'
    if reference_legs is None: reference_legs=axes
    else: assert axes==reference_legs, 'BaseHuman and Electrician rest axes differ'
    assert all(tuple(o.scale)==(1,1,1) and o.location.length<.00001 and all(abs(r)<.00001 for r in o.rotation_euler) for o in [rig,mesh])
    assert mesh.parent==rig and mesh.modifiers[0].object==rig
    for v in mesh.data.vertices:
        assert abs(sum(g.weight for g in v.groups)-1)<.00001
        assert all(mesh.vertex_groups[g.group].name in rig.data.bones for g in v.groups)
    loops={}
    if folder=='Electrician':
        assert {a.name for a in bpy.data.actions}=={'Idle','Walk','Run'}
        for action in bpy.data.actions:
            rig.animation_data.action=action
            poses=[]
            for frame in action.frame_range:
                bpy.context.scene.frame_set(int(frame))
                poses.append([tuple(p.matrix) for p in rig.pose.bones])
                assert rig.location.length<.00001 and rig.pose.bones['Hips'].location.length<.00001
            for first,last in zip(poses[0],poses[1]):
                assert all((a-b).length<.00001 for a,b in zip(first,last)),action.name
            loops[action.name]=list(action.frame_range)
    report[folder]={'bones':len(rig.data.bones),'vertices':len(mesh.data.vertices),'loops':loops,'leg_local_axes':axes,'passed':True}
(root/'Logs/C4SourceValidation.json').write_text(json.dumps(report,indent=2))
print('C1-C4 SOURCE REOPEN / RIG / WEIGHTS / TRANSFORMS / LOOPS PASSED')
