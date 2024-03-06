<template>
  <div class="self-test-container">
    <SelfTestCard v-for="(value, key) in testSelfTestData" :key="key" :data="value" :keyProp="key" />
  </div>
</template>

<script setup lang="ts">
import SelfTestCard from "~/components/SelfTestCard.vue";
import { ref } from 'vue';

const selfTestData = ref({});
const testSelfTestData = {
  database: {
    reachable: true,
    latency: "20ms",
    totalSingleTraceCount: 213897,
    totalDatabaseSize: "20mb"
  },
  counting: {
    totalEntriesSinceUptime: 238283,
    totalCounts: {
      "Netflow5": 82383,
      "Netflow9": 2838,
      "IPFIX": 293,
      "sFlow": 2838
    }
  },
  "multiplexers": {
    "2055": {
      enabled: true,
      name: "Main Multiplexers",
      port: "2055",
      acceptedProtocols: "Netflow5, Netflow9"
    }
  },
  config: {
    "VMWare Tagging": {
      enabled: true,
      targetServer: "1.1.1.1",
      cachedTagsCount: 283283,
      refreshPeriod: "12hrs",
      timeUntilNextRefresh: "20min"
    },
    "DNS Server": {
      enabled: true,
      cachedEntriesCount: 2238328,
      refreshPeriod: "12hrs",
      timeUntilNextRefresh: "20min"
    },
    "Duplicate Flagging": {
      claimLifetime: "12hrs",
      refreshPeriod: "5min"
    }
  },
  statistics: {
    startTime: "2.3.2024 12/12/2024",
    upTime: "00:12:00hr",
  }
};
</script>

<style scoped>
.self-test-container {
  display: flex;
  flex-wrap: wrap;
  margin: 4vh 0 4vh 2.5vw;
  width: 92.5vw;
  height: auto;
  justify-content: space-between;
}
</style>
