<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <!-- Nagłówek -->
    <div class="text-center">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-white mb-2">
        Panel Decyzji
      </h1>
      <p class="text-gray-400 text-sm md:text-base">
        Zarządzaj decyzjami i przedmiotami dla drużyn
      </p>
    </div>

    <div class="max-w-7xl mx-auto w-full">
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Lewa kolumna - Akcje -->
        <div class="space-y-6">
          <!-- Sekcja wyboru akcji -->
          <div class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl">
            <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
              <div class="bg-primary-500/20 p-2.5 rounded-lg">
                <font-awesome-icon :icon="faGamepad" class="h-6 text-primary-400" />
              </div>
              <h2 class="text-xl md:text-2xl font-bold text-white">Zarządzanie akcjami</h2>
            </div>

            <!-- Przełącznik kart/przedmiotów -->
            <div class="flex gap-3 mb-5">
              <Button
                :label="'Decyzje'"
                @click="actionMode = 'cards'"
                :severity="actionMode === 'cards' ? undefined : 'secondary'"
                class="flex-1"
                outlined
              >
              </Button>
              <Button
                :label="'Przedmioty'"
                @click="actionMode = 'items'"
                :severity="actionMode === 'items' ? undefined : 'secondary'"
                class="flex-1"
                outlined
              >
              </Button>
              <Button
                :label="'Zdarzenia'"
                @click="actionMode = 'events'"
                :severity="actionMode === 'events' ? undefined : 'secondary'"
                class="flex-1"
                outlined
              >
              </Button>
            </div>

            <!-- Wybór stołu -->
            <div v-if="!teamId && (actionMode === 'cards' || actionMode === 'items')" class="mb-4">
              <label class="block mb-2 text-sm font-semibold text-gray-300">Wybierz stół:</label>
              <Dropdown
                v-model="selectedTableId"
                :options="tables"
                optionLabel="teamName"
                optionValue="teamId"
                placeholder="Wybierz stół..."
                class="w-full"
              >
                <template #option="slotProps">
                  <div class="flex items-center justify-between gap-2 w-full">
                    <div class="flex gap-2 items-center">
                      <div :style="{ backgroundColor: slotProps.option.teamColor }" class="w-4 h-4 rounded-full"></div>
                      <span>{{ slotProps.option.teamName }}</span>
                    </div>
                    <span class="text-green-400">{{ slotProps.option.teamBud }} bitów</span>
                  </div>
                </template>
              </Dropdown>
            </div>

            <!-- Wybór karty -->
            <div v-if="actionMode === 'cards'" class="mb-4">
              <label class="block mb-2 text-sm font-semibold text-gray-300">Wybierz kartę:</label>
              <Dropdown
                v-model="selectedCardId"
                :options="cards"
                optionLabel="title"
                optionValue="id"
                placeholder="Wybierz kartę..."
                class="w-full"
                :disabled="loading.cards"
              >
                <template #value="slotProps">
                  <div v-if="slotProps.value" class="flex items-center gap-2">
                    <span class="text-blue-400">#{{ slotProps.value }}</span>
                    <span>{{ cards.find((c) => c.id === slotProps.value)?.title }}</span>
                  </div>
                  <span v-else class="text-gray-400">{{ slotProps.placeholder }}</span>
                </template>
                <template #option="slotProps">
                  <div class="flex items-center gap-2">
                    <span class="text-blue-400">#{{ slotProps.option.id }}</span>
                    <span>{{ slotProps.option.title }}</span>
                  </div>
                </template>
              </Dropdown>
            </div>

            <!-- Wybór przedmiotu -->
            <div v-if="actionMode === 'items'" class="mb-4">
              <label class="block mb-2 text-sm font-semibold text-gray-300">
                Wybierz przedmiot:
              </label>
              <Dropdown
                v-model="selectedItemId"
                :options="items"
                optionLabel="title"
                optionValue="id"
                placeholder="Wybierz przedmiot..."
                class="w-full"
                :disabled="loading.items"
              >
                <template #value="slotProps">
                  <div v-if="slotProps.value" class="flex items-center gap-2">
                    <span  :class="items.find((i) => i.id === slotProps.value)?.type ===  'software' ? 'text-green-400' : 'text-orange-400'">#{{ slotProps.value }}</span>
                    <span>{{ items.find((i) => i.id === slotProps.value)?.title }}</span>
                  </div>
                  <span v-else class="text-gray-400">{{ slotProps.placeholder }}</span>
                </template>
                <template #option="slotProps">
                  <div class="flex items-center gap-2">
                    <span :class="slotProps.option.type ===  'software' ? 'text-green-400' : 'text-orange-400'">#{{ slotProps.option.id }}</span>
                    <span>{{ slotProps.option.title }}</span>
                  </div>
                </template>
              </Dropdown>
            </div>

            <div v-if="actionMode === 'events'" class="mb-4">
              <label class="block mb-2 text-sm font-semibold text-gray-300">
                Wybierz zdarzenie:
              </label>
              <Dropdown
                v-model="selectedPendingEventIndex"
                :options="availableEvents"
                optionLabel="shortDesc"
                optionValue="eventId"
                placeholder="Wybierz zdarzenie..."
                class="w-full"
              >
                <template #value="slotProps">
                  <div v-if="slotProps.value !== null">
                    <span>
                      {{ availableEvents.find((e) => e.eventId === slotProps.value)?.shortDesc }}
                    </span>
                  </div>
                  <span v-else class="text-gray-400">{{ slotProps.placeholder }}</span>
                </template>
              </Dropdown>
            </div>

            <!-- Opis wybranej karty/przedmiotu -->
            <div
              v-if="
                (actionMode === 'cards' && selectedCard) || (actionMode === 'items' && selectedItem)
              "
              class="bg-surface-800 rounded-lg p-4 border border-surface-700 mb-4"
            >
              <p class="text-sm text-gray-400 mb-1">Opis:</p>
              <p class="text-sm text-gray-300">
                {{ actionMode === 'cards' ? selectedCard?.description : selectedItem?.description }}
              </p>
              <div class="flex items-center justify-between mt-3 pt-3 border-t border-surface-700">
                <span class="text-sm text-gray-400">Koszt:</span>
                <span class="text-lg font-bold text-green-400">
                  {{ (actionMode === 'cards' ? selectedCard?.cost : selectedItem?.cost) || 0 }}
                  bitów
                </span>
              </div>
            </div>

            <div
              v-if="selectedEvent && selectedEvent.eventId && actionMode === 'events'"
              class="bg-surface-800 rounded-lg p-4 border border-surface-700 mb-4"
            >
              <p class="text-sm text-gray-400 mb-1">Opis zdarzenia:</p>
              <p class="text-sm text-gray-300">{{ selectedEvent.longDesc }}</p>
            </div>

            <!-- Budżet drużyny -->
            <div
              v-if="
                (selectedTableId && actionMode === 'cards') ||
                (selectedTableId && actionMode === 'items')
              "
              class="bg-surface-800 rounded-lg p-4 border border-surface-700 mb-4"
            >
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm text-gray-400">Budżet drużyny:</p>
                  <p class="text-sm font-semibold text-white">{{ selectedTeam?.teamName }}</p>
                </div>
                <span class="text-2xl font-bold text-green-400">{{ currentBits }} bitów</span>
              </div>
            </div>

            <!-- Przyciski akcji -->
            <div class="flex justify-center">
              <Button
                v-if="actionMode === 'cards'"
                :disabled="!selectedCardId || !selectedTableId"
                @click="playCard"
                :label="'Zagraj kartę'"
                size="large"
                class="w-full"
              >
              </Button>
              <Button
                v-if="actionMode === 'items'"
                :disabled="!selectedItemId || !selectedTableId"
                @click="giveItem"
                :label="'Użyj przedmiot'"
                size="large"
                class="w-full"
              >
              </Button>
              <Button
                v-if="actionMode === 'events'"
                :disabled="!selectedPendingEventIndex"
                @click="applySelectedEvent"
                :label="'Zastosuj zdarzenie'"
                size="large"
                class="w-full"
              >
              </Button>
            </div>
          </div>

          <!-- Plansza rywali -->
          <div class="w-full flex justify-center">
            <GameBoard :config="enemyFormData" :game-mode="false" :pawns="enemyPawns" />
          </div>
        </div>

        <!-- Prawa kolumna - Panel decyzji -->
        <div class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl">
          <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
            <div class="bg-yellow-500/20 p-2.5 rounded-lg">
              <font-awesome-icon :icon="faClock" class="h-6 text-yellow-400" />
            </div>
            <h2 class="text-xl md:text-2xl font-bold text-white">Panel decyzji</h2>
          </div>

          <!-- Przełącznik widoku decyzji -->
          <div class="flex gap-3 mb-5">
            <Button
              :label="'Do zatwierdzenia'"
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
              :label="'Historia'"
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
              <p class="text-gray-400 mt-3">Ładowanie sugestii...</p>
            </div>

            <div
              v-else-if="pendingDecisions.length === 0"
              class="text-center py-12 border border-dashed border-surface-700 rounded-xl bg-surface-900/50"
            >
              <div
                class="bg-surface-800/50 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-3"
              >
                <font-awesome-icon :icon="faClock" class="h-8 text-surface-600" />
              </div>
              <p class="text-gray-400 text-sm font-medium">Brak decyzji do zatwierdzenia</p>
            </div>

            <div
              v-for="entry in pendingDecisions"
              :key="entry.logId"
              class="border-l-4 border-yellow-500 rounded-lg p-4 bg-surface-800 shadow-lg"
            >
              <div class="flex items-start justify-between mb-2">
                <div>
                  <p class="text-white font-semibold">{{ entry.tableName }}</p>
                  <p class="text-sm text-gray-400">sugeruje kartę</p>
                </div>
                <span
                  class="px-2 py-1 bg-yellow-500/20 text-yellow-400 text-xs font-semibold rounded"
                >
                  Oczekuje
                </span>
              </div>
              <div class="mb-2">
                <p class="text-lg font-bold text-primary-400 leading-tight">
                  {{ entry.cardTitle }}
                </p>
                <p class="text-xs text-gray-500 mt-1">
                  ID karty: <span class="font-semibold text-gray-400">{{ entry.cardId }}</span>
                </p>
              </div>
              <p class="text-xs text-gray-500">{{ formatDate(entry.timestamp) }}</p>

              <div class="flex gap-2 mt-4">
                <Button
                  @click="approveDecision(entry.logId)"
                  :label="'Zatwierdź'"
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
                  :label="'Odrzuć'"
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
              <p class="text-gray-400 mt-3">Ładowanie historii...</p>
            </div>

            <div
              v-else-if="decisions.length === 0"
              class="text-center py-12 border border-dashed border-surface-700 rounded-xl bg-surface-900/50"
            >
              <div
                class="bg-surface-800/50 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-3"
              >
                <font-awesome-icon :icon="faHistory" class="h-8 text-surface-600" />
              </div>
              <p class="text-gray-400 text-sm font-medium">Brak decyzji w historii</p>
            </div>

            <div v-for="(entry, index) in decisions" :key="index">
              <div
                v-if="entry.isEventNotification"
                class="border-l-4 border-blue-500 rounded-lg p-4 bg-blue-900/30 shadow-lg"
              >
                <div class="flex items-center gap-2 mb-2">
                  <font-awesome-icon :icon="faBolt" class="h-5 text-blue-400" />
                  <h3 class="font-bold text-lg text-blue-300">Nowe Wydarzenie</h3>
                </div>
                <p class="text-white mt-2">{{ entry.feedbackDescription }}</p>
                <p class="text-xs text-gray-500 mt-2">{{ formatDate(entry.timestamp) }}</p>
              </div>

              <div
                v-else
                class="border-l-4 rounded-lg p-4 bg-surface-800 shadow-lg relative"
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
                    <p class="text-sm text-gray-400">Karta ID: {{ entry.cardId }}</p>
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
                  {{ entry.feedbackDescription || 'Brak opisu feedbacku.' }}
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
import Button from 'primevue/button'
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
  title: string
  description: string
  cost?: number
  enablers?: unknown[]
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

