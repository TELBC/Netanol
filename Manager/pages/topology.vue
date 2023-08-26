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

const graphData = `{
  "nodes": {
    "1": {
      "id": 1,
      "ipAddress": "192.168.1.12"
    },
    "2": {
      "id": 2,
      "ipAddress": "10.5.12.254"
    },
    "3": {
      "id": 3,
      "ipAddress": "192.168.1.25"
    },
    "4": {
      "id": 4,
      "ipAddress": "10.5.12.1"
    },
    "5": {
      "id": 5,
      "ipAddress": "172.16.8.67"
    },
    "6": {
      "id": 6,
      "ipAddress": "192.168.2.50"
    },
    "7": {
      "id": 7,
      "ipAddress": "10.5.14.12"
    },
    "8": {
      "id": 8,
      "ipAddress": "192.168.3.77"
    },
    "9": {
      "id": 9,
      "ipAddress": "10.7.1.32"
    },
    "10": {
      "id": 10,
      "ipAddress": "172.16.10.21"
    },
    "11": {
      "id": 11,
      "ipAddress": "192.168.4.120"
    },
    "12": {
      "id": 12,
      "ipAddress": "10.7.2.89"
    },
    "13": {
      "id": 13,
      "ipAddress": "192.168.5.15"
    },
    "14": {
      "id": 14,
      "ipAddress": "10.8.1.5"
    },
    "15": {
      "id": 15,
      "ipAddress": "192.168.6.200"
    }
  },
  "traces": [
    {
      "sourceHostId": 1,
      "destinationHostId": 2,
      "count": 332
    },
    {
      "sourceHostId": 1,
      "destinationHostId": 3,
      "count": 127
    },
    {
      "sourceHostId": 2,
      "destinationHostId": 4,
      "count": 215
    },
    {
      "sourceHostId": 3,
      "destinationHostId": 5,
      "count": 73
    },
    {
      "sourceHostId": 3,
      "destinationHostId": 6,
      "count": 184
    },
    {
      "sourceHostId": 5,
      "destinationHostId": 6,
      "count": 94
    },
    {
      "sourceHostId": 6,
      "destinationHostId": 7,
      "count": 67
    },
    {
      "sourceHostId": 7,
      "destinationHostId": 8,
      "count": 321
    },
    {
      "sourceHostId": 8,
      "destinationHostId": 10,
      "count": 55
    },
    {
      "sourceHostId": 9,
      "destinationHostId": 10,
      "count": 267
    },
    {
      "sourceHostId": 3,
      "destinationHostId": 11,
      "count": 143
    },
    {
      "sourceHostId": 11,
      "destinationHostId": 12,
      "count": 98
    },
    {
      "sourceHostId": 12,
      "destinationHostId": 13,
      "count": 210
    },
    {
      "sourceHostId": 13,
      "destinationHostId": 14,
      "count": 75
    },
    {
      "sourceHostId": 14,
      "destinationHostId": 15,
      "count": 183
    }
  ]
}`;

const { nodes, edges } = parseJsonData(graphData);

</script>

<style scoped>
.graph{
  position: fixed;
  width: 100vw;
  height: 100vh;
}
</style>
