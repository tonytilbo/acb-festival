<script setup lang="ts">
import { ref, onMounted } from 'vue'

interface Summary {
  totalUsers: number
  firstRegistered: string | null
  lastRegistered: string | null
  totalRatings: number
  beersRated: number
  totalBeers: number
}

const summary = ref<Summary | null>(null)
const isLoading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    const response = await fetch('/api/summary')
    if (!response.ok) throw new Error('Failed to load summary')
    summary.value = await response.json()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Something went wrong'
  } finally {
    isLoading.value = false
  }
})

function formatDate(iso: string | null): string {
  if (!iso) return '—'
  return new Intl.DateTimeFormat('en-GB', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(iso))
}
</script>

<template>
  <div class="summary">
    <header class="summary__header">
      <div>
        <h1 class="summary__title">Summary</h1>
        <p class="summary__sub">Live festival stats</p>
      </div>
    </header>

    <div v-if="isLoading" class="summary__state">
      <span class="summary__spinner" aria-hidden="true" />
      <p>Loading…</p>
    </div>

    <div v-else-if="error" class="summary__state summary__state--error" role="alert">
      <p>{{ error }}</p>
    </div>

    <dl v-else-if="summary" class="summary__stats">
      <div class="summary__stat">
        <dt>Total registered</dt>
        <dd>{{ summary.totalUsers }}</dd>
      </div>
      <div class="summary__stat">
        <dt>First registered</dt>
        <dd>{{ formatDate(summary.firstRegistered) }}</dd>
      </div>
      <div class="summary__stat">
        <dt>Last registered</dt>
        <dd>{{ formatDate(summary.lastRegistered) }}</dd>
      </div>
      <div class="summary__stat">
        <dt>Total ratings submitted</dt>
        <dd>{{ summary.totalRatings }}</dd>
      </div>
      <div class="summary__stat">
        <dt>Beers that have been rated</dt>
        <dd>{{ summary.beersRated }} of {{ summary.totalBeers }}</dd>
      </div>
    </dl>
  </div>
</template>

<style scoped>
.summary {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 1.25rem 1rem 2rem;
  max-width: 640px;
  margin: 0 auto;
  width: 100%;
}

.summary__header {
  display: flex;
  align-items: center;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--color-border);
  margin-bottom: 1.25rem;
}

.summary__title {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--color-text);
  line-height: 1.2;
}

.summary__sub {
  font-size: 0.8rem;
  color: var(--color-text-muted);
  margin-top: 0.1rem;
}

.summary__state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding: 3rem 1rem;
  color: var(--color-text-muted);
  font-size: 0.95rem;
}

.summary__state--error { color: #fca5a5; }

.summary__spinner {
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

.summary__stats {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.summary__stat {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  padding: 0.875rem 1.125rem;
  gap: 1rem;
}

.summary__stat dt {
  font-size: 0.875rem;
  color: var(--color-text-muted);
}

.summary__stat dd {
  font-size: 1rem;
  font-weight: 700;
  color: var(--color-text);
  text-align: right;
}
</style>
