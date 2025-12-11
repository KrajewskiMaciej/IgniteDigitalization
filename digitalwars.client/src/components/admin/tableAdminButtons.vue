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
          {{ game.status === 'During' ? t('pauseGame') : t('resumeGame') }}
        </button>

        <!-- This button is always available for active/paused games to end them -->
        <button
          v-if="game.status !== 'End'"
          @click="handleEndGame"
          class="flex flex-auto items-center justify-center border-2 border-lgray-accent py-2 px-3 rounded-md hover:border-accent transition-colors duration-300"
          @dblclick.stop
        >
          <font-awesome-icon :icon="faPowerOff" class="h-4 mr-2 text-accent" />
          {{ t('endGame') }}
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
import { useI18n } from 'vue-i18n'

const { t } = useI18n();

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

    emit('update-status', { gameId: id, newStatus: newStatus })

    if (newStatus === 'End') {
      setTimeout(() => {
        router.push('/admin')
      }, 750)
    }
  } catch (error: unknown) {
    toast.error(t('errorUpdatingGameStatus') + ` ${error}`)
  }
}

const handleTogglePause = () => {
  const newStatus = game.value.status === 'During' ? 'Paused' : 'During'
  const action = newStatus === 'Paused' ? t('pause') : t('resume')

  confirm.require({
    message: t('toggleGameStatusConfirmation', { action: action }),
    header: newStatus === 'Paused' ? t('pauseGame') : t('resumeGame'),
    accept: () => {
      updateGameStatus(gameId, newStatus)
    },
    reject: () => {},
  })
}

const handleEndGame = () => {
  confirm.require({
    message: t('endGameConfirmation'),
    header: t('endGame'),
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
    toast.error(t('errorFetchingGameData') + ` ${error}`)
  }
}

// --- LIFECYCLE ---
onMounted(() => {
  getGameDetails()
})
</script>
