import json
from pathlib import Path
import shutil
import sys
import unittest
import uuid
from unittest.mock import patch
import numpy as np
from PIL import Image

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from wardrobe.atlas import unpack_region, roundtrip, assemble_page, write_atlas
from wardrobe.common import TOOL, read_json, run_runtime, write_json, fingerprint
from wardrobe.deployment import update_config, parse_rows, install_transaction, rollback, no_reparse, qa_collect, deploy
from wardrobe.pipeline import require_design, retry_issue
from wardrobe.render import geometry_report
from wardrobe.sheets import green_matte, ingest_sheet


class WorkDirectory(unittest.TestCase):
    def setUp(self):
        self.directory = TOOL / 'work' / 'tests' / uuid.uuid4().hex
        self.directory.mkdir(parents=True)

    def tearDown(self):
        # Explicitly checked project-local temporary directory, never the game.
        if not self.directory.resolve().is_relative_to((TOOL / 'work' / 'tests').resolve()):
            raise ValueError('Unsafe test cleanup')
        shutil.rmtree(self.directory)


class AtlasTests(WorkDirectory):
    def test_fit_cell_recovers_enlarged_part_without_cropping_edges(self):
        original = self.directory / 'original.png'
        Image.new('RGBA', (4, 6), (255, 255, 255, 255)).save(original)
        generated = Image.new('RGBA', (40, 40))
        generated.paste((255, 0, 0, 255), (8, 4, 32, 36))
        source = self.directory / 'generated.png'
        generated.save(source)
        layout = self.directory / 'layout.json'
        write_json(layout, {'sheetSize': [40, 40], 'placements': [{'region': 'cloth',
            'original': str(original), 'originalSha256': fingerprint([original])[str(original)],
            'box': [18, 17, 22, 23], 'cellBox': [0, 0, 40, 40], 'logicalSize': [4, 6]}]})
        result = ingest_sheet(source, layout, self.directory / 'selected', fit_cell=True)
        image = Image.open(result['cloth'])
        self.assertEqual((4, 6), image.size)
        self.assertEqual((255, 0, 0, 255), image.getpixel((0, 0)))
        self.assertEqual((255, 0, 0, 255), image.getpixel((3, 5)))

    def test_green_matte_preserves_black_white_and_soft_edges(self):
        image = Image.new('RGB', (8, 8), (0, 255, 0))
        image.putpixel((2, 2), (0, 0, 0))
        image.putpixel((3, 2), (255, 255, 255))
        image.putpixel((4, 2), (0, 128, 0))
        result = green_matte(image)
        self.assertEqual(0, result.getpixel((0, 0))[3])
        self.assertEqual((0, 0, 0, 255), result.getpixel((2, 2)))
        self.assertEqual((255, 255, 255, 255), result.getpixel((3, 2)))
        self.assertIn(result.getpixel((4, 2))[3], [126, 127])

    def test_green_matte_removes_uneven_saturated_background(self):
        image = Image.new('RGB', (8, 8), (0, 255, 0))
        image.putpixel((0, 0), (12, 210, 9))
        image.putpixel((1, 0), (4, 180, 12))
        image.putpixel((2, 0), (28, 25, 24))
        result = green_matte(image)
        self.assertEqual([0, 0, 255], [result.getpixel((x, 0))[3] for x in range(3)])

    def test_rotation_offsets_roundtrip_preserves_rgba_including_transparent_rgb(self):
        pixels = np.arange(12 * 10 * 4, dtype=np.uint8).reshape(10, 12, 4)
        page = Image.fromarray(pixels)
        r = dict(name='part', x=2, y=1, width=3, height=5, degrees=90,
                 originalWidth=9, originalHeight=8, offsetX=2, offsetY=3)
        part = unpack_region(page, r)
        self.assertEqual((9, 8), part.size)
        self.assertEqual(tuple(pixels[1, 2]), part.getpixel((6, 2)))
        self.assertTrue(roundtrip(page, [r], {'part': part}))

    def test_expanded_alpha_repack_retains_logical_canvas(self):
        page = Image.new('RGBA', (16, 16))
        page.paste((255, 0, 0, 255), (2, 2, 6, 6))
        r = dict(name='dress', x=2, y=2, width=4, height=4, degrees=0,
                 originalWidth=8, originalHeight=8, offsetX=2, offsetY=2)
        part = unpack_region(page, r)
        part.putpixel((0, 0), (0, 255, 0, 255))
        packed, regions = assemble_page(page, [r], {'dress': part}, {'dress'}, max_size=64)
        restored = unpack_region(packed, regions[0])
        self.assertTrue(np.array_equal(restored, part))
        atlas = self.directory / 'test.atlas'
        write_atlas(atlas, 'test.png', packed.size, regions)
        metadata = run_runtime('atlas', {'atlas': str(atlas)}, self.directory / 'metadata.json')
        self.assertEqual(regions[0]['width'], metadata['regions'][0]['width'])

    def test_wrong_part_canvas_rejected(self):
        r = dict(name='part', x=0, y=0, width=4, height=4, degrees=0,
                 originalWidth=4, originalHeight=4, offsetX=0, offsetY=0)
        with self.assertRaisesRegex(ValueError, 'logical canvas'):
            assemble_page(Image.new('RGBA', (8, 8)), [r], {'part': Image.new('RGBA', (5, 5))}, {'part'})


