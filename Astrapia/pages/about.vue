<template>
  <div>
    <svg ref="svg" :width="width" :height="height"></svg>
  </div>
</template>

<script>
import * as d3 from 'd3';

export default {
  data() {
    return {
      width: 960,
      height: 500,
      nodes: [],
      links: []
    };
  },
  mounted() {
    this.initializeForceLayout();
  },
  methods: {
    initializeForceLayout() {
      const svg = d3.select(this.$refs.svg);

      const simulation = d3.forceSimulation()
        .force("link", d3.forceLink().distance(200).strength(.6))
        .force("charge", d3.forceManyBody())
        .force("x", d3.forceX(this.width / 2))
        .force("y", d3.forceY(this.height / 2))
        .on("tick", this.tick);

      this.nodes = [{ id: "a", x: this.width / 2, y: this.height / 2 }];
      this.links = [];
      this.start(svg, simulation);

      setTimeout(() => {
        this.nodes = [
          { id: "a", x: this.width / 2, y: this.height / 2 },
          { id: "b", x: this.width / 2, y: this.height / 2 },
          { id: "c", x: this.width / 2, y: this.height / 2 }
        ];
        this.links = [
          { source: this.nodes[0], target: this.nodes[1] },
          { source: this.nodes[0], target: this.nodes[2] },
          { source: this.nodes[1], target: this.nodes[2] }
        ];
        this.start(svg, simulation);
      }, 1000);

      setTimeout(() => {
        this.nodes.splice(1, 1); // remove node B
        this.links = this.links.filter(link => link.source.id !== "b" && link.target.id !== "b");
        this.start(svg, simulation);
      }, 2000);

      setTimeout(() => {
        const b = { id: "b", x: this.width / 2, y: this.height / 2 };
        this.nodes.splice(1, 0, b); // add node B back
        this.links.push({ source: this.nodes[0], target: b }, { source: b, target: this.nodes[2] });
        this.start(svg, simulation);
      }, 3000);
    },
    start(svg, simulation) {
      const nodeElements = svg.selectAll(".node").data(this.nodes, d => d.id);
      const linkElements = svg.selectAll(".link").data(this.links, d => d.id);

      // Update links
      linkElements.enter()
        .insert("line", ".node")
        .attr("class", "link")
        .style("stroke", "#999")
        .style("stroke-width", "1.5px")
        .merge(linkElements)
        .attr("x1", d => d.source.x)
        .attr("y1", d => d.source.y)
        .attr("x2", d => d.target.x)
        .attr("y2", d => d.target.y);

      // Remove old links
      linkElements.exit().remove();

      // Update nodes
      nodeElements.enter()
        .append("circle")
        .attr("class", d => "node " + d.id)
        .attr("r", 8)
        .merge(nodeElements)
        .attr("cx", d => d.x)
        .attr("cy", d => d.y);

      // Remove old nodes
      nodeElements.exit().remove();

      simulation.nodes(this.nodes);
      simulation.force("link").links(this.links);
      simulation.restart();
    },
    tick() {
      const svg = d3.select(this.$refs.svg);
      const nodeElements = svg.selectAll(".node");
      const linkElements = svg.selectAll(".link");

      linkElements
        .attr("x1", d => d.source.x)
        .attr("y1", d => d.source.y)
        .attr("x2", d => d.target.x)
        .attr("y2", d => d.target.y);

      nodeElements
        .attr("cx", d => d.x)
        .attr("cy", d => d.y);
    }
  }
};
</script>

<style scoped>
</style>
