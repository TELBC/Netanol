<template>
  <div class="filter-condition-box-container">
    <div class="filter-condition-box-menu">
      <div class="list-icons-container" v-bind:class="{'list-icons-container-editing': filterConditionBoxState.isEditing}">
        <font-awesome-icon icon="fa-solid fa-plus" class="conditions-list-icons" v-bind:class="{'conditions-list-editing': filterConditionBoxState.isEditing}" @click="toggleIsEditing('edit')" />
        <font-awesome-icon icon="fa-solid fa-minus" class="conditions-list-icons" v-bind:class="{'conditions-list-editing': filterConditionBoxState.isEditing}" @click="deleteSelectedFilterCondition" />
      </div>
      <div class="edit-icons-container" v-bind:class="{'editing-icons-container-editing': filterConditionBoxState.isEditing}">
        <font-awesome-icon icon="fa-solid fa-arrow-left" class="editing-condition-icons" v-bind:class="{'editing-condition': filterConditionBoxState.isEditing}" @click="clearEditingInputs('list')" />
        <font-awesome-icon icon="fa-solid fa-floppy-disk" class="editing-condition-icons" v-bind:class="{'editing-condition': filterConditionBoxState.isEditing}" @click="saveFilterCondition" />
      </div>
    </div>
    <div class="filter-condition-list" v-bind:class="{'editing-filter-condition-list': filterConditionBoxState.isEditing}">
      <div class="conditions" v-for="(condition, index) in filterConditionBoxState.filterConditions" :key="index" @click="setFilterConditionSelected(index)" v-bind:class="{'selected-filter-condition': filterConditionBoxState.filterConditionSelected == index}" @dblclick="doubleClickFilterCondition(index)">
        <div class="conditions-src-dest">
          <div class="src-dest-address-container">
            <p class="include-exclude-filter-src-dest">
              Src:&nbsp;
            </p>
            {{ condition.sourceAddress }}
          </div>
          <div class="src-dest-address-container">
            <p class="include-exclude-filter-src-dest">
              Dest:&nbsp;
            </p>
            {{ condition.destinationAddress }}
          </div>
        </div>
        <p class="include-exclude-filter-src-dest">
          {{ condition.include ? 'Include' : 'Exclude' }}
        </p>
      </div>
    </div>
    <div class="filter-condition-editing" v-bind:class="{'editing-filter-condition-editing': filterConditionBoxState.isEditing}">
      <input id="source-input" class="scrollable-selector-input" type="text" placeholder="Source Address" v-model="filterConditionBoxState.editingCondition.sourceAddress" />
      <input id="source-mask-input" class="scrollable-selector-input" type="text" placeholder="Source Address Mask" v-model="filterConditionBoxState.editingCondition.sourceAddressMask" />
      <input id="source-port-input" class="filter-condition-editing-input" type="text" placeholder="Source Port" v-model="filterConditionBoxState.editingCondition.sourcePort" />
      <input id="destination-input" class="scrollable-selector-input" type="text" placeholder="Destination Address" v-model="filterConditionBoxState.editingCondition.destinationAddress" />
      <input id="destination-mask-input" class="scrollable-selector-input" type="text" placeholder="Destination Address Mask" v-model="filterConditionBoxState.editingCondition.destinationAddressMask" />
      <input id="destination-port-input" class="filter-condition-editing-input" type="text" placeholder="Destination Port" v-model="filterConditionBoxState.editingCondition.destinationPort" />
      <input id="protocol-input" class="scrollable-selector-input" type="text" placeholder="Protocol" v-model="filterConditionBoxState.editingCondition.protocol" />
      <div class="scrollable-selector-include-exclude-traffic">
        <p>Exclude</p>
        <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" v-model="filterConditionBoxState.editingCondition.include" />
        <p>Include</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import {ref, defineProps, watch} from "vue";

