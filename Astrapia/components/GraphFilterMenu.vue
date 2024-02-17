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
import {ref, watch} from "vue";
import layoutService from "~/services/layoutService";
import LayerManagement from "~/components/LayerManagement.vue";


const props = defineProps({
  layout: String
});

const graphFilterMenuState = ref({
  isExpanded: false,
  selectedLayout: {
    name: '',
    layers: [
      {
        name: 'Layer1Layer1Layer1Layer1Layer1Layer1Layer1',
        type: 'type1',
        enabled: false,
        description: 'description1'
      },
      {
        name: 'Layer2',
        type: 'type2',
        enabled: true,
        description: 'description2description2 djksnvsl ckdsnökn'
      },
      {
        name: 'Layer3',
        type: 'type3',
        enabled: true,
        description: 'description3'
      },
      {
        name: 'Layer4',
        type: 'type4',
        enabled: true,
        description: 'description4'
      },
      {
        name: 'Layer5',
        type: 'type5',
        enabled: true,
        description: 'description5'
      },
      {
        name: 'Layer6',
        type: 'type6',
        enabled: true,
        description: 'description6'
      },
      {
        name: 'Layer7',
        type: 'type7',
        enabled: true,
        description: 'description7'
      },
      {
        name: 'Layer8',
        type: 'type8',
        enabled: true,
        description: 'description8'
      },
      {
        name: 'Layer9',
        type: 'type9',
        enabled: true,
        description: 'description9'
      },
      {
        name: 'Layer10',
        type: 'type10',
        enabled: true,
        description: 'description10'
      },
      {
        name: 'Layer11',
        type: 'type11',
        enabled: true,
        description: 'description11'
      },
      {
        name: 'Layer12',
        type: 'type12',
        enabled: true,
        description: 'description12'
      },
      {
        name: 'Layer13',
        type: 'type13',
        enabled: true,
        description: 'description13'
      },
      {
        name: 'Layer14',
        type: 'type14',
        enabled: true,
        description: 'description14'
      },
      {
        name: 'Layer15',
        type: 'type15',
        enabled: true,
        description: 'description15'
      },
      {
        name: 'Layer16',
        type: 'type16',
        enabled: true,
        description: 'description16'
      },
      {
        name: 'Layer17',
        type: 'type17',
        enabled: true,
        description: 'description17'
      },
      {
        name: 'Layer18',
        type: 'type18',
        enabled: true,
        description: 'description18'
      },
      {
        name: 'Layer19',
        type: 'type19',
        enabled: true,
        description: 'description19'
      },
      {
        name: 'Layer20',
        type: 'type20',
        enabled: true,
        description: 'description20'
      },
      {
        name: 'Layer21',
        type: 'type21',
        enabled: true,
        description: 'description21'
      },
      {
        name: 'Layer22',
        type: 'type22',
        enabled: true,
        description: 'description22'
      },
      {
        name: 'Layer23',
        type: 'type23',
        enabled: true,
        description: 'description23'
      },
      {
        name: 'Layer24',
        type: 'type24',
        enabled: true,
        description: 'description24'
      },
      {
        name: 'Layer25',
        type: 'type25',
        enabled: true,
        description: 'description25'
      },
    ]
  }
})

async function getLayersOfLayout(layout: string) {
  const layoutData = await layoutService.getLayoutByName(layout);
  graphFilterMenuState.value.selectedLayout.layers = layoutData.layers;
}

watch(() => graphFilterMenuState.value.selectedLayout.name, async (newLayoutName) => {
  // await getLayersOfLayout(newLayoutName);
});

watch(() => props.layout!, (newLayout, oldLayout) => {
  graphFilterMenuState.value.selectedLayout.name = newLayout;
});
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
