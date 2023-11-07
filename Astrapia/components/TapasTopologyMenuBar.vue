<template>
  <div id="topology-menu">
    <font-awesome-icon class="icon" v-if="!togglePacketsBytes" @click="togglePacketsBytes=!togglePacketsBytes" icon="fa-solid fa-envelope" />
    <font-awesome-icon class="icon" v-if="togglePacketsBytes" @click="togglePacketsBytes=!togglePacketsBytes" icon="fa-solid fa-cookie-bite" />
    <font-awesome-icon class="icon" icon="fa-solid fa-object-group" />
    <font-awesome-icon class="icon" icon="fa-solid fa-object-ungroup" />
    <font-awesome-icon class="icon" icon="fa-solid fa-arrows-to-circle" />
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";

// false = packets, true = bytes
const togglePacketsBytes = ref(false)
</script>

<style scoped>
#topology-menu {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  color: white;
  background: #537B87;
  font-size: 4vh;
  position: absolute;
  z-index: 99;
  margin-top: 90vh;
  margin-left: 38vw;
  width: 20vw;
  border-radius: 4px;
}

.icon {
  cursor: pointer;
  border-radius: 4px;
  width: 100%;
  padding-top: 1.5vh;
  padding-bottom: 1.5vh;
  transition: 0.2s ease-in-out;
}

.icon:hover {
  background: #3E6474;
}

.icon:active {
  background: #294D61;
}
</style>
