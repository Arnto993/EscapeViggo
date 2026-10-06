"""C6/C7: author only Viggo actions on the saved rig; preserve mesh and weights.

Run with Tools/Invoke-Blender.ps1 -Script Tools/Blender/animate_viggo.py.
Javell is a breathing hold; the controller blends both hands onto/off the belly.
"""
import math
import sys
from pathlib import Path
import bpy
from mathutils import Matrix, Quaternion, Vector

sys.path.insert(0, str(Path(__file__).resolve().parent))
from pipeline import export_humanoid_fbx

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'ArtSource/Characters/Viggo/SRC_Viggo.blend'
CLIPS = [('Idle', 90), ('Walk', 32), ('Run', 24), ('Javell', 48)]
SIDES = [('Left', -1), ('Right', 1)]


def aim(bone, head, tail):
    """Set world pose from the rest basis, without introducing axial roll."""
    rest = bone.bone.matrix_local.to_quaternion()
    direction = rest @ Vector((0, 1, 0))
    rotation = direction.rotation_difference((tail - head).normalized()) @ rest
    bone.matrix = Matrix.LocRotScale(head, rotation, Vector((1, 1, 1)))
    bpy.context.view_layer.update()


def limb(rig, upper, lower, target, pole):
    a, b = rig.pose.bones[upper], rig.pose.bones[lower]
    start = a.head.copy()
    delta = target - start
    distance = min(delta.length, a.length + b.length - .0001)
    axis = delta.normalized()
    bend = pole - start
    bend = (bend - axis * bend.dot(axis)).normalized()
    along = (a.length ** 2 - b.length ** 2 + distance ** 2) / (2 * distance)
    elbow = start + axis * along + bend * math.sqrt(max(0, a.length ** 2 - along ** 2))
    aim(a, start, elbow)
    aim(b, elbow, start + axis * distance)


def author():
    bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
    rig = bpy.data.objects['EV_Humanoid']
    rig.animation_data_clear()
    for name, _ in CLIPS:
        if name in bpy.data.actions:
            bpy.data.actions.remove(bpy.data.actions[name])
    rig.animation_data_create()
    bpy.context.scene.render.fps = 30
    for name, frames in CLIPS:
        action = bpy.data.actions.new(name)
        action.use_fake_user = True
        rig.animation_data.action = action
        for frame in range(1, frames + 2):
            phase = 2 * math.pi * (frame - 1) / frames
            breath = math.sin(phase)
            moving = name in ('Walk', 'Run')
            running = name == 'Run'
            resting = name == 'Javell'
            for p in rig.pose.bones:
                p.rotation_mode = 'QUATERNION'
                p.rotation_quaternion = Quaternion()
                p.location = (0, 0, 0)
                p.scale = (1, 1, 1)
            # Small weight transfer, a low centre of mass and restrained vertical impact.
            hips = rig.pose.bones['Hips']
            hips.location.y = (-.045 if running else -.030) if moving else -.008
            if moving:
                hips.location.y += .008 * math.cos(2 * phase)
                hips.rotation_quaternion = Quaternion((0, 0, 1), .018 * breath)
            chest = rig.pose.bones['Chest']
            lean = 9 if resting else 7 if running else 3
            chest.rotation_quaternion = Quaternion((1, 0, 0), math.radians(-lean - (2.2 if resting else .7) * breath))
            rig.pose.bones['Head'].rotation_quaternion = Quaternion((1, 0, 0), math.radians(-2 if resting else 1))
            rig.pose.bones['Belly'].scale = (1 + .008 * breath, 1 + .012 * breath, 1 + .008 * breath)
            bpy.context.view_layer.update()
            for side, sign in SIDES:
                wave = math.sin(phase + (math.pi if sign > 0 else 0))
                lift = max(0, math.cos(phase + (math.pi if sign > 0 else 0)))
                foot = rig.pose.bones[side + 'Foot']
                target = Vector((sign * .205, (.19 if running else .145) * wave if moving else 0,
                                 .12 + ((.065 if running else .035) * lift if moving else 0)))
                limb(rig, side + 'UpperLeg', side + 'LowerLeg', target, Vector((sign * .205, 1, .35)))
                foot.matrix = Matrix.Translation(foot.head) @ foot.bone.matrix_local.to_quaternion().to_matrix().to_4x4()
                bpy.context.view_layer.update()
                if resting:
                    # Wrists on the upper belly, gloves extending over its front surface.
                    target = Vector((sign * .245, .365 + .006 * breath, 1.045 + .005 * breath))
                    limb(rig, side + 'UpperArm', side + 'LowerArm', target, Vector((sign * .85, .12, .98)))
                    hand = rig.pose.bones[side + 'Hand']
                    aim(hand, hand.head.copy(), hand.head + Vector((-sign * .025, .075, -.07)))
                else:
                    arm = rig.pose.bones[side + 'UpperArm']
                    rest = arm.bone.matrix_local.to_quaternion()
                    swing = math.radians(17 if running else 10) * -wave if moving else .01 * breath
                    q = Quaternion((1, 0, 0), swing) @ Quaternion((0, 1, 0), math.radians(72) * sign)
                    arm.rotation_quaternion = rest.inverted() @ q @ rest
                    forearm = rig.pose.bones[side + 'LowerArm']
                    rest = forearm.bone.matrix_local.to_quaternion()
                    forearm.rotation_quaternion = rest.inverted() @ Quaternion((0, 0, 1), math.radians(38 if running else 16) * sign) @ rest
            for p in rig.pose.bones:
                p.keyframe_insert('rotation_quaternion', frame=frame, group=p.name)
                p.keyframe_insert('location', frame=frame, group=p.name)
                p.keyframe_insert('scale', frame=frame, group=p.name)
        action['in_place'] = True
    # Export an unposed reference skeleton, independent of any clip's first frame.
    rig.animation_data.action = None
    for p in rig.pose.bones:
        p.matrix_basis = Matrix.Identity(4)
    bpy.context.view_layer.update()
    bpy.context.scene.frame_start = 1
    bpy.context.scene.frame_end = 91
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE))
    export_humanoid_fbx(ROOT / 'Assets/EscapeViggo/Art/Characters/Viggo/Models/Viggo.fbx',
                        [rig, bpy.data.objects['Viggo']])


if __name__ == '__main__':
    author()
