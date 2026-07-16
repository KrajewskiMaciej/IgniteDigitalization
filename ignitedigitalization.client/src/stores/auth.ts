import { defineStore } from 'pinia'
import apiServices from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'

interface AuthState {
  isAuthenticated: boolean
  role: number | null
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    isAuthenticated: false,
    role: null,
  }),
  actions: {
    async checkAuth() {
      try {
        const res = await apiServices.get<{ role: number }>(apiConfig.auth.me)
        this.isAuthenticated = true
        this.role = res.data.role
      } catch {
        this.isAuthenticated = false
        this.role = null
      }
    },
    setAuthenticated(value: boolean) {
      this.isAuthenticated = value
    },
  },
})
