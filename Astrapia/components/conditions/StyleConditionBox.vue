<template>
  <div>
    <div class="filter-condition-box-container">
      <div class="filter-condition-box-menu">Edge Styling</div>
      <div class="scrollable-selector-title">
        <label class="dropdown-title">Set Width</label>
        <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" v-model="filterConditionBoxState.editingCondition.edgeStyler.setWidth" />
      </div>
      <div v-if="filterConditionBoxState.editingCondition.edgeStyler.setWidth">
        <label class="dropdown-label">Width Type</label>
        <select class="create-dropdown-inputs" v-model="filterConditionBoxState.editingCondition.edgeStyler.widthScoringMode">
          <option value="Calculated" selected>Calculated</option>
          <option value="ByteCount">ByteCount</option>
          <option value="PacketCount">PacketCount</option>
        </select>
        <label class="dropdown-label">Min Edge Width:</label>
        <input id="source-input" class="scrollable-selector-input" type="number" step=".1" placeholder="Edge Min Width" v-model="filterConditionBoxState.editingCondition.edgeStyler.edgeMinWidth" />
        <label class="dropdown-label">Max Edge Width:</label>
        <input id="source-input" class="scrollable-selector-input" type="number" step=".1" placeholder="Edge Min Width" v-model="filterConditionBoxState.editingCondition.edgeStyler.edgeMaxWidth" />
      </div>
      <div class="section-seperator"/>
      <div class="scrollable-selector-title">
        <label class="dropdown-title">Set Color</label>
        <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" v-model="filterConditionBoxState.editingCondition.edgeStyler.setColor" />
      </div>
      <div v-if="filterConditionBoxState.editingCondition.edgeStyler.setColor">
        <label class="dropdown-label">Color Type</label>
        <select class="create-dropdown-inputs" v-model="filterConditionBoxState.editingCondition.edgeStyler.colorScoringMode">
          <option value="Calculated">Calculated</option>
          <option value="ByteCount" selected>ByteCount</option>
          <option value="PacketCount">PacketCount</option>
        </select>
        <div class="scrollable-selector-include-exclude-traffic">
          <label class="dropdown-label">Interpolate Colors</label>
          <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" v-model="filterConditionBoxState.editingCondition.edgeStyler.interpolateColors" />
        </div>
        <div class="scrollable-selector-include-exclude-traffic">
          <label class="dropdown-label">Use Protocol Colors</label>
          <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" v-model="filterConditionBoxState.editingCondition.edgeStyler.useProtocolColors" />
        </div>
        <div v-if="filterConditionBoxState.editingCondition.edgeStyler.useProtocolColors">
          <div class="protocol-colors-selection">
            <div class="grid-container">
              <div class="grid-item header">Protocol</div>
              <div class="grid-item header">Start Color</div>
              <div class="grid-item header">End Color</div>

              <div class="grid-label">
                <label class="dropdown-label">Unknown</label>
              </div>
              <div class="grid-item">
                <input class="selector-color-input" type="color" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Unknown.startHex" title="Start Color"/>
              </div>
              <div class="grid-item">
                <input class="selector-color-input" type="color" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Unknown.endHex" title="End Color"/>
              </div>

              <div class="grid-label">
                <label class="dropdown-label">TCP</label>
              </div>
              <div class="grid-item">
                <input class="selector-color-input" type="color" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Tcp.startHex" title="Start Color"/>
              </div>
              <div class="grid-item">
                <input class="selector-color-input" type="color" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Tcp.endHex" title="End Color"/>
              </div>

              <div class="grid-label">
                <label class="dropdown-label">UDP</label>
              </div>
              <div class="grid-item">
                <input class="selector-color-input" type="color" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Udp.startHex" title="Start Color"/>
              </div>
              <div class="grid-item">
                <input class="selector-color-input" type="color" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Udp.endHex" title="End Color"/>
              </div>

              <div class="grid-label">
                <label class="dropdown-label">ICMP</label>
              </div>
              <div class="grid-item">
                <input class="selector-color-input" type="color" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Icmp.startHex" title="Start Color"/>
              </div>
              <div class="grid-item">
                <input class="selector-color-input" type="color" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Icmp.endHex" title="End Color"/>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div class="filter-condition-box-container">
      <div class="filter-condition-box-menu">Node Styling</div>
      <div class="scrollable-selector-title">
        <label class="dropdown-title">Set Color</label>
        <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" v-model="filterConditionBoxState.editingCondition.nodeStyler.setColor" />
        <div v-if="filterConditionBoxState.editingCondition.nodeStyler.setColor">

        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {ref, watch} from "vue";

const props = defineProps<{
  editLayerFilterConditions: Matcher,
}>();

