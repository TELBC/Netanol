import { Nodes, Edges } from "v-network-graph"
export function parseJsonData(jsonData: string): { nodes: Nodes; edges: Edges } {
  const data = JSON.parse(jsonData);

  const nodes = {};
  for (const nodeId in data.nodes) {
    const node = data.nodes[nodeId];
    nodes[`node${nodeId}`] = { name: node.ipAddress, ipAddress: node.ipAddress };
  }
  const edges = {};
  data.traces.forEach((trace, index) => {
    edges[`edge${index}`] = {
      source: `node${trace.sourceHostId}`,
      target: `node${trace.destinationHostId}`,
      count: trace.count
    };
  });

  return { nodes, edges };
}
