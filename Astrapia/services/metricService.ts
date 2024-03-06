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
}

export default new MetricService();
