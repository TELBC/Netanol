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
              type="text"
              id="username"
              class="login_inputs"
        >
        <label>Username</label>
      </div>
      <div class="login_form">
        <input
          type="text"
          id="password"
          class="login_inputs"
        >
        <label>Password</label>
      </div>
      <button id="login_button">Login</button>
    </div>
  </div>
</template>

<script setup lang="ts">

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
  border: 1px solid #424242;
  height: 4vh;
  width: 20vw;
  margin-top: 4vh;
  padding-left: 0.5vw;
  font-size: 2.5vh;
  font-family: 'Open Sans', sans-serif;
}

label {
  font-family: 'Open Sans', sans-serif;
  font-size: 2.5vh;
  color: #424242;
  opacity: 60%;
  margin-top: 4.5vh;
  margin-left: 0.5vw;
  user-select: none;
  position: absolute;
  pointer-events: none;
  transition: 0.2s ease all;
}

.login_form input:focus~label {
  margin-top: 1.7vh;
  color: black;
  opacity: 100%;
  font-size: 1.5vh;
}

#login_button {
  border-radius: 4px;
  border-color: #424242;
  height: 4.7vh;
  width: 10vw;
  font-size: 2.5vh;
  background-color: #537B87;
  color: white;
  margin-top: 4vh;
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
