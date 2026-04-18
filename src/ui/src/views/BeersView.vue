<script setup lang="ts">
import { onMounted, computed, ref } from 'vue'
import { useBeersStore } from '@/stores/beers'
import { useRatingsStore } from '@/stores/ratings'
import BeerCard from '@/components/BeerCard.vue'
import ScrollFadeItem from '@/components/ScrollFadeItem.vue'

const beersStore = useBeersStore()
const ratingsStore = useRatingsStore()
const notesCounts = ref<Record<number, number>>({})

const remaining = computed(() =>
  beersStore.beers.filter(b => ratingsStore.getRating(b.id) === null).length
)

const sortedBeers = computed(() =>
  [...beersStore.beers].sort((a, b) => {
    const aRated = ratingsStore.getRating(a.id) !== null
    const bRated = ratingsStore.getRating(b.id) !== null
    if (aRated !== bRated) return aRated ? 1 : -1
    return a.beerName.localeCompare(b.beerName)
  })
)

onMounted(async () => {
  beersStore.fetchBeers()
  ratingsStore.fetchRatings()
  try {
    const res = await fetch('/api/notes-counts')
    if (res.ok) notesCounts.value = await res.json()
  } catch {}
})
</script>

<template>
  <div class="beers">
    <header class="beers__header">
      <span class="beers__icon">🍺</span>
      <div class="beers__header-text">
        <h1 class="beers__title">Shed Brewers Festival</h1>
        <p v-if="!beersStore.isLoading && beersStore.beers.length" class="beers__count">
          <template v-if="remaining === 0">All beers tasted!</template>
          <template v-else>{{ remaining }} of {{ beersStore.beers.length }} left to try</template>
        </p>
      </div>
    </header>

    <div v-if="beersStore.isLoading" class="beers__state">
      <span class="beers__spinner" aria-hidden="true" />
      <p>Loading beers…</p>
    </div>

    <div v-else-if="beersStore.error" class="beers__state beers__state--error" role="alert">
      <p>{{ beersStore.error }}</p>
      <button class="beers__retry" @click="beersStore.fetchBeers()">Try again</button>
    </div>

    <TransitionGroup v-else tag="ul" name="beer-list" class="beers__list">
      <ScrollFadeItem v-for="beer in sortedBeers" :key="beer.id">
        <BeerCard :beer="beer" :notes-count="notesCounts[beer.id] ?? 0" />
      </ScrollFadeItem>
    </TransitionGroup>
  </div>
</template>

<style scoped>
.beers {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 1.25rem 1rem 2rem;
  max-width: 640px;
  margin: 0 auto;
  width: 100%;
}

.beers__header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--color-border);
  margin-bottom: 1.25rem;
}

.beers__icon {
  font-size: 2rem;
  line-height: 1;
}

.beers__header-text {
  flex: 1;
}


.beers__title {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--color-text);
  line-height: 1.2;
}

.beers__count {
  font-size: 0.8rem;
  color: var(--color-text-muted);
  margin-top: 0.1rem;
}

.beers__list {
  list-style: none;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
}

.beer-list-move {
  transition: transform 0.5s ease;
}

.beers__state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding: 3rem 1rem;
  color: var(--color-text-muted);
  font-size: 0.95rem;
}

.beers__state--error {
  color: #fca5a5;
}

.beers__spinner {
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

.beers__retry {
  padding: 0.5rem 1.25rem;
  background: transparent;
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #fca5a5;
  border-radius: var(--radius);
  cursor: pointer;
  font-size: 0.875rem;
  transition: background 0.2s;
}

.beers__retry:hover {
  background: rgba(239, 68, 68, 0.1);
}
</style>
