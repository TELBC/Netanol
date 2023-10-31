<template>
  <div class="graph">
  <v-network-graph
    :nodes="nodes"
    :edges="edges"
    :configs="networkGraphConfigs"
    :event-handlers="eventHandlers"
  >
    <template #edge-label="{ edge, hovered, ...slotProps }">
      <v-edge-label v-if="hovered" :text="edge.label" align="center" vertical-align="above" v-bind="slotProps" />
    </template>
  </v-network-graph>
    <div id="tooltip"/>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  middleware: ["auth"]
})

import { ref, onMounted, onBeforeUnmount } from 'vue';
import { Edges, Nodes, VEdgeLabel, VNetworkGraph } from 'v-network-graph';
import { networkGraphConfigs } from 'assets/v-network-graph-configs';
import { parseJsonData } from '~/utils/networkGraphParsing';
import { createLayout, fetchGraphData } from '~/services/graphService';
import * as vNG from 'v-network-graph';
import { isEqual } from 'smob';

const nodes = ref<Nodes[]>([]);
const edges = ref<Edges[]>([]);
const layout = 'test';
const dateRange: { from: string; to: string } = { from: '2022-01-01', to: '2025-01-01' }; // TODO date range UTC.now-2min

const eventHandlers: vNG.EventHandlers = {
  'node:pointerover': ({ node, event }) => {
    const tooltip = document.getElementById('tooltip');
    tooltip.style.visibility = 'visible';
    tooltip.textContent = `${nodes.value[node].name}`;
    tooltip.style.left = `${event.clientX}px`;
    tooltip.style.top = `${event.clientY}px`;
  },
  'node:pointerout': () => {
    const tooltip = document.getElementById('tooltip');
    tooltip.style.visibility = 'hidden';
  },
};

const updateGraphData = (parsedNodes: Nodes[], parsedEdges: Edges[]) => {
  if (!isEqual(nodes.value, parsedNodes) || !isEqual(edges.value, parsedEdges)) {
    nodes.value = parsedNodes;
    edges.value = parsedEdges;
  }
};

const fetchGraphDataPeriodically: () => Promise<void> = async () => {
  try {
    const data = await fetchGraphData(dateRange, layout);
    const { nodes: parsedNodes, edges: parsedEdges } = parseJsonData(data.nodes, data.edges);
    updateGraphData(parsedNodes, parsedEdges);
  } catch (error) {
    console.error(error);
  }
};

onMounted(async () => {
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
