import ApiService from "~/services/restService";

export interface FilterConditions {
  sourceAddress: string,
  sourceAddressMask: string,
  sourcePort: number,
  destinationAddress: string,
  destinationAddressMask: string,
  destinationPort: number,
  protocol: string,
  include: boolean
}

export interface Layer {
  name: string,
  type: string,
  enabled: boolean,
  filterList: {
    conditions: FilterConditions[],
    implicitInclude: boolean
  }
}

class LayerService {
  public async getLayer(layoutName: string, index: number) {
    return await ApiService.get<Layer>(`/api/layout/${layoutName}/layers/${index}`).then(x => x.data);
  }

  public async createLayer(layer: Layer, layoutName: string) {
    return ApiService.post(`/api/layout/${layoutName}/layers`, layer);
  }

  public async moveLayer(layoutName: string, oldIndex: number, newIndex: number) {
    return ApiService.put(`/api/layout/${layoutName}/layers/${oldIndex}/${newIndex}`);
  }

  public async editLayer(layoutName: string, index: number) {
    return ApiService.put(`/api/layout/${layoutName}/layers/${index}`);
  }

public async deleteLayer(layoutName: string, index: number) {
    return ApiService.delete(`/api/layout/${layoutName}/layers/${index}`);
  }
}

export default new LayerService();
