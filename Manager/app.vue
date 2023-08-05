<script setup lang="ts">
const jsonData = `{
  "nodes": {
    "1" :{
      "id": 1,
      "ipAddress": "192.168.1.12"
    },
    "2" :{
      "id": 2,
      "ipAddress": "10.5.12.254"
    }
  },
  "traces": [
    {
      "sourceHostId": 1,
      "destinationHostId": 2,
      "count": 332
    }
  ]
}`;

const data = JSON.parse(jsonData);

const nodes = {};
const edges = {};

for (const nodeId in data.nodes) {
  const node = data.nodes[nodeId];
  nodes[`node${nodeId}`] = { name: node.ipAddress, ipAddress: node.ipAddress };
}

data.traces.forEach((trace, index) => {
  edges[`edge${index}`] = {
    source: `node${trace.sourceHostId}`,
    target: `node${trace.destinationHostId}`,
    count: trace.count
  };
});

</script>

<template>
  <v-network-graph
    class="graph"
    :nodes="nodes"
    :edges="edges"
  />
</template>

<style>
.graph {
  width: 800px;
  height: 600px;
  border: 1px solid #000;
}
</style>
