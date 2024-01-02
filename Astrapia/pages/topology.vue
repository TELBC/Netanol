<template>
  <div>
    <div>
      <TopologyMenuBar @changeLayout="handleLayoutChange" @change="handleTimeframeSelection" :from-value="timeframeSelectorFrom" :to-value="timeframeSelectorTo" />
    </div>
    <Graph :data="data"/>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref } from 'vue';
import debounce from 'lodash/debounce';
import topologyService from '~/services/topology.service';
import TopologyMenuBar from "~/components/TopologyMenuBar.vue";
import Graph from "~/components/Graph.vue";

const layout = ref('');
const timeframeSelectorFrom = ref(new Date(new Date().getTime() - 2 * 60 * 1000).toISOString().slice(0,16))
const timeframeSelectorTo = ref(new Date().toISOString().slice(0,16))

const handleTimeframeSelection = (from: string, to: string) => {
  timeframeSelectorFrom.value = from
  timeframeSelectorTo.value = to
}

const handleLayoutChange = (selectedLayout: string) => {
  layout.value = selectedLayout;
  fetchAndUpdateGraph();
}

const data = ref(null);

const fetchAndUpdateGraph: () => Promise<void> = async () => {
  const dateRange: { from: Date; to: Date } = {
    from: new Date(timeframeSelectorFrom.value + ':00.000Z'),
    to: new Date(timeframeSelectorTo.value + ':00.000Z')
  };
  data.value = await topologyService.getTopology(layout.value, dateRange.from, dateRange.to);
};

const debouncedFetchGraphData = debounce(fetchAndUpdateGraph, 100);

onMounted(async () => {
  const fetchInterval = setInterval(debouncedFetchGraphData, 5000);
  onBeforeUnmount(() => {
    clearInterval(fetchInterval);
  });
  await fetchAndUpdateGraph();
});
</script>

<style scoped>
</style>
