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
    <SelfTestContainer />
    <div class="divider"></div>
    <FlowMetricCardsContainer />
    <div class="divider"></div>
    <FlowImporterGraphContainer />
  </div>
</template>

<script setup lang="ts">
import { useHead } from '#imports'
import FlowMetricCardsContainer from "~/components/FlowMetricCardsContainer.vue";
import SelfTestContainer from "~/components/SelfTestContainer.vue";
import FlowImporterGraphContainer from "~/components/FlowImporterGraphContainer.vue";

useHead({
  bodyAttrs: {
    class: 'home'
  }
})
</script>

<style>
.home {
  background-color: white;
  font-family: 'Open Sans', sans-serif;
  overflow-x: hidden;
}
</style>

<style scoped>
.top-box {
  width: 96.5vw;
  height: 22vh;
  display: flex;
  align-items: center;
  justify-content: center;
  border-bottom: 1px solid #e0e0e0;
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

.divider {
  width: 96.5vw;
  height: 1px;
  background-color: #e0e0e0;
}
</style>
