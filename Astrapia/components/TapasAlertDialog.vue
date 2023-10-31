<template>
  <div id="alert-box-placement" v-if=showAlert>
    <div class="alert-box">
      <p style="font-weight: bold; margin-bottom:0;">{{ title }}</p>
      <p style="margin-bottom: 1.5vh; margin-top:0;">{{ message }}</p>
      <div class="alert-buttons">
        <button id="submit-button" v-if=submit>Ok</button>
        <button @click="showAlert=false">Close</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
const props = defineProps({
  showAlert: Boolean,
  submit: Boolean,
  title: String,
  message: String,
});
</script>

<style scoped>
#alert-box-placement {
  display: flex;
  justify-content: center;
  align-items: flex-start;
  padding-top: 1vh;
}

.alert-box {
  position: absolute;
  background-color: white;
  border: 1px solid #424242;
  border-radius: 4px;
  padding: 1vh;
  width: 25vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  z-index: 2;
  font-size: 2vh;
  font-family: 'Open Sans', sans-serif;
}

.alert-buttons {
  display: flex;
  justify-content: flex-end;
  width: 100%;
}

button {
  background-color: #7EA0A9;
  color: white;
  border: 1px solid #424242;
  border-radius: 4px;
  padding: 0.5vh;
  width: 10vh;
  height: 4vh;
  font-size: 2vh;
  font-family: 'Open Sans', sans-serif;
  cursor: pointer;
}

#submit-button {
  margin-right: 3vw;
}
</style>
