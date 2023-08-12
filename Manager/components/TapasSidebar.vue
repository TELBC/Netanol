<template>
  <div class="sidebar-main">
    <div class="toggle-sidebar-wrap">
      <font-awesome-icon id="toggle-sidebar" icon="fa-solid fa-bars" />
    </div>
    <NuxtLink to="/">
      <div class="links">
        <font-awesome-icon id="chart" class="faicon" icon="fa-solid fa-chart-line" />
        <div class="text-wrap" id="chart-text">
          <p>Network Graph</p>
        </div>
      </div>
    </NuxtLink>
    <NuxtLink to="/">
      <font-awesome-icon class="faicon" icon="fa-solid fa-diagram-project" />
      <div class="text-wrap">
        <p>"Packet Tracer"</p>
      </div>
    </NuxtLink>
    <NuxtLink to="/" id="settings-icon">
      <font-awesome-icon class="faicon" icon="fa-solid fa-gear" />
      <div class="text-wrap">
        <p>Settings</p>
      </div>
    </NuxtLink>
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
</script>

<style scoped>
.sidebar-main {
  display: flex;
  flex-direction: column;
  position: fixed;
  overflow: hidden;
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

.sidebar-main:hover {
  width: 18rem;
  overflow: visible;

  .toggle-sidebar-wrap {
    justify-content: flex-end;
    padding-right: 1rem;
  }
}

.sidebar-main:hover p {
  opacity: 1;
}

a {
  text-decoration: none;
  display: flex;
  flex-direction: row;
  flex-wrap: nowrap;
  align-items: center;
  padding-top: 1rem;
  padding-bottom: 1rem;
}

a:hover {
  background: #3E6474;
  transition: 0.2s ease-in-out;
}

a:active {
  background: #294D61;
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

#toggle-sidebar {
  margin-top: 1rem;
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
  transition: 0.1s ease-in-out;
  opacity: 0;
}

.faicon {
  cursor: pointer;
  color: white;
  margin-left: 1rem;
}

#settings-icon {
  padding-top: 1rem;
  margin-top: auto;
  margin-bottom: 1rem;
  cursor: pointer;
}
</style>
