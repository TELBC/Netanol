<template>
  <div class="aggregation-matcher-box-container">
    <div class="aggregation-matcher-box-menu">
      <div class="list-icons-container" v-bind:class="{'list-icons-container-editing': aggregationMatcherBoxState.isEditing}">
        <font-awesome-icon icon="fa-solid fa-plus" class="matchers-list-icons" v-bind:class="{'matchers-list-editing': aggregationMatcherBoxState.isEditing}" @click="toggleIsEditing('edit')" />
        <font-awesome-icon icon="fa-solid fa-minus" class="matchers-list-icons" v-bind:class="{'matchers-list-editing': aggregationMatcherBoxState.isEditing}" @click="deleteSelectedAggregationMatcher" />
      </div>
      <div class="edit-icons-container" v-bind:class="{'editing-icons-container-editing': aggregationMatcherBoxState.isEditing}">
        <font-awesome-icon icon="fa-solid fa-arrow-left" class="editing-matcher-icons" v-bind:class="{'editing-matcher': aggregationMatcherBoxState.isEditing}" @click="clearEditingInputs('list')" />
        <font-awesome-icon icon="fa-solid fa-floppy-disk" class="editing-matcher-icons" v-bind:class="{'editing-matcher': aggregationMatcherBoxState.isEditing}" @click="saveAggregationMatcher" />
      </div>
    </div>
    <div class="aggregation-matcher-list" v-bind:class="{'editing-aggregation-matcher-list': aggregationMatcherBoxState.isEditing}">
      <div class="matchers" v-for="(matcher, index) in aggregationMatcherBoxState.aggregationMatchers" :key="index" @click="setAggregationMatcherSelected(index)" v-bind:class="{'selected-aggregation-matcher': aggregationMatcherBoxState.aggregationMatcherSelected == index}" @dblclick="doubleClickAggregationMatcher(index)">
        <div class="matchers-src-dest">
          <div class="src-dest-address-container">
            <p class="include-exclude-aggregation-src-dest">
              Network:&nbsp;
            </p>
            {{ matcher.address }}
          </div>
          <div class="src-dest-address-container">
            <p class="include-exclude-aggregation-src-dest">
              Mask:&nbsp;
            </p>
            {{ matcher.mask }}
          </div>
        </div>
        <p class="include-exclude-aggregation-src-dest">
          {{ matcher.include ? 'Include' : 'Exclude' }}
        </p>
      </div>
    </div>
    <div class="aggregation-matcher-editing" v-bind:class="{'editing-aggregation-matcher-editing': aggregationMatcherBoxState.isEditing}">
      <input id="source-input" class="scrollable-selector-input" type="text" placeholder="Network Address" v-model="aggregationMatcherBoxState.editingMatcher.address" />
      <input id="source-mask-input" class="scrollable-selector-input" type="text" placeholder="Subnet Mask" v-model="aggregationMatcherBoxState.editingMatcher.mask" />
      <div class="scrollable-selector-include-exclude-traffic">
        <p>Exclude</p>
        <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" v-model="aggregationMatcherBoxState.editingMatcher.include" />
        <p>Include</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
import { ref, watch } from "vue";

const props = defineProps<{
  editLayerAggregationMatchers: { },
}>();

interface aggregationMatcher {
  "address": string
  "mask": string,
  "include": boolean,
  [key: string]: string | number | boolean | null
}

const aggregationMatcherBoxState = ref({
  isEditing: false,
  aggregationMatchers: [] as Array<aggregationMatcher>,
  aggregationMatcherSelected: -1,
  editingMatcher: {} as aggregationMatcher,
  editingMatcherIndex: -1,
});

function toggleIsEditing(to: string) {
  aggregationMatcherBoxState.value.isEditing = to === 'edit';
  aggregationMatcherBoxState.value.aggregationMatcherSelected = -1;
}

function clearEditingInputs(to: string) {
  toggleIsEditing(to);
  Object.assign(aggregationMatcherBoxState.value.editingMatcher, {
    "address": "",
    "mask": "",
    "include": false
  });
}

