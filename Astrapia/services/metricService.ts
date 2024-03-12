import ApiService from "~/services/restService";

export interface FlowImport {
  dateTime: string;
  endpoints: { [key: string]: number };
}

export interface GeneralFlowImporterData {
  receivedPacketCount: number,
  receivedByteCount: number,
  transmittedPacketCount: number,
  transmittedByteCount: number,
  successfullyParsedPacket: number,
  failedParsedPacket: number
}

export interface GeneralFlowImporterDataDictionary {
  [key: string]: GeneralFlowImporterData;
}

export interface SelfTestData {
  database: {
    reachable: boolean;
    latency: string;
    totalSingleTraceCount: number;
    totalDatabaseSize: string;
  };
  counting: {
    totalEntriesSinceUptime: number;
    totalCounts: {
      "Netflow5": number;
      "Netflow9": number;
      "IPFIX": number;
      "sFlow": number;
    };
  };
  "multiplexers": {
    [port: string]: {
      enabled: boolean;
      name: string;
      port: string;
      acceptedProtocols: string;
    };
  };
  config: {
    "VMWare Tagging": {
      enabled: boolean;
      targetServer: string;
      cachedTagsCount: number;
      refreshPeriod: string;
      timeUntilNextRefresh: string;
    };
    "DNS Server": {
      enabled: boolean;
      cachedEntriesCount: number;
      refreshPeriod: string;
      timeUntilNextRefresh: string;
    };
    "Duplicate Flagging": {
      claimLifetime: string;
      refreshPeriod: string;
    };
  };
  statistics: {
    startTime: string;
    upTime: string;
  };

}

class MetricService {
  public async getFlowImport(from?: string, to?: string) {
    let url = '/api/metrics/flowsSeries';
    if(from && to) {
      url += `?from=${from}&to=${to}`;
    }
    return await ApiService.get<FlowImport[]>(url)
      .then(x => x.data);
  }

  public async getGeneralFlowImporterData() {
    return await ApiService.get<GeneralFlowImporterDataDictionary>('/api/metrics/flowAggregated')
      .then(x => x.data);
  }

  public async getApplicationStatusData() {
    return await ApiService.get<SelfTestData>('/api/metrics/applicationStatus')
      .then(x => x.data);
  }
}

export default new MetricService();
