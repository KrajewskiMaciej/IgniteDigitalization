<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <!-- Nagłówek -->
    <div class="text-center">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-surface-500 mb-2">
        {{ t('decisionPanel') }}
      </h1>
      <p class="text-surface-400 text-sm md:text-base">
        {{ t('manageDecisionsAndItemsForTables') }}
      </p>
    </div>

    <div class="max-w-7xl mx-auto w-full">
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Lewa kolumna - Akcje -->
        <div class="space-y-6">
          <!-- Sekcja wyboru akcji -->
          <div class="border border-surface-700 rounded-xl p-6 bg-secondary shadow-2xl">
            <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
              <div class="bg-primary-500/20 p-3 rounded-lg">
                <font-awesome-icon :icon="faGamepad" class="h-6 text-primary-400" />
              </div>
              <h2 class="text-xl md:text-2xl font-bold text-surface-500">{{ t('actionManagment') }}</h2>
            </div>

            <!-- Wybór stołu -->
            <div v-if="!teamId" class="mb-4">
              <label class="block mb-2 text-sm font-semibold text-surface-300">{{
                t('selectTable')
              }}</label>
              <Dropdown
                v-model="selectedTableId"
                :options="tables"
                optionLabel="teamName"
                optionValue="teamId"
                :placeholder="t('selectTablePlaceholder')"
                class="w-full"
              >
                <template #option="slotProps">
                  <div class="flex items-center justify-between gap-2 w-full">
                    <div class="flex gap-2 items-center">
                      <div
                        :style="{ backgroundColor: slotProps.option.teamColor }"
                        class="w-4 h-4 rounded-full"
                      ></div>
                      <span>{{ slotProps.option.teamName }}</span>
                    </div>
                    <span class="text-green-400"
                      >{{ slotProps.option.teamBud }} {{ t('bits') }}</span
                    >
                  </div>
                </template>
              </Dropdown>
            </div>
            <div class="mb-4">
              <label class="block mb-2 text-sm font-semibold text-surface-300">{{
                t('selectCard')
              }}</label>
              <Dropdown
                v-model="selectedCardId"
                :options="cards"
                optionLabel="title"
                optionValue="id"
                :placeholder="t('selectCardPlaceholder')"
                class="w-full"
                :disabled="loading.cards"
              >
                <template #value="slotProps">
                  <div v-if="slotProps.value" class="flex items-center gap-2">
                    <span class="text-blue-400">#{{ slotProps.value }}</span>
                    <span>{{ cards.find((c) => c.id === slotProps.value)?.title }}</span>
                  </div>
                  <span v-else class="text-surface-400">{{ slotProps.placeholder }}</span>
                </template>
                <template #option="slotProps">
                  <div class="flex items-center gap-2">
                    <span class="text-blue-400">#{{ slotProps.option.id }}</span>
                    <span>{{ slotProps.option.title }}</span>
                  </div>
                </template>
              </Dropdown>
            </div>

            <!-- Opis wybranej karty -->
            <div
              v-if="selectedCard"
              class="bg-secondary rounded-lg p-4 border border-surface-700 mb-4"
            >
              <p class="text-sm text-surface-400 mb-1">{{ t('description') }}</p>
              <p class="text-sm text-surface-300">{{ selectedCard?.description }}</p>
              <div class="flex items-center justify-between mt-3 pt-3 border-t border-surface-700">
                <span class="text-sm text-surface-400">{{ t('cost') }}:</span>
                <span class="text-lg font-bold text-green-400">
                  {{ selectedCard?.cost || 0 }}
                  {{ t('bits') }}
                </span>
              </div>
            </div>

            <!-- Budżet drużyny -->
            <div
              v-if="selectedTableId"
              class="bg-secondary rounded-lg p-4 border border-surface-700 mb-4"
            >
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm text-surface-400">{{ t('teamBudget') }}:</p>
                  <p class="text-sm font-semibold text-surface-500">{{ selectedTeam?.teamName }}</p>
                </div>
                <span class="text-2xl font-bold text-green-400"
                  >{{ currentBits }} {{ t('bits') }}</span
                >
              </div>
            </div>

            <!-- Przyciski akcji -->
            <div class="flex justify-center">
              <Button
                :disabled="!selectedCardId || !selectedTableId || isSubmitting"
                @click="playCard"
                :label="isSubmitting ? t('sending') : t('playCard')"
                size="large"
                class="w-full"
              >
              </Button>
            </div>
          </div>

          <!-- Plansza rywali -->
          <div class="w-full flex justify-center">
            <GameBoard :config="enemyFormData" :game-mode="false" :pawns="enemyPawns" :use-percentage="true" />
          </div>
        </div>

        <!-- Prawa kolumna - Panel decyzji -->
        <div class="border border-surface-700 rounded-xl p-6 bg-secondary shadow-2xl">
          <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
            <div class="bg-yellow-500/20 p-3 rounded-lg">
              <font-awesome-icon :icon="faClock" class="h-6 text-yellow-400" />
            </div>
            <h2 class="text-xl md:text-2xl font-bold text-surface-500">{{ t('decisionPanel') }}</h2>
          </div>

          <!-- Przełącznik widoku decyzji -->
          <div class="flex gap-3 mb-5">
            <Button
              :label="t('toApprove')"
              :severity="decisionMode === 'pending' ? undefined : 'secondary'"
              @click="decisionMode = 'pending'"
              class="flex-1"
              outlined
            >
              <template #icon>
                <font-awesome-icon :icon="faClock" class="h-4" />
              </template>
            </Button>
            <Button
              :label="t('history')"
              :severity="decisionMode === 'history' ? undefined : 'secondary'"
              @click="decisionMode = 'history'"
              class="flex-1"
              outlined
            >
              <template #icon>
                <font-awesome-icon :icon="faHistory" class="h-4" />
              </template>
            </Button>
          </div>

          <!-- Decyzje do zatwierdzenia -->
          <div
            v-if="decisionMode === 'pending'"
            class="space-y-3 max-h-[75vh] overflow-y-auto custom-scrollbar"
          >
            <div v-if="loadingPending" class="text-center py-8">
              <ProgressSpinner style="width: 3rem; height: 3rem" strokeWidth="4" />
              <p class="text-surface-400 mt-3">{{ t('loadingSuggestions') }}</p>
            </div>

            <div
              v-else-if="pendingDecisions.length === 0"
              class="text-center py-12 border border-dashed border-surface-700 rounded-xl bg-secondary/50"
            >
              <div
                class="bg-secondary/50 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-3"
              >
                <font-awesome-icon :icon="faClock" class="h-8 text-surface-600" />
              </div>
              <p class="text-surface-400 text-sm font-medium">{{ t('noDecisionsToApprove') }}</p>
            </div>

            <div
              v-for="entry in pendingDecisions"
              :key="entry.logId"
              class="border-l-4 border-yellow-500 rounded-lg p-4 bg-secondary shadow-lg"
            >
              <div class="flex items-start justify-between mb-2">
                <div>
                  <p class="text-surface-500 font-semibold">{{ entry.tableName }}</p>
                  <p class="text-sm text-surface-400">{{ t('suggestsCard') }}</p>
                </div>
                <span
                  class="px-2 py-1 bg-yellow-500/20 text-yellow-400 text-xs font-semibold rounded"
                >
                  {{ t('awaits') }}
                </span>
              </div>
              <div class="mb-2">
                <p class="text-lg font-bold text-primary-400 leading-tight">
                  {{ entry.cardTitle }}
                </p>
                <p class="text-xs text-surface-300 mt-1">
                  {{ t('cardId') }}
                  <span class="font-semibold text-surface-400">{{ entry.cardId }}</span>
                </p>
              </div>
              <p class="text-xs text-surface-300">{{ formatDate(entry.timestamp) }}</p>

              <div class="flex gap-2 mt-4">
                <Button
                  @click="approveDecision(entry.logId)"
                  :label="t('approve')"
                  severity="success"
                  size="small"
                  class="flex-1"
                >
                  <template #icon>
                    <font-awesome-icon :icon="faCheck" class="h-3" />
                  </template>
                </Button>
                <Button
                  @click="rejectDecision(entry.logId)"
                  :label="t('reject')"
                  severity="danger"
                  size="small"
                  class="flex-1"
                >
                  <template #icon>
                    <font-awesome-icon :icon="faTimes" class="h-3" />
                  </template>
                </Button>
              </div>
            </div>
          </div>

          <!-- Historia decyzji -->
          <div v-else class="space-y-3 max-h-[75vh] overflow-y-auto custom-scrollbar">
            <div v-if="loadingHistory" class="text-center py-8">
              <ProgressSpinner style="width: 3rem; height: 3rem" strokeWidth="4" />
              <p class="text-surface-400 mt-3">{{ t('loadingDecisionHistory') }}</p>
            </div>

            <div
              v-else-if="decisions.length === 0"
              class="text-center py-12 border border-dashed border-surface-700 rounded-xl bg-secondary/50"
            >
              <div
                class="bg-secondary/50 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-3"
              >
                <font-awesome-icon :icon="faHistory" class="h-8 text-surface-600" />
              </div>
              <p class="text-surface-400 text-sm font-medium">{{ t('noDecisionHistory') }}</p>
            </div>

            <div v-for="(entry, index) in decisions" :key="index">
              <div
                v-if="entry.isEventNotification"
                class="border-l-4 border-blue-500 rounded-lg p-4 bg-blue-900/30 shadow-lg"
              >
                <div class="flex items-center gap-2 mb-2">
                  <font-awesome-icon :icon="faBolt" class="h-5 text-blue-400" />
                  <h3 class="font-bold text-lg text-blue-300">{{ t('newEvent') }}</h3>
                </div>
                <p class="text-surface-500 mt-2">{{ entry.feedbackDescription }}</p>
                <p class="text-xs text-surface-300 mt-2">{{ formatDate(entry.timestamp) }}</p>
              </div>

              <div
                v-else
                class="border-l-4 rounded-lg p-4 bg-secondary shadow-lg relative"
                :class="entry.result === 'Pozytywny' ? 'border-green-500' : 'border-red-500'"
              >
                <div
                  v-if="entry.eventAppliedId"
                  class="absolute top-2 right-2 px-2 py-1 bg-purple-600 text-white text-xs font-bold rounded-full"
                >
                  EVENT
                </div>
                <div class="flex items-start justify-between mb-2">
                  <div>
                    <p class="text-surface-500 font-semibold">{{ entry.tableName }}</p>
                    <p class="text-sm text-surface-400">{{ t('cardId') }}: {{ entry.cardId }}</p>
                  </div>
                  <span
                    class="px-2 py-1 text-xs font-semibold rounded"
                    :class="
                      entry.result === 'Pozytywny'
                        ? 'bg-green-500/20 text-green-400'
                        : 'bg-red-500/20 text-red-400'
                    "
                  >
                    {{ entry.result }}
                  </span>
                </div>
                <p class="text-lg font-bold text-primary-400 mb-2">{{ entry.cardTitle }}</p>
                <p class="text-sm text-surface-300 mb-2">
                  {{ entry.feedbackDescription || t('noFeedbackDescription') }}
                </p>
                <p class="text-xs text-surface-300">{{ formatDate(entry.timestamp) }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted, watch } from 'vue'