class DeploymentTests(WorkDirectory):
    CONFIG = '[Texture]\r\nEnableReplacePortrait = False\r\nEnabledPortraitPacks = [("old",True),("other",True)]\r\n\r\n[Other]\r\nValue = keep\r\n'

    def make_job(self):
        game = self.directory / 'game'
        plugin = game / 'BepInEx/plugins/BetterExperience'
        (plugin / 'ReplaceTexture/old').mkdir(parents=True)
        write_json(plugin / 'ReplaceTexture/old/old.portrait.json', {'id': 'old', 'target': 'stand_normal'})
        (plugin / 'BetterExperience.cfg').write_bytes(self.CONFIG.encode())
        package = self.directory / 'work/packages/stand_normal'
        package.mkdir(parents=True)
        write_json(package / 'stand_normal.portrait.json', {'id': 'new-stand_normal', 'target': 'stand_normal'})
        (package / 'stand_normal.attachments.png').write_bytes(b'test fixture')
        write_json(self.directory / 'work/original/stand_normal/skeleton.json', {
            'skins': [{'name': 'default'}], 'animations': {'stand': {}, 'mabataki': {}}})
        return {'gameDir': str(game), 'outputDir': str(self.directory / 'work'), 'id': 'new', 'targets': ['stand_normal']}

    def test_config_changes_only_target_selection(self):
        text = update_config(self.CONFIG, ['new'], {'old'})
        self.assertIn('("old",False),("other",True),("new",True)', text)
        self.assertIn('[Other]\r\nValue = keep', text)
        self.assertIn('EnableReplacePortrait = True', text)

    def test_config_rejects_executable_or_malformed_input(self):
        for text in ["__import__('os').system('echo bad')", '["id"]', '[("id",1)]']:
            with self.assertRaises((ValueError, SyntaxError)):
                parse_rows(text)

    def test_install_and_rollback_restore_exact_bytes(self):
        job = self.make_job()
        original = Path(job['gameDir']) / 'BepInEx/plugins/BetterExperience/BetterExperience.cfg'
        before = original.read_bytes()
        result = install_transaction(job, Path(job['outputDir']) / 'packages')
        installed = Path(job['gameDir']) / 'BepInEx/plugins/BetterExperience/ReplaceTexture/new/stand_normal/stand_normal.attachments.png'
        self.assertTrue(installed.exists())
        rollback(job, result['transactionId'])
        self.assertEqual(before, original.read_bytes())
        self.assertFalse(installed.exists())
        self.assertEqual('rolled-back', rollback(job, result['transactionId'])['status'])

    def test_rollback_refuses_concurrent_user_config_changes(self):
        job = self.make_job()
        result = install_transaction(job, Path(job['outputDir']) / 'packages')
        config = Path(job['gameDir']) / 'BepInEx/plugins/BetterExperience/BetterExperience.cfg'
        config.write_text('user edited', encoding='utf-8')
        with self.assertRaisesRegex(ValueError, 'Concurrent change'):
            rollback(job, result['transactionId'])
        self.assertEqual('user edited', config.read_text())

    def test_install_failure_rolls_back_already_written_files(self):
        job = self.make_job()
        import wardrobe.deployment as module
        original_replace = module.os.replace
        def fail_config(src, dst):
            if str(dst).endswith('BetterExperience.cfg'):
                raise OSError('simulated config write failure')
            return original_replace(src, dst)
        with patch.object(module.os, 'replace', side_effect=fail_config):
            with self.assertRaises(OSError):
                install_transaction(job, Path(job['outputDir']) / 'packages')
        root = Path(job['gameDir']) / 'BepInEx/plugins/BetterExperience'
        self.assertEqual(self.CONFIG.encode(), (root / 'BetterExperience.cfg').read_bytes())
        self.assertFalse((root / 'ReplaceTexture/new/stand_normal/stand_normal.attachments.png').exists())

    def test_path_escape_rejected(self):
        with self.assertRaises(ValueError):
            no_reparse(self.directory / 'game', self.directory / 'outside')

    def test_qa_collect_rejects_another_active_costume(self):
        job = self.make_job()
        root = Path(job['outputDir'])
        package = fingerprint((root / 'packages/stand_normal').glob('*'))
        queue = root / 'game-qa'
        write_json(queue / 'request.json', {'id': 'test', 'targets': [
            {'target': 'stand_normal', 'packageId': 'new-stand_normal', 'package': package}]})
        screenshot = root / 'screenshot.png'
        screenshot.write_bytes(b'screenshot fixture')
        write_json(queue / 'test/result.json', {'requestId': 'test',
            'screenshots': fingerprint([screenshot]), 'observations': [
                {'target': 'stand_normal', 'activePackageId': 'old'}]})
        with self.assertRaisesRegex(ValueError, 'different active costume'):
            qa_collect(job)
        self.assertFalse((queue / 'collected.json').exists())

    def test_promoting_qa_transaction_keeps_pre_install_backup(self):
        job = self.make_job()
        root = Path(job['outputDir'])
        result = install_transaction(job, root / 'packages')
        evidence = root / 'evidence.txt'
        evidence.write_text('test evidence', encoding='utf-8')
        names = ['animations', 'skins', 'observedCombinations', 'effects', 'refresh10',
                 'disableRestore', 'switch', 'conflict', 'corruptionFallback', 'resourceLifetime']
        write_json(root / 'game-qa/acceptance.json', {'passed': True,
            'evidenceFiles': fingerprint([evidence]), 'targets': {'stand_normal': {
                'package': fingerprint((root / 'packages/stand_normal').glob('*')),
                'checks': {name: True for name in names}}}})
        with patch('wardrobe.deployment.require_offline'):
            promoted = deploy(job, result['transactionId'])
        self.assertEqual(result['transactionId'], promoted['transactionId'])
        self.assertEqual('accepted', promoted['status'])
        rollback(job, result['transactionId'])
        config = Path(job['gameDir']) / 'BepInEx/plugins/BetterExperience/BetterExperience.cfg'
        self.assertEqual(self.CONFIG.encode(), config.read_bytes())

    def test_isolated_collection_preserves_previous_live_combinations(self):
        job = self.make_job()
        root = Path(job['outputDir'])
        queue = root / 'game-qa'
        package = fingerprint((root / 'packages/stand_normal').glob('*'))
        write_json(queue / 'request.json', {'id': 'test', 'targets': [
            {'target': 'stand_normal', 'packageId': 'new-stand_normal', 'package': package}]})
        screenshot = root / 'frame.png'
        screenshot.write_bytes(b'fixture')
        previous = {'combinations': [{'name': 'observed-0', 'skins': ['default'], 'animations': ['stand', 'mabataki']}]}
        destination = root / 'observations/stand_normal/combinations.json'
        write_json(destination, previous)
        write_json(queue / 'test/result.json', {'requestId': 'test', 'screenshots': fingerprint([screenshot]),
            'observations': [{'target': 'stand_normal', 'activePackageId': 'new-stand_normal',
                'scope': 'isolated-game-viewer', 'skins': ['default'], 'animations': ['stand']}]})
        qa_collect(job)
        self.assertEqual(previous, read_json(destination))

    def test_diagnostic_stage_relaxes_visual_review_only(self):
        from wardrobe.deployment import qa_stage
        job = self.make_job()
        with patch('wardrobe.deployment.require_offline') as gate:
            qa_stage(job, diagnostic=True)
        gate.assert_called_once_with(job, visual=False)
        with patch('wardrobe.deployment.require_offline', side_effect=ValueError('geometry failed')):
            with self.assertRaisesRegex(ValueError, 'geometry failed'):
                qa_stage(job, diagnostic=True)

    def test_refresh_metadata_collects_without_claiming_visual_evidence(self):
        job = self.make_job()
        root = Path(job['outputDir'])
        queue = root / 'game-qa'
        request = {'id': 'test', 'isolated': True, 'operation': 'refresh', 'targets': [
            {'target': 'stand_normal', 'packageId': 'new-stand_normal',
             'package': fingerprint((root / 'packages/stand_normal').glob('*'))}]}
        write_json(queue / 'request.json', request)
        write_json(queue / 'test/result.json', {'requestId': 'test', 'screenshots': {}, 'complete': False,
            'observations': [{'target': 'stand_normal', 'activePackageId': 'new-stand_normal',
                'scope': 'isolated-game-viewer', 'skins': ['default'], 'animations': ['stand']}]})
        result = qa_collect(job)
        self.assertEqual('metadata-only', result['evidenceKind'])
        self.assertFalse(result['complete'])
        request['operation'] = 'render'
        write_json(queue / 'request.json', request)
        with self.assertRaisesRegex(ValueError, 'screenshots missing'):
            qa_collect(job)

    def test_wrong_shared_animator_state_is_rejected_before_saving_combinations(self):
        job = self.make_job()
        root = Path(job['outputDir'])
        queue = root / 'game-qa'
        write_json(queue / 'request.json', {'id': 'test', 'targets': [
            {'target': 'stand_normal', 'packageId': 'new-stand_normal',
             'package': fingerprint((root / 'packages/stand_normal').glob('*'))}]})
        write_json(queue / 'test/result.json', {'requestId': 'test', 'observations': [
            {'target': 'stand_normal', 'activePackageId': 'new-stand_normal', 'scope': 'live-game-ui',
             'skins': ['default'], 'animations': ['only-on-another-skeleton']}]})
        with self.assertRaisesRegex(ValueError, 'shared animator'):
            qa_collect(job)
        self.assertFalse((root / 'observations/stand_normal/combinations.json').exists())


