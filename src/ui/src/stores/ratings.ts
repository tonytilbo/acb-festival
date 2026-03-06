import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import { useUserStore } from './user'

export const useRatingsStore = defineStore('ratings', () => {
  // beerId -> rating
  const ratings = ref<Record<number, number>>({})
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

  async function submitRating(beerId: number, rating: number) {
    if (!userStore.userId) return
    submitting.value = new Set(submitting.value).add(beerId)
    error.value = null
    try {
      const response = await fetch('/api/ratings', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userId: userStore.userId, beerId, rating }),
      })
      if (!response.ok) throw new Error('Failed to submit rating')
      ratings.value = { ...ratings.value, [beerId]: rating }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Something went wrong'
    } finally {
      const next = new Set(submitting.value)
      next.delete(beerId)
      submitting.value = next
    }
  }

  function getRating(beerId: number): number | null {
    return ratings.value[beerId] ?? null
  }

  function isSubmitting(beerId: number): boolean {
    return submitting.value.has(beerId)
  }

  return { fetchRatings, submitRating, getRating, isSubmitting, error }
})
