<template>
  <div v-if="props.isVisible" class="fixed inset-0 flex items-center justify-center z-10">
    <div class="absolute inset-0 bg-black/70" @click="closeModal"></div>

    <div
      class="bg-secondary z-20 text-white relative border border-surface-700 animate-jump-in w-full h-full p-4 overflow-y-auto custom-scrollbar sm:w-[90vw] sm:max-w-4xl sm:h-auto sm:max-h-[90vh] sm:rounded-lg sm:p-6 md:p-8 lg:p-10"
    >
      <button @click="closeModal" class="absolute top-3 right-3 sm:top-2 sm:right-2 w-8 h-8 z-10">
        <font-awesome-icon
          :icon="faXmark"
          class="h-5 text-white hover:text-primary-400 transition-all duration-100"
        />
      </button>

      <h1 class="text-center text-white font-nasalization text-lg sm:text-xl md:text-2xl mt-1 mb-3">
        {{ t('createNewGame') }}
      </h1>
      <hr class="my-3 border-surface-700" />

      <!-- Wskaźnik: 2 kropki -->
      <div class="flex flex-row justify-center space-x-2">
        <div class="rounded-full bg-primary-400 h-3 w-3"></div>
        <div
          class="rounded-full h-3 w-3"
          :class="step >= 2 ? 'bg-primary-400' : 'bg-surface-700'"
        ></div>
      </div>

      <form class="mt-3" @submit.prevent="handleSubmit">
        <!-- Krok 1: Szkolenie + tryb gry -->
        <div v-if="step === 1" :class="direction === 'backwards' ? 'animate-fade-left' : ''">
          <div class="space-y-1 mb-2">
            <label for="gameName" class="block font-bold text-left text-xs sm:text-sm">
              {{ t('gameName') }}
            </label>
            <InputText
              id="gameName"
              v-model="gameName"
              :placeholder="t('gameNamePlaceholder')"
              maxlength="25"
              required
              class="w-full"
            />
          </div>

          <div class="mb-2">
            <label for="selectTraining" class="block font-bold text-left text-xs mb-1">
              {{ t('training') }}
            </label>
            <Dropdown
              v-model="selectedTrainingId"
              :options="data.trainings"
              optionLabel="title"
              optionValue="id"
              :placeholder="t('selectTrainingPlaceholder')"
              class="w-full custom-dropdown"
              :loading="isLoadingEconomy"
            />
            <p
              v-if="selectedTraining && (selectedTraining.defaultTeamsBoardId || selectedTraining.defaultRivalsBoardId)"
              class="text-xs text-surface-400 mt-1"
            >
              {{ t('boardsAutoFilledFromTraining') }}
            </p>
          </div>

          <!-- Plansze – widoczne gdy szkolenie nie ma domyślnych -->
          <div
            v-if="selectedTrainingId && (!selectedTraining?.defaultTeamsBoardId || !selectedTraining?.defaultRivalsBoardId)"
            class="mb-3 space-y-2 border border-surface-700 rounded-lg p-3"
          >
            <p class="text-xs text-yellow-400 font-semibold mb-2">
              {{ t('trainingHasNoDefaultBoards') }}
            </p>
            <div v-if="!selectedTraining?.defaultTeamsBoardId">
              <label class="block font-bold text-left text-xs mb-1">{{ t('selectTeamBoard') }}</label>
              <Dropdown
                v-model="selectedTeamBoardId"
                :options="data.boards"
                optionLabel="name"
                optionValue="boards_Id"
                :placeholder="t('selectTeamBoardPlaceholder')"
                class="w-full custom-dropdown"
              />
            </div>
            <div v-if="!selectedTraining?.defaultRivalsBoardId">
              <label class="block font-bold text-left text-xs mb-1">{{ t('selectRivalBoard') }}</label>
              <Dropdown
                v-model="selectedRivalBoardId"
                :options="data.boards"
                optionLabel="name"
                optionValue="boards_Id"
                :placeholder="t('selectRivalBoardPlaceholder')"
                class="w-full custom-dropdown"
              />
            </div>
          </div>

          <p class="block font-bold text-left text-xs mb-2">{{ t('selectGameType') }}</p>
          <div class="flex gap-2 w-full mb-5">
            <div
              class="flex flex-col items-center justify-center rounded-md w-full h-20 gap-2 cursor-pointer"
              :class="
                selectedGameMode === 'remote'
                  ? 'bg-primary-400 shadow-md shadow-primary-400/60'
                  : 'bg-secondary transition delay-150 duration-300 ease-in-out hover:-translate-y-1 hover:scale-105 hover:bg-primary-400/70 border-2 border-surface-700'
              "
              @click="selectedGameMode = 'remote'"
            >
              <font-awesome-icon
                :icon="faGlobe"
                class="h-6"
                :class="selectedGameMode === 'remote' ? 'text-tertiary' : 'text-primary-400'"
              />
              <h2 class="block font-nasalization font-semibold">{{ t('remoteGame') }}</h2>
            </div>
            <div
              class="flex flex-col items-center justify-center rounded-md w-full h-20 gap-2 cursor-pointer"
              :class="
                selectedGameMode === 'stationary'
                  ? 'bg-primary-400 shadow-md shadow-primary-400/60'
                  : 'bg-secondary transition delay-150 duration-300 ease-in-out hover:-translate-y-1 hover:scale-105 hover:bg-primary-400/70 border-2 border-surface-700'
              "
              @click="selectedGameMode = 'stationary'"
            >
              <font-awesome-icon
                :icon="faBuilding"
                class="h-6"
                :class="selectedGameMode === 'stationary' ? 'text-primary' : 'text-primary-400'"
              />
              <h2 class="block font-nasalization font-semibold">{{ t('stationaryGame') }}</h2>
            </div>
          </div>
          <Button @click="handleNextStep" type="button" class="w-full">
            <span>{{ t('next') }}</span>
            <font-awesome-icon :icon="faArrowRight" class="ml-2" />
          </Button>
        </div>

        <!-- Krok 2: Konfiguracja drużyn -->
        <div
          v-if="step === 2"
          :class="direction === 'forwards' ? 'animate-fade-right' : 'animate-fade-left'"
        >
          <div class="flex flex-row gap-2">
            <div class="flex-1">
              <label for="numberOfTeams" class="block font-bold text-left text-xs sm:text-sm mb-1">
                {{ t('numberOfTeams') }}
              </label>
              <InputNumber
                id="numberOfTeams"
                v-model="numberOfTeams"
                :min="2"
                :max="15"
                showButtons
                class="w-full"
              />
            </div>
            <div class="flex-1">
              <label for="numberOfBits" class="block font-bold text-left text-xs sm:text-sm mb-1">
                {{ t('numberOfBits') }}
              </label>
              <InputNumber
                id="numberOfBits"
                v-model="numberOfBits"
                :min="1"
                :max="100000"
                showButtons
                class="w-full"
              />
            </div>
          </div>

          <div class="mb-6 mt-4">
            <label class="block text-left text-xs sm:text-sm font-bold text-white mb-2">
              {{ t('selectTeamToEdit') }}
            </label>
            <Dropdown
              v-model="currentlyEditingTeamId"
              :options="teams"
              optionLabel="name"
              optionValue="id"
              :placeholder="t('selectTeamToEditPlaceholder')"
              class="w-full custom-dropdown"
            >
              <template #value="slotProps">
                <div v-if="slotProps.value != null" class="flex items-center gap-3">
                  <div
                    class="w-4 h-4 rounded-full"
                    :style="{
                      backgroundColor: teams.find((t) => t.id === slotProps.value)?.colour,
                    }"
                  ></div>
                  <span>{{ teams.find((t) => t.id === slotProps.value)?.name }}</span>
                </div>
                <span v-else>{{ slotProps.placeholder }}</span>
              </template>
              <template #option="slotProps">
                <div class="flex items-center gap-3">
                  <div
                    class="w-4 h-4 rounded-full"
                    :style="{ backgroundColor: slotProps.option.colour }"
                  ></div>
                  <span>{{ slotProps.option.name }}</span>
                </div>
              </template>
            </Dropdown>
          </div>

          <div
            v-if="selectedTeam"
            class="p-4 rounded-lg bg-secondary border border-surface-700 mb-4"
          >
            <h3 class="font-bold text-center text-lg mb-4 text-white">
              {{ t('editing') }} <span class="text-primary-400">{{ selectedTeam.name }}</span>
            </h3>
            <div class="space-y-4">
              <div>
                <label
                  :for="'editTeamName-' + selectedTeam.id"
                  class="block text-sm font-medium text-gray-300 mb-1"
                >{{ t('teamName') }}</label>
                <InputText
                  :id="'editTeamName-' + selectedTeam.id"
                  v-model="selectedTeam.name"
                  class="w-full"
                />
              </div>
              <div class="flex flex-col items-center">
                <label
                  :for="'editTeamColor-' + selectedTeam.id"
                  class="block text-sm font-medium text-gray-300 mb-1"
                >{{ t('teamColor') }}</label>
                <input
                  type="color"
                  :id="'editTeamColor-' + selectedTeam.id"
                  v-model="selectedTeam.colour"
                  class="w-20 h-20 rounded-lg cursor-pointer bg-transparent"
                />
              </div>
            </div>
            <label
              :for="'decision-' + selectedTeam.id"
              class="block text-sm font-medium text-gray-300 mb-2 mt-5 cursor-pointer"
            >{{ t('canTeamMakeDecisions') }}</label>
            <div class="flex items-center gap-2">
              <label class="relative inline-block w-11 h-6">
                <input
                  type="checkbox"
                  :id="'decision-' + selectedTeam.id"
                  v-model="selectedTeam.isAbleToMakeDecisions"
                  class="sr-only peer"
                />
                <span
                  class="absolute cursor-pointer inset-0 bg-tertiary rounded-full transition-all duration-300 peer-checked:bg-primary-400 peer-focus:ring-2 peer-focus:ring-primary-400"
                ></span>
                <span
                  class="absolute left-1 top-1 bg-white w-4 h-4 rounded-full transition-transform duration-300 peer-checked:translate-x-5"
                ></span>
              </label>
              <span class="text-sm">{{
                selectedTeam.isAbleToMakeDecisions ? t('independentDecisions') : t('gmControl')
              }}</span>
              <div class="relative">
                <font-awesome-icon
                  :icon="faCircleQuestion"
                  class="text-primary-400 h-4 cursor-pointer"
                  @mouseover="showTip = true"
                  @mouseleave="showTip = false"
                />
                <div
                  class="absolute border border-surface-700 rounded-md bottom-full left-1/2 -translate-x-1/2 mb-1 bg-secondary p-2 text-white text-sm z-20 w-96 flex items-center"
                  v-show="showTip"
                >
                  <div>
                    <div>
                      <h2 class="font-nasalization mb-1 font-semibold text-orange-500">
                        {{ t('gmControl') }}
                      </h2>
                      <span>{{ t('gmControlDescription') }}</span>
                    </div>
                    <hr class="mt-2 border-surface-700" />
                    <div>
                      <h2 class="font-nasalization mb-1 mt-2 font-semibold text-green-500">
                        {{ t('independentDecisions') }}
                      </h2>
                      <span>{{ t('independentDecisionsDescription') }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="flex gap-2">
            <Button @click="handlePreviousStep" type="button" severity="secondary" class="w-full">
              <font-awesome-icon :icon="faArrowLeft" class="mr-2" />
              <span>{{ t('previous') }}</span>
            </Button>
            <Button type="submit" :label="t('createNewGame')" class="w-full" />
          </div>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import {
  faArrowRight,
  faArrowLeft,
  faXmark,
  faGlobe,
  faBuilding,
} from '@fortawesome/free-solid-svg-icons'
import { ref, reactive, watch, computed, onMounted } from 'vue'
import { faCircleQuestion } from '@fortawesome/free-regular-svg-icons'
import { useToast } from 'vue-toastification'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Dropdown from 'primevue/dropdown'
import Button from 'primevue/button'
import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

// --- INTERFEJSY ---
interface Training {
  id: number
  title: string
  defaultTeamsBoardId?: number | null
  defaultRivalsBoardId?: number | null
}
interface Board {
  boards_Id: number
  name: string
}
interface Team {
  id: number
  name: string
  colour: string
  isAbleToMakeDecisions: boolean
}
type ApiError = {
  response?: { data?: { title?: string } | string }
  message: string
}

// --- PROPSY I EMITY ---
const props = defineProps({ isVisible: { type: Boolean, default: false } })
const emits = defineEmits(['close', 'gameCreated'])

// --- ZMIENNE REAKTYWNE ---
const toast = useToast()
const gameName = ref('')
const selectedTrainingId = ref<number | null>(null) // odpowiada Training.id
const selectedTeamBoardId = ref<number | null>(null)
const selectedRivalBoardId = ref<number | null>(null)
const selectedGameMode = ref<'stationary' | 'remote'>('stationary')
const numberOfTeams = ref(2)
const numberOfBits = ref(40)
const teams = ref<Team[]>([])
const currentlyEditingTeamId = ref<number | undefined>(0)
const showTip = ref(false)
const isLoadingEconomy = ref(false)
const step = ref(1)
const direction = ref('')
const data = reactive<{ trainings: Training[]; boards: Board[] }>({ trainings: [], boards: [] })

// --- WŁAŚCIWOŚCI OBLICZENIOWE ---
const selectedTraining = computed<Training | undefined>(() =>
  selectedTrainingId.value !== null
    ? data.trainings.find((tr) => tr.id === selectedTrainingId.value)
    : undefined,
)
const selectedTeam = computed<Team | undefined>(() => {
  if (currentlyEditingTeamId.value === undefined) return undefined
  return teams.value.find((team) => team.id === currentlyEditingTeamId.value)
})

// --- FUNKCJE ---
const updateTeamsArray = (count: number) => {
  const newTeams: Team[] = []
  for (let i = 0; i < count; i++) {
    const existingTeam = teams.value.find((t) => t.id === i)
    newTeams.push({
      id: i,
      name: existingTeam?.name || `${t('team')} ${i + 1}`,
      colour: existingTeam?.colour || defaultColors[i % defaultColors.length],
      isAbleToMakeDecisions: existingTeam?.isAbleToMakeDecisions ?? false,
    })
  }
  teams.value = newTeams
  if (!teams.value.some((team) => team.id === currentlyEditingTeamId.value)) {
    currentlyEditingTeamId.value = teams.value.length > 0 ? 0 : undefined
  }
}

const handleNextStep = () => {
  if (step.value === 1 && !validateFirstStep()) return
  step.value++
  direction.value = 'forwards'
}
const handlePreviousStep = () => {
  step.value--
  direction.value = 'backwards'
}

const validateFirstStep = () => {
  const errors: string[] = []
  if (!gameName.value.trim()) errors.push(t('enterGameName'))
  if (selectedTrainingId.value === null) errors.push(t('selectTraining'))
  const tr = selectedTraining.value
  if (tr && !tr.defaultTeamsBoardId && !selectedTeamBoardId.value)
    errors.push(t('selectTeamBoard'))
  if (tr && !tr.defaultRivalsBoardId && !selectedRivalBoardId.value)
    errors.push(t('selectRivalBoard'))
  if (errors.length > 0) {
    toast.error(errors.join('\n'))
    return false
  }
  return true
}

const fetchTrainingsFromAPI = async () => {
  try {
    const response = await apiService.get<Training[]>(apiConfig.admin.deck.getAll)
    data.trainings = response.data
  } catch (error) {
    const typedError = error as ApiError
    toast.error(
      t('errorFetchingDecks', {
        error:
          (typeof typedError.response?.data === 'object'
            ? typedError.response?.data?.title
            : typedError.response?.data) || typedError.message,
      }),
    )
  }
}

const fetchBoardsFromAPI = async () => {
  try {
    const response = await apiService.get<Board[]>(apiConfig.boards.getAll)
    data.boards = response.data
  } catch {
    // plansze niedostępne – nie blokujemy
  }
}

const closeModal = () => {
  gameName.value = ''
  selectedTrainingId.value = null
  selectedTeamBoardId.value = null
  selectedRivalBoardId.value = null
  numberOfTeams.value = 2
  numberOfBits.value = 40
  step.value = 1
  emits('close')
}

const handleSubmit = async () => {
  if (step.value !== 2) {
    handleNextStep()
    return
  }
  if (teams.value.some((team) => !team.name.trim())) {
    toast.warning(t('pleaseEnterTeamNames'))
    return
  }
  if (numberOfBits.value < 1 || numberOfBits.value > 100000) {
    toast.warning(t('numberOfBitsMustBeBetween'))
    return
  }

  const training = selectedTraining.value
  const gamePayload = {
    GameName: gameName.value,
    BoardId: training?.defaultTeamsBoardId ?? selectedTeamBoardId.value ?? null,
    RivalBoardId: training?.defaultRivalsBoardId ?? selectedRivalBoardId.value ?? null,
    DeckId: selectedTrainingId.value,
    GameMode: selectedGameMode.value !== 'stationary',
    StartBits: Number(numberOfBits.value),
    Teams: teams.value.map((team) => ({
      name: team.name,
      colour: team.colour,
      isAbleToMakeDecisions: team.isAbleToMakeDecisions,
    })),
  }

  try {
    await apiService.post<{ message?: string }>(apiConfig.games.create, gamePayload)
    emits('gameCreated')
    closeModal()
  } catch (error) {
    const typedError = error as ApiError
    const errorMessage =
      (typeof typedError.response?.data === 'object'
        ? typedError.response?.data?.title
        : typedError.response?.data) ||
      typedError.message ||
      t('errorCreatingGame')
    toast.error(errorMessage as string)
  }
}

const defaultColors = [
  '#ef4444', '#8b5cf6', '#10b981', '#ec4899', '#a855f7',
  '#84cc16', '#06b6d4', '#f97316', '#eab308', '#14b8a6',
  '#d946ef', '#22c55e', '#f43f5e', '#6366f1', '#0ea5e9',
]

// Po wyborze szkolenia: pobierz zasady ekonomii i ustaw numberOfBits
watch(selectedTrainingId, async (newId) => {
  if (newId) {
    isLoadingEconomy.value = true
    try {
      const response = await apiService.get<{ map1_Starting_Budget?: number }>(
        apiConfig.admin.deck.getEconomy(newId),
      )
      if (response.data?.map1_Starting_Budget) {
        numberOfBits.value = response.data.map1_Starting_Budget
      }
    } catch {
      // jeśli brak zasad ekonomii, zostaw domyślne
    } finally {
      isLoadingEconomy.value = false
    }
  }
})

watch(
  numberOfTeams,
  (newCount) => {
    const count = Math.max(2, Math.min(15, newCount || 2))
    updateTeamsArray(count)
  },
  { immediate: true },
)

onMounted(async () => {
  await Promise.all([fetchTrainingsFromAPI(), fetchBoardsFromAPI()])
})
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 0.6rem;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
  margin: 0.5rem 0.3rem;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: theme('colors.secondary');
  border-radius: 0.25rem;
  border: 0.1rem solid transparent;
  background-clip: content-box;
}

:deep(.custom-dropdown .p-dropdown-label) {
  padding-left: 0.75rem !important;
}
:deep(.custom-dropdown .p-dropdown-trigger) {
  width: 2.5rem;
}
</style>
