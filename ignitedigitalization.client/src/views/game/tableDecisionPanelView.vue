<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <div class="text-center">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-surface-500 mb-2">
        {{ t('decisionPanel') }}
      </h1>
      <p class="text-surface-400 text-sm md:text-base mb-3">
        {{ t('manageDecisionsForTeam') }}
      </p>
      <div
        class="inline-flex gap-3 items-center px-6 py-3 bg-secondary border border-surface-700 rounded-xl shadow-lg cursor-pointer hover:border-primary-500 hover:bg-primary-500/10 transition-all duration-200"
        :title="t('clickToChangeTeam')"
        @click="showTeamPicker = true"
      >
        <template v-if="managedTeamIds.length <= 1">
          <div
            class="w-5 h-5 rounded-full ring-2 ring-surface-600 shadow-lg"
            :style="{ backgroundColor: teamData?.teamColor }"
          ></div>
          <span class="font-nasalization font-bold text-2xl text-surface-700 tracking-wide">
            {{ teamData?.teamName }}
          </span>
        </template>
        <template v-else>
          <div class="flex items-center gap-2 flex-wrap justify-center">
            <div
              v-for="team in managedTeamsData.slice(0, 3)"
              :key="team.teamId"
              class="flex items-center gap-1.5 px-2 py-1 rounded-lg border border-surface-600 bg-surface-800/60"
            >
              <div
                class="w-3.5 h-3.5 rounded-full ring-1 ring-surface-500 flex-shrink-0"
                :style="{ backgroundColor: team.teamColor }"
              ></div>
              <span class="font-semibold text-sm text-surface-700">{{ team.teamName }}</span>
            </div>
            <div
              v-if="managedTeamsData.length > 3"
              class="flex items-center gap-1.5 px-2 py-1 rounded-lg border border-surface-600 bg-surface-700/60 text-surface-700   text-sm font-semibold"
            >
              +{{ managedTeamsData.length - 3 }}
            </div>
          </div>
        </template>
        <font-awesome-icon :icon="faChevronDown" class="h-4 text-surface-400 flex-shrink-0" />
      </div>

      <Dialog
        v-model:visible="showTeamPicker"
        :header="t('changeTeam')"
        modal
        :style="{ width: '400px' }"
        :pt="{ root: { class: 'bg-secondary border border-surface-700' } }"
      >
        <div class="space-y-2 mt-2">
          <div
            v-if="loadingTeams"
            class="text-center py-6"
          >
            <ProgressSpinner style="width: 2rem; height: 2rem" strokeWidth="4" />
          </div>
          <div
            v-else
            v-for="team in availableTeams"
            :key="team.teamId"
            class="flex items-center gap-2 px-3 py-3 rounded-lg border transition-all duration-150"
            :class="isManagedTeam(team.teamId)
              ? 'border-primary-500 bg-primary-500/15'
              : 'border-surface-700 bg-surface-800'"
          >
            <input
              type="checkbox"
              :checked="isManagedTeam(team.teamId)"
              @change.stop="toggleManagedTeam(team)"
              class="w-4 h-4 flex-shrink-0 cursor-pointer accent-primary-500"
              :title="t('alsoManage')"
            />
            <div class="flex items-center gap-3 flex-1">
              <div
                class="w-4 h-4 rounded-full ring-2 ring-surface-600 flex-shrink-0"
                :style="{ backgroundColor: team.teamColor }"
              ></div>
              <span class="font-semibold text-surface-500">{{ team.teamName }}</span>
            </div>
          </div>
        </div>
      </Dialog>
    </div>

    <div class="max-w-7xl mx-auto w-full">
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div class="space-y-6">
          <div class="border border-surface-700 rounded-xl p-6 bg-secondary shadow-2xl">
            <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
              <div class="bg-primary-500/20 p-3 rounded-lg">
                <font-awesome-icon :icon="faGamepad" class="h-6 text-primary-400" />
              </div>
              <h2 class="text-xl md:text-2xl font-bold text-surface-500">{{ t('actionManagment') }}</h2>
            </div>

            <div v-if="managedTeamIds.length > 1" class="mb-4">
              <label class="block mb-2 text-sm font-semibold text-surface-300">{{ t('selectTable') }}</label>
              <Dropdown
                v-model="selectedPlayTeamId"
                :options="managedTeamsData"
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
                    <span class="text-green-400">{{ slotProps.option.teamBud }} {{ t('bits') }}</span>
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

            <div
              v-if="selectedCard"
              class="bg-secondary rounded-lg p-4 border border-surface-700 mb-4"
            >
              <p class="text-sm text-surface-400 mb-1">{{ t('description') }}:</p>
              <p class="text-sm text-surface-300">{{ selectedCard?.description }}</p>
              <div class="flex items-center justify-between mt-3 pt-3 border-t border-surface-700">
                <span class="text-sm text-surface-400">{{ t('cost') }}:</span>
                <span class="text-lg font-bold text-green-400">
                  {{ selectedCard?.cost || 0 }}
                  {{ t('bits') }}
                </span>
              </div>
            </div>

            <div
              v-if="selectedPlayTeam"
              class="bg-secondary rounded-lg p-4 border border-surface-700 mb-4"
            >
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm text-surface-400">{{ t('teamBudget') }}:</p>
                  <p class="text-sm font-semibold text-surface-500">{{ selectedPlayTeam.teamName }}</p>
                </div>
                <span class="text-2xl font-bold text-green-400"
                  >{{ selectedPlayTeam.teamBud }} {{ t('bits') }}</span
                >
              </div>
            </div>

            <div class="flex justify-center">
              <Button
                :disabled="!selectedCardId || isSubmitting"
                @click="playCard"
                :label="isSubmitting ? t('sending') : t('playCard')"
                size="large"
                class="w-full"
              />
            </div>
          </div>

          <div class="w-full space-y-3">
            <div class="flex gap-2 justify-center">
              <Button
                :label="t('yourBoard')"
                :severity="boardView === 'own' ? undefined : 'secondary'"
                @click="boardView = 'own'"
                size="small"
                outlined
              />
              <Button
                :label="t('rivalBoard')"
                :severity="boardView === 'rival' ? undefined : 'secondary'"
                @click="boardView = 'rival'"
                size="small"
                outlined
              />
            </div>
            <div v-if="boardView === 'own'" class="w-full flex justify-center">
              <GameBoardCartesian
                :config="formData"
                :pawns="managedTeamIds.length > 1 ? allManagedPawns : pawns"
                :circle-mode="managedTeamIds.length > 1"
              />
            </div>
            <div v-else class="w-full flex justify-center">
              <GameBoard :config="enemyFormData" :game-mode="false" :pawns="enemyPawns" :use-percentage="true" />
            </div>
          </div>
        </div>

        <div class="border border-surface-700 rounded-xl p-6 bg-secondary shadow-2xl">
          <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
            <div class="bg-yellow-500/20 p-3 rounded-lg">
              <font-awesome-icon :icon="faClock" class="h-6 text-yellow-400" />
            </div>
            <h2 class="text-xl md:text-2xl font-bold text-surface-500">{{ t('decisionPanel') }}</h2>
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
                  <div class="flex items-center gap-2 mb-0.5">
                    <div
                      v-if="managedTeamIds.length > 1 && entry.teamColor"
                      class="w-3 h-3 rounded-full flex-shrink-0"
                      :style="{ backgroundColor: entry.teamColor }"
                    ></div>
                    <p class="text-surface-500 font-semibold">{{ entry.tableName }}</p>
                  </div>
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
                <p class="text-xs text-surface-300 mt-1">
                  {{ t('cardId') }}:
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
                  :disabled="approvingLogIds.has(entry.logId)"
                  :loading="approvingLogIds.has(entry.logId)"
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
                  :disabled="rejectingLogIds.has(entry.logId)"
                  :loading="rejectingLogIds.has(entry.logId)"
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
                    <div class="flex items-center gap-2 mb-0.5">
                      <div
                        v-if="managedTeamIds.length > 1 && entry.teamColor"
                        class="w-3 h-3 rounded-full flex-shrink-0"
                        :style="{ backgroundColor: entry.teamColor }"
                      ></div>
                      <p class="text-surface-500 font-semibold">{{ entry.tableName }}</p>
                    </div>
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
import { ref, reactive, computed, watch, onMounted, onUnmounted } from 'vue'
import { useToast } from 'vue-toastification'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import {
  faGamepad,
  faBolt,
  faClock,
  faHistory,
  faCheck,
  faTimes,
  faChevronDown,
} from '@fortawesome/free-solid-svg-icons'
import Dropdown from 'primevue/dropdown'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import ProgressSpinner from 'primevue/progressspinner'

