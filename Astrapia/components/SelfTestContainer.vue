<template>
  <div class="self-test-container">
    <SelfTestCard v-for="(value, key) in selfTestData" :key="key" :data="value" :keyProp="key" />
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue';
import SelfTestCard from "~/components/SelfTestCard.vue";
import { ref } from 'vue';
import metricService from "~/services/metricService";

const selfTestData = ref({});
let intervalId: number;

async function getSelfTestData() {
  selfTestData.value = await metricService.getApplicationStatusData();
}

onMounted(() => {
  getSelfTestData();
  intervalId = window.setInterval(getSelfTestData, 30000);
});

onUnmounted(() => {
  clearInterval(intervalId);
});
</script>

<style scoped>
.self-test-container {
  display: flex;
  flex-wrap: wrap;
  margin: 4vh 0 4vh 2.5vw;
  width: 92.5vw;
  height: auto;
}
</style>
