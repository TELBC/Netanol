<template>
  <svg id="graph-svg" width="960" height="600"></svg>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount } from 'vue';
import * as d3 from 'd3';
import * as vNG from 'v-network-graph';
import debounce from 'lodash/debounce';
import topologyService from '~/services/topology.service';
import { reactive, ref, watch } from 'vue';
import {drag} from "d3";

const graph = ref<vNG.Instance>()
const graphData = reactive({ nodes: {} as vNG.Nodes, edges: {} as vNG.Edges });
let metaData = ref<IGraphStatistics | null>(null);
const layout = 'test';
let tooltip: HTMLElement | null;
const rangeValue = ref(2);

const fetchAndUpdateGraph: () => Promise<void> = async () => {
  const now = new Date();

  const dateRange: { from: Date; to: Date } = {
    from: new Date(now.getTime() - rangeValue.value * 60 * 1000),
    to: new Date()
  };

  const data = await topologyService.getTopology('test', dateRange.from, dateRange.to);
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
  const width = window.innerWidth;
  const height = window.innerHeight;

  // D3.js implementation
  const svg = d3.select("#graph-svg")
    .attr("width", width)
    .attr("height", height)

  const simulation = d3.forceSimulation(graphData.nodes)
    .force("link", d3.forceLink(graphData.edges).id((d: { id: any; }) => d.id).distance(100).strength(1))
    .force("charge", d3.forceManyBody().strength(-1000))
    .force("center", d3.forceCenter(width / 2, height / 2))
    .force("x", d3.forceX())
    .force("y", d3.forceY());

  let link = svg.append("g")
    .attr("stroke", "#999")
    .attr("stroke-opacity", 0.6)
    .selectAll("line")
    .data(graphData.edges)
    .join("line");

  let node = svg.append("g")
    .attr("stroke", "#fff")
    .attr("stroke-width", 1.5)
    .selectAll("circle")
    .data(graphData.nodes)
    .join("circle")
    .attr("r", 10)
    .attr("fill", "#537B87")
    .call(drag(simulation));

  simulation.on("tick", () => {
    link
      .attr("x1", d => d.source.x)
      .attr("y1", d => d.source.y)
      .attr("x2", d => d.target.x)
      .attr("y2", d => d.target.y);

    node
      .attr("cx", d => d.x)
      .attr("cy", d => d.y);
  });
});
interface IGraphStatistics {
  totalHostCount: number,
  totalByteCount: number,
  totalPacketCount: number,
  totalTraceCount: number
}
</script>

<style scoped>

</style>
