<template>
  <div>
    <div class="filter-condition-box-container">
      <div class="filter-condition-box-menu">Edge Styling</div>
      <div class="scrollable-selector-title">
        <label class="dropdown-title">Set Width</label>
        <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.setWidth" />
      </div>
      <div v-if="filterConditionBoxState.editingCondition.edgeStyler.setWidth">
        <label class="dropdown-label">Width Type</label>
        <select class="create-dropdown-inputs" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.widthScoringMode">
          <option value="Calculated" selected>Calculated</option>
          <option value="ByteCount">ByteCount</option>
          <option value="PacketCount">PacketCount</option>
        </select>
        <label class="dropdown-label">Min Edge Width:</label>
        <input id="source-input" class="scrollable-selector-input" type="number" step=".1" placeholder="Edge Min Width" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.edgeMinWidth" />
        <label class="dropdown-label">Max Edge Width:</label>
        <input id="source-input" class="scrollable-selector-input" type="number" step=".1" placeholder="Edge Min Width" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.edgeMaxWidth" />
      </div>
      <div class="section-seperator"/>
      <div class="scrollable-selector-title">
        <label class="dropdown-title">Set Color</label>
        <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.setColor" />
      </div>
      <div v-if="filterConditionBoxState.editingCondition.edgeStyler.setColor">
        <label class="dropdown-label">Color Type</label>
        <select class="create-dropdown-inputs" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.colorScoringMode">
          <option value="Calculated">Calculated</option>
          <option value="ByteCount" selected>ByteCount</option>
          <option value="PacketCount">PacketCount</option>
        </select>
        <div class="scrollable-selector-include-exclude-traffic">
          <label class="dropdown-label">Interpolate Colors</label>
          <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.interpolateColors" />
        </div>
        <div class="scrollable-selector-include-exclude-traffic">
          <label class="dropdown-label">Use Protocol Colors</label>
          <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.useProtocolColors" />
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
                <input class="selector-color-input" type="color" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Unknown.startHex" title="Start Color"/>
              </div>
              <div class="grid-item">
                <input class="selector-color-input" type="color" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Unknown.endHex" title="End Color"/>
              </div>

              <div class="grid-label">
                <label class="dropdown-label">TCP</label>
              </div>
              <div class="grid-item">
                <input class="selector-color-input" type="color" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Tcp.startHex" title="Start Color"/>
              </div>
              <div class="grid-item">
                <input class="selector-color-input" type="color" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Tcp.endHex" title="End Color"/>
              </div>

              <div class="grid-label">
                <label class="dropdown-label">UDP</label>
              </div>
              <div class="grid-item">
                <input class="selector-color-input" type="color" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Udp.startHex" title="Start Color"/>
              </div>
              <div class="grid-item">
                <input class="selector-color-input" type="color" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Udp.endHex" title="End Color"/>
              </div>
