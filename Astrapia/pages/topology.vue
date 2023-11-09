<template>
  <div class="graph">
    <v-network-graph
        :nodes="graphData.nodes"
        :edges="graphData.edges"
        :configs="networkGraphConfigs"
        :event-handlers="eventHandlers"
    >
      <template #edge-label="{ edge, hovered, ...slotProps }">
        <v-edge-label v-if="hovered" :text="edge.label" align="center" vertical-align="above" v-bind="slotProps" />
      </template>
    </v-network-graph>
    <div id="tooltip"/>
    <div>
      <TopologySlider label="Timeframe" v-model="rangeValue"/>
    </div>
  </div>
</template>

<script setup lang="ts">
import {onMounted, onBeforeUnmount} from 'vue';
import { Edge, Node, VEdgeLabel, VNetworkGraph } from 'v-network-graph';
import { networkGraphConfigs } from 'assets/v-network-graph-configs';
import { parseJsonData } from '~/utils/networkGraphParsing';
import { createLayout, fetchGraphData } from '~/services/graphService';
import * as vNG from 'v-network-graph';
import TopologySlider from "~/components/TopologySlider.vue";

definePageMeta({
  middleware: ["auth"]
})


const graphData = reactive({ nodes: [] as Node[], edges: [] as Edge[] });
const layout = 'test';
let tooltip: HTMLElement | null;
const rangeValue = ref(2);

const eventHandlers: vNG.EventHandlers = {
  'node:pointerover': ({ node, event }) => {
    if(tooltip) {
      tooltip.style.visibility = 'visible';
      tooltip.textContent = `${graphData.nodes[node].name}`;
      tooltip.style.left = `${event.clientX}px`;
      tooltip.style.top = `${event.clientY}px`;
    }
  },
  'node:pointerout': () => {
    if(tooltip) {
      tooltip.style.visibility = 'hidden';
    }
  },
};

const fetchGraphDataPeriodically: () => Promise<void> = async () => {
  try {
    const now = new Date();

    const dateRange: { from: string; to: string } = {
      from: new Date(now.getTime() - rangeValue.value * 60 * 1000).toISOString(),
      to: new Date().toISOString()
    };
    const data = await fetchGraphData(dateRange, layout);
    const { nodes: parsedNodes, edges: parsedEdges } = parseJsonData(data.nodes, data.edges);
    graphData.nodes = parsedNodes;
    graphData.edges = parsedEdges;
  } catch (error) {
    console.error(error);
  }
};

watch(rangeValue, async () => {
  await fetchGraphDataPeriodically();
});

onMounted(async () => {
  tooltip = document.getElementById('tooltip');
  const fetchInterval = setInterval(fetchGraphDataPeriodically, 5000);

  onBeforeUnmount(() => {
    clearInterval(fetchInterval);
  });

  await createLayout(layout);
  await fetchGraphDataPeriodically();
});
</script>

<style scoped>
.graph{
  position: fixed;
  width: 100vw;
  height: 100vh;
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
