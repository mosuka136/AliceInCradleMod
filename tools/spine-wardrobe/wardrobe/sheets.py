"""Fixed-cell imagegen references and deterministic return to logical part canvases."""
from pathlib import Path
import math
from PIL import Image, ImageDraw
import numpy as np
from .common import read_json, write_json, sha, within


def make_sheet(mapping_path, names, output, cell=768, columns=2):
    mapping_path, output = Path(mapping_path), Path(output)
    mapping = read_json(mapping_path)
    by_name = {r['region']: r for r in mapping['regions']}
    rows = math.ceil(len(names) / columns)
    sheet = Image.new('RGBA', (columns * cell, rows * cell))
    draw = ImageDraw.Draw(sheet)
    placements = []
    for index, name in enumerate(names):
        part_path = mapping_path.parent / by_name[name].get('referencePart', by_name[name]['part'])
        part = Image.open(part_path).convert('RGBA')
        thumb = part.copy()
        thumb.thumbnail((cell - 96, cell - 96))
        x, y = index % columns * cell, index // columns * cell
        ox, oy = x + (cell - thumb.width) // 2, y + 48 + (cell - 96 - thumb.height) // 2
        sheet.paste(thumb, (ox, oy))
        draw.text((x + 16, y + 16), f'{index + 1:02} {name}', fill=(100, 110, 130, 255), font_size=24)
        placements.append({'region': name, 'original': str(part_path.resolve()), 'originalSha256': sha(part_path),
                           'box': [ox, oy, ox + thumb.width, oy + thumb.height],
                           'cellBox': [x, y, x + cell, y + cell], 'logicalSize': list(part.size)})
    output.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(output)
    write_json(output.with_suffix('.json'), {'sheetSize': list(sheet.size), 'placements': placements})


def green_matte(image):
    """Extract an explicitly generated flat green screen; never erase a checkerboard heuristically."""
    values = np.asarray(image.convert('RGB'), dtype=np.float32) / 255
    green = values[..., 1] - np.maximum(values[..., 0], values[..., 2])
    if float(np.mean(green > 0.9)) < 0.2:
        raise ValueError('No substantial pure chroma-green background was found.')
    alpha = np.clip(1 - np.maximum(green, 0), 0, 1)
    # Generated green screens can vary in brightness. Pure saturated green is
    # background even when its G channel is below 255; otherwise it leaves speckles.
    saturated_key = (values[..., 1] > 0.6) & (values[..., 1] > np.maximum(values[..., 0], values[..., 2]) * 2.2)
    alpha[saturated_key] = 0
    alpha[alpha < 0.04] = 0
    rgba = np.zeros((*alpha.shape, 4), dtype=np.float32)
    foreground = values.copy()
    foreground[..., 1] -= 1 - alpha
    rgba[..., :3] = np.clip(foreground / np.maximum(alpha[..., None], 1e-6), 0, 1)
    rgba[..., 3] = alpha
    return Image.fromarray(np.uint8(rgba * 255))


def ingest_sheet(sheet_path, layout_path, output, key_green=False, fit_cell=False):
    sheet = Image.open(sheet_path)
    if key_green:
        sheet = green_matte(sheet)
    layout = read_json(layout_path)
    if sheet.mode != 'RGBA' or sheet.getchannel('A').getextrema()[0] == 255:
        raise ValueError('Imagegen output must have actual transparency; do not treat a checkerboard as alpha.')
    output = Path(output)
    output.mkdir(parents=True, exist_ok=True)
    sheet.save(output / 'sheet-rgba.png')
    scale_x, scale_y = sheet.width / layout['sheetSize'][0], sheet.height / layout['sheetSize'][1]
    if abs(scale_x - scale_y) > 0.005:
        raise ValueError('Generated sheet aspect ratio changed.')
    result = {}
    for index, placement in enumerate(layout['placements']):
        if sha(placement['original']) != placement['originalSha256']:
            raise ValueError('Original part changed after the reference sheet was prepared.')
        source_box = placement['cellBox'] if fit_cell else placement['box']
        box = [round(v * (scale_x if i % 2 == 0 else scale_y)) for i, v in enumerate(source_box)]
        crop = sheet.crop(box)
        if fit_cell:
            bounds = crop.getchannel('A').point(lambda a: 255 if a > 32 else 0).getbbox()
            if bounds is None:
                raise ValueError('Generated cell is empty: ' + placement['region'])
            crop = crop.crop(bounds)
            original = Image.open(placement['original']).convert('RGBA')
            target_box = original.getchannel('A').getbbox()
            tile = crop.resize((target_box[2] - target_box[0], target_box[3] - target_box[1]), Image.Resampling.LANCZOS)
            part = Image.new('RGBA', original.size)
            part.paste(tile, target_box[:2])
        else:
            part = crop.resize(placement['logicalSize'], Image.Resampling.LANCZOS)
        if part.getchannel('A').getbbox() is None:
            raise ValueError('Generated part is empty: ' + placement['region'])
        destination = output / f'{index:03}.png'
        part.save(destination)
        result[placement['region']] = str(destination.resolve())
    write_json(output / 'selection.json', {'parts': result, 'geometry': [], 'sourceSheet': str(Path(sheet_path).resolve()),
                                         'layout': str(Path(layout_path).resolve()), 'layoutSha256': sha(layout_path),
                                         'sourceSheetSha256': sha(sheet_path), 'fitCell': fit_cell,
                                         'requiresVisualRegistrationCheck': True})
    return result