const loading = reactive({ teams: true, cards: true, items: true })
const decisions = ref<DecisionLog[]>([])
const loadingHistory = ref(true)
const selectedCardId = ref<number | null>(null)
const selectedTableId = ref<number | null>(null)
const selectedPendingEventIndex = ref<number | null>(null)
const tables = ref<Team[]>([])
const cards = ref<Card[]>([])
const items = ref<Item[]>([])
const availableEvents = ref<GameEvent[]>([])
const { t } = useI18n();
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
})
const pendingDecisions = ref<PendingDecision[]>([])
const loadingPending = ref(true)

const selectedCard = computed<Card | undefined>(() =>
  cards.value.find((c) => c.id === selectedCardId.value),
)
const selectedItem = computed<Item | undefined>(() =>
  items.value.find((i) => i.id === selectedItemId.value),
)
const selectedTeam = computed<Team | undefined>(() =>
  tables.value.find((t) => t.teamId === selectedTableId.value),
)
const currentBits = computed(() => (selectedTeam.value ? selectedTeam.value.teamBud : 0))
const selectedEvent = computed<GameEvent | undefined>(() =>
  availableEvents.value.find((e) => e.eventId === selectedPendingEventIndex.value),
)

const decisionMode = ref('history')
const actionMode = ref<'cards' | 'items' | 'events'>('cards')
const selectedItemId = ref<number | null>(null)

