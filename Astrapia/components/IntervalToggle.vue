<template>
  <Tooltip :title="isChecked ? 'Custom Interval' : 'Fixed Interval'">
    <input type="checkbox" class="theme-checkbox" :checked="isChecked" @change="emitValue">
  </Tooltip>
</template>

<script setup lang="ts">
const props = defineProps<{
  isChecked: Boolean
}>();

const emit = defineEmits<{
  input: [check: boolean];
}>();

const emitValue = (event: Event) => {
  emit('input', (event.target as HTMLInputElement).checked);
};
</script>

<style scoped>
.theme-checkbox {
  --toggle-size: 8px;
  -webkit-appearance: none;
  -moz-appearance: none;
  appearance: none;
  width: 3.125em;
  height: 1.5625em;
  background-size: 205%;
  background: #e0e0e0 0;
  border: 1px solid #bdbcbc;
  -webkit-transition: 0.4s;
  -o-transition: 0.4s;
  transition: 0.4s;
  border-radius: 99em;
  position: relative;
  cursor: pointer;
  font-size: var(--toggle-size);
  margin-top: 7px;
}

.theme-checkbox::before {
  content: "";
  width: 1.125em;
  height: 1.125em;
  position: absolute;
  top: 0.119em;
  left: 0.21em;
  background-size: 205%;
  background: #797878 100%;
  border-radius: 50%;
  -webkit-transition: 0.4s;
  -o-transition: 0.4s;
  transition: 0.4s;
}

.theme-checkbox:checked::before {
  left: calc(100% - 1.125em - 0.21em);
  background-position: 0;
}

.theme-checkbox:checked {
  background-position: 100%;
}
</style>