<!--TODO: ICMP needs to be enabled by Michal-->
<!--              <div class="grid-label">-->
<!--                <label class="dropdown-label">ICMP</label>-->
<!--              </div>-->
<!--              <div class="grid-item">-->
<!--                <input class="selector-color-input" type="color" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Icmp.startHex" title="Start Color"/>-->
<!--              </div>-->
<!--              <div class="grid-item">-->
<!--                <input class="selector-color-input" type="color" @change="changeStyling" v-model="filterConditionBoxState.editingCondition.edgeStyler.protocolColors.Icmp.endHex" title="End Color"/>-->
<!--              </div>-->
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
      </div>
      <div class="style-node-box" v-if="filterConditionBoxState.editingCondition.nodeStyler.setColor">
        <div class="naming-condition-box-menu">
          <div class="list-icons-container" v-bind:class="{'list-icons-container-editing': filterConditionBoxState.isEditing}">
            <font-awesome-icon icon="fa-solid fa-plus" class="conditions-list-icons" v-bind:class="{'conditions-list-editing': filterConditionBoxState.isEditing}" @click="toggleIsEditing('edit')" />
            <font-awesome-icon icon="fa-solid fa-minus" class="conditions-list-icons" v-bind:class="{'conditions-list-editing': filterConditionBoxState.isEditing}" @click="deleteSelectedNamingCondition" />
          </div>
          <div class="edit-icons-container" v-bind:class="{'editing-icons-container-editing': filterConditionBoxState.isEditing}">
            <font-awesome-icon icon="fa-solid fa-arrow-left" class="editing-condition-icons" v-bind:class="{'editing-condition': filterConditionBoxState.isEditing}" @click="clearEditingInputs('list')" />
            <font-awesome-icon icon="fa-solid fa-floppy-disk" class="editing-condition-icons" v-bind:class="{'editing-condition': filterConditionBoxState.isEditing}" @click="saveNamingCondition" />
          </div>
        </div>
        <div class="naming-condition-list" v-bind:class="{'editing-naming-condition-list': filterConditionBoxState.isEditing}">
          <div class="conditions" v-for="(condition, index) in filterConditionBoxState.nodeMatcher" :key="index" @click="setNamingConditionSelected(index)" v-bind:class="{'selected-naming-condition': filterConditionBoxState.filterConditionSelected == index}" @dblclick="doubleClickNamingCondition(index)">
            <div class="conditions-src-dest">
              <div class="src-dest-address-container">
                <p class="include-exclude-naming-src-dest">
                  Color:&nbsp;
                </p>
                {{ condition.hexColor }}
              </div>
              <div class="src-dest-address-container">
                <p class="include-exclude-naming-src-dest">
                  IP:&nbsp;
                </p>
                {{ condition.matcher.address }}
              </div>
            </div>
            <p class="include-exclude-naming-src-dest">
              {{ condition.matcher.include ? 'Include' : 'Exclude' }}
            </p>
          </div>
        </div>
        <div class="naming-condition-editing" v-bind:class="{'editing-naming-condition-editing': filterConditionBoxState.isEditing}">
          <div class="selector-color-input-box">
            <label class="selector-color-input-label">Interpolate Colors: </label>
            <input class="selector-color-input" type="color" v-model="filterConditionBoxState.hexColor" title="Start Color"/>
          </div>
          <input id="source-mask-input" class="scrollable-selector-input" type="text" placeholder="Address" v-model="filterConditionBoxState.address" />
          <input id="source-port-input" class="scrollable-selector-input" type="text" placeholder="Address Mask" v-model="filterConditionBoxState.mask" />
          <div class="scrollable-selector-include-exclude-traffic">
            <p>Exclude</p>
            <input id="exclude-include-switch" class="include-exclude-traffic-switch" type="checkbox" v-model="filterConditionBoxState.include" />
            <p>Include</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {ref, onMounted} from "vue";
import {FontAwesomeIcon} from "@fortawesome/vue-fontawesome";

const props = defineProps<{
  editLayerNodeConditions: any,
  editLayerEdgeConditions: any,
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
    assignments: {
      matcher: {
        address: string;
        mask: string;
        include: boolean;
      };
      hexColor: string;
    }[]
  }
}

interface NodeMatcher {
  matcher: {
    address: string,
    mask: string,
    include: boolean
  },
  hexColor: string
}

const filterConditionBoxState = ref({
  isEditing: false,
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
        // Icmp:{
        //   startHex: "#000000",
        //   endHex: "#FFFFFF"
        // }
      }
    },
    nodeStyler: {
      setColor: true,
      assignments: []
    }
  } as Matcher,
  nodeMatcher: [] as Array<NodeMatcher>,
  address: "",
  mask: "",
  include: false,
  hexColor: "#00FF00",
  editingConditionIndex: -1,
})

function toggleIsEditing(to: string) {
  filterConditionBoxState.value.isEditing = to === 'edit';
  filterConditionBoxState.value.filterConditionSelected = -1;
}

function changeStyling() {
  emit('update-styling-conditions', filterConditionBoxState.value.editingCondition);
}

function saveNamingCondition() {
  filterConditionBoxState.value.nodeMatcher.push({
    matcher: {
      address: filterConditionBoxState.value.address,
      mask: filterConditionBoxState.value.mask,
      include: filterConditionBoxState.value.include
    },
    hexColor: filterConditionBoxState.value.hexColor
  });

  filterConditionBoxState.value.editingCondition.nodeStyler.assignments = filterConditionBoxState.value.nodeMatcher;
  emit('update-styling-conditions', filterConditionBoxState.value.editingCondition);
  clearEditingInputs('list');
}

function setNamingConditionSelected(condition: number) {
  filterConditionBoxState.value.filterConditionSelected = condition;
}

