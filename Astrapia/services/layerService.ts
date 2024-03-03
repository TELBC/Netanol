import ApiService from "~/services/restService";

class LayerService {
  public async getLayer(layoutName: string, index: number) {
    return await ApiService.get(`/api/layout/${layoutName}/layers/${index}`).then(x => x.data);
  }

  public async createLayer(layer: any, layoutName: string) {
    return ApiService.post(`/api/layout/${layoutName}/layers`, layer);
  }

  public async moveLayer(layoutName: string, oldIndex: number, newIndex: number) {
    return ApiService.put(`/api/layout/${layoutName}/layers/${oldIndex}/${newIndex}`);
  }

  public async editLayer(layoutName: string, index: number, layer: any) {
    return ApiService.put(`/api/layout/${layoutName}/layers/${index}`, layer);
  }

public async deleteLayer(layoutName: string, index: number) {
    return ApiService.delete(`/api/layout/${layoutName}/layers/${index}`);
  }
}

export default new LayerService();
