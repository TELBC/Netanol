<template>
  <div class="slide-menu" :class="{ 'slide-up': isOpen }">
    <div class="internal-container">
      <h3 class="title">{{ title }}</h3>
      <span class="description-box">
        <label class="description">{{description}}</label>
        <label class="value" :title="inputValue">{{inputValue}}</label>
        <label class="unit">{{selectedUnit}}</label>
      </span>
      <div class="input-container">
        <input class="input" v-model="inputValue" @input="handleInput" step="1" pattern="\d*" maxlength="10"/>
        <select class="dropdown" v-model="selectedUnit">
          <option value="sec">sec</option>
          <option value="min">min</option>
          <option value="hour">hrs</option>
        </select>
        <button class="apply" @click="applyChanges">Apply</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

const props = defineProps<{
  isOpen: Boolean;
  title?: string;
  description?: string;
}>();

const inputValue = ref<number>(5);
const selectedUnit = ref<string>("sec");

const handleInput = (event: Event) => {
  const target = event.target as HTMLInputElement;
  const parsedValue = parseInt(target.value);
  if (!isNaN(parsedValue)) {
    inputValue.value = parsedValue;
  } else {
    inputValue.value = 1;
  }
};

const emit = defineEmits<{
  change: [check: number],
  valueChange: [rawValue: number];
}>();

const applyChanges = () => {
  let multiplier = 1;
  if (selectedUnit.value === "min") {
    multiplier = 60;
  } else if (selectedUnit.value === "hour") {
    multiplier = 3600;
  }
  emit('change', inputValue.value * multiplier);
  emit('valueChange', inputValue.value)
};
</script>

<style scoped>
.slide-menu {
  font-family: 'Open Sans', sans-serif;
  font-size: 0.8rem;
  position: fixed;
  bottom: -70px;
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
  transform: translate(-50%, calc(-21%));
}

.internal-container {
  display: flex;
  flex-direction: column;
  padding-bottom: 10px;
}

.title {
  margin-top: -2px;
  margin-bottom: -5px;
}

.input-container {
  display: flex;
  align-items: center;
}

.input {
  flex: 1;
  font-size: 0.6rem;
  padding: 4px 3px;
  width: 60px;
  height: 19px;
}

.apply {
  font-size: 0.7rem;
  padding: 3px 3px;
  width: fit-content;
  height: 19px;
}

.dropdown {
  font-family: 'Open Sans', sans-serif;
  font-size: 0.6rem;
  padding: 4px 3px;
  width: fit-content;
  height: 21px;
  border: 1px solid #a2a2a2;
  background: #e0e0e0;
  color: #646464;
  margin-left: 2px;
  margin-right: 2px;
}

.input,
.apply {
  font-family: 'Open Sans', sans-serif;
  height: fit-content;
  border: 1px solid #a2a2a2;
  background: #e0e0e0;
  color: #646464;
  border-radius: 4px;
  transition: background-color 0.3s, color 0.3s;
}

.input:hover,
.apply:hover,
.dropdown:hover{
  background: #d0d0d0;
}

.input:focus,
.apply:focus,
.dropdown:focus{
  outline: 2px solid #537B87;
  outline-offset: -1px;
}

.apply:active,
.dropdown:active{
  background: #b0b0b0;
}
.description-box{
  padding-top: 5px;
  padding-bottom: 2px;
  font-size: 0.7rem;
}
.description{
  padding-right: 2px;
}
.value {
  font-weight: bold;
  padding-right: 1px;
}
</style>
