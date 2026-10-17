"""Render a preview without changing the saved model or the user's live scene."""
import bpy
from pathlib import Path
from mathutils import Vector

root = Path(__file__).resolve().parents[2]
bpy.ops.wm.open_mainfile(filepath=str(root / 'ArtSource/Models/heal_orb.blend'))
scene = bpy.context.scene
scene.render.engine = 'CYCLES'
scene.cycles.samples = 24
scene.render.resolution_x = 512
scene.render.resolution_y = 512
scene.render.resolution_percentage = 100
scene.world.color = (.025, .012, .018)
bpy.ops.object.camera_add(location=(1.4, -2.2, 1.25))
camera = bpy.context.object
camera.rotation_euler = (Vector((0, 0, .62)) - camera.location).to_track_quat('-Z', 'Y').to_euler()
camera.data.type = 'ORTHO'
camera.data.ortho_scale = 1.5
scene.camera = camera
bpy.ops.object.light_add(type='AREA', location=(0, -2, 3))
bpy.context.object.data.energy = 90
bpy.context.object.data.shape = 'DISK'
bpy.context.object.data.size = 3
bpy.context.object.rotation_euler = (Vector((0, 0, .6)) - bpy.context.object.location).to_track_quat('-Z', 'Y').to_euler()
scene.render.filepath = str(root / 'ArtPreview/Items/HealCrystal.png')
bpy.ops.render.render(write_still=True)
