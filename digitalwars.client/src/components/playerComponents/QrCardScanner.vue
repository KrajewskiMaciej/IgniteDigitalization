<template>
  <div class="w-full max-w-xl mx-auto flex flex-col gap-4">
    <!-- Skaner QR -->
    <div class="relative w-full aspect-square max-h-72 rounded-2xl overflow-hidden border border-primary-500/30 shadow-lg bg-black">
      <qrcode-stream
        v-if="!cameraError"
        class="w-full h-full"
        @detect="onDetect"
        @error="onCameraError"
        @camera-on="cameraReady = true"
      >
        <!-- Ładowanie kamery -->
        <div
          v-if="!cameraReady"
          class="absolute inset-0 flex items-center justify-center bg-black/80"
        >
          <p class="text-surface-300 text-sm">{{ t('initializingCamera') }}</p>
        </div>

        <!-- Ramka celownika -->
        <div class="absolute inset-0 pointer-events-none">
          <div class="absolute top-4 left-4 w-8 h-8 border-t-2 border-l-2 border-primary-400"></div>
          <div class="absolute top-4 right-4 w-8 h-8 border-t-2 border-r-2 border-primary-400"></div>
          <div class="absolute bottom-4 left-4 w-8 h-8 border-b-2 border-l-2 border-primary-400"></div>
          <div class="absolute bottom-4 right-4 w-8 h-8 border-b-2 border-r-2 border-primary-400"></div>
        </div>
      </qrcode-stream>

      <!-- Błąd kamery -->
      <div
        v-if="cameraError"
        class="absolute inset-0 flex items-center justify-center bg-black/80 px-4 text-center"
      >
        <p class="text-red-400 text-sm">{{ cameraError }}</p>
      </div>
    </div>

    <p class="text-center text-surface-400 text-sm">{{ t('scanCardQrHint') }}</p>

    <!-- Przycisk przełączenia na listę kart -->
    <button
      @click="emit('switch-to-carousel')"
      class="relative w-full py-3 rounded-2xl font-semibold text-sm transition-all duration-300 overflow-hidden group bg-secondary border border-primary-500/30 hover:border-primary-500/50 text-surface-200"
    >
      <span class="relative z-10">{{ t('switchToCardList') }}</span>
      <div
        class="absolute inset-0 bg-gradient-to-r from-primary-500/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
      ></div>
    </button>

    <!-- Modal potwierdzenia -->
    <Teleport to="body">
      <div
        v-if="pendingCard"
        class="fixed inset-0 z-50 flex items-end sm:items-center justify-center"
      >
        <div class="absolute inset-0 bg-black/70" @click="cancelConfirm"></div>

        <div
          class="relative z-10 bg-secondary border border-surface-700 rounded-t-2xl sm:rounded-2xl w-full sm:max-w-md p-6 flex flex-col gap-4 animate-jump-in"
        >
          <h2 class="text-white font-nasalization text-lg text-center">
            {{ t('confirmCardPlay') }}
          </h2>

          <!-- Mini podgląd karty -->
          <div
            class="rounded-xl p-4 text-center text-surface-0 font-semibold"
            :style="pendingCardStyle"
          >
            <p class="text-2xl font-bold">{{ pendingCard.title }}</p>
            <p class="text-xs mt-1 text-surface-200">ID: {{ pendingCard.id }}</p>
            <p v-if="props.isOnlineGame" class="text-sm mt-2 text-surface-100">{{ pendingCard.description }}</p>
          </div>

          <p class="text-surface-300 text-sm text-center">{{ t('confirmCardPlayHint') }}</p>

          <div class="flex gap-3 mt-2">
            <button
              @click="cancelConfirm"
              class="flex-1 py-3 rounded-xl font-semibold border border-surface-600 text-surface-300 hover:border-surface-400 transition-colors duration-200"
            >
              {{ t('cancel') }}
            </button>
            <button
              @click="confirmPlay"
              :disabled="isSubmitting"
              class="flex-1 py-3 rounded-xl font-semibold bg-gradient-to-r from-primary-600 to-primary-700 text-white shadow-lg shadow-primary-500/30 disabled:opacity-50 transition-all duration-200"
            >
              {{ isSubmitting ? '...' : buttonLabel }}
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watchEffect } from 'vue'
import { QrcodeStream } from 'vue-qrcode-reader'
import { useToast } from 'vue-toastification'
import { useI18n } from 'vue-i18n'
import apiConfig from '@/services/apiConfig'
import apiServices from '@/services/apiServices'

