"""C1-C4 authored low-poly electrician. Regeneration overwrites only named outputs.
Meshes remain editable; rerun export_character.py after hand editing the source.
"""
import sys, math, json, hashlib
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
from mathutils import Vector, Quaternion
from pipeline import reset_scene, apply_transforms, export_humanoid_fbx
ROOT = Path(__file__).resolve().parents[2]
reset_scene()
M = {}
for name, rgb in {'Blue':(.035,.10,.26),'Yellow':(.85,.92,.025),'Helmet':(1,.58,.012),
                  'Skin':(.65,.30,.15),'Hair':(.085,.032,.014),'Dark':(.022,.026,.032),
                  'Silver':(.57,.62,.64),'Leather':(.34,.14,.036),'White':(.92,.89,.78)}.items():
    m = bpy.data.materials.new('MAT_Electrician_'+name)
    m.diffuse_color = (*rgb,1); m.use_nodes=True
    m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(*rgb,1)
    m.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.78
    M[name]=m
parts=[]
def finish(obj,name,mat,bone):
    obj.name=name; apply_transforms(obj); obj.data.materials.append(M[mat])
    if bone: obj.vertex_groups.new(name=bone).add(list(range(len(obj.data.vertices))),1,'REPLACE')
    parts.append(obj)
    return obj

def ell(name,loc,scale,mat,bone,segments=12,rings=6):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rings,radius=1,location=loc)
    obj=bpy.context.object; obj.scale=scale
    return finish(obj,name,mat,bone)

def box(name,loc,scale,mat,bone,bevel=.015):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    obj=bpy.context.object; obj.scale=scale; apply_transforms(obj)
    if bevel:
        mod=obj.modifiers.new('Silhouette bevel','BEVEL'); mod.width=bevel; mod.segments=1
        bpy.context.view_layer.objects.active=obj; bpy.ops.object.modifier_apply(modifier=mod.name)
    return finish(obj,name,mat,bone)

def tube(name,rings,mat,bone,n=8):
    # Elliptical cross sections in XY, wound outwards; intermediate rings support bends.
    verts=[]; faces=[]
    for x,y,z,rx,ry in rings:
        verts.extend((x+rx*math.cos(2*math.pi*i/n),y+ry*math.sin(2*math.pi*i/n),z) for i in range(n))
    for j in range(len(rings)-1):
        for i in range(n): faces.append((j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i))
    faces.extend([tuple(reversed(range(n))),tuple((len(rings)-1)*n+i for i in range(n))])
    mesh=bpy.data.meshes.new(name); mesh.from_pydata(verts,[],faces); mesh.update()
    obj=bpy.data.objects.new(name,mesh); bpy.context.collection.objects.link(obj)
    return finish(obj,name,mat,bone)

# T-pose, ground origin, Blender +Y forward. 1.84 m including helmet.
bpy.ops.object.armature_add(); rig=bpy.context.object; rig.name='EV_Humanoid'
bpy.ops.object.mode_set(mode='EDIT'); rig.data.edit_bones.remove(rig.data.edit_bones[0])
def bone(name,head,tail,parent=None):
    b=rig.data.edit_bones.new(name); b.head=head; b.tail=tail
    if parent: b.parent=rig.data.edit_bones[parent]
    return b
bone('Hips',(0,0,.88),(0,0,1.03))
bone('Spine',(0,0,1.03),(0,0,1.19),'Hips'); bone('Chest',(0,0,1.19),(0,0,1.40),'Spine')
bone('Neck',(0,0,1.40),(0,0,1.49),'Chest'); bone('Head',(0,0,1.49),(0,0,1.77),'Neck')
for side,s in [('Left',1),('Right',-1)]:
    bone(side+'Shoulder',(s*.045,0,1.38),(s*.22,0,1.38),'Chest')
    bone(side+'UpperArm',(s*.22,0,1.38),(s*.48,0,1.38),side+'Shoulder')
    bone(side+'LowerArm',(s*.48,0,1.38),(s*.70,0,1.38),side+'UpperArm')
    bone(side+'Hand',(s*.70,0,1.38),(s*.81,0,1.38),side+'LowerArm')
    bone(side+'UpperLeg',(s*.115,0,.91),(s*.115,.012,.52),'Hips')
    bone(side+'LowerLeg',(s*.115,.012,.52),(s*.115,0,.13),side+'UpperLeg')
    bone(side+'Foot',(s*.115,0,.13),(s*.115,.19,.07),side+'LowerLeg')
    bone(side+'Toes',(s*.115,.19,.07),(s*.115,.26,.07),side+'Foot')
bpy.ops.object.mode_set(mode='OBJECT')

