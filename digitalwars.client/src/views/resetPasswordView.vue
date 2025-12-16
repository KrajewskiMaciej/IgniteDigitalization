<template>
  <div class="bg-surface-800 min-h-screen">
    <div class="fixed inset-0 flex items-center justify-center z-50 p-4">
      <div class="absolute inset-0 bg-black/40 transition-opacity duration-300"></div>
      <div
        class="bg-surface-900 text-white rounded-xl relative z-10 border-2 border-accent transition-all duration-300 p-5 sm:p-8 md:p-10 max-h-[90vh] w-full max-w-md overflow-y-auto"
      >
        <div v-if="isTokenValid">
          <h2 class="text-xl sm:text-2xl font-nasalization mb-4 text-center">
            {{ t('changePassword') }}
          </h2>

          <div class="w-full h-0.5 mb-5 bg-accent/60 rounded-full"></div>

          <form @submit.prevent="handleChangePassword">
            <label
              for="register-password"
              class="block font-medium text-xs sm:text-sm text-left mb-1.5"
            >
              {{ t('newPassword') }}
            </label>
            <div
              class="flex items-center gap-2 bg-tertiary border border-lgray-accent rounded-lg transition-all duration-200 focus-within:border-accent focus-within:ring-1 focus-within:ring-accent mb-3"
            >
              <input
                :type="showPassword ? 'text' : 'password'"
                id="register-password"
                v-model="changePasswordData.password"
                class="w-full px-3 py-2.5 bg-transparent focus:outline-none focus:ring-0 text-white flex-grow text-sm sm:text-base"
                required
              />
              <button
                @click="showPassword = !showPassword"
                class="h-9 w-9 flex-shrink-0 mr-1 flex items-center justify-center rounded-full text-surface-400 hover:bg-white/10 hover:text-white transition-all duration-200"
                type="button"
              >
                <font-awesome-icon
                  :icon="showPassword ? faEye : faEyeSlash"
                  class="h-4 w-4 sm:h-5 sm:w-5"
                />
              </button>
            </div>

            <div class="mb-4">
              <passwordStrength :password="changePasswordData.password" />
            </div>

            <label
              for="register-confirm-password"
              class="block font-medium text-xs sm:text-sm text-left mb-1.5"
            >
              {{ t('confirmNewPassword') }}
            </label>
            <div
              class="flex items-center gap-2 bg-tertiary border border-lgray-accent rounded-lg transition-all duration-200 focus-within:border-accent focus-within:ring-1 focus-within:ring-accent mb-4"
            >
              <input
                :type="showConfirmPassword ? 'text' : 'password'"
                id="register-confirm-password"
                v-model="changePasswordData.confirmPassword"
                class="w-full px-3 py-2.5 bg-transparent focus:outline-none focus:ring-0 text-white flex-grow text-sm sm:text-base"
                required
              />
              <button
                @click="showConfirmPassword = !showConfirmPassword"
                class="h-9 w-9 flex-shrink-0 mr-1 flex items-center justify-center rounded-full text-surface-400 hover:bg-white/10 hover:text-white transition-all duration-200"
                type="button"
              >
                <font-awesome-icon
                  :icon="showConfirmPassword ? faEye : faEyeSlash"
                  class="h-4 w-4 sm:h-5 sm:w-5"
                />
              </button>
            </div>

            <div class="bg-tertiary/50 rounded-lg px-4 py-3 mb-5 border border-white/5">
              <ul class="space-y-1.5 text-left pl-1">
                <li
                  :class="passwordRequirements.length ? 'text-green-400' : 'text-gray-500'"
                  class="text-xs sm:text-sm transition-colors duration-300 flex items-center gap-2"
                >
                  <span
                    class="w-1.5 h-1.5 rounded-full"
                    :class="passwordRequirements.length ? 'bg-green-400' : 'bg-gray-500'"
                  ></span>
                  {{ t('passwordRequirementLength') }}
                </li>
                <li
                  :class="passwordRequirements.uppercase ? 'text-green-400' : 'text-gray-500'"
                  class="text-xs sm:text-sm transition-colors duration-300 flex items-center gap-2"
                >
                  <span
                    class="w-1.5 h-1.5 rounded-full"
                    :class="passwordRequirements.uppercase ? 'bg-green-400' : 'bg-gray-500'"
                  ></span>
                  {{ t('passwordRequirementUppercase') }}
                </li>
                <li
                  :class="passwordRequirements.lowercase ? 'text-green-400' : 'text-gray-500'"
                  class="text-xs sm:text-sm transition-colors duration-300 flex items-center gap-2"
                >
                  <span
                    class="w-1.5 h-1.5 rounded-full"
                    :class="passwordRequirements.lowercase ? 'bg-green-400' : 'bg-gray-500'"
                  ></span>
                  {{ t('passwordRequirementLowercase') }}
                </li>
                <li
                  :class="passwordRequirements.special ? 'text-green-400' : 'text-gray-500'"
                  class="text-xs sm:text-sm transition-colors duration-300 flex items-center gap-2"
                >
                  <span
                    class="w-1.5 h-1.5 rounded-full"
                    :class="passwordRequirements.special ? 'bg-green-400' : 'bg-gray-500'"
                  ></span>
                  {{ t('passwordRequirementSpecialChar') }}
                </li>
                <li
                  :class="passwordRequirements.digit ? 'text-green-400' : 'text-gray-500'"
                  class="text-xs sm:text-sm transition-colors duration-300 flex items-center gap-2"
                >
                  <span
                    class="w-1.5 h-1.5 rounded-full"
                    :class="passwordRequirements.digit ? 'bg-green-400' : 'bg-gray-500'"
                  ></span>
                  {{ t('passwordRequirementNumber') }}
                </li>
              </ul>
            </div>

            <button
              type="submit"
              :disabled="!allPasswordRequirementsMet || isLoading"
              class="relative w-full py-2.5 sm:py-3 rounded-lg font-medium text-sm sm:text-base transition-all duration-300 overflow-hidden group disabled:opacity-50 disabled:cursor-not-allowed text-white"
              :class="
                allPasswordRequirementsMet
                  ? 'bg-gradient-to-r from-primary-600 to-primary-700 hover:from-primary-500 hover:to-primary-600 shadow-lg shadow-primary-500/30 hover:shadow-primary-500/50'
                  : 'bg-tertiary shadow-sm'
              "
            >
              <span class="relative z-10">{{
                isLoading ? t('changingPassword') : t('changePassword')
              }}</span>
              <div
                v-if="allPasswordRequirementsMet"
                class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-400/20 to-primary-400/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
              ></div>
            </button>
          </form>
        </div>

        <div v-else>
          <font-awesome-icon
            :icon="faCircleXmark"
            class="text-5xl sm:text-6xl text-red-500 mb-4 block mx-auto"
          />

          <h2 class="text-xl sm:text-2xl font-nasalization mb-4 text-center">Link wygasł</h2>

          <div class="w-full h-0.5 mb-5 bg-accent/60 rounded-full"></div>

          <div class="text-center text-sm sm:text-base text-gray-300 space-y-3">
            <p>{{ t('linkExpiredMessage') }}</p>
            <p>
              {{ t('toGetNewResetLink') }}
            </p>
          </div>

          <button
            @click="handleReturnToLogin"
            class="relative w-full py-2.5 sm:py-3 rounded-lg font-medium text-sm sm:text-base transition-all duration-300 overflow-hidden group text-white bg-gradient-to-r from-primary-600 to-primary-700 hover:from-primary-500 hover:to-primary-600 shadow-lg shadow-primary-500/30 hover:shadow-primary-500/50 mt-6"
          >
            <span class="relative z-10">{{ t('backToLogin') }}</span>
            <div
              class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-400/20 to-primary-400/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
            ></div>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useToast, POSITION } from 'vue-toastification' // <-- KROK 1: Import POSITION
