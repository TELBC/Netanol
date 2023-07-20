<script setup lang="ts">
import { LightTheme as theme} from '../assets/TapasTheme'
</script>

<template>
  <main>
    <div :style="{
    color: theme.colors.primaryText,
    backgroundColor: theme.colors.primarySurface,
    fontFamily: theme.fonts.primaryFont,}">
    <h1>test</h1>
    </div>
  </main>
</template>

<style scoped>
</style>

<script lang="ts">
export default {
  data() {
    return {
      theme,
    };
  }
}
</script>