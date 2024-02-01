<template>
  <div class="menu-bar">
    <div id="topology-menu">
      <div class="topology-menu-options" @click="toggleMenuBarOptions('grouping')">Grouping</div>
      <div class="topology-menu-options" @click="toggleMenuBarOptions('timeframe')">Timeframe</div>
      <div class="topology-menu-options" @click="toggleMenuBarOptions('clusters')">Clusters</div>
      <div class="topology-menu-options">Freeze</div>
      <div class="layout-dropdown"><Dropdown @changeLayout="handleLayoutChange" /></div>
    </div>
    <div class="menu-bar-options" v-if="menuBarOptions==='grouping'">
      <button class="menu-option-content grouping-buttons">Group selection</button>
      <button class="menu-option-content grouping-buttons">Ungroup selection</button>
    </div>
    <div class="menu-bar-options" v-if="menuBarOptions==='timeframe'">
      <TopologyTimeframeSelector @change="handleTimeframeSelection" :from-value="fromValue" :to-value="toValue" />
    </div>
    <div class="menu-bar-options" v-if="menuBarOptions==='clusters'"></div>
  </div>
</template>

<script setup lang="ts">
import {Dropdown} from "#components";
import { ref } from 'vue';
import TopologyTimeframeSelector from "~/components/TopologyTimeframeSelector.vue";

const props = defineProps({
  fromValue: {
    type: String
  },
  toValue: {
    type: String
  },
});

const menuBarOptions = ref("")

const toggleMenuBarOptions = (option: string) => {
  if (menuBarOptions.value === option) {
    menuBarOptions.value = ""
  } else {
    menuBarOptions.value = option
  }
}

const emit = defineEmits<{
  changeLayout: [selectedLayout: string];
  change: [from: string, to: string];
}>();

const handleLayoutChange = (selectedLayout: string) => {
  emit('changeLayout', selectedLayout);
}

const handleTimeframeSelection = (from: string, to: string) => {
  emit('change', from, to);
}
</script>

<style scoped>
.menu-bar {
  display: flex;
  flex-direction: column;
  position: fixed;
  z-index: 99;
}

#topology-menu {
  display: flex;
  flex-direction: row;
  align-items: center;
  width: 100vw;
  background-color: #537B87;
  font-size: 2vh;
  font-family: 'Open Sans', sans-serif;
}

.topology-menu-options {
  cursor: pointer;
  transition: 0.2s ease-in-out;
  padding: 1vh 2vw;
  user-select: none;
  color: white;
}

.topology-menu-options:hover {
  background-color: #3E6474;
}

.topology-menu-options:active {
  background-color: #294D61;
}

.menu-bar-options {
  display: flex;
  flex-direction: row;
  align-items: center;
  border: 0.1vw solid #424242;
  background-color: #e0e0e0;
  font-size: 2vh;
}

.menu-option-content {
  font-family: 'Open Sans', sans-serif;
}

.grouping-buttons {
  display: flex;
  justify-content: center;
  padding: 0.5vh 0.5vw;
  width: 8vw;
  border-radius: 4px;
  border: 0.1vh solid #424242;
  background-color: white;
  margin: 1vh 1vw 1vh 2vw;
  cursor: pointer;
  transition: 0.2s ease-in-out;
}

.grouping-buttons:hover {
  background-color: #7EA0A9;
  color: white;
}

.grouping-buttons:active {
  background-color: #617F87;
  color: white;
}

.layout-dropdown {
  margin-left: auto;
  margin-right: 4vw;
}
</style>
