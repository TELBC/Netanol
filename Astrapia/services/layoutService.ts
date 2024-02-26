import ApiService from "~/services/restService";

export interface Layouts {
  name: string,
  layerCount: number
}

export interface Layout {
  name: string,
  layers: []
}

class LayoutService {
  public async getLayouts() {
    return await ApiService.get<Layouts[]>(`/api/layout`).then(x => x.data);
  }

  public async getLayoutByName(name: string) {
    return await ApiService.get<Layout>(`/api/layout/${name}`).then(x => x.data);
  }

  public createLayout(name: string) {
    return ApiService.post(`/api/layout/${name}`);
  }

  public deleteLayout(name: string) {
    return ApiService.delete(`/api/layout/${name}`);
  }

  public updateLayout(name: string, newName: string) {
    return ApiService.put(`/api/layout/${name}/${newName}`);
  }
}

export default new LayoutService();
