<template>
  <div id="alert-box-placement" v-if=alertState.showAlert>
    <div class="alert-box">
      <p style="font-weight: bold; margin: 0.5vh 0 0 0;">{{ alertState.title }}</p>
      <p style="margin-bottom: 1.5vh; margin-top:0;">{{ alertState.message }}</p>
      <div class="alert-buttons">
        <button id="close-button" @click="hideAlert">{{ close_button }}</button>
        <button id="submit-button" v-if=alertState.submit @click="handleClick">Ok</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

const alertState = ref({
  showAlert: false,
  submit: false,
  title: '',
  message: '',
  onClick: (() => undefined) as Function,
});

const close_button = ref("Close"); // change button from "Close" to "Cancel" if submit is true ?

const handleClick = () => {
  hideAlert();
  alertState.value.onClick!();
}

function showAlertDialog(submit: boolean, title: string, message: string, onClick: Function) {
  alertState.value = {
    showAlert: true,
    submit,
    title,
    message,
    onClick
  };
}

function hideAlert() {
  alertState.value.showAlert = false;
}

defineExpose({
  showAlertDialog,
  hideAlert
});
</script>

<style scoped>
#alert-box-placement {
  display: flex;
  padding-top: 0.2vh;
  position: fixed;
  z-index: 99;
  left: 43vw;
}

.alert-box {
  background-color: white;
  border: 1px solid #424242;
  border-radius: 4px;
  padding: 0 1vh 1vh 1vh;
  width: 14vw;
  display: flex;
  flex-direction: column;
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
  width: 7.5vh;
  height: 3vh;
  font-family: 'Open Sans', sans-serif;
  cursor: pointer;
  transition: 0.2s ease-in-out;
}

button:hover {
  background-color: #617F87;
}

button:active {
  background-color: #4B6164;
}

#submit-button {
  background-color: #537B87;
  margin-left: 1vw;
}

#submit-button:hover {
  background-color: #3E6474;
}

#submit-button:active {
  background-color: #294D61;
}

#close-button {
  background-color: white;
  color: black;
}
</style>
