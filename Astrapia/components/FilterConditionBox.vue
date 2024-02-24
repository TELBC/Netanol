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
      <!--
      instead of dropdowns expanding and collapsing lists:
      eg Source Address (arrow down) and on click it expands to a list of available addresses and when one is selected it gets set instead of "Source Address"
      OWN COMPONENT?
      Search Bar for Source Addresses, styled to look like an expandable list, dropdownish, live searching
      -->
      <div class="scrollable-selector" v-bind:class="{'scrollable-selector-open': filterConditionBoxState.editSelectorOpen === 'source'}">
        <input id="source-input" class="scrollable-selector-input" type="text" placeholder="Source Address" v-model="filterConditionBoxState.editingCondition.sourceAddress" @focus="setOpenEditSelector('source')" />
        <div class="scrollable-selector-options">
          List of available addresses, scrollable
        </div>
      </div>
      <div class="scrollable-selector" v-bind:class="{'scrollable-selector-open': filterConditionBoxState.editSelectorOpen === 'sourceMask'}">
        <input id="source-mask-input" class="scrollable-selector-input" type="text" placeholder="Source Address Mask" v-model="filterConditionBoxState.editingCondition.sourceAddressMask" @focus="setOpenEditSelector('sourceMask')" />
        <div class="scrollable-selector-options">
          List of available addresses, scrollable
        </div>
      </div>
      <input id="source-port-input" class="filter-condition-editing-input" type="text" placeholder="Source Port" v-model="filterConditionBoxState.editingCondition.sourcePort" @focus="setOpenEditSelector('')" />
      <div class="scrollable-selector" v-bind:class="{'scrollable-selector-open': filterConditionBoxState.editSelectorOpen === 'destination'}">
        <input id="destination-input" class="scrollable-selector-input" type="text" placeholder="Destination Address" v-model="filterConditionBoxState.editingCondition.destinationAddress" @focus="setOpenEditSelector('destination')" />
        <div class="scrollable-selector-options">
          List of available addresses, scrollable
        </div>
      </div>
      <div class="scrollable-selector" v-bind:class="{'scrollable-selector-open': filterConditionBoxState.editSelectorOpen === 'destinationMask'}">
        <input id="destination-mask-input" class="scrollable-selector-input" type="text" placeholder="Destination Address Mask" v-model="filterConditionBoxState.editingCondition.destinationAddressMask" @focus="setOpenEditSelector('destinationMask')" />
        <div class="scrollable-selector-options">
          List of available addresses, scrollable
        </div>
      </div>
      <input id="destination-port-input" class="filter-condition-editing-input" type="text" placeholder="Destination Port" v-model="filterConditionBoxState.editingCondition.destinationPort" @focus="setOpenEditSelector('')" />
      <input id="protocol-input" class="filter-condition-editing-input" type="text" placeholder="Protocol" v-model="filterConditionBoxState.editingCondition.protocol" @focus="setOpenEditSelector('')" />
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
import {ref, inject, watch, Ref} from "vue";

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
  editSelectorOpen: '',
  filterConditionSelected: -1,
  editingCondition: {} as filterCondition,
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
    if (!newFilterCondition.hasOwnProperty(key)) {
      newFilterCondition[key] = defaultValues[key];
    }
  }
  const isDuplicate = filterConditionBoxState.value.filterConditions.some(condition =>
    JSON.stringify(condition) === JSON.stringify(newFilterCondition)
  );
  if (!isDuplicate) {
    filterConditionBoxState.value.filterConditions.push(newFilterCondition);
    clearEditingInputs('list');
    console.log(newFilterCondition)
  } else {
    // add error messages to the UI
  }
}

function setOpenEditSelector(selector: string) {
  filterConditionBoxState.value.editSelectorOpen = selector;
}

function setFilterConditionSelected(condition: number) {
  filterConditionBoxState.value.filterConditionSelected = condition;
}

function deleteSelectedFilterCondition() {
  if (filterConditionBoxState.value.filterConditionSelected != -1) {
    filterConditionBoxState.value.filterConditions.splice(filterConditionBoxState.value.filterConditionSelected, 1);
    filterConditionBoxState.value.filterConditionSelected = -1;
  }
}

function doubleClickFilterCondition(index: number) {
  toggleIsEditing('edit');
  filterConditionBoxState.value.editingCondition = { ...filterConditionBoxState.value.filterConditions[index] };
}

const emitUpdateFilterConditions = inject('emitUpdateFilterConditions') as Ref<boolean>;
const emit = defineEmits(['update-filter-conditions'])

watch(emitUpdateFilterConditions, (newValue) => {
  if (newValue) {
    emit('update-filter-conditions', filterConditionBoxState.value.filterConditions);
  }
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

.scrollable-selector {
  display: flex;
  flex-direction: column;
  width: 95%;
  padding: 0.25vh 0;
  margin: 0.5vh 0;
}

.scrollable-selector-open > .scrollable-selector-options {
  visibility: visible;
  opacity: 1;
  height: 10vh;
}

.scrollable-selector-input {
  border: none;
  font-size: 2vh;
  border-bottom: 1px solid #e0e0e0;
}

.scrollable-selector-input:focus {
  outline: none;
}

.scrollable-selector-options {
  visibility: hidden;
  opacity: 0;
  height: 0;
  transition: 0.2s ease-in-out;
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
</style>