import { useToast } from 'vue-toastification'
import { useI18n } from 'vue-i18n'
import {
  faGamepad,
  faBolt,
  faClock,
  faHistory,
  faCheck,
  faTimes,
} from '@fortawesome/free-solid-svg-icons'
import Dropdown from 'primevue/dropdown'
import Button from '@/components/base/AppButton.vue'
import ProgressSpinner from 'primevue/progressspinner'
import GameBoard from '@/components/game/gameBoard.vue'
import apiConfig from '@/services/apiConfig'
import apiServices from '@/services/apiServices'
import signalService from '@/services/signalService'

// --- INTERFACES ---
interface Team {
  teamId: number
  teamName: string
  teamBud: number
  deckId: number
  boardId: number
}
interface Card {
  id: number
  cardsId: number
  title: string
  description: string
  cost?: number
  enablers?: number[]
}
interface Item {
  id: number
  title: string
  description: string
  cost?: number
  type?: string
}
interface DecisionLog {
  isEventNotification: boolean
  timestamp: string
  feedbackDescription: string
  cardId?: number
  cardTitle?: string
  tableId?: number | null
  tableName?: string
  result?: 'Pozytywny' | 'Negatywny'
  eventAppliedId?: number | null
}
interface PendingDecision {
  logId: number
  cardId: number
  cardTitle: string
  tableId: number
  tableName: string
  timestamp: string
}
interface GameEvent {
  eventId: number | null
  shortDesc: string
  longDesc: string
}
interface Pawn {
  id: number
  x: number
  y: number
  color: string
  name: string
  maxX: number
  maxY: number
}
interface RawApiLog {
  logId: number
  gameEventId: number | null
  teamId: number | null
  timestamp: string
  cardId: number
  cardTitle: string
  teamName: string
  feedbackDescription: string
  enablerDescription?: string
  status: boolean
}
interface RawPendingLog {
  logId: number
  cardId: number
  cardTitle: string
  teamId: number
  teamName: string
  timestamp: string
}
interface RawPawnData {
  teamId: number
  posX: string | number
  posY: string | number
  teamColor: string
  teamName: string
  maxPosX?: number
  maxPosY?: number
}
interface RivalBoardConfigFromApi {
  boardId: number
  name: string
  labelsUp: string[]
  labelsRight: string[]
  descriptionDown: string
  descriptionLeft: string
  rows: number
  cols: number
  cellColor: string
  borderColor: string
  borderColors: string[]
  cellsDescriptions?: string
}
interface BoardConfigForComponent {
  name: string
  labelsUp: string[]
  labelsRight: string[]
  descriptionDown: string
  descriptionLeft: string
  rows: number
  cols: number
  cellColor: string
  borderColor: string
  borderColors: string[]
  boardId: number
  cellsDescriptions?: string
}
interface GameDetails {
  deckId: number
}

