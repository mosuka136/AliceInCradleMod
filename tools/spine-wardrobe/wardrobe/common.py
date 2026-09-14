"""Shared path, provenance and exact-runtime helpers."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import uuid

TOOL = Path(__file__).resolve().parents[1]
REPO = TOOL.parents[1]
RUNTIME = TOOL / 'Runtime/bin/Debug/net8.0/Wardrobe.Runtime.dll'


def read_json(path):
    return json.loads(Path(path).read_text(encoding='utf-8-sig'))


def write_json(path, value):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    text = json.dumps(value, ensure_ascii=False, indent=4, allow_nan=False) + '\n'
    temporary = path.with_suffix(path.suffix + '.tmp')
    temporary.write_bytes(text.replace('\n', '\r\n').encode('utf-8'))
    os.replace(temporary, path)


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def fingerprint(paths):
    return {str(Path(p).resolve()): sha(p) for p in sorted(map(Path, paths))}


def verify_fingerprint(value):
    return bool(value) and all(Path(p).is_file() and sha(p) == h for p, h in value.items())


def within(root, path):
    root, path = Path(root).resolve(), Path(path).resolve()
    if not path.is_relative_to(root):
        raise ValueError(f'Path escapes {root}: {path}')
    return path


def safe_name(name):
    if not name or name in ('.', '..') or any(c not in 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-' for c in name):
        raise ValueError(f'Use an ASCII resource identifier: {name!r}')
    return name


def run_runtime(command, request, destination):
    if not RUNTIME.is_file():
        raise RuntimeError('Build Runtime/Runtime.csproj first (see README).')
    destination = Path(destination).resolve()
    destination.parent.mkdir(parents=True, exist_ok=True)
    request = {**request, 'output': str(destination)}
    # Request files are outside the asset package and may contain absolute input paths.
    scratch = TOOL / 'work' / 'runtime-requests'
    scratch.mkdir(parents=True, exist_ok=True)
    path = scratch / (uuid.uuid4().hex + '.json')
    try:
        write_json(path, request)
        result = subprocess.run(['dotnet', str(RUNTIME), command, str(path)], capture_output=True, text=True)
        if result.returncode:
            raise RuntimeError(result.stderr.strip() or result.stdout.strip())
    finally:
        path.unlink(missing_ok=True)
    return read_json(destination)


def entries(skeleton):
    for skin in skeleton['skins']:
        for slot, attachments in skin.get('attachments', {}).items():
            for name, attachment in attachments.items():
                yield skin['name'], slot, name, attachment


def load_job(path):
    path = Path(path).resolve()
    job = read_json(path)
    job['gameDir'] = str(Path(job['gameDir']).resolve())
    job['outputDir'] = str((path.parent / job['outputDir']).resolve())
    safe_name(job['id'])
    if not job.get('targets') or len(set(job['targets'])) != len(job['targets']):
        raise ValueError('targets must be a nonempty unique list.')
    for target in job['targets']:
        safe_name(target)
    if Path(job['outputDir']).is_relative_to(Path(job['gameDir'])):
        raise ValueError('Working output must be outside the game directory.')
    return path, job


def package_files(directory):
    return sorted(p for p in Path(directory).glob('*') if p.is_file())


def runtime_fingerprint():
    shared = REPO / 'BetterExperience/Patches/ReplaceTexture'
    return fingerprint([RUNTIME, REPO / 'ReferenceLibrary/spine-unity.dll',
                        *[shared / (name + '.cs') for name in ['PortraitJson', 'PortraitCatalog', 'PortraitMerger']]])
