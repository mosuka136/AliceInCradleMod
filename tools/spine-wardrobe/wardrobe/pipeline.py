"""Stateful production pipeline; design approval and game QA are explicit gates."""
import json
from pathlib import Path
import shutil
import sys
import time
import importlib.metadata
from PIL import Image, ImageDraw, ImageChops
from .common import (REPO, TOOL, entries, fingerprint, package_files, read_json, run_runtime,
                     runtime_fingerprint, safe_name, sha, verify_fingerprint, within, write_json)
from .atlas import unpack_region, roundtrip, contact_sheet, assemble_page, write_atlas
from .render import geometry_report, preview_pair


def inspect(job):
    game = Path(job['gameDir'])
    root = Path(job['outputDir'])
    plugin = game / 'BepInEx/plugins/BetterExperience/BetterExperience.dll'
    runtime = game / 'AliceInCradle_Data/Managed/spine-unity.dll'
    reference = REPO / 'ReferenceLibrary/spine-unity.dll'
    report = {'gameDir': str(game), 'python': sys.executable,
              'packages': {p: importlib.metadata.version(p) for p in ['UnityPy', 'Pillow', 'numpy']},
              'plugin': {'path': str(plugin), 'resolvedPath': str(plugin.resolve()), 'symlink': plugin.is_symlink(),
                         'sha256': sha(plugin) if plugin.is_file() else None},
              'runtimeMatches': runtime.is_file() and sha(runtime) == sha(reference),
              'targets': {}}
    for target in job['targets']:
        paths = [game / 'AliceInCradle_Data/StreamingAssets/SpineAnim' / (target + suffix)
                 for suffix in ['.dat', '.atlas.dat']]
        report['targets'][target] = fingerprint(paths)
        for path in paths:
            with path.open('rb') as stream:
                if stream.read(7) != b'UnityFS':
                    raise ValueError('Not a UnityFS bundle: ' + str(path))
    write_json(root / 'inspect.json', report)
    if not report['runtimeMatches']:
        raise ValueError('Game and reference spine-unity.dll differ; do not substitute runtimes.')
    return report


def extract(job):
    import UnityPy
    report = inspect(job)
    root = Path(job['outputDir'])
    results = {}
    for target in job['targets']:
        destination = root / 'original' / target
        record = destination / 'source.json'
        if record.exists():
            previous = read_json(record)
            if previous['sources'] == report['targets'][target] and verify_fingerprint(previous['outputs']):
                results[target] = 'reused'
                continue
        destination.mkdir(parents=True, exist_ok=True)
        texts, textures = {}, {}
        for bundle in report['targets'][target]:
            env = UnityPy.load(bundle)
            for obj in env.objects:
                if obj.type.name == 'TextAsset':
                    data = obj.read()
                    text = data.m_Script
                    if isinstance(text, bytes):
                        text = text.decode('utf-8')
                    if data.m_Name in texts:
                        raise ValueError('Ambiguous TextAsset: ' + data.m_Name)
                    texts[data.m_Name] = text
                elif obj.type.name == 'Texture2D':
                    data = obj.read()
                    if data.m_Name in textures:
                        raise ValueError('Ambiguous Texture2D: ' + data.m_Name)
                    textures[data.m_Name] = data.image.convert('RGBA')
        if target not in texts or target + '.atlas' not in texts or target not in textures:
            raise ValueError(f'Expected original resources not found for {target}; inspect resource keys.')
        skeleton = json.loads(texts[target])
        if not skeleton['skeleton']['spine'].startswith('4.1.'):
            raise ValueError('Unsupported Spine export version')
        write_json(destination / 'skeleton.json', skeleton)
        atlas_text = texts[target + '.atlas'].replace('\r\n', '\n').replace('\r', '\n')
        (destination / 'source.atlas').write_bytes(atlas_text.replace('\n', '\r\n').encode('utf-8'))
        textures[target].save(destination / 'source.png')
        metadata = run_runtime('atlas', {'atlas': str(destination / 'source.atlas')}, destination / 'atlas.json')
        if len(metadata['pages']) != 1 or metadata['pages'][0]['pma']:
            raise ValueError('Source needs a single straight-alpha page')
        validate_source = run_runtime('validate', runtime_request(destination, destination), destination / 'parse.json')
        write_json(record, {'target': target, 'jsonKey': target, 'spine': skeleton['skeleton']['spine'],
                            'sources': report['targets'][target], 'outputs': fingerprint([destination / p for p in
                            ['skeleton.json', 'source.atlas', 'source.png', 'atlas.json']]), 'runtime': runtime_fingerprint()})
        results[target] = validate_source
    return results


