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
  public async createLayer(layer: Layer, layoutName: string) {
    return ApiService.post(`/api/layout/${layoutName}/layers`, layer);
  }


}
