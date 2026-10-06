"""Read-only Viggo saved-source, geometry, rig and C6/C7 action checks."""
import bpy, bmesh, json, hashlib
from pathlib import Path
from mathutils import Vector

root=Path(__file__).resolve().parents[2]
bpy.ops.wm.open_mainfile(filepath=str(root/'ArtSource/Characters/Viggo/SRC_Viggo.blend'))
rig=bpy.data.objects['EV_Humanoid']; mesh=bpy.data.objects['Viggo']
assert len(rig.data.bones)==22
assert set(bpy.data.actions.keys()) == {'Idle', 'Walk', 'Run', 'Javell'}
for action in bpy.data.actions:
    rig.animation_data.action = action
    rig.animation_data.action_slot = action.slots[0]
    ends = []
    for frame in range(int(action.frame_range[0]), int(action.frame_range[1]) + 1):
        bpy.context.scene.frame_set(frame)
        assert rig.location.length < 1e-6, 'Animated root translation'
        assert abs(rig.pose.bones['Hips'].head.x) < 1e-5
        assert abs(rig.pose.bones['Hips'].head.y) < 1e-5
        if action.name == 'Javell':
            for side in ['Left', 'Right']:
                hand = rig.pose.bones[side+'Hand']
                assert .98 < hand.head.z < 1.10 and .33 < hand.head.y < .40
                assert hand.tail.y > .42 and hand.tail.z > .92, 'Hands must cover belly, not knees'
        if frame in (int(action.frame_range[0]), int(action.frame_range[1])):
            ends.append([p.matrix.copy() for p in rig.pose.bones])
    assert all(max(abs(a[i][j]-b[i][j]) for i in range(4) for j in range(4)) < 1e-4
               for a,b in zip(*ends)), action.name+' loop seam'
rig.animation_data.action = None
assert rig.data.bones['Belly'].parent.name=='Spine'
assert mesh.parent==rig and mesh.modifiers[0].object==rig
assert not mesh.data.validate()
for obj in [rig,mesh]:
    assert obj.location.length<1e-6 and obj.rotation_euler.to_matrix().is_identity
    assert (obj.scale-Vector((1,1,1))).length<1e-6
for v in mesh.data.vertices:
    assert abs(sum(g.weight for g in v.groups)-1)<1e-5
    assert all(mesh.vertex_groups[g.group].name in rig.data.bones for g in v.groups)
bm=bmesh.new(); bm.from_mesh(mesh.data)
assert all(e.is_manifold for e in bm.edges), 'Open or non-manifold mesh component'
assert all(f.calc_area()>1e-10 for f in bm.faces)
bm.free()
axes={}
for side,sign in [('Left',-1),('Right',1)]:
    for part in ['Shoulder','UpperArm','LowerArm','Hand','UpperLeg','LowerLeg','Foot','Toes']:
        b=rig.data.bones[side+part]
        assert b.head_local.x*sign>0, b.name+' anatomical side'
        assert b.matrix_local.to_3x3().determinant()>.9999
    for part in ['UpperLeg','LowerLeg','Foot','Toes']:
        b=rig.data.bones[side+part]; basis=b.matrix_local.to_3x3()
        assert (basis.col[0]-Vector((1,0,0))).length<1e-5, b.name+' hinge'
        assert abs(b.head_local.x-b.tail_local.x)<1e-5
        axes[b.name]=[[round(c,6) for c in basis.col[i]] for i in range(3)]
    knee=rig.data.bones[side+'LowerLeg'].head_local
    assert knee.y>rig.data.bones[side+'UpperLeg'].head_local.y
    assert knee.y>rig.data.bones[side+'Foot'].head_local.y
    assert rig.data.bones[side+'Toes'].head_local.y>rig.data.bones[side+'Foot'].head_local.y
for part in ['UpperLeg','LowerLeg','Foot','Toes']:
    assert axes['Left'+part]==axes['Right'+part]
height=max(v.co.z for v in mesh.data.vertices)
assert 1.60<=height<=1.68 and abs(min(v.co.z for v in mesh.data.vertices))<1e-5
triangles=sum(len(p.vertices)-2 for p in mesh.data.polygons)
assert triangles<25000
def coords(v): return tuple(round(c,6) for c in v)
payload={
    'vertices':sorted((coords(v.co), sorted((mesh.vertex_groups[g.group].name,round(g.weight,6)) for g in v.groups)) for v in mesh.data.vertices),
    'faces':sorted((sorted(coords(mesh.data.vertices[i].co) for i in p.vertices),mesh.data.materials[p.material_index].name) for p in mesh.data.polygons),
    'materials':sorted((m.name,coords(m.diffuse_color)) for m in mesh.data.materials),
    'bones':sorted((b.name,b.parent.name if b.parent else None,coords(b.head_local),coords(b.tail_local),[coords(row) for row in b.matrix_local]) for b in rig.data.bones)}
report={'height':round(height,6),'triangles':triangles,'bones':len(rig.data.bones),
        'semantic_sha256':hashlib.sha256(json.dumps(payload,sort_keys=True).encode()).hexdigest(),
        'leg_axes':axes,'source_reopen_mesh_weights_transforms_pivot_passed':True}
baseline=json.loads((root/'ArtSource/Characters/Viggo/Review/SourceValidation.json').read_text())
assert report['semantic_sha256']==baseline['semantic_sha256'], 'C6/C7 changed C5 geometry, rig or skinning'
report['four_in_place_loops_and_both_hands_on_belly_passed']=True
# Core hierarchy must match the corrected base; only proportions and Belly differ.
parents={b.name:b.parent.name if b.parent else None for b in rig.data.bones if b.name!='Belly'}
bpy.ops.wm.open_mainfile(filepath=str(root/'ArtSource/Characters/BaseHuman/SRC_BaseHuman.blend'))
base=bpy.data.objects['EV_Humanoid']
assert parents=={b.name:b.parent.name if b.parent else None for b in base.data.bones}
report['base_hierarchy_matches']=True
(root/'Logs').mkdir(exist_ok=True)
(root/'Logs/C6C7Blender.json').write_text(json.dumps(report,indent=2))
print('C6-C7 SOURCE VALIDATION PASSED',json.dumps(report))
