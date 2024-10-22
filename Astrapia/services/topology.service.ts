import ApiService from "~/services/restService";

export interface GraphNode {
  id: string
  name: string
}

export interface GraphEdge {
  source: string,
  target: string,
  packetCount: number,
  byteCount: number,
  dataProtocol: Object
}

export interface IGraphStatistics {
  totalHostCount: number,
  totalByteCount: number,
  totalPacketCount: number,
  totalTraceCount: number
}

export interface GraphResponse {
  graphStatistics: IGraphStatistics
  nodes: { [key: string]: GraphNode },
  edges: { [key: string]: GraphEdge }
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