const props = defineProps({
  gameId: { type: [Number, String], required: true },
  teamId: { type: [Number, String] },
})

const toast = useToast()
const gameId = Number(props.gameId)
const teamId = computed(() => (props.teamId ? Number(props.teamId) : undefined))

const deckId = ref<number | null>(null)

const loading = reactive({ teams: true, cards: true })
const decisions = ref<DecisionLog[]>([])
const loadingHistory = ref(true)
const selectedCardId = ref<number | null>(null)
const selectedTableId = ref<number | null>(null)
const isSubmitting = ref(false)
const tables = ref<Team[]>([])
const cards = ref<Card[]>([])
const { t } = useI18n()
const enemyFormData = reactive<BoardConfigForComponent>({
  name: '',
  labelsUp: [],
  labelsRight: [],
  descriptionDown: '',
  descriptionLeft: '',
  rows: 8,
  cols: 8,
  cellColor: '#f0f0f0',
  borderColor: '#595959',
  borderColors: [],
  boardId: 0,
  cellsDescriptions: '',
})
const pendingDecisions = ref<PendingDecision[]>([])
const loadingPending = ref(true)

const selectedCard = computed<Card | undefined>(() =>
  cards.value.find((c) => c.id === selectedCardId.value),
)
const selectedTeam = computed<Team | undefined>(() =>
  tables.value.find((t) => t.teamId === selectedTableId.value),
)
const currentBits = computed(() => (selectedTeam.value ? selectedTeam.value.teamBud : 0))

