"""Export saved editable C4 source, without regenerating geometry or animation."""
import sys, argparse
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
import bpy
from pipeline import export_humanoid_fbx
root=Path(__file__).resolve().parents[2]
parser=argparse.ArgumentParser()
parser.add_argument('--character', choices=['Electrician','Viggo'], default='Electrician')
character=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []).character
bpy.ops.wm.open_mainfile(filepath=str(root/f'ArtSource/Characters/{character}/SRC_{character}.blend'))
export_humanoid_fbx(root/f'Assets/EscapeViggo/Art/Characters/{character}/Models/{character}.fbx',
                    [bpy.data.objects['EV_Humanoid'],bpy.data.objects[character]])