def runtime_request(original, candidate):
    candidate = Path(candidate)
    original = Path(original)
    is_original = (candidate / 'source.atlas').exists()
    if is_original:
        external, atlas, image = candidate / 'skeleton.json', candidate / 'source.atlas', candidate / 'source.png'
    else:
        manifests = list(candidate.glob('*.portrait.json'))
        if len(manifests) != 1:
            raise ValueError('Expected exactly one package manifest')
        manifest = read_json(manifests[0])
        external = within(candidate, candidate / manifest['json'])
        atlas = within(candidate, candidate / manifest['atlas'])
        image = within(candidate, atlas.parent / atlas.read_text(encoding='utf-8-sig').splitlines()[0])
    return {'original': str(original / 'skeleton.json'), 'external': str(external), 'atlas': str(atlas), 'image': str(image)}


def prepare(job):
    root = Path(job['outputDir'])
    for target in job['targets']:
        original = root / 'original' / target
        verify_source(original)
        output = root / 'prepared' / target
        (output / 'parts').mkdir(parents=True, exist_ok=True)
        (output / 'references').mkdir(parents=True, exist_ok=True)
        page = Image.open(original / 'source.png').convert('RGBA')
        regions = read_json(original / 'atlas.json')['regions']
        parts = {r['name']: unpack_region(page, r) for r in regions}
        if not roundtrip(page, regions, parts):
            raise ValueError('Lossless atlas roundtrip failed: ' + target)
        skeleton = read_json(original / 'skeleton.json')
        mapping = []
        by_key = {(s, sl, n): a for s, sl, n, a in entries(skeleton)}
        for index, region in enumerate(regions):
            name = region['name']
            part_path = output / 'parts' / f'{index:03}.png'
            parts[name].save(part_path)
            uses = []
            coverage = Image.new('L', (parts[name].width * 4, parts[name].height * 4))
            coverage_draw = ImageDraw.Draw(coverage)
            for skin, slot, attachment, data in entries(skeleton):
                if data.get('type', 'region') not in ('region', 'mesh', 'linkedmesh'):
                    continue
                if data.get('path', data.get('name', attachment)) != name:
                    continue
                chain, parent_data = [], data
                parent_skin = skin
                while parent_data.get('type') == 'linkedmesh':
                    parent_skin = parent_data.get('skin', 'default')
                    key = (parent_skin, slot, parent_data['parent'])
                    if key in chain:
                        raise ValueError('Linked mesh cycle')
                    chain.append(key)
                    parent_data = by_key[key]
                if parent_data.get('type') == 'mesh':
                    uvs = parent_data['uvs']
                    points = [(uvs[i] * coverage.width, uvs[i + 1] * coverage.height) for i in range(0, len(uvs), 2)]
                    triangles = parent_data['triangles']
                    for i in range(0, len(triangles), 3):
                        coverage_draw.polygon([points[v] for v in triangles[i:i + 3]], fill=255)
                else:
                    coverage_draw.rectangle((0, 0, coverage.width, coverage.height), fill=255)
                uses.append({'skin': skin, 'slot': slot, 'attachment': attachment, 'type': data.get('type', 'region'),
                             'parentChain': chain, 'vertices': len(parent_data.get('uvs', [])) // 2,
                             'weighted': len(parent_data.get('vertices', [])) != len(parent_data.get('uvs', []))})
            reference = parts[name].copy()
            if uses:
                mask = coverage.resize(reference.size, Image.Resampling.LANCZOS)
                reference.putalpha(ImageChops.multiply(reference.getchannel('A'), mask))
            reference_path = output / 'references' / f'{index:03}.png'
            reference.save(reference_path)
            mapping.append({'region': name, 'part': str(part_path.relative_to(output)),
                            'referencePart': str(reference_path.relative_to(output)), 'atlas': region,
                            'uses': uses, 'editable': False,
                            'anchors': {'left': [0, parts[name].height // 2], 'right': [parts[name].width, parts[name].height // 2]},
                            'anchorStatus': 'canvas references only; agent must mark anatomical seam anchors before design approval'})
        write_json(output / 'mapping.json', {'target': target, 'roundtripPassed': True, 'regions': mapping,
                                            'animations': list(skeleton.get('animations', {})),
                                            'deform': collect_deform(skeleton)})
        contact_sheet(parts, output / 'parts-sheet.png')
        frames = run_runtime('frames', {**runtime_request(original, original), 'setupOnly': True}, output / 'setup-frames.json')
        preview_pair(frames, frames, original / 'source.png', original / 'source.png', output / 'baseline', size=640)
    return {'passed': True}


def collect_deform(skeleton):
    result = []
    for animation, timelines in skeleton.get('animations', {}).items():
        for field in ('deform', 'attachments'):
            for skin, slots in timelines.get(field, {}).items():
                for slot, attachments in slots.items():
                    for attachment, data in attachments.items():
                        if field == 'deform' or 'deform' in data:
                            result.append({'animation': animation, 'attachment': '/'.join((skin, slot, attachment))})
    return result


def verify_source(original):
    source = read_json(Path(original) / 'source.json')
    if not verify_fingerprint(source['sources']) or not verify_fingerprint(source['outputs']):
        raise ValueError('Original resources changed; rerun extract and prepare.')


def design_identity(job):
    import hashlib
    payload = {k: job.get(k) for k in ('id', 'targets', 'description', 'referenceImages')}
    payload['references'] = fingerprint(job.get('referenceImages', []))
    return hashlib.sha256(json.dumps(payload, sort_keys=True, ensure_ascii=False).encode()).hexdigest()


def approve_design(job, evidence):
    value = read_json(evidence)
    if value.get('approvedBy') != 'user' or not value.get('userMessage'):
        raise ValueError('Approval must record the actual user confirmation; agents must not invent it.')
    if value.get('designIdentity') != design_identity(job):
        raise ValueError('Approval does not match this design request.')
    if not verify_fingerprint(value.get('conceptFiles')):
        raise ValueError('Concept files missing or changed.')
    if not value.get('allowedRegions') or set(value['allowedRegions']) != set(job['targets']):
        raise ValueError('Approval must specify editable regions for every target.')
    for target, names in value['allowedRegions'].items():
        mapping = read_json(Path(job['outputDir']) / 'prepared' / target / 'mapping.json')
        known = {r['region'] for r in mapping['regions']}
        if not names or not set(names).issubset(known):
            raise ValueError('Unknown/empty editable region list: ' + target)
    write_json(Path(job['outputDir']) / 'design-approval.json', value)
    return {'approved': True}


def require_design(job):
    path = Path(job['outputDir']) / 'design-approval.json'
    if not path.is_file():
        raise ValueError('Design is awaiting user confirmation; use approve-design with actual confirmation evidence.')
    value = read_json(path)
    if value['designIdentity'] != design_identity(job) or not verify_fingerprint(value['conceptFiles']):
        raise ValueError('Design changed since confirmation.')
    return value


def assemble(job, edits_path=None, baseline=False):
    root = Path(job['outputDir'])
    approval = None if baseline else require_design(job)
    if not baseline and not edits_path:
        raise ValueError('Provide --edits for a production candidate')
    edits = {} if baseline else read_json(edits_path)
    if not baseline and set(edits) != set(job['targets']):
        raise ValueError('Edits must cover exactly the job targets.')
    for target in job['targets']:
        original = root / 'original' / target
        verify_source(original)
        prepared = root / 'prepared' / target
        mapping = read_json(prepared / 'mapping.json')
        target_edits = edits.get(target, {})
        replacements = target_edits.get('parts', {})
        geometry = target_edits.get('geometry', [])
        allowed = set() if baseline else set(approval['allowedRegions'][target])
        if not set(replacements).issubset(allowed):
            raise ValueError('Attempt to replace a region outside the approved design.')
        regions = read_json(original / 'atlas.json')['regions']
        original_image = Image.open(original / 'source.png').convert('RGBA')
        parts = {r['name']: unpack_region(original_image, r) for r in regions}
        source_paths = []
        for region, path in replacements.items():
            path = (Path(edits_path).resolve().parent / path).resolve()
            image = Image.open(path)
            if image.mode != 'RGBA':
                raise ValueError('Replacement must have an explicit alpha channel: ' + str(path))
            if image.size != parts[region].size:
                raise ValueError('Replacement logical canvas mismatch: ' + region)
            mask_path = target_edits.get('preserveMasks', {}).get(region)
            if mask_path:
                mask_path = (Path(edits_path).resolve().parent / mask_path).resolve()
                mask = Image.open(mask_path).convert('L')
                if mask.size != image.size:
                    raise ValueError('Preservation mask canvas mismatch: ' + region)
                image = Image.composite(parts[region], image, mask)
                source_paths.append(mask_path)
            parts[region] = image.copy()
            source_paths.append(path)
        skeleton = read_json(original / 'skeleton.json')
        for edit in geometry:
            permitted = False
            for skin, slot, name, data in entries(skeleton):
                if '/'.join((skin, slot, name)) == edit['attachment']:
                    permitted = data.get('path', data.get('name', name)) in allowed
            if not permitted:
                raise ValueError('Geometry edit outside approved regions: ' + edit['attachment'])
        image, updated_regions = assemble_page(original_image, regions, parts, set(replacements))
        output = root / ('baseline-packages' if baseline else 'packages') / target
        output.mkdir(parents=True, exist_ok=True)
        image.save(output / (target + '.attachments.png'))
        write_atlas(output / (target + '.attachments.atlas'), target + '.attachments.png', image.size, updated_regions)
        if geometry:
            skeleton = run_runtime('geometry', {'original': str(original / 'skeleton.json'),
                                   'atlas': str(original / 'source.atlas'), 'edits': geometry}, output / (target + '.attachments.json'))
        write_json(output / (target + '.attachments.json'), skeleton)
        write_json(output / (target + '.portrait.json'), {'formatVersion': 1, 'id': job['id'] + '-' + target,
                   'target': target, 'jsonKey': target, 'json': target + '.attachments.json', 'atlas': target + '.attachments.atlas'})
        result = run_runtime('validate', runtime_request(original, output), root / 'qa' / target / ('baseline-parse.json' if baseline else 'parse.json'))
        write_json(root / 'qa' / target / ('baseline-assembly.json' if baseline else 'assembly.json'),
                   {'baseline': baseline, 'inputs': fingerprint(source_paths + ([Path(edits_path)] if edits_path else [])),
                    'package': fingerprint(package_files(output)), 'changedRegions': list(replacements),
                    'geometry': geometry, 'parse': result,
                    'approval': sha(root / 'design-approval.json') if not baseline else None})
    return {'assembled': job['targets'], 'baseline': baseline}


def validate(job, baseline=False, samples=9):
    root = Path(job['outputDir'])
    if not baseline:
        require_design(job)
    passed = True
    for target in job['targets']:
        original = root / 'original' / target
        verify_source(original)
        candidate = root / ('baseline-packages' if baseline else 'packages') / target
        output = root / 'qa' / target / ('baseline' if baseline else 'candidate')
        output.mkdir(parents=True, exist_ok=True)
        combinations_path = root / 'observations' / target / 'combinations.json'
        combinations = read_json(combinations_path)['combinations'] if combinations_path.exists() else []
        extra = {'samples': samples, 'combinations': combinations}
        a = run_runtime('frames', {**runtime_request(original, original), **extra}, output / 'original-frames.json')
        b = run_runtime('frames', {**runtime_request(original, candidate), **extra}, output / 'candidate-frames.json')
        report = geometry_report(a, b)
        report.update({'baseline': baseline, 'package': fingerprint(package_files(candidate)),
                       'runtime': runtime_fingerprint(), 'samplesPerAnimation': samples,
                       'observedCombinations': len(combinations), 'gameVerified': False,
                       'frameFiles': fingerprint([output / 'original-frames.json', output / 'candidate-frames.json'])})
        write_json(output / 'validation.json', report)
        passed &= report['passed']
    if not passed:
        raise ValueError('Geometry validation failed; see qa/<target>/*/validation.json')
    return {'passed': passed, 'baseline': baseline}


def preview(job, baseline=False):
    root = Path(job['outputDir'])
    for target in job['targets']:
        original = root / 'original' / target
        candidate = root / ('baseline-packages' if baseline else 'packages') / target
        output = root / 'qa' / target / ('baseline' if baseline else 'candidate')
        report = read_json(output / 'validation.json')
        if not verify_fingerprint(report['package']) or not verify_fingerprint(report['frameFiles']):
            raise ValueError('Candidate or frames changed; rerun validate.')
        request = runtime_request(original, candidate)
        preview_pair(read_json(output / 'original-frames.json'), read_json(output / 'candidate-frames.json'),
                     original / 'source.png', request['image'], output / 'preview')
    return {'previewed': job['targets']}


def record_review(job, evidence):
    """Store an agent's actual visual inspection; never turn numeric checks into visual approval."""
    root = Path(job['outputDir'])
    require_design(job)
    review = read_json(evidence)
    if review.get('reviewer') not in ('agent', 'user') or set(review.get('targets', {})) != set(job['targets']):
        raise ValueError('Review must identify reviewer and every target.')
    for target, value in review['targets'].items():
        expected = fingerprint(package_files(root / 'packages' / target))
        if value.get('package') != expected or not verify_fingerprint(value.get('images')):
            raise ValueError('Review evidence does not match current package and images.')
        if not value.get('checks') or value.get('passed') is not True:
            raise ValueError('Visual review has unresolved issues.')
    write_json(root / 'visual-review.json', review)
    return {'recorded': True}


def retry_issue(job, issue_id, stage, description):
    root = Path(job['outputDir'])
    path = root / 'issues.json'
    issues = read_json(path) if path.exists() else {}
    safe_name(issue_id)
    issue = issues.get(issue_id, {'attempts': 0})
    if issue['attempts'] >= 3:
        raise ValueError('Automatic repair limit reached; retain diagnostic and request design revision.')
    issue.update(attempts=issue['attempts'] + 1, stage=stage, description=description)
    issues[issue_id] = issue
    write_json(path, issues)
    return issue
