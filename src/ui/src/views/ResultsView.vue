<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useBeersStore } from '@/stores/beers'

interface BeerResult {
  beerId: number
  average: number
  count: number
}

interface BeerRating {
  rating: number
  notes: string | null
}

const beersStore = useBeersStore()
const results = ref<BeerResult[]>([])
const isLoading = ref(true)
const error = ref<string | null>(null)
const expandedBeerId = ref<number | null>(null)
const beerRatings = ref<Record<number, BeerRating[]>>({})
const loadingRatings = ref<Set<number>>(new Set())

onMounted(async () => {
  await beersStore.fetchBeers()
  try {
    const response = await fetch('/api/results')
    if (!response.ok) throw new Error('Failed to load results')
    results.value = await response.json()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Something went wrong'
  } finally {
    isLoading.value = false
  }
})

async function toggleBeer(beerId: number) {
  if (expandedBeerId.value === beerId) {
    expandedBeerId.value = null
    return
  }

  expandedBeerId.value = beerId

  if (beerRatings.value[beerId]) return

  loadingRatings.value = new Set(loadingRatings.value).add(beerId)
  try {
    const response = await fetch(`/api/results/${beerId}`)
    if (!response.ok) throw new Error('Failed to load ratings')
    beerRatings.value = { ...beerRatings.value, [beerId]: await response.json() }
  } finally {
    const next = new Set(loadingRatings.value)
    next.delete(beerId)
    loadingRatings.value = next
  }
}

function beerName(beerId: number): string {
  return beersStore.beers.find(b => b.id === beerId)?.beerName ?? '—'
}

function brewerName(beerId: number): string {
  return beersStore.beers.find(b => b.id === beerId)?.brewersName ?? '—'
}
</script>

<template>
  <div class="results">
    <header class="results__header">
      <div>
        <h1 class="results__title">Results</h1>
        <p class="results__sub">Average scores across all tasters</p>
      </div>
    </header>

    <div v-if="isLoading" class="results__state">
      <span class="results__spinner" aria-hidden="true" />
      <p>Loading results…</p>
    </div>

    <div v-else-if="error" class="results__state results__state--error" role="alert">
      <p>{{ error }}</p>
    </div>

    <div v-else-if="results.length === 0" class="results__state">
      <p>No ratings yet — get tasting!</p>
    </div>

    <ol v-else class="results__list">
      <li v-for="(result, index) in results" :key="result.beerId" class="results__item">
        <button
          class="results__row"
          :class="{ 'results__row--open': expandedBeerId === result.beerId }"
          :aria-expanded="expandedBeerId === result.beerId"
          @click="toggleBeer(result.beerId)"
        >
          <span class="results__rank" aria-label="`Rank ${index + 1}`">
            <template v-if="index === 0">🥇</template>
            <template v-else-if="index === 1">🥈</template>
            <template v-else-if="index === 2">🥉</template>
            <template v-else>{{ index + 1 }}</template>
          </span>
          <div class="results__beer">
            <span class="results__beer-name">{{ beerName(result.beerId) }}</span>
            <span class="results__brewer">{{ brewerName(result.beerId) }}</span>
          </div>
          <div class="results__score">
            <span class="results__average">{{ result.average.toFixed(1) }}</span>
            <span class="results__count">{{ result.count }} {{ result.count === 1 ? 'rating' : 'ratings' }}</span>
          </div>
          <svg
            class="results__chevron"
            width="14" height="14" viewBox="0 0 14 14" fill="none" aria-hidden="true"
            :class="{ 'results__chevron--open': expandedBeerId === result.beerId }"
          >
            <path d="M3 5l4 4 4-4" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </button>

        <Transition name="expand">
          <div v-if="expandedBeerId === result.beerId" class="results__breakdown">
            <div v-if="loadingRatings.has(result.beerId)" class="results__breakdown-loading">
              <span class="results__spinner results__spinner--sm" aria-hidden="true" />
            </div>
            <ul v-else-if="beerRatings[result.beerId]" class="results__ratings">
              <li
                v-for="(r, i) in beerRatings[result.beerId]"
                :key="i"
                class="results__rating-row"
              >
                <span class="results__rating-score">{{ r.rating }}/10</span>
                <span v-if="r.notes" class="results__rating-notes">{{ r.notes }}</span>
                <span v-else class="results__rating-notes results__rating-notes--empty">No notes</span>
              </li>
            </ul>
          </div>
        </Transition>
      </li>
    </ol>
  </div>
