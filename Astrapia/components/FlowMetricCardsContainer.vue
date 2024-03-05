<template>
  <div class="flow-metric-cards-container">
    <FlowMetricCard v-for="(data, key) in testMetrics" :key="key" :data="data" :keyProp="key" />
  </div>
</template>

<script setup lang="ts">
import {onMounted, ref} from 'vue'
import metricService from "~/services/metricService";
import FlowMetricCard from "~/components/FlowMetricCard.vue";

const metrics = ref({});
const testMetrics = {
  "172.19.0.4:46967": {
    "receivedPacketCount": 500,
    "receivedByteCount": 200000,
    "transmittedPacketCount": 400000,
    "transmittedByteCount": 3000,
    "successfullyParsedPacket": 400,
    "failedParsedPacket": 100
  },
  "172.19.0.5:46968": {
    "receivedPacketCount": 600,
    "receivedByteCount": 250000,
    "transmittedPacketCount": 450000,
    "transmittedByteCount": 3500,
    "successfullyParsedPacket": 500,
    "failedParsedPacket": 100
  },
  "172.19.0.6:46969": {
    "receivedPacketCount": 700,
    "receivedByteCount": 300000,
    "transmittedPacketCount": 500000,
    "transmittedByteCount": 4000,
    "successfullyParsedPacket": 600,
    "failedParsedPacket": 100
  },
  "172.19.0.7:46970": {
    "receivedPacketCount": 800,
    "receivedByteCount": 350000,
    "transmittedPacketCount": 550000,
    "transmittedByteCount": 4500,
    "successfullyParsedPacket": 700,
    "failedParsedPacket": 100
  },
  "172.19.0.8:46971": {
    "receivedPacketCount": 900,
    "receivedByteCount": 400000,
    "transmittedPacketCount": 600000,
    "transmittedByteCount": 5000,
    "successfullyParsedPacket": 800,
    "failedParsedPacket": 100
  },
  "172.19.0.9:46972": {
    "receivedPacketCount": 1000,
    "receivedByteCount": 450000,
    "transmittedPacketCount": 650000,
    "transmittedByteCount": 5500,
    "successfullyParsedPacket": 900,
    "failedParsedPacket": 100
  },
  "172.19.0.10:46973": {
    "receivedPacketCount": 1100,
    "receivedByteCount": 500000,
    "transmittedPacketCount": 700000,
    "transmittedByteCount": 6000,
    "successfullyParsedPacket": 1000,
    "failedParsedPacket": 100
  },
  "172.19.0.11:46974": {
    "receivedPacketCount": 1200,
    "receivedByteCount": 550000,
    "transmittedPacketCount": 750000,
    "transmittedByteCount": 6500,
    "successfullyParsedPacket": 1100,
    "failedParsedPacket": 100
  },
  "172.19.0.12:46975": {
    "receivedPacketCount": 1300,
    "receivedByteCount": 600000,
    "transmittedPacketCount": 800000,
    "transmittedByteCount": 7000,
    "successfullyParsedPacket": 1200,
    "failedParsedPacket": 100
  }
}

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
  flex-wrap: wrap;
  margin: 4vh 0 4vh 2.5vw;
  width: 92.5vw;
  height: auto;
}
</style>
