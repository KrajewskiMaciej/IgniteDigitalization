// BŁĄD TS2307: Ten błąd oznacza, że TypeScript nie może znaleźć typów dla biblioteki Pinia.
// Aby to naprawić, upewnij się, że masz zainstalowany pakiet. Uruchom w terminalu:
// npm install pinia
import { defineStore } from 'pinia';
import apiServices from '@/services/apiServices';
import apiConfig from '@/services/apiConfig';

// POPRAWKA: Definiujemy interfejs dla stanu, aby zapewnić ścisłe typowanie.
interface AuthState {
  isAuthenticated: boolean;
}

export const useAuthStore = defineStore('auth', {
  // POPRAWKA: Jawnie określamy, że funkcja state zwraca obiekt zgodny z interfejsem AuthState.
  state: (): AuthState => ({
    isAuthenticated: false,
  }),
  actions: {
    async checkAuth() {
      try {
        await apiServices.get(apiConfig.auth.me);
        // POPRAWKA: Teraz `this` jest poprawnie rozpoznawane i ma dostęp do `isAuthenticated` ze stanu.
        this.isAuthenticated = true;
      } catch {
        this.isAuthenticated = false;
      }
    },
    // POPRAWKA: Dodajemy jawny typ `boolean` do parametru `value`.
    setAuthenticated(value: boolean) {
      this.isAuthenticated = value;
    },
  },
});