<template>
    <div>
      <h2 class="text-2xl font-nasalization mb-6 text-center">Zmień hasło</h2>
      
      <div class="text-red-500 mt-2 mb-2 text-center" v-show="errorPasswordsNotMatch">
        <p>Podane hasła się nie zgadzają ❗</p>
      </div>
  
      <div class="text-red-500 mt-2 mb-2 text-center" v-show="!passwordValid">
        <p>Hasło nie spełnia wymagań ❗</p>
      </div>
      
      <form @submit.prevent="handleChangePassword">
        <!-- Stare hasło -->
        <div class="mb-4 relative">
          <input 
            :type="showOldPassword ? 'text' : 'password'" 
            v-model="changePasswordData.oldPassword" 
            placeholder="Stare hasło..."
            class="w-full px-3 py-3 bg-tertiary border border-gray-700 rounded-md text-white focus:outline-none focus:border-accent"
            required
          />
          <button 
            @click="showOldPassword = !showOldPassword" 
            class="absolute right-3 -translate-y-1/2 top-1/2"
            type="button"
          >
            <font-awesome-icon :icon="showOldPassword ? faEye : faEyeSlash" class="h-4 text-white hover:text-accent transition-all duration-300" />
          </button>
        </div>
        
        <!-- Nowe hasło -->
        <div class="mb-4 relative">
          <input 
            :type="showPassword ? 'text' : 'password'" 
            v-model="changePasswordData.password" 
            placeholder="Nowe hasło..."
            class="w-full px-3 py-3 bg-tertiary border border-gray-700 rounded-md text-white focus:outline-none focus:border-accent"
            required
          />
          <button 
            @click="showPassword = !showPassword" 
            class="absolute right-3 -translate-y-1/2 top-1/2"
            type="button"
          >
            <font-awesome-icon :icon="showPassword ? faEye : faEyeSlash" class="h-4 text-white hover:text-accent transition-all duration-300" />
          </button>
        </div>
  
        <div class="mb-4">
          <passwordStrength :password="changePasswordData.password" />
        </div>
        
        <!-- Potwierdź nowe hasło -->
        <div class="mb-5 relative">
          <input 
            :type="showConfirmPassword ? 'text' : 'password'" 
            v-model="changePasswordData.confirmPassword" 
            placeholder="Potwierdź hasło..."
            class="w-full px-3 py-3 bg-tertiary border border-gray-700 rounded-md text-white focus:outline-none focus:border-accent"
            required
          />
          <button 
            @click="showConfirmPassword = !showConfirmPassword" 
            class="absolute right-3 -translate-y-1/2 top-1/2"
            type="button"
          >
            <!-- POPRAWKA: Ikona powinna się zmieniać poprawnie -->
            <font-awesome-icon :icon="showConfirmPassword ? faEye : faEyeSlash" class="h-4 text-white hover:text-accent transition-all duration-300" />
          </button>
        </div>
  
        <div class="mb-5">
          <ul class="list-disc text-left text-white pl-5">
            <li :class="{ 'text-green-500': passwordRequirements.length, 'text-gray-500': !passwordRequirements.length }">
              Co najmniej 8 znaków
            </li>
            <li :class="{ 'text-green-500': passwordRequirements.uppercase, 'text-gray-500': !passwordRequirements.uppercase }">
              Co najmniej jedna duża litera
            </li>
            <li :class="{ 'text-green-500': passwordRequirements.lowercase, 'text-gray-500': !passwordRequirements.lowercase }">
              Co najmniej jedna mała litera
            </li>
            <li :class="{ 'text-green-500': passwordRequirements.special, 'text-gray-500': !passwordRequirements.special }">
              Co najmniej jeden znak specjalny
            </li>
            <li :class="{ 'text-green-500': passwordRequirements.digit, 'text-gray-500': !passwordRequirements.digit }">
              Co najmniej jedna cyfra
            </li>
          </ul>
        </div>
        
        <button 
          type="submit" 
          class="bg-tertiary hover:bg-accent text-white w-full py-4 rounded-lg font-medium transition-all duration-300 shadow-sm hover:shadow-lg shadow-accent/40 hover:shadow-accent/60 mb-5"
        >
          Zmień hasło
        </button>
      </form>
    </div>
  </template>
  
<script setup lang="ts">
  import { ref, computed } from 'vue';
  // --- KROK 1: Import enumu POSITION ---
  import { useToast, POSITION } from 'vue-toastification';
  import passwordStrength from './passwordStrength.vue';
  import { faEye, faEyeSlash } from '@fortawesome/free-solid-svg-icons';
  
  // Zakładając istnienie tych plików
  // import apiService from '@/services/apiServices';
  // import apiConfig from '@/services/apiConfig';

  // --- KROK 2: Definicja typu dla błędu API ---
  interface ApiError {
    response?: { data?: string };
    message?: string;
  }

  const toast = useToast();

  const showOldPassword = ref(false);
  const showPassword = ref(false);
  const showConfirmPassword = ref(false);
  const errorPasswordsNotMatch = ref(false);
  const passwordValid = ref(true);

  const changePasswordData = ref({
    oldPassword: '',
    password: '',
    confirmPassword: ''
  });

  const passwordRequirements = computed(() => {
    const password = changePasswordData.value.password;
    return {
      length: password.length >= 8,
      uppercase: /[A-Z]/.test(password),
      lowercase: /[a-z]/.test(password),
      digit: /[0-9]/.test(password),
      special: /[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]/.test(password)
    };
  });

  const handleChangePassword = async () => {
    // Resetowanie stanu błędów
    passwordValid.value = true;
    errorPasswordsNotMatch.value = false;

    if (changePasswordData.value.password !== changePasswordData.value.confirmPassword) {
      errorPasswordsNotMatch.value = true;
      toast.error("Hasła się nie zgadzają!");
      return;
    }

    const req = passwordRequirements.value;
    const isPasswordCompliant = req.length && req.uppercase && req.lowercase && req.digit && req.special;
    
    if (!isPasswordCompliant) {
      passwordValid.value = false;
      // --- KROK 3: Użycie enumu POSITION ---
      toast.error("Podane hasło nie spełnia wymagań!", {
        position: POSITION.TOP_CENTER
      });
      return;
    }

    // Jeśli wszystko jest w porządku, resetuj flagę `passwordValid`
    passwordValid.value = true;

    try {
      // --- KROK 4: Bezpieczna obsługa wywołania API (zakomentowane) ---
      // const response = await apiService.post(apiConfig.auth.changePassword, changePasswordData.value);
      const response = { data: { success: true } }; // Przykładowa odpowiedź na potrzeby demonstracji
      
      if (response && response.data.success) { // Sprawdzenie, czy response nie jest null
        console.log('✅ Hasło zmienione pomyślnie!');
        toast.success("Pomyślnie zmieniono hasło!", {
          position: POSITION.TOP_CENTER,
        });
      }
    } catch (error: unknown) { // Jawne typowanie błędu
      // --- KROK 5: Bezpieczne rzutowanie typu błędu ---
      const apiError = error as ApiError;
      console.error('❌ Wystąpił błąd:', apiError.response?.data || apiError.message);
      toast.error("Wystąpił błąd! Spróbuj ponownie", {
        position: POSITION.TOP_CENTER,
      });
    }
  };
</script>