import React, { useEffect, useMemo, useRef, useState } from 'react'
import { fetchRoles } from './api.js'
import Filters from './Filters.jsx'
import RoleCard from './RoleCard.jsx'
import RoleDetail from './RoleDetail.jsx'
import { fmtDate } from './format.js'

const PAGE_SIZE = 20

const EMPTY_SELECTIONS = {
  skill: [], location: [], level: [], job_family: [],
  status: [], client: [], rfe9: [], priority: [], sold: [],
}

export default function App() {
  const [q, setQ] = useState('')
  const [debouncedQ, setDebouncedQ] = useState('')
  const [selections, setSelections] = useState(EMPTY_SELECTIONS)
  const [sort, setSort] = useState('relevance')
  const [page, setPage] = useState(1)
  const [data, setData] = useState(null)
  const [error, setError] = useState(null)
  const [detailId, setDetailId] = useState(null)
  const listRef = useRef(null)

  useEffect(() => {
    const t = setTimeout(() => setDebouncedQ(q), 250)
    return () => clearTimeout(t)
  }, [q])

  useEffect(() => { setPage(1) }, [debouncedQ, selections, sort])

  useEffect(() => {
    let cancelled = false
    fetchRoles({ q: debouncedQ, selections, sort, page, pageSize: PAGE_SIZE })
      .then(d => { if (!cancelled) { setData(d); setError(null) } })
      .catch(e => { if (!cancelled) setError(String(e)) })
    return () => { cancelled = true }
  }, [debouncedQ, selections, sort, page])

  useEffect(() => { listRef.current?.scrollTo?.(0, 0) }, [page])

  const activeCount = useMemo(
    () => Object.values(selections).reduce((n, v) => n + v.length, 0),
    [selections]
  )

  function toggle(dim, value) {
    setSelections(prev => {
      const cur = prev[dim]
      const next = cur.includes(value) ? cur.filter(v => v !== value) : [...cur, value]
      return { ...prev, [dim]: next }
    })
  }

  function clearAll() {
    setSelections(EMPTY_SELECTIONS)
    setQ('')
  }

  const meta = data?.meta
  const pageCount = data ? Math.max(1, Math.ceil(data.total / PAGE_SIZE)) : 1

  return (
    <div className="app">
      <header className="header">
        <div className="header-inner">
          <div className="brand">
            <span className="brand-mark">Ava</span>Find
            <span className="brand-sub">role search</span>
          </div>
          <div className="searchbox">
            <svg viewBox="0 0 20 20" width="16" height="16" aria-hidden="true">
              <path d="M8.5 3a5.5 5.5 0 014.38 8.83l4.15 4.15-1.06 1.06-4.15-4.15A5.5 5.5 0 118.5 3zm0 1.5a4 4 0 100 8 4 4 0 000-8z" fill="currentColor"/>
            </svg>
            <input
              type="search"
              placeholder="Search titles, skills, clients, descriptions…"
              value={q}
              onChange={e => setQ(e.target.value)}
              autoFocus
            />
          </div>
          {meta?.extract_date && (
            <div className="asof" title={`Imported from ${meta.source_file}`}>
              <span className="asof-dot" />
              Data as of {fmtDate(meta.extract_date)} · static extract
            </div>
          )}
        </div>
      </header>

      <div className="body">
        <aside className="sidebar">
          <div className="sidebar-head">
            <h2>Filters</h2>
            {(activeCount > 0 || q) && (
              <button className="linkbtn" onClick={clearAll}>Clear all</button>
            )}
          </div>
          {data
            ? <Filters facets={data.facets} selections={selections} onToggle={toggle} />
            : !error && <div className="muted pad">Loading…</div>}
        </aside>

        <main className="results" ref={listRef}>
          {error && (
            <div className="error">
              Could not reach the API — {error}. Is the backend running?
            </div>
          )}
          {data && (
            <>
              <div className="toolbar">
                <div className="count">
                  <strong>{data.total}</strong> open role{data.total === 1 ? '' : 's'}
                  {meta && data.total !== meta.row_count && (
                    <span className="muted"> of {meta.row_count}</span>
                  )}
                </div>
                <label className="sort">
                  Sort by{' '}
                  <select value={sort} onChange={e => setSort(e.target.value)}>
                    <option value="relevance">Best match</option>
                    <option value="newest">Newest</option>
                    <option value="start">Starting soonest</option>
                    <option value="duration">Longest duration</option>
                  </select>
                </label>
              </div>

              {data.results.length === 0 && (
                <div className="empty">
                  <p>No roles match your search.</p>
                  <button className="linkbtn" onClick={clearAll}>Clear filters</button>
                </div>
              )}

              <ul className="cards">
                {data.results.map(r => (
                  <RoleCard key={r.role_id} role={r} query={debouncedQ} onOpen={() => setDetailId(r.role_id)} />
                ))}
              </ul>

              {pageCount > 1 && (
                <div className="pager">
                  <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}>← Prev</button>
                  <span>Page {page} of {pageCount}</span>
                  <button disabled={page >= pageCount} onClick={() => setPage(p => p + 1)}>Next →</button>
                </div>
              )}
            </>
          )}
        </main>
      </div>

      {detailId && <RoleDetail roleId={detailId} onClose={() => setDetailId(null)} />}
    </div>
  )
}
