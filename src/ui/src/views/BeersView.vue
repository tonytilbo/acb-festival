<script setup lang="ts">
import { onMounted } from 'vue'
import { useBeersStore } from '@/stores/beers'
import { useRatingsStore } from '@/stores/ratings'
import BeerCard from '@/components/BeerCard.vue'
import ScrollFadeItem from '@/components/ScrollFadeItem.vue'

const beersStore = useBeersStore()
const ratingsStore = useRatingsStore()

onMounted(() => {
  beersStore.fetchBeers()
  ratingsStore.fetchRatings()
})
</script>

<template>
  <div class="beers">
    <header class="beers__header">
      <span class="beers__icon">🍺</span>
      <div>
        <h1 class="beers__title">ACB Beer Festival</h1>
        <p v-if="!beersStore.isLoading && beersStore.beers.length" class="beers__count">
          {{ beersStore.beers.length }} beers on offer
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

    <ul v-else class="beers__list">
      <ScrollFadeItem v-for="beer in beersStore.beers" :key="beer.id">
        <BeerCard :beer="beer" />
      </ScrollFadeItem>
    </ul>
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

.beers__title {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--color-slate-900);
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
  color: #dc2626;
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
  border: 1px solid #fecaca;
  color: #dc2626;
  border-radius: var(--radius);
  cursor: pointer;
  font-size: 0.875rem;
  transition: background 0.2s;
}

.beers__retry:hover {
  background: #fef2f2;
}
</style>
