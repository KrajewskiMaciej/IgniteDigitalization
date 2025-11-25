<template>
  <!--Formularz rejestracji użytkownika-->
  <div class="px-3 py-2 sm:px-4 sm:py-2 md:px-6 md:py-3">
    <h2 class="text-lg sm:text-xl md:text-2xl font-nasalization mb-2 sm:mb-3 text-center">
      {{ t('createNewAccount') }}
    </h2>

    <form @submit.prevent="handleRegister" class="space-y-3 sm:space-y-4">
      <div class="space-y-1">
        <label for="register-username" class="block font-bold text-xs sm:text-sm text-left">
          {{ t('username') }}
        </label>
        <input
          type="text"
          id="register-username"
          v-model="registerData.username"
          class="w-full px-3 py-2 bg-tertiary border border-lgray-accent rounded-md text-white focus:outline-none focus:border-accent text-sm sm:text-base"
          required
        />
      </div>

      <div class="space-y-1">
        <label for="register-email" class="block font-bold text-xs sm:text-sm text-left">
          {{ t('email') }}
        </label>
        <input
          type="email"
          id="register-email"
          v-model="registerData.email"
          class="w-full px-3 py-2 bg-tertiary border border-lgray-accent rounded-md text-white focus:outline-none focus:border-accent text-sm sm:text-base"
          required
        />
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-3 sm:gap-4">
        <div class="space-y-1">
          <label for="register-password" class="block font-bold text-xs sm:text-sm text-left">
            {{ t('password') }}
          </label>
          <div
            class="flex items-center gap-2 bg-tertiary border border-lgray-accent rounded-md transition-all duration-200 focus-within:border-accent focus-within:ring-1 focus-within:ring-accent"
          >
            <input
              :type="showPassword ? 'text' : 'password'"
              id="register-password"
              v-model="registerData.password"
              class="w-full px-3 py-2 bg-transparent focus:outline-none focus:ring-0 text-white flex-grow"
              required
            />
            <button
              @click="showPassword = !showPassword"
              class="h-8 w-8 flex-shrink-0 mr-1 flex items-center justify-center rounded-full text-surface-400 hover:bg-white/10 hover:text-white transition-all duration-200"
              type="button"
            >
              <font-awesome-icon :icon="showPassword ? faEye : faEyeSlash" class="h-5 w-5" />
            </button>
          </div>
        </div>

        <div class="space-y-1">
          <label
            for="register-confirm-password"
            class="block font-bold text-xs sm:text-sm text-left"
          >
            {{ t('confirmPassword') }}
          </label>
          <div
            class="flex items-center gap-2 bg-tertiary border border-lgray-accent rounded-md transition-all duration-200 focus-within:border-accent focus-within:ring-1 focus-within:ring-accent"
          >
            <input
              :type="showConfirmPassword ? 'text' : 'password'"
              id="register-confirm-password"
              v-model="registerData.confirmPassword"
              class="w-full px-3 py-2 bg-transparent focus:outline-none focus:ring-0 text-white flex-grow"
              required
            />
            <button
              @click="showConfirmPassword = !showConfirmPassword"
              class="h-8 w-8 flex-shrink-0 mr-1 flex items-center justify-center rounded-full text-surface-400 hover:bg-white/10 hover:text-white transition-all duration-200"
              type="button"
            >
              <font-awesome-icon :icon="showConfirmPassword ? faEye : faEyeSlash" class="h-5 w-5" />
            </button>
          </div>
        </div>
      </div>

      <div class="text-xs sm:text-sm">
        <passwordStrength :password="registerData.password" />
      </div>

      <div class="bg-tertiary rounded-md px-3 py-2">
        <ul class="list-disc text-left text-white pl-4">
          <li
            :class="{
              'text-green-500': passwordRequirements.length,
              'text-gray-500': !passwordRequirements.length,
            }"
            class="text-sm transition-colors duration-300"
          >
            {{ t('passwordRequirementLength') }}
          </li>
          <li
            :class="{
              'text-green-500': passwordRequirements.uppercase,
              'text-gray-500': !passwordRequirements.uppercase,
            }"
            class="text-sm transition-colors duration-300"
          >
            {{ t('passwordRequirementUppercase') }}
          </li>
          <li
            :class="{
              'text-green-500': passwordRequirements.lowercase,
              'text-gray-500': !passwordRequirements.lowercase,
            }"
            class="text-sm transition-colors duration-300"
          >
            {{ t('passwordRequirementLowercase') }}
          </li>
          <li
            :class="{
              'text-green-500': passwordRequirements.special,
              'text-gray-500': !passwordRequirements.special,
            }"
            class="text-sm transition-colors duration-300"
          >
            {{ t('passwordRequirementSpecialChar') }}
          </li>
          <li
            :class="{
              'text-green-500': passwordRequirements.digit,
              'text-gray-500': !passwordRequirements.digit,
            }"
            class="text-sm transition-colors duration-300"
          >
            {{ t('passwordRequirementNumber') }}
          </li>
        </ul>
      </div>

      <button
        type="submit"
        :disabled="!isRegisterFormValid || isLoading"
        class="relative w-full py-2.5 sm:py-3 rounded-lg font-medium text-sm sm:text-base md:text-lg transition-all duration-300 overflow-hidden group disabled:opacity-50 disabled:cursor-not-allowed text-white"
        :class="
          isRegisterFormValid
            ? 'bg-gradient-to-r from-primary-600 to-primary-700 hover:from-primary-500 hover:to-primary-600 shadow-lg shadow-primary-500/30 hover:shadow-primary-500/50'
            : 'bg-tertiary shadow-sm'
        "
      >
        <span class="relative z-10">{{
          isLoading ? t('creatingAccount') : t('createAccount')
        }}</span>
        <div
          v-if="isRegisterFormValid"
          class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-400/20 to-primary-400/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
        ></div>
      </button>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, defineEmits } from 'vue'
