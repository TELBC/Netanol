import { useRoute } from 'vue-router';
export default defineNuxtRouteMiddleware(async () => {
  if (process.client) {
    const token = sessionStorage.getItem('jwtToken');
    if (!token && useRoute().path !== '/login') {
      return navigateTo('/login');
    }
  }
});