def skin_rings(obj,lower,upper,z0,z1):
    obj.vertex_groups.clear(); a=obj.vertex_groups.new(name=lower); b=obj.vertex_groups.new(name=upper)
    for v in obj.data.vertices:
        w=max(0,min(1,(v.co.z-z0)/(z1-z0)))
        if w<1: a.add([v.index],1-w,'REPLACE')
        if w>0: b.add([v.index],w,'REPLACE')

def make_body(base=False):
    mat='Skin' if base else 'Blue'
    torso=tube('Torso',[(0,0,.90,.17,.105),(0,0,1.03,.16,.11),(0,0,1.17,.18,.12),(0,0,1.34,.225,.12),(0,0,1.40,.17,.095)],mat,'Spine',12)
    skin_rings(torso,'Spine','Chest',1.10,1.29)
    if not base:
        # Recess the central lower cap into the trousers instead of leaving a
        # flat shirt tab projecting into the gap between the upper legs.
        for vertex in torso.data.vertices:
            if abs(vertex.co.z - .90) < .00001:
                edge = abs(vertex.co.x) / .17
                vertex.co.z += .05 * (1 - edge * edge)
                vertex.co.y *= .75 + .25 * edge
    ell('Neck',(0,0,1.46),(.064,.065,.105),'Skin','Neck')
    tube('Head',[(0,.01,1.48,.06,.058),(0,.015,1.52,.09,.085),(0,0,1.62,.115,.098),(0,-.008,1.71,.11,.092),(0,-.008,1.75,.07,.065)],'Skin','Head',12)
    for side,s in [('Left',1),('Right',-1)]:
        leg=tube(side+'Leg',[(s*.115,0,.13,.077,.075),(s*.115,0,.29,.077,.081),(s*.115,.012,.48,.075,.083),(s*.115,.012,.55,.085,.088),(s*.115,0,.75,.092,.10),(s*.115,0,.94,.10,.105)],mat,side+'UpperLeg')
        skin_rings(leg,side+'LowerLeg',side+'UpperLeg',.47,.58)
        ell(side+'Foot',(s*.115,.072,.085),(.092,.185,.085),'Skin' if base else 'Dark',side+'Foot')
        # Arms use ring topology along X, not disconnected elbow primitives.
        rings=[(.21,.092),(.30,.095),(.43,.078),(.48,.069),(.53,.065),(.64,.055),(.71,.047)]
        verts=[]; faces=[]; n=8
        for x,r in rings:
            verts.extend((s*x,r*math.sin(i*2*math.pi/n),1.38+r*math.cos(i*2*math.pi/n)) for i in range(n))
        for j in range(len(rings)-1):
            for i in range(n):
                f=(j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i)
                faces.append(f if s<0 else tuple(reversed(f)))
        faces.extend([tuple(reversed(range(n))),tuple((len(rings)-1)*n+i for i in range(n))] if s<0 else
                     [tuple(range(n)),tuple(reversed([(len(rings)-1)*n+i for i in range(n)]))])
        mesh=bpy.data.meshes.new(side+'Arm'); mesh.from_pydata(verts,[],faces); mesh.update()
        obj=bpy.data.objects.new(side+'Arm',mesh); bpy.context.collection.objects.link(obj); finish(obj,side+'Arm',mat,None)
        obj.data.materials.append(M['Skin'])
        for p in obj.data.polygons:
            if not base and abs(p.center.x)>.54: p.material_index=1
        a=obj.vertex_groups.new(name=side+'UpperArm'); b=obj.vertex_groups.new(name=side+'LowerArm')
        for v in mesh.vertices:
            w=max(0,min(1,(abs(v.co.x)-.44)/.09)); a.add([v.index],1-w,'REPLACE'); b.add([v.index],w,'REPLACE')
        ell(side+'ShoulderCap',(s*.22,0,1.38),(.096,.095,.086),mat,side+'UpperArm',8,6)
        ell(side+'Hand',(s*.765,.005,1.38),(.080,.051,.067),'Skin' if base else 'Dark',side+'Hand',10,6)
        ell(side+'Thumb',(s*.735,.050,1.35),(.04,.028,.042),'Skin' if base else 'Dark',side+'Hand',8,4)

make_body(True)
def join_skin(name):
    bpy.ops.object.select_all(action='DESELECT')
    for obj in parts: obj.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]; bpy.ops.object.join(); obj=bpy.context.object; obj.name=name
    mod=obj.modifiers.new('Humanoid skin','ARMATURE'); mod.object=rig; obj.parent=rig
    parts.clear(); return obj
