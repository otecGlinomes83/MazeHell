# MazeHell Art Assets

53 runtime PNGs, grouped by character, action and state. All paths use ASCII English names.

```text
Player/
  Face/
    Healthy/        # 4 frames per health state
    Scratched/
    Bleeding/
    Critical/
    NearDeath/
    Hit/            # temporary damage reaction
  Weapons/Shotgun/
    Idle/
    FireSingle/
    FireDouble/
    ReloadOpen/     # break action open, two empty chambers
    ReloadInsert/   # insert two shells
    Empty/
Enemies/Imp/
  Walk/             # front, two frames
  WalkSide/         # side, two frames; opposite direction uses flipX
  WalkBack/         # rear, two frames
  Windup/
  Lunge/
  Pain/
  Corpse/
HUD/
  Crosshair/
  Ammo/
  Dash/
  EnemyCounter/
World/
  Walls/Hellstone/
  Walls/Lava/
  Walls/Bones/
  Floor/CrackedStone/
Menu/MainMenu/
```

## Frames and import settings

Imp: 256x256. Shotgun: 640x256. Face: 128x128. HUD icons: 64x64. World tiles: 256x256. Menu: 1920x1080.

Face frame order: `_00`, `_01`, `_02`, `_03`. Health animations loop; `Hit` temporarily overrides the active portrait and returns to it after completion. Timing and current frame paths are in `animation_timing.json`. `inventory.json` lists all 47 PNGs.

Sprites: TextureImporterType.Sprite, real transparency, Point filtering, no compression, Clamp, no mipmaps, Full Rect. Imp pivot: `(0.5, 0.08)`. Tiles: Repeat with mipmaps; opposite RGB borders match exactly. Shotgun recoil uses a rigid 1/2-degree rotation without changing barrel perspective.

Resources examples: `Images/Player/Face/Healthy/healthy_00` and `Images/Enemies/Imp/Walk/walk_1`.

## Sources and previews

- `ArtPreview/START_HERE.md`: preview index, contact sheets and animated face GIFs.
- `ArtSource/Generated`: 29 full-resolution selected source images with the same category structure.
- `Tools/Art/generated_sources.json`: source/output mapping and image-generation prompts.
- `Tools/Art/asset_moves.json`: old-to-current runtime image paths.
- `Assets/MazeHell/Resources/Fonts`: Press Start 2P, including Cyrillic support and its OFL license.
- `Assets/MazeHell/Resources/Models/heal_orb.fbx`: animated healing crystal; Blender and GLB sources in `ArtSource/Models`. Hover loop: 2 seconds at 24 fps. Set up emissive URP materials when integrating the FBX; Blender/GLB preserve emissive materials.

Regenerate packaged assets from the repository root:
`python3 Tools/Art/prepare_images.py Tools/Art/generated_sources.json`

Raster artwork was generated with built-in image_gen; ImageMagick handles slicing, resizing and tile borders. Gameplay state machines and HUD animation playback are integrated separately.

Gameplay additions: imp side/back walking frames and shotgun Block state. Sources and generation prompts: `Tools/Art/gameplay_sources.json`. All 48 sprite textures have been imported as Sprite (Single) and loaded by Unity; the five world tiles remain repeatable textures. Animation and audio references are assigned in `Assets/MazeHell/Scriptables/UI/AnimationLibrary.asset` and `Audio.asset`.
