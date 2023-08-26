import { defineConfigs } from "v-network-graph"

export const networkGraphConfigs = defineConfigs({
  view: {
    minZoomLevel: 1,
    maxZoomLevel: 8,
    scalingObjects: true,
    grid: {
      visible: true,
      interval: 10,
      thickIncrements: 5,
      line: {
        color: "#e0e0e0",
        width: 2,
        dasharray: 1,
      },
      thick: {
        color: "#cccccc",
        width: 1,
        dasharray: 0,
      },
    }
  },
})