function saveAggregationMatcher() {
  let newAggregationMatcher: aggregationMatcher = { ...aggregationMatcherBoxState.value.editingMatcher };
  const defaultValues: aggregationMatcher = {
    "address": "",
    "mask": "",
    "include": false
  };
  for (let key in defaultValues as {[key: string]: any}) {
    if (!newAggregationMatcher.hasOwnProperty(key) || newAggregationMatcher[key] === "" || newAggregationMatcher[key] === null) {
      newAggregationMatcher[key] = defaultValues[key];
    }
  }
  const isDuplicate = aggregationMatcherBoxState.value.aggregationMatchers.some(matcher =>
    JSON.stringify(matcher) === JSON.stringify(newAggregationMatcher)
  );
  if (!isDuplicate && aggregationMatcherBoxState.value.editingMatcherIndex === -1) {
    aggregationMatcherBoxState.value.aggregationMatchers.push(newAggregationMatcher);
  } else if (!isDuplicate && aggregationMatcherBoxState.value.editingMatcherIndex > -1 && aggregationMatcherBoxState.value.editingMatcherIndex < aggregationMatcherBoxState.value.aggregationMatchers.length) {
    Object.assign(aggregationMatcherBoxState.value.aggregationMatchers[aggregationMatcherBoxState.value.editingMatcherIndex], newAggregationMatcher);
  }
  clearEditingInputs('list');
  const matchers = aggregationMatcherBoxState.value.aggregationMatchers.map(matcher => ({
    address: matcher.address,
    mask: matcher.mask,
    include: matcher.include
  }));
  emit('update-aggregation-matchers', matchers);
}

// set which aggregation matcher is highlighted by left click
function setAggregationMatcherSelected(matcher: number) {
  aggregationMatcherBoxState.value.aggregationMatcherSelected = matcher;
}

function deleteSelectedAggregationMatcher() {
  if (aggregationMatcherBoxState.value.aggregationMatcherSelected != -1) {
    aggregationMatcherBoxState.value.aggregationMatchers.splice(aggregationMatcherBoxState.value.aggregationMatcherSelected, 1);
    aggregationMatcherBoxState.value.aggregationMatcherSelected = -1;
  }
}

// open edit form for aggregation matcher by double clicking it
function doubleClickAggregationMatcher(index: number) {
  toggleIsEditing('edit');
  aggregationMatcherBoxState.value.editingMatcher = { ...aggregationMatcherBoxState.value.aggregationMatchers[index] };
  aggregationMatcherBoxState.value.editingMatcherIndex = index;
}

const emit = defineEmits({
  'update-aggregation-matchers': (payload: Array<{ address: string; mask: string; include: boolean; }>) => true,
});

// receive aggregation matchers from LayerManagement on edit of existing layer
watch(() => props.editLayerAggregationMatchers, (newVal) => {
  aggregationMatcherBoxState.value.aggregationMatchers = newVal;
});
</script>

<style scoped>
.aggregation-matcher-box-container {
  display: flex;
  flex-direction: column;
  border: 1px solid #424242;
  height: 45vh;
  width: 90%;
  border-radius: 4px;
  font-family: 'Open Sans', sans-serif;
  overflow: hidden;
}

.aggregation-matcher-box-menu {
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

.matchers-list-icons {
  margin-right: 0.5vw;
  cursor: pointer;
  transition: 0.2s ease-in-out;
}

.matchers-list-editing {
  visibility: hidden;
  opacity: 0;
}

.editing-matcher-icons {
  margin-right: 0.5vw;
  visibility: hidden;
  opacity: 0;
  cursor: pointer;
  transition: 0.2s ease-in-out;
}

.editing-matcher {
  visibility: visible;
  opacity: 1;
}

.aggregation-matcher-list {
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

.matchers {
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

.selected-aggregation-matcher {
  background-color: #e0e0e0;
}

.matchers-src-dest {
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

.include-exclude-aggregation-src-dest {
  font-size: 1.3vh;
  font-weight: normal;
  margin-left: 0.2vw;
}

.aggregation-matcher-editing {
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

.editing-aggregation-matcher-list {
  width: 0;
  visibility: hidden;
  opacity: 0;
  height: 0;
}

.editing-aggregation-matcher-editing {
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

.aggregation-matcher-editing-input {
  border: 1px solid #424242;
  border-radius: 4px;
  font-size: 2vh;
  width: 90%;
  padding: 2%;
  margin: 0.5vh 0;
}

.aggregation-matcher-editing-input:focus {
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
