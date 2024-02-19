<template>
  <div class="arrow-container" @click="toggleMenu">
    <font-awesome-icon :icon="isOpen ? 'fa-chevron-up' : 'fa-chevron-down'" />
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";

const props = defineProps<{
  isOpen: Boolean
}>();

const emit = defineEmits<{
  click: [];
}>();

const toggleMenu = () => {
  emit('click');
};
</script>

<style scoped>
.arrow-container {
  margin-top: 2px;
  cursor: pointer;
  margin-left: 4px;
  margin-right: 4px;
}
</style>
