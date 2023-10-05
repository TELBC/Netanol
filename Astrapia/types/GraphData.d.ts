export interface IGraph {
  graphStatistics: {
    TotalHostCount: number,
    TotalByteCount: number,
    TotalPacketCount: number,
    TotalTraceCount: number
  },
  requestStatistics: {
    NewHostCount: number,
    ProcessingTime: string
  },
  nodes: INode[],
  edges: IEdge[]
}

export interface INode {
  Id: number,
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
