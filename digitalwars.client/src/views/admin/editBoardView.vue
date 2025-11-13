<template>
  <div class="w-full">
    <div class="grid grid-cols-1 md:grid-cols-[55fr_45fr] gap-4">
      <div
        class="order-2 md:order-1 flex flex-col justify-start border-2 border-lgray-accent py-6 px-4 m-4 rounded-md text-white bg-tertiary"
      >
        <div class="flex flex-row w-full items-center justify-center gap-5 flex-shrink-0">
          <button
            class="border-2 border-lgray-accent py-2 px-2 rounded-md w-60 text-center hover:border-accent transition-colors duration-300"
            :class="{ 'border-accent': activeView === 'add' }"
            @click="activeView = 'add'"
          >
            <font-awesome-icon :icon="faPlus" class="h-4 text-accent" />
            Dodaj nową planszę
          </button>
          <button
            class="border-2 border-lgray-accent py-2 px-2 rounded-md w-60 text-center hover:border-accent transition-colors duration-300"
            :class="{ 'border-accent': activeView === 'edit' }"
            @click="activeView = 'edit'"
          >
            <font-awesome-icon :icon="faPenToSquare" class="h-4 text-accent" />
            Edytuj planszę
          </button>
        </div>

        <div class="w-full">
          <h1 class="mt-8 font-nasalization text-lg md:text-xl lg-text-2xl xl:test-3xl">
            {{ activeView === 'add' ? 'Dodaj nową planszę' : 'Edytuj planszę' }}
          </h1>

          <boardSelector
            :boards="boardsForSelector"
            v-model="selectedBoardId"
            :activeView="activeView"
            @delete="deleteBoard"
          />

          <form class="mt-4">
            <boardInfo
              v-model:name="formData.name"
              :cols="formData.cols"
              :rows="formData.rows"
              @blur="validateDescriptions"
            />

            <boardColorSettings
              v-model:cellColor="formData.cellColor"
              v-model:borderColor="formData.borderColor"
              v-model:borderColors="formData.borderColors"
            />

            <boardLabelsEditors
              v-model:labelsUp="formData.labelsUp"
              v-model:labelsRight="formData.labelsRight"
            />

            <boardDescriptions
              v-model:descriptionDown="formData.descriptionDown"
              v-model:descriptionLeft="formData.descriptionLeft"
              @blur="validateDescriptions"
            />

            <button
              type="button"
              class="bg-accent border-2 border-accent py-3 px-6 rounded-md mt-5 hover:bg-opacity-80 transition-all"
              @click="saveBoard"
            >
              <font-awesome-icon :icon="faSave" class="h-4 mr-2" />
              {{ activeView === 'add' ? 'Dodaj planszę' : 'Zapisz zmiany' }}
            </button>
          </form>
        </div>
      </div>

      <div
        class="order-1 md:order-2 border-2 border-lgray-accent py-6 px-8 m-4 rounded-md text-white bg-tertiary flex flex-col md:sticky md:top-4 self-start md:max-h-[calc(100vh-2rem)]"
      >
        <h2 class="text-xl mb-4 text-center flex-shrink-0">Podgląd planszy</h2>

        <div class="relative flex-grow min-h-0">
          <myBoard :config="previewConfig" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { faPlus, faPenToSquare, faSave } from '@fortawesome/free-solid-svg-icons'
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useToast } from 'vue-toastification'
import myBoard from '@/components/game/gameBoard.vue'
import boardSelector from '@/components/editBoard/boardSelector.vue'
import boardInfo from '@/components/editBoard/boardInfo.vue'
import boardColorSettings from '@/components/editBoard/boardColorSettings.vue'
import boardLabelsEditors from '@/components/editBoard/boardLabelsEditors.vue'
import boardDescriptions from '@/components/editBoard/boardDescriptions.vue'

import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'

// --- INTERFEJSY ---

// Interfejs reprezentujący obiekt Board zwracany przez API (z konwencją snake_case)
interface ApiBoard {
  boards_Id: number
  name: string
  labels_Up: string
  labels_Right: string
  description_Down: string
  description_Left: string
  rows: number
  cols: number
  cell_Color: string
  border_Color: string
  borders_Colors: string
}

