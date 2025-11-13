<template>
  <div class="animate-fade" v-if="!isEmailSent">
    <h2 class="text-lg sm:text-xl md:text-2xl font-nasalization mb-2 sm:mb-3 text-center">
      Zapomniałeś hasła?
    </h2>

    <font-awesome-icon :icon="faUserLock" class="h-20 mb-3 text-accent" />

    <div class="w-100 h-0.5 mb-1 sm:mb-2 md:mb-3 lg:mb-4 bg-accent"></div>

    <form @submit.prevent="handleSendEmail">
      <div class="text-sm text-gray-300 mt-2 mb-2 text-center">
        <p>Wprowadź swój adres e-mail.</p>
        <p>Wyślemy na niego link z instrukcjami do zmiany hasła.</p>
      </div>

      <div class="space-y-1 mb-5">
        <label for="email" class="block font-bold text-xs sm:text-sm text-left"> E-mail </label>
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
        class="bg-tertiary hover:bg-accent text-white w-full rounded-lg font-medium transition-all duration-300 shadow-sm hover:shadow-lg shadow-accent/40 hover:shadow-accent/60 py-2.5 sm:py-3 text-sm sm:text-base md:text-lg mb-5"
      >
        Zresetuj hasło
      </button>

      <span
        class="text-accent hover:text-purple-300 transition-colors cursor-pointer"
        @click="emit('backToLogin')"
      >
        Powrót do logowania
      </span>
    </form>
  </div>

  <div v-else class="animate-fade-right">
    <h2 class="text-lg sm:text-xl md:text-2xl font-nasalization mb-2 sm:mb-3 text-center">
      Sprawdź swój adres e-mail
    </h2>

    <font-awesome-icon :icon="faEnvelopeCircleCheck" class="mb-3 h-20 text-accent" />

    <div class="w-100 h-0.5 mb-1 sm:mb-2 md:mb-3 lg:mb-4 bg-accent"></div>

    <div class="text-sm text-gray-300 text-center">
      <p>Wysłaliśmy instrukcje resetowania hasła</p>
      <p>
        na adres: <b>{{ email }}</b>
      </p>
      <p>Jeśli nie widzisz wiadomości, sprawdź folder spam.</p>
    </div>

    <button
      @click="handleSendEmail"
      class="text-white w-full rounded-lg font-medium transition-all duration-300 shadow-sm shadow-accent/40 py-2.5 sm:py-3 text-sm sm:text-base md:text-lg mt-5"
      :class="
        canResend
          ? 'bg-accent/50 hover:shadow-lg hover:shadow-accent/60 hover:bg-accent'
          : 'bg-tertiary'
      "
      :disabled="!canResend"
    >
      {{ canResend ? 'Wyślij ponownie' : `Wyślij ponownie za ${countdown}s` }}
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

// --- KROK 3: Definicja typu dla błędu API ---
interface ApiError {
  message?: string
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

    toast.error('Nieprawidłowy e-mail lub nie istnieje. Spróbuj ponownie!', {
      // --- KROK 5: Użycie enumu POSITION ---
      position: POSITION.TOP_CENTER,
    })

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
