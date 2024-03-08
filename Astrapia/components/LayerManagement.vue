<template>
  <div class="layer-list-overflow-container" v-bind:class="{ 'create-open-hide': layerListState.createLayerOpen }">
    <div class="layer-container" v-for="(layer, index) in layerListState.selectedLayout.layers" :key="rerenderer.layerListRerender" @click="openCloseLayer(index)">
      <div class="layer">
        <div>
          <font-awesome-icon icon="fa-solid fa-chevron-right" v-bind:class="{'expand-layer-icon': layerListState.layerOpen === index}" class="expand-layer" />
        </div>
        <div class="layer-name">
          {{ layer.name }}
        </div>
        <input type="checkbox" :checked="layer.enabled" @change="setLayerEnabled(index, $event.target.checked)" class="theme-checkbox" @click.stop>
      </div>
      <div class="layer-info" v-bind:class="{ 'expand-layer-info': layerListState.layerOpen === index }">
        <div class="update-delete-layer">
          <font-awesome-icon icon="fa-solid fa-pen" class="edit-layer upd-del-layer-icon" @click="toggleEditExistingLayer(index)" />
          <font-awesome-icon icon="fa-solid fa-trash" class="upd-del-layer-icon" @click="deleteLayerFromLayout(index)" />
        </div>
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
    <select class="create-dropdown-inputs" required v-model="createLayerData.type">
      <option value="" disabled selected hidden>Select Layer Type</option>
      <option value="filter">Filter</option>
      <option value="aggregation">Grouping</option>
      <option value="tag-filter">Tag Filter</option>
      <option value="vmware-tagging">Tag VMware</option>
      <option value="naming">Naming</option>
      <option value="styling">Styling</option>
    </select>
    <div class="enable-new-layer" v-if="createLayerData.type !==''">
      <p>Enable on creation</p>
      <input type="checkbox" class="theme-checkbox" v-model="createLayerData.enabled" />
    </div>
    <div class="filter-condition" v-if="createLayerData.type === 'filter'">
      <FilterConditionBox :edit-layer-filter-conditions="createLayerData.filterList.conditions" @update-filter-conditions="handleFilterConditionsEmit" />
      <div class="enable-new-layer" >
        <p>Implicit Inclusion</p>
        <input type="checkbox" class="theme-checkbox" v-model="createLayerData.filterList.implicitInclude" />
      </div>
    </div>
    <div class="filter-condition" v-if="createLayerData.type === 'aggregation'">
      <AggregationConditionBox :edit-layer-aggregation-matchers="createLayerData.matchers" @update-aggregation-matchers="handleFilterConditionsEmit"  />
    </div>
    <div class="filter-condition" v-if="createLayerData.type === 'tag-filter'">
      <TagConditionBox :edit-layer-tag-conditions="createLayerData.conditions" @update-tag-conditions="handleFilterConditionsEmit" />
      <div class="enable-new-layer" >
        <p>Implicit Inclusion</p>
        <input type="checkbox" class="theme-checkbox" v-model="createLayerData.implicitInclude" />
      </div>
    </div>
    <div class="filter-condition" v-if="createLayerData.type === 'naming'">
      <NamingConditionBox :edit-layer-naming-conditions="createLayerData.matchers" @update-naming-conditions="handleFilterConditionsEmit" />
      <div class="enable-new-layer" >
        <p>Overwrite with DNS</p>
        <input type="checkbox" class="theme-checkbox" v-model="createLayerData.overwriteWithDns" />
      </div>
    </div>
    <div class="filter-condition" v-if="createLayerData.type === 'styling'">
      <StyleConditionBox :edit-layer-edge-conditions="createLayerData.edgeStyler" :edit-layer-node-conditions="createLayerData.nodeStyler" @update-styling-conditions="handleFilterConditionsEmit" />
    </div>
  </div>
  <div class="create-layer" @click="toggleCreateLayerOpen">
    <font-awesome-icon icon="fa-solid fa-plus" v-if="layerListState.isEditExistingLayer === -1" class="fa-plus" :class="{ 'rotate-icon': layerListState.createLayerOpen }" />
    <font-awesome-icon icon="fa-solid fa-pen" v-if="layerListState.isEditExistingLayer > -1" class="fa-create-layer-pen" />
  </div>