// Ujednolicony interfejs używany wewnątrz komponentu (z konwencją camelCase)
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

// --- ZMIENNE REAKTYWNE ---
const selectedBoardId = ref<number | undefined>(undefined)
const activeView = ref<'add' | 'edit'>('add')
const toast = useToast()

const data = reactive<{ boards: ApiBoard[] }>({
  boards: [],
})

// Funkcja zwracająca domyślny, czysty obiekt BoardConfig
const getDefaultFormData = (): BoardConfig => ({
  boardId: 0,
  name: 'Nowa plansza',
  labelsUp: ['Etykieta 1', 'Etykieta 2', 'Etykieta 3', 'Etykieta 4'],
  labelsRight: ['Etykieta A', 'Etykieta B', 'Etykieta C', 'Etykieta D'],
  descriptionDown: 'Opis dolny',
  descriptionLeft: 'Opis lewy',
  rows: 8,
  cols: 8,
  cellColor: '#ffffff',
  borderColor: '#000000',
  borderColors: ['#008000', '#FFFF00', '#FFA500', '#FF0000'],
})

const formData = reactive<BoardConfig>(getDefaultFormData())

// --- WŁAŚCIWOŚCI OBLICZENIOWE ---
const boardsForSelector = computed(() => {
  return data.boards.map((board) => ({
    boardId: board.boards_Id,
    name: board.name,
  }))
})

// --- WATCHERY ---
watch(
  () => formData.labelsUp,
  (newLabels) => {
    if (Array.isArray(newLabels)) {
      formData.cols = newLabels.length * 2
    }
  },
  { deep: true },
)

watch(
  () => formData.labelsRight,
  (newLabels) => {
    if (Array.isArray(newLabels)) {
      formData.rows = newLabels.length * 2
    }
  },
  { deep: true },
)

watch(selectedBoardId, (id) => {
  if (id) {
    loadSelectedBoard(id)
  } else {
    resetForm()
  }
})

watch(activeView, (newView) => {
  resetForm()
  if (newView === 'edit' && data.boards.length === 0) {
    toast.info('Brak plansz do edycji. Dodaj nową planszę.')
    activeView.value = 'add'
  }
})

// --- FUNKCJE POMOCNICZE ---
const stringToArray = (str: string): string[] => {
  if (typeof str !== 'string' || !str) return []
  return str
    .split(';')
    .map((item) => item.trim())
    .filter((item) => item)
}

const arrayToString = (arr: string[]): string => {
  if (!arr || !Array.isArray(arr)) return ''
  return arr.join(';')
}

// --- LOGIKA BIZNESOWA ---
const fetchBoardsFromAPI = async () => {
  try {
    const response = await apiService.get<ApiBoard[]>(apiConfig.boards.getAll)
    data.boards = response.data
    if (data.boards.length === 0 && activeView.value === 'edit') {
      toast.info('Brak plansz do edycji, przełączam na dodawanie.')
      activeView.value = 'add'
    }
  } catch (error: any) {
    console.error('Błąd pobierania plansz:', error.response?.data || error.message)
    toast.error(`Nie udało się pobrać plansz: ${error.response?.data?.title || error.message}`)
  }
}

const resetForm = () => {
  Object.assign(formData, getDefaultFormData())
  selectedBoardId.value = undefined
}

// Mapowanie danych z ApiBoard na wewnętrzny BoardConfig
const loadSelectedBoard = (boardId: number) => {
  const selectedBoard = data.boards.find((board) => board.boards_Id === boardId)
  if (!selectedBoard) {
    toast.error('Nie znaleziono wybranej planszy.')
    resetForm()
    return
  }

  formData.boardId = selectedBoard.boards_Id
  formData.name = selectedBoard.name
  formData.labelsUp = stringToArray(selectedBoard.labels_Up)
  formData.labelsRight = stringToArray(selectedBoard.labels_Right)
  formData.descriptionDown = selectedBoard.description_Down
  formData.descriptionLeft = selectedBoard.description_Left
  formData.rows = selectedBoard.rows
  formData.cols = selectedBoard.cols
  formData.cellColor = selectedBoard.cell_Color
  formData.borderColor = selectedBoard.border_Color
  formData.borderColors = stringToArray(selectedBoard.borders_Colors)

  toast.success(`Załadowano planszę: ${formData.name}`)
}

