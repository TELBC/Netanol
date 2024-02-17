import { library, config } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import {
  faGear,
  faBars,
  faChartLine,
  faDiagramProject,
  faHouse,
  faCircleInfo,
  faArrowsToCircle,
  faExpand,
  faChevronUp,
  faChevronDown,
  faPlus,
  faTrash,
  faPen,
  faChevronLeft,
  faChevronRight,
  faPlay,
  faPause,
  faMinus,
  faFloppyDisk,
  faArrowLeft,
} from '@fortawesome/free-solid-svg-icons'

library.add(
  faGear,
  faBars,
  faChartLine,
  faDiagramProject,
  faHouse,
  faCircleInfo,
  faArrowsToCircle,
  faExpand,
  faChevronUp,
  faChevronDown,
  faPlus,
  faTrash,
  faPen,
  faChevronLeft,
  faChevronRight,
  faPlay,
  faPause,
  faMinus,
  faFloppyDisk,
  faArrowLeft,
)

config.autoAddCss = false

export default defineNuxtPlugin((nuxtApp) => {
  nuxtApp.vueApp.component('font-awesome-icon', FontAwesomeIcon)
})
