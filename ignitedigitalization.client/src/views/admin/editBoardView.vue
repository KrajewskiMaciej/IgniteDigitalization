<template>
  <div class="w-full">
    <div class="grid grid-cols-1 md:grid-cols-[55fr_45fr]">
      <div
        class="order-2 md:order-1 flex flex-col justify-start border-2 border-surface-700 py-6 px-4 m-4 rounded-lg text-white bg-tertiary"
      >
        <div class="flex flex-row w-full items-center justify-center gap-5 flex-shrink-0">
          <Button
            :class="{ 'border-primary-400': activeView === 'add' }"
            @click="activeView = 'add'"
            outlined
            :severity="activeView === 'add' ? undefined : 'secondary'"
            :label="t('addNewBoard')"
            class="w-60"
          >
            <template #icon>
              <font-awesome-icon :icon="faPlus" />
            </template>
          </Button>
          <Button
            :class="{ 'border-primary-400': activeView === 'edit' }"
            @click="activeView = 'edit'"
            :severity="activeView === 'edit' ? undefined : 'secondary'"
            outlined
            :label="t('editBoard')"
            class="w-60"
          >
            <template #icon>
              <font-awesome-icon :icon="faPenToSquare" />
            </template>
          </Button>
        </div>

        <div class="w-full">
          <h1
            class="mt-8 mb-2 font-nasalization text-lg md:text-xl lg-text-2xl xl:test-3xl text-center"
          >
            {{ activeView === 'add' ? t('addNewBoard') : t('editBoard') }}
          </h1>

          <boardSelector
            :boards="boardsForSelector"
            v-model="selectedBoardId"
            :activeView="activeView"
            @deleteBoard="handleDeleteBoard"
          />

          <form class="mt-4 space-y-4">
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
              v-if="!isCartesianPreview"
              v-model:descriptionDown="formData.descriptionDown"
              v-model:descriptionLeft="formData.descriptionLeft"
              @blur="validateDescriptions"
            />

            <div class="mt-3 md:mt-5 px-4">
              <label class="block mb-3 text-sm font-medium text-white">
                {{ t('quadrantNames') }}
                <span class="text-xs text-surface-500 ml-2">{{ t('quadrantNamesHint') }}</span>
              </label>
              <div class="border-2 border-surface-700 px-3 py-3 rounded-lg mb-2 bg-secondary">
                <div class="flex flex-col gap-2">
                  <div
                    v-for="(name, index) in quadrantNamesArray"
                    :key="index"
                    class="flex items-center border-2 border-surface-600 rounded-lg p-2 gap-2 bg-secondary hover:border-primary-400 transition-colors duration-200"
                  >
                    <span class="text-xs text-surface-400 w-28 flex-shrink-0">{{ quadrantLabels[index] }}</span>
                    <InputText
                      :modelValue="name"
                      @update:modelValue="(v) => updateQuadrantName(index, v)"
                      class="flex-1"
                      placeholder="..."
                    />
                  </div>
                </div>
              </div>
              <p class="text-xs text-surface-500">
                {{ t('axisLabelsInfo') }}
                <span class="text-surface-400">{{ t('labelsTopRef') }}</span> {{ t('and') }}
                <span class="text-surface-400">{{ t('labelsRightRef') }}</span>.
              </p>
            </div>

            <div class="w-full px-4 flex items-center justify-center">
              <Button
                type="button"
                @click="saveBoard"
                class="mt-5 w-full"
                :label="activeView === 'add' ? t('addBoard') : t('saveChanges')"
              >
              </Button>
            </div>
          </form>
        </div>
      </div>

      <div
        class="order-1 md:order-2 border-2 border-surface-700 py-6 px-8 m-4 rounded-lg text-white bg-tertiary flex flex-col md:sticky md:top-4 self-start md:max-h-[calc(100vh-2rem)]"
      >
        <h2 class="text-xl mb-4 text-center flex-shrink-0">{{ t('gameBoardPreview') }}</h2>

        <div class="relative flex-grow min-h-0">
          <myBoardCartesian v-if="isCartesianPreview" :config="previewConfig" />
          <myBoard v-else :config="previewConfig" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { faPlus, faPenToSquare } from '@fortawesome/free-solid-svg-icons'
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useToast } from 'vue-toastification'
import { useConfirm } from 'primevue/useconfirm'
import Button from 'primevue/button'
import myBoard from '@/components/game/gameBoard.vue'
import myBoardCartesian from '@/components/game/gameBoardCartesian.vue'
import boardSelector from '@/components/editBoard/boardSelector.vue'
import boardInfo from '@/components/editBoard/boardInfo.vue'
import boardColorSettings from '@/components/editBoard/boardColorSettings.vue'
import boardLabelsEditors from '@/components/editBoard/boardLabelsEditors.vue'
import boardDescriptions from '@/components/editBoard/boardDescriptions.vue'
import InputText from 'primevue/inputtext'
import { useI18n } from 'vue-i18n'

import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'

