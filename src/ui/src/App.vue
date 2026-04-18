<script setup lang="ts">
import { onMounted } from 'vue'
import { RouterView, useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'

const router = useRouter()
const userStore = useUserStore()

onMounted(async () => {
  await userStore.validateSession()
  if (!userStore.isRegistered) {
    router.replace({ name: 'home' })
  }
})
</script>

<template>
  <RouterView />
</template>
