/**
 * Seed script — populates the local API with example ratings for UI testing.
 * Run with: node scripts/seed-ratings.mjs
 * Requires the API (make api) and Azurite (azurite --silent) to be running.
 */

const BASE_URL = 'http://localhost:5274'

// Realistic rating profiles — each user has a tendency and preferences
const users = [
  {
    // Hop-head, loves IPAs and pale ales
    ratings: {
      1: { rating: 10, notes: 'Perfect. Everything an IPA should be.' },
      2: { rating: 6, notes: 'Solid but a bit old fashioned for me.' },
      3: { rating: 8, notes: 'Dangerously drinkable.' },
      5: { rating: 9, notes: 'Tropical fruit bombs — love it.' },
      6: { rating: 9 },
      8: { rating: 10, notes: 'Big and bold, exactly what I wanted.' },
      9: { rating: 8 },
      13: { rating: 7, notes: 'A classic but Jaipur does it better.' },
      15: { rating: 9, notes: 'Pillow soft with insane hop aroma.' },
      18: { rating: 7 },
    },
  },
  {
    // Cask traditionalist, prefers bitters and milds
    ratings: {
      2: { rating: 9, notes: 'London Pride at its best tonight.' },
      3: { rating: 8 },
      6: { rating: 7 },
      7: { rating: 9, notes: 'Proper Northern bitter, no fuss.' },
      9: { rating: 7 },
      10: { rating: 8, notes: 'Warm and comforting, just right.' },
      14: { rating: 10, notes: 'The benchmark. Four CAMRA awards for a reason.' },
      16: { rating: 9, notes: "Harvey's never disappoints." },
      17: { rating: 8, notes: 'Proof you don\'t need ABV to have flavour.' },
      19: { rating: 7 },
    },
  },
  {
    // Stout and dark beer lover
    ratings: {
      1: { rating: 7 },
      2: { rating: 8 },
      7: { rating: 6 },
      8: { rating: 8, notes: 'A bit one-dimensional but enjoyable.' },
      10: { rating: 7, notes: 'Nice maltiness.' },
      11: { rating: 10, notes: 'Coffee and chocolate in a glass. Stunning.' },
      14: { rating: 9 },
      15: { rating: 6, notes: 'Too much for me but I can see why people love it.' },
      16: { rating: 8, notes: 'Underrated. Really solid bitter.' },
    },
  },
  {
    // Craft beer enthusiast, rates broadly
    ratings: {
      1: { rating: 9 },
      3: { rating: 8 },
      4: { rating: 8, notes: 'Spicy and complex, love the dry finish.' },
      5: { rating: 9, notes: 'Beavertown on form.' },
      6: { rating: 8 },
      11: { rating: 8, notes: 'Rich breakfast in a pint.' },
      12: { rating: 7, notes: 'Subtle and aromatic, a grower.' },
      13: { rating: 6, notes: 'Fine but feels a bit corporate now.' },
      15: { rating: 10, notes: 'Best beer at the festival. That mouthfeel!' },
      17: { rating: 9, notes: 'Absolutely class for the ABV.' },
      18: { rating: 8 },
    },
  },
  {
    // Casual drinker, rates everything, middling scores
    ratings: {
      1: { rating: 7 },
      2: { rating: 7 },
      3: { rating: 8 },
      4: { rating: 5, notes: 'Not really my thing.' },
      5: { rating: 8 },
      6: { rating: 7 },
      9: { rating: 7 },
      10: { rating: 8, notes: 'Very easy drinking.' },
      13: { rating: 6 },
      14: { rating: 8 },
      19: { rating: 6 },
      20: { rating: 7, notes: 'Refreshing, good with food.' },
    },
  },
  {
    // Session beer fan, prefers lower ABV
    ratings: {
      2: { rating: 7 },
      3: { rating: 9, notes: 'My go-to session beer all night.' },
      6: { rating: 8, notes: 'Citra magic at a sensible ABV.' },
      7: { rating: 8 },
      8: { rating: 5, notes: 'Too boozy for a session.' },
      10: { rating: 7 },
      14: { rating: 8 },
      15: { rating: 4, notes: 'Two pints and I\'d be done for the night.' },
      16: { rating: 9 },
      17: { rating: 10, notes: 'Wish every beer was like this. Drink all day.' },
      18: { rating: 9, notes: 'Punches well above its ABV.' },
      19: { rating: 8 },
    },
  },
  {
    // Balanced palate, appreciates variety
    ratings: {
      1: { rating: 8 },
      4: { rating: 7, notes: 'Interesting yeast character, takes some getting used to.' },
      5: { rating: 8 },
      7: { rating: 7 },
      9: { rating: 8, notes: 'Solid Cornish IPA.' },
      10: { rating: 9, notes: 'Cwtch is an apt name — very comforting.' },
      11: { rating: 7 },
      12: { rating: 8, notes: 'Really nice aromatics on this one.' },
      13: { rating: 7 },
      14: { rating: 9 },
      20: { rating: 6 },
    },
  },
  {
    // Wild card — strong opinions, full scores and 1s
    ratings: {
      1: { rating: 10, notes: 'Jaipur is untouchable.' },
      2: { rating: 5 },
      4: { rating: 3, notes: 'Too funky, not for me at all.' },
      6: { rating: 9 },
      8: { rating: 8 },
      11: { rating: 9, notes: 'Genuinely one of the best stouts I\'ve had.' },
      13: { rating: 2, notes: 'Overrated and overmarketed.' },
      15: { rating: 8 },
      16: { rating: 6 },
      18: { rating: 8, notes: 'Hazy and hoppy, great.' },
    },
  },
]

async function post(path, body) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  if (!res.ok) throw new Error(`POST ${path} failed: ${res.status}`)
  return res.json()
}

let totalRatings = 0

for (const user of users) {
  const { userId } = await post('/api/register', {})

  for (const [beerId, { rating, notes = null }] of Object.entries(user.ratings)) {
    await post('/api/ratings', { userId, beerId: Number(beerId), rating, notes })
    totalRatings++
  }
}

console.log(`Seeded ${users.length} users and ${totalRatings} ratings.`)
