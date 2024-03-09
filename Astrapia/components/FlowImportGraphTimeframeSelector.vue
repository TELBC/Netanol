<template>
  <div class="flow-import-graph-timeframe-selector">
    <p class="flow-import-graph-labels">from:</p>
    <input class="flow-import-graph-selectors" type="datetime-local" v-model="from">
    <p class="flow-import-graph-labels">to:</p>
    <input class="flow-import-graph-selectors" type="datetime-local" v-model="to">
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
  change: [from: string, to: string];
}>();

watch([from, to], ([newFrom, newTo]) => {
  emit('change', newFrom!, newTo!)
})
</script>

<style scoped>
.flow-import-graph-timeframe-selector {
  display: flex;
  align-items: center;
  margin: 4vh 0 4vh 2.5vw;
  width: 92.5vw;
  height: 2vh;
}

.flow-import-graph-labels {
  font-family: 'Open Sans', sans-serif;
}

.flow-import-graph-selectors {
  margin: 0 2vw 0 1vw;
  font-family: 'Open Sans', sans-serif;
}
</style>
