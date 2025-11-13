// BŁĄD TS2307: Ten błąd oznacza, że TypeScript nie może znaleźć typów dla biblioteki axios.
// Aby to naprawić, upewnij się, że masz zainstalowany pakiet. Uruchom w terminalu:
// npm install axios
import axios, {
  type InternalAxiosRequestConfig,
  type AxiosResponse,
  type AxiosRequestConfig,
} from 'axios'
import apiConfig from './apiConfig'

// 1. Utworzenie skonfigurowanej instancji axios
const apiClient = axios.create({
  baseURL: apiConfig.baseURL,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
})

// POPRAWKA: Dodajemy typ do parametru `config`
apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  // Przykładowa logika dodawania tokenu autoryzacyjnego
  // const token = localStorage.getItem('token');
  // if (token) {
  //   config.headers.Authorization = `Bearer ${token}`;
  // }
  return config
})

// Definiujemy interfejs dla naszego serwisu, aby zapewnić spójność
interface ApiService {
  get: <T>(endpoint: string, params?: any) => Promise<AxiosResponse<T>>
  post: <T>(endpoint: string, data: any, config?: AxiosRequestConfig) => Promise<AxiosResponse<T>>
  put: <T>(endpoint: string, data: any) => Promise<AxiosResponse<T>>
  delete: <T>(endpoint: string) => Promise<AxiosResponse<T>>
  getFile: (endpoint: string, params?: any) => Promise<AxiosResponse<Blob>>
  postForFile: (endpoint: string, data?: any) => Promise<AxiosResponse<Blob>>
}

// 2. Eksport metod, które używają skonfigurowanej instancji
const apiServices: ApiService = {
  // POPRAWKA: Dodajemy typy i generyk <T> dla lepszego bezpieczeństwa typów.
  // Zmieniono także sposób przekazywania parametrów, aby był zgodny z axios.
  get<T>(endpoint: string, params: any = {}) {
    return apiClient.get<T>(endpoint, { params })
  },

  post<T>(endpoint: string, data: any, config?: AxiosRequestConfig) {
    return apiClient.post<T>(endpoint, data, config)
  },

  put<T>(endpoint: string, data: any) {
    return apiClient.put<T>(endpoint, data)
  },

  delete<T>(endpoint: string) {
    return apiClient.delete<T>(endpoint)
  },

  getFile(endpoint: string, params: any = {}) {
    return apiClient.get<Blob>(endpoint, {
      params: params,
      responseType: 'blob',
    })
  },

  postForFile(endpoint: string, data: any = {}) {
    return apiClient.post<Blob>(endpoint, data, {
      responseType: 'blob',
    })
  },
}

export default apiServices
