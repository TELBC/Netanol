export interface IGraph {
  graphStatistics: {
    totalHostCount: number,
    totalByteCount: number,
    totalPacketCount: number,
    totalTraceCount: number
  },
  requestStatistics: {
    newHostCount: number,
    processingTime: string
  },
  nodes: INode[],
  edges: IEdge[]
}

export interface INode {
  id: number,
  displayName: string
}

export interface IEdge {
  sourceHostId: number,
  destinationHostId: number,
  packetCount: number,
  byteCount: number,
  traceCount: number
}

export interface ILayout {
  id: number,
  name: string,
  graphNodes: any // TODO object is not specified
}

export interface IDateRange {
  from: string;
  to: string;
}
