import { ref, computed } from 'vue'
import { defineStore } from 'pinia'

const COOKIE_NAME = 'acb_user_id'
const COOKIE_MAX_AGE = 60 * 60 * 24 * 365 // 1 year in seconds

function getCookie(name: string): string | null {
  const match = document.cookie.match(new RegExp(`(?:^|; )${name}=([^;]*)`))
  return match ? decodeURIComponent(match[1]!) : null
}

function setCookie(name: string, value: string, maxAge: number) {
  document.cookie = `${name}=${encodeURIComponent(value)}; max-age=${maxAge}; path=/; SameSite=Lax`
}

export const useUserStore = defineStore('user', () => {
  const userId = ref<string | null>(getCookie(COOKIE_NAME))
  const isRegistered = computed(() => userId.value !== null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function register() {
    isLoading.value = true
    error.value = null
    try {
      const response = await fetch('/api/register', { method: 'POST' })
      if (!response.ok) throw new Error('Registration failed')
      const data: { userId: string } = await response.json()
      userId.value = data.userId
      setCookie(COOKIE_NAME, data.userId, COOKIE_MAX_AGE)
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Something went wrong'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  function clearSession() {
    userId.value = null
    setCookie(COOKIE_NAME, '', 0)
  }

  async function validateSession() {
    if (!userId.value) return
    try {
      const response = await fetch(`/api/validate?userId=${encodeURIComponent(userId.value)}`)
      if (response.status === 404) clearSession()
    } catch {
      // network error — leave session intact and let user retry
    }
  }

  return { userId, isRegistered, isLoading, error, register, validateSession }
})
