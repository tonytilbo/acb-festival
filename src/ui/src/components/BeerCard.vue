<script setup lang="ts">
import { ref } from 'vue'
import type { Beer } from '@/stores/beers'
import { useRatingsStore } from '@/stores/ratings'
import RatingPicker from './RatingPicker.vue'

const props = defineProps<{ beer: Beer }>()

const ratingsStore = useRatingsStore()
const pickerOpen = ref(false)
const descriptionOpen = ref(false)

const currentRating = () => ratingsStore.getRating(props.beer.id)
const isSubmitting = () => ratingsStore.isSubmitting(props.beer.id)

function togglePicker() {
  pickerOpen.value = !pickerOpen.value
}

async function handleRate(value: number) {
  await ratingsStore.submitRating(props.beer.id, value)
  pickerOpen.value = false
}

function abvColour(abv: number): string {
  if (abv < 4.0) return 'abv--low'
  if (abv < 6.0) return 'abv--mid'
  return 'abv--high'
}
</script>

<template>
  <article class="card">
    <button
      class="card__top"
      :aria-expanded="descriptionOpen"
      :aria-controls="`desc-${beer.id}`"
      @click="descriptionOpen = !descriptionOpen"
    >
      <div class="card__names">
        <h2 class="card__beer-name">{{ beer.beerName }}</h2>
        <p class="card__brewery">{{ beer.brewersName }}</p>
      </div>
      <div class="card__right">
        <div class="card__meta">
          <span class="card__style">{{ beer.style }}</span>
          <span class="card__abv" :class="abvColour(beer.abv)">{{ beer.abv.toFixed(1) }}%</span>
          <span class="card__serving" :class="beer.servingMethod === 'Cask' ? 'serving--cask' : 'serving--keg'">
            {{ beer.servingMethod === 'Cask' ? '🪣 Cask' : '🔩 Keg' }}
          </span>
        </div>
      </div>
    </button>

    <div
      :id="`desc-${beer.id}`"
      class="card__description"
      :class="{ 'card__description--open': descriptionOpen }"
      @click="descriptionOpen = !descriptionOpen"
    >
      <p>{{ beer.description }}</p>
      <div class="card__description-fade" aria-hidden="true" />
    </div>

    <div class="card__footer">
      <Transition name="slide">
        <RatingPicker
          v-if="pickerOpen"
          :current="currentRating()"
          :submitting="isSubmitting()"
          @rate="handleRate"
        />
      </Transition>

      <div class="card__actions">
        <button
          class="card__rate-btn"
          :class="{ 'card__rate-btn--rated': currentRating() !== null, 'card__rate-btn--open': pickerOpen }"
          @click="togglePicker"
        >
          <template v-if="isSubmitting()">Saving…</template>
          <template v-else-if="currentRating() !== null">
            ★ {{ currentRating() }}/10 · {{ pickerOpen ? 'Cancel' : 'Change' }}
          </template>
          <template v-else>
            {{ pickerOpen ? '✕ Cancel' : '★ Rate this beer' }}
          </template>
        </button>
      </div>
    </div>
  </article>
</template>

<style scoped>
.card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 1rem 1.125rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  transition: border-color 0.2s, box-shadow 0.2s;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
}

.card:hover {
  border-color: var(--color-border-hover);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.card__top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 0.75rem;
  background: none;
  border: none;
  padding: 0;
  margin: 0;
  width: 100%;
  text-align: left;
  cursor: pointer;
  color: inherit;
}

.card__names {
  flex: 1;
  min-width: 0;
}

.card__beer-name {
  font-size: 1.05rem;
  font-weight: 700;
  color: var(--color-slate-900);
  line-height: 1.3;
}

.card__brewery {
  font-size: 0.8rem;
  color: var(--color-text-muted);
  margin-top: 0.15rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.card__right {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.4rem;
  flex-shrink: 0;
}

.card__meta {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.35rem;
}

.card__style {
  font-size: 0.7rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
  text-align: right;
  max-width: 110px;
  line-height: 1.3;
}

.card__abv {
  font-size: 0.8rem;
  font-weight: 700;
  padding: 0.2rem 0.5rem;
  border-radius: 999px;
  letter-spacing: 0.02em;
}

.abv--low  { background: #dcfce7; color: #166534; }
.abv--mid  { background: #dbeafe; color: #1e40af; }
.abv--high { background: #fee2e2; color: #991b1b; }

.card__serving {
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.2rem 0.5rem;
  border-radius: 999px;
  letter-spacing: 0.01em;
}

.serving--cask { background: #fef9c3; color: #854d0e; }
.serving--keg  { background: #f1f5f9; color: #475569; }

.card__description {
  position: relative;
  font-size: 0.875rem;
  color: var(--color-text-muted);
  line-height: 1.65;
  max-height: 1.65em;
  overflow: hidden;
  transition: max-height 0.35s ease;
  cursor: pointer;
}

.card__description--open {
  max-height: 20lh;
}

.card__description-fade {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 1.65em;
  background: linear-gradient(to bottom, transparent, var(--color-surface));
  transition: opacity 0.2s ease;
  pointer-events: none;
}

.card__description--open .card__description-fade {
  opacity: 0;
}

.card__footer {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
  border-top: 1px solid var(--color-border);
  padding-top: 0.75rem;
}

.card__actions {
  display: flex;
  justify-content: flex-end;
}

.card__rate-btn {
  background: var(--color-sky);
  border: none;
  color: #ffffff;
  font-size: 0.8rem;
  font-weight: 700;
  padding: 0.4rem 0.9rem;
  border-radius: 999px;
  cursor: pointer;
  transition: background 0.2s, transform 0.1s;
  letter-spacing: 0.01em;
}

.card__rate-btn:hover {
  background: var(--color-sky-dark);
}

.card__rate-btn:active {
  transform: scale(0.96);
}

.card__rate-btn--rated {
  background: #f0f9ff;
  color: var(--color-sky-dark);
  border: 1px solid var(--color-sky-light);
}

.card__rate-btn--rated:hover {
  background: #e0f2fe;
}

.card__rate-btn--open {
  background: var(--color-slate-100);
  color: var(--color-text-muted);
  border: 1px solid var(--color-border);
}

/* Slide transition for the rating picker */
.slide-enter-active,
.slide-leave-active {
  transition: opacity 0.2s, transform 0.2s;
}
.slide-enter-from,
.slide-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}
</style>
