import {IEdge, INode} from "~/types/GraphData";
import {Edges, Nodes} from "v-network-graph";

export function parseJsonData(rawNodes: INode[], rawEdges: IEdge[]): { nodes: Nodes; edges: Edges } {
  const nodes: Nodes = {};
  const edges: Edges = {};

  rawNodes.forEach((rawNode) => {
    nodes[rawNode.id] = {
      id: rawNode.id,
      name: rawNode.displayName,
    };
  });

  rawEdges.forEach((rawEdge, index) => {
    edges[index] = {
      source: rawEdge.sourceHostId.toString(),
      target: rawEdge.destinationHostId.toString(),
      label: rawEdge.packetCount,
    };
  });

  return { nodes, edges };
}
