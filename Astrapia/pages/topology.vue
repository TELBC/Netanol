<template>
  <div class="graph">
  <v-network-graph
    :nodes="nodes"
    :edges="edges"
    :configs="networkGraphConfigs"
  >
    <template #edge-label="{ edge, hovered, ...slotProps }">
      <v-edge-label v-if="hovered" :text="edge.label" align="center" vertical-align="above" v-bind="slotProps" />
    </template>
  </v-network-graph>
  </div>
</template>

<script setup lang="ts">
import {Edges, Nodes, VEdgeLabel, VNetworkGraph} from 'v-network-graph'
import { networkGraphConfigs } from 'assets/v-network-graph-configs'
import { parseJsonData } from "~/utils/networkGraphParsing";
import { createLayout, fetchGraphData } from "~/services/graphService";

const nodes = ref<Nodes[]>([])
const edges = ref<Edges[]>([])
const layout = "test";
const dateRange = { from: "2022-01-01", to: "2025-01-01"}//TODO date range UTC.now-2min

const fetchGraphDataPeriodically = async () => {
  try {
    const data = await fetchGraphData(dateRange, layout)
    const { nodes: parsedNodes, edges: parsedEdges } = parseJsonData(data.nodes, data.edges)
    nodes.value = parsedNodes
    edges.value = parsedEdges
  } catch (error) {
    console.error(error)
  }
}

onMounted(async () => {
  await createLayout(layout)
  await fetchGraphDataPeriodically()

  const fetchInterval = setInterval(fetchGraphDataPeriodically, 5000)

  onBeforeUnmount(() => {
    clearInterval(fetchInterval)
  })
})
</script>

<style scoped>
.graph{
  position: fixed;
  width: 100vw;
  height: 100vh;
}
</style>
