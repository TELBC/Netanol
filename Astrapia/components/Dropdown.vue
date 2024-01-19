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
import layoutService, {Layouts} from "~/services/layoutService";
import {watch} from "vue";

const dropdownState = ref({
  isOpen: false,
  selectedOption: '',
  options: [],
  newLayout: '',
  isCreate: true,
  oldName: ''
});

const rerender = ref({
  dropdownRerender: dropdownState.value.options.length
})

function selectOption(option: Layouts['name']) {
  dropdownState.value.selectedOption = option;
}

function changeDropdownState() {
  dropdownState.value.isOpen = !dropdownState.value.isOpen;
}

async function getLayouts() {
  dropdownState.value.options = await layoutService.getLayouts() as Layouts[];
  rerender.value.dropdownRerender = dropdownState.value.options.length;
}

async function createLayout() {
  await layoutService.createLayout(dropdownState.value.newLayout);
  await getLayouts();
  dropdownState.value.selectedOption = dropdownState.value.newLayout;
  dropdownState.value.isOpen = false;
  dropdownState.value.newLayout = '';
}

async function deleteLayout(layout: string) {
  await layoutService.deleteLayout(layout);
  await getLayouts();
}

async function updateLayout(name: string, newName: string) {
  await layoutService.updateLayout(name, newName);
  dropdownState.value.isCreate = true;
  dropdownState.value.newLayout = '';
  await getLayouts();
}

function onUpdate(name: string) {
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
  dropdownState.value.selectedOption = dropdownState.value.options[0]['name'] ?? 'No layouts found';
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
  font-size: 2vh;
  width: 15vw;
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
}

.option {
  display: flex;
  flex-direction: row;
  align-items: center;
  cursor: pointer;
  transition: background-color 0.3s ease, color 0.3s ease;
  width: 100%;
}

.option:hover {
  background-color: #7EA0A9;
  color: white;

  .affect-layout-button {
    color: white;
  }
}

.option:active {
  background-color: #617F87;
  color: white;

  .affect-layout-button {
    color: white;
  }
}

.option-name {
  width: 90%;
  padding: 0.5vh 0 0.5vh 0.5vw;
}

.affect-layout-button {
  color: #7EA0A9;
  font-size: 1.5vh;
}

.delete-button {
  display: flex;
  justify-content: flex-end;
  width: 10%;
  padding: 0.5vh 0.5vw 0.5vh 0;
}

.create-layout-container {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  cursor: pointer;
  padding: 0.5vh 0.5vw;
}

.new-layout {
  width: 85%;
}
</style>
