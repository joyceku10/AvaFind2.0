# AvaFind

A simpler, faster way to search Avanade project opportunities. Official
tooling covers 700k+ consultants but the experience is slow to navigate.
AvaFind is a lightweight viewer on top of role data, built to help
consultants find relevant projects quickly.

**v1 is a prototype built on a static Excel extract** (66 roles from the
A&I US demand report, extract date 2026-02-11). It is not connected to any
live system. The goal is to demonstrate the concept and make the case for a
v2 with real, sponsored data access.

## What it looks like

- **Faceted job search**, not a data grid: filters for skills, career level,
  work location, job family, status, priority, client, and specialty sit in a
  sidebar with live counts; results are scannable cards showing what a
  consultant checks first — title, client, location, level range, dates with
  duration, and skill chips (primary skill highlighted).
- **Free-text search** across titles, skills, clients, projects, and
  descriptions, with weighted relevance ranking (title matches outrank
  description matches).
- **Sorts that match how people pick roles**: best match, newest, starting
  soonest, longest duration.
- A **"data as of" badge** is always visible — this is a snapshot, not live
  data.
- Click a card for the full record: every field from the extract, all skills
  with proficiency levels, and the full description.

Facet counts follow standard faceted-search behavior (each facet's counts
ignore its own selections), career levels are treated as an inclusive range
(a CL8–10 role matches a CL9 filter), and the packed skill strings from the
extract are parsed into individual skills with proficiencies.

## Architecture

| Piece | Choice |
|---|---|
| Backend | Python / FastAPI, serves the API **and** the built frontend (one App Service = lowest cost) |
| Frontend | React (Vite), built to static files |
| Database | Azure SQL (Basic tier); local dev uses SQLite automatically |
| Infra | Bicep — one Linux App Service Plan (B1), one App Service, one SQL server + Basic DB |

Because the dataset is a small static extract, the API reads all roles from
the database and does filtering/faceting/ranking in Python. That keeps facet
counts correct and relevance scoring simple, with the database as the source
of truth. The search module ([backend/app/search.py](backend/app/search.py))
is the single thing to replace with real SQL/full-text queries if v2 gets
real data volume — the API surface would not change. Similarly, the import
layer is one script with an explicit column mapping, so pointing v2 at a
live feed replaces one file, not the app.

## Local setup

Prereqs: Python 3.11+, Node 18+.

```powershell
# Backend deps
cd backend
pip install -r requirements.txt   # pyodbc is only needed for Azure SQL; local dev uses SQLite

# Import the sample extract (creates backend/avafind.db)
python import_extract.py "../docs/samples/20260211-A&I_US_Demand.xlsx"

# Build the frontend into backend/static
cd ../frontend
npm install
npm run build

# Run
cd ../backend
python -m uvicorn app.main:app --port 8000
# open http://127.0.0.1:8000
```

For frontend development with hot reload, run `npm run dev` in `frontend/`
(it proxies `/api` to `127.0.0.1:8000`).

## Importing a new extract

```powershell
cd backend
python import_extract.py "path\to\YYYYMMDD-whatever.xlsx" [--as-of YYYY-MM-DD] [--sheet Export]
```

- The import is a **full replace** in one transaction.
- The extract date shown in the UI comes from a leading `YYYYMMDD` in the
  filename, or `--as-of`.
- The script validates that all expected columns are present and fails
  loudly if the extract format changed. It prints row counts and any
  per-row warnings (unparseable dates/skills) — check that output before
  trusting the data.

To import into **Azure SQL** instead of local SQLite, set the connection env
vars first (requires the
[Microsoft ODBC Driver 18](https://learn.microsoft.com/sql/connect/odbc/download-odbc-driver-for-sql-server)
locally):

```powershell
$env:SQL_SERVER   = "<server>.database.windows.net"   # `sqlServerFqdn` deployment output
$env:SQL_DATABASE = "avafind"
$env:SQL_USER     = "avafindadmin"
$env:SQL_PASSWORD = "<password>"
python import_extract.py "../docs/samples/20260211-A&I_US_Demand.xlsx"
```

## Deploying to Azure

```powershell
# 1. Parameters — copy the example and fill in a SQL password and YOUR public IP
cp infra/main.parameters.example.json infra/main.parameters.json  # gitignored

# 2. Provision everything (resource group, plan, app, SQL)
az deployment sub create --location eastus2 `
  --template-file infra/main.bicep --parameters "@infra/main.parameters.json"

# 3. Build the frontend, then zip and deploy the backend (which contains it)
cd frontend; npm install; npm run build; cd ..
python infra/make_deploy_zip.py   # NOT Compress-Archive — see note inside the script
az webapp deploy --resource-group rg-avafind --name <webAppName output> --src-path backend.zip --type zip

# 4. Load the data (from your machine, using the env vars shown above)
```

Tear down: `az group delete --name rg-avafind`.

When your home/office IP changes, edit `allowedClientIp` in
`main.parameters.json` and re-run step 2 — it updates both the App Service
access restriction and the SQL firewall rule in place.

## Access control — deliberate v1 choice

**There is no sign-in.** v1 runs on a personal Azure subscription and is not
connected to Avanade's corporate tenant, so SSO is out of scope until the
project is sponsored and goes through IT/security properly (that's the v2
conversation this prototype exists to start).

Instead, the app is **not reachable from the public internet**: the App
Service denies all traffic except one parameterized IP address
(`allowedClientIp`), and the deployment (SCM) endpoint has the same
restriction. This is network-level access control, done in infrastructure,
on purpose. There is deliberately **no** password gate, secret URL, or other
app-level workaround — those look like security without being it. If you
fork this, keep the IP restriction until you put real auth in front.

## Known v1 limitations

- **Static snapshot.** Data is whatever extract was last imported; the UI
  shows the extract date at all times.
- **Single-viewer by design.** One allowed IP, no auth (see above).
- **Search is in-process.** Fine for hundreds of rows, not for 700k — v2
  would push search/facets into SQL or a search service.
- **Schema mirrors this extract.** The importer fails loudly if columns
  change rather than guessing. `RFE 8`/`Charg Role` are constant in this
  extract and imported but not shown as filters; `RFE 9` is surfaced as
  "Specialty (RFE 9)".
- **The extract contains contact names.** `Role Primary Contact` and
  `Role Fulfillment Contact` are real people's names, shown in the role
  detail view. That's data in the source extract, not something AvaFind
  adds — but it's a reason the app (and this repo, while it contains a
  sample extract) should stay access-restricted.
- **No tests/CI.** Prototype; the import script's verification output is
  the main safety net.
