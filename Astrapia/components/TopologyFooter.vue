<template>
  <div>
    <div class="footer">
      <button class="icon-button" @click="$emit('recenter')" title="Recenter Graph">
        <font-awesome-icon icon="fa-solid fa-arrows-to-circle" />
      </button>
      <button class="icon-button" @click="toggleSimulation" title="Freeze Simulation">
        <font-awesome-icon :icon="simulationFrozen ? 'fa-solid fa-play' : 'fa-solid fa-pause'" />
      </button>
      <span class="separator"/>
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
          <span class="metaData-number" :title="byteHoverText">{{ convertedByteCount }}</span>
        </span>
      </div>
      <span class="separator"/>
      <div class="footer-info" title="Fullscreen">
        <FullscreenButton elementId="graph"/>
      </div>
      <span class="separator"/>
      <ArrowComponent :isOpen="isSlideMenuOpen" @click="toggleSlideMenu" title="Simulation Controls"/>
      <span class="separator"/>
      <IntervalToggle :is-checked="isIntervalToggle" @input="toggleIntervalMenu" title="Interval Control"/>
    </div>
    <SlideMenu :distance="distance" :force="force" :isOpen="isSlideMenuOpen" @updateDistance="updateLinkDistance" @updateSim="updateSim" />
    <IntervalMenu title="Interval Selection" description="Current Interval set to:" :isOpen="isIntervalMenuOpen" @change="handleIntervalChange"/>
  </div>
</template>

<script setup lang="ts">
import {onMounted, ref, watch} from 'vue';
import FullscreenButton from "~/components/FullscreenButton.vue";
import SlideMenu from "~/components/SlideMenu.vue";
import IntervalMenu from "~/components/IntervalMenu.vue";
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import IntervalToggle from "~/components/IntervalToggle.vue";
import ArrowComponent from "~/components/ArrowComponent.vue";

const props = defineProps<{
  elementId: String,
  metaData: IGraphStatistics
}>();

const simulationFrozen = ref(false);
const distance = ref(200);
const force = ref(800);
const convertedByteCount = ref('');
const byteHoverText = ref('');
const isSlideMenuOpen = ref(false);
const isIntervalMenuOpen = ref(false);
const isIntervalToggle = ref(false);

const emit = defineEmits<{
  toggleSimulation: [simulation: boolean],
  updateDistance: [distance: number],
  updateSim: [force: number],
  recenter: [],
  intervalAmount: [amount: number],
}>();

const toggleSimulation = () => {
  simulationFrozen.value = !simulationFrozen.value;
  emit('toggleSimulation', simulationFrozen.value);
};

const toggleSlideMenu = () => {
  isSlideMenuOpen.value = !isSlideMenuOpen.value;
  if (isIntervalMenuOpen.value && isIntervalToggle.value) {
    isIntervalMenuOpen.value = false;
    isIntervalToggle.value = false;
  }
};

const toggleIntervalMenu = (checked: boolean) => {
  isIntervalMenuOpen.value = checked;
  isIntervalToggle.value = checked;
  emit('intervalAmount', 0);
  if (checked && isSlideMenuOpen.value) {
    isSlideMenuOpen.value = false;
  }
};

const updateLinkDistance = (newDistance: number) => {
  distance.value = newDistance;
  emit('updateDistance', newDistance);
};

const updateSim = (newForce: number) => {
  force.value = newForce;
  emit('updateSim', newForce);
};

const handleIntervalChange = (value: number) => {
  isIntervalMenuOpen.value = false;
  isIntervalToggle.value = true;
  emit('intervalAmount', value);
};

const formatBytes = (bytes: number): string => {
  const units = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
  let i = 0;
  while (bytes >= 1024) {
    bytes /= 1024;
    i++;
  }
  return `${bytes.toFixed(2)} ${units[i]}`;
}

onMounted(() =>{
  convertedByteCount.value = formatBytes(props.metaData.totalByteCount);
  byteHoverText.value = `${props.metaData.totalByteCount} bytes`;
})

watch(() => props.metaData.totalByteCount, (newValue) => {
  convertedByteCount.value = formatBytes(newValue);
  byteHoverText.value = `${newValue} bytes`;
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
  z-index: 5;
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
