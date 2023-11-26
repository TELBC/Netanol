<template>
  <div class="slider-container">
    <span class="slider-label">{{ label }}</span>
    <input type="range" :min="min" :max="max" v-model="value" class="slider">
  </div>
</template>

<script setup lang="ts">
import { watchEffect } from 'vue';
import { watch } from 'vue';
import { ref } from 'vue';

const props = defineProps({
  label: {
    type: String,
    required: true,
  },
  min: {
    type: Number,
    default: 1,
  },
  max: {
    type: Number,
    default: 60,
  },
  modelValue: {
    type: Number,
    default: 2,
  },
});

const emit = defineEmits(['update:modelValue']);

const value = ref(props.modelValue);

watchEffect(() => {
  value.value = props.modelValue;
});

watch(value, (newValue) => {
  emit('update:modelValue', newValue);
});
</script>

<style scoped>
.slider-container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: fixed;
  right: 10px;
  bottom: 10px;
  background-color: #D7DFE7;
  border: 1px solid #424242;
  border-radius: 5px;
  padding: 10px;
}
.slider-label {
  font-family: 'Open Sans', sans-serif;
  margin-right: 10px;
}
.slider {
  width: 100%;
  height: 15px;
  outline: none;
  background-color: #D7DFE7;
  border-radius: 50%;
}

.slider::-webkit-slider-runnable-track {
  background: #537B87;
}

.slider::-moz-range-track {
  background: #537B87;
}

.slider::-webkit-slider-thumb {
  -webkit-appearance: media-sliderthumb;
  width: 15px;
  height: 15px;
  background: #424242;
  cursor: pointer;
  border: 1px solid #424242;
  border-radius: 50%;
}

.slider::-moz-range-thumb {
  width: 15px;
  height: 15px;
  background: #424242;
  cursor: pointer;
  border: 1px solid #424242;
  border-radius: 50%;
}
</style>
