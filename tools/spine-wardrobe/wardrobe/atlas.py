"""Lossless atlas unpacking and deterministic whole-page packing."""
from pathlib import Path
import math
import numpy as np
from PIL import Image, ImageDraw


def unpack_region(page, region):
    x, y, w, h = (int(region[k]) for k in ('x', 'y', 'width', 'height'))
    tile = page.crop((x, y, x + w, y + h))
    if region['degrees'] == 90:
        tile = tile.transpose(Image.Transpose.ROTATE_270)
    elif region['degrees'] != 0:
        raise ValueError('Unsupported rotation')
    canvas = Image.new('RGBA', (int(region['originalWidth']), int(region['originalHeight'])))
    left = int(region['offsetX'])
    top = canvas.height - int(region['offsetY']) - tile.height
    if min(left, top) < 0 or left + tile.width > canvas.width or top + tile.height > canvas.height:
        raise ValueError('Invalid logical region bounds: ' + region['name'])
    canvas.paste(tile, (left, top))
    return canvas


def packed_tile(canvas, region):
    width = int(region['height'] if region['degrees'] == 90 else region['width'])
    height = int(region['width'] if region['degrees'] == 90 else region['height'])
    left = int(region['offsetX'])
    top = canvas.height - int(region['offsetY']) - height
    tile = canvas.crop((left, top, left + width, top + height))
    return tile.transpose(Image.Transpose.ROTATE_90) if region['degrees'] == 90 else tile


def roundtrip(page, regions, parts):
    result = page.copy()
    for region in regions:
        result.paste(packed_tile(parts[region['name']], region), (region['x'], region['y']))
    return bool(np.array_equal(np.asarray(page), np.asarray(result)))


def contact_sheet(parts, destination):
    cell, columns = 220, 5
    rows = math.ceil(len(parts) / columns)
    result = Image.new('RGB', (columns * cell, rows * (cell + 32)), '#d8d8df')
    draw = ImageDraw.Draw(result)
    for i, (name, part) in enumerate(parts.items()):
        image = part.copy()
        image.thumbnail((cell - 16, cell - 16))
        x, y = i % columns * cell, i // columns * (cell + 32)
        result.paste(image, (x + (cell - image.width) // 2, y + (cell - image.height) // 2), image)
        draw.text((x + 5, y + cell), f'{i:03} {name[:27]}', fill='black')
    result.save(destination)


def write_atlas(path, page_name, size, regions):
    lines = [page_name, f'size:{size[0]},{size[1]}', 'filter:Linear,Linear', 'pma:false']
    for r in regions:
        # The game's reader swaps logical bounds at 90 degrees.
        w, h = (r['height'], r['width']) if r['degrees'] == 90 else (r['width'], r['height'])
        lines += [r['name'], f"bounds:{r['x']},{r['y']},{w},{h}",
                  f"offsets:{int(r['offsetX'])},{int(r['offsetY'])},{r['originalWidth']},{r['originalHeight']}"]
        if r['degrees']:
            lines.append('rotate:90')
    Path(path).write_bytes(('\r\n'.join(lines) + '\r\n').encode('utf-8'))


def assemble_page(original, regions, parts, changed, max_size=8192):
    needs_repack = False
    for region in regions:
        image = parts[region['name']]
        if image.size != (region['originalWidth'], region['originalHeight']):
            raise ValueError('Part must retain its full logical canvas: ' + region['name'])
        if region['name'] not in changed:
            continue
        projected = Image.new('RGBA', image.size)
        projected_tile = packed_tile(image, region)
        projected_page = Image.new('RGBA', original.size)
        projected_page.paste(projected_tile, (region['x'], region['y']))
        projected = unpack_region(projected_page, region)
        if not np.array_equal(np.asarray(projected)[:, :, 3], np.asarray(image)[:, :, 3]):
            needs_repack = True
    if not needs_repack:
        result = original.copy()
        for r in regions:
            if r['name'] in changed:
                result.paste(packed_tile(parts[r['name']], r), (r['x'], r['y']))
        return result, regions

    # Shelf pack every original logical canvas, trimming only transparent margins.
    tiles = []
    for region in regions:
        image = parts[region['name']]
        box = image.getchannel('A').getbbox() or (0, 0, 1, 1)
        tiles.append((region, image.crop(box), box))
    tiles.sort(key=lambda item: (-item[1].height, item[0]['name']))
    minimum = max(tile.width + 4 for _, tile, _ in tiles)
    width = max(minimum, math.ceil(math.sqrt(sum((t.width + 4) * (t.height + 4) for _, t, _ in tiles))))
    while width <= max_size:
        x = y = row_height = 2
        placements = []
        for region, tile, box in tiles:
            if x + tile.width + 2 > width:
                x, y, row_height = 2, y + row_height + 4, 0
            placements.append((region, tile, box, x, y))
            x += tile.width + 4
            row_height = max(row_height, tile.height)
        height = y + row_height + 2
        if height <= max_size:
            break
        if width == max_size:
            raise ValueError('Complete atlas does not fit the texture limit.')
        width = min(max_size, width * 2)
    else:
        raise ValueError('Region exceeds texture limit.')
    page = Image.new('RGBA', (width, height))
    packed = []
    for region, tile, box, x, y in placements:
        page.paste(tile, (x, y))
        # Extrude edge pixels into padding to avoid bilinear colour seams.
        for dx in (-2, -1, tile.width, tile.width + 1):
            edge = tile.crop((0 if dx < 0 else tile.width - 1, 0, 1 if dx < 0 else tile.width, tile.height))
            page.paste(edge, (x + dx, y))
        for dy in (-2, -1, tile.height, tile.height + 1):
            edge = page.crop((x - 2, y if dy < 0 else y + tile.height - 1, x + tile.width + 2, y + 1 if dy < 0 else y + tile.height))
            page.paste(edge, (x - 2, y + dy))
        packed.append({**region, 'x': x, 'y': y, 'width': tile.width, 'height': tile.height,
                       'degrees': 0, 'offsetX': box[0], 'offsetY': region['originalHeight'] - box[3]})
    return page, packed
