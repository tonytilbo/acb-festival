import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import { useUserStore } from './user'

export const useRatingsStore = defineStore('ratings', () => {
  // beerId -> { rating, notes }
  const ratings = ref<Record<number, { rating: number; notes: string | null }>>({})
  const submitting = ref<Set<number>>(new Set())
  const error = ref<string | null>(null)

  const userStore = useUserStore()

  async function fetchRatings() {
    if (!userStore.userId) return
    try {
      const response = await fetch(`/api/ratings?userId=${userStore.userId}`)
      if (!response.ok) throw new Error('Failed to load ratings')
      ratings.value = await response.json()
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Something went wrong'
    }
  }

  async function submitRating(beerId: number, rating: number, notes: string | null = null) {
    if (!userStore.userId) return
    submitting.value = new Set(submitting.value).add(beerId)
    error.value = null
    try {
      const response = await fetch('/api/ratings', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userId: userStore.userId, beerId, rating, notes }),
      })
      if (!response.ok) throw new Error('Failed to submit rating')
      ratings.value = { ...ratings.value, [beerId]: { rating, notes } }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Something went wrong'
    } finally {
      const next = new Set(submitting.value)
      next.delete(beerId)
      submitting.value = next
    }
  }

  async function clearRating(beerId: number) {
    if (!userStore.userId) return
    submitting.value = new Set(submitting.value).add(beerId)
    error.value = null
    try {
      const response = await fetch(
        `/api/ratings?userId=${userStore.userId}&beerId=${beerId}`,
        { method: 'DELETE' },
      )
      if (!response.ok) throw new Error('Failed to clear rating')
      const next = { ...ratings.value }
      delete next[beerId]
      ratings.value = next
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Something went wrong'
    } finally {
      const next = new Set(submitting.value)
      next.delete(beerId)
      submitting.value = next
    }
  }

  function getRating(beerId: number): number | null {
    return ratings.value[beerId]?.rating ?? null
  }

  function getNotes(beerId: number): string | null {
    return ratings.value[beerId]?.notes ?? null
  }

  function isSubmitting(beerId: number): boolean {
    return submitting.value.has(beerId)
  }

  return { fetchRatings, submitRating, clearRating, getRating, getNotes, isSubmitting, error }
})
