<template>
  <div class="w-full max-w-xl mx-auto mt-10">

  <!-- Popup potwierdzenia zagrania karty -->
  <Teleport to="body">
    <Transition
      enter-active-class="transition duration-300 ease-out"
      enter-from-class="opacity-0 scale-90 translate-y-4"
      enter-to-class="opacity-100 scale-100 translate-y-0"
      leave-active-class="transition duration-200 ease-in"
      leave-from-class="opacity-100 scale-100 translate-y-0"
      leave-to-class="opacity-0 scale-90 translate-y-4"
    >
      <div
        v-if="cardPopup.visible"
        class="fixed inset-0 z-50 flex items-center justify-center pointer-events-none"
      >
        <div
          class="pointer-events-auto mx-4 w-full max-w-sm rounded-2xl shadow-2xl overflow-hidden bg-gradient-to-br from-primary-900/95 to-primary-800/95 border border-primary-500/50"
        >
          <!-- Pasek postępu auto-zamknięcia -->
          <div class="h-1 w-full bg-white/10">
            <div
              class="h-full bg-primary-400 transition-all ease-linear"
              :style="{ width: `${cardPopup.progress}%`, transitionDuration: '100ms' }"
            />
          </div>

          <div class="p-5">
            <div class="flex items-start gap-4">
              <!-- Ikona -->
              <div class="flex-shrink-0 w-12 h-12 rounded-full flex items-center justify-center text-2xl bg-primary-500/30">
                <span>⏳</span>
              </div>

              <!-- Treść -->
              <div class="flex-1 min-w-0">
                <p class="font-bold text-lg leading-tight text-primary-300">
                  {{ t('cardPendingTitle') }}
                </p>
                <p class="text-white font-semibold mt-0.5 truncate">{{ cardPopup.cardTitle }}</p>
                <p class="text-white/60 text-sm mt-1">{{ t('cardPendingDesc') }}</p>
              </div>

              <!-- Przycisk zamknięcia -->
              <button
                @click="closeCardPopup"
                class="flex-shrink-0 text-white/40 hover:text-white/80 transition-colors text-xl leading-none"
              >
                ×
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
    <div v-if="loading" class="text-center text-surface-500">{{ t('loadingCards') }}</div>

    <div v-else-if="!displayCards || displayCards.length === 0" class="text-center text-surface-500">
      {{ t('noCardAvailableInThisCategory') }}
    </div>

    <div v-else class="relative">
      <!-- Wskaźniki (lista rozwijana) -->
      <div class="flex justify-center mt-6">
        <div class="relative w-full">
          <!-- Dropdown button -->
          <button
            @click="isDropdownOpen = !isDropdownOpen"
            class="w-full py-4 px-4 bg-accent backdrop-blur-sm rounded-t-2xl text-center text-surface-200 text-lg font-semibold transition-all duration-300 cursor-pointer flex items-center justify-between"
          >
            <span class="flex-1">
              {{ selectedCard ? `${selectedCard.id}: ${selectedCard.title}` : t('selectCard') }}
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
              class="absolute z-50 w-full bg-secondary border-2 border-primary-500/30 rounded-b-xl shadow-2xl max-h-80 overflow-y-auto custom-scrollbar"
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

      <!-- Karta -->
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
            :aria-label="t('previousCard')"
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
            :aria-label="t('nextCard')"
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
          :disabled="isSubmitting"
          class="relative w-full py-2 lg:py-5 mb-2 rounded-3xl font-semibold text-xl transition-all duration-300 overflow-hidden group bg-secondary hover:bg-gradient-to-r hover:from-primary-600/20 hover:to-primary-700/20 border border-primary-500/30 hover:border-primary-500/50 hover:shadow-lg hover:shadow-primary-500/30 text-surface-0"
        >
          <span class="relative z-10">{{ isSubmitting ? t('sending') : buttonLabel }}</span>
          <div
            class="absolute inset-0 bg-gradient-to-r from-primary-500/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
          ></div>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, watchEffect, onMounted, onUnmounted, reactive } from 'vue'
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
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

// --- DEFINICJE INTERFEJSÓW ---
interface Card {
  id: number
  cardsId: number
  title: string
  description: string
  cost: number
  enablers: any[]
  type: 'decision'
}

interface CardsApiResponse {
  decisionCards: Card[]
}

// --- Popup po zagraniu karty (tylko tryb pending) ---
const POPUP_DURATION = 4000
const cardPopup = reactive({ visible: false, cardTitle: '', progress: 100 })
let popupTimer: ReturnType<typeof setTimeout> | null = null
let popupInterval: ReturnType<typeof setInterval> | null = null

