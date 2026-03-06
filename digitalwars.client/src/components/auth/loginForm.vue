<template>
  <div class="w-full h-full">
    <h2 class="text-lg sm:text-xl md:text-2xl font-nasalization mb-3 sm:mb-4 text-center">
      {{ t('signInToYourAccount') }}
    </h2>

    <form @submit.prevent="handleLogin" class="space-y-4 sm:space-y-5">
      <div class="space-y-1.5">
        <label for="email" class="block font-bold text-xs sm:text-sm text-left">
          {{ t('emailOrUsername') }}
        </label>
        <input
          type="email"
          id="email"
          v-model="loginData.email"
          class="w-full px-3 py-2.5 sm:py-3 bg-tertiary border border-gray-600 rounded-lg text-white focus:outline-none focus:border-accent text-sm sm:text-base"
          required
        />
      </div>

      <div class="space-y-1.5">
        <label for="password" class="block font-bold text-xs sm:text-sm text-left">
          {{ t('password') }}
        </label>
        <div
          class="flex items-center gap-2 bg-tertiary border border-gray-600 rounded-lg transition-all duration-200 focus-within:border-accent focus-within:ring-1 focus-within:ring-accent"
        >
          <input
            :type="showPassword ? 'text' : 'password'"
            id="password"
            v-model="loginData.password"
            class="w-full px-3 py-2.5 sm:py-3 bg-transparent focus:outline-none focus:ring-0 text-white placeholder-gray-400 flex-grow text-sm sm:text-base"
            required
          />
          <button
            @click="showPassword = !showPassword"
            class="h-9 w-9 sm:h-10 sm:w-10 flex-shrink-0 mr-1.5 flex items-center justify-center rounded-full text-surface-400 hover:bg-white/10 hover:text-white transition-all duration-200"
            type="button"
          >
            <font-awesome-icon
              :icon="showPassword ? faEye : faEyeSlash"
              class="h-4 w-4 sm:h-5 sm:w-5"
            />
          </button>
        </div>
      </div>

      <div class="text-xs sm:text-sm text-right">
        <span
          class="text-accent hover:text-purple-300 transition-colors cursor-pointer"
          @click="emit('forgotPassword')"
        >
          {{ t('forgotPassword') }}
        </span>
      </div>

      <button
        type="submit"
        :disabled="!isLoginFormValid || isLoading"
        class="relative w-full py-2.5 sm:py-3 rounded-lg font-medium text-sm sm:text-base md:text-lg transition-all duration-300 overflow-hidden group disabled:opacity-50 disabled:cursor-not-allowed text-white"
        :class="
          isLoginFormValid
            ? 'bg-gradient-to-r from-primary-600 to-primary-700 hover:from-primary-500 hover:to-primary-600 shadow-lg shadow-primary-500/30 hover:shadow-primary-500/50'
            : 'bg-tertiary shadow-sm'
        "
      >
        <span class="relative z-10">{{ isLoading ? t('loggingIn') : t('login') }}</span>
        <div
          v-if="isLoginFormValid"
          class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-400/20 to-primary-400/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
        />
      </button>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, nextTick } from 'vue'
import { useToast, POSITION } from 'vue-toastification'
import router from '@/router'
import { faEye, faEyeSlash } from '@fortawesome/free-solid-svg-icons'
import { useAuthStore } from '@/stores/auth'
import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
interface ApiError {
  response?: { data?: string }
  message?: string
}

const authStore = useAuthStore()
const toast = useToast()
const emit = defineEmits(['login', 'close', 'forgotPassword', 'error'])

const showPassword = ref(false)
const isLoading = ref(false)

const loginData = ref({
  email: '',
  password: '',
})

const validateEmail = (email: string) => {
  if (!email) return false
  return String(email)
    .toLowerCase()
    .match(
      /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/,
    )
}

const isLoginFormValid = computed(() => {
  return (
    loginData.value.email.trim() !== '' &&
    loginData.value.password.trim() !== '' &&
    validateEmail(loginData.value.email)
  )
})

const handleLogin = async () => {
  toast.clear()

  if (!isLoginFormValid.value) {
    toast.error(t('pleaseEnterValidEmailAndPassword'), {
      position: POSITION.TOP_CENTER,
    })
    return
  }

  isLoading.value = true

  try {
    const payload = {
      username: loginData.value.email,
      password: loginData.value.password,
    }
    const response = await apiService.post<{ success: boolean }>(apiConfig.auth.login, payload)

    if (response.data.success) {
      authStore.setAuthenticated(true)
      emit('close')
      await nextTick()
      router.push('/admin')
    }
  } catch (error: unknown) {
    const apiError = error as ApiError
    emit('error')
    if (apiError.response?.data) {
      if (apiError.response.data === 'Invalid credentials') {
        toast.error(t('invalidEmailOrPassword'), {
          position: POSITION.TOP_CENTER,
        })
      } else if (apiError.response.data.includes('E-mail nie został potwierdzony')) {
        toast.warning(t('emailNotConfirmed'), {
          position: POSITION.TOP_CENTER,
        })
      } else {
        toast.error(t('loginError'), {
          position: POSITION.TOP_CENTER,
        })
      }
    } else {
      toast.error(t('errorServerUnavailable'), {
        position: POSITION.TOP_CENTER,
      })
    }
    console.error('❌ Wystąpił błąd:', apiError.response?.data || apiError.message)
  } finally {
    isLoading.value = false
  }
}
</script>
