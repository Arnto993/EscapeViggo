"""Generate ONLY the disposable C0 fixture. No production art is touched."""
import argparse
import json
from pathlib import Path
import sys

sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
from pipeline import reset_scene, apply_transforms, export_static_fbx

parser = argparse.ArgumentParser()
parser.add_argument('--project-root', type=Path, default=Path(__file__).resolve().parents[2])
args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else [])
root = args.project_root.resolve()
if not (root / 'ProjectSettings/ProjectVersion.txt').is_file():
    raise RuntimeError(f'Not a Unity project: {root}')
if bpy.app.version[:2] != (5, 2):
    raise RuntimeError('C0 requires Blender 5.2 LTS')
reset_scene()
# Ground pivot; Blender +Y is forward. The narrow raised nose identifies +Z in Unity.
bpy.ops.mesh.primitive_cube_add(size=1)
body = bpy.context.object
body.name = 'TEST_C0_MetreMesh'
body.scale = (1.0, 0.5, 2.0)
body.location = (0, 0, 1)
apply_transforms(body)
bpy.ops.mesh.primitive_cube_add(size=1)
nose = bpy.context.object
nose.scale = (0.25, 0.5, 0.25)
nose.location = (0, 0.5, 1.625)
apply_transforms(nose)
body.select_set(True)
bpy.context.view_layer.objects.active = body
bpy.ops.object.join()
body.data.name = 'TEST_C0_MetreMesh'
material = bpy.data.materials.new('MAT_C0_Test')
material.diffuse_color = (0.12, 0.55, 0.8, 1)
material.use_nodes = True
material.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value = material.diffuse_color
material.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value = 0.8
body.data.materials.append(material)
assert tuple(body.scale) == (1, 1, 1)
assert tuple(body.location) == (0, 0, 0)
assert tuple(body.rotation_euler) == (0, 0, 0)
assert len(body.data.vertices) == 16 and len(body.data.polygons) == 12
assert not body.data.validate()
source = root / 'ArtSource/Tests/C0/TEST_C0_MetreMesh.blend'
export = root / 'Assets/EscapeViggo/Art/Tests/C0/Models/TEST_C0_MetreMesh.fbx'
source.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(source))
# Reopen saved source to prove the export is independent of GUI/transient state.
bpy.ops.wm.open_mainfile(filepath=str(source))
body = bpy.data.objects['TEST_C0_MetreMesh']
assert len(body.data.materials) == 1
export_static_fbx(export, [body])
report = {'blender': bpy.app.version_string, 'vertices': 16, 'triangles': 24,
          'unity_bounds_size': [1, 2, 1], 'unity_bounds_center': [0, 1, 0.25],
          'source': source.relative_to(root).as_posix(), 'export': export.relative_to(root).as_posix()}
(root / 'Logs').mkdir(exist_ok=True)
(root / 'Logs/C0Blender.json').write_text(json.dumps(report, indent=2) + '\n')
print('C0 Blender source/reopen/mesh/material/FBX checks passed')
