<template>
  <div v-if="props.isVisible" class="fixed inset-0 flex items-center justify-center z-10">
    <div class="absolute inset-0 bg-black/70" @click="closeModal"></div>

    <div
      class="bg-surface-800 z-20 text-white relative border border-surface-700 animate-jump-in w-full h-full p-4 overflow-y-auto custom-scrollbar sm:w-[90vw] sm:max-w-4xl sm:h-auto sm:max-h-[90vh] sm:rounded-lg sm:p-6 md:p-8 lg:p-10"
    >
      <button @click="closeModal" class="absolute top-3 right-3 sm:top-2 sm:right-2 w-8 h-8 z-10">
        <font-awesome-icon
          :icon="faXmark"
          class="h-5 text-white hover:text-primary-400 transition-all duration-100"
        />
      </button>

      <h1 class="text-center text-white font-nasalization text-lg sm:text-xl md:text-2xl mt-1 mb-3">
        Utwórz nową grę
      </h1>
      <hr class="my-3 border-surface-700" />
      <div class="flex flex-row justify-center space-x-2">
        <div class="rounded-full bg-primary-400 h-3 w-3"></div>
        <div
          class="rounded-full h-3 w-3"
          :class="step >= 2 ? 'bg-primary-400' : 'bg-surface-900'"
        ></div>
        <div
          class="rounded-full h-3 w-3"
          :class="step === 3 ? 'bg-primary-400' : 'bg-surface-900'"
        ></div>
      </div>

      <form class="mt-3" @submit.prevent="handleSubmit">
        <!--Krok 1-->
        <div v-if="step === 1" :class="direction === 'backwards' ? 'animate-fade-left' : ''">
          <div class="space-y-1 mb-1 sm:mb-2">
            <label for="gameName" class="block font-bold text-left text-xs sm:text-sm"
              >Nazwa Gry</label
            >
            <InputText
              id="gameName"
              v-model="gameName"
              placeholder="Wprowadź nazwę gry..."
              maxlength="25"
              required
              class="w-full"
            />
          </div>
          <div class="mb-1 sm:mb-2">
            <label for="selectBoard" class="block font-bold text-left text-xs mb-1"
              >Wybierz planszę</label
            >
            <Dropdown
              v-model="selectedBoardId"
              :options="data.boards"
              optionLabel="name"
              optionValue="boards_Id"
              placeholder="Wybierz planszę"
              class="w-full custom-dropdown"
            />
          </div>
          <div class="mb-1 sm:mb-2">
            <label for="selectOpponentBoard" class="block font-bold text-left text-xs mb-1"
              >Wybierz planszę konkurencji</label
            >
            <Dropdown
              v-model="selectedOponentBoardId"
              :options="opponentBoardOptions"
              optionLabel="name"
              optionValue="boards_Id"
              placeholder="Wybierz planszę konkurencji"
              class="w-full"
            />
          </div>
          <div class="mb-1 sm:mb-2">
            <label for="selectDeck" class="block font-bold text-left text-xs mb-1"
              >Wybierz talię kart</label
            >
            <Dropdown
              v-model="selectedDeckId"
              :options="data.decks"
              optionLabel="title"
              optionValue="id"
              placeholder="Wybierz talię kart"
              class="w-full custom-dropdown"
            />
          </div>
          <p class="block font-bold text-left text-xs mb-2">Wybierz rodzaj rozgrywki</p>
          <div class="flex gap-2 w-full mb-5">
            <div
              class="flex flex-col items-center justify-center rounded-md w-full h-20 gap-2 cursor-pointer"
              :class="
                selectedGameMode === 'remote'
                  ? 'bg-primary-400 shadow-md shadow-primary-400/60'
                  : 'bg-surface-850 transition delay-150 duration-300 ease-in-out hover:-translate-y-1 hover:scale-105 hover:bg-primary-400/70 border-2 border-surface-700'
              "
              @click="selectedGameMode = 'remote'"
            >
              <font-awesome-icon
                :icon="faGlobe"
                class="h-6"
                :class="selectedGameMode === 'remote' ? 'text-tertiary' : 'text-primary-400'"
              />
              <h2 class="block font-nasalization font-semibold">Gra zdalna</h2>
            </div>
            <div
              class="flex flex-col items-center justify-center rounded-md w-full h-20 gap-2 cursor-pointer"
              :class="
                selectedGameMode === 'stationary'
                  ? 'bg-primary-400 shadow-md shadow-primary-400/60'
                  : 'bg-surface-850  transition delay-150 duration-300 ease-in-out hover:-translate-y-1 hover:scale-105 hover:bg-primary-400/70 border-2 border-surface-700'
              "
              @click="selectedGameMode = 'stationary'"
            >
              <font-awesome-icon
                :icon="faBuilding"
                class="h-6"
                :class="selectedGameMode === 'stationary' ? 'text-primary' : 'text-primary-400'"
              />
              <h2 class="block font-nasalization font-semibold">Gra stacjonarna</h2>
            </div>
          </div>
          <Button @click="handleNextStep" type="button" class="w-full">
            <span>Dalej</span>
            <font-awesome-icon :icon="faArrowRight" class="ml-2" />
          </Button>
        </div>

        <!--Krok 2-->
        <div
          v-if="step === 2"
          :class="direction === 'forwards' ? 'animate-fade-right' : 'animate-fade-left'"
        >
          <div class="flex flex-row gap-2">
            <div class="flex-1">
              <label for="numberOfTeams" class="block font-bold text-left text-xs sm:text-sm mb-1"
                >Liczba drużyn:</label
              >
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
              <label for="numberOfBits" class="block font-bold text-left text-xs sm:text-sm mb-1"
                >Liczba bitów na start</label
              >
              <InputNumber
                id="numberOfBits"
                v-model="numberOfBits"
                :min="20"
                :max="1000"
                showButtons
                class="w-full"
              />
            </div>
          </div>
          <div class="mb-6 mt-4">
            <label class="block text-left text-xs sm:text-sm font-bold text-white mb-2"
              >Wybierz drużynę do edycji:</label
            >
            <Dropdown
              v-model="currentlyEditingTeamId"
              :options="teams"
              optionLabel="name"
              optionValue="id"
              placeholder="Wybierz drużynę"
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
            class="p-4 rounded-lg bg-surface-850 border border-surface-700 mb-4"
          >
            <h3 class="font-bold text-center text-lg mb-4 text-white">
              Edytujesz: <span class="text-primary-400">{{ selectedTeam.name }}</span>
            </h3>
            <div class="space-y-4">
              <div>
                <label
                  :for="'editTeamName-' + selectedTeam.id"
                  class="block text-sm font-medium text-gray-300 mb-1"
                  >Nazwa drużyny</label
                >
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
                  >Kolor drużyny</label
                >
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
              >Czy drużyna może podejmować samodzielne decyzje?</label
            >
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
                selectedTeam.isAbleToMakeDecisions ? 'Samodzielne decyzje' : 'Kontrola Game Mastera'
              }}</span>
              <div class="relative">
                <font-awesome-icon
                  :icon="faCircleQuestion"
                  class="text-primary-400 h-4 cursor-pointer"
                  @mouseover="showTip = true"
                  @mouseleave="showTip = false"
                />
                <div
                  class="absolute border border-surface-700 rounded-md bottom-full left-1/2 -translate-x-1/2 mb-1 bg-surface-800 p-2 text-white text-sm z-20 w-96 flex items-center"
                  v-show="showTip"
                >
                  <div>
                    <div>
                      <h2 class="font-nasalization mb-1 font-semibold text-orange-500">
                        Kontrola GM'a
                      </h2>
                      <span
                        >Drużyna ma możliwość zasugerowania decyzji ale Game Master musi ją
                        zakceptować
                      </span>
                    </div>
                    <hr class="mt-2 border-surface-700" />
                    <div>
                      <h2 class="font-nasalization mb-1 mt-2 font-semibold text-green-500">
                        Samodzielne decyzje
                      </h2>
                      <span
                        >Drużyna podejmuje decyzje bezpośrednio z urządzenia i nie potrzebuje
                        akceptacji decyzji przez Game Mastera</span
                      >
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="flex gap-2">
            <Button @click="handlePreviousStep" type="button" severity="secondary" class="w-full">
              <font-awesome-icon :icon="faArrowLeft" class="mr-2" />
              <span>Wstecz</span>
            </Button>
            <Button @click="handleNextStep" type="button" class="w-full">
              <span>Dalej</span>
              <font-awesome-icon :icon="faArrowRight" class="ml-2" />
            </Button>
          </div>
        </div>
        <!-- Krok 3 -->
        <div
          v-if="step === 3"
          :class="direction === 'forwards' ? 'animate-fade-right' : 'animate-fade-left'"
        >
          <div class="flex justify-between items-center mb-3">
            <h2 class="block text-left text-sm sm:text-base font-bold text-white">
              Wybierz procesy:
            </h2>
            <Button
              @click="toggleAllProcesses"
              :label="allProcessesSelected ? 'Odznacz wszystkie' : 'Zaznacz wszystkie'"
              size="small"
              outlined
            />
          </div>
          <div v-if="isLoadingProcesses" class="text-center text-gray-400">
            <p>Ładowanie procesów...</p>
          </div>
          <div v-else-if="availableProcesses.length === 0" class="text-center text-gray-400">
            <p>Brak dostępnych procesów dla wybranej talii.</p>
          </div>
          <div v-else class="space-y-3 max-h-60 overflow-y-auto custom-scrollbar pr-2">
            <label
              v-for="process in availableProcesses"
              :key="process.processId"
              class="flex items-center p-3 bg-tertiary rounded-md cursor-pointer hover:bg-primary-400/30 transition-colors duration-200"
            >
              <Checkbox :value="process.processId" v-model="selectedProcessIds" :binary="false" />
              <div class="ml-3 flex items-center gap-2 flex-1">
                <div class="flex-1">
                  <span class="font-bold text-white">{{ process.processDesc }}</span>
                  <span class="text-sm text-gray-400 ml-2"> - {{ process.processLongDesc }}</span>
                </div>
                <span
                  class="w-6 h-6 rounded-full inline-block border-2 border-tertiary flex-shrink-0"
                  :style="{ backgroundColor: process.processColor }"
                ></span>
              </div>
            </label>
          </div>
        </div>
        <div v-if="step === 3" class="flex gap-2 mt-4">
          <Button @click="handlePreviousStep" type="button" severity="secondary" class="w-full">
            <font-awesome-icon :icon="faArrowLeft" class="mr-2" />
            <span>Wstecz</span>
          </Button>
          <Button type="submit" label="Utwórz nową grę" class="w-full" />
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
import Checkbox from 'primevue/checkbox'
import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'

