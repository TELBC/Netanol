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
  faPlay,
  faPause
} from '@fortawesome/free-solid-svg-icons'

library.add(faGear, faBars, faChartLine, faDiagramProject, faHouse, faCircleInfo, faArrowsToCircle, faExpand, faChevronUp, faChevronDown, faPlus, faTrash, faPen, faPlay, faPause)

config.autoAddCss = false

export default defineNuxtPlugin((nuxtApp) => {
  nuxtApp.vueApp.component('font-awesome-icon', FontAwesomeIcon)
})