</template>

<script setup lang="ts">
import {ref, watch, inject} from "vue";
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import layerService from "~/services/layerService";
import FilterConditionBox from "~/components/conditions/FilterConditionBox.vue";
import AggregationConditionBox from "~/components/conditions/AggregationConditionBox.vue";
import TagConditionBox from "~/components/conditions/TagConditionBox.vue";
import NamingConditionBox from "~/components/conditions/NamingConditionBox.vue";
import StyleConditionBox from "~/components/conditions/StyleConditionBox.vue";

export interface FilterConditions {
  sourceAddress: string,
  sourceAddressMask: string,
  sourcePort: number,
  destinationAddress: string,
  destinationAddressMask: string,
  destinationPort: number,
  protocol: string,
  include: boolean
}

export interface Layer {
  name: string,
  type: string,
  enabled: boolean,
  overwriteWithDns: boolean,
  filterList: {
    conditions: FilterConditions[],
    implicitInclude: boolean
  }
}

interface Layers {
  name: string;
  type: string;
  enabled: boolean;
  description: string;
}

const props = defineProps<{
  layout: string;
  layers: Array<Layers>;
}>();

// holds the data of the layer that is being created or edited
const createLayerData = ref({
  name: '',
  type: '',
  enabled: false,
  overwriteWithDns: true,
  filterList: {
    conditions: [] as Array<FilterConditions>,
    implicitInclude: true
  },
  implicitInclude: true,
  conditions: [],
  matchers:[],
  edgeStyler:{},
  nodeStyler:{}
})

const layerListState = ref({
  isExpanded: false,
  selectedLayout: {
    name: '',
    layers: [] as Array<Layers>,
  },
  layerOpen: -1,
  createLayerOpen: false,
  isEditExistingLayer: -1,
  doneEmittingFilterConditions: false,
});

// used to rerender the layer list when it is modified
const rerenderer = ref({
  layerListRerender: 0
})

const getLayersOfLayout = inject<() => void>('getLayersOfLayout');

async function getExistingLayer(index: number) {
  const layerToEdit = await layerService.getLayer(layerListState.value.selectedLayout.name, index);
  Object.assign(createLayerData.value, layerToEdit);
}

async function editExistingLayer(index: number) {
  await layerService.editLayer(layerListState.value.selectedLayout.name, index, createLayerData.value);
  getLayersOfLayout!();
}

async function createNewLayer(layer: Layer) {
  const response = await layerService.createLayer(layer, layerListState.value.selectedLayout.name);
  getLayersOfLayout!();
  return response;
}

function openCloseLayer(index: number) {
  if (layerListState.value.layerOpen === index) {
    layerListState.value.layerOpen = -1;
  } else {
    layerListState.value.layerOpen = index;
  }
}

function setLayerEnabled(index: number, value: boolean) {
  const layer = layerListState.value.selectedLayout.layers[index];
  layer.enabled = value;
  Object.assign(createLayerData.value, layer);
  setTimeout(() => {
    editExistingLayer(index);
    Object.assign(createLayerData.value, {
      name: '',
      type: '',
      enabled: false,
      overwriteWithDns: true,
      filterList: {
        conditions: [] as Array<FilterConditions>,
        implicitInclude: true
      },
      implicitInclude: true,
      conditions: [],
      matchers:[],
      edgeStyler:{},
      nodeStyler:{}
    })
  }, 250);
}

// handle emitted filter conditions from FilterConditionBox
function handleFilterConditionsEmit(newConditions: any, doneEmitting: boolean) {
  if(createLayerData.value.type === 'filter'){
    createLayerData.value.filterList.conditions = newConditions;
  }else if(createLayerData.value.type === 'aggregation'){
    createLayerData.value.matchers = newConditions;
  }else if(createLayerData.value.type === 'tag-filter'){
    createLayerData.value.conditions = newConditions;
  }else if(createLayerData.value.type === 'naming'){
    createLayerData.value.matchers = newConditions;
  }else if(createLayerData.value.type === 'styling'){
    delete (createLayerData.value as { matchers?: any }).matchers;
    createLayerData.value.edgeStyler = newConditions.edgeStyler;
    createLayerData.value.nodeStyler = newConditions.nodeStyler;
  }
  layerListState.value.doneEmittingFilterConditions = doneEmitting;
}

