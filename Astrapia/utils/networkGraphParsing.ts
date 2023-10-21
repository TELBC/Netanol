import { Nodes, Edges } from "v-network-graph";
import { IEdge, INode } from "~/types/GraphData";

export function parseJsonData(rawNodes: INode[], rawEdges: IEdge[]): { nodes: Nodes; edges: Edges } {
  const nodes: Nodes = {}
  const edges: Edges = {}
  const uniqueNodeIds = new Map<number, boolean>();

  Object.values(rawNodes).forEach((rawNode) => {
    if (!uniqueNodeIds.has(rawNode.id)) {
      uniqueNodeIds.set(rawNode.id, true);
      nodes[rawNode.id] = {
        id: rawNode.id,
        name: rawNode.displayName,
      }
    }
  });

  rawEdges.forEach((rawEdge, index) => {
    edges[index] = {
      source: rawEdge.sourceHostId.toString(),
      target: rawEdge.destinationHostId.toString(),
      label: rawEdge.packetCount,
    }
  });

  return { nodes, edges }
}
