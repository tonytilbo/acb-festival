import { createRouter, createWebHistory } from 'vue-router'
import { useUserStore } from '@/stores/user'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/beers',
      name: 'beers',
      component: () => import('../views/BeersView.vue'),
      meta: { requiresAuth: true },
    },
  ],
})

router.beforeEach((to) => {
  const userStore = useUserStore()

  // Already registered — skip welcome screen
  if (to.name === 'home' && userStore.isRegistered) {
    return { name: 'beers' }
  }

  // Requires registration — send to welcome screen
  if (to.meta.requiresAuth && !userStore.isRegistered) {
    return { name: 'home' }
  }
})

export default router
