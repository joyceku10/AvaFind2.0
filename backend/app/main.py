"""AvaFind API + static frontend host.

Single FastAPI app serves the JSON API under /api and the built React app
(backend/static, produced by `npm run build` in frontend/) for everything
else. One app = one App Service = lowest cost.
"""

from pathlib import Path

from fastapi import FastAPI, HTTPException, Query
from fastapi.responses import FileResponse
from fastapi.staticfiles import StaticFiles
from sqlalchemy.orm import selectinload

from .db import SessionLocal, engine
from .models import Base, ImportMeta, Role
from .search import search, to_card, to_detail

STATIC_DIR = Path(__file__).resolve().parent.parent / "static"

app = FastAPI(title="AvaFind", docs_url=None, redoc_url=None)


@app.on_event("startup")
def ensure_schema():
    Base.metadata.create_all(engine)


def load_roles(session):
    return session.query(Role).options(selectinload(Role.skills)).all()


def meta_payload(session):
    meta = session.get(ImportMeta, 1)
    if meta is None:
        return {"extract_date": None, "imported_at": None, "row_count": 0, "source_file": None}
    return {
        "extract_date": meta.extract_date.isoformat() if meta.extract_date else None,
        "imported_at": meta.imported_at.isoformat() if meta.imported_at else None,
        "row_count": meta.row_count,
        "source_file": meta.source_file,
    }


@app.get("/api/meta")
def get_meta():
    with SessionLocal() as session:
        return meta_payload(session)


@app.get("/api/roles")
def list_roles(
    q: str | None = None,
    skill: list[str] = Query(default=[]),
    location: list[str] = Query(default=[]),
    level: list[str] = Query(default=[]),
    job_family: list[str] = Query(default=[]),
    status: list[str] = Query(default=[]),
    client: list[str] = Query(default=[]),
    rfe9: list[str] = Query(default=[]),
    priority: list[str] = Query(default=[]),
    sold: list[str] = Query(default=[]),
    sort: str = Query(default="relevance", pattern="^(relevance|newest|start|duration)$"),
    page: int = Query(default=1, ge=1),
    page_size: int = Query(default=20, ge=1, le=100),
):
    selections = {
        "skill": skill, "location": location, "level": level,
        "job_family": job_family, "status": status, "client": client,
        "rfe9": rfe9, "priority": priority, "sold": sold,
    }
    with SessionLocal() as session:
        roles = load_roles(session)
        items, total, facets = search(roles, q, selections, sort, page, page_size)
        return {
            "total": total,
            "page": page,
            "page_size": page_size,
            "results": [to_card(r) for r in items],
            "facets": facets,
            "meta": meta_payload(session),
        }


@app.get("/api/roles/{role_id}")
def get_role(role_id: str):
    with SessionLocal() as session:
        role = (
            session.query(Role)
            .options(selectinload(Role.skills))
            .filter(Role.role_id == role_id)
            .one_or_none()
        )
        if role is None:
            raise HTTPException(status_code=404, detail="Role not found")
        return to_detail(role)


# --- static frontend (must be registered after the API routes) ---
if STATIC_DIR.is_dir():
    app.mount("/assets", StaticFiles(directory=STATIC_DIR / "assets"), name="assets")

    @app.get("/{path:path}", include_in_schema=False)
    def spa(path: str):
        candidate = STATIC_DIR / path
        if path and candidate.is_file() and candidate.resolve().is_relative_to(STATIC_DIR):
            return FileResponse(candidate)
        return FileResponse(STATIC_DIR / "index.html")
