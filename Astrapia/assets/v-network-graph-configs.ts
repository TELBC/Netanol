import { defineConfigs } from "v-network-graph"
import {ForceEdgeDatum, ForceLayout, ForceNodeDatum} from "v-network-graph/lib/force-layout";

export const networkGraphConfigs = defineConfigs({
  node: {
    normal: {
      color: "#537B87",
    },
    hover: {
      color: "#3E6474",
    },
    label: {
      fontFamily: "'Open Sans', sans-serif",
      directionAutoAdjustment: true,
      fontSize: 13,
    }
  },
  edge: {
    hoverable: true,
    normal: {
      color: "#7EA0A9",
    },
    hover: {
      color: "#7095AB",
    },
    label: {
      background: "transparent",
      fontFamily: "'Open Sans', sans-serif",
      directionAutoAdjustment: true,
      fontSize: 12,
    },
    marker: {
      target: {
        type: "arrow",
        width: 6,
        height: 4,
        color: "#7095AB",
      },
    },
  },
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
        width: 0.2,
        dasharray: 1,
      },
      thick: {
        color: "#cccccc",
        width: 0.1,
        dasharray: 0,
      },
    },
    layoutHandler: new ForceLayout({
        createSimulation: (d3, nodes, edges) => {
          const forceLink = d3.forceLink<ForceNodeDatum, ForceEdgeDatum>(edges).id(_ => _.id)
          return d3.forceSimulation(nodes)
            .force("edge", forceLink.distance(400).strength(2))
            .force("charge", d3.forceManyBody().strength(-400))
        }
      }
    )
  },

})
