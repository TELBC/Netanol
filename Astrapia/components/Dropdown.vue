<template>
  <div class="dropdown">
    <div class="select" v-on:click.stop="changeDropdownState">
      <div class="selected-option" v-text="dropdownState.selectedOption"></div>
      <font-awesome-icon icon="fa-solid fa-chevron-down" v-if="!dropdownState.isOpen" />
      <font-awesome-icon icon="fa-solid fa-chevron-up" v-if="dropdownState.isOpen" />
    </div>
    <div class="options" v-bind:class="{ 'open': dropdownState.isOpen }">
      <div class="options-wrapper">
        <div class="option" v-for="option in dropdownState.options" :key="rerender.dropdownRerender">
          <div class="option-name" v-on:click="selectOption(option['name'])" v-on:click.stop="changeDropdownState">
            {{ option['name'] }}
          </div>
          <div class="affect-layout-button" v-on:click="onUpdate(option['name'])">
            <font-awesome-icon icon="fa-solid fa-pen" />
          </div>
          <div class="delete-button affect-layout-button" v-on:click="deleteLayout(option['name'])">
            <font-awesome-icon icon="fa-solid fa-trash" />
          </div>
        </div>
      </div>
      <div class="create-layout-container">
        <input class="new-layout" type="text" v-model="dropdownState.newLayout" />
        <div class="error-create-layout" v-bind:class="{ 'create-error-appear': dropdownState.createError }">!</div>
        <div class="create-error-tooltip">{{ dropdownState.createErrorMessage }} Error</div>
        <div v-if="dropdownState.isCreate">
          <font-awesome-icon icon="fa-solid fa-plus" v-on:click="createLayout" />
        </div>
        <div v-if="!dropdownState.isCreate" v-on:click="updateLayout(dropdownState.oldName, dropdownState.newLayout)">
          <font-awesome-icon icon="fa-solid fa-pen" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import layoutService from "~/services/layoutService";
import {onMounted,watch,ref} from "vue";
export interface Layouts {
  name: string,
  layerCount: number
}

const dropdownState = ref({
  isOpen: false,
  selectedOption: '',
  options: [],
  newLayout: '',
  isCreate: true,
  oldName: '',
  createError: false,
  createErrorMessage: '',
});

const rerender = ref({
  dropdownRerender: dropdownState.value.options.length
})

function selectOption(option: Layouts['name']) {
  dropdownState.value.selectedOption = option;
}

function changeDropdownState() {
  dropdownState.value.isOpen = !dropdownState.value.isOpen;
  dropdownState.value.createError = false;
  dropdownState.value.isCreate = true;
  dropdownState.value.newLayout = '';
}

async function getLayouts() {
  dropdownState.value.options = await layoutService.getLayouts() as Layouts[];
  rerender.value.dropdownRerender = dropdownState.value.options.length;
}

async function createLayout() {
  const newLayout = dropdownState.value.newLayout.trim();
  dropdownState.value.createError = false;
  if (newLayout !== '' && !dropdownState.value.options.some((option: { name: string }) => option.name === newLayout)) {
    await layoutService.createLayout(newLayout);
    await getLayouts();
    dropdownState.value.selectedOption = newLayout;
    dropdownState.value.isOpen = false;
    dropdownState.value.newLayout = '';
  }
  else if (dropdownState.value.options.some((option: { name: string }) => option.name === newLayout)) {
    dropdownState.value.createError = true;
    dropdownState.value.createErrorMessage = 'Duplicate Layout Name';
  }
}

async function deleteLayout(layout: string) {
  await layoutService.deleteLayout(layout);
  await getLayouts();
  if (dropdownState.value.selectedOption === layout) {
    if (dropdownState.value.options.length > 0) {
      dropdownState.value.selectedOption = dropdownState.value.options[0]['name'] ?? 'No layouts found';
    } else {
      dropdownState.value.selectedOption = 'No layouts found';
    }
  }
}

