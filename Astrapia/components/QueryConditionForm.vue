<template>
  <div class="query-condition-form-overlay" @click="handleOverlayClick">
    <div class="query-condition-form" @click.stop>
      <div class="title">Query Conditions</div>
      <div class="layout-title">Layout-name: <b><u>{{ props.layout }}</u></b></div>
      <div class="scrollable-selector-include-exclude-traffic">
        <div class="subtitle-duplicates">Duplicates:</div>
        <input type="checkbox" class="theme-checkbox" v-model="allowDuplicates" />
      </div>
      <div class="subtitle">Flow Whitelist:</div>
      <select class="first-tag-condition-editing-input" v-model="selectedFlowProtocols" @change="addProtocol('flow')">
        <option value="">Select Protocol</option>
        <option value="Ipfix">Ipfix</option>
        <option value="Netflow5">Netflow5</option>
        <option value="Netflow9">Netflow9</option>
        <option value="sFlow">sFlow</option>
      </select>
      <div v-for="(protocol, index) in flowProtocolsWhitelist" :key="index" class="whitelist-item">
        <span class="dot">&#8226;</span>
        <span class="protocol-text">{{ protocol }}</span>
        <font-awesome-icon icon="fa-solid fa-minus" class="removal-icon" @click="removeProtocol('flow', index)"/>
      </div>
      <div class="subtitle">Protocol Whitelist:</div>
      <select class="first-tag-condition-editing-input" v-model="selectedDataProtocols" @change="addProtocol('data')">
        <option value="">Select Protocol</option>
        <option value="Unknown">Unknown</option>
        <option value="Tcp">TCP</option>
        <option value="Udp">UDP</option>
      </select>
      <div v-for="(protocol, index) in dataProtocolsWhitelist" :key="index" class="whitelist-item">
        <span class="dot">&#8226;</span>
        <span class="protocol-text">{{ protocol }}</span>
        <font-awesome-icon icon="fa-solid fa-minus" class="removal-icon" @click="removeProtocol('data', index)"/>
      </div>
      <div class="subtitle">Port Whitelist:</div>
      <div class="port-input-container">
        <input class="first-tag-condition-editing-input" type="number" v-model="selectedPort" placeholder="Enter Port Number" @keydown.enter.prevent="addPort">
        <font-awesome-icon icon="fa-solid fa-plus" class="adding-icon" @click="addPort"/>
      </div>
      <div v-for="(port, index) in portsWhitelist" :key="index" class="whitelist-item">
        <span class="dot">&#8226;</span>
        <span class="protocol-text">{{ port }}</span>
        <font-awesome-icon icon="fa-solid fa-minus" class="removal-icon" @click="removePort(index)"/>
      </div>
      <div class="button-container">
        <button @click="saveForm">Save</button>
        <button @click="cancelForm">Cancel</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";
import LayoutService from "~/services/layoutService";

const emit = defineEmits<{
  isVisible: []
}>()

const props = defineProps<{
  layout: string
}>();

const allowDuplicates = ref(true);
const flowProtocolsWhitelist = ref([]);
const dataProtocolsWhitelist = ref([]);
const portsWhitelist = ref([]);

let selectedFlowProtocols = '';
let selectedDataProtocols = '';
let selectedPort = '';

const saveForm = () => {
  const jsonData = {
    allowDuplicates: allowDuplicates.value,
    flowProtocolsWhitelist: flowProtocolsWhitelist.value,
    dataProtocolsWhitelist: dataProtocolsWhitelist.value,
    portsWhitelist: portsWhitelist.value
  };
  console.log(jsonData);
  LayoutService.setQueryConditions('test',jsonData);
  cancelForm();
};

const cancelForm = () => {
  allowDuplicates.value = true;
  flowProtocolsWhitelist.value = [];
  dataProtocolsWhitelist.value = [];
  portsWhitelist.value = [];
  emit('isVisible');
};

const addProtocol = (type) => {
  const selectedProtocol = type === 'flow' ? selectedFlowProtocols : selectedDataProtocols;
  if (selectedProtocol && !flowProtocolsWhitelist.value.includes(selectedProtocol) && !dataProtocolsWhitelist.value.includes(selectedProtocol)) {
    if (type === 'flow') {
      flowProtocolsWhitelist.value.push(selectedProtocol);
    } else {
      dataProtocolsWhitelist.value.push(selectedProtocol);
    }
  }
  if (type === 'flow') {
    selectedFlowProtocols = '';
  } else {
    selectedDataProtocols = '';
  }
};

