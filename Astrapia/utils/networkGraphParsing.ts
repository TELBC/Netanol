import { Nodes, Edges } from "v-network-graph";
import { IEdge, INode } from "~/types/GraphData";

export function parseJsonData(rawNodes: INode[], rawEdges: IEdge[]): { nodes: Nodes; edges: Edges } {
  const nodes: Nodes = {}
  const edges: Edges = {}

  for (const [key, rawNode] of Object.entries(rawNodes)) {
    nodes[key] = {
      id: rawNode.id,
      name: rawNode.displayName,
    }
  }

  for (const [index, rawEdge] of rawEdges.entries()) {
    edges[index] = {
      source: rawEdge.sourceHostId.toString(),
      target: rawEdge.destinationHostId.toString(),
      label: rawEdge.packetCount,
    }
  }

  return { nodes, edges }
}
