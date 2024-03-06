<template>
  <div class="tag-condition-box-container">
    <div class="tag-condition-box-menu">
      <div class="list-icons-container" v-bind:class="{'list-icons-container-editing': tagConditionBoxState.isEditing}">
        <font-awesome-icon icon="fa-solid fa-plus" class="conditions-list-icons" v-bind:class="{'conditions-list-editing': tagConditionBoxState.isEditing}" @click="toggleIsEditing('edit')" />
        <font-awesome-icon icon="fa-solid fa-minus" class="conditions-list-icons" v-bind:class="{'conditions-list-editing': tagConditionBoxState.isEditing}" @click="deleteSelectedTagCondition" />
      </div>
      <div class="edit-icons-container" v-bind:class="{'editing-icons-container-editing': tagConditionBoxState.isEditing}">
        <font-awesome-icon icon="fa-solid fa-arrow-left" class="editing-condition-icons" v-bind:class="{'editing-condition': tagConditionBoxState.isEditing}" @click="clearEditingInputs('list')" />
        <font-awesome-icon icon="fa-solid fa-floppy-disk" class="editing-condition-icons" v-bind:class="{'editing-condition': tagConditionBoxState.isEditing}" @click="saveTagCondition" />
      </div>
    </div>
    <div class="tag-condition-list" v-bind:class="{'editing-tag-condition-list': tagConditionBoxState.isEditing}">
      <div class="conditions" v-for="(condition, index) in tagConditionBoxState.tagConditions" :key="index" @click="setTagConditionSelected(index)" v-bind:class="{'selected-tag-condition': tagConditionBoxState.tagConditionSelected == index}" @dblclick="doubleClickTagCondition(index)">
        <div class="conditions-src-dest">
          <div class="src-dest-address-container">
            <p class="include-exclude-tag-src-dest">
              Type:&nbsp;
            </p>
            {{ condition.type }}
          </div>
          <div class="src-dest-address-container">
            <p class="include-exclude-tag-src-dest">
              Regex:&nbsp;
            </p>
            <div class="regex-list" :title="condition.regexes.join(', ')">
              {{ condition.regexes.join(', ').length > 10 ? condition.regexes.join(', ').slice(0, 10) + '...' : condition.regexes.join(', ') }}
            </div>
          </div>
        </div>
        <p class="include-exclude-tag-src-dest">
          {{ condition.include ? 'Include' : 'Exclude' }}
        </p>
      </div>
    </div>
    <div class="tag-condition-editing" v-bind:class="{'editing-tag-condition-editing': tagConditionBoxState.isEditing}">
      <select class="create-dropdown-inputs" v-model="tagConditionBoxState.editingCondition.type">
        <option value="MatchesNone">Matches None</option>
        <option value="MatchesAny">Matches Any</option>
        <option value="MatchesAll" selected>Matches All</option>
        <option value="MatchesExactly">Matches Exactly</option>
      </select>
      <input class="first-tag-condition-editing-input" type="text" placeholder="Regex" v-model="tagConditionBoxState.editingCondition.regexes[0]" />
      <div class="tag-condition-editing-inputs">
        <div v-for="(regex, index) in tagConditionBoxState.editingCondition.regexes.slice(1)" :key="index">
          <input class="tag-condition-editing-input" type="text" placeholder="Regex" v-model="tagConditionBoxState.editingCondition.regexes[index + 1]" />
          <font-awesome-icon icon="fa-solid fa-minus" class="delete-input-icon" @click="deleteInputField(index + 1)"/>
        </div>
      </div>
      <font-awesome-icon icon="fa-solid fa-plus" class="add-input-icon" @click="addInputField"/>
      <div class="scrollable-selector-include-exclude-traffic">
        <p>Exclude</p>
        <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" v-model="tagConditionBoxState.editingCondition.include" />
        <p>Include</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import {ref, watch} from "vue";

const props = defineProps<{
  editLayerTagConditions: Array<tagCondition>,
}>();

interface tagCondition {
  "type":string,
  "regexes": Array<string>,
  "include": boolean,
  [key: string]: string | number | boolean | null | string[]
}

const tagConditionBoxState = ref({
  isEditing: false,
  tagConditions: [] as Array<tagCondition>,
  tagConditionSelected: -1,
  editingCondition: {
    type: "MatchesAll",
    regexes: [],
    include: true,
  } as tagCondition,
  editingConditionIndex: -1,
})

function toggleIsEditing(to: string) {
  tagConditionBoxState.value.isEditing = to === 'edit';
  tagConditionBoxState.value.tagConditionSelected = -1;
}

function addInputField() {
  const regexes = tagConditionBoxState.value.editingCondition.regexes;
  if (regexes.length === 0 || regexes[regexes.length - 1].trim() !== "") {
    tagConditionBoxState.value.editingCondition.regexes.push("");
  }
}