base=join_skin('BaseHuman')
base_path=ROOT/'ArtSource/Characters/BaseHuman/SRC_BaseHuman.blend'; base_path.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(base_path))
bpy.data.objects.remove(base,do_unlink=True)
make_body(False)
# Vest follows the same torso weights. Open blue centre keeps work-shirt readable.
vest=tube('HiVisVest',[(0,0,1.01,.169,.12),(0,0,1.10,.175,.126),(0,0,1.26,.209,.131),(0,0,1.37,.218,.126)],'Yellow','Chest',12)
skin_rings(vest,'Spine','Chest',1.10,1.29)
box('BluePlacket',(0,.134,1.20),(.048,.018,.38),'Blue','Chest',.003)
for z,rx,ry,b in [(1.07,.176,.132,'Spine'),(1.16,.19,.137,'Chest')]:
    tube('ReflectiveWaist',[(0,0,z-.022,rx,ry),(0,0,z+.022,rx,ry)],'Silver',b,12)
for s in [-1,1]:
    for y in [-.115,.125]: box('ReflectiveShoulder',(s*.14,y,1.29),(.028,.021,.20),'Silver','Chest',.004)
    collar=box('Collar',(s*.065,.078,1.405),(.098,.08,.04),'Blue','Chest',.009)
    # Deliberate face masses, glasses, brows and a modest smile.
    ell('Ear',(s*.113,0,1.615),(.027,.026,.045),'Skin','Head',8,5)
    ell('EyeWhite',(s*.047,.096,1.64),(.036,.017,.034),'White','Head',12,6)
    ell('Pupil',(s*.047,.113,1.64),(.014,.008,.020),'Dark','Head',10,6)
    # Four bevelled frame bars leave clear open lenses.
    for dx,dz,w,h in [(0,.040,.079,.009),(0,-.036,.075,.009),(-.039,0,.009,.072),(.039,0,.009,.072)]:
        box('Glasses',(s*.049+dx,.121,1.64+dz),(w,.013,h),'Dark','Head',.004)
    box('Eyebrow',(s*.046,.098,1.697),(.070,.019,.016),'Hair','Head',.006)
    box('GlassesTemple',(s*.10,.035,1.658),(.013,.17,.013),'Dark','Head',.004)
    ell('SideHair',(s*.098,-.027,1.683),(.029,.079,.073),'Hair','Head',8,5)
box('GlassesBridge',(0,.126,1.65),(.025,.016,.010),'Dark','Head',.003)
ell('Nose',(0,.113,1.608),(.028,.035,.037),'Skin','Head',8,5)
box('Smile',(0,.103,1.563),(.055,.009,.009),'Hair','Head',.004)
ell('HairBack',(0,-.047,1.693),(.107,.071,.071),'Hair','Head',12,6)
# Helmet dome, projecting brim and a single crown ridge, no tiny fasteners.
tube('HardHat',[(0,0,1.715,.133,.119),(0,0,1.755,.132,.119),(0,0,1.802,.105,.096),(0,0,1.831,.052,.054)],'Helmet','Head',12)
ell('HelmetBrim',(0,.022,1.725),(.151,.162,.021),'Helmet','Head',16,4)
box('CrownRidge',(0,0,1.805),(.031,.18,.061),'Helmet','Head',.012)
box('HelmetBadge',(0,.115,1.77),(.043,.010,.049),'Silver','Head',.004)
tube('Belt',[(0,0,.963,.179,.121),(0,0,1.009,.179,.121)],'Dark','Hips',12)
box('Buckle',(0,.125,.986),(.061,.021,.043),'Silver','Hips',.005)
for side,s in [('Left',1),('Right',-1)]:
    box('BeltPouch',(s*.195,.040,.90),(.096,.104,.15),'Leather','Hips',.017)
    box('PouchFlap',(s*.195,.099,.94),(.092,.025,.06),'Leather','Hips',.008)
    box('ToolHandle',(s*.21,.027,1.005),(.025,.033,.10),'Helmet','Hips',.008)
    box('KneePad',(s*.115,.112,.50),(.118,.029,.143),'Dark',side+'LowerLeg',.014)
    tube('TrouserReflector',[(s*.115,0,.235,.081,.087),(s*.115,0,.283,.081,.087)],'Silver',side+'LowerLeg')
    box('BootSole',(s*.115,.069,.028),(.191,.337,.048),'Dark',side+'Foot',.018)
    box('GloveCuff',(s*.699,0,1.38),(.047,.112,.12),'Dark',side+'Hand',.009)
