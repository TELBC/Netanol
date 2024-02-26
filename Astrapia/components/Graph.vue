<template>
  <div id="graph">
    <svg ref="svg" width="960" height="600"></svg>
    <TopologyFooter v-if="metaData" @recenter="recenterGraph" :metaData="metaData" @toggleSimulation="toggleSimulation" @updateDistance="updateLinkDistance" @updateSim="updateSimForce" @intervalAmount="$emit('intervalAmount', $event)" element-id="graph"/>
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
      previousData: {},
      metaData: null,
      zoom: null,
      simulationFrozen: false,
      linkDistance: 100,
      strengthForce: 500
    };
  },
  watch: {
    data: {
      handler(newData) {
        if (newData && this.hasDataChanged(newData)) {
          this.metaData = newData.graphStatistics;
          this.previousData = newData;
          this.recenterGraph();
          this.updateChart(newData);
        }
      }
    }
  },
  mounted() {
    this.initChart();
  },
  methods: {
    initChart() {
      this.simulation = d3.forceSimulation()
        .force("charge", d3.forceManyBody().strength((-1) * this.strengthForce))
        .force("link", d3.forceLink().id(d => d.id).distance(this.linkDistance))
        .force("x", d3.forceX())
        .force("y", d3.forceY())
        .on("tick", this.ticked);

      this.chart = d3.select(this.$refs.svg)
        .attr("viewBox", [-window.innerWidth / 2, -window.innerHeight / 2, window.innerWidth, window.innerHeight])
        .attr("width", window.innerWidth)
        .attr("height", window.innerHeight);

      this.zoom = d3.zoom().on('zoom', (e) => {
        const transform = e.transform;
        this.chart.selectAll("line").attr('transform', transform);
        this.chart.selectAll("circle").attr('transform', transform);
        this.chart.selectAll("text").attr('transform', transform);
      });

      this.chart.call(this.zoom);

      this.link = this.chart.append("g")
        .attr("stroke", "#a4a4a4")
        .attr("stroke-width", 0.5)
        .selectAll("line");

      this.linkHitArea = this.chart.append("g")
        .attr("stroke", "transparent")
        .attr("stroke-width", 5)
        .attr("opacity", 0)
        .selectAll("line");

      this.node = this.chart.append("g")
        .attr("stroke", "#fff")
        .attr("stroke-width", 1)
        .selectAll("circle");

      this.label = this.chart.append("g")
        .attr("font-family", "Arial")
        .style("fill", "#414141")
        .selectAll("text");
    },
    ticked() {
      this.link
        .attr("x1", d => d.source.x)
        .attr("y1", d => d.source.y)
        .attr("x2", d => d.target.x)
        .attr("y2", d => d.target.y);

      this.linkHitArea
        .attr("x1", d => d.source.x)
        .attr("y1", d => d.source.y)
        .attr("x2", d => d.target.x)
        .attr("y2", d => d.target.y);

      this.node
        .attr("cx", d => d.x)
        .attr("cy", d => d.y);

      this.label
        .attr("x", d => d.x)
        .attr("y", d => d.y);
    },
    updateChart({nodes, edges}) {
      const old = new Map(this.node.data().map(d => [d.id, d]));
      nodes = nodes.map(d => ({...old.get(d.id), ...d}));
      edges = edges.map(d => ({...d}));

      const packetCountScale = d3.scaleLinear()
        .domain([1, d3.max(edges, d => d.packetCount)])
        .range([0.5, 2]);

      const colorScale = d3.scaleSequential()
        .domain([0, d3.max(edges, d => d.byteCount)])
        .interpolator((t) => d3.interpolate('#d7d7d7', '#494949')(1 - t));

      this.link = this.link
        .data(edges, d => [d.source, d.target])
        .join(enter => enter.insert("line", "circle")
          .attr("stroke-width", d => packetCountScale(d.packetCount))
          .attr("stroke", d => colorScale(d.byteCount)));

      this.linkHitArea = this.linkHitArea
        .data(edges, d => [d.source, d.target])
        .join(enter => enter.insert("line", "circle"))
        .call(link => link.append("title")
          .text(d => `Bytes: ${d.byteCount} \nPackets: ${d.packetCount} \nProtocols: ${d.dataProtocol}`));

      this.node = this.node
        .data(nodes, d => d.id)
        .join(enter => enter.append("circle")
          .attr("r", 8)
          .attr("fill", "#537B87")
          .call(this.drag(this.simulation))
          .call(node => node.append("title")
            .text(d => d.name))
        );

      this.label = this.label
        .data(nodes, d => d.name)
        .join(enter => enter.append("text")
          .attr("text-anchor", "middle")
          .attr("dy", "1.8em")
          .attr("font-size", "10px")
          .text(d => d.name)
        );

      this.simulation.nodes(nodes);
      this.simulation.force("link").links(edges);
      this.simulation.alpha(1).restart().tick();
      this.ticked();
    },
    drag(simulation) {
      function dragstarted(event) {
        if (!event.active) simulation.alphaTarget(0.3).restart();
        event.subject.fx = null;
        event.subject.fy = null;
      }

      function dragged(event) {
        event.subject.fx = event.x;
        event.subject.fy = event.y;
      }

      function dragended() {
        simulation.alphaTarget(0).restart();
      }

      return d3.drag()
        .on("start", dragstarted)
        .on("drag", dragged)
        .on("end", dragended);
    },
    updateLinkDistance(distance) {
      this.linkDistance = distance;
      if (this.simulation) {
        this.simulation.force("link").distance(this.linkDistance);
        if (!this.simulationFrozen) {
          this.simulation.alpha(0.5).restart();
        }
      }
    },
    updateSimForce(force) {
      this.strengthForce = force;
      if (this.simulation) {
        this.simulation.force("charge", d3.forceManyBody().strength((-1) * this.strengthForce))
        if (!this.simulationFrozen) {
          this.simulation.alpha(0.5).restart();
        }
      }
    },
    toggleSimulation(frozen) {
      this.simulationFrozen = frozen;

      if (this.simulation) {
        if (this.simulationFrozen) {
          this.simulation.stop();
        } else {
          this.simulation.restart();
        }
      }
    },
    recenterGraph() {
      this.chart.transition().duration(250).call(this.zoom.transform, d3.zoomIdentity);
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
</style>