// opens or closes create/edit dialog and on close calls either createNewLayerAndReset or editExistingLayerAndReset
function toggleCreateLayerOpen() {
  if (layerListState.value.createLayerOpen) {
    layerListState.value.createLayerOpen = false;
    if (layerListState.value.isEditExistingLayer > -1) {
      editExistingLayerAndReset();
    }
    else {
      createNewLayerAndReset();
    }
  }
  else {
    layerListState.value.createLayerOpen = true;
  }
  Object.assign(createLayerData.value, {
    name: '',
    type: '',
    enabled: false,
    overwriteWithDns: true,
    filterList: {
      conditions: [] as Array<FilterConditions>,
      implicitInclude: true
    },
    implicitInclude: true,
    conditions: [],
    matchers:[],
    edgeStyler:{},
    nodeStyler:{}
  })
}

function toggleEditExistingLayer(index: number) {
  toggleCreateLayerOpen();
  getExistingLayer(index);
  layerListState.value.isEditExistingLayer = index;
}

function editExistingLayerAndReset() {
  editExistingLayer(layerListState.value.isEditExistingLayer);
  layerListState.value.isEditExistingLayer = -1;
  Object.assign(createLayerData.value, {
    name: '',
    type: '',
    enabled: false,
    overwriteWithDns: true,
    filterList: {
      conditions: [] as Array<FilterConditions>,
      implicitInclude: true
    },
    implicitInclude: true,
    conditions: [],
    matchers:[],
    edgeStyler:{},
    nodeStyler:{}
  });
}

function createNewLayerAndReset() {
  const layer = createLayerData.value;
  createNewLayer(layer);
  Object.assign(createLayerData.value, {
    name: '',
    type: '',
    enabled: false,
    overwriteWithDns: true,
    filterList: {
      conditions: [] as Array<FilterConditions>,
      implicitInclude: true
    },
    implicitInclude: true,
    conditions: [],
    matchers:[],
    edgeStyler:{},
    nodeStyler:{}
  });
}

async function deleteLayerFromLayout(index: number) {
  const response = await layerService.deleteLayer(layerListState.value.selectedLayout.name, index);
  if (response.status === 200) {
    getLayersOfLayout!();
  }
}

watch(() => props.layout!, (newLayout, oldLayout) => {
  layerListState.value.selectedLayout.name = newLayout;
});

watch(() => props.layers!, (newLayers, oldLayers) => {
  layerListState.value.selectedLayout.layers = newLayers;
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
  display: flex;
  flex-direction: column;
}

.expand-layer-info {
  display: flex;
  flex-direction: column;
  height: auto;
  visibility: visible;
  opacity: 1;
  padding: 0 1vw 2.5vh 1vw;
}

.update-delete-layer {
  margin: 0;
  display: flex;
  flex-direction: row;

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
  overflow-y: auto;
  overflow-x: hidden;
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

.create-dropdown-inputs {
  width: 91%;
  border: 1px solid #424242;
  background: white;
  border-radius: 4px;
  font-size: 2vh;
  margin-bottom: 1.5vh;
  padding: 2%;
  outline: none;
  color: #424242;
  font-family: 'Open Sans', sans-serif;
}

.create-dropdown-inputs:focus {
  outline: none;
}

.filter-condition{
  margin-left: 10%;
  width: 100%;
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

.fa-create-layer-pen {
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

.theme-checkbox:checked::before, .create-form:deep(.include-exclude-traffic-switch):checked::before {
  left: calc(100% - 1.3em - 0.1em);
  background-position: 0;
}

.theme-checkbox:checked, .create-form:deep(.include-exclude-traffic-switch):checked {
  background-position: 100%;
}

.infos {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  height: auto;
  margin-bottom: 0.5vh;
}

.infos > p {
  font-weight: bolder;
  margin: 0 0.5vw 0 0;
}

.infos > div {
  display: inline-flex;
  word-wrap: anywhere;
}
</style>
