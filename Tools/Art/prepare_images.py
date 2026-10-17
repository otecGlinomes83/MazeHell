"""Package generated raster art; ImageMagick only resizes, slices and tiles it.

Usage: python3 Tools/Art/prepare_images.py Tools/Art/generated_sources.json
The source manifest records the original generated PNGs and their prompts.
"""
import json
import subprocess
import sys
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/MazeHell/Resources/Images'
PREVIEW = ROOT / 'ArtPreview'
PREVIEW.mkdir(exist_ok=True)
META_TEMPLATE = (ROOT / 'Assets/TutorialInfo/Icons/URP.png.meta').read_text()

def magick(*args):
    subprocess.run(['magick', *map(str, args)], check=True)

def meta(path, kind):
    dest = Path(str(path) + '.meta')
    if dest.exists():
        return
    text = META_TEMPLATE.replace('727a75301c3d24613a3ebcec4a24c2c8', uuid.uuid4().hex)
    text = text.replace('textureCompression: 1', 'textureCompression: 0')
    if kind == 'tile':
        text = text.replace('textureType: 2', 'textureType: 0').replace('wrapU: 1', 'wrapU: 0').replace('wrapV: 1', 'wrapV: 0')
        text = text.replace('enableMipMap: 0', 'enableMipMap: 1').replace('alphaIsTransparency: 1', 'alphaIsTransparency: 0')
    else:
        text = text.replace('textureType: 2', 'textureType: 8')
        text = text.replace('spriteMeshType: 1', 'spriteMeshType: 0')
        text = text.replace('spriteMode: 0', 'spriteMode: 1')
        if kind == 'enemy':
            text = text.replace('alignment: 0', 'alignment: 9').replace('spritePivot: {x: 0.5, y: 0.5}', 'spritePivot: {x: 0.5, y: 0.08}')
    dest.write_text(text)

def save(source, relative, size, kind):
    dest = OUT / relative
    dest.parent.mkdir(parents=True, exist_ok=True)
    if kind == 'tile':
        # Blend only a narrow wrap margin; keep the generated interior intact.
        # Unlike mirroring the whole cell, this avoids kaleidoscopic repetition.
        magick(source, '-filter', 'point', '-resize', '256x256!',
               '(', '+clone', '-flop', ')',
               '(', '-size', '256x256', 'xc:black', '-fx', '0.5*max(0,1-min(i,w-1-i)/12)', ')', '-compose', 'Over', '-composite',
               '(', '+clone', '-flip', ')',
               '(', '-size', '256x256', 'xc:black', '-fx', '0.5*max(0,1-min(j,h-1-j)/12)', ')', '-compose', 'Over', '-composite',
               '(', '+clone', '-crop', '1x256+0+0', '+repage', ')', '-geometry', '+255+0', '-composite',
               '(', '+clone', '-crop', '256x1+0+0', '+repage', ')', '-geometry', '+0+255', '-composite', dest)
    else:
        magick(source, '-filter', 'point', '-resize', f'{size[0]}x{size[1]}!', dest)
        if kind == 'weapon' and dest.stem in ('shot_1', 'volley'):
            # Recoil is a rigid screen-space rotation, never a change in barrel geometry.
            degrees = 1 if dest.stem == 'shot_1' else 2
            magick(dest, '-background', 'none', '-virtual-pixel', 'transparent',
                   '-filter', 'point', '-define', 'distort:viewport=640x256+0+0',
                   '-distort', 'SRT', f'640,256 1 {degrees} 640,256', dest)
    meta(dest, kind)
    return dest

def main():
    sources = json.loads(Path(sys.argv[1]).read_text())
    inventory = []
    for asset in sources:
        kind = asset['kind']
        if kind == 'face_sheet':
            state = asset['id']
            # Equal 2x2 grid: no resampling of different cells together.
            for frame in range(4):
                tmp = PREVIEW / f'_slice_{state}_{frame}.png'
                magick(asset['source'], '-crop', '2x2@', '+repage', '-delete', ','.join(str(i) for i in range(4) if i != frame), tmp)
                relative = f'{asset["output_directory"]}/{state}_{frame:02d}.png'
                dest = save(tmp, relative, (128, 128), 'face')
                tmp.unlink()
                inventory.append({'path': str(dest.relative_to(OUT)), 'width': 128, 'height': 128, 'kind': 'face', 'state': state, 'frame': frame})
        else:
            dest = save(asset['source'], asset['output'], asset['size'], kind)
            inventory.append({'path': str(dest.relative_to(OUT)), 'width': asset['size'][0], 'height': asset['size'][1], 'kind': kind})
    (OUT / 'inventory.json').write_text(json.dumps(inventory, indent=2) + '\n')

    # Transparent-background animations are flattened only in previews.
    for asset in sources:
        if asset['kind'] != 'face_sheet':
            continue
        state = asset['id']
        frames = sorted((OUT / asset['output_directory']).glob(f'{state}_*.png'))
        if not frames:
            continue
        durations = [75, 35, 12, 50] if state != 'hit' else [7, 9, 8, 10]
        args = []
        for frame, delay in zip(frames, durations):
            args += ['-delay', str(delay), '(', frame, '-background', '#160e13', '-alpha', 'remove', '-filter', 'point', '-resize', '256x256', ')']
        preview_path = PREVIEW / asset['preview_output']
        preview_path.parent.mkdir(parents=True, exist_ok=True)
        magick(*args, '-loop', '0', preview_path)
    print(f'Packaged {len(inventory)} PNG assets to {OUT}')

if __name__ == '__main__':
    main()
