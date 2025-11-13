<template>
  <div class="w-full max-w-xl mx-auto mt-10">
    <div v-if="loading" class="text-center text-white">Ładowanie kart...</div>
    <div v-else-if="fetchError" class="text-center text-red-500">
      Błąd ładowania kart: {{ fetchError }}
    </div>

    <div v-else-if="!displayCards || displayCards.length === 0" class="text-center text-white">
      Brak dostępnych kart w tej kategorii.
    </div>

    <div v-else class="relative">
      <!-- Wskaźniki (lista rozwijana) -->
      <div class="flex flex-wrap justify-center gap-2 mt-6">
        <select v-model="currentIndex" class="w-full py-5 border rounded-t-2xl text-center">
          <option v-for="(card, index) in displayCards" :key="card.id" :value="index">
            {{ card.id }}: {{ card.title }}
          </option>
        </select>
      </div>

      <!-- Karta z dynamicznym tłem -->
      <div
        @mousedown="startHold"
        @mouseup="cancelHold"
        @mouseleave="cancelHold"
        class="w-full h-full bg-gradient-to-br from-transparent to-transparent rounded-b-2xl shadow-2xl text-white"
        :style="cardStyle"
      >
        <div class="flex h-full">
          <!-- Lewy przycisk -->
          <button
            @click.stop="prevCard"
            class="w-12 flex items-center justify-center py-5 hover:bg-black/20 rounded-bl-2xl transition"
            aria-label="Poprzednia karta"
            :disabled="displayCards.length <= 1"
          >
            <p>&lt;</p>
          </button>

          <!-- Środek karty -->
          <div
            v-if="selectedCard"
            class="flex-1 flex flex-col items-center justify-center px-6 text-center py-5"
          >
            <h2 class="text-3xl font-bold mb-2">
              {{ selectedCard.title }}
            </h2>

            <p v-if="props.isOnlineGame" class="text-l text-white/90 font-bold">
              {{ selectedCard.description }}
            </p>

            <p class="text-xs mt-2">ID: {{ selectedCard.id }}</p>
          </div>

          <!-- Prawy przycisk -->
          <button
            @click.stop="nextCard"
            class="w-12 flex items-center justify-center py-5 hover:bg-black/20 rounded-br-2xl transition"
            aria-label="Następna karta"
            :disabled="displayCards.length <= 1"
          >
            <p>&gt;</p>
          </button>
        </div>
      </div>

      <!-- Przyciski akcji -->
      <div class="flex justify-center mt-6">
        <button
          @click="sendCardSelection"
          class="bg-black text-white px-6 py-2 rounded-full shadow-md hover:bg-gray-800 transition-colors duration-200"
        >
          {{ buttonLabel }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, watchEffect } from 'vue'
import apiConfig from '@/services/apiConfig'
import apiServices from '@/services/apiServices'
import { useToast } from 'vue-toastification'

// --- DEFINICJE INTERFEJSÓW ---
interface Card {
  id: number
  title: string
  description: string
  cost: number
  enablers: any[]
  type: 'decision' | 'hardware' | 'software'
}

interface CardsApiResponse {
  decisionCards: Card[]
  hardwareCards: Card[] // Zakładamy, że API rozdziela karty przedmiotów
  softwareCards: Card[]
}

// --- Reaktywne referencje i stałe ---
const toast = useToast()
const decisionCards = ref<Card[]>([])
const itemCards = ref<Card[]>([])
const currentIndex = ref(0)
const loading = ref(true)
const fetchError = ref<string | null>(null)

// --- Definiowanie propsów i emitów ---
const props = defineProps({
  deckId: Number,
  gameId: Number,
  teamId: Number,
  boardId: Number,
  gameProcessId: Number,
  currentBudget: { type: Number, default: 0 },
  showingDecisionCards: { type: Boolean, required: true },
  isOnlineGame: { type: Boolean, default: true },
  isIndependentTeam: { type: Boolean, default: true },
})

const emit = defineEmits(['card-action-completed'])

// --- Computed Properties (Właściwości Obliczeniowe) ---
const displayCards = computed<Card[]>(() => {
  return props.showingDecisionCards ? decisionCards.value : itemCards.value
})

const selectedCard = computed<Card | null>(() => {
  if (!displayCards.value || currentIndex.value >= displayCards.value.length) {
    return null
  }
  return displayCards.value[currentIndex.value]
})

