<template>
  <div class="layer-list">
    <div class="collapse-expand-container" @click="layerListState.isExpanded = !layerListState.isExpanded">
      <font-awesome-icon icon="fa-solid fa-chevron-right" v-if="layerListState.isExpanded" class="collapse-expand-icon" />
      <font-awesome-icon icon="fa-solid fa-chevron-left" v-if="!layerListState.isExpanded" class="collapse-expand-icon" />
    </div>
    <div class="layer-list-container" :class="{ 'expanded': layerListState.isExpanded }">

    </div>
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";

const layerListState = ref({
  isExpanded: false,
})
</script>

<style scoped>
.layer-list {
  position: absolute;
  display: flex;
  flex-direction: row;
  justify-content: flex-end;
  top: 4.7vh;
  right: 0;
  width: 15vw;
  margin: 0;
  height: 95.3vh;
  z-index: 14;
  overflow: hidden;
}

.collapse-expand-container {
  position: relative;
  top: 7vh;
  left: auto;
  width: 2vw;
  height: 5vh;
  border-left: 1px solid #424242;
  border-top: 1px solid #424242;
  border-bottom: 1px solid #424242;
  border-radius: 4px;
  cursor: pointer;
  margin: 0;
  margin-right: -1vw;
  background-color: #e0e0e0;
  color: #8d8d8d;
}

.collapse-expand-icon {
  position: relative;
  top: 1.5vh;
  left: 0.25vw;
  font-size: 2vh;
}

.layer-list-container {
  width: 0;
  height: 100%;
  background-color: white;
  border-left: 1px solid #424242;
  border-top: 1px solid #424242;
  margin: 0;
  transition: 0.2s ease-in-out;
  left: auto;
  right: 0;
  z-index: 14;
}

.layer-list-container.expanded {
  width: 14vw;
}
</style>
