export async function fetchRoles(params) {
  const qs = new URLSearchParams()
  if (params.q) qs.set('q', params.q)
  for (const [dim, values] of Object.entries(params.selections)) {
    for (const v of values) qs.append(dim, v)
  }
  qs.set('sort', params.sort)
  qs.set('page', params.page)
  qs.set('page_size', params.pageSize)
  const res = await fetch(`/api/roles?${qs}`)
  if (!res.ok) throw new Error(`API error ${res.status}`)
  return res.json()
}

export async function fetchRole(roleId) {
  const res = await fetch(`/api/roles/${encodeURIComponent(roleId)}`)
  if (!res.ok) throw new Error(`API error ${res.status}`)
  return res.json()
}