import GameBoardCartesian from '@/components/game/gameBoardCartesian.vue'
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
interface AvailableTeam {
  teamId: number
  teamName: string
  teamColor: string
  teamBud: number
  deckId: number
  boardId: number
}
interface Card {
  id: number
  cardsId: number
  deckId: number
  displayOrder: number
  title: string
  description: string
  cost?: number
  enablers?: unknown[]
}

interface DecisionLog {
  isEventNotification: boolean
  timestamp: string
  feedbackDescription: string
  cardId?: number
  cardTitle?: string
  tableId?: number
  tableName?: string
  teamColor?: string
  result?: 'Pozytywny' | 'Negatywny'
  eventAppliedId?: number | null
}
interface PendingDecision {
  logId: number
  cardId: number
  cardTitle: string
  tableId: number
  tableName: string
  teamColor?: string
  timestamp: string
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
interface RawPawn {
  gpId: number
  posX: string
  posY: string
  color: string
  name: string
  maxPosX?: number
  maxPosY?: number
}
interface RivalBoardConfig {
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
interface RawPawnData {
  teamId: number
  posX: string | number
  posY: string | number
  teamColor: string
  teamName: string
  maxPosX?: number
  maxPosY?: number
}

const props = defineProps({
  gameId: { type: [Number, String], required: true },
  teamId: { type: [Number, String], required: false },
  allTeams: { type: Boolean, default: false },
})

const toast = useToast()
const { t } = useI18n()
const router = useRouter()

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

const loading = reactive({ teamData: true, cards: true, history: true, pending: true })
const teamData = ref<TeamData | null>(null)
const availableTeams = ref<AvailableTeam[]>([])
const showTeamPicker = ref(false)
const loadingTeams = ref(false)
const currentTeamId = ref<number>(props.teamId ? Number(props.teamId) : 0)
const managedTeamIds = ref<number[]>(props.teamId ? [Number(props.teamId)] : [])
const boardView = ref<'own' | 'rival'>('own')
const enemyFormData = reactive<RivalBoardConfig>({
  boardId: 0,
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
  cellsDescriptions: '',
})
const enemyPawns = ref<Pawn[]>([])
const cards = ref<Card[]>([])
const decisions = ref<DecisionLog[]>([])
const pendingDecisions = ref<PendingDecision[]>([])
const approvingLogIds = ref<Set<number>>(new Set())
const rejectingLogIds = ref<Set<number>>(new Set())
const pawns = ref<Pawn[]>([])

const decisionMode = ref<'pending' | 'history'>('history')
const selectedCardId = ref<number | null>(null)
const isSubmitting = ref(false)
const selectedPlayTeamId = ref<number>(props.teamId ? Number(props.teamId) : 0)
const allManagedPawns = ref<Pawn[]>([])

const managedTeamsData = computed<AvailableTeam[]>(() => {
  const fromAvailable = availableTeams.value.filter((t) => managedTeamIds.value.includes(t.teamId))
  // Ensure primary team is always present even if availableTeams hasn't loaded yet
  if (teamData.value && !fromAvailable.find((t) => t.teamId === teamData.value!.teamId)) {
    return [{ ...teamData.value } as AvailableTeam, ...fromAvailable]
  }
  return fromAvailable
})

const selectedPlayTeam = computed<TeamData | AvailableTeam | null>(() => {
  if (selectedPlayTeamId.value === currentTeamId.value && teamData.value) return teamData.value
  return availableTeams.value.find((t) => t.teamId === selectedPlayTeamId.value) ?? null
})

watch(selectedPlayTeamId, () => {
  selectedCardId.value = null
  fetchAvailableCardsAndItems()
})

const selectedCard = computed<Card | undefined>(() =>
  cards.value.find((c) => c.id === selectedCardId.value),
)

const isManagedTeam = (teamId: number) => managedTeamIds.value.includes(teamId)

const toggleManagedTeam = (team: AvailableTeam) => {
  const idx = managedTeamIds.value.indexOf(team.teamId)
  if (idx === -1) {
    managedTeamIds.value.push(team.teamId)
    fetchDecisionHistory()
    fetchPendingDecisions()
    fetchAllManagedPawns()
  } else {
    if (team.teamId === currentTeamId.value) {
      const remaining = managedTeamIds.value.filter((id) => id !== team.teamId)
      if (remaining.length === 0) return // ostatnia drużyna — nie można usunąć
      managedTeamIds.value.splice(idx, 1)
      const nextTeamId = remaining[0]
      currentTeamId.value = nextTeamId
      selectedPlayTeamId.value = nextTeamId
      router.replace({ name: 'table-decision-panel', params: { gameId: props.gameId, teamId: nextTeamId } })
      fetchAllDataForTeam()
      return
    }
    managedTeamIds.value.splice(idx, 1)
    fetchDecisionHistory()
    fetchPendingDecisions()
    fetchAllManagedPawns()
  }
}

const fetchTeams = async () => {
  loadingTeams.value = true
  try {
    const response = await apiServices.get<AvailableTeam[]>(
      apiConfig.player.getTeamsManagement(Number(props.gameId)),
    )
    availableTeams.value = response.data
  } catch (error) {
    console.error('Błąd pobierania drużyn:', error)
  } finally {
    loadingTeams.value = false
  }
}

const fetchAllDataForTeam = async () => {
  const gameIdNum = Number(props.gameId)
  const teamIdNum = currentTeamId.value
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
    ])
    if (managedTeamIds.value.length > 1) await fetchAllManagedPawns()
  } catch (error) {
    toast.error(t('errorLoadingTeamData'))
    console.error('Błąd w fetchAllDataForTeam:', error)
  } finally {
    Object.keys(loading).forEach((k) => (loading[k as keyof typeof loading] = false))
  }
}

