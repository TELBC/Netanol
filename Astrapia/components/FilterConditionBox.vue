<template>
  <div class="filter-condition-box-container">
    <div class="filter-condition-box-menu">
      <div class="list-icons-container" v-bind:class="{'list-icons-container-editing': filterConditionBoxState.isEditing}">
        <font-awesome-icon icon="fa-solid fa-plus" class="conditions-list-icons" v-bind:class="{'conditions-list-editing': filterConditionBoxState.isEditing}" @click="toggleIsEditing('edit')" />
        <font-awesome-icon icon="fa-solid fa-minus" class="conditions-list-icons" v-bind:class="{'conditions-list-editing': filterConditionBoxState.isEditing}" />
      </div>
      <div class="edit-icons-container" v-bind:class="{'editing-icons-container-editing': filterConditionBoxState.isEditing}">
        <font-awesome-icon icon="fa-solid fa-arrow-left" class="editing-condition-icons" v-bind:class="{'editing-condition': filterConditionBoxState.isEditing}" @click="toggleIsEditing('list')" />
        <font-awesome-icon icon="fa-solid fa-floppy-disk" class="editing-condition-icons" v-bind:class="{'editing-condition': filterConditionBoxState.isEditing}" />
      </div>
    </div>
    <div class="filter-condition-list">

    </div>
    <div class="filter-condition-editing">
      <!--
      instead of dropdowns expanding and collapsing lists:
      eg Source Address (arrow down) and on click it expands to a list of available addresses and when one is selected it gets set instead of "Source Address"
      OWN COMPONENT?
      -->
      <div>
        Search Bar for Source Addresses, styled to look like an expandable list, dropdownish, live searching
        <input type="text" placeholder="Source Address" />
        <div>
          List of available addresses, scrollable
        </div>
      </div>
      <input type="text" placeholder="Source Address Mask" />
      <input type="text" placeholder="Source Port" />
      <input type="text" placeholder="Destination Address" />
      <input type="text" placeholder="Destination Address Mask" />
      <input type="text" placeholder="Destination Port" />
      <input type="text" placeholder="Protocol" />
      <input type="checkbox" />
    </div>
  </div>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import {ref} from "vue";

// put into other file
interface filterCondition {
  "sourceAddress": string
  "sourceAddressMask": string,
  "sourcePort": number | null,
  "destinationAddress": string,
  "destinationAddressMask": string,
  "destinationPort": number | null,
  "protocol": string,
  "include": boolean
}

const filterConditionBoxState = ref({
  isEditing: false,
  filterConditions: [] as Array<filterCondition>,
  ipAddresses: [] as Array<string>,
  addressMasks: [] as Array<string>,
  ports: [] as Array<number>,
  protocols: [] as Array<string>
})

function toggleIsEditing(to: string) {
  filterConditionBoxState.value.isEditing = to === 'edit';
}
</script>

<style scoped>
.filter-condition-box-container {
  display: flex;
  flex-direction: column;
  border: 1px solid #424242;
  height: 25vh;
  width: 90%;
  border-radius: 4px;
  font-family: 'Open Sans', sans-serif;
}

.filter-condition-box-menu {
  display: flex;
  align-items: center;
  width: 90%;
  height: 2vh;
  border-bottom: 1px solid #424242;
  padding: 0.5vh 5%;
  background-color: #e0e0e0;
}

.list-icons-container {
  display: flex;
  align-items: center;
  justify-items: flex-start;
  width: 100%;
  transition: 0.2s ease-in-out;
}

.list-icons-container-editing {
  width: 0;
}

.edit-icons-container {
  display: flex;
  align-items: center;
  justify-items: flex-start;
  width: 0;
  transition: 0.2s ease-in-out;
}

.editing-icons-container-editing {
  width: 100%;
}

.conditions-list-icons {
  margin-right: 0.5vw;
  cursor: pointer;
  transition: 0.2s ease-in-out;
}

.conditions-list-editing {
  visibility: hidden;
  opacity: 0;
}

.editing-condition-icons {
  margin-right: 0.5vw;
  visibility: hidden;
  opacity: 0;
  cursor: pointer;
  transition: 0.2s ease-in-out;
}

.editing-condition {
  visibility: visible;
  opacity: 1;
}
</style>
