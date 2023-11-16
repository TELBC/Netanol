<template>
  <div>
    <TapasAlertDialog
      :showAlert=alertState.showAlert
      :submit=alertState.submit
      :title=alertState.title
      :message=alertState.message
      :onClick=alertState.onClick
    />
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
  message: '',
  onClick: (() => undefined) as Function,
});

function showAlert(submit: boolean, title: string, message: string, onClick: Function) {
  alertState.value = {
    showAlert: true,
    submit,
    title,
    message,
    onClick
  };
}

function hideAlert() {
  alertState.value.showAlert = false;
}

provide('showAlert', showAlert);
provide('hideAlert', hideAlert);
</script>

