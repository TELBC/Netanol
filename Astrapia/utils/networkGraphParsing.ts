import { Node, Edge } from "v-network-graph";
import { IEdge, INode } from "~/types/GraphData";

export function parseJsonData(rawNodes: INode[], rawEdges: IEdge[]): { nodes: Node[]; edges: Edge[] } {

  const nodes = [...rawNodes].map((node: INode) => ({
    id: node.id,
    name: node.displayName,
  }));

  const edges = [...rawEdges].map((edge: IEdge) => ({
    source: edge.sourceHostId,
    target: edge.destinationHostId,
    label: edge.packetCount,
    byteCount: edge.byteCount,
    traceCount: edge.traceCount,
  }));

  return { nodes, edges }
}
