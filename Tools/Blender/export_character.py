"""Export saved editable C4 source, without regenerating geometry or animation."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
import bpy
from pipeline import export_humanoid_fbx
root=Path(__file__).resolve().parents[2]
bpy.ops.wm.open_mainfile(filepath=str(root/'ArtSource/Characters/Electrician/SRC_Electrician.blend'))
export_humanoid_fbx(root/'Assets/EscapeViggo/Art/Characters/Electrician/Models/Electrician.fbx',
                    [bpy.data.objects['EV_Humanoid'],bpy.data.objects['Electrician']])
