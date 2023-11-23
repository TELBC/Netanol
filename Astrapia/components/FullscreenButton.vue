<template>
  <button class="icon-button" @click="fullScreen">
    <font-awesome-icon icon="fa-solid fa-expand" />
  </button>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import { ref, defineProps } from 'vue';

const props = defineProps({
  elementId: {
    type: String,
    required: true,
  },
});

const isFullscreen = ref(false);

function fullScreen() {
  const fullscreen = document.querySelector(`#${props.elementId}`);

  if (!isFullscreen.value) fullscreen.requestFullscreen();
  else document.exitFullscreen();

  isFullscreen.value = !isFullscreen.value;
}
</script>



<style scoped>
.icon-button {
  color: #424242;
  background: none;
  border: none;
  cursor: pointer;
  outline: none;
}
</style>
