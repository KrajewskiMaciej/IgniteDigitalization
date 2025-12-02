<template>
  <div 
    class=" animate-fade absolute inset-0 z-20 w-screen h-screen bg-surface-900 text-surface-0" 
    :class="isBigScreen ? 'grid grid-cols-2' : ''"
  >
    <div v-if="isBigScreen">
      <RobotAuth />
    </div>
    
    <div 
      class="relative flex justify-center items-center h-full"
      :class="isBigScreen ? 'border-l border-primary-500/30' : ''"
    >
      <div class="absolute inset-0 bg-gradient-to-br from-primary-500/5 via-transparent to-primary-400/5" />
      
     <button
        @click="emit('close')"
        class="absolute top-4 right-4 z-20 w-10 h-10 flex items-center justify-center rounded-full text-surface-0 hover:text-primary-400 hover:bg-surface-700 backdrop-blur-sm transition-all duration-200 hover:shadow-lg hover:shadow-primary-500/30"
      >
        <font-awesome-icon :icon="faXmark" class="text-xl" />
      </button>

      <div 
        class="relative z-10 w-full max-w-lg px-6 sm:px-8 md:px-12 py-8 overflow-y-auto max-h-screen"
      >
        <div
          v-if="activeView === 'login' || activeView === 'register'"
          class="flex items-center gap-2 p-1.5 mb-8 bg-surface-800/80 rounded-full backdrop-blur-sm border border-surface-700"
        >
          <button
            @click="activeView = 'login'"
            class="flex-1 py-3 sm:py-3.5 px-4 sm:px-6 rounded-full transition-all duration-300 text-sm sm:text-base font-medium relative overflow-hidden"
            :class="
              activeView === 'login'
                ? 'bg-gradient-to-r from-primary-500 to-primary-600 text-white shadow-lg shadow-primary-500/40'
                : 'text-surface-400 hover:text-surface-200 hover:bg-surface-700/50'
            "
          >
            <span class="relative z-10">{{ t('signIn') }}</span>
          </button>

          <button
            @click="activeView = 'register'"
            class="flex-1 py-3 sm:py-3.5 px-4 sm:px-6 rounded-full transition-all duration-300 text-sm sm:text-base font-medium relative overflow-hidden"
            :class="
              activeView === 'register'
                ? 'bg-gradient-to-r from-primary-500 to-primary-600 text-white shadow-lg shadow-primary-500/40'
                : 'text-surface-400 hover:text-surface-200 hover:bg-surface-700/50'
            "
          >
            <span class="relative z-10">{{ t('signUp') }}</span>
          </button>
        </div>

        <div>
          <LoginForm
            v-if="activeView === 'login'"
            @close="emit('close')"
            @forgotPassword="handleForgotPassword"
            class="animate-fade-left"
          />

          <RegisterForm
            v-if="activeView === 'register'"
            @close="emit('close')"
            @switchToConfirmEmail="handleSwitchToConfirmEmail"
            class="animate-fade-right"
          />

          <ForgotPassword
            v-if="activeView === 'forgotPassword'"
            @back-to-login="handleBackToLogin"
            class="animate-fade-right"
          />

          <ConfirmEmail
            v-if="activeView === 'confirmEmail'"
            :email="emailToConfirm"
            @back-to-login="handleBackToLogin"
            class="animate-fade-left"
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
const isBigScreen = breakpoints.greater('lg');
const { t } = useI18n();
const activeView = ref<'login' | 'register' | 'forgotPassword' | 'confirmEmail'>('login');
const emailToConfirm = ref<string>('')

const emit = defineEmits<{
  (e: 'close'): void
}>();

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
</script>
