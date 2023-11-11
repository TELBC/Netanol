import {defineConfigs} from "v-network-graph"
import {ForceLayout} from "v-network-graph/lib/force-layout";

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
      color: "#ff1500",
    },
    label: {
      background: "transparent",
      fontFamily: "'Open Sans', sans-serif",
      color: "#ff1500",
      directionAutoAdjustment: true,
      fontSize: 15,
    },
    marker: {
      target: {
        type: "arrow",
        width: 6,
        height: 4,
      },
    },
  },
  view: {
    minZoomLevel: 1,
    maxZoomLevel: 8,
    scalingObjects: true,
    autoPanAndZoomOnLoad: "fit-content",
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
        return d3.forceSimulation(nodes)
            .force("link", d3.forceLink(edges).id((d: { id: any; }) => d.id).distance(100).strength(100))
            .force("charge", d3.forceManyBody().strength(-1000))
            .force("x", d3.forceX())
            .force("y", d3.forceY());
        }
      }
    )
  }
})
