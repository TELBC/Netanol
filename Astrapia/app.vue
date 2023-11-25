<template>
  <div>
    <TapasSidebar v-if="$route.path !== '/login'" />
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </div>
</template>

<script setup lang="ts">

</script>

