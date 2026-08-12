import React, { useEffect, useState } from 'react'
import { fetchRole } from './api.js'
import { fmtDate, fmtDuration, fmtLevels, statusClass } from './format.js'

function Field({ label, children }) {
  if (children == null || children === '') return null
  return (
    <div className="dfield">
      <div className="dlabel">{label}</div>
      <div className="dvalue">{children}</div>
    </div>
  )
}

export default function RoleDetail({ roleId, onClose }) {
  const [role, setRole] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false
    setRole(null)
    fetchRole(roleId)
      .then(r => { if (!cancelled) setRole(r) })
      .catch(e => { if (!cancelled) setError(String(e)) })
    return () => { cancelled = true }
  }, [roleId])

  useEffect(() => {
    const onKey = e => { if (e.key === 'Escape') onClose() }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [onClose])

  return (
    <div className="overlay" onClick={onClose}>
      <div className="drawer" onClick={e => e.stopPropagation()} role="dialog" aria-modal="true">
        <button className="drawer-close" onClick={onClose} aria-label="Close">✕</button>
        {error && <div className="error">{error}</div>}
        {!role && !error && <div className="muted pad">Loading…</div>}
        {role && (
          <>
            <div className="drawer-head">
              <h2>{role.role_title}</h2>
              <div className="card-badges">
                {role.priority === 'High' && <span className="badge priority">High priority</span>}
                <span className={`badge status ${statusClass(role.role_status)}`}>{role.role_status}</span>
              </div>
              <div className="card-org">
                {role.client || '—'}
                {role.project_name && <span className="muted"> · {role.project_name}</span>}
              </div>
            </div>

            <div className="dgrid">
              <Field label="Work location">{role.work_location}</Field>
              <Field label="Career level">{fmtLevels(role.min_level, role.max_level)}</Field>
              <Field label="Start">{fmtDate(role.start_date)}</Field>
              <Field label="End">
                {fmtDate(role.end_date)}
                {role.duration_days != null && ` (${fmtDuration(role.duration_days)})`}
              </Field>
              <Field label="Assigned role">{role.assigned_role}</Field>
              <Field label="Job family">{role.job_family_group}</Field>
              <Field label="Specialty (RFE 9)">{role.rfe9}</Field>
              <Field label="Channel">{role.channel}</Field>
              <Field label="Sold role">{role.sold_role == null ? null : role.sold_role ? 'Yes' : 'No'}</Field>
              <Field label="Posted">{fmtDate(role.created_date)}</Field>
              <Field label="Role ID">{role.role_id}</Field>
              <Field label="Project ID">{role.project_id}</Field>
              <Field label="Primary contact">{role.primary_contact}</Field>
              <Field label="Fulfillment contact">{role.fulfillment_contact}</Field>
            </div>

            {role.skills.length > 0 && (
              <div className="dsection">
                <h3>Skills</h3>
                <table className="skilltable">
                  <tbody>
                    {role.skills.map((s, i) => (
                      <tr key={i}>
                        <td>{s.name}</td>
                        <td className="muted">{s.proficiency || '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            {role.description && (
              <div className="dsection">
                <h3>Description</h3>
                <p className="desc">{role.description.replace(/\t+/g, '\n')}</p>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  )
}