const cardStyle = computed(() => {
  if (!selectedCard.value) return {}

  let colorFrom = '#5DBB63'
  let colorTo = '#607D3B'

  switch (selectedCard.value.type) {
    case 'decision':
      colorFrom = '#00b1eb'
      colorTo = '#008bb5'
      break
    case 'software':
      colorFrom = '#009641'
      colorTo = '#007534'
      break
    case 'hardware':
      colorFrom = '#ef7d00'
      colorTo = '#c06400'
      break
  }
  return { '--tw-gradient-from': colorFrom, '--tw-gradient-to': colorTo }
})

const buttonLabel = computed(() => {
  return props.isIndependentTeam ? 'Wybierz kartę' : 'Sugeruj kartę'
})

// --- Metody ---
const startHold = () => {
  /* ... */
}
const cancelHold = () => {
  /* ... */
}

async function fetchCards() {
  const { deckId, gameId, teamId } = props
  if (deckId == null || gameId == null || teamId == null) {
    fetchError.value = 'Brak wymaganych danych do pobrania kart.'
    loading.value = false
    return
  }
  loading.value = true
  fetchError.value = null

  try {
    const response = await apiServices.get<CardsApiResponse>(
      apiConfig.player.getCards(deckId, gameId, teamId),
      { params: { gameId, teamId } },
    )

    decisionCards.value = (response.data?.decisionCards ?? []).map((card) => ({
      ...card,
      type: 'decision',
    }))

    // --- KLUCZOWA POPRAWKA ---
    // Jawnie typujemy stałe jako Card[], aby TypeScript poprawnie
    // zinterpretował typ właściwości 'type' i uniknął błędu.
    const hardware: Card[] = (response.data?.hardwareCards ?? []).map((card) => ({
      ...card,
      type: 'hardware',
    }))
    const software: Card[] = (response.data?.softwareCards ?? []).map((card) => ({
      ...card,
      type: 'software',
    }))
    // --- KONIEC POPRAWKI ---

    itemCards.value = [...software, ...hardware]
  } catch (error: any) {
    decisionCards.value = []
    itemCards.value = []
    fetchError.value = 'Wystąpił błąd podczas pobierania kart.'
    console.error('[CardCarousel] Błąd pobierania kart:', error)
  } finally {
    loading.value = false
  }
}

const nextCard = () => {
  if (displayCards.value.length === 0) return
  currentIndex.value = (currentIndex.value + 1) % displayCards.value.length
}

const prevCard = () => {
  if (displayCards.value.length === 0) return
  currentIndex.value =
    (currentIndex.value - 1 + displayCards.value.length) % displayCards.value.length
}

const sendCardSelection = async () => {
  if (!selectedCard.value) {
    toast.warning('Nie wybrano żadnej karty.')
    return
  }
  const { id: cardId, cost, enablers } = selectedCard.value
  const hasEnablers = Array.isArray(enablers) && enablers.length > 0
  const hasSufficientBudget = props.currentBudget >= cost
  const isSuccess = !hasEnablers && hasSufficientBudget

  const cardPlayData = {
    gameId: props.gameId,
    teamId: props.teamId,
    deckId: props.deckId,
    boardId: props.boardId,
    gameProcessId: props.gameProcessId,
    cost: cost,
  }

  const apiUrl = isSuccess
    ? apiConfig.player.playCardSuccess(cardId)
    : apiConfig.player.playCardFailure(cardId)

  try {
    const response = await apiServices.post<{ message?: string; newTeamBudget: number }>(
      apiUrl,
      cardPlayData,
    )
    toast.success(response.data?.message ?? 'Akcja została wykonana.')
    emit('card-action-completed', {
      success: true,
      newBudget: response.data.newTeamBudget,
    })
    await fetchCards()
  } catch (err: any) {
    toast.error(err.response?.data?.message ?? 'Wystąpił błąd podczas komunikacji z serwerem.')
    console.error('Błąd podczas zagrywania karty:', err)
    emit('card-action-completed', { success: false })
  }
}

// --- Watchers ---
watch(displayCards, () => {
  currentIndex.value = 0
})

watchEffect(() => {
  const { deckId, gameId, teamId } = props
  if (deckId != null && gameId != null && teamId != null) {
    fetchCards()
  }
})
</script>
