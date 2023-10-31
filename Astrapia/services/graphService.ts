import {IDateRange, IGraph, ILayout} from '~/types/GraphData';
import ApiService from '~/services/apiService';
import {AxiosError} from "axios";

export async function fetchGraphData(dateRange: IDateRange, layoutName:String): Promise<IGraph> {
  try {
    const response = await ApiService.request({
      method: 'post',
      url: `/api/graph/${layoutName}`,
      data: dateRange,
      headers: { Authorization: `Bearer ${sessionStorage.getItem('jwtToken')}` }
    });

    return response.data;
  } catch (error: AxiosError) {
    throw new Error(`Failed to fetch data: ${error.message}`);
  }
}

async function layoutExists(name: String): Promise<boolean> {
  try {
    const response = await ApiService.request({
      method: 'get',
      url: `/api/layout`,
      headers: { Authorization: `Bearer ${sessionStorage.getItem('jwtToken')}` }
    });
    return response.data.find((layout: { name: String; }) => layout.name === name) !== undefined;
  } catch (error) {
    return false;
  }
}

export async function createLayout(name: string): Promise<ILayout> {
  try {
    const layoutAlreadyExists = await layoutExists(name);

    if (layoutAlreadyExists) {
      return {} as IGraph;
    }
    const response = await ApiService.request({
      method: 'post',
      url: `/api/layout/${name}`,
      headers: { Authorization: `Bearer ${sessionStorage.getItem('jwtToken')}` }
    });

    return response.data;
  } catch (error: AxiosError) {
    throw new Error(`Failed to create layout: ${error.message}`);
  }
}
