<template>
    <Login v-if="!auth.isAuthenticated" />

    <div v-if="auth.isAuthenticated">
      <Sidebar />
      <div class="content-with-sidebar">
        <NuxtLayout>
          <NuxtPage />
        </NuxtLayout>
      </div>
    </div>
</template>

<style scoped>
.content-with-sidebar {
  margin-left: 3vw;
}
</style>

<script setup lang="ts">
import Login from "~/components/Login.vue"
import {onMounted} from 'vue'
import {useAuth} from "~/composables/auth"
import AuthService from "~/services/authService";
import Sidebar from "./components/Sidebar.vue";

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
