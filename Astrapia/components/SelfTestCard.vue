<template>
  <div class="self-test-card">
    <p class="self-test-card-key">{{ props.keyProp.toUpperCase() }}</p>
    <div v-for="(value, key) in props.data" :key="key" class="data-line">
      <span class="data-key">{{ key }}: </span>
      <span v-if="isObject(value)" class="nested-key-value" v-html="objectToString(value)"></span>
      <span v-else class="data-value">{{ value }}&nbsp;</span>
      <span v-if="typeof value === 'boolean'" :class="value ? 'true-value' : 'false-value'"></span>
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
    if (typeof value === 'boolean') {
      str += `<div><span class="keys">${key}</span>: <span class="values">${value}</span> <span class="${value ? 'true-value' : 'false-value'}"></span></div>`;
    } else {
      str += `<div><span class="keys">${key}</span>: <span class="values">${value}</span></div>`;
    }
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
  margin-right: 0.62vw;
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
}

.data-value {
  font-weight: bold;
  color: #005a8a;
}

.nested-key-value {
  text-indent: 2em;
}

.self-test-card:deep(.keys) {
  color: #4D4D4D;
  font-weight: bold;
}

.self-test-card:deep(.values) {
  color: #005a8a;
  font-weight: bold;
}

.self-test-card:deep(.true-value)  {
  display: inline-block;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background-color: #33cc33;
  margin-right: 5px;
}

.self-test-card:deep(.false-value) {
  display: inline-block;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background-color: red;
  margin-right: 5px;
}
</style>
