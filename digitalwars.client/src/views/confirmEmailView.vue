<template>
  <div class="bg-secondary min-h-screen">
    <div class="fixed inset-0 flex items-center justify-center z-50 p-4">
      <div class="absolute inset-0 bg-black/40 transition-opacity duration-300"></div>
      <div
        class="bg-secondary text-white rounded-xl relative z-10 border-2 border-accent transition-all duration-300 p-5 sm:p-8 md:p-10 max-h-[90vh] w-full max-w-md overflow-y-auto"
      >
        <div v-if="isTokenValid">
          <font-awesome-icon
            :icon="faUserCheck"
            class="text-5xl sm:text-6xl text-accent mb-4 block mx-auto"
          />

          <h2 class="text-xl sm:text-2xl font-nasalization mb-4 text-center">
            {{ t('accountActive') }}
          </h2>

          <div class="w-full h-0.5 mb-5 bg-accent/60 rounded-full"></div>

          <div class="text-center text-sm sm:text-base text-gray-300 space-y-3">
            <p>{{ t('accountIsActiveYouCanUseTheApp') }}</p>
            <p>{{ t('returnToLoginPageAndSignIn') }}</p>
            <p class="text-gray-400">
              {{ t('automaticRedirectIn') }}
              <span class="text-accent font-bold text-lg">{{ time }}{{ t('seconds') }}</span>
            </p>
          </div>

          <button
            @click="handleReturnToLogin"
            class="relative w-full py-2.5 sm:py-3 rounded-lg font-medium text-sm sm:text-base transition-all duration-300 overflow-hidden group text-white bg-gradient-to-r from-primary-600 to-primary-700 hover:from-primary-500 hover:to-primary-600 shadow-lg shadow-primary-500/30 hover:shadow-primary-500/50 mt-6"
          >
            <span class="relative z-10">{{ t('signIn') }}</span>
            <div
              class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-400/20 to-primary-400/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
            ></div>
          </button>
        </div>

        <div v-else>
          <font-awesome-icon
            :icon="faCircleXmark"
            class="text-5xl sm:text-6xl text-red-500 mb-4 block mx-auto"
          />

          <h2 class="text-xl sm:text-2xl font-nasalization mb-4 text-center">
            {{ t('linkExpired') }}
          </h2>

          <div class="w-full h-0.5 mb-5 bg-accent/60 rounded-full"></div>

          <div class="text-center text-sm sm:text-base text-gray-300 space-y-3">
            <p>{{ t('linkExpiredMessage') }}</p>
            <p>{{ t('toGetNewResetLink') }}</p>
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
import { faUserCheck } from '@fortawesome/free-solid-svg-icons'
import { faCircleXmark } from '@fortawesome/free-regular-svg-icons'
import { onMounted, ref, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import apiConfig from '@/services/apiConfig.js'
import apiService from '@/services/apiServices.js'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

// --- POPRAWKA: Definicja interfejsu dla odpowiedzi API ---
interface ConfirmEmailResponse {
  success: boolean
}

const router = useRouter()
const time = ref(20)
const isTokenValid = ref(false)

const handleReturnToLogin = () => {
  sessionStorage.setItem('showLoginAfterRedirect', 'true')
  router.push('/')
}

onMounted(async () => {
  const tokenParam = router.currentRoute.value.params.token
  // --- POPRAWKA: Upewniamy się, że token jest pojedynczym stringiem ---
  const token = Array.isArray(tokenParam) ? tokenParam[0] : tokenParam

  console.log('Token', token)
  if (token) {
    try {
      // --- POPRAWKA: Dodajemy typ generyczny do wywołania API ---
      const response = await apiService.get<ConfirmEmailResponse>(
        apiConfig.auth.confirmEmail(token),
      )

      // Teraz TypeScript wie, że response.data.success istnieje i jest typu boolean
      if (response.data.success) {
        isTokenValid.value = true
      }
    } catch (error) {
      console.error('Błąd potwierdzania tokenu:', error)
      isTokenValid.value = false
    }
  }

  if (isTokenValid.value) {
    const interval = setInterval(() => {
      if (time.value > 0) {
        time.value--
      } else {
        clearInterval(interval)
        handleReturnToLogin()
      }
    }, 1000)

    onUnmounted(() => {
      clearInterval(interval)
    })
  }
})
</script>
