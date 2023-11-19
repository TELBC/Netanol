<template>
  <div class="menu-bar">
    <div id="topology-menu">
      <div class="topology-menu-options" @click="toggleMenuBarOptions('grouping')">Grouping</div>
      <div class="topology-menu-options" @click="toggleMenuBarOptions('timeframe')">Timeframe</div>
      <div class="topology-menu-options" @click="toggleMenuBarOptions('clusters')">Clusters</div>
      <div class="topology-menu-options" @click="toggleMenuBarOptions('layouts')">Layouts</div>
      <div class="topology-menu-options">Freeze</div>
    </div>
    <div class="menu-bar-options" v-if="menuBarOptions==='grouping'">
      <button class="menu-option-content grouping-buttons">Group selection</button>
      <button class="menu-option-content grouping-buttons">Ungroup selection</button>
    </div>
    <div class="menu-bar-options" v-if="menuBarOptions==='timeframe'">
      <p class="menu-option-content timeframe-labels">from</p>
      <input class="menu-option-content" type="date">
      <p class="menu-option-content timeframe-labels">to</p>
      <input class="menu-option-content" type="date">
    </div>
    <div class="menu-bar-options" v-if="menuBarOptions==='clusters'"></div>
    <div class="menu-bar-options" v-if="menuBarOptions==='layouts'"></div>
  </div>
</template>

<script setup lang="ts">
const menuBarOptions = ref("")

const toggleMenuBarOptions = (option: string) => {
  if (menuBarOptions.value === option) {
    menuBarOptions.value = ""
  } else {
    menuBarOptions.value = option
  }
}
</script>

<style scoped>
.menu-bar {
  display: flex;
  flex-direction: column;
}

#topology-menu {
  display: flex;
  flex-direction: row;
  align-items: center;
  width: 88.5vw;
  background-color: #7EA0A9;
  margin-left: 3vw;
  font-size: 2vh;
  margin-top: -1vh;
  color: white;
  font-family: 'Open Sans', sans-serif;
}

.topology-menu-options {
  cursor: pointer;
  transition: 0.2s ease-in-out;
  padding: 1vh 2vw;
  user-select: none;
}

.topology-menu-options:hover {
  background-color: #617F87;
}

.topology-menu-options:active {
  background-color: #4B6164;
}

.menu-bar-options {
  display: flex;
  flex-direction: row;
  align-items: center;
  width: 88.4vw;
  border: 0.1vw solid #7EA0A9;
  margin-left: 3vw;
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
  border: 0.1vh solid #7EA0A9;
  background-color: white;
  margin: 1vh 1vw 1vh 2vw;
  cursor: pointer;
  transition: 0.2s ease-in-out;
}

.grouping-buttons:hover {
  box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1);
}

.grouping-buttons:active {
  box-shadow: 0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1);
}

.timeframe-labels {
  margin: 1vh 1vw 1vh 2vw;
}
</style>