class GateAndGeometryTests(WorkDirectory):
    def test_no_design_approval_stops_production(self):
        with self.assertRaisesRegex(ValueError, 'awaiting user'):
            require_design({'outputDir': str(self.directory)})

    def test_retry_limit_persists(self):
        job = {'outputDir': str(self.directory)}
        for i in range(3):
            self.assertEqual(i + 1, retry_issue(job, 'sleeve-gap', 'image', 'gap')['attempts'])
        with self.assertRaisesRegex(ValueError, 'limit'):
            retry_issue(job, 'sleeve-gap', 'image', 'gap')

    def test_inverted_triangle_reports_attachment_and_pose(self):
        draw = dict(slot='cloth', attachment='dress', vertices=[0, 0, 1, 0, 0, 1], triangles=[0, 1, 2])
        frame = dict(name='stand', time=0, bones=[[0, 0, 1, 0, 0, 1]], draws=[draw])
        candidate = json.loads(json.dumps(frame))
        candidate['draws'][0]['vertices'] = [0, 0, 0, 1, 1, 0]
        result = geometry_report({'frames': [frame]}, {'frames': [candidate]})
        self.assertFalse(result['passed'])
        self.assertIn('cloth', result['errors'][0])


if __name__ == '__main__':
    unittest.main()