function deleteSelectedNamingCondition() {
  if (filterConditionBoxState.value.filterConditionSelected != -1) {
    filterConditionBoxState.value.editingCondition.nodeStyler.assignments.splice(filterConditionBoxState.value.filterConditionSelected, 1);
    filterConditionBoxState.value.nodeMatcher.splice(filterConditionBoxState.value.filterConditionSelected, 1)
    filterConditionBoxState.value.filterConditionSelected = -1;
  }
}

function doubleClickNamingCondition(index: number) {
  toggleIsEditing('edit');
  filterConditionBoxState.value.address = filterConditionBoxState.value.nodeMatcher[index].matcher.address;
  filterConditionBoxState.value.mask = filterConditionBoxState.value.nodeMatcher[index].matcher.mask;
  filterConditionBoxState.value.hexColor = filterConditionBoxState.value.nodeMatcher[index].hexColor;
  filterConditionBoxState.value.include = filterConditionBoxState.value.nodeMatcher[index].matcher.include;
  filterConditionBoxState.value.editingConditionIndex = index;
}

function clearEditingInputs(to: string) {
  toggleIsEditing(to);
  filterConditionBoxState.value.address = "";
  filterConditionBoxState.value.mask = "";
  filterConditionBoxState.value.include = false;
  filterConditionBoxState.value.hexColor = "#00FF00";
}

const emit = defineEmits({
  'update-styling-conditions': (payload: Matcher) => true,
});

onMounted(() => {
  if (Object.keys(props.editLayerEdgeConditions).length !== 0) {
    filterConditionBoxState.value.editingCondition.edgeStyler = props.editLayerEdgeConditions;
  }
  if (Object.keys(props.editLayerNodeConditions).length !== 0) {
    filterConditionBoxState.value.editingCondition.nodeStyler = props.editLayerNodeConditions;
    filterConditionBoxState.value.nodeMatcher = props.editLayerNodeConditions.assignments;
  }
});
</script>

<style scoped>
.naming-condition-box-menu {
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

.selected-naming-condition {
  background-color: #e0e0e0;
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

.edit-icons-container {
  display: flex;
}

.editing-icons-container-editing {
  width: 100%;
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

.naming-condition-list {
  width: 100%;
  height: 100%;
  visibility: visible;
  opacity: 1;
  transition: 0.2s ease-in-out;
  overflow-y: auto;
  overflow-x: hidden;
  word-break: break-word;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.editing-naming-condition-list {
  width: 0;
  visibility: hidden;
  opacity: 0;
  height: 0;
}

.conditions {
  width: 90%;
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  font-size: 2vh;
  font-weight: bolder;
  cursor: pointer;
  height: 4.9vh;
  transition: 0.2s ease-in-out;
  padding-top: 2.5%;
  padding-bottom: 2.5%;
}

.naming-condition-editing {
  visibility: hidden;
  opacity: 0;
  transition: 0.2s ease-in-out;
  overflow-y: auto;
  overflow-x: hidden;
  word-break: break-word;
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-top: 5%;
}

.editing-naming-condition-editing {
  visibility: visible;
  opacity: 1;
}

.src-dest-address-container {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: flex-start;
  height: 45%;
  width: 6vw;
}

.style-node-box {
  display: flex;
  flex-direction: column;
  border: 1px solid #424242;
  height: 35vh;
  width: 90%;
  border-radius: 4px;
  font-family: 'Open Sans', sans-serif;
  overflow: hidden;
  margin-left: 5%;
  margin-bottom: 5%;
}

.include-exclude-naming-src-dest {
  font-size: 1.2vh;
  font-weight: normal;
  margin-left: 1.5%;
}

.conditions {
  width: 90%;
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  font-size: 1.4vh;
  cursor: pointer;
  height: 4.9vh;
  transition: 0.2s ease-in-out;
  padding-top: 2.5%;
  padding-right: 2.5%;
  padding-left: 2.5%;
}

.conditions-src-dest {
  display: flex;
  flex-direction: column;
  height: 100%;
}

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
  font-size: 1.4vh;
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

.selector-color-input-label{
  font-family: "Open Sans", sans-serif;
  font-size: 1.4vh;
  background: white;
  color: #424242;
  margin-left: 5%;
  margin-right: 10%;
  font-weight: bold;
}

.selector-color-input-box{
  width: 100%;
}
</style>
