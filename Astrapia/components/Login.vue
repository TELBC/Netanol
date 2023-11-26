<template>
  <div>
    <div id="login_page">
      <img id="logo" src="/NETANOL_Logo.png" alt="Netanol Logo">
      <h1 id="welcome">
        <span>Welcome&nbsp;</span>
        <span>to&nbsp;</span>
        <span>Netanol</span>
        <span>...</span>
      </h1>
      <div class="login_form">
        <input
          v-model="username"
          type="text"
          id="username"
          class="login_inputs"
          placeholder=" "
        >
        <label>Username</label>
      </div>
      <div class="login_form">
        <input
          v-model="password"
          type="password"
          id="password"
          class="login_inputs"
          placeholder=" "
          @keyup.enter="signIn"
        >
        <label>Password</label>
      </div>

      <p class="status_banner"><b>Status: </b><span>{{ auth.message }}</span></p>

      <button id="login_button" @click="signIn()">Login</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import ApiService from "~/services/restService";
import {useAuth} from "~/composables/auth";
import {onMounted, ref} from "vue";
import AuthService from "~/services/authService";
import {navigateTo} from "#app";

const username = ref('')
const password = ref('')

const auth = useAuth()
onMounted(async () => {
  const authenticated = await AuthService.getStatus();

  if (authenticated) {
    auth.value.isAuthenticated = true;
    navigateTo('/');
  }
})

async function signIn() {
  const response: any = await ApiService.post<any>("/api/auth/login", {
    username: username.value,
    password: password.value
  });

  if (response.status !== undefined && response.status == 204) {
    auth.value.isAuthenticated = true
    auth.value.message = 'Now authenticated'
    return
  }

  auth.value.message = `Failed to log in... Received ${response.response.status}: ${response.response.data}`
}
</script>

<style scoped>
#logo {
  height: 20vh;
  margin-bottom: 3vh;
  user-select: none;
}

#welcome {
  font-size: 5vh;
  margin-bottom: 3vh;
  font-family: 'Open Sans', sans-serif;
  color: #537B87;
  user-select: none;
  transform: scale(0.94);
  animation: scale 3s forwards cubic-bezier(0.5, 1, 0.89, 1);
}

@keyframes scale {
  100% {
    transform: scale(1);
  }
}

span {
  display: inline-block;
  opacity: 0;
}

span:nth-child(1) {
  animation: fade-in 0.8s 0.1s forwards cubic-bezier(0.11, 0, 0.5, 0);
}

span:nth-child(2) {
  animation: fade-in 0.8s 0.2s forwards cubic-bezier(0.11, 0, 0.5, 0);
}

span:nth-child(3) {
  animation: fade-in 0.8s 0.3s forwards cubic-bezier(0.11, 0, 0.5, 0);
}

span:nth-child(4) {
  animation: fade-in 0.8s 0.4s forwards cubic-bezier(0.11, 0, 0.5, 0);
}

@keyframes fade-in {
  100% {
    opacity: 1;
  }
}

.status_banner {
  font-family: 'Open Sans', sans-serif;
}

#login_page {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding-top: 8vh;
  max-height: 100vh;
}

.login_form {
  display: flex;
  flex-direction: column;
}

.login_inputs {
  border-radius: 4px;
  border: 0.1vh solid #424242;
  height: 3.9vh;
  width: 20vw;
  margin-top: 4vh;
  padding-left: 0.5vw;
  font-size: 2.5vh;
  font-family: 'Open Sans', sans-serif;
}

label {
  font-family: 'Open Sans', sans-serif;
  font-size: 2.5vh;
  line-height: 4vh;
  color: #424242;
  opacity: 60%;
  margin-top: 4.1vh;
  margin-left: 0.5vw;
  user-select: none;
  position: absolute;
  pointer-events: none;
  transition: 0.2s ease all;
}

.login_form input:focus {
  outline: none;
}

.login_form input:focus ~ label {
  margin-top: 0.8vh;
  color: black;
  opacity: 100%;
  font-size: 2vh;
}

.login_form input:not(:placeholder-shown):not(input:focus) ~ label {
  margin-top: 0.8vh;
  color: black;
  opacity: 100%;
  font-size: 2vh;
}

#login_button {
  border-radius: 4px;
  border-color: #424242;
  height: 4.7vh;
  width: 10vw;
  font-size: 2.5vh;
  background-color: #537B87;
  color: white;
  font-family: 'Open Sans', sans-serif;
}

#login_button:hover {
  background-color: #3E6474;
  color: white;
  cursor: pointer;
}

#login_button:active {
  background-color: #294D61;
  color: white;
}
</style>