const saveBoard = async () => {
  try {
    if (!formData.name.trim()) {
      toast.error('Nazwa planszy jest wymagana!')
      return
    }
    if (
      formData.labelsUp.some((label) => !label.trim()) ||
      formData.labelsRight.some((label) => !label.trim())
    ) {
      toast.error('Wszystkie etykiety muszą być wypełnione!')
      return
    }

    // Mapowanie danych z BoardConfig na format oczekiwany przez API
    const payload = {
      Name: formData.name,
      Labels_Up: arrayToString(formData.labelsUp),
      Labels_Right: arrayToString(formData.labelsRight),
      Description_Down: formData.descriptionDown,
      Description_Left: formData.descriptionLeft,
      Rows: formData.rows,
      Cols: formData.cols,
      Cell_Color: formData.cellColor,
      Border_Color: formData.borderColor,
      Borders_Colors: arrayToString(formData.borderColors),
    }

    if (activeView.value === 'add') {
      const response = await apiService.post<ApiBoard>(apiConfig.boards.create, payload)
      toast.success(`Plansza "${response.data.name}" dodana pomyślnie!`)
      await fetchBoardsFromAPI()
      resetForm()
    } else {
      if (!selectedBoardId.value) {
        toast.warning('Wybierz planszę do edycji!')
        return
      }
      const response = await apiService.put<ApiBoard>(
        apiConfig.boards.update(selectedBoardId.value),
        payload,
      )
      toast.success(`Plansza "${response.data.name}" zaktualizowana pomyślnie!`)
      await fetchBoardsFromAPI()
    }
  } catch (error: any) {
    console.error('Błąd podczas zapisywania planszy:', error.response?.data || error.message)
    const errorMessage = error.response?.data?.title || error.response?.data || error.message
    toast.error(`Błąd zapisu: ${errorMessage}`)
  }
}

const deleteBoard = async () => {
  if (!selectedBoardId.value) {
    toast.warning('Nie wybrano planszy do usunięcia!')
    return
  }

  const boardToDelete = data.boards.find((b) => b.boards_Id === selectedBoardId.value)
  const boardName = boardToDelete ? boardToDelete.name : 'wybrana plansza'

  if (confirm(`Czy na pewno chcesz usunąć planszę "${boardName}"?`)) {
    try {
      await apiService.delete(apiConfig.boards.delete(selectedBoardId.value))
      toast.success('Plansza została usunięta pomyślnie!')

      await fetchBoardsFromAPI()
      resetForm()
      if (data.boards.length === 0) {
        activeView.value = 'add'
      }
    } catch (error: any) {
      console.error('Błąd podczas usuwania planszy:', error.response?.data || error.message)
      const errorMessage = error.response?.data?.title || error.response?.data || error.message
      toast.error(`Błąd usuwania: ${errorMessage}`)
    }
  }
}

const validateDescriptions = () => {
  if (!formData.descriptionDown?.trim()) {
    formData.descriptionDown = 'Opis dolny'
    toast.warning('Opis dolny nie może być pusty. Ustawiono wartość domyślną.')
  }
  if (!formData.descriptionLeft?.trim()) {
    formData.descriptionLeft = 'Opis lewy'
    toast.warning('Opis lewy nie może być pusty. Ustawiono wartość domyślną.')
  }
}

// --- COMPUTED & LIFECYCLE ---
const previewConfig = computed<BoardConfig>(() => {
  // Bezpośrednie użycie formData, ponieważ ma już poprawny kształt
  return { ...formData }
})

onMounted(fetchBoardsFromAPI)
</script>