watch(selectedTableId, (newTeamId) => {
  selectedCardId.value = null
  selectedItemId.value = null
  if (newTeamId) {
    fetchAvailableCardsForTeam()
  } else {
    cards.value = []
    items.value = []
  }
})

const fetchGameDetails = async () => {
  if (!gameId) return
  try {
    const response = await apiServices.get<GameDetails>(apiConfig.games.getById(gameId))
    deckId.value = response.data.deckId
  } catch (error: any) {
    toast.error('Nie udało się pobrać szczegółów gry.')
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
    toast.error('Wystąpił błąd podczas ładowania historii decyzji.')
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

    console.log('Pobrane decyzje oczekujące:', pendingDecisions.value);
  } catch (error: any) {
    toast.error('Błąd podczas pobierania sugestii.')
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

    console.log('Pobrane drużyny:', tables.value);
  } catch (error: any) {
    toast.error('Błąd pobierania drużyn.')
    console.error('Błąd pobierania drużyn:', error.response?.data || error.message)
  }
}

const fetchAvailableCardsForTeam = async () => {
  const team = selectedTeam.value
  if (!team || !deckId.value) return
  loading.cards = true
  loading.items = true
  try {
    const url = apiConfig.player.getCards(deckId.value, gameId, team.teamId)
    const response = await apiServices.get(url)
    const data = response.data as { decisionCards: Card[]; hardwareCards: Item[]; softwareCards: Item[] }
    cards.value = data.decisionCards || []
    
    const softwareCards = data.softwareCards.map((item) => ({
      ...item,
      type: 'software',
    }));

    console.log('Pobrane karty oprogramowania:', softwareCards);

    const hardwareCards = data.hardwareCards.map((item) => ({
      ...item,
      type: 'hardware',
    })); 


  

    items.value = [...softwareCards, ...hardwareCards]

  } catch (error: any) {
    toast.error('Błąd pobierania dostępnych kart i przedmiotów.')
    console.error('Błąd pobierania kart:', error.response?.data || error.message)
  } finally {
    loading.cards = false
    loading.items = false
  }
}

