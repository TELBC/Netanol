<template>
  <div class="flow-metric-card">
    <p class="flow-metric-card-key">{{ props.keyProp }}:</p>
    <div v-for="(value, key) in props.data" :key="key" class="data-line">
      <span class="data-key">{{ key }}: </span>
      <span class="data-value">{{ value }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
interface GeneralFlowImporterData {
  receivedPacketCount: number,
  receivedByteCount: number,
  transmittedPacketCount: number,
  transmittedByteCount: number,
  successfullyParsedPacket: number,
  failedParsedPacket: number
}

const props = defineProps<{
  data: GeneralFlowImporterData;
  keyProp: string;
}>();
</script>

<style scoped>
.flow-metric-card {
  display: flex;
  justify-content: flex-start;
  align-items: flex-start;
  flex-direction: column;
  width: auto;
  height: auto;
  border: 1px solid #424242;
  border-radius: 4px;
  padding: 1vh 0.5vw;
  box-shadow: 4px 4px 8px 0 #e0e0e0;
  user-select: none;
}

.flow-metric-card-key {
  font-weight: bold;
  margin: 0;

}

.data-line {
  text-indent: 1em;
}

.data-key {
  font-weight: bold;
  color: #294D61;
}

.data-value {
  font-weight: bold;
}
</style>
