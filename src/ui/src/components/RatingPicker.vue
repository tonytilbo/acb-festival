<script setup lang="ts">
defineProps<{
  current: number | null
  submitting: boolean
}>()

const emit = defineEmits<{
  rate: [value: number]
}>()

const scores = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]

function scoreClass(score: number, current: number | null): string {
  if (current === null) return 'pip'
  if (score === current) return 'pip pip--selected'
  if (score < current) return 'pip pip--filled'
  return 'pip pip--empty'
}
</script>

<template>
  <div class="picker" :class="{ 'picker--disabled': submitting }" role="group" aria-label="Rate this beer">
    <button
      v-for="score in scores"
      :key="score"
      class="pip"
      :class="scoreClass(score, current)"
      :aria-label="`Rate ${score} out of 10`"
      :aria-pressed="current === score"
      :disabled="submitting"
      @click="emit('rate', score)"
    >
      {{ score }}
    </button>
  </div>
</template>

<style scoped>
.picker {
  display: flex;
  gap: 0.3rem;
  flex-wrap: nowrap;
}

.picker--disabled {
  opacity: 0.5;
  pointer-events: none;
}

.pip {
  flex: 1;
  min-width: 0;
  aspect-ratio: 1;
  border-radius: 6px;
  border: 1px solid var(--color-border);
  background: transparent;
  color: var(--color-text-muted);
  font-size: 0.75rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s, border-color 0.15s, color 0.15s, transform 0.1s;
  padding: 0;
  line-height: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}

.pip:hover:not(:disabled) {
  border-color: var(--color-sky);
  color: var(--color-sky);
  transform: scale(1.1);
}

.pip--filled {
  background: rgba(56, 189, 248, 0.15);
  border-color: rgba(56, 189, 248, 0.4);
  color: var(--color-sky);
}

.pip--selected {
  background: var(--color-sky);
  border-color: var(--color-sky);
  color: #0f172a;
  font-weight: 800;
  transform: scale(1.1);
}

.pip--empty {
  border-color: var(--color-border);
  color: var(--color-text-muted);
  opacity: 0.4;
}
</style>
