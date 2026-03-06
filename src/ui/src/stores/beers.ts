import { ref } from 'vue'
import { defineStore } from 'pinia'

export interface Beer {
  id: number
  brewersName: string
  beerName: string
  style: string
  abv: number
  description: string
}

export const useBeersStore = defineStore('beers', () => {
  const beers = ref<Beer[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchBeers() {
    if (beers.value.length > 0) return
    isLoading.value = true
    error.value = null
    try {
      const response = await fetch('/api/beers')
      if (!response.ok) throw new Error('Failed to load beers')
      beers.value = await response.json()
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Something went wrong'
    } finally {
      isLoading.value = false
    }
  }

  return { beers, isLoading, error, fetchBeers }
})
