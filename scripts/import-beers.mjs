/**
 * Import beers from a CSV file.
 * Usage: node scripts/import-beers.mjs <path-to-csv>
 *
 * Env vars:
 *   API_URL   — base URL of the API (default: http://localhost:5274)
 *   ADMIN_KEY — required when the API has ADMIN_KEY configured
 *
 * CSV format (with header row):
 *   id,brewersName,beerName,style,abv,description,servingMethod
 */

import { readFileSync } from 'fs'

const API_URL  = process.env.API_URL  ?? 'http://localhost:5274'
const ADMIN_KEY = process.env.ADMIN_KEY ?? ''

const csvPath = process.argv[2]
if (!csvPath) {
  console.error('Usage: node scripts/import-beers.mjs <path-to-csv>')
  process.exit(1)
}

const csv = readFileSync(csvPath, 'utf8')
const lines = csv.split(/\r?\n/).filter(l => l.trim())

// Parse a single CSV line, handling quoted fields
function parseLine(line) {
  const fields = []
  let current = ''
  let inQuotes = false
  for (let i = 0; i < line.length; i++) {
    const ch = line[i]
    if (ch === '"') {
      if (inQuotes && line[i + 1] === '"') { current += '"'; i++ }
      else inQuotes = !inQuotes
    } else if (ch === ',' && !inQuotes) {
      fields.push(current)
      current = ''
    } else {
      current += ch
    }
  }
  fields.push(current)
  return fields
}

const [headerLine, ...dataLines] = lines
const headers = parseLine(headerLine).map(h => h.trim())

const required = ['id', 'brewersName', 'beerName', 'style', 'abv', 'description', 'servingMethod']
const missing = required.filter(h => !headers.includes(h))
if (missing.length) {
  console.error(`CSV is missing required columns: ${missing.join(', ')}`)
  process.exit(1)
}

const beers = dataLines.map((line, i) => {
  const values = parseLine(line)
  const row = Object.fromEntries(headers.map((h, j) => [h, values[j]?.trim() ?? '']))
  const id = parseInt(row.id, 10)
  const abv = parseFloat(row.abv)
  if (isNaN(id) || isNaN(abv)) {
    console.error(`Row ${i + 2}: invalid id or abv — "${line}"`)
    process.exit(1)
  }
  return {
    id,
    brewersName:   row.brewersName,
    beerName:      row.beerName,
    style:         row.style,
    abv,
    description:   row.description,
    servingMethod: row.servingMethod,
  }
})

console.log(`Parsed ${beers.length} beers from ${csvPath}`)

const headers_ = { 'Content-Type': 'application/json' }
if (ADMIN_KEY) headers_['X-Admin-Key'] = ADMIN_KEY

const res = await fetch(`${API_URL}/api/admin/beers`, {
  method: 'POST',
  headers: headers_,
  body: JSON.stringify(beers),
})

if (!res.ok) {
  const text = await res.text()
  console.error(`Import failed (${res.status}): ${text}`)
  process.exit(1)
}

const { imported } = await res.json()
console.log(`Imported ${imported} beers.`)
