<template>
  <div id="graph">
    <svg id="graph-svg" width="960" height="600"></svg>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import * as d3 from 'd3';
import topologyService from '~/services/topology.service';
import { ref } from 'vue';

const layout = 'test';
const rangeValue = ref(2);

const fetchAndUpdateGraph: () => Promise<void> = async () => {
  const now = new Date();
  const dateRange: { from: Date; to: Date } = {
    from: new Date(now.getTime() - rangeValue.value * 60 * 1000),
    to: new Date()
  };

  const data = await topologyService.getTopology('test', dateRange.from, dateRange.to);
  const nodes = Object.values(data.nodes);
  const edges = Object.values(data.edges);

  const zoom = d3.zoom().on('zoom', (e) => {
    const transform = e.transform;
    link.attr('transform', transform);
    node.attr('transform', transform);
  });

  const drag = d3.drag()
    .on("start", dragstarted)
    .on("drag", dragged)
    .on("end", dragended);

  const svg = d3.select("#graph-svg")
    .attr("width", window.innerWidth)
    .attr("height", window.innerHeight)
    .call(zoom);

  function dragstarted(event) {
    if (!event.active) simulation.alphaTarget(0.3).restart();
    event.subject.fx = null;
    event.subject.fy = null;
  }

  function dragged(event) {
    event.subject.fx = event.x;
    event.subject.fy = event.y;
  }

  function dragended(event) {
    if (!event.active) simulation.alphaTarget(0);
  }


  const simulation = d3.forceSimulation(nodes)
    .force("link", d3.forceLink(edges).id((d: { id: any; }) => d.id).distance(100).strength(1))
    .force("charge", d3.forceManyBody().strength(-1000))
    .force("center", d3.forceCenter(window.innerWidth / 2, window.innerHeight / 2))
    .force("x", d3.forceX())
    .force("y", d3.forceY())
    .force("collide", d3.forceCollide(10));

  let link = svg.append("g")
    .attr("stroke", "#999")
    .attr("stroke-opacity", 0.6)
    .selectAll("line")
    .data(edges)
    .join("line");

  let node = svg.append("g")
    .attr("stroke", "#fff")
    .attr("stroke-width", 1.5)
    .selectAll("circle")
    .data(nodes)
    .join("circle")
    .attr("r", 10)
    .attr("fill", "#537B87")
    .call(drag);

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
};

onMounted(async () => {
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
</style>