const props = defineProps<{
  emitFilterConditions: Boolean,
  editLayerFilterConditions: Array<filterCondition>,
}>();

interface filterCondition {
  "sourceAddress": string
  "sourceAddressMask": string,
  "sourcePort": number | null,
  "destinationAddress": string,
  "destinationAddressMask": string,
  "destinationPort": number | null,
  "protocol": string,
  "include": boolean,
  [key: string]: string | number | boolean | null
}

const filterConditionBoxState = ref({
  isEditing: false,
  filterConditions: [] as Array<filterCondition>,
  ipAddresses: [] as Array<string>,
  addressMasks: [] as Array<string>,
  ports: [] as Array<number>,
  protocols: [] as Array<string>,
  filterConditionSelected: -1,
  editingCondition: {} as filterCondition,
  editingConditionIndex: -1,
})

function toggleIsEditing(to: string) {
  filterConditionBoxState.value.isEditing = to === 'edit';
  filterConditionBoxState.value.filterConditionSelected = -1;
}

function clearEditingInputs(to: string) {
  toggleIsEditing(to);
  Object.assign(filterConditionBoxState.value.editingCondition, {
    "sourceAddress": "",
    "sourceAddressMask": "",
    "sourcePort": null,
    "destinationAddress": "",
    "destinationAddressMask": "",
    "destinationPort": null,
    "protocol": "",
    "include": true
  });
}

function saveFilterCondition() {
  let newFilterCondition: filterCondition = { ...filterConditionBoxState.value.editingCondition };
  const defaultValues: filterCondition = {
    "sourceAddress": "0.0.0.0",
    "sourceAddressMask": "0.0.0.0",
    "sourcePort": null,
    "destinationAddress": "0.0.0.0",
    "destinationAddressMask": "0.0.0.0",
    "destinationPort": null,
    "protocol": "tcp",
    "include": false
  };
  for (let key in defaultValues as {[key: string]: any}) {
    if (!newFilterCondition.hasOwnProperty(key) || newFilterCondition[key] === "" || newFilterCondition[key] === null) {
      newFilterCondition[key] = defaultValues[key];
    }
  }
  const isDuplicate = filterConditionBoxState.value.filterConditions.some(condition =>
    JSON.stringify(condition) === JSON.stringify(newFilterCondition)
  );
  if (!isDuplicate && filterConditionBoxState.value.editingConditionIndex === -1) {
    filterConditionBoxState.value.filterConditions.push(newFilterCondition);
    clearEditingInputs('list');
  } else if (!isDuplicate && filterConditionBoxState.value.editingConditionIndex > -1 && filterConditionBoxState.value.editingConditionIndex < filterConditionBoxState.value.filterConditions.length) {
    Object.assign(filterConditionBoxState.value.filterConditions[filterConditionBoxState.value.editingConditionIndex], newFilterCondition);
    clearEditingInputs('list');
  }
}

// set which filter condition is highlighted by left click
function setFilterConditionSelected(condition: number) {
  filterConditionBoxState.value.filterConditionSelected = condition;
}

function deleteSelectedFilterCondition() {
  if (filterConditionBoxState.value.filterConditionSelected != -1) {
    filterConditionBoxState.value.filterConditions.splice(filterConditionBoxState.value.filterConditionSelected, 1);
    filterConditionBoxState.value.filterConditionSelected = -1;
  }
}

// open edit form for filter condition by double clicking it
function doubleClickFilterCondition(index: number) {
  toggleIsEditing('edit');
  filterConditionBoxState.value.editingCondition = { ...filterConditionBoxState.value.filterConditions[index] };
  filterConditionBoxState.value.editingConditionIndex = index;
}

const emit = defineEmits({
  'update-filter-conditions': (payload: { filterConditions: Array<filterCondition>, done: boolean }) => true,
});

