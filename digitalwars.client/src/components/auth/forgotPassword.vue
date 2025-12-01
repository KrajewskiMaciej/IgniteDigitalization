<template>
  <div class="animate-fade" v-if="!isEmailSent">
    <div class="flex justify-center items-center">
      <font-awesome-icon :icon="faUserLock" class="text-6xl mb-3 text-accent" />
    </div>

    <h2 class="text-lg sm:text-xl md:text-2xl font-nasalization mb-2 sm:mb-4 text-center">
      {{ t('forgotPasswordQuestion') }}
    </h2>

    <div
      class="h-[2px] bg-gradient-to-r from-transparent via-primary-500 to-transparent mb-6 sm:mb-8"
    ></div>

    <form @submit.prevent="handleSendEmail">
      <div class="text-gray-300 mt-2 mb-2 text-center">
        <p>{{ t('enterYourEmail') }}</p>
        <p>{{ t('weWillSendPasswordResetLink') }}</p>
      </div>

      <div class="space-y-1 mb-5">
        <label for="email" class="block font-bold text-xs sm:text-sm text-left">
          {{ t('email') }}
        </label>
        <input
          type="email"
          id="email"
          v-model="email"
          class="w-full px-3 py-2 bg-tertiary border border-gray-600 rounded-md text-white focus:outline-none focus:border-accent text-sm sm:text-base"
          required
        />
      </div>

      <button
        type="submit"
        class="relative w-full py-2.5 sm:py-3 rounded-lg font-medium text-sm sm:text-base md:text-lg transition-all duration-300 overflow-hidden group text-white mb-5 bg-gradient-to-r from-primary-600 to-primary-700 hover:from-primary-500 hover:to-primary-600 shadow-lg shadow-primary-500/30 hover:shadow-primary-500/50"
      >
        <span class="relative z-10">{{ t('resetPassword') }}</span>
        <div
          class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-400/20 to-primary-400/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
        ></div>
      </button>

      <div class="flex justify-center items-center">
        <span
          class="text-accent hover:text-purple-300 transition-colors cursor-pointer"
          @click="emit('backToLogin')"
        >
          {{ t('backToLogin') }}
        </span>
      </div>
    </form>
  </div>

  <div v-else class="animate-fade-right">
    <div class="flex justify-center items-center">
      <font-awesome-icon :icon="faEnvelopeCircleCheck" class="mb-3 text-6xl text-accent" />
    </div>

    <h2 class="text-lg sm:text-xl md:text-2xl font-nasalization mb-2 sm:mb-3 text-center">
      {{ t('checkYourEmail') }}
    </h2>

    <div class="w-100 h-0.5 mb-1 sm:mb-2 md:mb-3 lg:mb-4 bg-accent"></div>

    <div class="text-sm text-gray-300 text-center">
      <p>{{ t('weHaveSentPasswordResetInstructions') }}</p>
      <p>
        {{ t('toAddress') }} <b>{{ email }}</b>
      </p>
      <p>{{ t('checkSpamFolder') }}</p>
    </div>

    <button
      @click="handleSendEmail"
      :disabled="!canResend"
      class="relative w-full py-2.5 sm:py-3 rounded-lg font-medium text-sm sm:text-base md:text-lg transition-all duration-300 overflow-hidden group disabled:opacity-50 disabled:cursor-not-allowed text-white mt-5"
      :class="
        canResend
          ? 'bg-gradient-to-r from-primary-600 to-primary-700 hover:from-primary-500 hover:to-primary-600 shadow-lg shadow-primary-500/30 hover:shadow-primary-500/50'
          : 'bg-tertiary shadow-sm'
      "
    >
      <span class="relative z-10">{{
        canResend ? t('resendAgain') : `${t('resendAgainIn')} ${countdown}s`
      }}</span>
      <div
        v-if="canResend"
        class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-400/20 to-primary-400/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
      ></div>
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref, defineEmits } from 'vue'
// --- KROK 1: Import enumu POSITION ---
import { useToast, POSITION } from 'vue-toastification'
// --- KROK 2: Usunięcie nieużywanego importu 'apiClient' ---
import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'
import { faEnvelopeCircleCheck, faUserLock } from '@fortawesome/free-solid-svg-icons'
import { useI18n } from 'vue-i18n'
const { t } = useI18n()

// --- KROK 3: Definicja typu dla błędu API ---
interface ApiError {
  message?: string
  status: number
  toString: () => string
}

const email = ref('')
const isEmailSent = ref(false)
const canResend = ref(true)
const countdown = ref(0)
const toast = useToast()
const emit = defineEmits(['backToLogin'])

const handleSendEmail = async () => {
  try {
    const response = await apiService.post<{ success: boolean }>(apiConfig.auth.forgotPassword, {
      Email: email.value,
    })

    if (response.data.success) {
      console.log('✅ Wysłano email do zmiany hasła')

      if (!isEmailSent.value) {
        isEmailSent.value = true
      }

      startCooldown()
    }
  } catch (error: unknown) {
    // Jawne typowanie błędu
    // --- KROK 4: Bezpieczne rzutowanie typu błędu ---
    const apiError = error as ApiError
    const status: number = apiError.status

    console.log('Jaki błąd otrzymuje:', apiError.status)

    if (status === 409) {
      toast.warning(t('invalidEmailOrEmailDoesntExist'), {
        position: POSITION.TOP_CENTER,
      })
    } else {
      toast.error(t('errorServerUnavailable'), {
        position: POSITION.TOP_CENTER,
      })
    }
    console.error('Wystąpił błąd:', apiError.message || apiError.toString())
  }
}

const startCooldown = () => {
  canResend.value = false
  countdown.value = 60

  const timer = setInterval(() => {
    countdown.value--
    if (countdown.value <= 0) {
      canResend.value = true
      clearInterval(timer)
    }
  }, 1000)
}
</script>
