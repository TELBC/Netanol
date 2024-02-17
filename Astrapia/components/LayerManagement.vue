<template>
  <div class="layer-list-overflow-container" v-bind:class="{ 'create-open-hide': layerListState.createLayerOpen }">
    <div class="layer-container" v-for="layer in layerListState.selectedLayout.layers" :key="rerenderer.layerListRerender" @click="openCloseLayer(layer.name)">
      <div class="layer">
        <div>
          <font-awesome-icon icon="fa-solid fa-chevron-right" v-bind:class="{'expand-layer-icon': layerListState.layerOpen === layer.name}" class="expand-layer" />
        </div>
        <div class="layer-name">
          {{ layer.name }}
        </div>
        <input type="checkbox" :checked="getLayerEnabled(layer.name)" @change="setLayerEnabled(layer.name, $event.target.checked)" class="theme-checkbox" @click.stop>
      </div>
      <div class="layer-info" v-bind:class="{ 'expand-layer-info': layerListState.layerOpen === layer.name }">
        <div class="update-delete-layer">
          <font-awesome-icon icon="fa-solid fa-pen" class="edit-layer" />
          <font-awesome-icon icon="fa-solid fa-trash" />
        </div>
        <!-- fix p and layer.type and layer.description not aligned flex start -->
        <div class="infos">
          <p>type: </p>
          {{ layer.type }}
        </div>
        <div class="infos">
          <p>description: </p>
          <div>{{ layer.description }}</div>
        </div>
      </div>
    </div>
  </div>
  <div class="create-form" v-bind:class="{ 'create-layer-open': layerListState.createLayerOpen }">
    <input type="text" placeholder="Layer Name" />
    <input type="text" placeholder="Layer Type" />
    <div class="enable-new-layer">
      <p>Enable?</p>
      <input type="checkbox" class="theme-checkbox" />
    </div>
    <FilterConditionBox />
  </div>
  <div class="create-layer" @click="layerListState.createLayerOpen = !layerListState.createLayerOpen">
    <font-awesome-icon icon="fa-solid fa-plus" class="fa-plus" :class="{ 'rotate-icon': layerListState.createLayerOpen }" />
  </div>
</template>

<script setup lang="ts">
import {onMounted, ref, watch} from "vue";
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import FilterConditionBox from "~/components/FilterConditionBox.vue";

// put into other file!!
interface Layer {
  name: string;
  type: string;
  enabled: boolean;
  description: string;
}

const props = defineProps<{
  layout: string;
  layers: Array<Layer>;
}>();

const layerListState = ref({
  isExpanded: false,
  selectedLayout: {
    name: '',
    layers: [] as Array<Layer>,
  },
  layerOpen: '',
  createLayerOpen: false,
});

const rerenderer = ref({
  layerListRerender: 0
})

function openCloseLayer(layerName: string) {
  if (layerListState.value.layerOpen === layerName) {
    layerListState.value.layerOpen = '';
  } else {
    layerListState.value.layerOpen = layerName;
  }
}

function setLayerEnabled(layerName: string, value: boolean) {
  const layer = layerListState.value.selectedLayout.layers.find((layer) => layer.name === layerName);
  if (layer) {
    layer.enabled = value;
  }
}

function getLayerEnabled(layerName: string) {
  const layer = layerListState.value.selectedLayout.layers.find((layer) => layer.name === layerName);
  return layer ? layer.enabled : false;
}

watch(() => props.layout!, (newLayout, oldLayout) => {
  layerListState.value.selectedLayout.name = newLayout;
});

watch(() => props.layers!, (newLayers, oldLayers) => {
  layerListState.value.selectedLayout.layers = newLayers;
  rerenderer.value.layerListRerender += 1;
});

onMounted(() => {
  layerListState.value.selectedLayout.layers = props.layers!;
  rerenderer.value.layerListRerender += 1;
});
</script>

<style scoped>
.layer-list-overflow-container {
  overflow-y: auto;
  overflow-x: hidden;
  height: 94.5%;
  transition: 0.2s ease-in-out;
}

.layer-container {
  display: flex;
  flex-direction: column;
}

.layer {
  display: flex;
  flex-direction: row;
  align-items: center;
  padding: 1vh 1vw;
  cursor: pointer;
  transition: 0.2s ease-in-out;
  width: 90%;
}

.expand-layer {
  transition: transform 0.2s ease-in-out;
  margin-right: 0.5vw;
}

.expand-layer-icon {
  transform: rotate(-90deg);
}

.layer-name {
  width: 60%;
  overflow-x: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.layer-name:hover {
  word-wrap: break-word;
  white-space: normal;
}

.layer-info {
  visibility: hidden;
  opacity: 0;
  height: 0;
  transition: 0.2s ease-in-out;
  display: flex;
  flex-direction: column;
}

.expand-layer-info {
  display: flex;
  flex-direction: column;
  min-height: 10vh;
  visibility: visible;
  opacity: 1;
  padding: 0 1vw 2.5vh 1vw;
}

.update-delete-layer {
  margin: 0;
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: flex-end;
  color: #7EA0A9;
}

.edit-layer {
  margin-right: 0.5vw;
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

.enable-new-layer {
  display: flex;
  flex-direction: row;
  align-items: center;
  width: 90%;
  justify-content: space-between;
}

.create-layer-open {
  display: flex;
  flex-direction: column;
  align-items: center;
  height: 94.5%;
  visibility: visible;
  opacity: 1;
}

.create-open-hide {
  visibility: hidden;
  opacity: 0;
  height: 0;
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

.infos {
  display: flex;
  flex-direction: row;
  align-items: flex-start;
  min-height: 1vh;
}

.infos p {
  font-weight: bolder;
  margin-right: 0.5vw;
}

.infos div {
  display: inline-flex;
  word-wrap: anywhere;
}
</style>
