"""Filtering, faceting, and ranking.

The dataset is a small static extract (tens of rows), so the API reads all
roles from the DB and does search/facets/ranking in Python. That keeps facet
counts correct (each facet ignores its own selection) and lets relevance use
weighted field matching without SQL gymnastics. If v2 ever gets real data
volume, this module is the thing to swap for SQL/full-text queries — the API
surface stays the same.
"""

from collections import Counter
from datetime import date

_EPOCH = date(1900, 1, 1)

# Facet dimension -> function extracting that role's value(s) for the dimension.
DIMENSIONS = {
    "skill": lambda r: [s.name for s in r.skills],
    "location": lambda r: [r.work_location] if r.work_location else [],
    "level": lambda r: _level_range(r),
    "job_family": lambda r: [r.job_family_group] if r.job_family_group else [],
    "status": lambda r: [r.role_status] if r.role_status else [],
    "client": lambda r: [r.client] if r.client else [],
    "rfe9": lambda r: [r.rfe9] if r.rfe9 else [],
    "priority": lambda r: [r.priority] if r.priority else [],
    "sold": lambda r: ["Yes" if r.sold_role else "No"] if r.sold_role is not None else [],
}

# Weighted fields for relevance: what a consultant scans for first weighs most.
SEARCH_FIELDS = [
    (5, lambda r: r.role_title or ""),
    (4, lambda r: r.primary_skill_name or ""),
    (3, lambda r: " ".join(s.name for s in r.skills)),
    (2, lambda r: r.assigned_role or ""),
    (2, lambda r: r.client or ""),
    (1, lambda r: r.project_name or ""),
    (1, lambda r: r.description or ""),
]


def _level_range(r):
    """Career levels where lower number = more senior; min/max can come in
    either order in the extract, so treat them as an inclusive range."""
    if r.min_level is None and r.max_level is None:
        return []
    lo = min(v for v in (r.min_level, r.max_level) if v is not None)
    hi = max(v for v in (r.min_level, r.max_level) if v is not None)
    return [str(v) for v in range(lo, hi + 1)]


def _matches_query(role, tokens):
    if not tokens:
        return True, 0
    fields = [(w, get(role).lower()) for w, get in SEARCH_FIELDS]
    score = 0
    for token in tokens:
        token_score = sum(w for w, text in fields if token in text)
        if token_score == 0:
            return False, 0  # every token must match somewhere
        score += token_score
    return True, score


def _matches_dim(role, dim, selected):
    return not selected or bool(set(DIMENSIONS[dim](role)) & selected)


def search(roles, q, selections, sort, page, page_size):
    tokens = [t for t in (q or "").lower().split() if t]
    selections = {d: set(v) for d, v in selections.items() if v}

    # Roles passing the text query, with score
    scored = []
    for role in roles:
        ok, score = _matches_query(role, tokens)
        if ok:
            scored.append((role, score))

    # Facets: each dimension counts over roles matching all OTHER selections
    facets = {}
    for dim in DIMENSIONS:
        counter = Counter()
        for role, _ in scored:
            if all(_matches_dim(role, d, sel) for d, sel in selections.items() if d != dim):
                counter.update(DIMENSIONS[dim](role))
        facets[dim] = [{"value": v, "count": c}
                       for v, c in sorted(counter.items(), key=lambda kv: (-kv[1], kv[0]))]

    results = [(role, score) for role, score in scored
               if all(_matches_dim(role, d, sel) for d, sel in selections.items())]

    if sort == "relevance" and tokens:
        results.sort(key=lambda rs: (-rs[1], -(rs[0].created_date or _EPOCH).toordinal()))
    elif sort == "start":
        results.sort(key=lambda rs: (rs[0].start_date or _EPOCH, rs[0].role_id))
    elif sort == "duration":
        results.sort(key=lambda rs: (-_duration_days(rs[0]), rs[0].role_id))
    else:  # newest (also the fallback for "relevance" without a query)
        results.sort(key=lambda rs: (-(rs[0].created_date or _EPOCH).toordinal(), rs[0].role_id))

    total = len(results)
    start = (page - 1) * page_size
    page_items = [role for role, _ in results[start:start + page_size]]
    return page_items, total, facets


def _duration_days(role):
    if role.start_date and role.end_date:
        return (role.end_date - role.start_date).days
    return -1


def to_card(role):
    return {
        "role_id": role.role_id,
        "role_title": role.role_title,
        "client": role.client,
        "project_name": role.project_name,
        "work_location": role.work_location,
        "job_family_group": role.job_family_group,
        "role_status": role.role_status,
        "priority": role.priority,
        "min_level": role.min_level,
        "max_level": role.max_level,
        "start_date": role.start_date.isoformat() if role.start_date else None,
        "end_date": role.end_date.isoformat() if role.end_date else None,
        "duration_days": (d if (d := _duration_days(role)) >= 0 else None),
        "created_date": role.created_date.isoformat() if role.created_date else None,
        "primary_skill": role.primary_skill_name,
        "skills": [{"name": s.name, "proficiency": s.proficiency} for s in role.skills],
    }


def to_detail(role):
    return {
        **to_card(role),
        "rfe8": role.rfe8,
        "rfe9": role.rfe9,
        "project_id": role.project_id,
        "assigned_role": role.assigned_role,
        "primary_contact": role.primary_contact,
        "fulfillment_contact": role.fulfillment_contact,
        "sold_role": role.sold_role,
        "charg_role": role.charg_role,
        "channel": role.channel,
        "project_geo": role.project_geo,
        "primary_skill_proficiency": role.primary_skill_proficiency,
        "description": role.description,
    }