// --- DEFINICJE INTERFEJSÓW ---
interface Board {
  boards_Id: number
  name: string
}
interface Deck {
  id: number
  title: string
}
interface Team {
  id: number
  name: string
  colour: string
  isAbleToMakeDecisions: boolean
}
interface GameProcess {
  processId: number
  processDesc: string
  processLongDesc: string
  processColor: string
}
type ApiError = {
  response?: { data?: { title?: string } }
  message: string
}

// --- PROPSY I EMITY ---
const props = defineProps({ isVisible: { type: Boolean, default: false } })
const emits = defineEmits(['close', 'gameCreated'])

// --- ZMIENNE REAKTYWNE Z TYPOWANIEM ---
const toast = useToast()
const gameName = ref('')
const selectedBoardId = ref<number | null>(null)
const selectedOponentBoardId = ref<number | null>(null)
const selectedDeckId = ref<number | null>(null)
const selectedGameMode = ref<'stationary' | 'remote'>('stationary')
const numberOfTeams = ref(2)
const numberOfBits = ref(20)
const teams = ref<Team[]>([])
const currentlyEditingTeamId = ref<number | undefined>(0)
const showTip = ref(false)
const availableProcesses = ref<GameProcess[]>([])
const selectedProcessIds = ref<number[]>([])
const isLoadingProcesses = ref(false)
const step = ref(1)
const direction = ref('')
const data = reactive<{ boards: Board[]; decks: Deck[] }>({ boards: [], decks: [] })