const fetchGameEvents = async (currentDeckId: number) => {
  if (!currentDeckId) {
    toast.error('Brak ID talii, nie można pobrać zdarzeń.')
    return
  }
  try {
    const url = apiConfig.player.getGameEvents(currentDeckId)
    const response = await apiServices.get(url)
    availableEvents.value = [
      { eventId: null, shortDesc: 'Brak zdarzenia', longDesc: '' },
      ...(response.data as GameEvent[]),
    ]
  } catch (error: any) {
    toast.error('Nie udało się pobrać listy zdarzeń.')
    console.error('Błąd pobierania zdarzeń:', error.response?.data || error.message)
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
      await fetchRivalPawns()
    }
  } catch (error: any) {
    toast.error('Wystąpił błąd podczas ładowania danych planszy rywala.')
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
    }))
  } catch (err: any) {
    console.error('Błąd pobierania pionków rywala:', err.response?.data || err.message)
  }
}

// --- ACTIONS ---
async function executeAction(isCard: boolean) {
  const entity = isCard ? selectedCard.value : selectedItem.value
  const team = selectedTeam.value

  if (!entity || !team || !deckId.value) {
    toast.error('Brak kluczowych danych (drużyna, talia, karta/przedmiot), aby wykonać akcję.')
    return
  }
  if (!team.boardId || team.boardId === 0) {
    toast.error(`Wybrana drużyna "${team.teamName}" nie ma przypisanego ID planszy.`)
    return
  }
  if (team.teamBud < (entity.cost || 0)) {
    toast.warning(t('teamHasNotEnoughBits', { teamName: team.teamName }));
    return
  }
  let wasSuccess: boolean
  if (isCard) {
    const cardEntity = entity as Card
    wasSuccess = !(
      cardEntity.enablers &&
      Array.isArray(cardEntity.enablers) &&
      cardEntity.enablers.length > 0
    )
  } else {
    wasSuccess = true
  }

  const endpoint = wasSuccess
    ? apiConfig.player.playCardSuccess(entity.id)
    : apiConfig.player.playCardFailure(entity.id)

  const payload = {
    gameId: gameId,
    teamId: team.teamId,
    deckId: deckId.value,
    boardId: team.boardId,
    cost: entity.cost || 0,
    ForceExecution: true,
  }

  try {
    console.log('Wysyłany Id Karty: ', entity.id)
    const response = await apiServices.post<{ message?: string; newTeamBudget: number }>(
      endpoint,
      payload,
    )

    toast.success(response.data?.message || 'Akcja przetworzona pomyślnie.')

    const teamToUpdate = tables.value.find((t) => t.teamId === team.teamId)
    if (teamToUpdate) {
      teamToUpdate.teamBud = response.data.newTeamBudget
    } else {
      await fetchTeams()
    }

    await fetchAvailableCardsForTeam()
  } catch (error: any) {
    if (error.response?.data?.errorCode === 'NotEnoughBudget') {
      toast.warning(t('teamHasNotEnoughBits', { teamName: team.teamName }));
      return
    }
    toast.error(error.response?.data?.message || 'Wystąpił błąd podczas wykonywania akcji.')
    console.error('Błąd akcji karty/przedmiotu:', error.response?.data || error.message)
  }
}
const playCard = () => executeAction(true)
const giveItem = () => executeAction(false)

