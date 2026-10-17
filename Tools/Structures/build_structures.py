"""Run through Blender MCP: original box-only modular kit and interchange manifest."""
import bpy, json, math
from pathlib import Path
ROOT=Path('/home/otecglinomes/Repos/Unity/MazeHell')
OUT=ROOT/'ArtSource/Structures'; UNITY=ROOT/'Assets/MazeHell/Resources/Models/Structures'
UNITY.mkdir(parents=True,exist_ok=True)
previous_scene=bpy.context.window.scene
scene=bpy.data.scenes.new('MAZEHELL_Architecture_Revised')
bpy.context.window.scene=scene
roots={}
scene.unit_settings.system='METRIC'
scene.unit_settings.scale_length=1
M={}
for name,rel,col,glow in [
 ('MH_Wall_Hellstone','Walls/Hellstone/wall_base.png',(1,1,1),0),
 ('MH_Wall_Bones','Walls/Bones/wall_bones.png',(1,1,1),0),
 ('MH_Wall_Lava','Walls/Lava/wall_lava.png',(1,1,1),.6),
 ('MH_Floor_Cracked','Floor/CrackedStone/floor_cracked.png',(1,1,1),0),
 ('MH_Wood_AshGray','Wood/AshGray/wood_ash_gray.png',(.16,.15,.14),0),
 ('MH_Ceiling_Basalt','Floor/CrackedStone/floor_cracked.png',(.35,.38,.43),0),
 ('MH_Column_Stone','Walls/Hellstone/wall_base.png',(.28,.24,.23),0),
 ('MH_Column_Band','Walls/Hellstone/wall_base.png',(.12,.10,.09),0),
 ('MH_Metal_Dark',None,(.06,.07,.08),0),('MH_Ivory_Flat',None,(.72,.64,.46),0)]:
 m=bpy.data.materials.new(name+'_Revision');m.use_nodes=True;m.diffuse_color=(*col,1)
 n=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
 n.inputs['Base Color'].default_value=(*col,1); n.inputs['Roughness'].default_value=.88
 if rel:
  tex=m.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(ROOT/'ArtSource/Generated/World'/rel),check_existing=True);tex.interpolation='Closest'
  tint=m.node_tree.nodes.new('ShaderNodeMixRGB');tint.blend_type='MULTIPLY';tint.inputs[0].default_value=1;tint.inputs[2].default_value=(*col,1)
  m.node_tree.links.new(tex.outputs['Color'],tint.inputs[1]);m.node_tree.links.new(tint.outputs[0],n.inputs['Base Color'])
  if glow:m.node_tree.links.new(tex.outputs['Color'],n.inputs['Emission Color']);n.inputs['Emission Strength'].default_value=glow
 M[name]=m
manifest={'modules':[],'units':'meters','slotConvention':'outer mating plane; Blender +Y outward; Unity +Z outward','tileMeters':4}
current=None;entry=None

def start(name,kind='room'):
 global current,entry
 current=bpy.data.objects.new(name,None);scene.collection.objects.link(current)
 roots[name]=current
 current['purpose']='MazeHell static architecture; flat floor; separate wooden doors; no gameplay or decor'
 entry={'name':name,'kind':kind,'boxes':[],'slots':[]};manifest['modules'].append(entry)
 return current

