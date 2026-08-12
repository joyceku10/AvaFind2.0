"""Build backend.zip for `az webapp deploy`.

Don't use PowerShell's Compress-Archive for this: on Windows PowerShell 5.1 it
writes zip entries with backslash separators, which Linux App Service extracts
as literal flat filenames ("app\\main.py") — the app then fails with
ModuleNotFoundError. This script always writes POSIX paths.

Usage (from repo root): python infra/make_deploy_zip.py
"""

import zipfile
from pathlib import Path

root = Path(__file__).resolve().parent.parent
backend = root / "backend"
out = root / "backend.zip"

static = backend / "static"
if not static.is_dir():
    raise SystemExit("backend/static missing — run `npm run build` in frontend/ first")

with zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED) as z:
    for target in ["app", "static"]:
        for f in (backend / target).rglob("*"):
            if f.is_file() and "__pycache__" not in f.parts:
                z.write(f, f.relative_to(backend).as_posix())
    z.write(backend / "requirements.txt", "requirements.txt")
    z.write(backend / "import_extract.py", "import_extract.py")

print(f"wrote {out} ({out.stat().st_size:,} bytes)")
