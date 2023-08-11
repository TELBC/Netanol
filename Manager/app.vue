<template>
  <v-network-graph
    class="graph"
    :nodes="nodes"
    :edges="edges"
    :configs="configs"
  />
</template>

<style scoped>
.graph{
  height: 100vh;
  width: 100vw;
}
</style>

<script setup lang="ts">
import {parseJsonData} from "~/components/NetworkGraphParsing";
import * as vNG from "v-network-graph"

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

const { nodes, edges } = parseJsonData(jsonData);

const configs = vNG.defineConfigs({
  view: {
    grid: {
      visible: true,
      interval: 10,
      thickIncrements: 10,
      line: {
        color: "#e0e0e0",
        width: 1,
        dasharray: 1,
      },
      thick: {
        color: "#cccccc",
        width: 1,
        dasharray: 0,
      },
    },
    layoutHandler: new vNG.GridLayout({ grid: 15 }),
  },
})

</script>
