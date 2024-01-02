<template>
  <div id="graph">
    <svg id="graph-svg" width="960" height="600"></svg>
    <div id="tooltip" style="visibility: hidden;"></div>
    <TopologyFooter @recenter="recenterNodes" :metaData="metaData" element-id="graph"/>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref,watch } from 'vue';
import * as d3 from 'd3';
import TopologyFooter from "~/components/TopologyFooter.vue";

const props = defineProps({
  data: {
    type: Object,
    required: true,
  },
});
const { data } = props;

let nodes, edges, simulation, zoom, svg;
const metaData = ref();
let isDragging = false;

onMounted(async () => {
  await UpdateGraph();
  await initGraph();
});

const UpdateGraph = async () => {
  if (props.data) {
    metaData.value = props.data.graphStatistics;
    nodes = Object.values(props.data.nodes);
    edges = Object.values(props.data.edges);
  }
};

watch(() => props.data, async (newVal) => {
  if (newVal) {
    await UpdateGraph();
    await initGraph();
  }
}, { immediate: true });

function recenterNodes() {
  const centerX = nodes.reduce((sum, node) => sum + node.x, 0) / nodes.length;
  const centerY = nodes.reduce((sum, node) => sum + node.y, 0) / nodes.length;

  svg.call(zoom.translateTo, centerX, centerY);
}

const initGraph = async () => {
  zoom = d3.zoom().on('zoom', (e) => {
    const transform = e.transform;
    link.attr('transform', transform);
    node.attr('transform', transform);
    label.attr('transform', transform);
  });

  const drag = d3.drag()
      .on("start", function (event) {
        isDragging = true;
        if (!event.active) simulation.alphaTarget(0.3).restart();
        event.subject.fx = null;
        event.subject.fy = null;
      })
      .on("drag", function (event) {
        event.subject.fx = event.x;
        event.subject.fy = event.y;
      })
      .on("end", function () {
        isDragging = false;
        simulation.alphaTarget(0).restart();
      });

  svg = d3.select("#graph-svg")
      .attr("width", window.innerWidth)
      .attr("height", window.innerHeight)
      .call(zoom);

  simulation = d3.forceSimulation(nodes)
      .force("link", d3.forceLink(edges).id((d: { id: any; }) => d.id).distance(300).strength(1))
      .force("charge", d3.forceManyBody().strength(-7000))
      .force("center", d3.forceCenter(window.innerWidth / 2, window.innerHeight / 2))
      .force("x", d3.forceX().strength(0.5))
      .force("y", d3.forceY().strength(0.5))
      .force("collide", d3.forceCollide(10));

  let strokeWidthScale = d3.scaleLinear()
      .domain([0, 50000000])
      .range([2, 10]);


  let link = svg.append("g")
      .attr("stroke", "#999")
      .attr("stroke-opacity", 0.6)
      .selectAll("line")
      .data(edges)
      .join("line")
      .attr("stroke-width", d => strokeWidthScale(d.byteCount));


  let linkHitArea = svg.append("g")
      .attr("stroke", "transparent")
      .attr("stroke-width", 10)
      .selectAll("line")
      .data(edges)
      .join("line")
      .on("mouseover", function(event, d) {
        tooltip.style("visibility", "visible");
        tooltip.html(`Bytes: ${d.byteCount} <br> Traces: ${d.traceCount} <br> Packets: ${d.packetCount}`);
      })
      .on("mousemove", function(event) {
        tooltip.style("left", (event.pageX-80) + "px")
            .style("top", (event.pageY+20) + "px");
      })

      .on("mouseleave", function() {
        tooltip.style("visibility", "hidden");
      });

  let node = svg.append("g")
      .attr("stroke", "#fff")
      .attr("stroke-width", 1.5)
      .selectAll("circle")
      .data(nodes)
      .join("circle")
      .attr("r", 10)
      .attr("fill", "#537B87")
      .call(drag);

  const tooltip = d3.select("#tooltip");

  node
      .on("mouseenter", function (event, d) {
        if (!isDragging) {
          tooltip.style("visibility", "visible");
          tooltip.html(d.name);
        }
      })
      .on("mousemove", function (event) {
        if (!isDragging) {
          tooltip.style("left", (d3.select(this).attr("cx") + event.pageX) + "px")
              .style("top", (d3.select(this).attr("cy") + event.pageY) + "px");
        }
      })
      .on("mouseleave", function () {
        if (!isDragging) {
          tooltip.style("visibility", "hidden");
        }
      });

  let label = svg.append("g")
      .selectAll("text")
      .data(nodes)
      .enter()
      .append("text")
      .attr("x", 0)
      .attr("dy", "1.5em")
      .attr("text-anchor", "middle")
      .attr("font-family", "Arial")
      .style("fill", "#414141")
      .text(function (d) {
        return d.name;
      });

  simulation.alpha(0.1).alphaTarget(0).restart();
  simulation.on("tick", () => {
    link
        .attr("x1", d => d.source.x)
        .attr("y1", d => d.source.y)
        .attr("x2", d => d.target.x)
        .attr("y2", d => d.target.y);

    linkHitArea
        .attr("x1", d => d.source.x)
        .attr("y1", d => d.source.y)
        .attr("x2", d => d.target.x)
        .attr("y2", d => d.target.y);

    node
        .attr("cx", d => d.x)
        .attr("cy", d => d.y);

    label
        .attr("x", d => d.x)
        .attr("y", d => d.y);
  });
};
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
