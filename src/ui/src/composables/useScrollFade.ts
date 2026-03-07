import { onMounted, onUnmounted, ref, type Ref } from 'vue'

export function useScrollFade(target: Ref<HTMLElement | null>) {
  const isVisible = ref(false)
  let observer: IntersectionObserver | null = null

  onMounted(() => {
    if (!target.value) return

    observer = new IntersectionObserver(
      ([entry]) => {
        if (entry?.isIntersecting) {
          isVisible.value = true
          observer?.disconnect()
        }
      },
      { threshold: 0.12 },
    )

    observer.observe(target.value)
  })

  onUnmounted(() => observer?.disconnect())

  return { isVisible }
}
