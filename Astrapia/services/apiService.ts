import axios, {AxiosRequestConfig, AxiosResponse} from 'axios';

const runtimeConfig = useRuntimeConfig();
const baseApiUrl = runtimeConfig.public.apiBase;

export default class ApiService {
  static async request(config: AxiosRequestConfig): Promise<AxiosResponse> {
    try {
      return await axios({
        baseURL: baseApiUrl,
        ...config,
      });
    } catch (error) {
      throw error;
    }
  }
}
