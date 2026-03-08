<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useBeersStore } from '@/stores/beers'

interface BeerResult {
  beerId: number
  average: number
  count: number
}

const beersStore = useBeersStore()
const results = ref<BeerResult[]>([])
const isLoading = ref(true)
const error = ref<string | null>(null)

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
        <span class="results__rank">{{ index + 1 }}</span>
        <div class="results__beer">
          <span class="results__beer-name">{{ beerName(result.beerId) }}</span>
          <span class="results__brewer">{{ brewerName(result.beerId) }}</span>
        </div>
        <div class="results__score">
          <span class="results__average">{{ result.average.toFixed(1) }}</span>
          <span class="results__count">{{ result.count }} {{ result.count === 1 ? 'rating' : 'ratings' }}</span>
        </div>
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

@keyframes spin {
  to { transform: rotate(360deg); }
}

.results__list {
  list-style: none;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.results__item {
  display: flex;
  align-items: center;
  gap: 0.875rem;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 0.75rem 1rem;
}

.results__rank {
  font-size: 0.8rem;
  font-weight: 700;
  color: var(--color-text-muted);
  min-width: 1.25rem;
  text-align: center;
}

.results__item:nth-child(1) .results__rank { color: #fde047; }
.results__item:nth-child(2) .results__rank { color: #94a3b8; }
.results__item:nth-child(3) .results__rank { color: #fb923c; }

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
</style>
