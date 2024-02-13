<template>
  <div class="layer-list">
    <div class="collapse-expand-container" @click="layerListState.isExpanded = !layerListState.isExpanded">
      <font-awesome-icon icon="fa-solid fa-chevron-left" class="collapse-expand-icon" v-bind:class="{ 'collapse-expand-layer-rotate': layerListState.isExpanded }" />
    </div>
    <div class="layer-list-container" :class="{ 'expanded': layerListState.isExpanded }">
      <div class="layer" v-for="layer in layerListState.selectedLayout.layers" :key="rerenderer.layerListRerender" @click="openCloseLayer(layer.name)" v-bind:class="{ 'create-open-hide': layerListState.createLayerOpen }">
        <div>
          <font-awesome-icon icon="fa-solid fa-chevron-right" v-bind:class="{'expand-layer-icon': layerListState.layerOpen === layer.name}" class="expand-layer" />
        </div>
        <div class="layer-name">
          {{ layer.name }}
        </div>
        <input type="checkbox" class="theme-checkbox" @click.stop>
      </div>
      <div class="create-form" v-bind:class="{ 'create-layer-open': layerListState.createLayerOpen }">
        <input type="text" placeholder="Layer Name" />
      </div>
      <div class="create-layer" @click="layerListState.createLayerOpen = !layerListState.createLayerOpen">
        <font-awesome-icon icon="fa-solid fa-plus" class="fa-plus" :class="{ 'rotate-icon': layerListState.createLayerOpen }" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import {ref, watch} from "vue";
import layoutService from "~/services/layoutService";


const props = defineProps({
  layout: String
});

const layerListState = ref({
  isExpanded: false,
  selectedLayout: {
    name: '',
    layers: [
      {
        name: 'Layer1'
      },
      {
        name: 'Layer2'
      },
      {
        name: 'Layer3'
      },
      {
        name: 'Layer4'
      },
      {
        name: 'Layer5'
      },
      {
        name: 'Layer6'
      },
      {
        name: 'Layer7'
      },
      {
        name: 'Layer8'
      },
      {
        name: 'Layer9'
      },
      {
        name: 'Layer10'
      }
    ]
  },
  layerOpen: '',
  createLayerOpen: false
})

const rerenderer = ref({
  layerListRerender: layerListState.value.selectedLayout.layers.length
})

function openCloseLayer(layerName: string) {
  if (layerListState.value.layerOpen === layerName) {
    layerListState.value.layerOpen = '';
  } else {
    layerListState.value.layerOpen = layerName;
  }
}

async function getLayersOfLayout(layout: string) {
  const layoutData = await layoutService.getLayoutByName(layout);
  layerListState.value.selectedLayout.layers = layoutData.layers;
  console.log(layoutData.layers)
  console.log(layerListState.value.selectedLayout.layers)
}

watch(() => layerListState.value.selectedLayout.name, async (newLayoutName) => {
  // await getLayersOfLayout(newLayoutName);
});

watch(() => props.layout!, (newLayout, oldLayout) => {
  layerListState.value.selectedLayout.name = newLayout;
});
</script>

<style scoped>
.layer-list {
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

.layer-list-container {
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

.layer-list-container.expanded {
  width: 14vw;
}

.layer {
  display: flex;
  flex-direction: row;
  align-items: center;
  padding: 1vh 1vw;
  cursor: pointer;
  transition: 0.2s ease-in-out;
  height: 3vh;
  width: 90%;
}

.expand-layer {
  transition: transform 0.2s ease-in-out;
  margin-right: 0.5vw;
}

.expand-layer-icon {
  transform: rotate(-90deg);
}

.create-layer {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: center;
  padding: 1vh 1vw;
  border-top: 1px solid #424242;
  cursor: pointer;
  transition: 0.2s ease-in-out;
  position: fixed;
  bottom: 0;
  width: 13vw;
  height: 3%;
}

.create-form {
  transition: 0.2s ease-in-out;
  visibility: hidden;
  opacity: 0;
}

.create-layer-open {
  display: flex;
  flex-direction: column;
  height: 94.5%;
  visibility: visible;
  opacity: 1;
}

.create-open-hide {
  visibility: hidden;
  opacity: 0;
}

.fa-plus {
  transition: transform 0.2s ease-in-out;
  font-weight: bolder;
  font-size: 2.5vh;
}

.rotate-icon {
  transform: rotate(-90deg);
}

.theme-checkbox {
  position: relative;
  --toggle-size: 16px;
  -webkit-appearance: none;
  -moz-appearance: none;
  appearance: none;
  width: 2.75em;
  height: 1.625em;
  background: linear-gradient(to right, white 50%, #7EA0A9 50%) no-repeat;
  background-size: 205%;
  background-position: 0;
  -webkit-transition: 0.4s;
  -o-transition: 0.4s;
  transition: 0.4s;
  border-radius: 99em;
  cursor: pointer;
  font-size: var(--toggle-size);
  border: 1px solid #424242;
  margin-left: auto;
}

.theme-checkbox::before {
  content: "";
  width: 1.3em;
  height: 1.3em;
  top: 50%;
  transform: translateY(-50%);
  left: 0.15em;
  position: absolute;
  background: linear-gradient(to right, white 50%, #7EA0A9 50%) no-repeat;
  background-size: 205%;
  background-position: 100%;
  border-radius: 50%;
  -webkit-transition: 0.4s;
  -o-transition: 0.4s;
  transition: 0.4s;
}

.theme-checkbox:checked::before {
  left: calc(100% - 1.3em - 0.1em);
  background-position: 0;
}

.theme-checkbox:checked {
  background-position: 100%;
}
</style>
