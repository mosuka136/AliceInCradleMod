import argparse
import sys
import traceback
from pathlib import Path
from wardrobe.common import load_job, read_json, write_json
from wardrobe import pipeline


def main():
    parser = argparse.ArgumentParser(description='Alice In Cradle Spine wardrobe pipeline')
    parser.add_argument('command', choices=['inspect', 'extract', 'prepare', 'assemble', 'validate', 'preview',
                        'approve-design', 'record-review', 'deploy', 'rollback', 'qa-stage', 'qa-request', 'qa-collect', 'retry-issue',
                        'make-sheet', 'ingest-sheet'])
    parser.add_argument('--job', required=True)
    parser.add_argument('--edits')
    parser.add_argument('--evidence')
    parser.add_argument('--baseline', action='store_true')
    parser.add_argument('--samples', type=int, default=9)
    parser.add_argument('--transaction')
    parser.add_argument('--issue')
    parser.add_argument('--stage')
    parser.add_argument('--description')
    parser.add_argument('--target')
    parser.add_argument('--regions', nargs='+')
    parser.add_argument('--image')
    parser.add_argument('--layout')
    parser.add_argument('--output')
    parser.add_argument('--key-green', action='store_true')
    parser.add_argument('--fit-cell', action='store_true')
    parser.add_argument('--isolated', action='store_true')
    parser.add_argument('--diagnostic', action='store_true', help='Temporary QA only; still requires approved design and valid geometry')
    parser.add_argument('--qa-operation', choices=['observe', 'render', 'refresh', 'effects'], default='observe')
    args = parser.parse_args()
    _, job = load_job(args.job)
    root = Path(job['outputDir'])
    root.mkdir(parents=True, exist_ok=True)
    state_path = root / 'checkpoint.json'
    state = read_json(state_path) if state_path.exists() else {'stages': {}}
    try:
        if args.command in ('inspect', 'extract', 'prepare'):
            result = getattr(pipeline, args.command)(job)
        elif args.command == 'assemble':
            result = pipeline.assemble(job, args.edits, args.baseline)
        elif args.command == 'validate':
            result = pipeline.validate(job, args.baseline, args.samples)
        elif args.command == 'preview':
            result = pipeline.preview(job, args.baseline)
        elif args.command == 'approve-design':
            result = pipeline.approve_design(job, args.evidence)
        elif args.command == 'record-review':
            result = pipeline.record_review(job, args.evidence)
        elif args.command == 'retry-issue':
            result = pipeline.retry_issue(job, args.issue, args.stage, args.description)
        elif args.command == 'make-sheet':
            from wardrobe.sheets import make_sheet
            if args.target not in job['targets']:
                raise ValueError('Choose a job target')
            make_sheet(root / 'prepared' / args.target / 'mapping.json', args.regions, args.output)
            result = {'sheet': args.output}
        elif args.command == 'ingest-sheet':
            from wardrobe.sheets import ingest_sheet
            pipeline.require_design(job)
            result = ingest_sheet(args.image, args.layout, args.output, args.key_green, args.fit_cell)
        elif args.command == 'qa-request':
            from wardrobe import deployment
            result = deployment.qa_request(job, args.transaction, args.qa_operation, args.isolated, args.diagnostic)
        elif args.command == 'qa-stage':
            from wardrobe import deployment
            result = deployment.qa_stage(job, args.transaction, args.diagnostic)
        else:
            from wardrobe import deployment
            result = getattr(deployment, args.command.replace('-', '_'))(job, args.transaction)
        key = args.command + ('-baseline' if args.baseline else '')
        state['stages'][key] = {'status': 'passed', 'result': result}
        write_json(state_path, state)
        print(f'{args.command}: completed; output={root}')
        return 0
    except Exception as error:
        key = args.command + ('-baseline' if args.baseline else '')
        state['stages'][key] = {'status': 'failed', 'reason': str(error)}
        write_json(state_path, state)
        traceback.print_exc()
        return 1


if __name__ == '__main__':
    sys.exit(main())
