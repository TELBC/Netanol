<template>
  <div class="naming-condition-box-container">
    <div class="naming-condition-box-menu">
      <div class="list-icons-container" v-bind:class="{'list-icons-container-editing': namingConditionBoxState.isEditing}">
        <font-awesome-icon icon="fa-solid fa-plus" class="conditions-list-icons" v-bind:class="{'conditions-list-editing': namingConditionBoxState.isEditing}" @click="toggleIsEditing('edit')" />
        <font-awesome-icon icon="fa-solid fa-minus" class="conditions-list-icons" v-bind:class="{'conditions-list-editing': namingConditionBoxState.isEditing}" @click="deleteSelectedNamingCondition" />
      </div>
      <div class="edit-icons-container" v-bind:class="{'editing-icons-container-editing': namingConditionBoxState.isEditing}">
        <font-awesome-icon icon="fa-solid fa-arrow-left" class="editing-condition-icons" v-bind:class="{'editing-condition': namingConditionBoxState.isEditing}" @click="clearEditingInputs('list')" />
        <font-awesome-icon icon="fa-solid fa-floppy-disk" class="editing-condition-icons" v-bind:class="{'editing-condition': namingConditionBoxState.isEditing}" @click="saveNamingCondition" />
      </div>
    </div>
    <div class="naming-condition-list" v-bind:class="{'editing-naming-condition-list': namingConditionBoxState.isEditing}">
      <div class="conditions" v-for="(condition, index) in namingConditionBoxState.namingConditions" :key="index" @click="setNamingConditionSelected(index)" v-bind:class="{'selected-naming-condition': namingConditionBoxState.namingConditionSelected == index}" @dblclick="doubleClickNamingCondition(index)">
        <div class="conditions-src-dest">
          <div class="src-dest-address-container">
            <p class="include-exclude-naming-src-dest">
              Name:&nbsp;
            </p>
            {{ condition.name }}
          </div>
          <div class="src-dest-address-container">
            <p class="include-exclude-naming-src-dest">
              IP:&nbsp;
            </p>
            {{ condition.matcher.address }}
          </div>
        </div>
        <p class="include-exclude-naming-src-dest">
          {{ condition.matcher.include ? 'Include' : 'Exclude' }}
        </p>
      </div>
    </div>
    <div class="naming-condition-editing" v-bind:class="{'editing-naming-condition-editing': namingConditionBoxState.isEditing}">
      <input id="source-input" class="scrollable-selector-input" type="text" placeholder="Name" v-model="namingConditionBoxState.editingCondition.name" />
      <input id="source-mask-input" class="scrollable-selector-input" type="text" placeholder="Address" v-model="namingConditionBoxState.editingCondition.matcher.address" />
      <input id="source-port-input" class="scrollable-selector-input" type="text" placeholder="Address Mask" v-model="namingConditionBoxState.editingCondition.matcher.mask" />
      <div class="scrollable-selector-include-exclude-traffic">
        <p>Exclude</p>
        <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" v-model="namingConditionBoxState.editingCondition.matcher.include" />
        <p>Include</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import {ref, defineProps, watch} from "vue";

const props = defineProps<{
  editLayerNamingConditions: Array<Matcher>,
}>();

interface Matcher {
  "name": string,
  "matcher": {
    "address": string
    "mask": string,
    "include": boolean
  },
  [key: string]: string | number | boolean | null | {}
}

const namingConditionBoxState = ref({
  isEditing: false,
  namingConditions: [] as Array<Matcher>,
  namingConditionSelected: -1,
  editingCondition: { name: "", matcher: { address: "", mask: "", include: false } },
  editingConditionIndex: -1,
})

function toggleIsEditing(to: string) {
  namingConditionBoxState.value.isEditing = to === 'edit';
  namingConditionBoxState.value.namingConditionSelected = -1;
}

function clearEditingInputs(to: string) {
  toggleIsEditing(to);
  Object.assign(namingConditionBoxState.value.editingCondition, {
    "name": "",
    "matcher": {
      "address": "",
      "mask": "",
      "include": false
    }
  });
}

function saveNamingCondition() {
  let newNamingCondition: Matcher = { ...namingConditionBoxState.value.editingCondition };
  const defaultValues: Matcher = {
    "name": "",
    "matcher": {
      "address": "",
      "mask": "",
      "include": false
    }
  };
  for (let key in defaultValues as {[key: string]: any}) {
    if (!newNamingCondition.hasOwnProperty(key) || newNamingCondition[key] === "" || newNamingCondition[key] === null) {
      newNamingCondition[key] = defaultValues[key];
    }
  }
  const isDuplicate = namingConditionBoxState.value.namingConditions.some(condition =>
    JSON.stringify(condition) === JSON.stringify(newNamingCondition)
  );
  if (!isDuplicate && namingConditionBoxState.value.editingConditionIndex === -1) {
    namingConditionBoxState.value.namingConditions.push(newNamingCondition);
  } else if (!isDuplicate && namingConditionBoxState.value.editingConditionIndex > -1 && namingConditionBoxState.value.editingConditionIndex < namingConditionBoxState.value.namingConditions.length) {
    Object.assign(namingConditionBoxState.value.namingConditions[namingConditionBoxState.value.editingConditionIndex], newNamingCondition);
  }
  clearEditingInputs('list');
  const matchers = namingConditionBoxState.value.namingConditions.map((condition) => {
    return {
      matcher: {
        address: condition.matcher.address,
        mask: condition.matcher.mask,
        include: condition.matcher.include
      },
      name: condition.name
    };
  });
  emit('update-naming-conditions', matchers);
}

// set which naming condition is highlighted by left click
function setNamingConditionSelected(condition: number) {
  namingConditionBoxState.value.namingConditionSelected = condition;
}

function deleteSelectedNamingCondition() {
  if (namingConditionBoxState.value.namingConditionSelected != -1) {
    namingConditionBoxState.value.namingConditions.splice(namingConditionBoxState.value.namingConditionSelected, 1);
    namingConditionBoxState.value.namingConditionSelected = -1;
  }
}

// open edit form for naming condition by double clicking it
function doubleClickNamingCondition(index: number) {
  toggleIsEditing('edit');
  namingConditionBoxState.value.editingCondition = { ...namingConditionBoxState.value.namingConditions[index] };
  namingConditionBoxState.value.editingConditionIndex = index;
}

const emit = defineEmits({
  'update-naming-conditions': (payload: Array<Matcher>) => true,
});

// receive naming conditions from LayerManagement on edit of existing layer
watch(() => props.editLayerNamingConditions, (newVal) => {
  namingConditionBoxState.value.namingConditions = newVal;
});
</script>

<style scoped>
.naming-condition-box-container {
  display: flex;
  flex-direction: column;
  border: 1px solid #424242;
  height: 45vh;
  width: 90%;
  border-radius: 4px;
  font-family: 'Open Sans', sans-serif;
  overflow: hidden;
}

.naming-condition-box-menu {
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

.naming-condition-list {
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

.selected-naming-condition {
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

.include-exclude-naming-src-dest {
  font-size: 1.5vh;
  font-weight: normal;
  margin-left: 0.5vw;
}

.naming-condition-editing {
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

.editing-naming-condition-list {
  width: 0;
  visibility: hidden;
  opacity: 0;
  height: 0;
}

.editing-naming-condition-editing {
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
