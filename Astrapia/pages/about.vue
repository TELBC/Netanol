<template>
  <div>
    <div class="top-box">
      <h1 class="welcome-text">
        <span>About Netanol</span>
      </h1>
      <div class="spacer"></div>
      <div class="logo-container">
        <img class="logo" src="/NETANOL_Logo.png" alt="Netanol Logo">
      </div>
    </div>
    <div class="text-box">
      <h2 class="text-box-title">> What is Netanol?</h2>
      <p class="text-box-description">
        Netanol is a solution that focuses on collecting, analyzing, and presenting Flow data from network devices in order to efficiently gather network information and troubleshoot. By simplifying the information in a visual manner, Netanol allows system administrators to make well-informed decisions and effectively allocate their resources.
      </p>
    </div>
    <div class="text-box">
      <h2 class="text-box-title">> Who are we?</h2>
      <p class="text-box-description">
        We are a team of 5 student developers from HTBLVA Spengergasse in Austria, collaborating with NTS (our Project Partner) to further develop Netanol. This product serves as our Diplomaproject and therefore has a relatively short development time of around 7-8 months. However, we have designed Netanol to be easily continued even after the official development period. Our main objective is to assist system administrators in troubleshooting and monitoring their networks.
      </p>
    </div>
    <div class="text-box">
      <h2 class="text-box-title">> Why is Netanol different?</h2>
      <p class="text-box-description">
        Unlike other Flow analyzers, Netanol is completely free. We have made extensive documentation of our progress, features, and architecture available among other places in our Github Wiki. Additionally, what sets us apart is our Topology Page. With this page, you can quickly visualize the interconnected devices and their communication patterns.
      </p>
    </div>
    <div class="text-box">
      <h2 class="text-box-title">> Who are we working with?</h2>
        <div class="description-with-logo">
          <p class="text-box-description">
            We are really happy to be working together with NTS (Netzwerk Telekom Service AG), a leading telecommunications solutions provider in Europe. Working together has allowed us to benefit from their expertise and further develop our application.
          </p>
            <div class="nts-logo">
            <a href="https://www.nts.eu/">
              <img src="/nts-logo.png" alt="NTS" height="100">
            </a>
          </div>
      </div>
    </div>
    <div class="footer">
      <a href="https://github.com/TELBC/Netanol" target="_blank">
        <img src="/github.svg" alt="GitHub" width="20" height="20">
      </a>
      <a href="/IEEESpecification.pdf">IEEE Specification</a>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useHead } from '#imports'

useHead({
  bodyAttrs: {
    class: 'home'
  }
})
</script>

<style scoped>
.top-box {
  background-color: white;
  height: 22vh;
  display: flex;
  align-items: center;
  justify-content: center;
}

.logo-container {
  display: flex;
  justify-content: center;
}

.spacer {
  width: 22vw;
  height: auto;
}

.logo {
  max-width: 25vh;
  height: auto;
  user-select: none;
}

.welcome-text {
  font-family: 'Open Sans', sans-serif;
  color: #294d61;
  font-size: 8vh;
  user-select: none;
}

.text-box {
  margin-top: 20px;
  padding: 3px 10px;
  margin-right: 10px;
  margin-left: 20px;
  background-color: white;
  border-radius: 5px;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.07);
}

.text-box-title {
  font-weight: bold;
  margin-bottom: 10px;
}

.footer{
  font-family: 'Open Sans', sans-serif;
  margin-top: 10px;
  background: white;
  width: 95vw;
  margin-bottom: -20px;
  padding: 10px;
  display: flex;
  justify-content: center;
  align-items: center;
}

.footer a {
  color: black;
  text-decoration: none;
  margin: 0 30px;
}

.description-with-logo {
  display: flex;
  align-items: center;
  margin-top: -15px;
}

.text-box-description {
  flex: 1;
}

.nts-logo {
  margin-left: 10px;
  margin-right: 10px;
  margin-top: -20px;
  padding-bottom: 5px;
}
</style>
