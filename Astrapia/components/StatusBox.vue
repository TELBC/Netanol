<template>
  <div class="box">
    {{ placeholderText }}
  </div>
</template>

<script setup lang="ts">
defineProps({
    placeholderText: {
      type: String,
      required: true
    }
  }
);
</script>

<style scoped>
.box {
  background-color: white;
  width: 25vw;
  height: 70vh;
  border-radius: 10px;
  box-shadow: 0 0 10px #0000004F;
  margin: 3vh 2vw 0;
}
</style>