// --- WŁAŚCIWOŚCI OBLICZENIOWE ---
const selectedTeam = computed<Team | undefined>(() => {
  if (currentlyEditingTeamId.value === undefined) return undefined
  return teams.value.find((team) => team.id === currentlyEditingTeamId.value)
})

const opponentBoardOptions = computed<Board[]>(() => {
  if (!selectedBoardId.value) {
    return data.boards
  }
  return data.boards.filter((board) => board.boards_Id !== selectedBoardId.value)
})

const allProcessesSelected = computed(() => {
  return (
    availableProcesses.value.length > 0 &&
    selectedProcessIds.value.length === availableProcesses.value.length
  )
})

// --- FUNKCJE ---
const toggleAllProcesses = () => {
  if (allProcessesSelected.value) {
    selectedProcessIds.value = []
  } else {
    selectedProcessIds.value = availableProcesses.value.map((p) => p.processId)
  }
}

const updateTeamsArray = (count: number) => {
  const newTeams: Team[] = []
  for (let i = 0; i < count; i++) {
    const existingTeam = teams.value.find((t) => t.id === i)
    newTeams.push({
      id: i,
      name: existingTeam?.name || `Drużyna ${i + 1}`,
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
  if (!gameName.value.trim()) errors.push('Wprowadź nazwę gry')
  if (selectedBoardId.value === null) errors.push('Wybierz planszę')
  if (selectedOponentBoardId.value === null) errors.push('Wybierz planszę konkurencji')
  if (selectedDeckId.value === null) errors.push('Wybierz talię kart')
  if (errors.length > 0) {
    toast.error(errors.join('\n'))
    return false
  }
  return true
}

const fetchBoardsFromAPI = async () => {
  try {
    const response = await apiService.get<Board[]>(apiConfig.boards.getAll)
    data.boards = response.data
  } catch (error) {
    const typedError = error as ApiError
    toast.error(
      `Nie udało się pobrać plansz: ${typedError.response?.data?.title || typedError.message}`,
    )
  }
}

const fetchDecksFromAPI = async () => {
  try {
    const response = await apiService.get<Deck[]>(apiConfig.admin.deck.getAll)
    data.decks = response.data
  } catch (error) {
    const typedError = error as ApiError
    toast.error(
      `Nie udało się pobrać talii kart: ${typedError.response?.data?.title || typedError.message}`,
    )
  }
}

const closeModal = () => {
  gameName.value = ''
  selectedBoardId.value = null
  selectedOponentBoardId.value = null
  selectedDeckId.value = null
  numberOfTeams.value = 2
  step.value = 1
  numberOfBits.value = 20
  emits('close')
}

const handleSubmit = async () => {
  if (step.value !== 3) {
    handleNextStep()
    return
  }
  if (teams.value.some((team) => !team.name.trim())) {
    toast.error(`Nazwy drużyn nie mogą być puste.`)
    return
  }
  if (numberOfBits.value < 1 || numberOfBits.value > 100000) {
    toast.error('Liczba bitów na start musi być pomiędzy 1 a 100000.')
    return
  }
  if (selectedProcessIds.value.length === 0) {
    toast.error('Wybierz co najmniej jeden proces do gry.')
    return
  }

  const selectedProcessesForPayload = availableProcesses.value
    .filter((p) => selectedProcessIds.value.includes(p.processId))
    .map((p) => ({ Name: p.processLongDesc, ShortName: p.processDesc }))

  const gamePayload = {
    GameName: gameName.value,
    BoardId: selectedBoardId.value,
    RivalBoardId: selectedOponentBoardId.value,
    DeckId: selectedDeckId.value,
    GameMode: selectedGameMode.value !== 'stationary',
    StartBits: Number(numberOfBits.value),
    Teams: teams.value.map((team) => ({
      Name: team.name,
      Colour: team.colour,
      IsAbleToMakeDecisions: team.isAbleToMakeDecisions,
    })),
    Processes: selectedProcessesForPayload,
  }

  try {
    const response = await apiService.post<{ message?: string }>(
      apiConfig.games.create,
      gamePayload,
    )
    toast.success(response.data.message || `Gra "${gamePayload.GameName}" utworzona pomyślnie!`)
    emits('gameCreated')
    closeModal()
  } catch (error) {
    const typedError = error as {
      response?: { data?: { title?: string } | string }
      message: string
    }
    const errorMessage =
      (typeof typedError.response?.data === 'object'
        ? typedError.response.data.title
        : typedError.response?.data) ||
      typedError.message ||
      'Nie udało się utworzyć gry.'
    toast.error(errorMessage)
  }
}

const defaultColors = [
  '#ef4444', // red-500
  '#8b5cf6', // purple-500
  '#10b981', // green-500
  '#ec4899', // pink-500
  '#a855f7', // violet-500
  '#84cc16', // lime-500
  '#06b6d4', // cyan-500
  '#f97316', // orange-500
  '#eab308', // yellow-500
  '#14b8a6', // teal-500
  '#d946ef', // fuchsia-500
  '#22c55e', // green-400
  '#f43f5e', // rose-500
  '#6366f1', // indigo-500
  '#0ea5e9', // sky-500
]

watch(selectedBoardId, (newId) => {
  if (newId === selectedOponentBoardId.value) {
    selectedOponentBoardId.value = null
  }
})

watch(selectedDeckId, async (newDeckId) => {
  if (newDeckId) {
    isLoadingProcesses.value = true
    availableProcesses.value = []
    selectedProcessIds.value = []
    try {
      const response = await apiService.get<GameProcess[]>(apiConfig.processes.getByDeck(newDeckId))
      availableProcesses.value = response.data
    } catch {
      toast.error('Nie udało się pobrać procesów dla wybranej talii.')
    } finally {
      isLoadingProcesses.value = false
    }
  } else {
    availableProcesses.value = []
    selectedProcessIds.value = []
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
  await fetchBoardsFromAPI()
  await fetchDecksFromAPI()
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
  background: #a78bfa;
  border-radius: 0.25rem;
  border: 0.1rem solid transparent;
  background-clip: content-box;
}

/* Fix dla dropdownów - usuwa padding z lewej strony */
:deep(.custom-dropdown .p-dropdown-label) {
  padding-left: 0.75rem !important;
}

:deep(.custom-dropdown .p-dropdown-trigger) {
  width: 2.5rem;
}
</style>