const decisionMode = ref('history')

watch(selectedTableId, (newTeamId) => {
  selectedCardId.value = null
  if (newTeamId) {
    fetchAvailableCardsForTeam()
  } else {
    cards.value = []
  }
})

const fetchGameDetails = async () => {
  if (!gameId) return
  try {
    const response = await apiServices.get<GameDetails>(apiConfig.games.getById(gameId))
    deckId.value = response.data.deckId
  } catch (error: any) {
    toast.error(t('errorFetchingGameData') + error)
    console.error('Błąd pobierania szczegółów gry:', error.response?.data || error.message)
  }
}

const fetchDecisionHistory = async () => {
  if (!gameId) return
  loadingHistory.value = true
  try {
    const response = await apiServices.post(apiConfig.player.getPlayerHistory, { gameId })
    const responseData = response.data as RawApiLog[]
    decisions.value = responseData.map((log) => ({
      isEventNotification: !!log.gameEventId && !log.teamId,
      timestamp: log.timestamp,
      feedbackDescription: log.feedbackDescription,
      cardId: log.cardId,
      cardTitle: log.cardTitle,
      tableId: log.teamId,
      tableName: log.teamName,
      result: log.status ? 'Pozytywny' : 'Negatywny',
      eventAppliedId: log.gameEventId,
    }))
  } catch (error: any) {
    toast.error(t('errorFetchingDecisionHistory'))
    console.error('Błąd ładowania historii:', error.response?.data || error.message)
  } finally {
    loadingHistory.value = false
  }
}

