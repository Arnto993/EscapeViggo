"""Small static-mesh helpers; rig/animation export needs separate validation."""
from pathlib import Path
import bpy


def reset_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.scene.unit_settings.system = 'METRIC'
    bpy.context.scene.unit_settings.scale_length = 1.0
    bpy.context.preferences.filepaths.save_version = 0
    bpy.context.preferences.filepaths.file_preview_type = 'NONE'


def apply_transforms(obj):
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)


def export_static_fbx(path, objects):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.export_scene.fbx(
        filepath=str(path), check_existing=False, use_selection=True,
        object_types={'MESH'}, global_scale=1.0, apply_unit_scale=True,
        apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y',
        bake_space_transform=True, use_triangles=True, bake_anim=False,
        add_leaf_bones=False, path_mode='STRIP', use_metadata=False)
    if not path.read_bytes().startswith(b'Kaydara FBX Binary'):
        raise RuntimeError(f'Not a binary FBX: {path}')
