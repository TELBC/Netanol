import ApiService from "~/services/restService";
import {Edge, Node} from "v-network-graph";

interface GraphNode extends Node {
  displayName: string
}

interface GraphEdge extends Edge {
  packetCount: number,
  byteCount: number
}

interface GraphResponse {
  nodes: GraphNode[],
  edges: GraphEdge[]
}

/**
 * Handles logic for the /topology page.
 */
class TopologyService {
  /**
   * Get the topology for a specific layout and time range.
   */
  public async getTopology(layout: string, from: Date, to: Date): Promise<GraphResponse> {
    return await ApiService.post<GraphResponse>(`/api/graph/${layout}`, {
        from: from.toISOString(),
        to: to.toISOString()
    }).then(x => x.data);
  }
}

export default new TopologyService();
