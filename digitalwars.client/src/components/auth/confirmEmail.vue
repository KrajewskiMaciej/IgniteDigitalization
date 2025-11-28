<template>
  <!--Informacja o potwierdzeniu adresu email-->
  <div class="animate-fade-right ">

    <div class="flex justify-center items-center">
      <font-awesome-icon :icon="faEnvelopeOpen" class="mb-3 text-6xl text-accent" />
    </div>
    <h2 class="text-lg sm:text-xl md:text-2xl font-nasalization mb-2 sm:mb-3 text-center">
      {{ t('registrationSuccessful') }}
    </h2>

    

    <div
      class="h-[2px] bg-gradient-to-r from-transparent via-primary-500 to-transparent mb-6 sm:mb-8"
    ></div>

    <div class="text-sm text-gray-300 mb-3 text-center">
      <p>{{ t('oneMoreStepLeft') }}</p>
      <p>
        {{ t('toYourEmailAddress') }} <b>{{ email }}</b>
      </p>
      <p>{{ t('weHaveSentVerificationLink') }}</p>
      <p>{{ t('checkSpamFolder') }}</p>
    </div>
    <div class="flex justify-center items-center">
       <span
          class="text-accent hover:text-purple-300 transition-colors cursor-pointer"
          @click="emit('backToLogin')"
        >
          {{ t('goToLogin') }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { faEnvelopeOpen } from '@fortawesome/free-regular-svg-icons'
import { defineProps, defineEmits } from 'vue'
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
