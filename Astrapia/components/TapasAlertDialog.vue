<template>
  <div id="alert-box-placement" v-if=showAlert>
    <div class="alert-box">
      <p style="font-weight: bold; margin-bottom:0;">{{ title }}</p>
      <p style="margin-bottom: 1.5vh; margin-top:0;">{{ message }}</p>
      <div class="alert-buttons">
        <button id="submit-button" v-if=submit @click="handleClick">Ok</button>
        <button @click="hideAlert">{{ close_button }}</button>
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
  onClick: Function
});

const close_button = ref("Close"); // change button from "Close" to "Cancel" if submit is true ?

const hideAlert = inject('hideAlert') as () => void;

const handleClick = () => {
  hideAlert();
  props.onClick!();
}
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
  padding: 1vh;
  width: 14vw;
  display: flex;
  flex-direction: column;
  align-items: center;
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
  transition: 0.2s ease-in-out;
}

button:hover {
  background-color: #617F87;
}

button:active {
  background-color: #4B6164;
}

#submit-button {
  margin-right: 4.5vw;
  background-color: #537B87;
}

#submit-button:hover {
  background-color: #3E6474;
}

#submit-button:active {
  background-color: #294D61;
}
</style>
