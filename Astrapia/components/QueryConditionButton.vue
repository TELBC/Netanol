<template>
  <div>
    <div class="query-conditions-selector">
      <p class="query-conditions-label">Query Conditions:</p>
      <button class="icon-button" @click="toggleForm" title="Query Conditions">
        <font-awesome-icon icon="fa-solid fa-filter" />
      </button>
    </div>
    <QueryConditionForm v-if="isVisible" @isVisible="toggleForm" :layout="layout" :queryConditions="queryConditions"/>
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import { ref } from 'vue';
import QueryConditionForm from "~/components/QueryConditionForm.vue";

const props = defineProps<{
  layout: any,
  queryConditions: any
}>();

const isVisible = ref(false);
const layout = props.layout;
const queryConditions = props.queryConditions;

const toggleForm = () => {
  isVisible.value = !isVisible.value;
};
</script>

<style scoped>
.icon-button {
  color: #585858;
  margin-left: 4px;
  margin-right: 4px;
  background: none;
  border: solid 1px #797878;
  border-radius: 3px;
  cursor: pointer;
  outline: none;
  width: 1.5vw;
}

.icon-button:hover {
  color: #000000;
  border: solid 2px #3E6474;
}

.query-conditions-label {
  font-family: 'Open Sans', sans-serif;
  color: #797878;
  margin: 0 0 0 1vw;
  font-size: 0.8rem;
}

.query-conditions-selector {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  font-family: 'Open Sans', sans-serif;
  height: 3vh;
  align-items: center;
}
</style>
