import { defineConfigs } from "v-network-graph"
import {ForceEdgeDatum, ForceLayout, ForceNodeDatum} from "v-network-graph/lib/force-layout";

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
    },
    layoutHandler: new ForceLayout({
        createSimulation: (d3, nodes, edges) => {
          const forceLink = d3.forceLink<ForceNodeDatum, ForceEdgeDatum>(edges).id(d => d.id)
          return d3.forceSimulation(nodes)
            .force("edge", forceLink.distance(100).strength(2))
            .force("charge", d3.forceManyBody().strength(-500))
            .alphaMin(0.1)
        }
      }
    )
  },
})
