<template>
  <div class="slide-menu" :class="{ 'slide-up': isOpen }">
    <div class="slider-container">
      <input type="range" min="50" max="800" v-model="distance" @input="updateDistance" class="slider" title="Change link distance">
      <input type="range" min="200" max="3000" v-model="force" @input="updateSim" class="slider" title="Change Simulation force">
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";

const distance = ref(100);
const force = ref(500);

const props = defineProps<{
  isOpen: Boolean
}>();

const emit = defineEmits<{
  updateDistance: [distance: number],
  updateSim: [force: number]
}>();

const updateDistance = (event: InputEvent) => {
  const newDistance = Number((event.target as HTMLInputElement).value);
  emit('updateDistance', newDistance);
};

const updateSim = (event: InputEvent) => {
  const newForce = Number((event.target as HTMLInputElement).value);
  emit('updateSim', newForce);
};
</script>

<style scoped>
.slide-menu {
  position: fixed;
  bottom: -60px;
  left: 580px;
  transform: translateX(-50%);
  background-color: #e0e0e0;
  transition: transform 0.5s ease, bottom 0.5s ease;
  padding: 5px;
  width: auto;
  border-radius: 4px;
  z-index: 2;
}

.slide-up {
  bottom: 0;
  transform: translate(-50%, calc(-27%));
}

.slider-container {
  display: flex;
  flex-direction: column;
}

.slider {
  width: 100px;
  outline: none;
  background-color: #e0e0e0;
  border-radius: 50%;
  margin-top: 4px;
}

.slider::-webkit-slider-runnable-track,
.slider::-moz-range-track {
  background: #bdbcbc;
  height: 3px;
  border-radius: 3px;
}


.slider::-webkit-slider-thumb {
  -webkit-appearance: media-sliderthumb;
  width: 10px;
  height: 10px;
  background: #8d8d8d;
  cursor: pointer;
  border: 1px solid #8d8d8d;
  border-radius: 50%;
}

.slider::-webkit-slider-thumb:hover {
  background: #797878;
}

.slider::-moz-range-thumb {
  width: 10px;
  height: 10px;
  background: #8d8d8d;
  cursor: pointer;
  border: 1px solid #8d8d8d;
  border-radius: 50%;
}

.slider::-moz-range-thumb:hover {
  background: #797878;
}
</style>