const removeProtocol = (type, index) => {
  if (type === 'flow') {
    flowProtocolsWhitelist.value.splice(index, 1);
  } else {
    dataProtocolsWhitelist.value.splice(index, 1);
  }
};

const addPort = () => {
  const port = parseInt(selectedPort);
  if (!isNaN(port) && !portsWhitelist.value.includes(port)) {
    portsWhitelist.value.push(port);
    selectedPort = '';
  }
};

const removePort = (index) => {
  portsWhitelist.value.splice(index, 1);
};

const handleOverlayClick = (event) => {
  const form = document.querySelector('.query-condition-form');
  if (!form.contains(event.target)) {
    cancelForm();
  }
};

onMounted(() => {
  document.addEventListener('keydown', (event) => {
    if (event.key === 'Escape') {
      cancelForm();
    }
  });
});

</script>

<style scoped>
.first-tag-condition-editing-input{
  border: 1px solid #424242;
  border-radius: 4px;
  font-size: 2vh;
  width: 90%;
  padding: 2%;
  margin: 0.5vh 0;
}

.first-tag-condition-editing-input:focus {
  outline: none;
}

.whitelist-item {
  display: flex;
  align-items: center;
  margin-bottom: 5px;
  margin-left: 15px;
}

.dot {
  margin-right: 10px;
  font-size: 1.2em;
}

.protocol-text {
  margin-right: 10px;
}

.port-input-container {
  display: flex;
  align-items: center;
}

.port-input-container input {
  margin-right: 10px;
}

.port-input-container button {
  padding: 8px 16px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
}

.port-input-container button:hover {
  background-color: #f0f0f0;
}

.query-condition-form-overlay {
  font-family: 'Open Sans', sans-serif;
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
}

.query-condition-form {
  background-color: white;
  padding: 20px;
  border-radius: 5px;
  max-height: 400px;
  overflow-y: auto;
}

.title {
  font-weight: bold;
  font-size: 24px;
  margin-bottom: 10px;
}

.layout-title{
  font-size: 18px;
  margin-bottom: 15px;
}

.subtitle {
  font-size: 12px;
  margin-bottom: 10px;
  color: #666;
}

.subtitle-duplicates {
  font-size: 1.8vh;
  margin-bottom: 10px;
  color: #666;
}

.query-condition-form input,
.query-condition-form select {
  margin-bottom: 10px;
  width: 100%;
  padding: 8px;
  box-sizing: border-box;
}

.button-container {
  display: flex;
  justify-content: space-between;
  margin-top: 20px;
}

.button-container button {
  padding: 10px 20px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
}

.button-container button:hover {
  background-color: #f0f0f0;
}

.theme-checkbox, .scrollable-selector-include-exclude-traffic input[type="checkbox"] {
  position: relative;
  --toggle-size: 16px;
  -webkit-appearance: none;
  -moz-appearance: none;
  appearance: none;
  width: 2.75em;
  height: 1.625em;
  background: linear-gradient(to right, white 50%, #7EA0A9 50%) no-repeat;
  background-size: 205%;
  background-position: 0;
  -webkit-transition: 0.4s;
  -o-transition: 0.4s;
  transition: 0.4s;
  border-radius: 99em;
  cursor: pointer;
  font-size: var(--toggle-size);
  border: 1px solid #424242;
}

.theme-checkbox::before, .scrollable-selector-include-exclude-traffic input[type="checkbox"]::before {
  content: "";
  width: 1.3em;
  height: 1.3em;
  top: 50%;
  transform: translateY(-50%);
  left: 0.15em;
  position: absolute;
  background: linear-gradient(to right, white 50%, #7EA0A9 50%) no-repeat;
  background-size: 205%;
  background-position: 100%;
  border-radius: 50%;
  -webkit-transition: 0.4s;
  -o-transition: 0.4s;
  transition: 0.4s;
}

.theme-checkbox:checked::before, .scrollable-selector-include-exclude-traffic input[type="checkbox"]:checked::before {
  left: calc(100% - 1.3em - 0.1em);
  background-position: 0;
}

.theme-checkbox:checked, .scrollable-selector-include-exclude-traffic input[type="checkbox"]:checked {
  background-position: 100%;
}
.scrollable-selector-include-exclude-traffic {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  width: 70%;
  font-size: 2vh;
}
</style>
