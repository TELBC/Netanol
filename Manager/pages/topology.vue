<template>
  <v-network-graph
    class="graph"
    :nodes="nodes"
    :edges="edges"
    :configs="networkGraphConfigs"
  />
</template>

<script setup lang="ts">
import { VNetworkGraph} from "v-network-graph"
import {networkGraphConfigs} from "assets/v-network-graph-configs";

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

</script>

<style scoped>
.graph{
  position: fixed;
  width: 100vw;
  height: 100vh;
}
</style>
