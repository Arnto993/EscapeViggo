"""C5 Viggo base model. Reuses the corrected BaseHuman hierarchy, not its mesh.

No animation is authored here. Review poses use the existing shared Idle action.
Run export_character.py --character Viggo to preserve subsequent manual edits.
"""
import sys, math
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
from mathutils import Vector
from pipeline import apply_transforms, export_humanoid_fbx

ROOT = Path(__file__).resolve().parents[2]
SIDES = [('Left', -1), ('Right', 1)]
SOURCE = ROOT/'ArtSource/Characters/Viggo/SRC_Viggo.blend'
REVIEW = SOURCE.parent/'Review'


def build():
    bpy.ops.wm.open_mainfile(filepath=str(ROOT/'ArtSource/Characters/BaseHuman/SRC_BaseHuman.blend'))
    bpy.context.preferences.filepaths.save_version = 0
    rig = bpy.data.objects['EV_Humanoid']
    for obj in list(bpy.data.objects):
        if obj != rig: bpy.data.objects.remove(obj, do_unlink=True)
    rig.animation_data_clear()
    for action in list(bpy.data.actions): bpy.data.actions.remove(action)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.object.mode_set(mode='EDIT')
    def bone(name, head, tail):
        b = rig.data.edit_bones[name]
        b.head, b.tail = head, tail
        if name.endswith(('UpperLeg', 'LowerLeg')): b.align_roll(Vector((0, 1, 0)))
        elif name.endswith(('Foot', 'Toes')): b.align_roll(Vector((0, 0, 1)))
    bone('Hips', (0,0,.64), (0,0,.79))
    bone('Spine', (0,0,.79), (0,0,1.02))
    bone('Chest', (0,0,1.02), (0,0,1.27))
    bone('Neck', (0,0,1.27), (0,0,1.36))
    bone('Head', (0,0,1.36), (0,0,1.61))
    for side, s in SIDES:
        bone(side+'Shoulder', (s*.055,0,1.23), (s*.32,0,1.23))
        bone(side+'UpperArm', (s*.32,0,1.23), (s*.57,0,1.23))
        bone(side+'LowerArm', (s*.57,0,1.23), (s*.79,0,1.23))
        bone(side+'Hand', (s*.79,0,1.23), (s*.91,0,1.23))
        bone(side+'UpperLeg', (s*.205,0,.66), (s*.205,.014,.365))
        bone(side+'LowerLeg', (s*.205,.014,.365), (s*.205,0,.12))
        bone(side+'Foot', (s*.205,0,.12), (s*.205,.205,.065))
        bone(side+'Toes', (s*.205,.205,.065), (s*.205,.285,.065))
    belly = rig.data.edit_bones.new('Belly')
    belly.head, belly.tail = (0,.16,.88), (0,.40,.88)
    belly.parent = rig.data.edit_bones['Spine']
    bpy.ops.object.mode_set(mode='OBJECT')
    materials = {}
    for name, rgb in {'Orange':(.95,.205,.012), 'Charcoal':(.038,.042,.048),
                      'Rubber':(.016,.019,.024), 'Reflector':(.66,.70,.70),
                      'Skin':(.66,.335,.215), 'Flush':(.70,.225,.145),
                      'GreyHair':(.28,.29,.285), 'Beard':(.42,.43,.405),
                      'Brow':(.16,.17,.16), 'Eye':(.86,.83,.72),
                      'Iris':(.12,.07,.033)}.items():
        m = bpy.data.materials.new('MAT_Viggo_'+name)
        m.diffuse_color = (*rgb,1); m.use_nodes = True
        m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = (*rgb,1)
        m.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value = .8
        materials[name] = m
    parts = []
    def finish(obj, name, mat, weights):
        obj.name = name; apply_transforms(obj); obj.data.materials.append(materials[mat])
        for name, weight in weights.items():
            obj.vertex_groups.new(name=name).add(list(range(len(obj.data.vertices))), weight, 'REPLACE')
        parts.append(obj)
        return obj
    def ell(name, loc, scale, mat, owner='Head', segments=12, rings=6):
        bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, radius=1, location=loc)
        obj = bpy.context.object; obj.scale = scale
        return finish(obj, name, mat, {owner:1})
    def box(name, loc, scale, mat, owner, bevel=.012, tilt=0):
        bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
        obj = bpy.context.object; obj.scale = scale; obj.rotation_euler.y = tilt; apply_transforms(obj)
        mod = obj.modifiers.new('Silhouette bevel', 'BEVEL'); mod.width = bevel; mod.segments = 1
        bpy.ops.object.modifier_apply(modifier=mod.name)
        return finish(obj, name, mat, {owner:1})
    def tube(name, sections, mat, owner, n=16):
        verts = [(x+rx*math.cos(i*2*math.pi/n), y+ry*math.sin(i*2*math.pi/n), z)
                 for x,y,z,rx,ry in sections for i in range(n)]
        faces = [(j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i)
                 for j in range(len(sections)-1) for i in range(n)]
        faces += [tuple(reversed(range(n))), tuple((len(sections)-1)*n+i for i in range(n))]
        mesh = bpy.data.meshes.new(name); mesh.from_pydata(verts, [], faces); mesh.update()
        obj = bpy.data.objects.new(name, mesh); bpy.context.collection.objects.link(obj)
        return finish(obj, name, mat, {owner:1})
    def weights(obj, fn):
        obj.vertex_groups.clear()
        for v in obj.data.vertices:
            for name, weight in fn(v.co).items():
                if weight > 0:
                    group = obj.vertex_groups.get(name) or obj.vertex_groups.new(name=name)
                    group.add([v.index], weight, 'REPLACE')
    def torso_weights(v):
        chest = max(0, min(1, (v.z-.99)/.22))
        hips = max(0, min(.65, (.78-v.z)/.18))
        belly = max(0, min(.75, (v.y+.02)/.46))*(1-chest)*(1-hips)
        return {'Hips':hips, 'Chest':chest, 'Belly':belly, 'Spine':1-hips-chest-belly}
    # One shaped jacket surface: the lower front swells well beyond the chest.
    sections = [(0,.04,.61,.285,.23), (0,.10,.68,.35,.31),
                (0,.14,.80,.405,.37), (0,.15,.855,.414,.38),
                (0,.13,.96,.40,.365), (0,.075,1.075,.36,.29),
                (0,.015,1.18,.325,.225), (0,0,1.27,.265,.18),
                (0,0,1.30,.125,.11)]
    torso = tube('Round work jacket', sections, 'Orange', 'Spine')
    weights(torso, torso_weights)
    torso.data.materials.append(materials['Reflector'])
    torso.data.materials.append(materials['Charcoal'])
    for p in torso.data.polygons:
        if .80 < p.center.z < .855: p.material_index = 1
        elif p.center.z > 1.1 and p.center.y > .14 and abs(p.center.x) < .13: p.material_index = 2
    hips = ell('Trouser seat', (0,-.005,.62), (.325,.255,.18), 'Charcoal', 'Hips',16,6)
    belt = tube('Belt', [(0,.045,.618,.294,.239),(0,.064,.66,.32,.263)],'Rubber','Hips')
    box('Belt buckle', (0,.335,.65), (.082,.026,.049),'Reflector','Hips',.006)
    ell('Neck',(0,0,1.32),(.112,.11,.105),'Skin','Neck')
    tube('Collar',[(0,0,1.275,.16,.14),(0,-.016,1.32,.14,.12)],'Charcoal','Chest',12)
    for side,s in SIDES:
        shoulder=ell(side+' shoulder',(s*.305,0,1.235),(.133,.145,.115),'Orange',side+'UpperArm',12,6)
        weights(shoulder,lambda v: {side+'UpperArm':.8,'Chest':.2})
        leg = tube(side+' trouser leg', [(s*.205,.04,.13,.108,.115),
                   (s*.205,0,.24,.112,.12),(s*.205,.014,.335,.12,.13),
                   (s*.205,.014,.395,.136,.14),(s*.205,0,.53,.147,.16),
                   (s*.205,0,.68,.15,.17)],'Charcoal',side+'UpperLeg',12)
        def leg_weights(v):
            w = max(0,min(1,(v.z-.32)/.10))
            return {side+'UpperLeg':w,side+'LowerLeg':1-w}
        weights(leg,leg_weights)
        tube(side+' trouser reflector',[(s*.205,0,.235,.115,.123),(s*.205,0,.27,.117,.127)],
             'Reflector',side+'LowerLeg',12)
        ell(side+' boot',(s*.205,.083,.092),(.124,.223,.092),'Rubber',side+'Foot',12,6)
        box(side+' sole',(s*.205,.085,.025),(.246,.421,.05),'Rubber',side+'Foot',.018)
        box(side+' toe cap',(s*.205,.22,.10),(.221,.139,.095),'Charcoal',side+'Foot',.027)
        # The established ring method along X supplies blended elbow deformation.
        rings = [(.29,.133),(.38,.145),(.47,.134),(.52,.122),(.565,.107),(.61,.10),(.70,.09),(.79,.073)]
        n = 12
        verts = [(s*x,r*math.sin(i*2*math.pi/n),1.23+r*math.cos(i*2*math.pi/n)) for x,r in rings for i in range(n)]
        faces = []
        for j in range(len(rings)-1):
            for i in range(n):
                f = (j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i)
                faces.append(f if s<0 else tuple(reversed(f)))
        caps = [tuple(reversed(range(n))),tuple((len(rings)-1)*n+i for i in range(n))]
        faces += caps if s<0 else [tuple(reversed(f)) for f in caps]
        mesh=bpy.data.meshes.new(side+' arm'); mesh.from_pydata(verts,[],faces); mesh.update()
        arm=bpy.data.objects.new(side+' arm',mesh); bpy.context.collection.objects.link(arm)
        finish(arm,side+' arm','Orange',{})
        arm.data.materials.append(materials['Skin']); arm.data.materials.append(materials['Reflector'])
        for p in mesh.polygons:
            if abs(p.center.x)>.52: p.material_index=1
            elif abs(p.center.x)>.47: p.material_index=2
        def arm_weights(v):
            w=max(0,min(1,(abs(v.x)-.52)/.10))
            return {side+'UpperArm':1-w, side+'LowerArm':w}
        weights(arm,arm_weights)
        ell(side+' glove',(s*.866,.008,1.23),(.106,.082,.096),'Rubber',side+'Hand',12,6)
        ell(side+' thumb',(s*.828,.072,1.186),(.052,.043,.064),'Rubber',side+'Hand',8,6)
        box(side+' cuff',(s*.787,0,1.23),(.05,.161,.169),'Charcoal',side+'Hand',.012)
    # Broad, older face. Hair/beard are simple masses, with a bare receding forehead.
    ell('Head',(0,-.012,1.475),(.166,.144,.166),'Skin',segments=16,rings=8)
    ell('Beard jaw',(0,.035,1.38),(.165,.152,.108),'Beard',segments=16,rings=6)
    ell('Chin beard',(0,.12,1.347),(.116,.071,.068),'Beard',segments=12,rings=5)
    ell('Nose bridge',(0,.129,1.472),(.041,.044,.067),'Skin',segments=8,rings=6)
    ell('Large flushed nose',(0,.184,1.444),(.061,.064,.044),'Flush',segments=12,rings=6)
    for side,s in SIDES:
        ell('Ear',(s*.169,-.004,1.471),(.04,.036,.067),'Skin',segments=8,rings=6)
        ell('Ear inset',(s*.187,.022,1.474),(.017,.009,.035),'Flush',segments=8,rings=4)
        ell('Cheek',(s*.102,.101,1.429),(.07,.046,.054),'Skin',segments=10,rings=6)
        ell('Tired lower eyelid',(s*.070,.134,1.486),(.052,.022,.021),'Flush',segments=10,rings=4)
        ell('Eye',(s*.069,.143,1.502),(.045,.019,.021),'Eye',segments=12,rings=6)
        ell('Heavy upper eyelid',(s*.070,.143,1.519),(.046,.020,.012),'Skin',segments=10,rings=4)
        ell('Iris',(s*.063,.161,1.500),(.014,.006,.015),'Iris',segments=10,rings=6)
        ell('Pupil',(s*.063,.166,1.500),(.007,.003,.010),'Rubber',segments=8,rings=4)
        box('Heavy brow',(s*.070,.139,1.531),(.108,.035,.027),'Brow','Head',.008,tilt=s*.13)
        ell('Side beard',(s*.127,.042,1.402),(.048,.090,.084),'Beard',segments=8,rings=6)
        ell('Moustache',(s*.042,.153,1.405),(.055,.026,.023),'Beard',segments=8,rings=4)
        box('Downturned mouth',(s*.032,.179,1.400),(.069,.014,.009),'Brow','Head',.004,tilt=s*.19)
        ell('Temple hair',(s*.140,-.043,1.524),(.037,.091,.079),'GreyHair',segments=10,rings=6)
    ell('Back hair',(0,-.094,1.527),(.143,.065,.093),'GreyHair',segments=12,rings=6)
    # A few broad swept locks, deliberately no strands or high-frequency detail.
    for i,(x,y,z,sx,sy,sz) in enumerate([(-.11,-.015,1.604,.071,.066,.034),
          (-.05,-.037,1.62,.062,.07,.028),(.012,-.035,1.63,.047,.058,.030),
          (.070,-.044,1.614,.061,.07,.032),(.123,-.032,1.588,.051,.07,.031)]):
        ell('Swept hair '+str(i),(x,y,z),(sx,sy,sz),'GreyHair',segments=7,rings=4)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in parts: obj.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]; bpy.ops.object.join()
    mesh=bpy.context.object; mesh.name='Viggo'
    mod=mesh.modifiers.new('Humanoid skin','ARMATURE'); mod.object=rig; mesh.parent=rig
    SOURCE.parent.mkdir(parents=True,exist_ok=True)
    bpy.context.scene.render.fps=30
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE))
    bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
    export_humanoid_fbx(ROOT/'Assets/EscapeViggo/Art/Characters/Viggo/Models/Viggo.fbx',
                        [bpy.data.objects['EV_Humanoid'],bpy.data.objects['Viggo']])


