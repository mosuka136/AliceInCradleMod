"""CPU preview of vertices computed by the game's own Spine runtime.

This previews atlas colour and slot blending, not the game's material effects.
"""
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
import os
import numpy as np
from PIL import Image, ImageDraw
from .common import write_json


def bounds(documents):
    arrays = [np.asarray(draw['vertices']).reshape(-1, 2) for doc in documents for frame in doc['frames']
              for draw in frame['draws'] if draw['vertices'] and draw['color'][3] > 0]
    if not arrays:
        raise ValueError('No visible vertices')
    values = np.concatenate(arrays)
    lo, hi = values.min(axis=0), values.max(axis=0)
    return lo, hi


def render_frame(frame, texture, view, size=640):
    lo, hi = view
    scale = (size - 32) / max(*(hi - lo), 1)
    dimensions = np.maximum(1, np.ceil((hi - lo) * scale).astype(int)) + 32
    width, height = dimensions
    canvas = np.zeros((height, width, 4), dtype=np.float32)
    for draw in frame['draws']:
        positions = np.asarray(draw['vertices'], dtype=float).reshape(-1, 2)
        positions = (positions - lo) * scale + 16
        positions[:, 1] = height - positions[:, 1]
        uvs = np.asarray(draw['uvs'], dtype=float).reshape(-1, 2)
        tint = np.asarray(draw['color'])
        layer = np.zeros_like(canvas)
        for triangle in np.asarray(draw['triangles']).reshape(-1, 3):
            p = positions[triangle]
            minimum = np.maximum(0, np.floor(p.min(axis=0)).astype(int))
            maximum = np.minimum((width, height), np.ceil(p.max(axis=0)).astype(int) + 1)
            if np.any(maximum <= minimum):
                continue
            x0, y0 = minimum
            x1, y1 = maximum
            grid_y, grid_x = np.mgrid[y0:y1, x0:x1]
            px, py = grid_x + 0.5, grid_y + 0.5
            den = (p[1, 1] - p[2, 1]) * (p[0, 0] - p[2, 0]) + (p[2, 0] - p[1, 0]) * (p[0, 1] - p[2, 1])
            if abs(den) < 1e-8:
                continue
            a = ((p[1, 1] - p[2, 1]) * (px - p[2, 0]) + (p[2, 0] - p[1, 0]) * (py - p[2, 1])) / den
            b = ((p[2, 1] - p[0, 1]) * (px - p[2, 0]) + (p[0, 0] - p[2, 0]) * (py - p[2, 1])) / den
            c = 1 - a - b
            mask = (a >= -1e-6) & (b >= -1e-6) & (c >= -1e-6)
            uv = a[..., None] * uvs[triangle[0]] + b[..., None] * uvs[triangle[1]] + c[..., None] * uvs[triangle[2]]
            # Spine atlas V coordinates use the image's top-down pixel convention.
            tx = np.clip(uv[..., 0] * texture.shape[1] - 0.5, 0, texture.shape[1] - 1)
            ty = np.clip(uv[..., 1] * texture.shape[0] - 0.5, 0, texture.shape[0] - 1)
            ix, iy = tx.astype(int), ty.astype(int)
            jx, jy = np.minimum(ix + 1, texture.shape[1] - 1), np.minimum(iy + 1, texture.shape[0] - 1)
            fx, fy = (tx - ix)[..., None], (ty - iy)[..., None]
            colour = ((texture[iy, ix] * (1 - fx) + texture[iy, jx] * fx) * (1 - fy)
                      + (texture[jy, ix] * (1 - fx) + texture[jy, jx] * fx) * fy) * tint
            layer[y0:y1, x0:x1][mask] = colour[mask]
        alpha = layer[..., 3:4]
        dst_alpha = canvas[..., 3:4]
        out_alpha = alpha + dst_alpha * (1 - alpha)
        rgb = layer[..., :3]
        mode = draw['blend'].lower()
        if mode == 'additive':
            numerator = canvas[..., :3] * dst_alpha + rgb * alpha
        else:
            if mode == 'multiply':
                rgb = rgb * canvas[..., :3] * dst_alpha + rgb * (1 - dst_alpha)
            elif mode == 'screen':
                rgb = (1 - (1 - rgb) * (1 - canvas[..., :3])) * dst_alpha + rgb * (1 - dst_alpha)
            elif mode != 'normal':
                raise ValueError('Unsupported blend: ' + mode)
            numerator = rgb * alpha + canvas[..., :3] * dst_alpha * (1 - alpha)
        canvas[..., :3] = numerator / np.maximum(out_alpha, 1e-8)
        canvas[..., 3:4] = out_alpha
    return Image.fromarray(np.uint8(np.clip(canvas, 0, 1) * 255))


_worker_textures = None
_worker_view = None
_worker_size = None


def _initialize_renderer(paths, view, size):
    global _worker_textures, _worker_view, _worker_size
    _worker_textures = [np.asarray(Image.open(p).convert('RGBA'), dtype=np.float32) / 255 for p in paths]
    _worker_view, _worker_size = view, size


