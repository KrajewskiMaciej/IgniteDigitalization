<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <div class="text-center">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-white mb-2">
        {{ t('decisionPanel') }}
      </h1>
      <p class="text-surface-400 text-sm md:text-base mb-3">
        {{ t('manageDecisionsAndItemsForTeam') }}
      </p>
      <div
        class="inline-flex gap-3 items-center px-6 py-3 bg-secondary border border-surface-700 rounded-xl shadow-lg"
      >
        <div
          class="w-5 h-5 rounded-full ring-2 ring-surface-600 shadow-lg"
          :style="{ backgroundColor: teamData?.teamColor }"
        ></div>
        <span class="font-nasalization font-bold text-2xl text-surface-0 tracking-wide">
          {{ teamData?.teamName }}
        </span>
      </div>
    </div>

    <div class="max-w-7xl mx-auto w-full">
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div class="space-y-6">
          <div class="border border-surface-700 rounded-xl p-6 bg-secondary shadow-2xl">
            <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
              <div class="bg-primary-500/20 p-3 rounded-lg">
                <font-awesome-icon :icon="faGamepad" class="h-6 text-primary-400" />
              </div>
              <h2 class="text-xl md:text-2xl font-bold text-white">{{ t('actionManagment') }}</h2>
            </div>

            <div class="flex gap-3 mb-5">
              <Button
                :label="t('decisions')"
                @click="actionMode = 'cards'"
                :severity="actionMode === 'cards' ? undefined : 'secondary'"
                class="flex-1"
                outlined
              />
              <Button
                :label="t('items')"
                @click="actionMode = 'items'"
                :severity="actionMode === 'items' ? undefined : 'secondary'"
                class="flex-1"
                outlined
              />
            </div>

            <div v-if="actionMode === 'cards'" class="mb-4">
              <label class="block mb-2 text-sm font-semibold text-gray-300">{{
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

            <div v-if="actionMode === 'items'" class="mb-4">
              <label class="block mb-2 text-sm font-semibold text-gray-300">
                {{ t('selectItem') }}
              </label>
              <Dropdown
                v-model="selectedItemId"
                :options="items"
                optionLabel="title"
                optionValue="id"
                :placeholder="t('selectItemPlaceholder')"
                class="w-full"
                :disabled="loading.items"
              >
                <template #value="slotProps">
                  <div v-if="slotProps.value" class="flex items-center gap-2">
                    <span
                      :class="
                        items.find((i) => i.id === slotProps.value)?.type === 'software'
                          ? 'text-green-400'
                          : 'text-orange-400'
                      "
                      >#{{ slotProps.value }}</span
                    >
                    <span>{{ items.find((i) => i.id === slotProps.value)?.title }}</span>
                  </div>
                  <span v-else class="text-surface-400">{{ slotProps.placeholder }}</span>
                </template>
                <template #option="slotProps">
                  <div class="flex items-center gap-2">
                    <span
                      :class="
                        slotProps.option.type === 'software' ? 'text-green-400' : 'text-orange-400'
                      "
                      >#{{ slotProps.option.id }}</span
                    >
                    <span>{{ slotProps.option.title }}</span>
                  </div>
                </template>
              </Dropdown>
            </div>

            <div
              v-if="
                (actionMode === 'cards' && selectedCard) || (actionMode === 'items' && selectedItem)
              "
              class="bg-secondary rounded-lg p-4 border border-surface-700 mb-4"
            >
              <p class="text-sm text-surface-400 mb-1">{{ t('description') }}:</p>
              <p class="text-sm text-gray-300">
                {{ actionMode === 'cards' ? selectedCard?.description : selectedItem?.description }}
              </p>
              <div class="flex items-center justify-between mt-3 pt-3 border-t border-surface-700">
                <span class="text-sm text-surface-400">{{ t('cost') }}:</span>
                <span class="text-lg font-bold text-green-400">
                  {{ (actionMode === 'cards' ? selectedCard?.cost : selectedItem?.cost) || 0 }}
                  {{ t('bits') }}
                </span>
              </div>
            </div>

            <div
              v-if="teamData"
              class="bg-secondary rounded-lg p-4 border border-surface-700 mb-4"
            >
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm text-surface-400">{{ t('teamBudget') }}:</p>
                  <p class="text-sm font-semibold text-white">{{ teamData.teamName }}</p>
                </div>
                <span class="text-2xl font-bold text-green-400"
                  >{{ teamData.teamBud }} {{ t('bits') }}</span
                >
              </div>
            </div>

            <div class="flex justify-center">
              <Button
                v-if="actionMode === 'cards'"
                :disabled="!selectedCardId"
                @click="playCard"
                :label="t('playCard')"
                size="large"
                class="w-full"
              />
              <Button
                v-if="actionMode === 'items'"
                :disabled="!selectedItemId"
                @click="giveItem"
                :label="t('useItem')"
                size="large"
                class="w-full"
              />
            </div>
          </div>

          <div v-if="showOwnBoard" class="w-full flex justify-center">
            <GameBoard :config="formData" :game-mode="true" :pawns="pawns" :use-percentage="true" />
          </div>
        </div>

        <div class="border border-surface-700 rounded-xl p-6 bg-secondary shadow-2xl">
          <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
            <div class="bg-yellow-500/20 p-3 rounded-lg">
              <font-awesome-icon :icon="faClock" class="h-6 text-yellow-400" />
            </div>
            <h2 class="text-xl md:text-2xl font-bold text-white">{{ t('decisionPanel') }}</h2>
          </div>

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

          <div
            v-if="decisionMode === 'pending'"
            class="space-y-3 max-h-[75vh] overflow-y-auto custom-scrollbar"
          >
            <div v-if="loading.pending" class="text-center py-8">
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
                  <p class="text-white font-semibold">{{ entry.tableName }}</p>
                  <p class="text-sm text-surface-400">{{ t('suggectCard') }}</p>
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
                <p class="text-xs text-gray-500 mt-1">
                  {{ t('cardId') }}:
                  <span class="font-semibold text-surface-400">{{ entry.cardId }}</span>
                </p>
              </div>
              <p class="text-xs text-gray-500">{{ formatDate(entry.timestamp) }}</p>

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

          <div v-else class="space-y-3 max-h-[75vh] overflow-y-auto custom-scrollbar">
            <div v-if="loading.history" class="text-center py-8">
              <ProgressSpinner style="width: 3rem; height: 3rem" strokeWidth="4" />
              <p class="text-surface-400 mt-3">{{ t('loadingHistory') }}</p>
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
                <p class="text-white mt-2">{{ entry.feedbackDescription }}</p>
                <p class="text-xs text-gray-500 mt-2">{{ formatDate(entry.timestamp) }}</p>
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
                    <p class="text-white font-semibold">{{ entry.tableName }}</p>
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
                <p class="text-sm text-gray-300 mb-2">
                  {{ entry.feedbackDescription || t('noFeedbackDescription') }}
                </p>
                <p class="text-xs text-gray-500">{{ formatDate(entry.timestamp) }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
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
import Button from 'primevue/button'
import ProgressSpinner from 'primevue/progressspinner'

import GameBoard from '@/components/game/gameBoard.vue'
import apiConfig from '@/services/apiConfig'
import apiServices from '@/services/apiServices'
import signalService from '@/services/signalService'

interface SessionData {
  teamId: number
  teamName: string
  teamColor: string
  teamBud: number
  deckId: number
  boardConfig: BoardConfig & { boardId: number }
}
interface TeamData {
  teamId: number
  teamName: string
  teamColor: string
  teamBud: number
  deckId: number
  boardId: number
}
interface Card {
  id: number
  deckId: number
  displayOrder: number
  title: string
  description: string
  cost?: number
  enablers?: unknown[]
}

interface ICardsResponse {
  decisionCards: Card[]
  hardwareCards: Card[]
  softwareCards: Card[]
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
  tableId?: number
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
interface BoardConfig {
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
}
interface RawHistoryLog {
  isEventNotification: boolean
  eventDescription: string
  timestamp: string
  cardId: number
  cardTitle: string
  teamId: number
  teamName: string
  feedbackDescription: string
  enablerDescription?: string
  status: boolean
  gameEventId: number | null
}
interface RawPendingLog {
  logId: number
  cardId: number
  cardTitle: string
  teamId: number
  teamName: string
  timestamp: string
}
interface RawPawn {
  gpId: number
  posX: string
  posY: string
  color: string
  name: string
  maxPosX?: number
  maxPosY?: number
}

const props = defineProps({
  gameId: { type: [Number, String], required: true },
  teamId: { type: [Number, String], required: true },
})

const toast = useToast()
const { t } = useI18n()

const formData = reactive<BoardConfig>({
  boardId: 0,
  name: '',
  labelsUp: [],
  labelsRight: [],
  descriptionDown: '',
  descriptionLeft: '',
  rows: 8,
  cols: 8,
  cellColor: '#ffffff',
  borderColor: '#000000',
  borderColors: [],
})

const loading = reactive({ teamData: true, cards: true, items: true, history: true, pending: true })
const teamData = ref<TeamData | null>(null)
const cards = ref<Card[]>([])
const items = ref<Item[]>([])
const decisions = ref<DecisionLog[]>([])
const pendingDecisions = ref<PendingDecision[]>([])
const pawns = ref<Pawn[]>([])
const availableEvents = ref<GameEvent[]>([])

const actionMode = ref<'cards' | 'items' | 'events'>('cards')
const decisionMode = ref<'pending' | 'history'>('history')
const selectedCardId = ref<number | null>(null)
const selectedItemId = ref<number | null>(null)
const selectedPendingEventIndex = ref<number | null>(null)
const showOwnBoard = ref(true)

const selectedCard = computed<Card | undefined>(() =>
  cards.value.find((c) => c.id === selectedCardId.value),
)
const selectedItem = computed<Item | undefined>(() =>
  items.value.find((i) => i.id === selectedItemId.value),
)

const fetchAllDataForTeam = async () => {
  const gameIdNum = Number(props.gameId)
  const teamIdNum = Number(props.teamId)
  if (isNaN(gameIdNum) || isNaN(teamIdNum)) return

  Object.keys(loading).forEach((k) => (loading[k as keyof typeof loading] = true))

  try {
    const response = await apiServices.get<SessionData>(
      apiConfig.player.getTeamInfo(gameIdNum, teamIdNum),
    )
    const sessionData = response.data

    if (sessionData.boardConfig) {
      Object.assign(formData, sessionData.boardConfig)
    }

    console.log('Dane drużyny:', response.data)

    teamData.value = {
      teamId: sessionData.teamId,
      teamName: sessionData.teamName,
      teamColor: sessionData.teamColor,
      teamBud: sessionData.teamBud,
      deckId: sessionData.deckId,
      boardId: sessionData.boardConfig?.boardId,
    }

    await Promise.all([
      fetchAvailableCardsAndItems(),
      fetchPawns(),
      fetchDecisionHistory(),
      fetchPendingDecisions(),
      fetchGameEvents(teamData.value.deckId),
    ])
  } catch (error) {
    toast.error(t('errorLoadingTeamData'))
    console.error('Błąd w fetchAllDataForTeam:', error)
  } finally {
    Object.keys(loading).forEach((k) => (loading[k as keyof typeof loading] = false))
  }
}

const fetchAvailableCardsAndItems = async () => {
  if (!teamData.value?.deckId || !props.gameId || !props.teamId) return
  try {
    const url = apiConfig.player.getCards(
      teamData.value.deckId,
      Number(props.gameId),
      Number(props.teamId),
    )

    const response = await apiServices.get<ICardsResponse>(url)

    console.log('Pobrane dane kart', response.data)

    cards.value = response.data.decisionCards || []

    const softwareCards = (response.data.softwareCards || []).map((item: Item) => ({
      ...item,
      type: 'software',
    }))

    const hardwareCards = (response.data.hardwareCards || []).map((item: Item) => ({
      ...item,
      type: 'hardware',
    }))

    items.value = [...softwareCards, ...hardwareCards]
  } catch (error) {
    toast.error(t('errorFetchingCardsAndItems'))
    console.error('Błąd pobierania kart:', error)
  }
}

const fetchDecisionHistory = async () => {
  try {
    const response = await apiServices.post<RawHistoryLog[]>(apiConfig.player.getPlayerHistory, {
      gameId: props.gameId,
      teamId: props.teamId,
    })
    const logs = response.data
    if (Array.isArray(logs)) {
      decisions.value = logs.map((log) =>
        log.isEventNotification
          ? {
              isEventNotification: true,
              feedbackDescription: log.eventDescription || 'Aktywowano nowe wydarzenie.',
              timestamp: log.timestamp,
            }
          : {
              isEventNotification: false,
              cardId: log.cardId,
              cardTitle: log.cardTitle,
              tableId: log.teamId,
              tableName: log.teamName,
              timestamp: log.timestamp,
              feedbackDescription: log.feedbackDescription,
              result: log.status ? 'Pozytywny' : 'Negatywny',
              eventAppliedId: log.gameEventId,
            },
      )
    }
  } catch (error) {
    // Używam klucza z literówką "Decsion", bo tak jest w Twoim pliku pl.ts
    toast.error(t('errorFetchingDecsionHistory'))
  }
}

const fetchPendingDecisions = async () => {
  try {
    const response = await apiServices.get<PendingDecision[]>(
      apiConfig.player.getPendingLogsForTeam(Number(props.gameId), Number(props.teamId)),
    )
    pendingDecisions.value = response.data
    console.log('Pobrane decyzje do akceptacji:', pendingDecisions.value)
  } catch (error) {
    toast.error(t('errorFetchingSuggestions'))
  }
}

const fetchPawns = async () => {
  if (!teamData.value?.boardId || !props.gameId || !props.teamId) return
  try {
    const url = apiConfig.player.getPawns(
      Number(props.gameId),
      Number(props.teamId),
      teamData.value.boardId,
    )

    const response = await apiServices.get<RawPawn[]>(url)

    pawns.value = response.data.map((p) => ({
      id: p.gpId,
      x: Number(p.posX),
      y: Number(p.posY),
      color: p.color,
      name: p.name,
      maxX: Number(p.maxPosX) || 1,
      maxY: Number(p.maxPosY) || 1,
    }))
  } catch (err) {
    console.error('Błąd pobierania pionków:', err)
  }
}

const fetchGameEvents = async (deckId: number) => {
  if (!deckId) return
  try {
    const url = apiConfig.player.getGameEvents(deckId)
    const response = await apiServices.get(url)
    availableEvents.value = [
      { eventId: null, shortDesc: 'Brak zdarzenia', longDesc: '' },
      ...(response.data as GameEvent[]),
    ]
  } catch (error) {
    console.error('Błąd pobierania zdarzeń:', error)
  }
}

const executeCardOrItemAction = async (isCard: boolean) => {
  const entity = isCard ? selectedCard.value : selectedItem.value
  const team = teamData.value

  if (!entity || !team) {
    toast.error(t('missingActionData'))
    return
  }

  if (!team.boardId || team.boardId === 0) {
    toast.error(t('teamNoBoardId', { teamName: team.teamName }))
    return
  }

  if (team.teamBud < (entity.cost || 0)) {
    toast.warning(t('teamHasNotEnoughBits', { teamName: team.teamName }))
    return
  }

  let wasSuccess = true
  if (isCard) {
    const cardEntity = entity as Card
    wasSuccess = !(
      cardEntity.enablers &&
      Array.isArray(cardEntity.enablers) &&
      cardEntity.enablers.length > 0
    )
  }

  const minEnablerId =
    isCard && !wasSuccess && Array.isArray((entity as Card).enablers) && (entity as Card).enablers!.length > 0
      ? Math.min(...((entity as Card).enablers as number[]))
      : undefined

  const endpoint = wasSuccess
    ? apiConfig.player.playCardSuccess(entity.id)
    : apiConfig.player.playCardFailure(entity.id)

  const payload = {
    gameId: Number(props.gameId),
    teamId: team.teamId,
    deckId: team.deckId,
    boardId: team.boardId,
    cost: entity.cost || 0,
    ForceExecution: true,
    enablerId: minEnablerId,
  }

  try {
    console.log('Wysyłany Id Karty:', entity.id)
    const response = await apiServices.post<{ message?: string; newTeamBudget: number }>(
      endpoint,
      payload,
    )

    // Usunięty toast.success zgodnie z instrukcją
    // toast.success(response.data?.message || 'Akcja przetworzona pomyślnie.')

    if (teamData.value) {
      teamData.value.teamBud = response.data.newTeamBudget
    }

    await fetchAvailableCardsAndItems()
  } catch (error: any) {
    if (error.response?.data?.errorCode === 'NotEnoughBudget') {
      toast.warning(t('teamHasNotEnoughBits', { teamName: team.teamName }))
      return
    }
    toast.error(error.response?.data?.message || t('actionExecutionError'))
    console.error('Błąd akcji karty/przedmiotu:', error.response?.data || error.message)
  }

  if (isCard) {
    selectedCardId.value = null
  } else {
    selectedItemId.value = null
  }
}

const playCard = () => executeCardOrItemAction(true)
const giveItem = () => executeCardOrItemAction(false)

const approveDecision = async (logId: number) => {
  try {
    await apiServices.post(apiConfig.player.approveLog(logId), {})
    // Usunięty toast.success zgodnie z instrukcją
  } catch (error) {
    toast.error(t('errorApprovingSuggestion'))
  }
}

const rejectDecision = async (logId: number) => {
  try {
    await apiServices.delete(apiConfig.player.rejectLog(logId))
    toast.info(t('suggestionRejected'))
  } catch (error) {
    toast.error(t('errorRejectingSuggestion'))
  }
}

const formatDate = (timestamp: string) => new Date(timestamp).toLocaleString('pl-PL')

onMounted(async () => {
  const gameIdNum = Number(props.gameId)
  if (isNaN(gameIdNum)) {
    toast.error(t('criticalGameIdError'))
    return
  }

  await fetchAllDataForTeam()

  signalService.connection.on('HistoryUpdated', () => fetchDecisionHistory())
  signalService.connection.on('PendingUpdated', () => fetchPendingDecisions())
  signalService.connection.on('BoardUpdated', () => fetchPawns())
  signalService.connection.on('BudgetUpdated', () => fetchAllDataForTeam())

  try {
    await signalService.start()
    await signalService.joinGameRoomAsPlayer(String(gameIdNum), String(props.teamId))
    console.log(
      `Pomyślnie dołączono do pokoju SignalR dla gry: ${gameIdNum}, zespół: ${props.teamId}`,
    )
  } catch (err) {
    console.error('Błąd połączenia SignalR: ', err)
  }
})

onUnmounted(() => {
  if (props.gameId) {
    signalService.leaveGameRoomAsPlayer(String(props.gameId), String(props.teamId))
    signalService.connection.off('HistoryUpdated')
    signalService.connection.off('PendingUpdated')
    signalService.connection.off('BoardUpdated')
    signalService.connection.off('BudgetUpdated')
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
