<template>
  <div id="graph">
    <svg ref="svg" width="960" height="600"></svg>
    <div id="tooltip" style="visibility: hidden;"></div>
    <TopologyFooter @recenter="recenterGraph" :metaData="metaData" element-id="graph"/>
  </div>
</template>

<script>
import * as d3 from 'd3';
import TopologyFooter from "~/components/TopologyFooter.vue";

export default {
  components: {TopologyFooter},
  props: {
    data: {
      type: Object,
      default: null
    }
  },
  data() {
    return {
      previousData: null,
      metaData: null,
      isDragging: false,
      graph: null,
      svg: null,
      zoom: null
    };
  },
  watch: {
    data(newData) {
      if (newData && this.hasDataChanged(newData)) {
        this.metaData = newData.graphStatistics;
        this.renderGraph(newData);
      }
    }
  },
  mounted() {
    if (this.data) {
      this.renderGraph(this.data);
    }
  },
  methods: {
    renderGraph(graph) {
      const svg = d3.select(this.$refs.svg)
        .attr("width", window.innerWidth)
        .attr("height", window.innerHeight);

      const zoom = d3.zoom().on('zoom', (e) => {
        const transform = e.transform;
        svg.selectAll(".link").attr('transform', transform);
        svg.selectAll(".node").attr('transform', transform);
        svg.selectAll(".label").attr('transform', transform);
        svg.selectAll(".link-overlay").attr('transform', transform);
      });

      svg.call(zoom);

      const simulation = d3.forceSimulation(graph.nodes)
        .force("link", d3.forceLink(graph.edges).id(d => d.id).distance(200).strength(1))
        .force("charge", d3.forceManyBody().strength(-3000))
        .force("center", d3.forceCenter(window.innerWidth / 2, window.innerHeight / 2))
        .force("collide", d3.forceCollide(10));

      const drag = d3.drag()
        .on("start", function (event) {
          this.isDragging = true;
          if (!event.active) simulation.alphaTarget(0.3).restart();
          event.subject.fx = null;
          event.subject.fy = null;
        })
        .on("drag", function (event) {
          event.subject.fx = event.x;
          event.subject.fy = event.y;
        })
        .on("end", function () {
          this.isDragging = false;
          simulation.alphaTarget(0).restart();
        });

      //links
      const linkWidthScale = d3.scaleLinear()
        .domain([0, 1000])
        .clamp(true)
        .range([0.5, 4]);

      const linkColorScale = d3.scaleLinear()
        .domain([0, 10000])
        .clamp(true)
        .range(["#a4a4a4", "#3f3f3f"]);

      const links = svg.selectAll(".link")
        .data(graph.edges, d => d.source.id + '-' + d.target.id);

      links.exit().remove();

      const newLinks = links.enter()
        .append("line")
        .attr("class", "link")
        .merge(links)
        .attr("stroke", d => linkColorScale(d.byteCount))
        .attr("stroke-width", d => linkWidthScale(d.packetCount));

      //linkOverlays
      const linkOverlays = svg.selectAll(".link-overlay")
        .data(graph.edges, d => d.source.id + '-' + d.target.id);

      linkOverlays.exit().remove();

      const newLinkOverlays = linkOverlays.enter()
        .append("line")
        .attr("class", "link-overlay")
        .merge(links)
        .attr("stroke", "transparent")
        .attr("stroke-width", 8);

      //nodes
      const nodes = svg.selectAll(".node")
        .data(graph.nodes, d => d.id);

      nodes.exit().remove();

      const newNodes = nodes.enter()
        .append("circle")
        .attr("class", "node")
        .attr("r", 8)
        .attr("fill", "#537B87")
        .attr("cx", window.innerWidth/2 )
        .attr("cy", window.innerHeight /2)
        .merge(nodes);

      //label

      const labels = svg.selectAll(".label")
        .data(graph.nodes, d => d.id);

      labels.exit().remove();

      const newLabels = labels.enter()
        .append("text")
        .attr("class", "label")
        .attr("x", 0)
        .attr("dy", "1.5em")
        .attr("text-anchor", "middle")
        .attr("font-family", "Arial")
        .style("fill", "#414141")
        .merge(labels)
        .text(d => d.id);

      //tooltip
      const tooltip = d3.select("#tooltip");

      svg.selectAll(".node").call(drag)
        .on("mouseenter", function (event, d) {
          if (!this.isDragging) {
            tooltip.style("visibility", "visible");
            tooltip.html(d.name);
          }
        })
        .on("mousemove", function (event) {
          if (!this.isDragging) {
            const [x, y] = d3.pointer(event, svg.node());
            tooltip.style("left", (x + 10) + "px")
              .style("top", (y + 10) + "px");
          }
        })
        .on("mouseleave", function () {
          if (!this.isDragging) {
            tooltip.style("visibility", "hidden");
          }
        });

      svg.selectAll(".link-overlay")
        .on("mouseover", function (event, d) {
          tooltip.style("visibility", "visible");
          tooltip.html(`Bytes: ${d.byteCount} <br> Traces: ${d.traceCount} <br> Packets: ${d.packetCount}`);
        })
        .on("mousemove", function (event) {
          const tooltipWidth = tooltip.node().getBoundingClientRect().width;
          tooltip.style("left", (event.pageX - tooltipWidth / 2) + "px")
            .style("top", (event.pageY + 10) + "px");
        })

        .on("mouseleave", function () {
          tooltip.style("visibility", "hidden");
        });

      simulation.on("tick", () => {
        newLinks
          .attr("x1", d => d.source.x)
          .attr("y1", d => d.source.y)
          .attr("x2", d => d.target.x)
          .attr("y2", d => d.target.y);

        newLinkOverlays
          .attr("x1", d => d.source.x)
          .attr("y1", d => d.source.y)
          .attr("x2", d => d.target.x)
          .attr("y2", d => d.target.y);

        newNodes
          .attr("cx", d => d.x)
          .attr("cy", d => d.y);

        newLabels
          .attr("x", d => d.x)
          .attr("y", d => d.y);
      });

      this.previousData = JSON.parse(JSON.stringify(graph));
      this.graph = graph;
      this.svg = svg;
      this.zoom = zoom;
    },
    recenterGraph() {
      if (!this.graph || !this.svg || !this.zoom) return;

      const centerX = this.graph.nodes.reduce((sum, node) => sum + node.x, 0) / this.graph.nodes.length;
      const centerY = this.graph.nodes.reduce((sum, node) => sum + node.y, 0) / this.graph.nodes.length;

      this.svg.call(this.zoom.translateTo, centerX, centerY);
    },
    hasDataChanged(newData) {
      if (this.previousData === null) {
        return newData !== null;
      }

      if (newData === null) {
        return true;
      }

      return !this.isEqual(this.previousData, newData);
    },
    isEqual(obj1, obj2) {
      if (typeof obj1 !== 'object' || typeof obj2 !== 'object')
        return obj1 === obj2;

      const props1 = Object.keys(obj1);
      const props2 = Object.keys(obj2);
      if (props1.length !== props2.length)
        return false;

      for (let prop of props1) {
        if (!this.isEqual(obj1[prop], obj2[prop]))
          return false;
      }

      return true;
    }
  }
};
</script>

<style scoped>
svg {
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
