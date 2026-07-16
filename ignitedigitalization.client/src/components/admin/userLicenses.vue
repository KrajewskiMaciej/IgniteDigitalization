<template>
  <div>
    <div class="flex items-center justify-center mb-6">
      <h2 class="text-2xl font-nasalization">{{ t('licenses') }}</h2>
    </div>

    <div class="flex justify-center mb-6">
      <font-awesome-icon :icon="faTicket" class="text-6xl" />
    </div>

    <hr class="border-lgray-accent mb-4" />

    <div class="mb-4 flex justify-between items-start">
      <p class="font-bold ml-2">{{ t('gamesInProgress') }}</p>
      <p class="mr-2">{{ licencesData.gamesInProgress }}</p>
    </div>
    <hr class="border-lgray-accent mb-4" />

    <div class="mb-4 flex justify-between items-start">
      <p class="font-bold ml-2">{{ t('gamesCompleted') }}</p>
      <p class="mr-2">{{ licencesData.gamesCompleted }}</p>
    </div>
    <hr class="border-lgray-accent mb-4" />

    <div class="mb-4 flex justify-between items-start">
      <p class="font-bold ml-2">{{ t('licencesLeft') }}</p>
      <p class="mr-2">{{ licencesData.licensesLeft }}</p>
    </div>
    <hr class="border-lgray-accent mb-6" />

    <!-- ponytail: zakup licencji ukryty do czasu wdrożenia płatności -->
    <button
      v-if="false"
      type="button"
      class="relative w-full py-4 rounded-lg font-medium transition-all duration-300 overflow-hidden group text-white mb-5 bg-gradient-to-r from-primary-600 to-primary-700 hover:from-primary-500 hover:to-primary-600 shadow-lg shadow-primary-500/30 hover:shadow-primary-500/50"
    >
      <span class="relative z-10">{{ t('buyMoreLicences') }}</span>
      <div
        class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-400/20 to-primary-400/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
      ></div>
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { faTicket } from '@fortawesome/free-solid-svg-icons'
import { useI18n } from 'vue-i18n'
import apiConfig from '@/services/apiConfig.js'
import apiService from '@/services/apiServices.js'

const { t } = useI18n()

interface LicencesData {
  gamesInProgress: number
  gamesCompleted: number
  licensesLeft: number
}

const licencesData = ref<LicencesData>({
  gamesInProgress: 0,
  gamesCompleted: 0,
  licensesLeft: 0,
})

onMounted(async () => {
  try {
    const response = await apiService.get<LicencesData>(apiConfig.admin.settings.licenses)

    licencesData.value = {
      gamesInProgress: response.data.gamesInProgress,
      gamesCompleted: response.data.gamesCompleted,
      licensesLeft: response.data.licensesLeft,
    }
  } catch (error) {
    console.error('Błąd podczas pobierania danych o licencjach:', error)
  }
})
</script>
