import ApiService from "~/services/restService";

export interface FlowImport {
  dateTime: string;
  endpoints: { [key: string]: number };
}

class NetworkAnalysisService {
  public async getFlowImport() {
    return await ApiService.get<FlowImport[]>('/api/metrics/flowImporter')
      .then(x => x.data);
  }
}

export default new NetworkAnalysisService();
