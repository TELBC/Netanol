<template>
  <div class="flow-metric-cards-container">
    <FlowMetricCard v-for="(data, key) in metrics" :key="key" :data="data" :keyProp="key" />
  </div>
</template>

<script setup lang="ts">
import {onMounted, ref} from 'vue'
import metricService from "~/services/metricService";
import FlowMetricCard from "~/components/FlowMetricCard.vue";

const metrics = ref({});

async function getGeneralFlowImporterMetrics() {
  const metricData = await metricService.getGeneralFlowImporterData();
  metrics.value = metricData;
}

onMounted(async () => {
  await getGeneralFlowImporterMetrics();
});
</script>

<style scoped>
.flow-metric-cards-container {
  display: flex;
  margin: 4vh 2vw;
  width: 92.5vw;
  height: auto;
}
</style>
