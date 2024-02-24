<template>
  <div class="layer-list-overflow-container" v-bind:class="{ 'create-open-hide': layerListState.createLayerOpen }">
    <div class="layer-container" v-for="(layer, index) in layerListState.selectedLayout.layers" :key="rerenderer.layerListRerender" @click="openCloseLayer(layer.name)">
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
          <font-awesome-icon icon="fa-solid fa-pen" class="edit-layer upd-del-layer-icon" @click="toggleEditExistingLayer(index)" />
          <font-awesome-icon icon="fa-solid fa-trash" class="upd-del-layer-icon" @click="deleteLayerFromLayout(index)" />
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
    <input class="create-inputs" type="text" placeholder="Layer Name" v-model="createLayerData.name" />
    <input class="create-inputs" type="text" placeholder="Layer Type" v-model="createLayerData.type" />
    <div class="enable-new-layer">
      <p>Enable?</p>
      <input type="checkbox" class="theme-checkbox" v-model="createLayerData.enabled" />
    </div>
    <FilterConditionBox :emit-filter-conditions="layerListState.emitFilterConditions" @update-filter-conditions="handleFilterConditionsEmit" />
  </div>
  <div class="create-layer" @click="toggleCreateLayerOpen">
    <font-awesome-icon icon="fa-solid fa-plus" class="fa-plus" :class="{ 'rotate-icon': layerListState.createLayerOpen }" />
  </div>
</template>

<script setup lang="ts">
import {onMounted, ref, watch, inject} from "vue";
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import FilterConditionBox from "~/components/FilterConditionBox.vue";
import layerService from "~/services/layerService";
import {FilterConditions} from "~/services/layerService";

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

const createLayerData = ref({
  name: '',
  type: '',
  enabled: false,
  filterList: {
    conditions: [] as Array<FilterConditions>,
    implicitInclude: false
  }
})

const layerListState = ref({
  isExpanded: false,
  selectedLayout: {
    name: '',
    layers: [] as Array<Layer>,
  },
  layerOpen: '',
  createLayerOpen: false,
  emitFilterConditions: false,
});

const rerenderer = ref({
  layerListRerender: 0
})

const getLayersOfLayout = inject<() => void>('getLayersOfLayout');

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

function handleFilterConditionsEmit(newConditions: Array<FilterConditions>) {
  createLayerData.value.filterList.conditions = newConditions;
}

function toggleCreateLayerOpen() {
  if (layerListState.value.createLayerOpen) {
    layerListState.value.emitFilterConditions = true;
    console.log("i am here")
  } else {
    layerListState.value.createLayerOpen = true;
  }
}

watch(() => createLayerData.value.filterList.conditions, async (newConditions) => {
  if (newConditions.length > 0 && layerListState.value.emitFilterConditions) {
    try {
      const layer = createLayerData.value;
      const response = await layerService.createLayer(layer, layerListState.value.selectedLayout.name);
      if (response.status === 200) {
        layerListState.value.createLayerOpen = false;
        getLayersOfLayout!();
        Object.assign(createLayerData.value, {
          name: '',
          type: '',
          enabled: false,
          filterList: {
            conditions: [] as Array<FilterConditions>,
            implicitInclude: false
          }
        });
        layerListState.value.emitFilterConditions = false;
      } else {
        console.log(response.status)
      }
    } catch (error) {
      // add error messages to UI
    }
  }
});

function toggleEditExistingLayer(index: number) {
  console.log(layerListState.value.selectedLayout.layers[index])
}

async function deleteLayerFromLayout(index: number) {
  try {
    const response = await layerService.deleteLayer(layerListState.value.selectedLayout.name, index);
    if (response.status === 200) {
      getLayersOfLayout!();
    }
  } catch (error) {
    console.error('Error:', error);
  }
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

.upd-del-layer-icon {
  cursor: pointer;
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
  background-color: white;
}

.create-form {
  transition: 0.2s ease-in-out;
  visibility: hidden;
  opacity: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  height: 94.5%;
}

.create-inputs {
  width: 86%;
  border: 1px solid #424242;
  border-radius: 4px;
  font-size: 2vh;
  margin-bottom: 1.5vh;
  padding: 2%;
}

.create-inputs:focus {
  outline: none;
}

.enable-new-layer {
  display: flex;
  flex-direction: row;
  align-items: center;
  width: 90%;
  justify-content: space-between;
}

.create-layer-open {
  padding-top: 1vh;
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

.theme-checkbox, .create-form:deep(.include-exclude-traffic-switch) {
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

.theme-checkbox::before, .create-form:deep(.include-exclude-traffic-switch)::before {
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
