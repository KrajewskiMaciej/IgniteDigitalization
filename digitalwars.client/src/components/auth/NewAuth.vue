<template>
  <!-- Tło z efektem rozmycia -->
  <div
    class="animate-fade fixed inset-0 z-50 flex items-center justify-center bg-black/70 backdrop-blur-sm p-4"
  >
    <!-- Główny Kontener -->
    <div
      class="relative w-full max-w-lg bg-gradient-to-br from-surface-900 to-surface-950 text-white rounded-2xl border border-primary-500/30 shadow-2xl shadow-primary-500/20 flex flex-col max-h-[95vh] overflow-hidden"
    >
      <!-- Przycisk Zamknij -->
      <button
        @click="emit('close')"
        class="absolute top-4 right-4 z-20 w-10 h-10 flex items-center justify-center rounded-full text-surface-400 hover:text-primary-400 hover:bg-secondary transition-all duration-200"
      >
        <font-awesome-icon :icon="faXmark" class="text-xl" />
      </button>

      <!-- Scrollowalna treść -->
      <div class="px-6 sm:px-10 py-10 overflow-y-auto">
        
        <!-- Przełącznik widoków (Login / Register) -->
        <div
          v-if="activeView === 'login' || activeView === 'register'"
          class="flex items-center gap-2 p-1.5 mb-8 bg-secondary/80 rounded-xl border border-surface-700/50"
        >
          <button
            @click="activeView = 'login'"
            class="flex-1 py-2.5 rounded-lg transition-all duration-300 text-sm font-semibold"
            :class="
              activeView === 'login'
                ? 'bg-primary-600 text-white shadow-md shadow-primary-900/40'
                : 'text-surface-400 hover:text-surface-200 hover:bg-secondary'
            "
          >
            {{ t('signIn') }}
          </button>

          <button
            @click="activeView = 'register'"
            class="flex-1 py-2.5 rounded-lg transition-all duration-300 text-sm font-semibold"
            :class="
              activeView === 'register'
                ? 'bg-primary-600 text-white shadow-md shadow-primary-900/40'
                : 'text-surface-400 hover:text-surface-200 hover:bg-secondary'
            "
          >
            {{ t('signUp') }}
          </button>
        </div>

        <!-- Linia dekoracyjna -->
        <div v-if="activeView === 'login' || activeView === 'register'" 
             class="h-px bg-gradient-to-r from-transparent via-primary-500/30 to-transparent mb-8">
        </div>

        <!-- Formularze -->
        <div class="relative">
          <LoginForm
            v-if="activeView === 'login'"
            @close="emit('close')"
            @forgotPassword="handleForgotPassword"
            @error="handleError"
            class="animate-fade-in"
          />

          <RegisterForm
            v-if="activeView === 'register'"
            @close="emit('close')"
            @switchToConfirmEmail="handleSwitchToConfirmEmail"
            @error="handleError"
            class="animate-fade-in"
          />

          <ForgotPassword
            v-if="activeView === 'forgotPassword'"
            @back-to-login="handleBackToLogin"
            @error="handleError"
            class="animate-fade-in"
          />

          <ConfirmEmail
            v-if="activeView === 'confirmEmail'"
            :email="emailToConfirm"
            @back-to-login="handleBackToLogin"
            class="animate-fade-in"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import RobotAuth from '../animations/RobotAuth.vue'
import { faXmark } from '@fortawesome/free-solid-svg-icons'
import { useI18n } from 'vue-i18n'
import { ref } from 'vue'
import LoginForm from './loginForm.vue'
import RegisterForm from './registerForm.vue'
import ConfirmEmail from './confirmEmail.vue'
import ForgotPassword from './forgotPassword.vue'

import { breakpointsTailwind, useBreakpoints } from '@vueuse/core'

const breakpoints = useBreakpoints(breakpointsTailwind)
const isBigScreen = breakpoints.greater('lg')
const { t } = useI18n()
const activeView = ref<'login' | 'register' | 'forgotPassword' | 'confirmEmail'>('login')
const emailToConfirm = ref<string>('')
const robotRef = ref<InstanceType<typeof RobotAuth> | null>(null)

const emit = defineEmits<{
  (e: 'close'): void
}>()

const handleForgotPassword = () => {
  activeView.value = 'forgotPassword'
}

const handleBackToLogin = () => {
  activeView.value = 'login'
}

const handleSwitchToConfirmEmail = (email: string) => {
  activeView.value = 'confirmEmail'
  emailToConfirm.value = email
}

const handleError = () => {
  robotRef.value?.setEmotion('sad')

  setTimeout(() => {
    robotRef.value?.setEmotion('happy')
  }, 5000)
}
</script>