const { t } = useI18n()
const toast = useToast()

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
  hardwareCards: Card[]
  softwareCards: Card[]
}

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

const emit = defineEmits<{
  (e: 'switch-to-carousel'): void
}>()

// --- Stan kamery ---
const cameraReady = ref(false)
const cameraError = ref<string | null>(null)

// --- Karty ---
const allCards = ref<Card[]>([])

const onCameraError = (err: Error) => {
  cameraError.value = t('cameraError')
  console.error('[QrCardScanner] Camera error:', err)
}

// --- Potwierdzenie ---
const pendingCard = ref<Card | null>(null)
const isSubmitting = ref(false)
let detectLocked = false

const pendingCardStyle = computed(() => {
  if (!pendingCard.value) return {}
  switch (pendingCard.value.type) {
    case 'decision': return { background: 'linear-gradient(135deg, #00b1eb, #008bb5)' }
    case 'software': return { background: 'linear-gradient(135deg, #009641, #007534)' }
    case 'hardware': return { background: 'linear-gradient(135deg, #ef7d00, #c06400)' }
    default: return { background: 'linear-gradient(135deg, #5DBB63, #607D3B)' }
  }
})

const buttonLabel = computed(() =>
  props.isIndependentTeam ? t('playCard') : t('suggectCard'),
)

// --- Fetch kart ---
async function fetchCards() {
  const { deckId, gameId, teamId } = props
  if (deckId == null || gameId == null || teamId == null) return

  try {
    const response = await apiServices.get<CardsApiResponse>(
      apiConfig.player.getCards(deckId, gameId, teamId),
    )
    const decisions: Card[] = (response.data?.decisionCards ?? []).map((c) => ({ ...c, type: 'decision' as const }))
    const hardware: Card[] = (response.data?.hardwareCards ?? []).map((c) => ({ ...c, type: 'hardware' as const }))
    const software: Card[] = (response.data?.softwareCards ?? []).map((c) => ({ ...c, type: 'software' as const }))
    allCards.value = [...decisions, ...hardware, ...software]
  } catch (err) {
    console.error('[QrCardScanner] Błąd pobierania kart:', err)
  }
}

interface DetectedBarcode {
  rawValue: string
}

const onDetect = (detected: DetectedBarcode[]) => {
  if (detectLocked || pendingCard.value) return
  const raw = detected?.[0]?.rawValue?.trim()
  if (!raw) return

  const cardId = parseInt(raw, 10)
  if (isNaN(cardId)) {
    toast.warning(t('qrNotACard'))
    return
  }

  const found = allCards.value.find((c) => c.id === cardId)
  if (!found) {
    toast.warning(t('cardNotFound'))
    return
  }

  detectLocked = true
  pendingCard.value = found
}

const cancelConfirm = () => {
  pendingCard.value = null
  setTimeout(() => { detectLocked = false }, 1500)
}

const confirmPlay = async () => {
  if (!pendingCard.value || isSubmitting.value) return
  isSubmitting.value = true

  const { id: cardId, cost, enablers } = pendingCard.value
  const hasEnablers = Array.isArray(enablers) && enablers.length > 0
  const hasSufficientBudget = props.currentBudget >= cost
  const isSuccess = !hasEnablers && hasSufficientBudget
  const apiUrl = isSuccess
    ? apiConfig.player.playCardSuccess(cardId)
    : apiConfig.player.playCardFailure(cardId)

  const cardPlayData = {
    gameId: props.gameId,
    teamId: props.teamId,
    deckId: props.deckId,
    boardId: props.boardId,
    gameProcessId: props.gameProcessId,
    cost,
    enablerId: hasEnablers ? Math.min(...(enablers as number[])) : undefined,
  }

  try {
    await apiServices.post(apiUrl, cardPlayData)
    toast.success(t('cardPlayed'))
    pendingCard.value = null
    setTimeout(() => { detectLocked = false }, 2000)
  } catch (err: any) {
    if (err.response?.data?.errorCode === 'NotEnoughBudget') {
      toast.warning(t('warningNotEnoughBudget'))
    } else {
      toast.error(err.response?.data?.message ?? t('errorServerUnavailable'))
    }
    pendingCard.value = null
    setTimeout(() => { detectLocked = false }, 1500)
  } finally {
    isSubmitting.value = false
  }
}

watchEffect(() => {
  const { deckId, gameId, teamId } = props
  if (deckId != null && gameId != null && teamId != null) {
    fetchCards()
  }
})
</script>
