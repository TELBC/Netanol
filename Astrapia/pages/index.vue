<template>
  <div>
    <div class="top-box">
        <h1 class="welcome-text">
          <span>Welcome to Netanol</span>
        </h1>
      <div class="spacer"></div>
        <div class="logo-container">
          <img class="logo" src="/NETANOL_Logo.png" alt="Netanol Logo">
        </div>
    </div>
    <div class="bottom-boxes">
      <StatusBox placeholderText="placeholder Astrapia"/>
      <StatusBox placeholderText="placeholder Fennec"/>
      <StatusBox placeholderText="placeholder DB"/>
    </div>
  </div>
</template>

<script setup lang="ts">
import StatusBox from "~/components/StatusBox.vue";
import { useHead } from '#imports'

useHead({
  bodyAttrs: {
    class: 'home'
  }
})
</script>

<style>
.home {
  background-color: #f5f5f5;
  font-family: 'Open Sans', sans-serif;
}
</style>

<style scoped>
.top-box {
  background-color: white;
  width: 97.2vw;
  height: 22vh;
  display: flex;
  align-items: center;
  justify-content: center;
}

.bottom-boxes {
  display: flex;
  justify-content: center;
  align-items: flex-end;
  width: 97.2vw;
}

.logo-container {
  display: flex;
  justify-content: center;
}
.spacer{
  width:10vw;
  height: auto;
}

.logo {
  max-width: 25vh;
  height: auto;
  user-select: none;
}

.welcome-text {
  color: #294d61;
  font-size: 8vh;
  user-select: none;
}
</style>