// emit filter conditions to LayerManagement on receiving emitFilterConditions prop
watch(() => props.emitFilterConditions, (newVal) => {
  if (newVal === true) {
    emit('update-filter-conditions', {
      filterConditions: filterConditionBoxState.value.filterConditions,
      done: true
    });
  } else {
    filterConditionBoxState.value.filterConditions = [];
  }
});

// receive filter conditions from LayerManagement on edit of existing layer
watch(() => props.editLayerFilterConditions, (newVal) => {
  filterConditionBoxState.value.filterConditions = newVal;
});
</script>

<style scoped>
.filter-condition-box-container {
  display: flex;
  flex-direction: column;
  border: 1px solid #424242;
  height: 45vh;
  width: 90%;
  border-radius: 4px;
  font-family: 'Open Sans', sans-serif;
  overflow: hidden;
}

.filter-condition-box-menu {
  display: flex;
  align-items: center;
  width: 90%;
  height: 2vh;
  border-bottom: 1px solid #424242;
  padding: 0.5vh 5%;
  background-color: #e0e0e0;
}

.list-icons-container {
  display: flex;
  align-items: center;
  justify-items: flex-start;
  width: 100%;
  transition: 0.2s ease-in-out;
}

.list-icons-container-editing {
  width: 0;
}

.edit-icons-container {
  display: flex;
  align-items: center;
  justify-items: flex-start;
  width: 0;
  transition: 0.2s ease-in-out;
}

.editing-icons-container-editing {
  width: 100%;
}

.conditions-list-icons {
  margin-right: 0.5vw;
  cursor: pointer;
  transition: 0.2s ease-in-out;
}

.conditions-list-editing {
  visibility: hidden;
  opacity: 0;
}

.editing-condition-icons {
  margin-right: 0.5vw;
  visibility: hidden;
  opacity: 0;
  cursor: pointer;
  transition: 0.2s ease-in-out;
}

.editing-condition {
  visibility: visible;
  opacity: 1;
}

.filter-condition-list {
  width: 100%;
  height: 100%;
  visibility: visible;
  opacity: 1;
  transition: 0.2s ease-in-out;
  overflow-y: auto;
  overflow-x: hidden;
  word-break: break-word;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.conditions {
  width: 90%;
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  font-size: 2vh;
  font-weight: bolder;
  cursor: pointer;
  height: 4.9vh;
  transition: 0.2s ease-in-out;
  padding: 2.5% 5% 2.5% 5%;
}

.selected-filter-condition {
  background-color: #e0e0e0;
}

.conditions-src-dest {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.src-dest-address-container {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: flex-start;
  margin-left: -0.5vw;
  height: 45%;
}

.include-exclude-filter-src-dest {
  font-size: 1.5vh;
  font-weight: normal;
  margin-left: 0.5vw;
}

.filter-condition-editing {
  width: 0;
  height: 0;
  visibility: hidden;
  opacity: 0;
  transition: 0.2s ease-in-out;
  overflow-y: auto;
  overflow-x: hidden;
  word-break: break-word;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.editing-filter-condition-list {
  width: 0;
  visibility: hidden;
  opacity: 0;
  height: 0;
}

.editing-filter-condition-editing {
  width: 100%;
  height: 100%;
  visibility: visible;
  opacity: 1;
}

.scrollable-selector-input {
  border: none;
  width: 95%;
  font-size: 2vh;
  border-bottom: 1px solid #e0e0e0;
  padding: 0.25vh 0;
  margin: 0.5vh 2.5%;
}

.scrollable-selector-input:focus {
  outline: none;
}

.filter-condition-editing-input {
  border: 1px solid #424242;
  border-radius: 4px;
  font-size: 2vh;
  width: 90%;
  padding: 2%;
  margin: 0.5vh 0;
}

.filter-condition-editing-input:focus {
  outline: none;
}

.scrollable-selector-include-exclude-traffic {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  width: 90%;
  font-size: 2vh;
}

#exclude-include-switch {
  margin-left: 0;
}
</style>