const { t } = useI18n()

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
  cells_Descriptions: string
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
  cellsDescriptions: string
}

// --- ZMIENNE REAKTYWNE ---
const selectedBoardId = ref<number | undefined>(undefined)
const activeView = ref<'add' | 'edit'>('add')
const toast = useToast()
const confirm = useConfirm()

const data = reactive<{ boards: ApiBoard[] }>({
  boards: [],
})

// Funkcja zwracająca domyślny, czysty obiekt BoardConfig
const getDefaultFormData = (): BoardConfig => ({
  boardId: 0,
  name: t('newBoard'),
  labelsUp: [t('label1'), t('label2'), t('label3'), t('label4')],
  labelsRight: [t('labelA'), t('labelB'), t('labelC'), t('labelD')],
  descriptionDown: t('bottomDescription'),
  descriptionLeft: t('leftDescription'),
  rows: 8,
  cols: 8,
  cellColor: '#ffffff',
  borderColor: '#000000',
  borderColors: ['#008000', '#FFFF00', '#FFA500', '#FF0000'],
  cellsDescriptions: '',
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
    toast.info(t('noBoardsToEditAddNewBoard'))
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
      activeView.value = 'add'
    }
  } catch (error: any) {
    toast.error(t('errorFetchigBoards') + error)
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
    toast.error(t('noBoardSelected'))
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
  formData.cellsDescriptions = selectedBoard.cells_Descriptions ?? ''
}

const saveBoard = async () => {
  try {
    if (!formData.name.trim()) {
      toast.error(t('boardNameCaonnotBeEmpty'))
      return
    }
    if (
      formData.labelsUp.some((label) => !label.trim()) ||
      formData.labelsRight.some((label) => !label.trim())
    ) {
      toast.error(t('allLabelsMustBeFilled'))
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
      Cells_Descriptions: formData.cellsDescriptions,
    }

    if (activeView.value === 'add') {
      const response = await apiService.post<ApiBoard>(apiConfig.boards.create, payload)
      await fetchBoardsFromAPI()
      resetForm()
    } else {
      if (!selectedBoardId.value) {
        toast.warning(t('noBoardSelected'))
        return
      }
      const response = await apiService.put<ApiBoard>(
        apiConfig.boards.update(selectedBoardId.value),
        payload,
      )
      await fetchBoardsFromAPI()
    }
  } catch (error: any) {
    console.error('Błąd podczas zapisywania planszy:', error.response?.data || error.message)
    const errorMessage = error.response?.data?.title || error.response?.data || error.message
    toast.error(t('errorSavingBoard'))
  }
}

const handleDeleteBoard = () => {
  if (!selectedBoardId.value) {
    toast.warning(t('noBoardSelected'))
    return
  }

  const boardToDelete = data.boards.find((b) => b.boards_Id === selectedBoardId.value)
  const boardName = boardToDelete ? boardToDelete.name : t('selectedBoard')

  confirm.require({
    header: t('deleteBoard'),
    message: t('deleteBoardConfirmation', { boardName }),
    accept: async () => {
      try {
        await apiService.delete(apiConfig.boards.delete(selectedBoardId.value!))

        await fetchBoardsFromAPI()
        resetForm()
        if (data.boards.length === 0) {
          activeView.value = 'add'
        }
      } catch (error: any) {
        if (error.response?.status === 409) {
          toast.warning(t('errorBoardDeleteConflict'))
          return
        }
        const errorMessage = error.response?.data || error.message || 'Nieznany błąd'
        toast.error(t('errorDeletingBoard') + ': ' + errorMessage)
      }
    },
    reject: () => {},
  })
}

const validateDescriptions = () => {
  if (!formData.descriptionDown?.trim()) {
    formData.descriptionDown = t('defaultDescriptionDown')
    toast.warning(t('descriptionDownEmptyWarning'))
  }
  if (!formData.descriptionLeft?.trim()) {
    formData.descriptionLeft = t('defaultDescriptionLeft')
    toast.warning(t('descriptionLeftEmptyWarning'))
  }
}

// --- NAZWY ĆWIARTEK ---
const quadrantLabels = computed(() => [t('quadrantTopLeft'), t('quadrantTopRight'), t('quadrantBottomLeft'), t('quadrantBottomRight')])

const quadrantNamesArray = computed(() => {
  const parts = formData.cellsDescriptions.split(';').map((s) => s.trim())
  return [parts[0] || '', parts[1] || '', parts[2] || '', parts[3] || '']
})

const updateQuadrantName = (index: number, value: string | undefined) => {
  const arr = [...quadrantNamesArray.value]
  arr[index] = value?.trim() ?? ''
  formData.cellsDescriptions = arr.every((s) => !s) ? '' : arr.join(';')
}

// --- COMPUTED & LIFECYCLE ---
const previewConfig = computed<BoardConfig>(() => {
  return { ...formData }
})

const isCartesianPreview = computed(() => quadrantNamesArray.value.some((n) => n !== ''))

onMounted(fetchBoardsFromAPI)
</script>
