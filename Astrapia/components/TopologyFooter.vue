<template>
  <div>
    <div class="footer">
      <Tooltip title="Recenter Graph">
      <button class="icon-button" @click="$emit('recenter')">
        <font-awesome-icon icon="fa-solid fa-arrows-to-circle" />
      </button>
    </Tooltip>
      <Tooltip title="Freeze Simulation">
        <button class="icon-button" @click="toggleSimulation">
          <font-awesome-icon :icon="simulationFrozen ? 'fa-solid fa-play' : 'fa-solid fa-pause'" />
        </button>
      </Tooltip>
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
      <Tooltip title="Fullscreen">
        <div class="footer-info">
          <FullscreenButton elementId="graph"/>
        </div>
      </Tooltip>
      <span class="separator"></span>
      <ArrowComponent :isOpen="isMenuOpen" @click="toggleMenu" />
    </div>
    <SlideMenu :distance="distance" :force="force" :isOpen="isMenuOpen" @updateDistance="updateLinkDistance" @updateSim="updateSim" />
  </div>
</template>

<script setup lang="ts">
import { ref, watch} from 'vue';
import FullscreenButton from "~/components/FullscreenButton.vue";
import SlideMenu from "~/components/SlideMenu.vue";
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";

const props = defineProps<{
  elementId: String,
  metaData: IGraphStatistics
}>();

const simulationFrozen = ref(false);
const distance = ref(50);
const force = ref(500);
const isMenuOpen = ref(false);

const toggleMenu = () => {
  isMenuOpen.value = !isMenuOpen.value;
};

const emit = defineEmits<{
  toggleSimulation: [simulation: boolean],
  updateDistance: [distance: number],
  updateSim: [force:number];
}>();
const toggleSimulation = () => {
  simulationFrozen.value = !simulationFrozen.value;
  emit('toggleSimulation', simulationFrozen.value);
};

const updateLinkDistance = (newDistance: number) => {
  distance.value = newDistance;
  emit('updateDistance', newDistance);
};

const updateSim = (newForce: number) => {
  force.value = newForce;
  emit('updateSim', newForce);
};

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
}

watch(() => props.metaData?.totalByteCount, (newValue) => {
  if (newValue !== undefined) {
    convertedByteCount.value = formatBytes(newValue);
    byteHoverText.value = `${newValue} bytes`;
  }
});

interface IGraphStatistics {
  totalHostCount: number,
  totalByteCount: number,
  totalPacketCount: number,
  totalTraceCount: number
}
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
  display: flex;
  align-items: center;
  z-index:5;
}

.icon-button {
  color: #8d8d8d;
  margin-left: 4px;
  margin-right: 4px;
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
  margin-right: 10px;
}

.metaData-number {
  color: #797878;
  font-weight: bold;
}

.metadata-item {
  margin-right: 10px;
}
</style>
