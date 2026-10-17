import asyncio
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import sys

from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client
import server


class ClaudeCodeMcpTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.previous_jobs = server.JOBS
        server.JOBS = Path(self.directory.name)

    def tearDown(self):
        server.JOBS = self.previous_jobs
        self.directory.cleanup()

    def create_job(self, status='running'):
        job_id = '1' * 32
        path = server.JOBS / job_id
        path.mkdir()
        server.write_json(path / 'state.json', {'job_id': job_id, 'status': status})
        return job_id, path

    def test_rejects_path_traversal(self):
        with self.assertRaises(ValueError):
            server.claude_code_status('../config')

    def test_rejects_empty_prompt_and_unknown_effort(self):
        with self.assertRaises(ValueError):
            server.claude_code_start(' ')
        with self.assertRaises(ValueError):
            server.claude_code_start('task', effort='unlimited')

    def test_rejects_invalid_resume_session(self):
        with self.assertRaises(ValueError):
            server.claude_code_start('task', resume_session='--command')

    def test_status_reads_assistant_text_and_session(self):
        job_id, path = self.create_job()
        events = [
            {'type': 'system', 'subtype': 'init', 'session_id': 'test-session'},
            {'type': 'assistant', 'message': {'content': [{'type': 'text', 'text': 'Implemented'}]}},
            {'type': 'result', 'session_id': 'test-session', 'result': 'Finished', 'is_error': False}
        ]
        (path / 'events.jsonl').write_text('\n'.join(json.dumps(event) for event in events))
        result = server.claude_code_status(job_id)
        self.assertEqual(result['session_id'], 'test-session')
        self.assertIn('Implemented', result['output'])
        self.assertIn('Finished', result['output'])

    def test_cancel_preserves_completed_job(self):
        job_id, path = self.create_job('completed')
        result = server.claude_code_cancel(job_id)
        self.assertEqual(result['status'], 'completed')
        self.assertFalse((path / 'cancelled').exists())

    def test_cancel_marks_starting_job_before_worker_launch(self):
        job_id, path = self.create_job('starting')
        server.claude_code_cancel(job_id)
        self.assertTrue((path / 'cancelled').exists())
        self.assertEqual(server.read_json(path / 'state.json')['status'], 'cancelled')

    def test_start_uses_argument_vector_and_persists_prompt(self):
        with patch.object(server.subprocess, 'Popen') as launch:
            launch.return_value.pid = 42
            result = server.claude_code_start('literal $(do-not-execute)', effort='high')
        path = server.job_path(result['job_id'])
        self.assertEqual((path / 'prompt.txt').read_text(), 'literal $(do-not-execute)')
        state = server.read_json(path / 'state.json')
        self.assertEqual(state['effort'], 'high')
        self.assertEqual(state['model'], 'glm-5.3-flash[1M]')
        self.assertIsInstance(launch.call_args.args[0], list)
        self.assertNotIn('shell', launch.call_args.kwargs)

    def test_worker_passes_model_effort_and_literal_prompt_to_claude_code(self):
        job_id, path = self.create_job('starting')
        state = server.read_json(path / 'state.json')
        state.update({'model': 'glm-5.3-flash[1M]', 'effort': 'high',
                      'resume_session': '', 'session_id': 'test-session'})
        server.write_json(path / 'state.json', state)
        (path / 'prompt.txt').write_text('/goal ultrathink $(literal)')
        with patch.object(server.subprocess, 'Popen') as launch:
            launch.return_value.pid = 43
            launch.return_value.returncode = 0
            server.run_worker(job_id)
            launch.return_value.communicate.assert_called_once_with(b'/goal ultrathink $(literal)')
        arguments = launch.call_args.args[0]
        self.assertEqual(arguments[0], 'claude')
        self.assertEqual(arguments[arguments.index('--effort') + 1], 'high')
        self.assertEqual(arguments[arguments.index('--model') + 1], 'glm-5.3-flash[1M]')
        self.assertIn('--strict-mcp-config', arguments)
        self.assertIn('MAX_THINKING_TOKENS', launch.call_args.kwargs['env'])
        self.assertEqual(launch.call_args.kwargs['env']['CLAUDE_CODE_EFFORT_LEVEL'], 'high')
        self.assertEqual(server.read_json(path / 'state.json')['status'], 'completed')

    def test_long_goal_is_sent_as_a_short_file_backed_instruction(self):
        job_id, path = self.create_job('starting')
        state = server.read_json(path / 'state.json')
        state.update({'model': 'glm-5.3-flash[1M]', 'effort': 'high',
                      'resume_session': '', 'session_id': 'test-session'})
        server.write_json(path / 'state.json', state)
        full_prompt = '/goal ' + 'implementation details ' * 300
        (path / 'prompt.txt').write_text(full_prompt)
        with patch.object(server.subprocess, 'Popen') as launch:
            launch.return_value.pid = 44
            launch.return_value.returncode = 0
            server.run_worker(job_id)
            sent = launch.return_value.communicate.call_args.args[0].decode()
        self.assertTrue(sent.startswith('/goal '))
        self.assertLess(len(sent), 4000)
        self.assertIn(str(path / 'prompt.txt'), sent)
        self.assertEqual((path / 'prompt.txt').read_text(), full_prompt)

    def test_cancelled_worker_never_launches_claude(self):
        job_id, path = self.create_job('starting')
        state = server.read_json(path / 'state.json')
        state.update({'model': 'glm-5.3-flash[1M]', 'effort': 'high',
                      'resume_session': '', 'session_id': 'test-session'})
        server.write_json(path / 'state.json', state)
        (path / 'cancelled').touch()
        with patch.object(server.subprocess, 'Popen') as launch:
            server.run_worker(job_id)
            launch.assert_not_called()
        self.assertEqual(server.read_json(path / 'state.json')['status'], 'cancelled')

    def test_real_stdio_protocol_and_error_response(self):
        async def protocol():
            parameters = StdioServerParameters(command=sys.executable,
                args=[str(Path(__file__).with_name('server.py'))])
            async with stdio_client(parameters) as (reader, writer):
                async with ClientSession(reader, writer) as session:
                    await session.initialize()
                    tools = await session.list_tools()
                    self.assertEqual({tool.name for tool in tools.tools},
                        {'claude_code_start', 'claude_code_status', 'claude_code_cancel'})
                    result = await session.call_tool('claude_code_status', {'job_id': '../escape'})
                    self.assertTrue(result.isError)
        asyncio.run(protocol())


if __name__ == '__main__':
    unittest.main()
