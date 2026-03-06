<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'

const router = useRouter()
const userStore = useUserStore()

async function enter() {
  await userStore.register()
  router.push({ name: 'beers' })
}
</script>

<template>
  <main class="welcome">
    <div class="welcome__content">
      <div class="welcome__badge">🍺</div>

      <h1 class="welcome__title">ACB Beer Festival</h1>
      <p class="welcome__subtitle">Discover this year's finest ales, lagers &amp; craft brews</p>

      <div class="welcome__divider" />

      <p class="welcome__body">
        Browse our full selection of beers, find your favourites, and make the most of your
        festival experience.
      </p>

      <p v-if="userStore.error" class="welcome__error" role="alert">
        {{ userStore.error }} — please try again.
      </p>

      <button
        class="welcome__cta"
        :disabled="userStore.isLoading"
        @click="enter"
      >
        <span v-if="userStore.isLoading">Setting up your experience…</span>
        <span v-else>Enter the Festival</span>
      </button>
    </div>
  </main>
</template>

<style scoped>
.welcome {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100dvh;
  padding: 2rem 1.25rem;
  background: linear-gradient(160deg, #0c1a2e 0%, #0f172a 50%, #0c1a2e 100%);
}

.welcome__content {
  width: 100%;
  max-width: 420px;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1.25rem;
}

.welcome__badge {
  font-size: 4rem;
  line-height: 1;
  filter: drop-shadow(0 4px 16px rgba(56, 189, 248, 0.4));
}

.welcome__title {
  font-size: 2.25rem;
  font-weight: 800;
  letter-spacing: -0.02em;
  color: var(--color-text);
  line-height: 1.1;
}

.welcome__subtitle {
  font-size: 1.05rem;
  color: var(--color-text-muted);
  line-height: 1.5;
}

.welcome__divider {
  width: 48px;
  height: 3px;
  background: var(--color-sky);
  border-radius: 2px;
}

.welcome__body {
  font-size: 0.95rem;
  color: var(--color-text-muted);
  line-height: 1.7;
  max-width: 320px;
}

.welcome__error {
  font-size: 0.875rem;
  color: #fca5a5;
  background: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.3);
  border-radius: var(--radius);
  padding: 0.75rem 1rem;
  width: 100%;
}

.welcome__cta {
  margin-top: 0.5rem;
  width: 100%;
  padding: 1rem 2rem;
  background: var(--color-sky);
  color: #ffffff;
  font-size: 1.1rem;
  font-weight: 700;
  border: none;
  border-radius: var(--radius-lg);
  cursor: pointer;
  transition: background 0.2s, transform 0.1s, opacity 0.2s;
  letter-spacing: 0.01em;
  box-shadow: 0 4px 14px rgba(56, 189, 248, 0.3);
}

.welcome__cta:hover:not(:disabled) {
  background: var(--color-sky-dark);
}

.welcome__cta:active:not(:disabled) {
  transform: scale(0.98);
}

.welcome__cta:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
