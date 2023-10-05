import {IDateRange, IGraph, ILayout} from '~/types/GraphData';
import ApiService from '~/services/apiService';

export async function fetchGraphData(dateRange: IDateRange, layoutName:String): Promise<IGraph> {
  try {
    const response = await ApiService.request({
      method: 'post',
      url: `/graph/${layoutName}`,
      data: dateRange,
      headers: {
        'Content-Type': 'application/json',
        Accept: '*/*',
      },
    });

    return response.data;
  } catch (error) {
    throw new Error(`Failed to fetch data: ${error.message}`);
  }
}

async function layoutExists(name: String): Promise<boolean> {
  try {
    const response = await ApiService.request({
      method: 'get',
      url: `/layout`,
      headers: {
        'Content-Type': 'application/json',
        Accept: '*/*',
      },
    });
    return response.data.find((layout) => layout.name === name) !== undefined;
  } catch (error) {
    return false;
  }
}

export async function createLayout(name: string): Promise<IGraph> {
  try {
    const layoutAlreadyExists = await layoutExists(name);

    if (layoutAlreadyExists) {
      console.log(`Layout '${name}' already exists.`);
      return {} as IGraph;
    }
    const response = await ApiService.request({
      method: 'post',
      url: `/layout/${name}`,
      headers: {
        'Content-Type': 'application/json',
        Accept: '*/*',
      },
    });

    return response.data;
  } catch (error) {
    throw new Error(`Failed to create layout: ${error.message}`);
  }
}
