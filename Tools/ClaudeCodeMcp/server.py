import json
import os
from pathlib import Path
import re
import signal
import subprocess
import sys
import time
import uuid

from mcp.server.fastmcp import FastMCP

ROOT = Path(__file__).resolve().parents[2]
JOBS = ROOT / 'Library' / 'ClaudeCodeMcp'
JOBS.mkdir(parents=True, exist_ok=True)
SERVER = FastMCP('claude-code-glm')


def job_path(job_id: str) -> Path:
    if re.fullmatch(r'[a-f0-9]{32}', job_id) is None:
        raise ValueError('Invalid job ID')
    path = JOBS / job_id
    if path.is_dir() is False:
        raise ValueError('Unknown job ID')
    return path


def read_json(path: Path) -> dict:
    return json.loads(path.read_text())


def write_json(path: Path, value: dict) -> None:
    temporary = path.with_suffix('.tmp')
    temporary.write_text(json.dumps(value, ensure_ascii=False, indent=2))
    temporary.replace(path)


@SERVER.tool()
def claude_code_start(prompt: str, effort: str = 'high', model: str = 'glm-5.3-flash[1M]',
                      resume_session: str = '') -> dict:
    """Start an asynchronous GLM coding task through the installed Claude Code CLI in this repository."""
    if effort not in ['low', 'medium', 'high', 'xhigh', 'max']:
        raise ValueError('Unsupported effort')
    if len(prompt.strip()) == 0:
        raise ValueError('Prompt is empty')
    if resume_session:
        uuid.UUID(resume_session)
    job_id = uuid.uuid4().hex
    path = JOBS / job_id
    path.mkdir()
    (path / 'prompt.txt').write_text(prompt)
    write_json(path / 'state.json', {
        'job_id': job_id, 'status': 'starting', 'model': model,
        'effort': effort, 'resume_session': resume_session,
        'session_id': resume_session or str(uuid.uuid4()), 'started_at': time.time()
    })
    with (path / 'worker.log').open('ab') as output:
        worker = subprocess.Popen([sys.executable, str(Path(__file__).resolve()), '--worker', job_id],
                                  cwd=ROOT, stdin=subprocess.DEVNULL, stdout=output, stderr=output,
                                  start_new_session=True)
    return {'job_id': job_id, 'worker_pid': worker.pid, 'status': 'starting'}


@SERVER.tool()
def claude_code_status(job_id: str, max_chars: int = 8000) -> dict:
    """Read task status and the latest assistant text/tool summaries without blocking."""
    path = job_path(job_id)
    state = read_json(path / 'state.json')
    max_chars = max(200, min(max_chars, 24000))
    output = path / 'events.jsonl'
    messages = []
    if output.exists():
        with output.open('rb') as stream:
            stream.seek(max(0, output.stat().st_size - 1000000))
            lines = stream.read().decode('utf-8', errors='replace').splitlines()
        for line in lines:
            try:
                event = json.loads(line)
            except ValueError:
                continue
            if event.get('type') == 'system' and event.get('subtype') == 'init':
                state['session_id'] = event.get('session_id', '')
            if event.get('type') == 'assistant':
                for block in event.get('message', {}).get('content', []):
                    if block.get('type') == 'text':
                        messages.append(block.get('text', ''))
                    if block.get('type') == 'tool_use':
                        arguments = block.get('input', {})
                        messages.append('Tool: ' + block.get('name', '') + ' ' +
                                        str(arguments.get('file_path', arguments.get('description', ''))))
            if event.get('type') == 'result':
                state['session_id'] = event.get('session_id', '')
                state['is_error'] = event.get('is_error', False)
                messages.append(event.get('result', ''))
    state['output'] = '\n'.join(messages)[-max_chars:]
    errors = path / 'stderr.log'
    if errors.exists():
        state['stderr'] = errors.read_text(errors='replace')[-2000:]
    return state


@SERVER.tool()
def claude_code_cancel(job_id: str) -> dict:
    """Stop a running task's Claude Code subprocess group, preserving logs and session history."""
    path = job_path(job_id)
    state = read_json(path / 'state.json')
    if state['status'] not in ['starting', 'running']:
        return state
    (path / 'cancelled').touch()
    pid = state.get('claude_pid')
    if pid is not None:
        try:
            os.killpg(pid, signal.SIGTERM)
        except ProcessLookupError:
            pass
    state['status'] = 'cancelled'
    write_json(path / 'state.json', state)
    return state


def run_worker(job_id: str) -> None:
    path = job_path(job_id)
    state = read_json(path / 'state.json')
    arguments = ['claude', '--print', '--verbose', '--output-format', 'stream-json',
                 '--effort', state['effort'], '--model', state['model'],
                 '--permission-mode', 'acceptEdits', '--allowedTools',
                 'Read,Write,Edit,Glob,Grep,Bash', '--strict-mcp-config',
                 '--mcp-config', '{"mcpServers":{}}']
    if state['resume_session']:
        arguments.extend(['--resume', state['resume_session']])
    else:
        arguments.extend(['--session-id', state['session_id']])
    environment = os.environ.copy()
    environment.pop('CLAUDECODE', None)
    environment['CLAUDE_CODE_EFFORT_LEVEL'] = state['effort']
    environment.setdefault('MAX_THINKING_TOKENS', '8192')
    if (path / 'cancelled').exists():
        state['status'] = 'cancelled'
        write_json(path / 'state.json', state)
        return
    try:
        with (path / 'events.jsonl').open('ab') as output, (path / 'stderr.log').open('ab') as errors:
            process = subprocess.Popen(arguments, cwd=ROOT, env=environment, stdin=subprocess.PIPE,
                                       stdout=output, stderr=errors, start_new_session=True)
            state['claude_pid'] = process.pid
            state['status'] = 'running'
            write_json(path / 'state.json', state)
            if (path / 'cancelled').exists():
                os.killpg(process.pid, signal.SIGTERM)
            prompt_text = (path / 'prompt.txt').read_text()
            if prompt_text.lstrip().startswith('/goal ') and len(prompt_text) > 3900:
                prompt_text = (
                    '/goal Complete the authorized implementation task in ' + str(path / 'prompt.txt') +
                    '. Read that file first and follow its complete instructions. Implement actual files, '
                    'then verify completion against the task. ultrathink, high effort.'
                )
            process.communicate(prompt_text.encode('utf-8'))
        current_state = read_json(path / 'state.json')
        if current_state['status'] != 'cancelled':
            state['status'] = 'completed' if process.returncode == 0 else 'failed'
            for line in (path / 'events.jsonl').read_text(errors='replace').splitlines():
                try:
                    event = json.loads(line)
                except ValueError:
                    continue
                if event.get('type') == 'result':
                    state['session_id'] = event.get('session_id', '')
                    if event.get('is_error', False) or 'Goal condition is limited to' in event.get('result', ''):
                        state['status'] = 'failed'
                        state['error'] = event.get('result', 'Claude Code reported an error')
            state['exit_code'] = process.returncode
            state['finished_at'] = time.time()
            write_json(path / 'state.json', state)
    except Exception as error:
        state['status'] = 'failed'
        state['error'] = str(error)
        write_json(path / 'state.json', state)


if __name__ == '__main__':
    if len(sys.argv) == 3 and sys.argv[1] == '--worker':
        run_worker(sys.argv[2])
    else:
        SERVER.run(transport='stdio')
