const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

export function fmtDate(iso) {
  if (!iso) return '—'
  const [y, m, d] = iso.split('-').map(Number)
  return `${MONTHS[m - 1]} ${d}, ${y}`
}

export function fmtDateShort(iso) {
  if (!iso) return '—'
  const [y, m, d] = iso.split('-').map(Number)
  return `${MONTHS[m - 1]} ${d} '${String(y).slice(2)}`
}

export function fmtDuration(days) {
  if (days == null) return null
  if (days < 50) return `${Math.max(1, Math.round(days / 7))} wk`
  return `${Math.round(days / 30.4)} mo`
}

// Career levels: lower number = more senior; show the span consistently.
export function fmtLevels(min, max) {
  if (min == null && max == null) return null
  if (min == null || max == null || min === max) return `CL ${min ?? max}`
  const lo = Math.min(min, max)
  const hi = Math.max(min, max)
  return `CL ${lo}–${hi}`
}

export function statusClass(status) {
  if (!status) return 'neutral'
  if (status.includes('New')) return 'new'
  if (status.includes('In Process')) return 'inprocess'
  return 'feedback'
}