const fetchAllManagedPawns = async () => {
  const gameIdNum = Number(props.gameId)
  const teamsToFetch = managedTeamIds.value
    .map((tId) => {
      const t = availableTeams.value.find((x) => x.teamId === tId)
      const teamColor = t?.teamColor ?? (tId === currentTeamId.value ? teamData.value?.teamColor : undefined) ?? '#ffffff'
      if (t?.boardId) return { teamId: tId, boardId: t.boardId, teamColor }
      if (tId === currentTeamId.value && teamData.value?.boardId)
        return { teamId: tId, boardId: teamData.value.boardId, teamColor }
      return null
    })
    .filter((x): x is { teamId: number; boardId: number; teamColor: string } => !!x)
  try {
    const results = await Promise.all(
      teamsToFetch.map(async ({ teamId, boardId, teamColor }) => {
        const url = apiConfig.player.getPawns(gameIdNum, teamId, boardId)
        const r = await apiServices.get<RawPawn[]>(url)
        return r.data.map((p) => ({
          id: p.gpId,
          x: Number(p.posX),
          y: Number(p.posY),
          color: teamColor,
          name: p.name,
          maxX: Number(p.maxPosX) || 1,
          maxY: Number(p.maxPosY) || 1,
        }))
      }),
    )
    allManagedPawns.value = results.flat()
  } catch (err) {
    console.error('Błąd pobierania pionków wielu drużyn:', err)
  }
}

