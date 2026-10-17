"""Build the original MazeHell healing pickup in an isolated Blender process."""
import bpy
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'ArtSource/Models'
OUT.mkdir(parents=True, exist_ok=True)
UNITY_OUT = ROOT / 'Assets/MazeHell/Resources/Models'
UNITY_OUT.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)

def material(name, color, glow=0, metal=0):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    shader = next(n for n in mat.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
    shader.inputs['Base Color'].default_value = (*color, 1)
    shader.inputs['Metallic'].default_value = metal
    shader.inputs['Roughness'].default_value = .3
    shader.inputs['Emission Color'].default_value = (*color, 1)
    shader.inputs['Emission Strength'].default_value = glow
    return mat

amber = material('Heal_Amber_Emission', (1, .26, .025), 2.5)
ivory = material('Heal_Ivory_Core', (1, .78, .38), 4)
dark = material('Heal_Oxblood_Facet', (.22, .018, .012), .15)
gold = material('Heal_Antique_Brass', (.42, .19, .045), .15, .8)
root = bpy.data.objects.new('HealOrb', None)
bpy.context.collection.objects.link(root)
root['purpose'] = 'MazeHell floating healing crystal. Height 0.8m. Amber emissive facets.'

# Hand-shaped six-sided crystal with a long lower point and a split crown.
verts = [(0, 0, -.42)]
for z, radius in [(-.12, .22), (.16, .20)]:
    verts += [(radius * math.cos(i*math.tau/6), radius*math.sin(i*math.tau/6), z) for i in range(6)]
verts.append((.035, -.015, .39))
faces = []
for i in range(6):
    j = (i + 1) % 6
    faces.extend([(0, 1+j, 1+i), (1+i, 1+j, 7+j), (1+i, 7+j, 7+i), (7+i, 7+j, 13)])
mesh = bpy.data.meshes.new('HealCrystal_Faceted')
mesh.from_pydata(verts, [], faces)
mesh.update()
crystal = bpy.data.objects.new('HealCrystal', mesh)
bpy.context.collection.objects.link(crystal)
crystal.parent = root
for mat in [amber, ivory, dark]:
    crystal.data.materials.append(mat)
for poly in mesh.polygons:
    poly.material_index = 1 if poly.index % 7 == 0 else (2 if poly.index % 5 == 0 else 0)

for z, radius in [(-.10, .245), (.14, .225)]:
    bpy.ops.mesh.primitive_torus_add(major_segments=6, minor_segments=4, location=(0, 0, z), major_radius=radius, minor_radius=.018)
    ring = bpy.context.object
    ring.name = 'Brass_Crystal_Collar'
    ring.data.materials.append(gold)
    ring.parent = root

# Three smaller floating shards communicate a living magical pickup.
for i in range(3):
    angle = i * math.tau / 3
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=.08, location=(.34*math.cos(angle), .34*math.sin(angle), .035))
    shard = bpy.context.object
    shard.name = f'Heal_Satellite_{i+1}'
    shard.scale = (.55, .55, 1.8)
    shard.data.materials.append(ivory)
    shard.parent = root

scene = bpy.context.scene
scene.render.fps = 24
scene.frame_start = 1
scene.frame_end = 49
for frame, z, turn in [(1, .62, 0), (13, .69, math.pi/2), (25, .62, math.pi), (37, .55, 3*math.pi/2), (49, .62, math.tau)]:
    root.location.z = z
    root.rotation_euler.z = turn
    root.keyframe_insert(data_path='location', frame=frame)
    root.keyframe_insert(data_path='rotation_euler', frame=frame)
if root.animation_data and root.animation_data.action:
    root.animation_data.action.name = 'HealOrb_Hover_Loop'
scene.frame_set(1)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT / 'heal_orb.blend'))
bpy.ops.export_scene.fbx(filepath=str(UNITY_OUT / 'heal_orb.fbx'), use_selection=False, object_types={'EMPTY', 'MESH'}, apply_unit_scale=True, bake_anim=True, bake_anim_use_all_actions=False, bake_anim_use_nla_strips=False, add_leaf_bones=False)
bpy.ops.export_scene.gltf(filepath=str(OUT / 'heal_orb.glb'), export_format='GLB', export_animations=True)
print('MAZEHELL_HEAL_ORB_OK', len(mesh.polygons), 'crystal faces; files saved to', OUT)