// --- KROK 1: Import enumu POSITION ---
import { useToast, POSITION } from 'vue-toastification'
import passwordStrength from './passwordStrength.vue'
import { faEye, faEyeSlash } from '@fortawesome/free-solid-svg-icons'
import { useI18n } from 'vue-i18n'
const { t } = useI18n()

// Poprawiono ścieżki importu
import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'

// --- KROK 2: Definicja typu dla błędu API ---
interface ApiError {
  response?: {
    data?: string
  }
}

const toast = useToast()
const emit = defineEmits(['register', 'close', 'switchToConfirmEmail'])

const showPassword = ref(false)
const showConfirmPassword = ref(false)
const isLoading = ref(false)

// --- KROK 3: Jawne otypowanie parametru 'email' ---
const validateEmail = (email: string) => {
  if (!email) return false
  return String(email)
    .toLowerCase()
    .match(
      /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/,
    )
}

const isRegisterFormValid = computed(() => {
  const req = passwordRequirements.value
  const data = registerData.value

  return (
    data.username.trim() !== '' &&
    validateEmail(data.email) &&
    req.length &&
    req.uppercase &&
    req.lowercase &&
    req.digit &&
    req.special &&
    data.password === data.confirmPassword
  )
})

const registerData = ref({
  username: '',
  email: '',
  password: '',
  confirmPassword: '',
})

const passwordRequirements = computed(() => {
  const password = registerData.value.password
  return {
    length: password.length >= 8,
    uppercase: /[A-Z]/.test(password),
    lowercase: /[a-z]/.test(password),
    digit: /[0-9]/.test(password),
    special: /[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]/.test(password),
  }
})

const handleRegister = async () => {
  toast.clear()

  if (registerData.value.password !== registerData.value.confirmPassword) {
    // --- KROK 4: Użycie enumu POSITION ---
    toast.error(t('passwordsDoNotMatch'), {
      position: POSITION.TOP_CENTER,
    })
    return
  }

  try {
    isLoading.value = true
    const response = await apiService.post<{ success: boolean }>(
      apiConfig.auth.register,
      registerData.value,
    )

    if (response.data.success) {
      console.log('✅ Zarejestrowano pomyślnie!')
      toast.success(t('registrationSuccessful'), {
        position: POSITION.TOP_CENTER,
      })
      emit('switchToConfirmEmail', registerData.value.email)
    }
  } catch (error: unknown) {
    // Jawne otypowanie błędu jako 'unknown'
    // --- KROK 5: Bezpieczne rzutowanie typu błędu ---
    const apiError = error as ApiError

    if (apiError.response?.data) {
      if (apiError.response.data === 'Email already exist.') {
        toast.error(t('emailAlreadyRegistered'), {
          position: POSITION.TOP_CENTER,
        })
      } else {
        toast.error(t('registrationError'), {
          position: POSITION.TOP_CENTER,
        })
      }
    } else {
      // Ogólny błąd, jeśli struktura jest inna
      toast.error(t('errorServerUnavailable'), {
        position: POSITION.TOP_CENTER,
      })
    }
  } finally {
    isLoading.value = false
  }
}
</script>
