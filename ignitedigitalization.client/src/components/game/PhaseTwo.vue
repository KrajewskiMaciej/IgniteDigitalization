<template>
  <div v-if="isVisible" class="fixed inset-0 flex items-center justify-center z-50">
    <!-- Tło -->
    <div class="absolute inset-0 bg-black/70" @click="handleClose"></div>

    <!-- Modal -->
    <div
      class="bg-secondary z-20 text-surface-500 relative border border-surface-700 animate-jump-in w-full h-full flex flex-col justify-center p-6 sm:w-[90vw] sm:max-w-lg sm:h-auto sm:max-h-[90vh] sm:rounded-lg sm:p-8"
    >
      <!-- Treść główna -->
      <div class="flex flex-col justify-center items-center text-center px-2">
        <h1 class="text-surface-500 font-nasalization text-xl md:text-2xl mb-4">
          {{ t('phaseTwoTitle') }}
        </h1>

        <p class="text-base text-surface-300">
          {{ t('phaseTwoIntro') }}
          <span class="text-primary-400 font-semibold">{{ teamName }}</span
          >.
        </p>

        <p class="font-bold text-surface-200 mt-2 text-sm md:text-base">
          {{ t('phaseTwoInfo') }}
        </p>
      </div>

      <hr class="my-6 border-surface-700" />

      <!-- Przyciski -->
      <div class="mt-6 flex justify-center px-2">
        <Button type="button" class="px-6 w-full sm:w-auto" @click="handleClose" label="OK" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import Button from '@/components/base/AppButton.vue'

const { t } = useI18n()

defineProps<{
  isVisible: boolean
  teamName: string
}>()

const emit = defineEmits<{
  (e: 'close'): void
}>()

const handleClose = () => {
  emit('close')
}
</script>