const fetchAvailableCardsAndItems = async () => {
  const playTeam = selectedPlayTeam.value
  if (!playTeam?.deckId || !props.gameId) return
  try {
    const url = apiConfig.player.getCards(
      playTeam.deckId,
      Number(props.gameId),
      playTeam.teamId,
    )
    const response = await apiServices.get<{ decisionCards: Card[] }>(url)
    cards.value = response.data.decisionCards || []
  } catch (error) {
    toast.error(t('errorFetchingCardsAndItems'))
    console.error('Błąd pobierania kart:', error)
  }
}

const fetchDecisionHistory = async () => {
  try {
    const responses = await Promise.all(
      managedTeamIds.value.map((tId) =>
        apiServices.post<RawHistoryLog[]>(apiConfig.player.getPlayerHistory, {
          gameId: props.gameId,
          teamId: tId,
        }),
      ),
    )
    const allLogs = responses.flatMap((r) => r.data || [])
    allLogs.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime())
    decisions.value = allLogs.map((log) => {
      const teamColor =
        availableTeams.value.find((x) => x.teamId === log.teamId)?.teamColor ??
        (log.teamId === currentTeamId.value ? teamData.value?.teamColor : undefined)
      return log.isEventNotification
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
            teamColor,
            timestamp: log.timestamp,
            feedbackDescription: log.feedbackDescription,
            result: log.status ? 'Pozytywny' : 'Negatywny',
            eventAppliedId: log.gameEventId,
          }
    })
  } catch (error) {
    toast.error(t('errorFetchingDecsionHistory'))
  }
}

