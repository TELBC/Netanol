<template>
  <div>
    <div>
      <TopologyMenuBar @changeLayout="handleLayoutChange" />
    </div>
    <div id="graph">
    <v-network-graph
      ref="graph"
      :nodes="graphData.nodes"
      :edges="graphData.edges"
      :configs="networkGraphConfigs"
      :event-handlers="eventHandlers"
    >
      <template #edge-label="{ edge, hovered, ...slotProps }">
        <v-edge-label v-if="hovered" :text="edge.label" align="center" vertical-align="above" v-bind="slotProps" />
      </template>
    </v-network-graph>
    <div id="tooltip" />
    <div>
      <TopologyFooter :graph="graph" :metaData="metaData" element-id="graph"/>
      <TopologySlider label="Timeframe" v-model="rangeValue" />
    </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount } from 'vue';
import { VEdgeLabel, VNetworkGraph } from 'v-network-graph';
import { networkGraphConfigs } from 'assets/v-network-graph-configs';
import * as vNG from 'v-network-graph';
import TopologySlider from '~/components/TopologySlider.vue';
import debounce from 'lodash/debounce';
import TopologyFooter from "~/components/TopologyFooter.vue";
import topologyService, {IGraphStatistics} from '~/services/topology.service';
import { reactive } from 'vue';
import { ref } from 'vue';
import { watch } from 'vue';
import TopologyMenuBar from "~/components/TopologyMenuBar.vue";

const graph = ref<vNG.Instance>()
const graphData = reactive({ nodes: {} as vNG.Nodes, edges: {} as vNG.Edges });
let metaData = ref<IGraphStatistics | null>(null);
const layout = ref('test');
let tooltip: HTMLElement | null;
const rangeValue = ref(2);

const handleLayoutChange = (selectedLayout: string) => {
  layout.value = selectedLayout;
  fetchAndUpdateGraph();
}

const eventHandlers: vNG.EventHandlers = {
  'node:pointerover': ({ node, event }) => {
    if (tooltip) {
      tooltip.style.visibility = 'visible';
      tooltip.textContent = `${graphData.nodes[node].name}`;
      tooltip.style.left = `${event.clientX}px`;
      tooltip.style.top = `${event.clientY}px`;
    }
  },
  'node:pointerout': () => {
    if (tooltip) {
      tooltip.style.visibility = 'hidden';
    }
  },
};

const fetchAndUpdateGraph: () => Promise<void> = async () => {
  const now = new Date();

  const dateRange: { from: Date; to: Date } = {
    from: new Date(now.getTime() - rangeValue.value * 60 * 1000),
    to: new Date()
  };

  const data = await topologyService.getTopology(layout.value, dateRange.from, dateRange.to);
  metaData = data.graphStatistics;
  graphData.nodes = data.nodes;
  graphData.edges = data.edges;
};

const debouncedFetchGraphData = debounce(fetchAndUpdateGraph, 100);

watch(rangeValue, async () => {
  await debouncedFetchGraphData();
});

onMounted(async () => {
  tooltip = document.getElementById('tooltip');
  const fetchInterval = setInterval(debouncedFetchGraphData, 5000);

  onBeforeUnmount(() => {
    clearInterval(fetchInterval);
  });

  await fetchAndUpdateGraph();
});
</script>

<style scoped>
#graph{
  position: fixed;
  width: 100vw;
  height: 100vh;
  background-color: white;
}

#tooltip {
  position: absolute;
  visibility: hidden;
  background-color: #D7DFE7;
  color: black;
  padding: 2px;
  border-radius: 5px;
  text-align: center;
  font-family: 'Open Sans', sans-serif;
  font-size: 0.8rem;
}
</style>
