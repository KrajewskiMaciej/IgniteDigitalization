<template>
  <div>
    <h2 class="text-2xl font-nasalization mb-6 text-center">{{ t('changePassword') }}</h2>

    <div class="text-red-500 mt-2 mb-2 text-center" v-show="errorPasswordsNotMatch">
      <p>{{ t('passwordsDoNotMatch') }}</p>
    </div>

    <div class="text-red-500 mt-2 mb-2 text-center" v-show="!passwordValid">
      <p>{{ t('passwordDoesntMeetRequirements') }}</p>
    </div>

    <form @submit.prevent="handleChangePassword">
      <!-- Stare hasło -->
      <div class="mb-4 relative">
        <input
          :type="showOldPassword ? 'text' : 'password'"
          v-model="changePasswordData.oldPassword"
          :placeholder="t('oldPasswordPlaceholder')"
          class="w-full px-3 py-3 bg-tertiary border border-lgray-accent rounded-md text-white focus:outline-none focus:border-accent"
          required
        />
        <button
          @click="showOldPassword = !showOldPassword"
          class="absolute right-3 -translate-y-1/2 top-1/2"
          type="button"
        >
          <font-awesome-icon
            :icon="showOldPassword ? faEye : faEyeSlash"
            class="h-4 text-surface-500 hover:text-accent transition-all duration-300"
          />
        </button>
      </div>

      <!-- Nowe hasło -->
      <div class="mb-4 relative">
        <input
          :type="showPassword ? 'text' : 'password'"
          v-model="changePasswordData.password"
          :placeholder="t('newPasswordPlaceholder')"
          class="w-full px-3 py-3 bg-tertiary border border-lgray-accent rounded-md text-white focus:outline-none focus:border-accent"
          required
        />
        <button
          @click="showPassword = !showPassword"
          class="absolute right-3 -translate-y-1/2 top-1/2"
          type="button"
        >
          <font-awesome-icon
            :icon="showPassword ? faEye : faEyeSlash"
            class="h-4 text-surface-500 hover:text-accent transition-all duration-300"
          />
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
          :placeholder="t('confirmPasswordPlaceholder')"
          class="w-full px-3 py-3 bg-tertiary border border-lgray-accent rounded-md text-white focus:outline-none focus:border-accent"
          required
        />
        <button
          @click="showConfirmPassword = !showConfirmPassword"
          class="absolute right-3 -translate-y-1/2 top-1/2"
          type="button"
        >
          <font-awesome-icon
            :icon="showConfirmPassword ? faEye : faEyeSlash"
            class="h-4 text-surface-500 hover:text-accent transition-all duration-300"
          />
        </button>
      </div>

      <div class="mb-5">
        <ul class="list-disc text-left text-surface-500 pl-5">
          <li
            :class="{
              'text-green-500': passwordRequirements.length,
              'text-surface-300': !passwordRequirements.length,
            }"
          >
            {{ t('passwordRequirementLength') }}
          </li>
          <li
            :class="{
              'text-green-500': passwordRequirements.uppercase,
              'text-surface-300': !passwordRequirements.uppercase,
            }"
          >
            {{ t('passwordRequirementUppercase') }}
          </li>
          <li
            :class="{
              'text-green-500': passwordRequirements.lowercase,
              'text-surface-300': !passwordRequirements.lowercase,
            }"
          >
            {{ t('passwordRequirementLowercase') }}
          </li>
          <li
            :class="{
              'text-green-500': passwordRequirements.special,
              'text-surface-300': !passwordRequirements.special,
            }"
          >
            {{ t('passwordRequirementSpecialChar') }}
          </li>
          <li
            :class="{
              'text-green-500': passwordRequirements.digit,
              'text-surface-300': !passwordRequirements.digit,
            }"
          >
            {{ t('passwordRequirementNumber') }}
          </li>
        </ul>
      </div>

      <button
        type="submit"
        class="relative w-full py-4 rounded-lg font-medium transition-all duration-300 overflow-hidden group text-white mb-5 bg-gradient-to-r from-primary-600 to-primary-700 hover:from-primary-500 hover:to-primary-600 shadow-lg shadow-primary-500/30 hover:shadow-primary-500/50"
      >
        <span class="relative z-10">{{ t('changePassword') }}</span>
        <div
          class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-400/20 to-primary-400/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
        ></div>
      </button>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useToast, POSITION } from 'vue-toastification'
import passwordStrength from './passwordStrength.vue'
import { faEye, faEyeSlash } from '@fortawesome/free-solid-svg-icons'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
interface ApiError {
  response?: { data?: string }
  message?: string
}

const toast = useToast()

const showOldPassword = ref(false)
const showPassword = ref(false)
const showConfirmPassword = ref(false)
const errorPasswordsNotMatch = ref(false)
const passwordValid = ref(true)

const changePasswordData = ref({
  oldPassword: '',
  password: '',
  confirmPassword: '',
})

const passwordRequirements = computed(() => {
  const password = changePasswordData.value.password
  return {
    length: password.length >= 8,
    uppercase: /[A-Z]/.test(password),
    lowercase: /[a-z]/.test(password),
    digit: /[0-9]/.test(password),
    special: /[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]/.test(password),
  }
})

const handleChangePassword = async () => {
  passwordValid.value = true
  errorPasswordsNotMatch.value = false

  if (changePasswordData.value.password !== changePasswordData.value.confirmPassword) {
    errorPasswordsNotMatch.value = true
    toast.error(t('passwordsDoNotMatchExclamation'))
    return
  }

  const req = passwordRequirements.value
  const isPasswordCompliant =
    req.length && req.uppercase && req.lowercase && req.digit && req.special

  if (!isPasswordCompliant) {
    passwordValid.value = false
    toast.error(t('passwordDoesntMeetRequirements'), {
      position: POSITION.TOP_CENTER,
    })
    return
  }

  passwordValid.value = true

  try {
    const response = { data: { success: true } }

    if (response && response.data.success) {

      toast.success(t('passwordChangedSuccessfully'), {
        position: POSITION.TOP_CENTER,
      })
    }
  } catch (error: unknown) {
    const apiError = error as ApiError
    console.error('❌ Wystąpił błąd:', apiError.response?.data || apiError.message)
    toast.error(t('errorServerUnavailable'), {
      position: POSITION.TOP_CENTER,
    })
  }
}
</script>
