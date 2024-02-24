<template>
  <div class="graph-filter-menu">
    <div class="collapse-expand-container" @click="graphFilterMenuState.isExpanded = !graphFilterMenuState.isExpanded">
      <font-awesome-icon icon="fa-solid fa-chevron-left" class="collapse-expand-icon" v-bind:class="{ 'collapse-expand-layer-rotate': graphFilterMenuState.isExpanded }" />
    </div>
    <div class="graph-filter-menu-container" :class="{ 'expanded': graphFilterMenuState.isExpanded }">
      <LayerManagement v-bind:layout="graphFilterMenuState.selectedLayout.name" v-bind:layers="graphFilterMenuState.selectedLayout.layers" />
    </div>
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import {ref, watch, provide} from "vue";
import layoutService from "~/services/layoutService";
import LayerManagement from "~/components/LayerManagement.vue";

const props = defineProps({
  layout: String
});

const graphFilterMenuState = ref({
  isExpanded: false,
  selectedLayout: {
    name: '',
    layers: []
  }
})

async function getLayersOfLayout() {
  const layoutData = await layoutService.getLayoutByName(graphFilterMenuState.value.selectedLayout.name);
  graphFilterMenuState.value.selectedLayout.layers = layoutData.layers;
}

watch(() => graphFilterMenuState.value.selectedLayout.name, async () => {
  await getLayersOfLayout();
});

watch(() => props.layout!, (newLayout, oldLayout) => {
  graphFilterMenuState.value.selectedLayout.name = newLayout;
});

provide('getLayersOfLayout', getLayersOfLayout);
</script>

<style scoped>
.graph-filter-menu {
  position: absolute;
  display: flex;
  flex-direction: row;
  justify-content: flex-end;
  top: 4.7vh;
  right: 0;
  width: 15vw;
  margin: 0;
  height: 95.3vh;
  z-index: 14;
  overflow: hidden;
}

.graph-filter-menu-container.expanded {
  width: 14vw;
}

.collapse-expand-container {
  position: relative;
  top: 7vh;
  left: auto;
  width: 2vw;
  height: 5vh;
  border-left: 1px solid #424242;
  border-top: 1px solid #424242;
  border-bottom: 1px solid #424242;
  border-radius: 4px;
  cursor: pointer;
  margin: 0;
  margin-right: -1vw;
  background-color: #e0e0e0;
  color: #8d8d8d;
}

.collapse-expand-icon {
  position: relative;
  top: 1.5vh;
  left: 0.25vw;
  font-size: 2vh;
  transition: transform 0.2s ease-in-out;
}

.collapse-expand-layer-rotate {
  transform: rotate(180deg);
}

.graph-filter-menu-container {
  width: 0;
  height: 100%;
  background-color: white;
  border-left: 1px solid #424242;
  border-top: 1px solid #424242;
  margin: 0;
  transition: 0.2s ease-in-out;
  left: auto;
  right: 0;
  z-index: 14;
  font-family: 'Open Sans', sans-serif;
  user-select: none;
}
</style>
