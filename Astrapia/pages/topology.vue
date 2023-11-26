<template>
  <div class="graph">
    <v-network-graph
      :nodes="graphData.nodes"
      :edges="graphData.edges"
      :configs="networkGraphConfigs"
      :event-handlers="eventHandlers"
    >
      <template #edge-label="{ edge, hovered, ...slotProps }">
        <v-edge-label v-if="hovered" :text="edge.source" align="center" vertical-align="above" v-bind="slotProps"/>
      </template>
    </v-network-graph>
    <div id="tooltip"/>
    <div>
      <TopologySlider label="Timeframe" v-model="rangeValue"/>
    </div>
  </div>
</template>

<script setup lang="ts">
import {onMounted, onBeforeUnmount, reactive} from 'vue';
import {type Edge, type Node, VEdgeLabel, VNetworkGraph} from 'v-network-graph';
import {networkGraphConfigs} from 'assets/v-network-graph-configs';
import * as vNG from 'v-network-graph';
import TopologyService from "~/services/topology.service";
import TopologySlider from "~/components/TopologySlider.vue";
import { fetch } from 'ofetch';

const graphData = reactive({nodes: [] as Node[], edges: [] as Edge[]});
const layout = 'test';
let tooltip: HTMLElement | null;
const rangeValue = ref(2);

const eventHandlers: vNG.EventHandlers = {
  'node:pointerover': ({node, event}) => {
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

const fetchGraphDataPeriodically: () => Promise<void> = async () => {
  const now = new Date()
  const dateRange: { from: Date; to: Date } = {
    from: new Date(now.getTime() - rangeValue.value * 60 * 1000),
    to: new Date()
  }

  const response = await TopologyService.getTopology(layout, dateRange.from, dateRange.to)
  const nodes: Node[] = []
  const edges: Edge[] = []

  for (const edge in response.edges) {
    const e = response.edges[edge]
    edges.push({
      source: e.source,
      target: e.target
    })
  }

  for (const node in response.nodes) {
    const n = response.nodes[node]
    nodes.push({
      name: n.displayName
    })
  }

  graphData.edges = edges
  graphData.nodes = nodes
}

/*
const debouncedFetchGraphData = debounce(fetchGraphDataPeriodically, 100);

watch(rangeValue, async () => {
  await debouncedFetchGraphData();
});
*/

onMounted(async () => {
  await fetchGraphDataPeriodically();

  /*
  tooltip = document.getElementById('tooltip');
  const fetchInterval = setInterval(debouncedFetchGraphData, 5000);

  onBeforeUnmount(() => {
    clearInterval(fetchInterval);
  });

  await debouncedFetchGraphData(); */
});
</script>

<style scoped>
.graph {
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
