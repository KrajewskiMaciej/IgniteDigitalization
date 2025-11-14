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
      <div class="flex justify-center mt-6">
        <div class="relative w-full">
          <!-- Dropdown button -->
          <button
            @click="isDropdownOpen = !isDropdownOpen"
            class="w-full py-4 px-4 bg-surface-0 backdrop-blur-sm rounded-t-2xl text-center text-surface-900 text-lg font-semibold transition-all duration-300 cursor-pointer flex items-center justify-between"
          >
            <span class="flex-1">
              {{ selectedCard ? `${selectedCard.id}: ${selectedCard.title}` : 'Wybierz kartę' }}
            </span>
            <font-awesome-icon
              :icon="isDropdownOpen ? faChevronUp : faChevronDown"
              class="text-surface-800 transition-transform duration-300"
            />
          </button>

          <!-- Dropdown menu -->
          <transition
            enter-active-class="transition ease-out duration-200"
            enter-from-class="opacity-0 -translate-y-2"
            enter-to-class="opacity-100 translate-y-0"
            leave-active-class="transition ease-in duration-150"
            leave-from-class="opacity-100 translate-y-0"
            leave-to-class="opacity-0 -translate-y-2"
          >
            <div
              v-show="isDropdownOpen"
              class="absolute z-50 w-full bg-surface-800 border-2 border-primary-500/30 rounded-b-xl shadow-2xl max-h-80 overflow-y-auto custom-scrollbar"
            >
              <div class="p-2">
                <button
                  v-for="(card, index) in displayCards"
                  :key="card.id"
                  @click="selectCard(index)"
                  class="w-full text-left px-4 py-3 rounded-lg text-surface-0 font-medium transition-all duration-200 hover:bg-primary-500/20 hover:translate-x-1 mb-1"
                  :class="{
                    'bg-gradient-to-r from-primary-600 to-primary-700 text-white shadow-lg shadow-primary-500/30':
                      currentIndex === index,
                  }"
                >
                  {{ card.id }}: {{ card.title }}
                </button>
              </div>
            </div>
          </transition>
        </div>
      </div>

      <!-- Karta z dynamicznym tłem -->
      <div
        ref="cardRef"
        class="w-full h-full bg-gradient-to-br from-transparent to-transparent rounded-b-2xl shadow-2xl text-surface-0 flex"
        :style="cardStyle"
      >
        <div class="flex h-full">
          <!-- Lewy przycisk -->
          <button
            @click.stop="prevCard"
            class="flex items-center justify-center lg:px-2 hover:bg-black/10 rounded-bl-2xl transition-colors duration-300 ease-out"
            aria-label="Poprzednia karta"
            :disabled="displayCards.length <= 1"
          >
            <font-awesome-icon :icon="faChevronLeft" class="font-bold text-lg lg:text-xl" />
          </button>

          <!-- Środek karty -->
          <div
            v-if="selectedCard"
            class="flex-1 flex flex-col items-center justify-between px-2 text-center py-2"
          >
            <h2 class="text-3xl font-bold mb-2">
              {{ selectedCard.title }}
            </h2>

            <p v-if="props.isOnlineGame" class="text-lg text-surface-100 font-semibold">
              {{ selectedCard.description }}
            </p>

            <p class="text-xs mt-2">ID: {{ selectedCard.id }}</p>
          </div>

          <!-- Prawy przycisk -->
          <button
            @click.stop="nextCard"
            class="flex items-center justify-center lg:px-2 hover:bg-black/10 rounded-br-2xl transition-colors duration-300 ease-out"
            aria-label="Następna karta"
            :disabled="displayCards.length <= 1"
          >
            <font-awesome-icon :icon="faChevronRight" class="font-bold text-lg lg:text-xl" />
          </button>
        </div>
      </div>

      <!-- Przyciski akcji -->
      <div class="flex justify-center mt-6">
        <button
          @click="sendCardSelection"
          class="relative w-full py-2 lg:py-5 mb-2 rounded-3xl font-semibold text-xl transition-all duration-300 overflow-hidden group bg-surface-800 hover:bg-gradient-to-r hover:from-primary-600/20 hover:to-primary-700/20 border border-primary-500/30 hover:border-primary-500/50 hover:shadow-lg hover:shadow-primary-500/30 text-surface-0"
        >
          <span class="relative z-10">{{ buttonLabel }}</span>
          <div
            class="absolute inset-0 bg-gradient-to-r from-primary-500/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
          ></div>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, watchEffect } from 'vue'
import {
  faChevronLeft,
  faChevronRight,
  faChevronUp,
  faChevronDown,
} from '@fortawesome/free-solid-svg-icons'
import apiConfig from '@/services/apiConfig'
import apiServices from '@/services/apiServices'
import { useToast } from 'vue-toastification'
import { useSwipe } from '@vueuse/core'
import { useTemplateRef } from 'vue'

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
const isDropdownOpen = ref<boolean>(false)
const selectCard = (index: number) => {
  currentIndex.value = index
  isDropdownOpen.value = false
}

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

const cardRef = useTemplateRef('cardRef')
const { isSwiping, direction } = useSwipe(cardRef)

// Dla mobilek gesty dotykowe
useSwipe(cardRef, {
  onSwipeEnd(e, direction) {
    if (direction === 'left') {
      prevCard()
    }
    if (direction === 'right') {
      nextCard()
    }
  },
})

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

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 8px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: rgba(30, 41, 59, 0.5);
  border-radius: 4px;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(139, 92, 246, 0.5);
  border-radius: 4px;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: rgba(139, 92, 246, 0.7);
}
</style>
