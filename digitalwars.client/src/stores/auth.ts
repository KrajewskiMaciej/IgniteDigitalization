import { defineStore } from 'pinia'
import apiServices from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'

interface AuthState {
  isAuthenticated: boolean
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    isAuthenticated: false,
  }),
  actions: {
    async checkAuth() {
      try {
        await apiServices.get(apiConfig.auth.me)
        this.isAuthenticated = true
      } catch {
        this.isAuthenticated = false
      }
    },
    setAuthenticated(value: boolean) {
      this.isAuthenticated = value
    },
  },
})