def box(name,a,b,c,d,e=0,f=8,mat='MH_Wall_Hellstone'):
 # Explicit face-separated box ensures hard normals and metric box UVs.
 vs=[(a,c,e),(b,c,e),(b,d,e),(a,d,e),(a,c,f),(b,c,f),(b,d,f),(a,d,f)]
 faces=[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(vs,[],faces);mesh.update()
 uv=mesh.uv_layers.new(name='UVMap')
 for poly in mesh.polygons:
  axis=max(range(3),key=lambda i:abs(poly.normal[i]));axes=[i for i in range(3) if i!=axis]
  for li in poly.loop_indices:
   v=mesh.vertices[mesh.loops[li].vertex_index].co
   uv.data[li].uv=(v[axes[0]]/4,v[axes[1]]/4)
 obj=bpy.data.objects.new(name,mesh);scene.collection.objects.link(obj);obj.parent=current;mesh.materials.append(M[mat])
 entry['boxes'].append({'name':name,'min':[a,c,e],'max':[b,d,f],'material':mat})
 return obj

def slot(name,x,y,angle=0,width=6,height=4.5,door='door_wood'):
 obj=bpy.data.objects.new(name,None);scene.collection.objects.link(obj);obj.parent=current;obj.location=(x,y,0);obj.rotation_euler.z=math.radians(angle)
 obj.empty_display_size=.7; obj['width']=width;obj['height']=height;obj['door']=door
 entry['slots'].append({'name':name,'position':[x,y,0],'angle':angle,'width':width,'height':height,'door':door})

def column(x,y,i):
 box(f'Column_{i}_Base',x-1,x+1,y-1,y+1,0,.5,'MH_Column_Band')
 box(f'Column_{i}_Shaft',x-.65,x+.65,y-.65,y+.65,.5,7.5,'MH_Column_Stone')
 box(f'Column_{i}_Capital',x-1,x+1,y-1,y+1,7.5,8,'MH_Column_Band')

def slab(r):
 box('Geo_Floor',-r-.5,r+.5,-r-.5,r+.5,-.2,0,'MH_Floor_Cracked')
 box('Geo_Ceiling',-r-.5,r+.5,-r-.5,r+.5,8,8.2,'MH_Ceiling_Basalt')

def wall(side,r,width=6,h=4.5,gate=False,closed=False):
 def w(n,u,v,z0=0,z1=8,material='MH_Wall_Hellstone'):
  if side=='N':return box(n,u,v,r,r+.5,z0,z1,material)
  if side=='S':return box(n,u,v,-r-.5,-r,z0,z1,material)
  if side=='E':return box(n,r,r+.5,u,v,z0,z1,material)
  return box(n,-r-.5,-r,u,v,z0,z1,material)
 extent=r+.5 if side in ['N','S'] else r
 if closed:w('Geo_Wall_'+side,-extent,extent);return
 w('Geo_Wall_'+side+'_L',-extent,-width/2);w('Geo_Wall_'+side+'_R',width/2,extent)
 if gate:
  w('Geo_Gate_Header',-5,5,7,8)
  for sx in [-1,1]:
   w('Geo_Gate_Shoulder_Outer',*sorted([sx*4,sx*5]),5.5,7)
   w('Geo_Gate_Shoulder_Inner',*sorted([sx*3,sx*4]),6.25,7)
   # Monumental frame follows the stepped opening. It touches the wall's interior face.
   for n,a,b,z0,z1 in [('Pier',5,6.5,0,5.5),('StepLow',4,6.5,5.5,5.9),('RiserLow',4,4.4,5.9,6.25),('StepHigh',3,4.4,6.25,6.65),('RiserHigh',3,3.4,6.65,7)]:
    box('Geo_Gate_Frame_'+n,*sorted([sx*a,sx*b]),r-.8,r,z0,z1,'MH_Column_Stone')
  box('Geo_Gate_Frame_Crown',-3.4,3.4,r-.8,r,7,7.4,'MH_Column_Stone')
 else:
  w('Geo_Wall_'+side+'_Lintel',-3,3,4.5,8)
  for j,(rw,z0,z1) in enumerate([(6,4.5,5),(4.5,5,5.5),(3,5.5,6)]):
   n='Geo_Arch_'+side+'_'+str(j)
   if side=='N':box(n,-rw/2,rw/2,r-.12,r,z0,z1)
   if side=='S':box(n,-rw/2,rw/2,-r,-r+.12,z0,z1)
   if side=='E':box(n,r-.12,r,-rw/2,rw/2,z0,z1)
   if side=='W':box(n,-r,-r+.12,-rw/2,rw/2,z0,z1)
 x,y,ang={'N':(0,r+.5,0),'S':(0,-r-.5,180),'W':(-r-.5,0,90),'E':(r+.5,0,-90)}[side]
 slot('Gate_N' if gate else 'Door_'+side,x,y,ang,width,7 if gate else h,'gate_big' if gate else 'door_wood')

def room(name,r=20,gate=False):
 start(name);slab(r)
 for side in 'NSWE':wall(side,r,10 if gate and side=='N' else 6,4.5,gate and side=='N',gate and side in 'WE')

def mirror(rects,prefix):
 for sx in [-1,1]:
  for sy in [-1,1]:
   for i,(a,b,c,d) in enumerate(rects):box(f'Geo_Inner_{prefix}_{sx}_{sy}_{i}',*sorted([sx*a,sx*b]),*sorted([sy*c,sy*d]),mat='MH_Wall_Bones')

room('room_quarters')
# Original small subrooms stay 7.5 x 8m at the corners; central cross widens to 24m.
mirror([(-20,-12,11.5,12),(-12.5,-12,12,14.5),(-12.5,-12,17.5,20)],'Sub')
for sx in [-1,1]:
 for sy in [-1,1]:
  box('Geo_Sub_Lintel',*sorted([sx*12,sx*12.5]),*sorted([sy*14.5,sy*17.5]),3.5,8,'MH_Wall_Bones')
  slot(f'DoorSub_{sx}_{sy}',sx*12.25,sy*16,90 if sx==-1 else -90,3,3.5,'door_wood_small')
for i,(x,y) in enumerate([(x,y) for x in [-5,5] for y in [-5,5]]):column(x,y,i)
room('room_ring')
for side in 'NSWE':
 for a,b in [(-9.5,-3),(3,9.5)]:
  if side=='N':box('Geo_Inner_N',a,b,9.5,10)
  if side=='S':box('Geo_Inner_S',a,b,-10,-9.5)
  if side=='E':box('Geo_Inner_E',9.5,10,a,b)
  if side=='W':box('Geo_Inner_W',-10,-9.5,a,b)
 if side in 'NS':box('Geo_Inner_Lintel_'+side,-3,3,9.5 if side=='N' else -10,10 if side=='N' else -9.5,5,8)
 else:box('Geo_Inner_Lintel_'+side,9.5 if side=='E' else -10,10 if side=='E' else -9.5,-3,3,5,8)
for x in [-10,9.5]:
 for y in [-10,9.5]:box('Geo_Inner_Corner',x,x+.5,y,y+.5)
mirror([(-20,-16,15.5,16),(-16.5,-16,16,16.5),(-16.5,-16,19.5,20)],'Pocket')
for sx in [-1,1]:
 for sy in [-1,1]:
  box('Geo_Pocket_Lintel',*sorted([sx*16,sx*16.5]),*sorted([sy*16.5,sy*19.5]),3.5,8,'MH_Wall_Bones')
  slot(f'DoorSub_{sx}_{sy}',sx*16.25,sy*18,90 if sx==-1 else -90,3,3.5,'door_wood_small')
for i,(x,y) in enumerate([(x,y) for x in [-5,5] for y in [-5,5]]):column(x,y,i)
room('room_comb')
for sy in [-1,1]:
 for i,(a,b) in enumerate([(-18,-17.5),(-7.5,-7),(7,7.5),(17.5,18)]):box(f'Geo_Tooth_{sy}_{i}',a,b,*sorted([sy*15,sy*20]))
for i,(x,y) in enumerate([(x,y) for x in [-6,6] for y in [-6,6]]):column(x,y,i)
room('room_gate',gate=True)
for sx in [-1,1]:
 for a,b in [(5.5,6),(10,10.5),(-6,-5.5),(-10.5,-10)]:box('Geo_Niche',*sorted([sx*18.5,sx*20]),a,b)
for i,(x,y) in enumerate([(x,y) for x in [-6,6] for y in [-12,-4,4]]):column(x,y,i)
for sx in [-1,1]:
 box('Geo_Tambour_1',*sorted([sx*4,sx*6.5]),20.5,21.5,0,8)
 box('Geo_Tambour_2',*sorted([sx*3,sx*6.5]),21.5,22.5,0,8)
box('Geo_Tambour_Floor',-6.5,6.5,20.5,22.5,-.2,0,'MH_Floor_Cracked')
box('Geo_Tambour_Ceiling',-6.5,6.5,20.5,22.5,8,8.2,'MH_Ceiling_Basalt')
# Fill the ceiling-height difference above the 6m corridor without narrowing the floor.
box('Geo_Tambour_Exit_Header',-3,3,22,22.5,6,8)
slot('Anchor_Corridor_N',0,22.5,0,6,6,'')
room('room_mini_pillars',12)
for i,(x,y) in enumerate([(x,y) for x in [-4,4] for y in [-4,4]]):column(x,y,i)
room('room_mini_cover',12)
box('Geo_Cover_W',-7,-5,-7,-2);box('Geo_Cover_E',5,7,2,7)
start('corridor_straight','corridor')
box('Geo_Wall_W',-3.5,-3,-12,12,0,6);box('Geo_Wall_E',3,3.5,-12,12,0,6)
box('Geo_Floor',-3.5,3.5,-12,12,-.2,0,'MH_Floor_Cracked');box('Geo_Ceiling',-3.5,3.5,-12,12,6,6.2,'MH_Ceiling_Basalt')
slot('Slot_S',0,-12,180,6,6,'');slot('Slot_N',0,12,0,6,6,'')
start('corridor_corner','corridor')
for n,a,b,c,d in [('W',-3.5,-3,-12,-3.5),('E',3,3.5,-12,3),('N',-12,3.5,3,3.5),('S',-12,-3,-3.5,-3)]:box('Geo_Wall_'+n,a,b,c,d,0,6)
for z0,z1,n,mat in [(-.2,0,'Floor','MH_Floor_Cracked'),(6,6.2,'Ceiling','MH_Ceiling_Basalt')]:
 box('Geo_'+n+'_V',-3.5,3.5,-12,3.5,z0,z1,mat);box('Geo_'+n+'_H',-12,-3.5,-3.5,3.5,z0,z1,mat)
slot('Slot_S',0,-12,180,6,6,'');slot('Slot_W',-12,0,90,6,6,'')

def wooden_door(name,w,h,gate=False):
 start(name,'door')
 current['motion']='Single solid panel; translate vertically only; bottom-center origin; no animation'
 boards=10 if gate else 8
 for i in range(boards):
  x0=-w/2+w*i/boards;x1=-w/2+w*(i+1)/boards
  center=abs((x0+x1)/2)
  top=(5.5 if center>4 else 6.25 if center>3 else 7) if gate else h
  ob=box('Oak_Plank',x0,x1,-.25,.25,0,top,'MH_Wood_AshGray')
  for loop in ob.data.uv_layers.active.data:loop.uv.x+=i*.19
 for face in [-1,1]:
  ya,yb=(-.34,-.25) if face==-1 else (.25,.34)
  for z in ([1,3,4.8] if gate else [h*.22,h*.76]):
   box('Continuous_Iron_Strap',-w/2+.06,w/2-.06,ya,yb,z,z+.18,'MH_Metal_Dark')
  if gate:
   for i in range(boards):
    x0=-w/2+i;x1=x0+1;center=abs((x0+x1)/2);top=5.5 if center>4 else 6.25 if center>3 else 7
    box('Stepped_Iron_Crown',x0,x1,ya,yb,top-.16,top,'MH_Metal_Dark')
wooden_door('door_wood',6,4.5)
wooden_door('door_wood_small',3,3.5)
wooden_door('gate_big',10,7,True)

for mat in M.values():
 for node in mat.node_tree.nodes:
  if node.type=='TEX_IMAGE' and node.image:node.image.pack()
for item in manifest['modules']:
 root=roots[item['name']];objects=[root]+list(root.children_recursive)
 bpy.ops.object.select_all(action='DESELECT')
 for obj in objects:obj.select_set(True)
 bpy.context.view_layer.objects.active=root
 bpy.ops.export_scene.fbx(filepath=str(UNITY/(item['name']+'.fbx')),use_selection=True,object_types={'EMPTY','MESH'},apply_unit_scale=True,bake_anim=False,add_leaf_bones=False,axis_forward='-Z',axis_up='Y',use_custom_props=True)
 bpy.ops.export_scene.gltf(filepath=str(OUT/(item['name']+'.glb')),use_selection=True,use_active_scene=True,export_format='GLB',export_animations=False)
 bpy.data.libraries.write(str(OUT/(item['name']+'.blend')),set(objects),fake_user=True)
 item['triangles']=sum(len(o.data.polygons)*2 for o in objects if o.type=='MESH')
for i,item in enumerate(manifest['modules']):roots[item['name']].location=(i%4*55,i//4*55,0)
manifest['sceneName']=scene.name
(OUT/'structures_manifest.json').write_text(json.dumps(manifest,indent=2))
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'mazehell_structures.blend'),copy=True)
bpy.context.window.scene=previous_scene
print('REVISED_STATIC_KIT',[(m['name'],m['triangles']) for m in manifest['modules']])
