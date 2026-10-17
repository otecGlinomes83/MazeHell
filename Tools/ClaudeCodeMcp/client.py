import asyncio
import json
from pathlib import Path
import sys

from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client


async def main() -> None:
    parameters = StdioServerParameters(command=sys.executable,
                                       args=[str(Path(__file__).with_name('server.py'))])
    async with stdio_client(parameters) as (reader, writer):
        async with ClientSession(reader, writer) as session:
            await session.initialize()
            if sys.argv[1] == 'list':
                result = await session.list_tools()
            else:
                arguments = json.loads(Path(sys.argv[2]).read_text())
                result = await session.call_tool(sys.argv[1], arguments)
            print(result.model_dump_json(indent=2))


asyncio.run(main())