const fetchPendingDecisions = async () => {
  try {
    const groups = await Promise.all(
      managedTeamIds.value.map(async (tId) => {
        const teamColor =
          availableTeams.value.find((x) => x.teamId === tId)?.teamColor ??
          (tId === currentTeamId.value ? teamData.value?.teamColor : undefined)
        const r = await apiServices.get<PendingDecision[]>(
          apiConfig.player.getPendingLogsForTeam(Number(props.gameId), tId),
        )
        return (r.data || []).map((entry) => ({ ...entry, teamColor }))
      }),
    )
    pendingDecisions.value = groups.flat()
  } catch (error) {
    toast.error(t('errorFetchingSuggestions'))
  }
}

const fetchRivalBoard = async () => {
  try {
    const url = apiConfig.player.getGameData(Number(props.gameId))
    const response = await apiServices.get<{ rivalBoardConfig: RivalBoardConfig }>(url)
    if (response.data.rivalBoardConfig) {
      Object.assign(enemyFormData, response.data.rivalBoardConfig)
      await fetchRivalPawns()
    }
  } catch (error) {
    console.error('Błąd pobierania planszy rywali:', error)
  }
}

const fetchRivalPawns = async () => {
  if (!enemyFormData.boardId || !props.gameId) return
  try {
    const url = apiConfig.player.getRivalPawns(Number(props.gameId), enemyFormData.boardId)
    const response = await apiServices.get<RawPawnData[]>(url)
    enemyPawns.value = (response.data || []).map((p) => ({
      id: p.teamId,
      x: Number(p.posX),
      y: Number(p.posY),
      color: p.teamColor,
      name: p.teamName,
      maxX: Number(p.maxPosX) || 1,
      maxY: Number(p.maxPosY) || 1,
    }))
  } catch (err) {
    console.error('Błąd pobierania pionków rywali:', err)
  }
}

const fetchPawns = async () => {
  if (!teamData.value?.boardId || !props.gameId || !currentTeamId.value) return
  try {
    const url = apiConfig.player.getPawns(
      Number(props.gameId),
      currentTeamId.value,
      teamData.value.boardId,
    )

    const response = await apiServices.get<RawPawn[]>(url)

    pawns.value = response.data.map((p) => ({
      id: p.gpId,
      x: Number(p.posX),
      y: Number(p.posY),
      color: teamData.value?.teamColor ?? p.color,
      name: p.name,
      maxX: Number(p.maxPosX) || 1,
      maxY: Number(p.maxPosY) || 1,
    }))
  } catch (err) {
    console.error('Błąd pobierania pionków:', err)
  }
}

