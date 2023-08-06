<template>
  <div class="sidebar-main" :class="`${is_expanded ? 'is-expanded': ''}`">
    <div class="toggle-sidebar-wrap">
      <font-awesome-icon id="toggle-sidebar" @click="toggleSidebar" icon="fa-solid fa-bars" />
    </div>
    <NuxtLink to="/">
      <div class="links">
        <font-awesome-icon id="chart" class="faicon" icon="fa-solid fa-chart-line" />
        <div class="text-wrap" id="chart-text">
          <p v-if="is_expanded">Network Graph</p>
        </div>
      </div>
    </NuxtLink>
    <NuxtLink to="/" class="link-between">
      <font-awesome-icon class="faicon" icon="fa-solid fa-diagram-project" />
      <div class="text-wrap">
        <p v-if="is_expanded">"Packet Tracer"</p>
      </div>
    </NuxtLink>
    <NuxtLink to="/" id="settings-icon">
      <font-awesome-icon class="faicon" icon="fa-solid fa-gear" />
      <div class="text-wrap">
        <p v-if="is_expanded">Settings</p>
      </div>
    </NuxtLink>
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import { ref } from "vue";

const is_expanded = ref(false);

const toggleSidebar = () => {
  is_expanded.value = !is_expanded.value;
  console.log(is_expanded.value);
}
</script>

<style scoped>
.sidebar-main {
  display: flex;
  flex-direction: column;
  overflow: hidden;
  position: fixed;
  left: 0;
  top: 0;
  width: 4rem;
  height: 100vh;
  background: #537B87;
  color: white;
  font-size: 2rem;
  //font-family: 'Open Sans', sans-serif;
  transition: 0.2s ease-in-out;
  user-select: none;
  z-index: 999;
}

a {
  text-decoration: none;
  display: flex;
  flex-direction: row;
  flex-wrap: nowrap;
  align-items: center;
  margin-left: 1rem;
}

.links {
  display: flex;
}

.text-wrap {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  height: 2rem;
}

#chart-text {
  margin-top: 1rem;
}

.link-between {
  margin-top: 2rem;
}

#toggle-sidebar {
  margin-top: 1rem;
  cursor: pointer;
}

.toggle-sidebar-wrap {
  display: flex;
  justify-content: center;
  padding-bottom: 1rem;
  border-bottom: 1px solid white;
}

p {
  color: white;
  white-space: nowrap;
  font-size: 1.3rem;
  margin-left: 1rem;
}

.is-expanded {
  width: 18rem;

  .toggle-sidebar-wrap {
    justify-content: flex-end;
    padding-right: 1rem;
  }
}

#chart {
  margin-top: 1rem;
}

.faicon {
  cursor: pointer;
  color: white;
}

#settings-icon {
  padding-top: 2rem;
  margin-top: auto;
  margin-bottom: 1rem;
  cursor: pointer;
}
</style>
