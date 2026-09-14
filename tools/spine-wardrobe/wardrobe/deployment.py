"""Transactional installation and evidence-bound QA handoff."""
import ast
import os
from pathlib import Path
import re
import shutil
import time
import uuid
from .common import (fingerprint, package_files, read_json, safe_name, sha,
                     verify_fingerprint, within, write_json)
from .pipeline import require_design


def parse_rows(value):
    # The existing loader uses C# bool spellings; never evaluate config as code.
    rows = ast.literal_eval(re.sub(r'\btrue\b', 'True', re.sub(r'\bfalse\b', 'False', value, flags=re.I), flags=re.I))
    if not isinstance(rows, list) or any(not isinstance(row, tuple) or len(row) != 2
                                       or not isinstance(row[0], str) or type(row[1]) is not bool for row in rows):
        raise ValueError('EnabledPortraitPacks is not a list of (id, bool) tuples.')
    return rows


def update_config(text, enabled_ids, conflicting_ids):
    import json
    lines = text.replace('\r\n', '\n').splitlines()
    section = None
    touched = set()
    for i, line in enumerate(lines):
        match = re.match(r'^\s*\[([^]]+)\]\s*$', line)
        if match:
            section = match.group(1)
        if section != 'Texture' or line.lstrip().startswith(('#', ';')):
            continue
        match = re.match(r'^(\s*)(EnableReplacePortrait|EnabledPortraitPacks)\s*=\s*(.*)$', line)
        if not match:
            continue
        key = match.group(2)
        if key in touched:
            raise ValueError('Duplicate configuration key: ' + key)
        touched.add(key)
        if key == 'EnableReplacePortrait':
            value = 'True'
        else:
            rows = parse_rows(match.group(3))
            seen = set()
            selected = []
            for name, enabled in rows:
                if name in seen:
                    raise ValueError('Duplicate portrait config id: ' + name)
                seen.add(name)
                selected.append((name, True if name in enabled_ids else False if name in conflicting_ids else enabled))
            selected.extend((name, True) for name in enabled_ids if name not in seen)
            value = '[' + ','.join('(' + json.dumps(name, ensure_ascii=False) + ',' + str(flag) + ')' for name, flag in selected) + ']'
        lines[i] = match.group(1) + key + ' = ' + value
    if touched != {'EnableReplacePortrait', 'EnabledPortraitPacks'}:
        raise ValueError('Expected current BetterExperience [Texture] configuration keys.')
    return '\r\n'.join(lines) + '\r\n'


def no_reparse(root, path):
    """Validate lexical containment AND reparse points before any installation write."""
    root, path = Path(root).absolute(), Path(path).absolute()
    if not path.is_relative_to(root):
        raise ValueError('Destination escapes installation root')
    for current in (path, *path.parents):
        if current.exists() and (current.is_symlink() or current.is_junction()):
            raise ValueError('Refusing to write through reparse point: ' + str(current))
        if current == root:
            break
    return within(root, path)


def require_offline(job, visual=True):
    root = Path(job['outputDir'])
    approval = require_design(job)
    review = read_json(root / 'visual-review.json') if visual else None
    for target in job['targets']:
        expected = fingerprint(package_files(root / 'packages' / target))
        validation = read_json(root / 'qa' / target / 'candidate' / 'validation.json')
        assembly = read_json(root / 'qa' / target / 'assembly.json')
        if not validation['passed'] or validation['baseline'] or validation['package'] != expected:
            raise ValueError('Missing/stale successful offline validation: ' + target)
        if not verify_fingerprint(validation['runtime']) or not verify_fingerprint(validation['frameFiles']):
            raise ValueError('Runtime or sampled frames changed; validate again.')
        if assembly['approval'] != sha(root / 'design-approval.json') or assembly['package'] != expected:
            raise ValueError('Assembly does not match confirmed design.')
        if visual:
            item = review['targets'][target]
            if item['passed'] is not True or item['package'] != expected or not verify_fingerprint(item['images']):
                raise ValueError('Missing/stale visual review: ' + target)
    return approval


def qa_stage(job, transaction=None, diagnostic=False):
    require_offline(job, visual=not diagnostic)
    result = install_transaction(job, Path(job['outputDir']) / 'packages')
    result['purpose'] = 'temporary-game-qa; not production acceptance'
    result['diagnostic'] = diagnostic
    return result


