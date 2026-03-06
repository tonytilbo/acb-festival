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

async function handleClear() {
  await ratingsStore.clearRating(props.beer.id)
  pickerOpen.value = false
}</script>

<template>
  <article class="card" :class="{ 'card--tasted': currentRating() !== null }">
    <button
      class="card__top"
      :aria-expanded="descriptionOpen"
      :aria-controls="`desc-${beer.id}`"
      @click="descriptionOpen = !descriptionOpen"
    >
      <div class="card__names">
        <div class="card__title-row">
          <h2 class="card__beer-name">{{ beer.beerName }}</h2>
          <span class="card__abv">{{ beer.abv.toFixed(1) }}%</span>
        </div>
        <p class="card__sub">{{ beer.style }}</p>
        <p class="card__brewery">{{ beer.brewersName }}</p>
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
          @clear="handleClear"
        />
      </Transition>

      <div class="card__actions">
        <span class="card__serving" :class="beer.servingMethod === 'Cask' ? 'serving--cask' : 'serving--keg'">
          {{ beer.servingMethod }}
        </span>
        <button
          class="card__rate-btn"
          :class="{ 'card__rate-btn--rated': currentRating() !== null, 'card__rate-btn--open': pickerOpen }"
          @click="togglePicker"
        >
          <template v-if="isSubmitting()">Saving…</template>
          <template v-else-if="currentRating() !== null">
            ★ {{ currentRating() }}/10
          </template>
          <template v-else>
            {{ pickerOpen ? '✕ Cancel' : '★ Rate' }}
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
  gap: 0.6rem;
  transition: border-color 0.2s, box-shadow 0.2s;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
}

.card:hover {
  border-color: var(--color-border-hover);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.card--tasted {
  filter: saturate(0.6) brightness(0.88);
  transition: filter 0.3s ease;
}

.card--tasted:hover {
  filter: saturate(0.8) brightness(0.95);
}

.card__top {
  display: flex;
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

.card__title-row {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: 0.5rem;
}

.card__beer-name {
  font-size: 1.05rem;
  font-weight: 700;
  color: var(--color-text);
  line-height: 1.3;
}

.card__abv {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--color-text-muted);
  flex-shrink: 0;
}

.card__sub {
  font-size: 0.78rem;
  color: var(--color-text-muted);
  margin-top: 0.15rem;
}

.card__brewery {
  font-size: 0.75rem;
  color: var(--color-border-hover);
  margin-top: 0.1rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

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
  padding-top: 0.25rem;
}

.card__actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card__serving {
  font-size: 0.8rem;
  font-weight: 700;
  padding: 0.4rem 0.9rem;
  border-radius: 999px;
  letter-spacing: 0.01em;
}

.serving--cask { background: rgba(234, 179, 8, 0.15);  color: #fde047; }
.serving--keg  { background: rgba(148, 163, 184, 0.15); color: #94a3b8; }

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
  background: rgba(56, 189, 248, 0.1);
  color: var(--color-sky);
  border: 1px solid rgba(56, 189, 248, 0.3);
}

.card__rate-btn--rated:hover {
  background: rgba(56, 189, 248, 0.2);
}

.card__rate-btn--open {
  background: var(--color-border);
  color: var(--color-text-muted);
  border: 1px solid var(--color-border-hover);
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
