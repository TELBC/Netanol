<template>
  <div class="sidebar-main">
    <div class="toggle-sidebar-wrap">
      <font-awesome-icon id="toggle-sidebar" icon="fa-solid fa-bars" />
    </div>
    <NuxtLink to="/">
      <div class="links">
        <div class="icon-placement">
          <font-awesome-icon id="chart" class="faicon" icon="fa-solid fa-house" />
        </div>
        <div class="text-wrap" id="chart-text">
          <p>Main Menu</p>
        </div>
      </div>
    </NuxtLink>
    <NuxtLink to="/network-analysis">
      <div class="links">
        <div class="icon-placement">
          <font-awesome-icon class="faicon" icon="fa-solid fa-chart-line" />
        </div>
        <div class="text-wrap">
          <p>Network Analysis</p>
        </div>
      </div>
    </NuxtLink>
    <NuxtLink to="/topology">
      <div class="links">
        <div class="icon-placement">
          <font-awesome-icon class="faicon" icon="fa-solid fa-diagram-project" />
        </div>
        <div class="text-wrap">
          <p>Topology</p>
        </div>
      </div>
    </NuxtLink>
    <NuxtLink to="/about">
      <div class="links">
        <div class="icon-placement">
          <font-awesome-icon class="faicon" icon="fa-solid fa-circle-info" />
        </div>
        <div class="text-wrap">
          <p>About</p>
        </div>
      </div>
    </NuxtLink>
    <NuxtLink to="/settings" id="settings-icon">
      <div class="links">
        <div class="icon-placement">
          <font-awesome-icon class="faicon" icon="fa-solid fa-gear" />
        </div>
        <div class="text-wrap">
          <p>Settings</p>
        </div>
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
  width: 3.5vw;
  height: 100vh;
  background: #537B87;
  color: white;
  font-size: 2rem;
  font-family: 'Open Sans', sans-serif;
  transition: 0.2s ease-in-out;
  user-select: none;
  z-index: 1;
}

a {
  text-decoration: none;
  display: flex;
  flex-direction: row;
  align-items: center;
  padding-top: 1.5vh;
  padding-bottom: 1.5vh;
}

.links {
  display: flex;
}

.icon-placement {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 3.5vw;
}

#settings-icon {
  padding-top: 1.5vh;
  margin-top: auto;
  margin-bottom: 1.5vh;
  cursor: pointer;
}

.faicon {
  cursor: pointer;
  color: white;
}

.text-wrap {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  height: 2rem;
}

.toggle-sidebar-wrap {
  display: flex;
  justify-content: center;
  padding-bottom: 1.5vh;
  border-bottom: 1px solid white;
}

#toggle-sidebar {
  margin-top: 1.5vh;
}

p {
  color: white;
  white-space: nowrap;
  font-size: 1.3rem;
  transition: 0.1s ease-in-out;
  opacity: 0;
}

.sidebar-main:hover {
  width: 15.75vw;
  overflow: visible;
}

.sidebar-main:hover .toggle-sidebar-wrap {
  justify-content: flex-end;
  padding-right: 0.875vw;
}

.sidebar-main:hover p {
  opacity: 1;
}

a:hover {
  background: #3E6474;
  transition: 0.2s ease-in-out;
}

a:active {
  background: #294D61;
}
</style>
