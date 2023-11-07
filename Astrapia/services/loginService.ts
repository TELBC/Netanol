import ApiService from '~/services/apiService';
import {AxiosError} from "axios";
import {IAPIResponse} from "~/types/Login";

export async function login(username: string, password: string): Promise<IAPIResponse> {
  try {
    const response = await ApiService.request({
      method: 'post',
      url: `/api/auth/login`,
      data: { username, password }
    });

    return response.data;
  } catch (error: AxiosError) {
    throw new Error(`Failed to login: ${error.message}`);
  }
}
