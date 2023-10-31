<template>
  <div>
    <TapasSidebar v-if="$route.path !== '/login'" />
    <!-- <tapas-alert-dialogue /> -->
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </div>
</template>

<script setup lang="ts">

</script>

