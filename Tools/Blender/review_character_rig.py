"""Render the saved C4 rig in bind/idle/walk/run; never changes the source."""
import bpy,json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'ArtSource/Characters/Electrician/SRC_Electrician.blend'))
rig=bpy.data.objects['EV_Humanoid']; worker=bpy.data.objects['Electrician']
source=(ROOT/'Tools/Blender/create_electrician.py').read_text()
setup=source[source.index('# Neutral studio views'):source.index('for name,loc in')]
exec(compile(setup,str(__file__),'exec'))
out=ROOT/'ArtSource/Characters/Electrician/Review/BlenderRig'; out.mkdir(parents=True,exist_ok=True)
for pose,frame in [('Neutral',1),('Idle',8),('Walk',8),('Run',6)]:
    rig.data.pose_position='REST' if pose=='Neutral' else 'POSE'
    if pose!='Neutral': rig.animation_data.action=bpy.data.actions[pose]
    scene.frame_set(frame)
    for view,location in [('Front',(0,5,1.1)),('ThreeQuarter',(3.6,5,2.25))]:
        camera.location=location
        camera.rotation_euler=(Vector((0,0,.94))-camera.location).to_track_quat('-Z','Y').to_euler()
        scene.render.filepath=str(out/(pose+view+'.png')); bpy.ops.render.render(write_still=True)
# Show actual rest-frame axes without mesh occlusion. RGB = local XYZ.
rig.data.pose_position='REST'; worker.hide_render=True
colors=[(1,.04,.02,1),(.03,.8,.08,1),(.03,.20,1,1)]
materials=[]
for axis,color in zip('XYZ',colors):
    material=bpy.data.materials.new('Axis'+axis); material.diffuse_color=color; material.use_nodes=True
    material.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=color
    materials.append(material)
def line(start,end,material,radius):
    direction=end-start
    bpy.ops.mesh.primitive_cylinder_add(vertices=8,radius=radius,depth=direction.length,location=(start+end)/2)
    obj=bpy.context.object; obj.rotation_euler=direction.to_track_quat('Z','Y').to_euler(); obj.data.materials.append(material)
axes={}
for side in ['Left','Right']:
    for part in ['UpperLeg','LowerLeg','Foot']:
        bone=rig.data.bones[side+part]; basis=bone.matrix_local.to_3x3()
        axes[bone.name]={axis:[round(c,6) for c in basis.col[i]] for i,axis in enumerate('XYZ')}
        line(bone.head_local,bone.tail_local,materials[1],.009)
        for i in range(3): line(bone.head_local,bone.head_local+basis.col[i]*.10,materials[i],.005)
        bpy.ops.object.text_add(location=bone.head_local+Vector((-.28 if side=='Left' else .10,0,.025)))
        text=bpy.context.object; text.data.body=bone.name; text.data.size=.027
        text.rotation_euler=(Vector((2.4,4,1.8))-text.location).to_track_quat('Z','Y').to_euler()
        text.data.materials.append(materials[0])
camera.location=(2.4,4,1.8); camera.rotation_euler=(Vector((0,0,.52))-camera.location).to_track_quat('-Z','Y').to_euler(); camera.data.ortho_scale=1.3
scene.render.filepath=str(out/'LegAxes.png'); bpy.ops.render.render(write_still=True)
(out/'LegAxes.json').write_text(json.dumps(axes,indent=2)+'\n')
print('C4 bind/idle/walk/run and RGB local-leg-axis renders complete')
