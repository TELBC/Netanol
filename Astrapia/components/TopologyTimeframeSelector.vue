<template>
  <div class="timeframe-selector">
    <p class="timeframe-selector-labels">from:</p>
    <input class="timeframe-selector-input" type="datetime-local" v-model="from">
    <p class="timeframe-selector-labels">to:</p>
    <input class="timeframe-selector-input" type="datetime-local" v-model="to">
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';

const props = defineProps({
  fromValue: {
    type: String
  },
  toValue: {
    type: String
  },
});

const from = ref(props.fromValue);
const to = ref(props.toValue);

const emit = defineEmits<{
  change: [from: string, to: string]
}>()

watch([from, to], ([newFrom, newTo]) => {
  emit('change', newFrom!, newTo!)
})
</script>

<style scoped>
.timeframe-selector {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  font-family: 'Open Sans', sans-serif;
  height: 5vh;
  align-items: center;
}

.timeframe-selector-labels {
  margin: 0 1vw 0 2vw;
}

.timeframe-selector-input {
  height: 2vh;
  font-family: 'Open Sans', sans-serif;
}
</style>