worker=join_skin('Electrician')
# Three minimum in-place actions, 30 fps, no animated object/root translation.
rig.animation_data_create(); bpy.context.scene.render.fps=30
for name,frames,stride in [('Idle',60,0),('Walk',30,28),('Run',20,44)]:
    action=bpy.data.actions.new(name); rig.animation_data.action=action; action.use_fake_user=True
    for frame in range(1,frames+2):
        phase=2*math.pi*(frame-1)/frames
        for p in rig.pose.bones: p.rotation_mode='XYZ'; p.rotation_euler=(0,0,0); p.location=(0,0,0)
        for side,s in [('Left',1),('Right',-1)]:
            wave=math.sin(phase)*s
            rig.pose.bones[side+'UpperLeg'].rotation_euler.x=math.radians(stride)*wave
            rig.pose.bones[side+'LowerLeg'].rotation_euler.x=-math.radians(stride*.95)*max(0,-wave)
            rig.pose.bones[side+'Foot'].rotation_euler.x=math.radians(stride*.35)*max(0,-wave)
            # Bone local Z lowers T-pose arms toward the sides.
            p=rig.pose.bones[side+'UpperArm']; rest=p.bone.matrix_local.to_quaternion()
            q=Quaternion((1,0,0),math.radians(stride*.7)*-wave) @ Quaternion((0,1,0),math.radians(78)*s)
            p.rotation_euler=(rest.inverted() @ q @ rest).to_euler()
            p=rig.pose.bones[side+'LowerArm']; rest=p.bone.matrix_local.to_quaternion()
            p.rotation_euler=(rest.inverted() @ Quaternion((0,0,1),math.radians(12 if name!='Run' else 55)*s) @ rest).to_euler()
        rig.pose.bones['Chest'].rotation_euler.y=.014*math.sin(phase)
        if name=='Run': rig.pose.bones['Chest'].rotation_euler.x=math.radians(-7)
        for p in rig.pose.bones: p.keyframe_insert('rotation_euler',frame=frame,group=p.name)
    action['in_place']=True
rig.animation_data.action=bpy.data.actions['Idle']; bpy.context.scene.frame_set(1)
bpy.context.scene.frame_end=61
source=ROOT/'ArtSource/Characters/Electrician/SRC_Electrician.blend'; source.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(source))
bpy.ops.wm.open_mainfile(filepath=str(source))
rig=bpy.data.objects['EV_Humanoid']; worker=bpy.data.objects['Electrician']
export_humanoid_fbx(ROOT/'Assets/EscapeViggo/Art/Characters/Electrician/Models/Electrician.fbx',[rig,worker])
# Validate weights, applied transforms, topology and record semantic determinism.
assert all(abs(sum(g.weight for g in v.groups)-1)<.0001 for v in worker.data.vertices)
assert all(tuple(o.scale)==(1,1,1) and o.location.length<.0001 for o in [rig,worker])
assert not worker.data.validate()
triangles=sum(len(p.vertices)-2 for p in worker.data.polygons)
def coordinates(v): return tuple(round(c,6) for c in v)
# Blender's join can reorder vertices. Compare geometry, materials and weights,
# independent of internal IDs and FBX timestamps.
payload={'vertices':sorted((coordinates(v.co),sorted((worker.vertex_groups[g.group].name,round(g.weight,6)) for g in v.groups)) for v in worker.data.vertices),
         'faces':sorted((sorted(coordinates(worker.data.vertices[i].co) for i in p.vertices),worker.data.materials[p.material_index].name) for p in worker.data.polygons),
         'bones':sorted((b.name,coordinates(b.head_local),coordinates(b.tail_local)) for b in rig.data.bones)}
report={'triangles':triangles,'bones':len(rig.data.bones),'height':round(max(v.co.z for v in worker.data.vertices),4),
        'semantic_sha256':hashlib.sha256(json.dumps(payload,sort_keys=True).encode()).hexdigest(),'actions':['Idle','Walk','Run']}
(ROOT/'Logs').mkdir(exist_ok=True); (ROOT/'Logs/C4Blender.json').write_text(json.dumps(report,indent=2))
# Neutral studio views of actual skinned geometry in relaxed idle pose.
scene=bpy.context.scene; scene.render.engine='CYCLES'; scene.cycles.samples=24
scene.render.resolution_x=900; scene.render.resolution_y=1100; scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new('ReviewWorld'); scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.14,.17,.22,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=.5
for loc,power,size in [((3,4,5),450,4),((-3,1,3),250,3),((1,-3,4),500,3)]:
    bpy.ops.object.light_add(type='AREA',location=loc); lamp=bpy.context.object; lamp.data.energy=power; lamp.data.shape='DISK'; lamp.data.size=size
    lamp.rotation_euler=(Vector((0,0,1))-lamp.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add(); camera=bpy.context.object; camera.data.type='ORTHO'; camera.data.ortho_scale=2.16; scene.camera=camera
out=ROOT/'ArtSource/Characters/Electrician/Review'; out.mkdir(parents=True,exist_ok=True)
for name,loc in [('Front',(0,5,1.1)),('Side',(5,0,1.1)),('Back',(0,-5,1.1)),('ThreeQuarter',(3.6,5,2.25))]:
    camera.location=loc; camera.rotation_euler=(Vector((0,0,.94))-camera.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=str(out/(name+'.png')); bpy.ops.render.render(write_still=True)
print('C1-C4 BLENDER PASSED',json.dumps(report))