interface Matcher {
  edgeStyler: {
    setWidth: boolean,
    widthScoringMode: string,
    edgeMinWidth: number,
    edgeMaxWidth: number,
    setColor: boolean,
    colorScoringMode: string,
    interpolateColors: boolean,
    useProtocolColors: boolean,
    protocolColors: {
      [key:string]: {
        startHex: string,
        endHex: string
      }
    }
  }
  nodeStyler: {
    setColor: boolean,
    assignments: [
      {
        matcher: {
          address: string,
          mask: string,
          include: boolean
        },
        hexColor: string
      }
    ]
  }
}

const filterConditionBoxState = ref({
  isEditing: true,
  filterConditionSelected: -1,
  editingCondition: {
    edgeStyler: {
      setWidth: false,
      widthScoringMode: "Calculated",
      edgeMinWidth: 0.5,
      edgeMaxWidth: 2.0,
      setColor: false,
      colorScoringMode: "ByteCount",
      interpolateColors: false,
      useProtocolColors: true,
      protocolColors: {
        Unknown:{
          startHex: "#000000",
          endHex: "#FFFFFF"
        },
        Tcp:{
          startHex: "#000000",
          endHex: "#FFFFFF"
        },
        Udp:{
          startHex: "#000000",
          endHex: "#FFFFFF"
        },
        Icmp:{
          startHex: "#000000",
          endHex: "#FFFFFF"
        }
      }
    },
    nodeStyler: {
      setColor: false,
      assignments: [
        {
          matcher: {
            address: "0.0.0.0",
            mask: "0.0.0.0",
            include: false
          },
          hexColor: "#00FF00"
        }
      ]
    }
  } as Matcher,
  editingConditionIndex: -1,
})

const emit = defineEmits({
  'update-styling-conditions': (payload: Matcher) => true,
});


// receive filter conditions from LayerManagement on edit of existing layer
watch(() => props.editLayerFilterConditions, (newVal) => {
  filterConditionBoxState.value.editingCondition = newVal;
});
</script>

<style scoped>
.protocol-colors-selection{
  margin: 2% 2.5% 5%;
}

.grid-container {
  display: grid;
  grid-template-columns: auto auto auto;
  grid-gap: 10px;
  font-family: "Open Sans", sans-serif;
  font-size: 1.4vh;
  background: white;
  color: #424242;
  position: relative;
}

.grid-item {
  text-align: center;
}

.grid-label {
  text-align: left;
}

.header {
  font-weight: bold;
  text-align: left;
  font-size: 1.3vh;
  margin-left: 5%;
}

.selector-color-input{
  width: 2vh;
  height: 2vh;
  padding: 0;
  margin-left: 2%;
  margin-right: 2%;
  border: none;
  vertical-align: middle;
}

.filter-condition-box-container {
  display: flex;
  flex-direction: column;
  border: 1px solid #424242;
  height: auto;
  width: 90%;
  border-radius: 4px;
  font-family: 'Open Sans', sans-serif;
  overflow: hidden;
  margin-bottom: 15px;
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

.scrollable-selector-input {
  border: none;
  width: 90%;
  font-size: 1.5vh;
  border-bottom: 1px solid #e0e0e0;
  padding: 0.25vh 0;
  margin: 0.5vh 5%;
}

.scrollable-selector-input:focus {
  outline: none;
}

.scrollable-selector-include-exclude-traffic {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  width: 80%;
  font-size: 2vh;
  margin-left: 1%;
}

.section-seperator{
  border-top: 1px solid #b7b7b7;
  width: 95%;
  height: 1px;
  margin-left: 2.5%;
  margin-right: 2.5%;
  margin-top: 2%;
}

.scrollable-selector-title {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  width: 80%;
  font-size: 2vh;
  margin-left: 6%;
  margin-top: 3%;
  margin-bottom: 2%;
}

#exclude-include-switch {
  margin-left: 0;
}

.create-dropdown-inputs {
  font-family: "Open Sans", sans-serif;
  width: 55%;
  border: 1px solid #424242;
  border-radius: 4px;
  font-size: 1.5vh;
  padding: 2%;
  background: white;
  color: black;
  margin: 0.5vh 0 0.5vh 5%;
  vertical-align: middle;
}

.create-dropdown-inputs:focus {
  outline: none;
}

.dropdown-title{
  font-family: "Open Sans", sans-serif;
  width: 90%;
  font-size: 1.8vh;
  background: white;
  color: #424242;
}

.dropdown-label{
  font-family: "Open Sans", sans-serif;
  width: 90%;
  font-size: 1.4vh;
  background: white;
  color: #424242;
  margin-left: 5%;
}
</style>
