"""Apply the C4/C5 clothing palette and face-material braces to saved sources.

Preserves geometry, skinning, rig, actions and existing material identifiers.
The generators carry the same palette/brace assignments for future rebuilds.
"""
import sys, json, hashlib
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
from mathutils import Vector
from pipeline import export_humanoid_fbx

ROOT = Path(__file__).resolve().parents[2]


def structure(mesh, rig):
    data = {
        'vertices': [(tuple(v.co), [(g.group, g.weight) for g in v.groups]) for v in mesh.data.vertices],
        'faces': [tuple(p.vertices) for p in mesh.data.polygons],
        'bones': [(b.name, b.parent.name if b.parent else None,
                   [tuple(row) for row in b.matrix_local]) for b in rig.data.bones],
        'groups': [g.name for g in mesh.vertex_groups],
    }
    return hashlib.sha256(json.dumps(data).encode()).hexdigest()


def update(character, material, color):
    source = ROOT/f'ArtSource/Characters/{character}/SRC_{character}.blend'
    bpy.ops.wm.open_mainfile(filepath=str(source))
    bpy.context.preferences.filepaths.save_version = 0
    mesh, rig = bpy.data.objects[character], bpy.data.objects['EV_Humanoid']
    before = structure(mesh, rig)
    mat = bpy.data.materials[material]
    mat.diffuse_color = (*color, 1)
    mat.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = (*color, 1)
    if character == 'Viggo':
        # The first component is the original 9-ring, 16-sided jacket.
        torso = list(mesh.data.polygons)[:128]
        assert all(len(p.vertices) == 4 and
                   all(.609 < mesh.data.vertices[i].co.z < 1.301 for i in p.vertices) for p in torso)
        charcoal = next(i for i, m in enumerate(mesh.data.materials) if m.name == 'MAT_Viggo_Charcoal')
        yellow = next(i for i, m in enumerate(mesh.data.materials) if m.name == 'MAT_Viggo_Orange')
        for i, p in enumerate(torso):
            if p.material_index == charcoal: p.material_index = yellow
            if i % 16 in (2, 5, 10, 13): p.material_index = charcoal
    assert structure(mesh, rig) == before
    bpy.ops.wm.save_as_mainfile(filepath=str(source))
    bpy.ops.wm.open_mainfile(filepath=str(source))
    mesh, rig = bpy.data.objects[character], bpy.data.objects['EV_Humanoid']
    assert structure(mesh, rig) == before
    export_humanoid_fbx(ROOT/f'Assets/EscapeViggo/Art/Characters/{character}/Models/{character}.fbx', [rig, mesh])
    return before


def render_electrician():
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'; scene.cycles.samples = 24
    scene.render.resolution_x = 900; scene.render.resolution_y = 1100
    scene.render.resolution_percentage = 100
    scene.world = bpy.data.worlds.new('ReviewWorld'); scene.world.use_nodes = True
    scene.world.node_tree.nodes['Background'].inputs[0].default_value = (.14,.17,.22,1)
    scene.world.node_tree.nodes['Background'].inputs[1].default_value = .5
    for loc, power, size in [((3,4,5),450,4),((-3,1,3),250,3),((1,-3,4),500,3)]:
        bpy.ops.object.light_add(type='AREA', location=loc)
        lamp = bpy.context.object; lamp.data.energy = power; lamp.data.shape = 'DISK'; lamp.data.size = size
        lamp.rotation_euler = (Vector((0,0,1))-lamp.location).to_track_quat('-Z','Y').to_euler()
    bpy.ops.object.camera_add()
    camera = bpy.context.object; camera.data.type = 'ORTHO'; camera.data.ortho_scale = 2.16
    scene.camera = camera
    for name, loc in [('Front',(0,5,1.1)),('Side',(5,0,1.1)),('Back',(0,-5,1.1)),('ThreeQuarter',(3.6,5,2.25))]:
        camera.location = loc
        camera.rotation_euler = (Vector((0,0,.94))-camera.location).to_track_quat('-Z','Y').to_euler()
        scene.render.filepath = str(ROOT/f'ArtSource/Characters/Electrician/Review/{name}.png')
        bpy.ops.render.render(write_still=True)


if __name__ == '__main__':
    evidence = {'Electrician': update('Electrician', 'MAT_Electrician_Yellow', (.95,.205,.012))}
    render_electrician()
    evidence['Viggo'] = update('Viggo', 'MAT_Viggo_Orange', (.85,.92,.025))
    from create_viggo import render
    render()
    (ROOT/'Logs/WorkwearStructure.json').write_text(json.dumps(evidence, indent=2))
    print('WORKWEAR: geometry, weights and rig unchanged before/after saved-source update', evidence)
