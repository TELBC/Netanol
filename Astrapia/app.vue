<template>
  <div>
    <TapasAlertDialog
      :showAlert=alertState.showAlert
      :submit=alertState.submit
      :title=alertState.title
      :message=alertState.message  />
    <TapasSidebar v-if="$route.path !== '/login'" />
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </div>
</template>

<script setup lang="ts">
const alertState = ref({
  showAlert: false,
  submit: false,
  title: '',
  message: ''
});

function showAlert(submit: boolean, title: string, message: string) {
  alertState.value = {
    showAlert: true,
    submit,
    title,
    message
  };
}

function hideAlert() {
  alertState.value.showAlert = false;
}

provide('showAlert', showAlert);
provide('hideAlert', hideAlert);
</script>