def qa_request(job, transaction=None, operation='observe', isolated=False, diagnostic=False):
    """Queue a read-only live snapshot. Use a staged QA install for final lifecycle tests."""
    require_offline(job, visual=not diagnostic)
    root = Path(job['outputDir'])
    queue = root / 'game-qa'
    queue.mkdir(parents=True, exist_ok=True)
    request_id = uuid.uuid4().hex
    targets = []
    for target in job['targets']:
        targets.append({'target': target, 'packageId': job['id'] + '-' + target,
                        'package': fingerprint(package_files(root / 'packages' / target))})
    request = {'formatVersion': 1, 'id': request_id, 'operation': operation, 'targets': targets,
               'createdUnixSeconds': time.time(), 'expiresUnixSeconds': time.time() + 3600,
               'isolated': isolated,
               'diagnostic': diagnostic,
               'output': str(queue / request_id), 'timeoutSeconds': 900 if operation == 'render' else 300,
               'note': 'Capture actual live viewer states. Unseen states remain unverified.'}
    write_json(queue / 'request.json', request)
    return {'requestId': request_id, 'bridgeRoot': str(queue), 'requires': 'Game running with Wardrobe.QA plugin and candidate staged for testing'}


def qa_collect(job, transaction=None):
    root = Path(job['outputDir'])
    queue = root / 'game-qa'
    request = read_json(queue / 'request.json')
    response = read_json(queue / request['id'] / 'result.json')
    if response.get('requestId') != request['id']:
        raise ValueError('QA response identity mismatch')
    expected = {t['target']: t for t in request['targets']}
    if set(expected) != set(job['targets']):
        raise ValueError('QA target set changed.')
    for target, item in expected.items():
        if item['package'] != fingerprint(package_files(root / 'packages' / target)):
            raise ValueError('Candidate changed after QA request: ' + target)
    if response.get('errors'):
        raise ValueError('Game QA reported errors; inspect result.json.')
    for item in response.get('observations', []):
        if item['target'] not in expected or item.get('activePackageId') != expected[item['target']]['packageId']:
            raise ValueError('QA captured a different active costume.')
        if item.get('scope', 'live-game-ui') == 'live-game-ui':
            source = read_json(root / 'original' / item['target'] / 'skeleton.json')
            known_animations = set(source.get('animations', {}))
            raw_skins = source.get('skins', [])
            known_skins = set(raw_skins) if isinstance(raw_skins, dict) else {skin['name'] for skin in raw_skins}
            if set(item.get('animations', [])) - known_animations or set(item.get('skins', [])) - known_skins:
                raise ValueError('QA state does not belong to this target; check shared animator binding.')
    screenshots = response.get('screenshots', {})
    metadata_only = (request.get('isolated') is True and request.get('operation') in ('observe', 'refresh')
                     and bool(response.get('observations')))
    if not isinstance(screenshots, dict) or (screenshots and not verify_fingerprint(screenshots)):
        raise ValueError('QA screenshots missing or altered')
    if not screenshots and not metadata_only:
        raise ValueError('QA screenshots missing or altered')
    for target in job['targets']:
        observations = [o for o in response.get('observations', []) if o['target'] == target and o.get('scope', 'live-game-ui') == 'live-game-ui']
        destination = root / 'observations' / target / 'combinations.json'
        previous = read_json(destination).get('combinations', []) if destination.exists() else []
        combinations = {(tuple(item['skins']), tuple(item['animations'])): item for item in previous}
        for item in observations:
            key = (tuple(item['skins']), tuple(item['animations']))
            if key not in combinations:
                combinations[key] = {'name': 'observed-' + str(len(combinations)), 'skins': item['skins'], 'animations': item['animations']}
        write_json(destination, {'combinations': list(combinations.values())})
    write_json(root / 'game-qa' / 'collected.json', response)
    return {'captured': len(response.get('observations', [])), 'complete': response.get('complete', False),
            'evidenceKind': 'screenshots-and-metadata' if screenshots else 'metadata-only'}


def deploy(job, transaction=None):
    """Install only fully verified candidates. A QA staging command is separate."""
    require_offline(job)
    root = Path(job['outputDir'])
    evidence = read_json(root / 'game-qa' / 'acceptance.json')
    if evidence.get('passed') is not True or not verify_fingerprint(evidence.get('evidenceFiles')):
        raise ValueError('Game acceptance is absent/stale; a snapshot is not full acceptance.')
    required = {'animations', 'skins', 'observedCombinations', 'effects', 'refresh10', 'disableRestore',
                'switch', 'conflict', 'corruptionFallback', 'resourceLifetime'}
    for target in job['targets']:
        item = evidence['targets'][target]
        if item['package'] != fingerprint(package_files(root / 'packages' / target)):
            raise ValueError('Game QA ran against another package.')
        if not required.issubset(item.get('checks', {})) or any(item['checks'][k] is not True for k in required):
            raise ValueError('Incomplete game acceptance: ' + target)
    if transaction:
        safe_name(transaction)
        path = root / 'transactions' / transaction / 'transaction.json'
        record = read_json(path)
        if Path(record['gameDir']).resolve() != Path(job['gameDir']).resolve() or record['status'].startswith('rolled-back'):
            raise ValueError('Cannot promote this QA transaction.')
        for item in record['files']:
            destination = no_reparse(Path(job['gameDir']), Path(item['destination']))
            if not destination.is_file() or sha(destination) != item['after']:
                raise ValueError('QA installation changed before promotion.')
        installed = {Path(item['destination']).name: item['after'] for item in record['files']}
        for target in job['targets']:
            for source in package_files(root / 'packages' / target):
                if installed.get(source.name) != sha(source):
                    raise ValueError('QA transaction contains another candidate.')
        record['status'] = 'accepted'
        record['acceptanceSha256'] = sha(root / 'game-qa' / 'acceptance.json')
        write_json(path, record)
        return {'transactionId': transaction, 'status': 'accepted', 'rollbackPreservesPreQaState': True}
    return install_transaction(job, root / 'packages')


