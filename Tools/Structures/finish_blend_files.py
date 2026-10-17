"""Make each exported object library directly openable as its own scene."""
import bpy,json
from pathlib import Path
p=Path('/home/otecglinomes/Repos/Unity/MazeHell/ArtSource/Structures')
for module in json.loads((p/'structures_manifest.json').read_text())['modules']:
 path=p/(module['name']+'.blend')
 bpy.ops.wm.read_factory_settings(use_empty=True)
 with bpy.data.libraries.load(str(path),link=False) as (src,dst):dst.objects=src.objects
 for obj in dst.objects:
  if obj and obj.name not in bpy.context.scene.objects:bpy.context.scene.collection.objects.link(obj)
 bpy.context.scene.name=module['name']
 bpy.context.scene.unit_settings.system='METRIC'
 temp=path.with_name(path.stem+'_ready.blend')
 bpy.ops.wm.save_as_mainfile(filepath=str(temp))
 temp.replace(path)
 print('SAVED_OPENABLE',module['name'],len(bpy.context.scene.objects))
# Main deliverable contains only architecture, not the source project's scenes.
main=p/'mazehell_structures.blend'
bpy.ops.wm.open_mainfile(filepath=str(main))
scene_name=json.loads((p/'structures_manifest.json').read_text()).get('sceneName')
kit=bpy.data.scenes.get(scene_name) if scene_name else next(s for s in bpy.data.scenes if s.name.startswith('MAZEHELL_Architecture_Revised'))
bpy.context.window.scene=kit
for s in list(bpy.data.scenes):
 if s!=kit:bpy.data.scenes.remove(s)
for ob in list(bpy.data.objects):
 if not ob.users_scene:bpy.data.objects.remove(ob,do_unlink=True)
for ob in kit.objects:
 if ob.name.startswith('Geo_Ceiling'):ob.hide_set(True)
from mathutils import Euler
import math
for screen in bpy.data.screens:
 for area in screen.areas:
  if area.type=='VIEW_3D':
   view=area.spaces.active;view.shading.type='MATERIAL';view.region_3d.view_distance=190;view.region_3d.view_location=(78,48,0);view.region_3d.view_rotation=Euler((math.radians(55),0,math.radians(25)),'XYZ').to_quaternion()
bpy.ops.wm.save_as_mainfile(filepath=str(main))
print('CLEAN_ARCHITECTURE_MAIN',len(kit.objects))
