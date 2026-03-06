<script setup lang="ts">
import { ref } from 'vue'
import { useScrollFade } from '@/composables/useScrollFade'

const el = ref<HTMLElement | null>(null)
const { isVisible } = useScrollFade(el)
</script>

<template>
  <li ref="el" class="fade-item" :class="{ 'fade-item--visible': isVisible }">
    <slot />
  </li>
</template>

<style scoped>
.fade-item {
  opacity: 0;
  transform: translateY(16px);
  transition: opacity 0.4s ease, transform 0.4s ease;
}

.fade-item--visible {
  opacity: 1;
  transform: translateY(0);
}

@media (prefers-reduced-motion: reduce) {
  .fade-item {
    opacity: 1;
    transform: none;
    transition: none;
  }
}
</style>
