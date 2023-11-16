<template>
  <div class="footer">
    <button class="icon-button" @click="graph?.panToCenter()">
      <font-awesome-icon icon="fa-solid fa-arrows-to-circle" />
    </button>
    <span class="separator"></span>
    <div class="footer-info" v-if="metaData">
      <span class="metadata-item">
        Hosts: <span class="metaData-number">{{ metaData.totalHostCount }}</span>
      </span>
      <span class="metadata-item">
        Traces: <span class="metaData-number">{{ metaData.totalTraceCount }}</span>
      </span>
      <span class="metadata-item">
        Packets: <span class="metaData-number">{{ metaData.totalPacketCount }}</span>
      </span>
      <span class="metadata-item">
        Bytes:
        <Tooltip :title="byteHoverText">
          <span class="metaData-number">{{ convertedByteCount }}</span>
        </Tooltip>
      </span>
    </div>
    <span class="separator"></span>
    <div class="footer-info">
      <FullscreenButton elementId="graph"/>
    </div>
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import VNetworkGraph from "~/plugins/v-network-graph";
import {IGraphStatistics} from "~/services/topology.service";
import FullscreenButton from "~/components/FullscreenButton.vue";

const props = defineProps<{
  elementId: String,
  graph?: typeof VNetworkGraph;
  metaData?: IGraphStatistics;
}>();

const convertedByteCount = ref('');
const byteHoverText = ref('');

const formatBytes = (bytes: number): string => {
  const units = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
  let i = 0;
  while (bytes >= 1024) {
    bytes /= 1024;
    i++;
  }
  return `${bytes.toFixed(2)} ${units[i]}`;
};

watch(() => props.metaData?.totalByteCount, (newValue) => {
  if (newValue !== undefined && newValue !== null) {
    convertedByteCount.value = formatBytes(newValue);
    byteHoverText.value = `${newValue} bytes`;
  }
});
</script>

<style scoped>
.footer {
  font-family: 'Open Sans', sans-serif;
  font-size: 0.8rem;
  background-color: #e0e0e0;
  color: #8d8d8d;
  position: fixed;
  bottom: 0;
  width: 100%;
  padding: 2px;
  display: flex;
  align-items: center;
}

.icon-button {
  color: #8d8d8d;
  padding-left: 10px;
  background: none;
  border: none;
  cursor: pointer;
  outline: none;
}

.icon-button:hover {
  color: #797878;
}

.separator {
  border-left: 2px solid #bdbcbc;
  height: 15px;
  margin-left: 10px;
}

.footer-info{
  padding-left: 10px;
}

.metaData-number {
  color: #797878;
  font-weight: bold;
}

.metadata-item {
  margin-right: 15px;
}
</style>
