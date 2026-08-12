"""Database engine setup.

Resolution order:
1. DATABASE_URL env var (any SQLAlchemy URL) — used for local dev overrides.
2. SQL_SERVER/SQL_DATABASE/SQL_USER/SQL_PASSWORD env vars — Azure SQL via pyodbc.
   These are what the Bicep deployment sets on the App Service.
3. Fallback: local SQLite file next to the backend (dev default, zero setup).
"""

import os
from pathlib import Path

from sqlalchemy import create_engine
from sqlalchemy.engine import URL
from sqlalchemy.orm import sessionmaker

BACKEND_DIR = Path(__file__).resolve().parent.parent


def _build_url():
    url = os.environ.get("DATABASE_URL")
    if url:
        return url
    server = os.environ.get("SQL_SERVER")
    if server:
        return URL.create(
            "mssql+pyodbc",
            username=os.environ["SQL_USER"],
            password=os.environ["SQL_PASSWORD"],
            host=server,
            port=1433,
            database=os.environ["SQL_DATABASE"],
            query={
                "driver": "ODBC Driver 18 for SQL Server",
                "Encrypt": "yes",
                "TrustServerCertificate": "no",
            },
        )
    return f"sqlite:///{BACKEND_DIR / 'avafind.db'}"


engine = create_engine(_build_url(), pool_pre_ping=True)
SessionLocal = sessionmaker(bind=engine, expire_on_commit=False)
