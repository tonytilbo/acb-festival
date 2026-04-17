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
      <img src="/images/cloud.png" alt="" class="welcome__cloud" aria-hidden="true" />
      <img src="/images/logo-text.png" alt="Shed Brewers Beer Festival" class="welcome__logo" />

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
        <span v-else>Continue</span>
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
  padding: 50px 1.25rem 2rem;
  background: linear-gradient(160deg, #0D0D0D 0%, #111111 50%, #0D0D0D 100%);
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


.welcome__cloud {
  width: 220px;
  height: auto;
  position: relative;
  z-index: 2;
  margin-bottom: -50px;
  filter: drop-shadow(0 6px 20px rgba(0, 0, 0, 0.5));
}

.welcome__logo {
  width: 100%;
  max-width: 360px;
  height: auto;
  position: relative;
  z-index: 1;
  filter: drop-shadow(0 4px 20px rgba(212, 137, 58, 0.35));
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
  color: #111111;
  font-size: 1.1rem;
  font-weight: 700;
  border: none;
  border-radius: var(--radius-lg);
  cursor: pointer;
  transition: background 0.2s, transform 0.1s, opacity 0.2s;
  letter-spacing: 0.01em;
  box-shadow: 0 4px 14px rgba(212, 137, 58, 0.4);
  font-family: 'Barlow Condensed', sans-serif;
  font-weight: 800;
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
