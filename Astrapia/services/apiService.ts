import axios, {AxiosRequestConfig, AxiosResponse} from 'axios';

export default class ApiService {
  static async request(config: AxiosRequestConfig): Promise<AxiosResponse> {
    try {
      return await axios({
        ...config
      });
    } catch (error) {
      throw error;
    }
  }
}
