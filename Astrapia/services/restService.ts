import axios, {
  type AxiosInstance,
  type AxiosRequestConfig,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from 'axios';
import {useAuth} from "~/composables/auth";

/**
 * Central service to handle all HTTP requests to the backend.
 *
 * Automatically adds the JWT token to all outbound requests once
 * it has been set to a non-null value.
 */
class RestService {
  private axiosInstance: AxiosInstance;

  constructor() {
    this.axiosInstance = axios.create();

    this.axiosInstance.interceptors.request.use(this.handleRequest, this.handleError);
    this.axiosInstance.interceptors.response.use(this.handleResponse, this.handleError);
  }

  private handleRequest = (config: InternalAxiosRequestConfig<any>): InternalAxiosRequestConfig<any> => {
    // config.url = `/api${config.url}`
    return config
  }

  private handleResponse = (response: AxiosResponse): AxiosResponse => {
    return response;
  }

  private handleError = (error: any): Promise<any> => {
    if (error.response.status == 401) {
      const auth = useAuth();
      auth.value.isAuthenticated = false;
    }
    return Promise.reject(error);
  }

  /**
   * Performs a GET request and returns data of the specified type.
   */
  public async get<T>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T, any>> {
    return await this.axiosInstance.get<T>(url, config);
  }

  /**
   * Performs a POST request and returns data of the specified type.
   */
  public async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<AxiosResponse<T, any>> {
    return await this.axiosInstance.post<T>(url, data, config);
  }
}

export default new RestService();
