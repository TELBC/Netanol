<template>
  <div class="topology-menu">
    <Dropdown class="layout-dropdown" @changeLayout="handleLayoutChange" />
    <Graph :data="data" @intervalAmount="handleIntervalAmount"/>
    <GraphFilterMenu v-bind:layout="layout" @menuOpened="handleMenuOpened" />
    <TopologyTimeframeSelector class="topology-timeframe"
                               :style="{ right: timeframeSelectorRight }"
                               @change="handleTimeframeSelection"
                               :from-value="timeframeSelectorFrom"
                               :to-value="timeframeSelectorTo"/>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref } from 'vue';
import debounce from 'lodash/debounce';
import topologyService from '~/services/topology.service';
import Graph from "~/components/Graph.vue";
import Dropdown from "~/components/Dropdown.vue";
import GraphFilterMenu from "~/components/GraphFilterMenu.vue";
import TopologyTimeframeSelector from "~/components/TopologyTimeframeSelector.vue";

const layout = ref('');
const timeframeSelectorFrom = ref(new Date(new Date().getTime() - 2 * 60 * 1000).toISOString().slice(0,16))
const timeframeSelectorTo = ref(new Date().toISOString().slice(0,16))
const data = ref();
const intervalAmount = ref<number>(0);
const timeframeSelectorRight = ref('1vw'); // Initial position

let fetchInterval: NodeJS.Timeout | null = null;
let isMenuOpened = ref(false);

const handleTimeframeSelection = (from: string, to: string) => {
  timeframeSelectorFrom.value = from;
  timeframeSelectorTo.value = to;
  fetchAndUpdateGraph();
}

const handleIntervalAmount = (amount: number) => {
  intervalAmount.value = amount;
  if (fetchInterval) {
    clearInterval(fetchInterval);
    fetchInterval = null;
  }

  if (intervalAmount.value !== 0) {
    fetchAndUpdateGraph();
    const intervalMs = intervalAmount.value * 1000;
    fetchInterval = setInterval(debouncedFetchGraphData, intervalMs);
  }
}

const handleLayoutChange = (selectedLayout: string) => {
  layout.value = selectedLayout;
  fetchAndUpdateGraph();
}

const handleMenuOpened = (opened: boolean) => {
  isMenuOpened.value = opened;
  updateTimeframeSelectorPosition();
}

const updateTimeframeSelectorPosition = () => {
  if (isMenuOpened.value) {
    timeframeSelectorRight.value = '15vw';
  } else {
    timeframeSelectorRight.value = '1vw';
  }
}

const fetchAndUpdateGraph: () => Promise<void> = async () => {
  const dateRange: { from: Date; to: Date } = {
    from: new Date(timeframeSelectorFrom.value + ':00.000Z'),
    to: new Date(timeframeSelectorTo.value + ':00.000Z')
  };
  data.value = await topologyService.getTopology(layout.value, dateRange.from, dateRange.to);
};

const debouncedFetchGraphData = debounce(fetchAndUpdateGraph, 100);

onMounted(async () => {
  if (intervalAmount.value !== 0) {
    await fetchAndUpdateGraph();
    const intervalMs = intervalAmount.value * 1000;
    fetchInterval = setInterval(debouncedFetchGraphData, intervalMs);
  }
});

onBeforeUnmount(() => {
  if (fetchInterval) {
    clearInterval(fetchInterval);
    fetchInterval = null;
  }
});
</script>

<style scoped>
.topology-menu {
  overflow-y: hidden;
}

.layout-dropdown {
  position: absolute;
  top: 0;
  right: 0;
  z-index: 15;
  margin: 0.75vh 1vw 0 0;
}

.topology-timeframe {
  position: absolute;
  bottom: 0;
  right: 1vw;
  z-index: 15;
  transition: right 0.2s ease-in-out;
}
</style>
