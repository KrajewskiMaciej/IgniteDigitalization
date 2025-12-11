<template>
  <div class="flex flex-row justify-center gap-3 items-center mt-5 mb-5">
    <div class="flex gap-2">
      <button
        class="border-2 border-lgray-accent py-2 px-4 rounded-md text-center hover:border-accent transition-colors duration-300 flex items-center"
        @click="openCreateGame"
      >
        <font-awesome-icon :icon="faPlus" class="h-4 text-accent mr-2" />
        {{ t('createNewGame') }}
      </button>
    </div>
    <div class="flex gap-2">
      <button
        @click="handleStopAllGames"
        :disabled="isStoppingGames || isEndingGames"
        class="border-2 border-lgray-accent py-2 px-4 rounded-md text-center hover:border-accent transition-colors duration-300 flex items-center disabled:opacity-50 disabled:cursor-not-allowed"
      >
        <font-awesome-icon :icon="faCircleStop" class="h-4 text-accent mr-2" />
        {{ t('stopAllGames') }}
      </button>
      <button
        @click="handleEndAllGames"
        :disabled="isEndingGames || isStoppingGames"
        class="border-2 border-lgray-accent py-2 px-4 rounded-md text-center hover:border-accent transition-colors duration-300 flex items-center disabled:opacity-50 disabled:cursor-not-allowed"
      >
        <font-awesome-icon :icon="faPowerOff" class="h-4 text-accent mr-2" />
        {{ t('endAllGames') }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { faPowerOff, faPlus } from '@fortawesome/free-solid-svg-icons'
import { faCircleStop } from '@fortawesome/free-regular-svg-icons'
import { useToast } from 'vue-toastification'
import apiServices from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'
import { useConfirm } from 'primevue/useconfirm'
import { useI18n } from 'vue-i18n'

const { t } = useI18n();

// --- DEFINICJA TYPU DLA BŁĘDU API ---
interface ApiError {
  response?: {
    data?: {
      message?: string
    }
  }
  message?: string
}

const toast = useToast()
const confirm = useConfirm()
const emit = defineEmits(['openCreateGame', 'update-status'])

const openCreateGame = () => {
  emit('openCreateGame')
}

const isStoppingGames = ref(false)
const isEndingGames = ref(false)

// Handler dla zatrzymywania gier - wywołuje confirm dialog
const handleStopAllGames = () => {
  if (isStoppingGames.value) return

  confirm.require({
    header: t('stopAllGames'),
    message: t('stopAllGamesConfirmation'),
    accept: () => {
      stopAllGames()
    },
    reject: () => {
      // Użytkownik anulował
    },
  })
}

const stopAllGames = async () => {
  isStoppingGames.value = true
  try {
    const response = await apiServices.post(apiConfig.games.stopAll, {})

    if (response.status === 200 || response.status === 204) {
      emit('update-status')
    } else {
      toast.error(t('stopAllGamesError') + ` ${response.status}`);
    }
  } catch (error: unknown) {

    toast.error(t('stopAllGamesError') + error);
  } finally {
    isStoppingGames.value = false
  }
}

const handleEndAllGames = () => {
  if (isEndingGames.value) return

  confirm.require({
    header: t('endAllGames'),
    message: t('endAllGamesConfirmation'),
    accept: () => {
      endAllGames()
    },
    reject: () => {},
  })
}

// Funkcja do kończenia wszystkich gier
const endAllGames = async () => {
  isEndingGames.value = true
  try {
    const response = await apiServices.post(apiConfig.games.endAll, {})

    if (response.status === 200 || response.status === 204) {
      emit('update-status')
    } else {
      toast.error(t('endAllGamesError') + ` ${response.status}`)
    }
  } catch (error: unknown) {
    toast.error(t('endAllGamesError') + ` ${error}`);
  } finally {
    isEndingGames.value = false
  }
}
</script>