import passwordStrength from '@/components/auth/passwordStrength.vue'
import { faEye, faEyeSlash } from '@fortawesome/free-solid-svg-icons'
import { faCircleXmark } from '@fortawesome/free-regular-svg-icons'
import { useRouter } from 'vue-router'
import apiConfig from '@/services/apiConfig.js'
import apiService from '@/services/apiServices.js'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

// --- KROK 2: Definicje interfejsów dla odpowiedzi API ---
interface ValidateTokenResponse {
  valid: boolean
}
interface ResetPasswordResponse {
  success: boolean
}

const toast = useToast()
const router = useRouter()
const token = ref('')

onMounted(async () => {
  // --- KROK 3: Poprawne przypisanie tokenu ---
  const tokenParam = router.currentRoute.value.params.token
  token.value = Array.isArray(tokenParam) ? tokenParam[0] : tokenParam

  if (!token.value) {
    isTokenValid.value = false
    return
  }

  try {
    // --- KROK 4: Otypowanie odpowiedzi API ---
    const res = await apiService.get<ValidateTokenResponse>(
      apiConfig.auth.validateResetToken(token.value),
    )
    isTokenValid.value = res.data.valid
  } catch (err) {
    isTokenValid.value = false
    toast.error(t('linkExpired'), {
      position: POSITION.TOP_CENTER, // <-- KROK 5: Użycie POSITION
    })
  }
})

const isTokenValid = ref(false)
const showPassword = ref(false)
const showConfirmPassword = ref(false)
const isLoading = ref(false)

const changePasswordData = ref({
  password: '',
  confirmPassword: '',
})

const passwordRequirements = computed(() => ({
  length: changePasswordData.value.password.length >= 8,
  uppercase: /[A-Z]/.test(changePasswordData.value.password),
  lowercase: /[a-z]/.test(changePasswordData.value.password),
  digit: /[0-9]/.test(changePasswordData.value.password),
  special: /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(changePasswordData.value.password),
}))

const allPasswordRequirementsMet = computed(() => {
  const req = passwordRequirements.value
  return (
    req.length &&
    req.uppercase &&
    req.lowercase &&
    req.digit &&
    req.special &&
    changePasswordData.value.password.trim() !== '' &&
    changePasswordData.value.confirmPassword.trim() !== ''
  )
})

const handleReturnToLogin = () => {
  sessionStorage.setItem('showLoginAfterRedirect', 'true')
  router.push('/')
}

const handleChangePassword = async () => {
  toast.clear()
  if (changePasswordData.value.password !== changePasswordData.value.confirmPassword) {
    toast.warning(t('passwordsDoNotMatch'), {
      position: POSITION.TOP_CENTER, // <-- KROK 5: Użycie POSITION
    })
    return
  }

  try {
    isLoading.value = true
    const response = await apiService.post<ResetPasswordResponse>(apiConfig.auth.resetPassword, {
      token: token.value,
      newPassword: changePasswordData.value.password,
    })

    if (response.data.success) {
      router.push('/')
    }
  } catch (error: any) {
    console.error('❌ Wystąpił błąd:', error.response?.data || error.message)
    toast.error(t('errorServerUnavailable'), {
      position: POSITION.TOP_CENTER, // <-- KROK 5: Użycie POSITION
    })
  } finally {
    isLoading.value = false
  }
}
</script>