async function playCard() {
  const entity = selectedCard.value
  const team = selectedPlayTeam.value

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

  const hasEnablers = Array.isArray(entity.enablers) && entity.enablers.length > 0
  if (hasEnablers) {
    const enablerTitles = (entity.enablers as number[])
      .map((id) => cards.value.find((c) => c.id === id)?.title ?? `ID ${id}`)
      .join(', ')
    toast.warning(t('cardRequiresEnablers', { enablers: enablerTitles }))
  }

  const wasSuccess = !hasEnablers
  const minEnablerId = hasEnablers
    ? Math.min(...(entity.enablers as number[]))
    : undefined

  const endpoint = wasSuccess
    ? apiConfig.player.playCardSuccess(entity.cardsId)
    : apiConfig.player.playCardFailure(entity.cardsId)

  const payload = {
    gameId: Number(props.gameId),
    teamId: team.teamId,
    deckId: team.deckId,
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

    if (team.teamId === currentTeamId.value && teamData.value) {
      teamData.value.teamBud = response.data.newTeamBudget
    } else {
      const managedTeam = availableTeams.value.find((x) => x.teamId === team.teamId)
      if (managedTeam) managedTeam.teamBud = response.data.newTeamBudget
    }

    await Promise.all([fetchAvailableCardsAndItems(), fetchDecisionHistory()])
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
  if(approvingLogIds.value.has(logId)) return
  approvingLogIds.value = new Set(approvingLogIds.value).add(logId)
  try {
    await apiServices.post(apiConfig.player.approveLog(logId), {})
    toast.success(t('decisionApprovedSuccess'))
    selectedCardId.value = null
    await fetchAllDataForTeam()
  } catch (error) {
    toast.error(t('errorApprovingSuggestion'))
  } finally {
    const next = new Set(approvingLogIds.value)
    next.delete(logId)
    approvingLogIds.value = next
  }
}

const rejectDecision = async (logId: number) => {
  if(rejectingLogIds.value.has(logId)) return
  rejectingLogIds.value = new Set(rejectingLogIds.value).add(logId)
  try {
    await apiServices.delete(apiConfig.player.rejectLog(logId))
    toast.info(t('suggestionRejected'))
    await fetchPendingDecisions()
  } catch (error) {
    toast.error(t('errorRejectingSuggestion'))
  } finally {
    const next = new Set(rejectingLogIds.value)
    next.delete(logId)
    rejectingLogIds.value = next
  }
}

const formatDate = (timestamp: string) => new Date(timestamp).toLocaleString('pl-PL')

const isViewMounted = ref(true)

onMounted(async () => {
  const gameIdNum = Number(props.gameId)
  if (isNaN(gameIdNum)) {
    toast.error(t('criticalGameIdError'))
    return
  }

  if (props.allTeams) {
    await Promise.all([fetchTeams(), fetchRivalBoard()])
    if (availableTeams.value.length > 0) {
      const firstTeam = availableTeams.value[0]
      currentTeamId.value = firstTeam.teamId
      selectedPlayTeamId.value = firstTeam.teamId
      managedTeamIds.value = availableTeams.value.map((t) => t.teamId)
    }
    await fetchAllDataForTeam()
  } else {
    await Promise.all([fetchAllDataForTeam(), fetchTeams(), fetchRivalBoard()])
  }

  signalService.connection.on('HistoryUpdated', () => fetchDecisionHistory())
  signalService.connection.on('PendingUpdated', () => fetchPendingDecisions())
  signalService.connection.on('BoardUpdated', () => {
    fetchPawns()
    fetchRivalPawns()
    if (managedTeamIds.value.length > 1) fetchAllManagedPawns()
  })
  signalService.connection.on('BudgetUpdated', () => fetchAllDataForTeam())
  signalService.connection.on('PhaseUpdated', () => fetchAllDataForTeam())

  try {
    await signalService.start()
    await signalService.joinGameRoomAsPlayer(String(gameIdNum), String(currentTeamId.value))
    signalService.connection.onreconnected(async () => {
      if (!isViewMounted.value) return
      await signalService.joinGameRoomAsPlayer(String(gameIdNum), String(currentTeamId.value))
      await fetchAllDataForTeam()
    })
  } catch (err) {
    console.error('Błąd połączenia SignalR: ', err)
  }
})

onUnmounted(() => {
  isViewMounted.value = false
  if (props.gameId) {
    signalService.leaveGameRoomAsPlayer(String(props.gameId), String(currentTeamId.value))
    signalService.connection.off('HistoryUpdated')
    signalService.connection.off('PendingUpdated')
    signalService.connection.off('BoardUpdated')
    signalService.connection.off('BudgetUpdated')
    signalService.connection.off('PhaseUpdated')
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
