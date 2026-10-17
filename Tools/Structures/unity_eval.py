import subprocess,sys
from pathlib import Path
code=Path(sys.argv[1]).read_text() if Path(sys.argv[1]).is_file() else sys.argv[1]
r=subprocess.run(['unity','command','eval',code,'--timeout','60','--json'],capture_output=True,text=True)
print(r.stdout);print(r.stderr);sys.exit(r.returncode)
