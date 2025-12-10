<template>
  <div class="w-full h-full animate-fade-right">
    <div class="flex justify-center items-center">
      <font-awesome-icon :icon="faEnvelopeOpen" class="mb-3 sm:mb-4 text-5xl sm:text-6xl text-accent" />
    </div>
    
    <h2 class="text-lg sm:text-xl md:text-2xl font-nasalization mb-3 sm:mb-4 text-center">
      {{ t('registrationSuccessful') }}
    </h2>

    <div
      class="h-[2px] bg-gradient-to-r from-transparent via-primary-500 to-transparent mb-4 sm:mb-6"
    />

    <div class="text-sm sm:text-base text-gray-300 mb-4 sm:mb-6 text-center space-y-1.5">
      <p>{{ t('oneMoreStepLeft') }}</p>
      <p>
        {{ t('toYourEmailAddress') }} <b class="text-white">{{ email }}</b>
      </p>
      <p>{{ t('weHaveSentVerificationLink') }}</p>
      <p class="text-xs sm:text-sm text-gray-400">{{ t('checkSpamFolder') }}</p>
    </div>
    
    <div class="flex justify-center items-center">
      <button
        @click="emit('backToLogin')"
        class="px-5 py-2.5 sm:px-6 sm:py-3 text-sm sm:text-base font-medium text-accent hover:text-white bg-transparent hover:bg-accent/20 border border-accent/50 hover:border-accent rounded-lg transition-all duration-300"
      >
        {{ t('goToLogin') }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { faEnvelopeOpen } from '@fortawesome/free-regular-svg-icons'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const emit = defineEmits(['backToLogin'])

const props = defineProps({
  email: {
    type: String,
    required: true,
  },
})
</script>