</template>

<style scoped>
.results {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 1.25rem 1rem 2rem;
  max-width: 640px;
  margin: 0 auto;
  width: 100%;
}

.results__header {
  display: flex;
  align-items: center;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--color-border);
  margin-bottom: 1.25rem;
}

.results__title {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--color-text);
  line-height: 1.2;
}

.results__sub {
  font-size: 0.8rem;
  color: var(--color-text-muted);
  margin-top: 0.1rem;
}

.results__state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding: 3rem 1rem;
  color: var(--color-text-muted);
  font-size: 0.95rem;
}

.results__state--error { color: #fca5a5; }

.results__spinner {
  display: block;
  width: 2rem;
  height: 2rem;
  border: 3px solid var(--color-border);
  border-top-color: var(--color-sky);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

.results__spinner--sm {
  width: 1.25rem;
  height: 1.25rem;
  border-width: 2px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.results__breakdown-loading {
  display: flex;
  justify-content: center;
  padding: 0.75rem;
}

.results__list {
  list-style: none;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.results__item {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  overflow: hidden;
  transition: border-color 0.15s;
}

.results__item:has(.results__row--open) {
  border-color: var(--color-border-hover);
}

.results__row {
  display: flex;
  align-items: center;
  gap: 0.875rem;
  padding: 0.75rem 1rem;
  width: 100%;
  background: none;
  border: none;
  color: inherit;
  cursor: pointer;
  text-align: left;
}

.results__row:hover {
  background: rgba(255, 255, 255, 0.03);
}

.results__rank {
  font-size: 0.8rem;
  font-weight: 700;
  color: var(--color-text-muted);
  min-width: 1.25rem;
  text-align: center;
  flex-shrink: 0;
}

.results__item:nth-child(-n+3) .results__rank { font-size: 1rem; }

.results__beer {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
}

.results__beer-name {
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--color-text);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.results__brewer {
  font-size: 0.75rem;
  color: var(--color-text-muted);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.results__score {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.1rem;
  flex-shrink: 0;
}

.results__average {
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--color-sky);
}

.results__count {
  font-size: 0.7rem;
  color: var(--color-text-muted);
}

.results__chevron {
  color: var(--color-text-muted);
  flex-shrink: 0;
  transition: transform 0.2s ease;
}

.results__chevron--open {
  transform: rotate(180deg);
}

/* Breakdown panel */

.results__breakdown {
  border-top: 1px solid var(--color-border);
  padding: 0.5rem 0;
}

.results__breakdown-loading {
  display: flex;
  justify-content: center;
  padding: 0.75rem;
}

.results__ratings {
  list-style: none;
  padding: 0;
}

.results__rating-row {
  display: flex;
  align-items: baseline;
  gap: 0.75rem;
  padding: 0.4rem 1rem;
  border-bottom: 1px solid var(--color-border);
}

.results__rating-row:last-child {
  border-bottom: none;
}

.results__rating-score {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--color-sky);
  flex-shrink: 0;
  min-width: 2.5rem;
}

.results__rating-notes {
  font-size: 0.8rem;
  color: var(--color-text-muted);
}

.results__rating-notes--empty {
  font-style: italic;
  opacity: 0.5;
}

/* Expand transition */
.expand-enter-active,
.expand-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.expand-enter-from,
.expand-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}
</style>
