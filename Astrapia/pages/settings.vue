<template>
  <div>
    <tapas-alert-dialogue v-if="showAlert" />
    <button @click="showAlert = true">Alert</button>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  middleware: ["auth"]
})
let showAlert = false;
</script>

<style scoped>
button {
  margin-left: 50vw;
}
</style>