def _render_pair(task):
    left, right, destination = task
    images = [render_frame(left, _worker_textures[0], _worker_view, _worker_size)]
    if left == right and np.array_equal(_worker_textures[0], _worker_textures[1]):
        images.append(images[0])
    else:
        images.append(render_frame(right, _worker_textures[1], _worker_view, _worker_size))
    pair = Image.new('RGB', (images[0].width * 2, images[0].height + 30), '#d4d6dc')
    for n, image in enumerate(images):
        pair.paste(image, (n * image.width, 30), image)
    ImageDraw.Draw(pair).text((8, 8), f"{left['name']} t={left['time']:.3f}   ORIGINAL | CANDIDATE", fill='black')
    pair.save(destination)
    return destination


def preview_pair(original, candidate, original_image, candidate_image, output, size=480):
    output = Path(output)
    output.mkdir(parents=True, exist_ok=True)
    view = bounds([original, candidate])
    groups, index, tasks = {}, [], []
    for i, (left, right) in enumerate(zip(original['frames'], candidate['frames'], strict=True)):
        if (left['name'], left['time']) != (right['name'], right['time']):
            raise ValueError('Frame identity mismatch')
        filename = f'{i:05}.png'
        tasks.append((left, right, str(output / filename)))
        groups.setdefault(left['name'], []).append(output / filename)
        index.append({'file': filename, 'case': left['name'], 'time': left['time']})
    paths = [str(original_image), str(candidate_image)]
    workers = min(4, os.cpu_count() or 1, len(tasks))
    if len(tasks) < 16:
        _initialize_renderer(paths, view, size)
        for task in tasks:
            _render_pair(task)
    else:
        _initialize_renderer(paths, view, size)
        with ThreadPoolExecutor(max_workers=workers) as pool:
            for i, _ in enumerate(pool.map(_render_pair, tasks, chunksize=4)):
                if i % 100 == 0:
                    print(f'preview {output.parent.name}: {i + 1}/{len(tasks)}', flush=True)
    selected = []
    for i, (name, files) in enumerate(groups.items()):
        images = [Image.open(path).convert('RGB') for path in files]
        images[0].save(output / f'case-{i:03}.gif', save_all=True, append_images=images[1:], duration=125, loop=0)
        thumb = images[0].copy()
        thumb.thumbnail((240, 260))
        selected.append(thumb)
        for image in images:
            image.close()
    thumb_width = 240
    thumbs = []
    for image in selected:
        image = image.copy()
        image.thumbnail((thumb_width, 260))
        thumbs.append(image)
    sheet = Image.new('RGB', (thumb_width * 4, 280 * ((len(thumbs) + 3) // 4)), 'white')
    for i, image in enumerate(thumbs):
        sheet.paste(image, (i % 4 * thumb_width, i // 4 * 280))
    sheet.save(output / 'contact-sheet.png')
    write_json(output / 'index.json', {'scope': 'offline-colour-preview', 'frames': index, 'cases': list(groups)})


def geometry_report(original, candidate):
    errors, warnings = [], []
    if len(original['frames']) != len(candidate['frames']):
        return {'passed': False, 'errors': ['Frame count changed'], 'warnings': []}
    for left, right in zip(original['frames'], candidate['frames']):
        identity = f"{left['name']}@{left['time']}"
        if (left['name'], left['time']) != (right['name'], right['time']):
            errors.append(identity + ': frame identity changed')
        if not np.allclose(left['bones'], right['bones'], atol=1e-5, rtol=0):
            errors.append(identity + ': bone motion changed')
        if len(left['draws']) != len(right['draws']):
            errors.append(identity + ': draw order changed')
            continue
        for a, b in zip(left['draws'], right['draws']):
            if (a['slot'], a['attachment']) != (b['slot'], b['attachment']):
                errors.append(identity + ': attachment selection changed')
                continue
            av, bv = np.asarray(a['vertices']).reshape(-1, 2), np.asarray(b['vertices']).reshape(-1, 2)
            if a['triangles'] != b['triangles'] or av.shape != bv.shape:
                warnings.append(identity + ': clipping topology differs; review ' + a['slot'])
                continue
            for tri in np.asarray(a['triangles']).reshape(-1, 3):
                p, q = av[tri], bv[tri]
                cross = lambda x: (x[1, 0] - x[0, 0]) * (x[2, 1] - x[0, 1]) - (x[1, 1] - x[0, 1]) * (x[2, 0] - x[0, 0])
                old_area, new_area = cross(p), cross(q)
                if abs(old_area) > 1e-4 and (old_area * new_area < 0 or abs(new_area) < 1e-6):
                    errors.append(identity + ': inverted/degenerate triangle in ' + a['slot'])
                edges_a = np.linalg.norm(p - np.roll(p, 1, axis=0), axis=1)
                edges_b = np.linalg.norm(q - np.roll(q, 1, axis=0), axis=1)
                valid = edges_a > 0.1
                if np.any(edges_b[valid] / edges_a[valid] > 2.5):
                    errors.append(identity + ': excessive edge stretch in ' + a['slot'])
    return {'passed': not errors, 'frames': len(original['frames']), 'errors': sorted(set(errors)), 'warnings': sorted(set(warnings))}
