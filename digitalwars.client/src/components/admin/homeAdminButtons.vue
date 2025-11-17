<template>
  <div class="flex flex-row justify-center gap-3 items-center mt-5 mb-5">
    <div class="flex gap-2">
      <button
        class="border-2 border-lgray-accent py-2 px-4 rounded-md text-center hover:border-accent transition-colors duration-300 flex items-center"
        @click="openCreateGame"
      >
        <font-awesome-icon :icon="faPlus" class="h-4 text-accent mr-2" />
        Stwórz nową grę
      </button>
    </div>
    <div class="flex gap-2">
      <button
        @click="handleStopAllGames"
        :disabled="isStoppingGames || isEndingGames"
        class="border-2 border-lgray-accent py-2 px-4 rounded-md text-center hover:border-accent transition-colors duration-300 flex items-center disabled:opacity-50 disabled:cursor-not-allowed"
      >
        <font-awesome-icon :icon="faCircleStop" class="h-4 text-accent mr-2" />
        Zatrzymaj wszystkie gry
      </button>
      <button
        @click="handleEndAllGames"
        :disabled="isEndingGames || isStoppingGames"
        class="border-2 border-lgray-accent py-2 px-4 rounded-md text-center hover:border-accent transition-colors duration-300 flex items-center disabled:opacity-50 disabled:cursor-not-allowed"
      >
        <font-awesome-icon :icon="faPowerOff" class="h-4 text-accent mr-2" />
        Zakończ wszystkie gry
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
    header: 'Zatrzymaj wszystkie gry',
    message: 'Czy na pewno chcesz zatrzymać wszystkie aktywne gry?',
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
      toast.success('Wszystkie gry zostały zatrzymane.')
      emit('update-status')
    } else {
      toast.error(`Nie udało się zatrzymać gier. Serwer odpowiedział: ${response.status}`)
    }
  } catch (error: unknown) {
    const apiError = error as ApiError
    const errorMessage = apiError.response?.data?.message || apiError.message || 'Nieznany błąd'

    console.error('Błąd podczas zatrzymywania gier:', error)
    toast.error(`Błąd podczas zatrzymywania gier: ${errorMessage}`)
  } finally {
    isStoppingGames.value = false
  }
}

const handleEndAllGames = () => {
  if (isEndingGames.value) return

  confirm.require({
    header: 'Zakończ wszystkie gry',
    message:
      'Jesteś pewien, że chesz zakończyć wszystkie aktywne gry? Ta akcja jest nieodwracalna.',
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
      toast.success('Wszystkie gry zostały zakończone.')
      emit('update-status')
    } else {
      toast.error(`Nie udało się zakończyć gier. Serwer odpowiedział: ${response.status}`)
    }
  } catch (error: unknown) {
    const apiError = error as ApiError
    const errorMessage = apiError.response?.data?.message || apiError.message || 'Nieznany błąd'

    console.error('Błąd podczas kończenia gier:', error)
    toast.error(`Błąd podczas kończenia gier: ${errorMessage}`)
  } finally {
    isEndingGames.value = false
  }
}
</script>