async function updateLayout(name: string, newName: string) {
  const newLayout = newName.trim();
  dropdownState.value.createError = false;
  if (newLayout !== '' && !dropdownState.value.options.some((option: { name: string }) => option.name === newLayout)) {
    await layoutService.updateLayout(name, newLayout);
    dropdownState.value.isCreate = true;
    dropdownState.value.newLayout = '';
    if (dropdownState.value.selectedOption === name) {
      dropdownState.value.selectedOption = newLayout
    }
    await getLayouts();
  }
  else if (dropdownState.value.options.some((option: { name: string }) => option.name === newLayout)) {
    dropdownState.value.createError = true;
    if (name === newLayout) {
      dropdownState.value.createErrorMessage = 'Layout Name is already "' + newLayout + '"';
    }
    else {
      dropdownState.value.createErrorMessage = 'Duplicate Layout Name';
    }
  }

}

function onUpdate(name: string) {
  dropdownState.value.createError = false;
  dropdownState.value.newLayout = name;
  dropdownState.value.oldName = name;
  dropdownState.value.isCreate = false;
}

const emit = defineEmits<{
  changeLayout: [layout: string]
}>()

watch([() => dropdownState.value.selectedOption], () => {
  emit('changeLayout', dropdownState.value.selectedOption);
})

onMounted(async() => {
  await getLayouts();
  dropdownState.value.selectedOption = dropdownState.value.options.length > 0 ? dropdownState.value.options[0]['name'] : 'No layouts found';
})
</script>

<style>
.dropdown {
  position: relative;
  user-select: none;
  border-radius: 4px;
  border: 1px solid #424242;
  padding: 0.5vh 0.5vw;
  font-family: 'Open Sans', sans-serif;
  background-color: white;
  cursor: pointer;
  font-size: 1.5vh;
  width: 10vw;
}

.select {
  display: flex;
  flex-direction: row;
  align-items: center;
  width: 100%;
  justify-content: space-between;
}

.selected-option {
  padding-right: 0.5vw;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.options {
  width: 100%;
  position: absolute;
  top: 100%;
  left: 0;
  display: none;
  border-radius: 4px;
  border: 1px solid #424242;
  background-color: white;
}

.options.open {
  display: flex;
  flex-direction: column;
}

.options-wrapper {
  max-height: 30vh;
  overflow-y: auto;
  overflow-x: hidden;
}

.option {
  display: flex;
  flex-direction: row;
  align-items: center;
  cursor: pointer;
  transition: background-color 0.3s ease, color 0.3s ease;
  width: 100%;
}

.option-name {
  width: 90%;
  padding: 0.5vh 0 0.5vh 0.5vw;
  overflow-x: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.option:hover {
  background-color: #7EA0A9;
  color: white;


  .affect-layout-button {
    color: white;
  }

  .option-name {
    width: 90%;
    word-wrap: break-word;
    white-space: normal;
    background-color: #7EA0A9;
  }
}

.option:active {
  background-color: #617F87;
  color: white;

  .affect-layout-button {
    color: white;
  }
}

.affect-layout-button {
  color: #7EA0A9;
  font-size: 1.5vh;
  padding: 0.5vh 0 0.5vh 0.5vw;
}

.delete-button {
  display: flex;
  justify-content: flex-end;
  width: 10%;
  padding-right: 0.5vw;
}

.create-layout-container {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  padding: 0.5vh 0.5vw;
  border-top: 1px solid #424242;
}

.error-create-layout {
  color: white;
  font-family: 'Open Sans', sans-serif;
  font-weight: bolder;
  font-size: 0.7vh;
  width: 5%;
  user-select: none;
}

.create-error-appear {
  display: flex;
  justify-content: center;
  align-items: center;
  border: 1.5px solid #EB5050;
  border-radius: 50%;
  color: #EB5050;
}

.create-error-tooltip {
  display: none;
  position: absolute;
  bottom: 3.2vh;
  width: 80%;
  word-wrap: anywhere;
}

.create-error-tooltip::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 89%;
  width: 0;
  height: 0;
  border: 12px solid transparent;
  border-top-color: #eb5050;
  border-bottom: 0;
  margin-left: -7px;
  margin-bottom: -7px;
}

.error-create-layout.create-error-appear:hover + .create-error-tooltip {
    display: flex;
    align-items: center;
    padding: 0.3vh 0.3vw;
    font-size: 1.3vh;
    background-color: #EB5050;
    color: white;
    border-radius: 4px;
    border: 1px solid #424242;
}

.new-layout {
  width: 75%;
}
</style>
