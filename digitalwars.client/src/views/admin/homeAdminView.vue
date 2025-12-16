<template>
  <div class="w-full h-full text-white flex flex-col">
    <div class="m-4 px-2 py-2 flex-1 flex flex-col overflow-hidden">
      <h1
        class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-white mb-2 text-center mt-2"
      >
        {{ t('gamesInSession') }}
      </h1>
      <homeAdminButtons @open-create-game="showCreateGame = true" />
      <hr class="mt-2 border-lgray-accent" />

      <div v-if="loadingGames" class="text-center py-4 text-surface-400">{{ t('loadingGamesInSession') }}</div>
      <div v-else-if="fetchError" class="text-center py-4 text-red-500">{{ fetchError }}</div>
      <div v-else-if="activeGames.length === 0" class="text-center py-4 text-surface-400">
        {{ t('noActiveGames') }}
      </div>
      <div v-else class="overflow-auto px-2 pb-4">
        <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4 mt-6 auto-rows-min">
          <gameCard
            v-for="game in activeGames"
            :key="game.id"
            :game="game"
            :color="getGameColor(game.id)"
            @update-status="handleUpdateGameStatus"
          />
        </div>
      </div>
    </div>
    <createGame
      :isVisible="showCreateGame"
      @close="showCreateGame = false"
      @gameCreated="handleGameCreated"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useToast } from 'vue-toastification'
import gameCard from '@/components/game/gameCard.vue'
import homeAdminButtons from '@/components/admin/homeAdminButtons.vue'
import CreateGame from '@/components/game/createGame.vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
// FIX: Usunięto rozszerzenia .js z importów
import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'

// --- Definicje interfejsów ---
interface Game {
  id: number
  name: string
  status: string
  // Możesz dodać inne właściwości, które zwraca Twoje API
}

interface GameCreationResponse {
  message?: string
  // Inne pola z odpowiedzi po utworzeniu gry
}

const toast = useToast()
const showCreateGame = ref(false)

// FIX: Jawne typowanie dla zmiennych stanu
const activeGames = ref<Game[]>([])
const loadingGames = ref(false)
const fetchError = ref<string | null>(null)

// FIX: Dodano typ dla parametru funkcji
const getGameColor = (gameId: number): string => {
  const colors = [
    '#E53E3E',
    '#3182CE',
    '#38A169',
    '#D69E2E',
    '#805AD5',
    '#D53F8C',
    '#DD6B20',
    '#319795',
    '#5A67D8',
  ]
  // Użycie modulo zapewnia, że zawsze dostaniemy prawidłowy indeks
  return colors[gameId % colors.length]
}

const fetchActiveGames = async () => {
  loadingGames.value = true
  fetchError.value = null
  try {
    const response = await apiService.get(apiConfig.games.getAll)
    // FIX: Rzutowanie typu danych z odpowiedzi API
    activeGames.value = response.data as Game[]
  } catch (error: any) {
    // FIX: Jawne typowanie błędu
    const errorMessage =
      error.response?.data?.message || error.message || 'Wystąpił nieoczekiwany błąd.'
    fetchError.value = `Nie udało się pobrać gier: ${errorMessage}`
    console.error('Błąd pobierania aktywnych gier:', error)
    toast.error(t('errorFetchingGames'))
  } finally {
    loadingGames.value = false
  }
}

// FIX: Dodano typ dla parametru funkcji
const handleGameCreated = (creationResponse: GameCreationResponse) => {
  fetchActiveGames() // Odśwież listę gier po dodaniu nowej
}

// FIX: Uproszczona i w pełni otypowana funkcja
const handleUpdateGameStatus = async (payload: { gameId: number; newStatus: string }) => {
  const { gameId, newStatus } = payload

  try {
    // FIX: Wysyłanie danych w ciele żądania jako obiekt
    const apiPayload = { status: newStatus }
    const response = await apiService.put(apiConfig.games.updateStatus(gameId), apiPayload)


    // Najprostsze i najbezpieczniejsze podejście: odśwież całą listę
    await fetchActiveGames()
  } catch (error: any) {
    const errorMessage = error.response?.data?.message || 'Błąd serwera.'
    toast.error(t('errorUpdatingGameStatus'))
    console.error(`Błąd aktualizacji statusu gry ${gameId}:`, error)
  }
}

onMounted(() => {
  fetchActiveGames()
})
</script>
