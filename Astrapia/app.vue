<template>
    <Login v-if="!auth.isAuthenticated" />

    <div v-if="auth.isAuthenticated">
        <TapasSidebar v-if="$route.path !== '/login'" />

        <NuxtLayout>
            <NuxtPage />
        </NuxtLayout>
    </div>
</template>

<script setup lang="ts">
import Login from "~/components/Login.vue"
import {onMounted} from 'vue'
import {useAuth} from "~/composables/auth"
import TapasSidebar from "~/components/TapasSidebar.vue";
import AuthService from "~/services/authService";

const auth = useAuth()
auth.value.message = 'Loading web page'
onMounted(async () => {
  auth.value.message = "Checking authentication status"
  const response = await AuthService.getStatus();

  if (response) {
    auth.value.message = 'Authenticated'
  } else {
    auth.value.message = 'Not authenticated... Please log in'
  }
})
</script>