const approveDecision = async (logId: number) => {
  console.log('Jaki log jest do zatwierdzenia  ?', logId);
  try {
    const response = await apiServices.post(apiConfig.player.approveLog(logId), {})
    toast.success('Sugestia została zatwierdzona!');
    console.log(response.data, 'Co otrzymałem po approve ?');
    await Promise.all([fetchPendingDecisions(), fetchDecisionHistory()])
    console.log('Zaktualizowano listę decyzji po zatwierdzeniu do zatwierdzenia:', pendingDecisions.value);
    console.log('Pobrana historia decyzji:', decisions.value);
  } catch (error: any) {
    toast.error('Wystąpił błąd podczas zatwierdzania sugestii.')
    console.error('Błąd zatwierdzania:', error.response?.data || error.message)
  }
}

const rejectDecision = async (logId: number) => {
  try {
    await apiServices.delete(apiConfig.player.rejectLog(logId))
    toast.info('Sugestia została odrzucona.')
    await fetchPendingDecisions()
  } catch (error: any) {
    toast.error('Wystąpił błąd podczas odrzucania sugestii.')
    console.error('Błąd odrzucania:', error.response?.data || error.message)
  }
}

async function applySelectedEvent() {
  if (!selectedPendingEventIndex.value) {
    toast('Proszę wybrać zdarzenie do aktywacji.')
    return
  }
  try {
    await apiServices.post(apiConfig.player.applyEvent(gameId), {
      eventId: selectedPendingEventIndex.value,
    })
    toast.success('Zdarzenie zostało aktywowane!')
  } catch (error: any) {
    toast.error('Błąd podczas aktywacji zdarzenia.')
    console.error('Błąd aktywacji zdarzenia:', error.response?.data || error.message)
  }
}

const formatDate = (timestamp: string) => new Date(timestamp).toLocaleString('pl-PL')

onMounted(async () => {
  if (!gameId) return

  await fetchGameDetails()

  if (deckId.value) {
    await Promise.all([
      fetchTeams(),
      fetchDecisionHistory(),
      fetchPendingDecisions(),
      fetchGameEvents(deckId.value),
      fetchRivalBoard(),
    ])
  } else {
    toast.error('Nie udało się pobrać ID talii. Niektóre funkcje mogą nie działać.')
    await Promise.all([
      fetchTeams(),
      fetchDecisionHistory(),
      fetchPendingDecisions(),
      fetchRivalBoard(),
    ])
  }

  if (teamId.value) {
    selectedTableId.value = teamId.value
  }

  try {
    await signalService.start()
    await signalService.joinGameRoomAsAdmin(String(gameId))
    console.log('Połączono z SignalR i dołączono do pokoju gry.')
    signalService.connection.on('HistoryUpdated', () => fetchDecisionHistory())
    signalService.connection.on('PendingUpdated', () => fetchPendingDecisions())
    signalService.connection.on('BoardUpdated', () => fetchRivalPawns())
  } catch (err: any) {
    console.error('Błąd połączenia SignalR: ', err)
  }
})

onUnmounted(() => {
  if (gameId) signalService.leaveGameRoomAsAdmin(String(gameId))
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