const startPopupTimer = () => {
  if (popupTimer) clearTimeout(popupTimer)
  if (popupInterval) clearInterval(popupInterval)
  cardPopup.progress = 100
  const step = 100 / (POPUP_DURATION / 100)
  popupInterval = setInterval(() => { cardPopup.progress = Math.max(0, cardPopup.progress - step) }, 100)
  popupTimer = setTimeout(closeCardPopup, POPUP_DURATION)
}

const showCardPopup = (cardTitle: string, isIndependent: boolean) => {
  if (isIndependent) {
    toast.success(t('cardPlayedTitle'), { timeout: 5000 })
    return
  }
  cardPopup.cardTitle = cardTitle
  cardPopup.visible = true
  startPopupTimer()
}

const showApprovedPopup = () => {
  toast.success(t('cardApprovedTitle'), { timeout: 5000 })
}

const closeCardPopup = () => {
  cardPopup.visible = false
  if (popupTimer) { clearTimeout(popupTimer); popupTimer = null }
  if (popupInterval) { clearInterval(popupInterval); popupInterval = null }
}

onUnmounted(() => { closeCardPopup() })

// --- Reaktywne referencje i stałe ---
const toast = useToast()
const decisionCards = ref<Card[]>([])
const currentIndex = ref(0)
const loading = ref(true)
const fetchError = ref<string | null>(null)
const isDropdownOpen = ref<boolean>(false)
const isSubmitting = ref<boolean>(false)
const selectCard = (index: number) => {
  currentIndex.value = index
  isDropdownOpen.value = false
}

const emit = defineEmits<{ 'card-played': [newBudget: number] }>()

// --- Definiowanie propsów i emitów ---
const props = defineProps({
  deckId: Number,
  gameId: Number,
  teamId: Number,
  boardId: Number,
  gameProcessId: Number,
  currentBudget: { type: Number, default: 0 },
  isOnlineGame: { type: Boolean, default: true },
  isIndependentTeam: { type: Boolean, required: true },
})

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

const displayCards = computed<Card[]>(() => decisionCards.value)

const selectedCard = computed<Card | null>(() => {
  if (!displayCards.value || currentIndex.value >= displayCards.value.length) {
    return null
  }
  return displayCards.value[currentIndex.value]
})

const cardStyle = computed(() => {
  if (!selectedCard.value) return {}

  return { '--tw-gradient-from': '#00b1eb', '--tw-gradient-to': '#008bb5' }
})

const buttonLabel = computed(() => {
  return props.isIndependentTeam ? t('playCard') : t('suggectCard')
})

async function fetchCards() {
  const { deckId, gameId, teamId } = props
  if (deckId == null || gameId == null || teamId == null) {
    fetchError.value = t('missingCardsData')
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
  } catch (error: any) {
    decisionCards.value = []
    fetchError.value = t('errorFetchingCards')
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
    toast.warning(t('noCardSelected'))
    return
  }
  if(isSubmitting.value) return
  isSubmitting.value = true
  const { cardsId, cost, enablers } = selectedCard.value
  const hasEnablers = Array.isArray(enablers) && enablers.length > 0
  const hasSufficientBudget = props.currentBudget >= cost
  const isSuccess = !hasEnablers && hasSufficientBudget
  const apiUrl = isSuccess
    ? apiConfig.player.playCardSuccess(cardsId)
    : apiConfig.player.playCardFailure(cardsId)

  const cardPlayData = {
    gameId: props.gameId,
    teamId: props.teamId,
    deckId: props.deckId,
    boardId: props.boardId,
    gameProcessId: props.gameProcessId,
    cost: cost,
    enablerId: hasEnablers ? Math.min(...(enablers as number[])) : undefined,
  }

  const playedCardTitle = selectedCard.value.title
  try {
    const response = await apiServices.post<{ message?: string; newTeamBudget: number }>(
      apiUrl,
      cardPlayData,
    )
    showCardPopup(playedCardTitle, props.isIndependentTeam ?? false)
    emit('card-played', response.data.newTeamBudget)
    await fetchCards()
  } catch (err: any) {
    if (err.response?.data?.errorCode === 'NotEnoughBudget') {
      toast.warning(t('warningNotEnoughBudget'))
    } else {
      toast.error(err.response?.data?.message ?? t('errorServerUnavailable'))
      console.error('Błąd podczas zagrywania karty:', err)
    }
  } finally {
    isSubmitting.value = false
  }
}

defineExpose({
  fetchCards,
  showApprovedPopup,
})

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
