<template>
  <!-- Use v-if to prevent rendering before game data is loaded -->
  <div v-if="game.status" class="flex flex-row justify-center gap-3 items-center mt-5 mb-5">
    <div class="flex gap-2">
      <div class="flex flex-row gap-2">
        <!-- This button only shows for games that are 'During' or 'Paused' -->
        <button
          v-if="game.status === 'During' || game.status === 'Paused'"
          @click="handleTogglePause"
          class="flex flex-auto items-center justify-center border-2 border-lgray-accent py-2 px-3 rounded-md hover:border-accent transition-colors duration-300"
          @dblclick.stop
        >
          <font-awesome-icon
            :icon="game.status === 'During' ? faCircleStop : faCirclePlay"
            class="h-4 text-accent mr-2"
          />
          {{ game.status === 'During' ? 'Wstrzymaj grę' : 'Wznów grę' }}
        </button>

        <!-- This button is always available for active/paused games to end them -->
        <button
          v-if="game.status !== 'End'"
          @click="handleEndGame"
          class="flex flex-auto items-center justify-center border-2 border-lgray-accent py-2 px-3 rounded-md hover:border-accent transition-colors duration-300"
          @dblclick.stop
        >
          <font-awesome-icon :icon="faPowerOff" class="h-4 mr-2 text-accent" />
          Zakończ grę
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { faCircleStop, faCirclePlay, faPowerOff } from '@fortawesome/free-solid-svg-icons'
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import { useConfirm } from 'primevue/useconfirm'
import apiService from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'

// --- DEFINICJA TYPÓW ---
type GameStatus = 'During' | 'Paused' | 'End'

interface Game {
  status?: GameStatus
  name?: string
}

interface ApiError {
  response?: {
    data?: {
      message?: string
    }
  }
}

// --- HOOKS ---
const route = useRoute()
const router = useRouter()
const toast = useToast()
const confirm = useConfirm()
const emit = defineEmits(['update-status'])

// --- ZMIENNE REAKTYWNE ---
const gameId = Number(route.params.gameId)
const game = ref<Game>({})

// --- FUNKCJE ---
const updateGameStatus = async (id: number, newStatus: GameStatus) => {
  try {
    const apiPayload = { status: newStatus }
    const response = await apiService.put(apiConfig.games.updateStatus(gameId), apiPayload)

    if (game.value) {
      game.value.status = newStatus
    }

    toast.success(
      (response.data as { message: string }).message ||
        `Status gry został pomyślnie zaktualizowany.`,
    )
    emit('update-status', { gameId: id, newStatus: newStatus })

    if (newStatus === 'End') {
      setTimeout(() => {
        router.push('/admin')
      }, 750)
    }
  } catch (error: unknown) {
    const apiError = error as ApiError
    const errorMessage = apiError.response?.data?.message || 'Wystąpił nieznany błąd.'

    console.error(`Błąd aktualizacji statusu gry ${id}:`, error)
    toast.error(`Nie udało się zaktualizować statusu gry: ${errorMessage}`)
  }
}

const handleTogglePause = () => {
  const newStatus = game.value.status === 'During' ? 'Paused' : 'During'
  const action = newStatus === 'Paused' ? 'wstrzymać' : 'wznowić'

  confirm.require({
    message: `Czy na pewno chcesz ${action} grę ?`,
    header: newStatus === 'Paused' ? 'Wstrzymaj grę' : 'Wznów grę',
    rejectLabel: 'Anuluj',
    acceptLabel: 'Potwierdź',
    accept: () => {
      updateGameStatus(gameId, newStatus)
    },
    reject: () => {},
  })
}

const handleEndGame = () => {
  confirm.require({
    message: `Czy na pewno chcesz zakończyć grę? Tej operacji nie można cofnąć.`,
    header: 'Zakończ grę',
    rejectLabel: 'Anuluj',
    acceptLabel: 'Zakończ',
    accept: () => {
      updateGameStatus(gameId, 'End')
    },
    reject: () => {},
  })
}

const getGameDetails = async () => {
  try {
    const response = await apiService.get<Game>(apiConfig.admin.games.getGames(gameId))
    game.value = response.data
  } catch (error) {
    console.error('Błąd przy pobieraniu danych gry:', error)
    toast.error('Nie udało się załadować szczegółów gry.')
  }
}

// --- LIFECYCLE ---
onMounted(() => {
  getGameDetails()
})
</script>