const fetchPendingDecisions = async () => {
  if (!gameId) return
  loadingPending.value = true
  try {
    const response = await apiServices.get(apiConfig.player.getPendingLogs(gameId))
    let data = response.data as RawPendingLog[]
    if (teamId.value) {
      data = data.filter((log) => log.teamId === teamId.value)
    }
    pendingDecisions.value = data.map((log) => ({
      ...log,
      tableId: log.teamId,
      tableName: log.teamName,
    }))
  } catch (error: any) {
    toast.error(t('errorFetchingSuggestions') + error)
    console.error('Błąd pobierania sugestii:', error.response?.data || error.message)
  } finally {
    loadingPending.value = false
  }
}

const fetchTeams = async () => {
  if (!gameId) return
  try {
    const response = await apiServices.get(apiConfig.player.getTeamsManagement(gameId))
    tables.value = response.data as Team[]
  } catch (error: any) {
    toast.error(t('errorFetchingTeams') + error)
    console.error('Błąd pobierania drużyn:', error.response?.data || error.message)
  }
}

const fetchAvailableCardsForTeam = async () => {
  const team = selectedTeam.value
  if (!team || !deckId.value) return
  loading.cards = true
  try {
    const url = apiConfig.player.getCards(deckId.value, gameId, team.teamId)
    const response = await apiServices.get(url)
    const data = response.data as { decisionCards: Card[] }
    cards.value = data.decisionCards || []
  } catch (error: any) {
    toast.error(t('errorFetchingDecisionCards'))
    console.error('Błąd pobierania kart:', error.response?.data || error.message)
  } finally {
    loading.cards = false
  }
}

const fetchRivalBoard = async () => {
  if (!gameId) return
  try {
    const url = apiConfig.player.getGameData(gameId)
    const response = await apiServices.get(url)
    const data = response.data as { rivalBoardConfig: RivalBoardConfigFromApi }
    if (data.rivalBoardConfig) {
      const config = data.rivalBoardConfig
      enemyFormData.boardId = config.boardId
      enemyFormData.name = config.name
      enemyFormData.labelsUp = config.labelsUp
      enemyFormData.labelsRight = config.labelsRight
      enemyFormData.descriptionDown = config.descriptionDown
      enemyFormData.descriptionLeft = config.descriptionLeft
      enemyFormData.rows = config.rows
      enemyFormData.cols = config.cols
      enemyFormData.cellColor = config.cellColor
      enemyFormData.borderColor = config.borderColor
      enemyFormData.borderColors = config.borderColors
      enemyFormData.cellsDescriptions = config.cellsDescriptions ?? ''
      await fetchRivalPawns()
    }
  } catch (error: any) {
    toast.error(t('errorFetchingRivalBoard') + error)
    console.error('Błąd w fetchRivalBoard:', error.response?.data || error.message)
  }
}

const enemyPawns = ref<Pawn[]>([])
const fetchRivalPawns = async () => {
  if (!gameId || !enemyFormData.boardId) return
  try {
    const url = apiConfig.player.getRivalPawns(gameId, enemyFormData.boardId)
    const response = await apiServices.get(url)
    enemyPawns.value = (response.data as RawPawnData[]).map((p) => ({
      id: p.teamId,
      x: Number(p.posX),
      y: Number(p.posY),
      color: p.teamColor,
      name: p.teamName,
      maxX: Number(p.maxPosX) || 1,
      maxY: Number(p.maxPosY) || 1,
    }))
  } catch (err: any) {
    console.error(t('errorFetchingPawns'), err.response?.data || err.message)
  }
}

