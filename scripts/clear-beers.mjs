/**
 * Clear all beers from the Beers table.
 * Usage: node scripts/clear-beers.mjs
 *
 * Env vars:
 *   API_URL   — base URL of the API (default: http://localhost:5274)
 *   ADMIN_KEY — required when the API has ADMIN_KEY configured
 */

const API_URL   = process.env.API_URL   ?? 'http://localhost:5274'
const ADMIN_KEY = process.env.ADMIN_KEY ?? ''

const headers = {}
if (ADMIN_KEY) headers['X-Admin-Key'] = ADMIN_KEY

const res = await fetch(`${API_URL}/api/admin/beers`, { method: 'DELETE', headers })

if (!res.ok) {
  const text = await res.text()
  console.error(`Clear failed (${res.status}): ${text}`)
  process.exit(1)
}

const { deleted } = await res.json()
console.log(`Cleared ${deleted} beers.`)
