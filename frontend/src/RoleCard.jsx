import React from 'react'
import { fmtDateShort, fmtDuration, fmtLevels, statusClass } from './format.js'

const MAX_CHIPS = 5

export default function RoleCard({ role, onOpen }) {
  const levels = fmtLevels(role.min_level, role.max_level)
  const duration = fmtDuration(role.duration_days)
  const extraSkills = role.skills.length - MAX_CHIPS

  return (
    <li className="card" onClick={onOpen} tabIndex={0}
        onKeyDown={e => { if (e.key === 'Enter') onOpen() }}>
      <div className="card-top">
        <h3 className="card-title">{role.role_title || '(untitled role)'}</h3>
        <div className="card-badges">
          {role.priority === 'High' && <span className="badge priority">High priority</span>}
          <span className={`badge status ${statusClass(role.role_status)}`}>{role.role_status}</span>
        </div>
      </div>

      <div className="card-org">
        {role.client || '—'}
        {role.project_name && <span className="muted"> · {role.project_name}</span>}
      </div>

      <div className="card-meta">
        {role.work_location && <span className="meta-item">📍 {role.work_location}</span>}
        {levels && <span className="meta-item">{levels}</span>}
        <span className="meta-item">
          {fmtDateShort(role.start_date)} → {fmtDateShort(role.end_date)}
          {duration && <span className="muted"> ({duration})</span>}
        </span>
        {role.created_date && (
          <span className="meta-item muted">Posted {fmtDateShort(role.created_date)}</span>
        )}
      </div>

      {role.skills.length > 0 && (
        <div className="chips">
          {role.skills.slice(0, MAX_CHIPS).map((s, i) => (
            <span key={i} className={`chip${s.name === role.primary_skill ? ' primary' : ''}`}
                  title={s.proficiency || undefined}>
              {s.name}
            </span>
          ))}
          {extraSkills > 0 && <span className="chip more">+{extraSkills}</span>}
        </div>
      )}
    </li>
  )
}
