<template>
  <FlowImportGraphTimeframeSelector :from-value="from" :to-value="to" />
  <FlowImportGraph :from="from" :to="to" />
</template>

<script setup lang="ts">
import {ref} from 'vue';
import FlowImportGraph from "~/components/FlowImportGraph.vue";
import FlowImportGraphTimeframeSelector from "~/components/FlowImportGraphTimeframeSelector.vue";

const from = ref(new Date(new Date().getTime() - 5 * 60 * 1000).toISOString().slice(0,16));
const to = ref(new Date().toISOString().slice(0,16));
</script>

<style scoped>

</style>
