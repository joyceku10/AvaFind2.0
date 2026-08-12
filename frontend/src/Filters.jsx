import React, { useState } from 'react'

// Order and labels chosen for how a consultant narrows a role search:
// what can I do (skills, level), where (location), then role metadata.
const GROUPS = [
  { dim: 'skill', label: 'Skills', searchable: true, initial: 8 },
  { dim: 'level', label: 'Career level', format: v => `CL ${v}`, sortNumeric: true, initial: 12 },
  { dim: 'location', label: 'Work location', searchable: true, initial: 8 },
  { dim: 'job_family', label: 'Job family', initial: 6 },
  { dim: 'status', label: 'Status', initial: 6 },
  { dim: 'priority', label: 'Priority', initial: 4 },
  { dim: 'client', label: 'Client', searchable: true, initial: 6 },
  { dim: 'rfe9', label: 'Specialty (RFE 9)', initial: 4 },
  { dim: 'sold', label: 'Sold role', initial: 2 },
]

function FilterGroup({ group, options, selected, onToggle }) {
  const [expanded, setExpanded] = useState(false)
  const [filter, setFilter] = useState('')

  let opts = options
  if (group.sortNumeric) {
    opts = [...options].sort((a, b) => Number(a.value) - Number(b.value))
  }
  if (filter) {
    opts = opts.filter(o => o.value.toLowerCase().includes(filter.toLowerCase()))
  }
  // Keep selected options visible even when collapsed
  const visible = expanded ? opts : opts.filter(
    (o, i) => i < group.initial || selected.includes(o.value)
  )
  const hidden = opts.length - visible.length

  if (options.length === 0 && selected.length === 0) return null

  return (
    <fieldset className="fgroup">
      <legend>{group.label}</legend>
      {group.searchable && options.length > group.initial && (
        <input
          className="fsearch"
          type="search"
          placeholder={`Filter ${group.label.toLowerCase()}…`}
          value={filter}
          onChange={e => setFilter(e.target.value)}
        />
      )}
      {visible.map(o => (
        <label key={o.value} className="fopt">
          <input
            type="checkbox"
            checked={selected.includes(o.value)}
            onChange={() => onToggle(group.dim, o.value)}
          />
          <span className="fopt-label">{group.format ? group.format(o.value) : o.value}</span>
          <span className="fopt-count">{o.count}</span>
        </label>
      ))}
      {hidden > 0 && (
        <button className="linkbtn" onClick={() => setExpanded(true)}>
          Show {hidden} more
        </button>
      )}
      {expanded && opts.length > group.initial && (
        <button className="linkbtn" onClick={() => setExpanded(false)}>Show less</button>
      )}
    </fieldset>
  )
}

export default function Filters({ facets, selections, onToggle }) {
  return (
    <div className="filters">
      {GROUPS.map(g => (
        <FilterGroup
          key={g.dim}
          group={g}
          options={facets[g.dim] || []}
          selected={selections[g.dim]}
          onToggle={onToggle}
        />
      ))}
    </div>
  )
}
