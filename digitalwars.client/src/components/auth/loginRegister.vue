<template>
  <div
    v-if="props.isVisible"
    class="fixed inset-0 flex items-center justify-center z-50 md:p-4 lg:p-6 xl:p-8"
  >
    <div
      class="absolute inset-0 bg-black/70 backdrop-blur-sm transition-opacity duration-300 md:block hidden"
      @click="closeModal"
    ></div>

    <div
      class="bg-gradient-to-br from-surface-850 to-surface-900 text-surface-0 relative z-50 transition-all duration-300 w-full h-full md:h-auto md:max-w-lg lg:max-w-xl xl:max-w-2xl md:max-h-[95vh] overflow-y-auto custom-scrollbar md:rounded-2xl md:border md:border-primary-500/30 md:shadow-2xl md:shadow-primary-500/20"
      :class="
        props.isVisible
          ? 'md:scale-100 md:translate-y-0 opacity-100'
          : 'md:scale-95 md:translate-y-4 opacity-0'
      "
    >
      <button
        @click="closeModal"
        class="absolute top-4 right-4 z-20 w-10 h-10 flex items-center justify-center rounded-full text-surface-0 hover:text-primary-400 hover:bg-surface-700 backdrop-blur-sm transition-all duration-200 hover:shadow-lg hover:shadow-primary-500/30"
      >
        <font-awesome-icon :icon="faXmark" class="md:text-2xl sm:text-4xl" />
      </button>

      <div class="px-4 sm:px-5 md:px-6 lg:px-8 pt-16 pb-4 sm:pb-6 md:pb-8">
        <div
          v-if="activeView === 'login' || activeView === 'register'"
          class="flex items-center gap-2 p-2 mb-4 sm:mb-6 bg-surface-800 rounded-full backdrop-blur-sm border border-primary-500"
        >
          <button
            @click="activeView = 'login'"
            class="flex-1 py-2 sm:py-3 rounded-full transition-all duration-300 text-sm sm:text-base relative overflow-hidden group"
            :class="
              activeView === 'login'
                ? 'bg-gradient-to-r from-primary-400 to-primary-500 font-semibold shadow-lg shadow-primary-500/50'
                : 'bg-transparent hover:bg-gradient-to-r hover:from-primary-400/20 hover:to-primary-500/20 hover:shadow-md hover:shadow-primary-500/20'
            "
          >
            <span class="relative z-10">{{ t('signIn') }}</span>
            <div
              v-if="activeView !== 'login'"
              class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
            />
          </button>

          <button
            @click="activeView = 'register'"
            class="flex-1 py-3 sm:py-3.5 rounded-full transition-all duration-300 text-sm sm:text-base relative overflow-hidden group"
            :class="
              activeView === 'register'
                ? 'bg-gradient-to-r from-primary-400 to-primary-500 font-semibold shadow-lg shadow-primary-500/50'
                : 'bg-transparent hover:bg-gradient-to-r hover:from-primary-400/20 hover:to-primary-500/20 hover:shadow-md hover:shadow-primary-500/20'
            "
          >
            <span class="relative z-10">{{ t('signUp') }}</span>
            <div
              v-if="activeView !== 'register'"
              class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
            />
          </button>
        </div>

        <div
          v-if="activeView === 'login' || activeView === 'register'"
          class="h-[2px] bg-gradient-to-r from-transparent via-primary-500 to-transparent mb-6 sm:mb-8"
        ></div>

        <!-- Forms -->
        <div class="min-h-[30vh]">
          <LoginForm
            v-if="activeView === 'login'"
            @close="closeModal"
            @forgotPassword="handleForgotPassword"
            class="animate-fade-left"
          />

          <RegisterForm
            v-if="activeView === 'register'"
            @close="closeModal"
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
import { ref, watch } from 'vue'
import { faXmark } from '@fortawesome/free-solid-svg-icons'
import LoginForm from '@/components/auth/loginForm.vue'
import RegisterForm from '@/components/auth/registerForm.vue'
import ForgotPassword from './forgotPassword.vue'
import ConfirmEmail from './confirmEmail.vue'
import { useI18n } from 'vue-i18n'
import { breakpointsTailwind, useBreakpoints } from '@vueuse/core'

const breakpoints = useBreakpoints(breakpointsTailwind)

const bigScreen = breakpoints.greater('md');

const { t } = useI18n()

type AuthView = 'login' | 'register' | 'forgotPassword' | 'confirmEmail'

const props = defineProps({
  isVisible: {
    type: Boolean,
    default: false,
  },
  initialView: {
    type: String,
    default: 'login',
    validator: (value: unknown) =>
      typeof value === 'string' && ['login', 'register'].includes(value),
  },
})

const emit = defineEmits(['register', 'close', 'switchToLogin'])

const activeView = ref<AuthView>(props.initialView as AuthView)
const emailToConfirm = ref('')

watch(
  () => props.isVisible,
  (newValue) => {
    if (newValue) {
      activeView.value = props.initialView as AuthView
    }
  },
)

watch(
  () => props.initialView,
  (newValue) => {
    activeView.value = newValue as AuthView
  },
)

const closeModal = () => {
  emit('close')
}

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

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 8px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: rgba(30, 41, 59, 0.5);
  border-radius: 4px;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(139, 92, 246, 0.5);
  border-radius: 4px;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: rgba(139, 92, 246, 0.7);
}
</style>