function deleteInputField(index: number) {
  if (index > -1) {
    tagConditionBoxState.value.editingCondition.regexes.splice(index, 1);
  }
}

function clearEditingInputs(to: string) {
  toggleIsEditing(to);
  Object.assign(tagConditionBoxState.value.editingCondition, {
    "type":"MatchesAll",
    "regexes": [],
    "include": true,
  });
}

function saveTagCondition() {
  let newtagCondition: tagCondition = { ...tagConditionBoxState.value.editingCondition };
  const defaultValues: tagCondition = {
    "type":"MatchesAll",
    "regexes": [],
    "include": false,
  };
  for (let key in defaultValues as {[key: string]: any}) {
    if (!newtagCondition.hasOwnProperty(key) || newtagCondition[key] === "" || newtagCondition[key] === null) {
      newtagCondition[key] = defaultValues[key];
    }
  }
  const isDuplicate = tagConditionBoxState.value.tagConditions.some(condition =>
    JSON.stringify(condition) === JSON.stringify(newtagCondition)
  );
  if (!isDuplicate && tagConditionBoxState.value.editingConditionIndex === -1) {
    tagConditionBoxState.value.tagConditions.push(newtagCondition);
  } else if (!isDuplicate && tagConditionBoxState.value.editingConditionIndex > -1 && tagConditionBoxState.value.editingConditionIndex < tagConditionBoxState.value.tagConditions.length) {
    Object.assign(tagConditionBoxState.value.tagConditions[tagConditionBoxState.value.editingConditionIndex], newtagCondition);
  }
  clearEditingInputs('list');
  const conditions = tagConditionBoxState.value.tagConditions.map(condition => ({
    type:condition.type,
    regexes: condition.regexes,
    include: condition.include
  }));
  emit('update-tag-conditions',  conditions);
}

// set which tag condition is highlighted by left click
function setTagConditionSelected(condition: number) {
  tagConditionBoxState.value.tagConditionSelected = condition;
}

function deleteSelectedTagCondition() {
  if (tagConditionBoxState.value.tagConditionSelected != -1) {
    tagConditionBoxState.value.tagConditions.splice(tagConditionBoxState.value.tagConditionSelected, 1);
    tagConditionBoxState.value.tagConditionSelected = -1;
  }
}

// open edit form for tag condition by double clicking it
function doubleClickTagCondition(index: number) {
  toggleIsEditing('edit');
  tagConditionBoxState.value.editingCondition = { ...tagConditionBoxState.value.tagConditions[index] };
  tagConditionBoxState.value.editingConditionIndex = index;
}

const emit = defineEmits({
  'update-tag-conditions': (payload: Array<tagCondition>) => true,
});

// receive tag conditions from LayerManagement on edit of existing layer
watch(() => props.editLayerTagConditions, (newVal) => {
  tagConditionBoxState.value.tagConditions = newVal;
});
</script>

<style scoped>
.tag-condition-box-container {
  display: flex;
  flex-direction: column;
  border: 1px solid #424242;
  height: 45vh;
  width: 90%;
  border-radius: 4px;
  font-family: 'Open Sans', sans-serif;
  overflow: hidden;
}

.tag-condition-box-menu {
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

.regex-list {
  max-width: 100%;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
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

.tag-condition-list {
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
  font-size: 1.5vh;
  font-weight: bold;
  cursor: pointer;
  height: 4.9vh;
  transition: 0.2s ease-in-out;
  padding: 2.5% 5% 2.5% 5%;
}

.selected-tag-condition {
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

.include-exclude-tag-src-dest {
  font-size: 1.5vh;
  font-weight: normal;
  margin-left: 0.5vw;
}

.tag-condition-editing {
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

.editing-tag-condition-list {
  width: 0;
  visibility: hidden;
  opacity: 0;
  height: 0;
}

.editing-tag-condition-editing {
  width: 100%;
  height: 100%;
  visibility: visible;
  opacity: 1;
}

.tag-condition-editing-input {
  border: 1px solid #424242;
  border-radius: 4px;
  font-size: 2vh;
  width: 76%;
  padding: 2%;
  margin: 0.5vh 0 0.5vh 6px;
}

.tag-condition-editing-input:focus {
  outline: none;
}

.first-tag-condition-editing-input{
  border: 1px solid #424242;
  border-radius: 4px;
  font-size: 2vh;
  width: 90%;
  padding: 2%;
  margin: 0.5vh 0;
}

.first-tag-condition-editing-input:focus {
  outline: none;
}

.delete-input-icon{
  margin-left: 0.5vw;
  margin-right: 5px;
}

.create-dropdown-inputs {
  border: 1px solid #424242;
  border-radius: 4px;
  font-size: 2vh;
  width: 94.5%;
  padding: 2%;
  margin: 0.5vh 0;
  background: white;
  color: #424242;
}

.create-dropdown-inputs:focus {
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
