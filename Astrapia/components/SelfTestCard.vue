<template>
  <div class="self-test-card">
    <p class="self-test-card-key">{{ props.keyProp.toUpperCase() }}</p>
    <div v-for="(value, key) in props.data" :key="key" class="data-line">
      <!-- TODO: red or green dot next to boolean values to indicate value -->
      <!-- TODO: more highlighting for nested key value pairs, nested keys not bold and values coloured -->
      <span class="data-key">{{ key }}: </span>
      <span v-if="isObject(value)" class="nested-key-value" v-html="objectToString(value)"></span>
      <span v-else class="data-value">{{ value }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
const props = defineProps< {
  data: any;
  keyProp: string;
}>();

const isObject = (item: any) => {
  return (typeof item === "object" && !Array.isArray(item) && item !== null);
}

const objectToString = (obj: any) => {
  let str = '';
  for (const [key, value] of Object.entries(obj)) {
    str += `<div><strong>${key}</strong>: ${value}</div>`;
  }
  return str;
}
</script>

<style scoped>
.self-test-card {
  display: flex;
  justify-content: flex-start;
  align-items: flex-start;
  flex-direction: column;
  width: auto;
  height: auto;
  border: 1px solid #424242;
  border-radius: 4px;
  padding: 1.5vh 1vw;
  box-shadow: 4px 4px 8px 0 #e0e0e0;
  user-select: none;
  margin-right: 1vw;
  margin-bottom: 4vh;
}

.self-test-card-key {
  font-weight: bold;
  margin: 0 0 1vh 0;
  font-size: 2vh;
  color: #294D61;
}

.data-line {
  text-indent: 1em;
}

.data-key {
  font-weight: bold;
  color: #4D4D4D;
}

.data-value {
  font-weight: bold;
}

.nested-key-value {
  text-indent: 2em;
}
</style>