// --- ACTIONS ---
async function playCard() {
  const entity = selectedCard.value
  const team = selectedTeam.value

  if (!entity || !team || !deckId.value) {
    toast.error(t('missingActionData'))
    return
  }
  if (!team.boardId || team.boardId === 0) {
    toast.error(`Wybrana drużyna "${team.teamName}" nie ma przypisanego ID planszy.`)
    return
  }
  if (team.teamBud < (entity.cost || 0)) {
    toast.warning(t('teamHasNotEnoughBits', { teamName: team.teamName }))
    return
  }

  const hasEnablers = Array.isArray(entity.enablers) && entity.enablers.length > 0
  if (hasEnablers) {
    const enablerTitles = entity.enablers!
      .map((id) => cards.value.find((c) => c.id === id)?.title ?? `ID ${id}`)
      .join(', ')
    toast.warning(t('cardRequiresEnablers', { enablers: enablerTitles }))
  }

  const wasSuccess = !hasEnablers
  const minEnablerId = hasEnablers
    ? Math.min(...entity.enablers!)
    : undefined

  const endpoint = wasSuccess
    ? apiConfig.player.playCardSuccess(entity.cardsId)
    : apiConfig.player.playCardFailure(entity.cardsId)

  const payload = {
    gameId: gameId,
    teamId: team.teamId,
    deckId: deckId.value,
    boardId: team.boardId,
    cost: entity.cost || 0,
    ForceExecution: true,
    enablerId: minEnablerId,
  }

  isSubmitting.value = true
  try {
    const response = await apiServices.post<{ message?: string; newTeamBudget: number }>(
      endpoint,
      payload,
    )

    const teamToUpdate = tables.value.find((t) => t.teamId === team.teamId)
    if (teamToUpdate) {
      teamToUpdate.teamBud = response.data.newTeamBudget
    } else {
      await fetchTeams()
    }

    await Promise.all([fetchAvailableCardsForTeam(), fetchDecisionHistory()])
    selectedCardId.value = null
  } catch (error: any) {
    if (error.response?.data?.errorCode === 'NotEnoughBudget') {
      toast.warning(t('teamHasNotEnoughBits', { teamName: team.teamName }))
      return
    }
    toast.error(error.response?.data?.message || t('actionExecutionError'))
    console.error('Błąd akcji karty:', error.response?.data || error.message)
  } finally {
    isSubmitting.value = false
  }
}

const approveDecision = async (logId: number) => {
  try {
    await apiServices.post(apiConfig.player.approveLog(logId), {})
    toast.success(t('decisionApprovedSuccess'))
    selectedCardId.value = null
    await Promise.all([
      fetchDecisionHistory(),
      fetchPendingDecisions(),
      fetchTeams(),
      fetchRivalPawns(),
      fetchAvailableCardsForTeam(),
    ])
  } catch (error: any) {
    toast.error(t('errorApprovingSuggestion') + error)
    console.error('Błąd zatwierdzania:', error.response?.data || error.message)
  }
}

const rejectDecision = async (logId: number) => {
  try {
    await apiServices.delete(apiConfig.player.rejectLog(logId))
    await fetchPendingDecisions()
  } catch (error: any) {
    toast.error(t('errorRejectingSuggestion') + error)
    console.error('Błąd odrzucania:', error.response?.data || error.message)
  }
}

const formatDate = (timestamp: string) => new Date(timestamp).toLocaleString('pl-PL')

const isViewMounted = ref(true)

const refreshAllData = () =>
  Promise.all([fetchTeams(), fetchDecisionHistory(), fetchPendingDecisions(), fetchRivalPawns()])

onMounted(async () => {
  if (!gameId) return

  await fetchGameDetails()

  await Promise.all([
    fetchTeams(),
    fetchDecisionHistory(),
    fetchPendingDecisions(),
    fetchRivalBoard(),
  ])

  if (teamId.value) {
    selectedTableId.value = teamId.value
  }

  try {
    await signalService.start()
    await signalService.joinGameRoomAsAdmin(String(gameId))
    signalService.connection.on('HistoryUpdated', () => fetchDecisionHistory())
    signalService.connection.on('PendingUpdated', () => fetchPendingDecisions())
    signalService.connection.on('BoardUpdated', () => fetchRivalPawns())
    signalService.connection.on('BudgetUpdated', () => fetchTeams())
    signalService.connection.on('PhaseUpdated', () =>
      Promise.all([fetchTeams(), fetchAvailableCardsForTeam(), fetchRivalPawns()]),
    )
    signalService.connection.onreconnected(async () => {
      if (!isViewMounted.value) return
      await signalService.joinGameRoomAsAdmin(String(gameId))
      await refreshAllData()
    })
  } catch (err: any) {
    console.error('Błąd połączenia SignalR: ', err)
  }
})

onUnmounted(() => {
  isViewMounted.value = false
  if (gameId) signalService.leaveGameRoomAsAdmin(String(gameId))
  signalService.connection.off('HistoryUpdated')
  signalService.connection.off('PendingUpdated')
  signalService.connection.off('BoardUpdated')
  signalService.connection.off('BudgetUpdated')
  signalService.connection.off('PhaseUpdated')
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