def render():
    rig=bpy.data.objects['EV_Humanoid']
    # Preview only: never save this action or relaxed pose into the production source.
    electrician=ROOT/'ArtSource/Characters/Electrician/SRC_Electrician.blend'
    with bpy.data.libraries.load(str(electrician),link=False) as (src,dst): dst.actions=['Idle']
    rig.animation_data_create(); rig.animation_data.action=dst.actions[0]
    rig.animation_data.action_slot=dst.actions[0].slots[0]
    for p in rig.pose.bones: p.rotation_mode='XYZ'
    bpy.context.scene.frame_set(1)
    scene=bpy.context.scene; scene.render.engine='CYCLES'; scene.cycles.samples=24
    scene.render.resolution_x=900; scene.render.resolution_y=1100; scene.render.resolution_percentage=100
    scene.world=bpy.data.worlds.new('ReviewWorld'); scene.world.use_nodes=True
    scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.17,.18,.20,1)
    scene.world.node_tree.nodes['Background'].inputs[1].default_value=.6
    for loc,power,size in [((3,4,5),450,4),((-3,1,3),250,3),((1,-3,4),500,3)]:
        bpy.ops.object.light_add(type='AREA',location=loc); lamp=bpy.context.object
        lamp.data.energy=power; lamp.data.shape='DISK'; lamp.data.size=size
        lamp.rotation_euler=(Vector((0,0,1))-lamp.location).to_track_quat('-Z','Y').to_euler()
    bpy.ops.object.camera_add(); camera=bpy.context.object; camera.data.type='ORTHO'; scene.camera=camera
    REVIEW.mkdir(parents=True,exist_ok=True)
    for name,loc,target,scale in [('Front',(0,5,.9),(0,0,.83),1.97),
          ('Side',(5,0,.9),(0,0,.83),1.97),('Back',(0,-5,.9),(0,0,.83),1.97),
          ('ThreeQuarter',(3.6,5,2.0),(0,.08,.83),1.97),
          ('FaceCloseup',(1.1,5,1.75),(0,.02,1.46),.53)]:
        camera.location=loc; camera.data.ortho_scale=scale
        camera.rotation_euler=(Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler()
        scene.render.filepath=str(REVIEW/(name+'.png')); bpy.ops.render.render(write_still=True)
    with bpy.data.libraries.load(str(electrician),link=False) as (src,dst):
        dst.objects=['EV_Humanoid','Electrician']
    for obj in dst.objects: scene.collection.objects.link(obj)
    other=next(o for o in dst.objects if o.type=='ARMATURE')
    other.location.x=-.70; rig.location.x=.65
    scene.frame_set(1); scene.render.resolution_x=1400; scene.render.resolution_y=1100
    camera.location=(0,5,1); camera.data.ortho_scale=2.75
    camera.rotation_euler=(Vector((0,0,.94))-camera.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=str(REVIEW/'ViggoVsElectrician.png'); bpy.ops.render.render(write_still=True)


if __name__ == '__main__':
    build()
    if '--no-render' not in sys.argv: render()