def install_transaction(job, packages):
    root = Path(job['outputDir'])
    game = Path(job['gameDir'])
    plugin = game / 'BepInEx/plugins/BetterExperience'
    replace = plugin / 'ReplaceTexture'
    config = no_reparse(game, plugin / 'BetterExperience.cfg')
    selected, conflicts = [], set()
    for manifest_path in replace.rglob('*.portrait.json'):
        if manifest_path.is_symlink():
            continue
        manifest = read_json(manifest_path)
        if manifest.get('target') in job['targets']:
            conflicts.add(manifest['id'])
    files = []
    for target in job['targets']:
        directory = Path(packages) / target
        manifest = read_json(directory / (target + '.portrait.json'))
        selected.append(manifest['id'])
        for source in package_files(directory):
            destination = no_reparse(game, replace / job['id'] / target / source.name)
            files.append((source, destination))
    text = config.read_text(encoding='utf-8-sig')
    updated = update_config(text, selected, conflicts)
    transaction_id = uuid.uuid4().hex
    backup = root / 'transactions' / transaction_id
    backup.mkdir(parents=True)
    record = {'id': transaction_id, 'gameDir': str(game), 'status': 'prepared', 'files': []}
    for index, (source, destination) in enumerate(files + [(None, config)]):
        existed = destination.exists()
        saved = backup / f'{index:03}.bin'
        if existed:
            shutil.copy2(destination, saved)
        record['files'].append({'destination': str(destination), 'backup': str(saved), 'existed': existed,
                                'before': sha(destination) if existed else None, 'after': None})
    write_json(backup / 'transaction.json', record)
    try:
        for (source, destination), item in zip(files + [(None, config)], record['files']):
            no_reparse(game, destination)
            destination.parent.mkdir(parents=True, exist_ok=True)
            temporary = destination.with_name(destination.name + '.wardrobe-' + transaction_id + '.tmp')
            if source is not None:
                shutil.copyfile(source, temporary)
            else:
                temporary.write_bytes(updated.encode('utf-8'))
            item['after'] = sha(temporary)
            write_json(backup / 'transaction.json', record)
            os.replace(temporary, destination)
        record['status'] = 'installed-awaiting-safe-switch'
        write_json(backup / 'transaction.json', record)
    except Exception:
        # Rollback only files this transaction wrote; preserve concurrent changes.
        rollback_record(record, game)
        record['status'] = 'rolled-back-after-failure'
        write_json(backup / 'transaction.json', record)
        raise
    return {'transactionId': transaction_id, 'status': record['status'], 'next': 'Reload config, refresh textures, then verify next safe portrait switch'}


def rollback_record(record, game):
    for item in reversed(record['files']):
        path = no_reparse(game, Path(item['destination']))
        if item['after'] is None:
            continue
        current = sha(path) if path.is_file() else None
        if current == item['before']:
            continue
        if current != item['after']:
            raise ValueError('Concurrent change; refusing to overwrite during rollback: ' + str(path))
        if item['existed']:
            if sha(item['backup']) != item['before']:
                raise ValueError('Backup damaged: ' + item['backup'])
            shutil.copyfile(item['backup'], path)
        else:
            path.unlink()


def rollback(job, transaction=None):
    if not transaction:
        raise ValueError('Provide --transaction ID; never guess which installation to undo.')
    safe_name(transaction)
    path = Path(job['outputDir']) / 'transactions' / transaction / 'transaction.json'
    record = read_json(path)
    if Path(record['gameDir']).resolve() != Path(job['gameDir']).resolve():
        raise ValueError('Transaction belongs to another installation.')
    if record['status'].startswith('rolled-back'):
        return {'transactionId': transaction, 'status': record['status']}
    rollback_record(record, Path(job['gameDir']))
    record['status'] = 'rolled-back'
    write_json(path, record)
    return {'transactionId': transaction, 'status': 'rolled-back'}
