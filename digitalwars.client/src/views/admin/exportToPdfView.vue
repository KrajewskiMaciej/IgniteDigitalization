<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <div class="text-center">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-white mb-2">
        {{ t('gameExportToPdf') }}
      </h1>
      <p class="text-surface-400 text-sm md:text-base">{{ t('generatePdfDescription') }}</p>
    </div>

    <div class="max-w-6xl mx-auto w-full space-y-6">
      <div class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl">
        <div class="flex items-center gap-3 mb-6 pb-4 border-b border-surface-700">
          <div class="bg-primary-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faFileExport" class="h-6 text-primary-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-white">{{ t('cardExport') }}</h2>
        </div>

        <div class="space-y-4">
          <div>
            <label for="deck-select" class="block mb-2 text-sm font-semibold text-gray-300">
              {{ t('selectDeck') }}
            </label>
            <Dropdown
              id="deck-select"
              v-model="selectedDeck"
              :options="decksData"
              optionLabel="title"
              optionValue="id"
              :placeholder="t('selectDeckPlaceholder')"
              class="w-full"
              :disabled="isLoading"
            >
            </Dropdown>
          </div>

          <div class="flex justify-center pt-2">
            <Button
              @click="exportDeckToPDF"
              :disabled="!isFormDeckValid || isLoading"
              :loading="isLoading"
              class="w-full sm:w-auto"
              size="large"
            >
              <template #icon>
                <font-awesome-icon :icon="faFileExport" class="h-4" />
              </template>
              <span class="ml-2">{{
                isLoading ? t('generating') : t('generateCardPdf')
              }}</span>
            </Button>
          </div>
        </div>
      </div>

      <div class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl">
        <div class="flex items-center gap-3 mb-6 pb-4 border-b border-surface-700">
          <div class="bg-green-400/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faFileExport" class="h-6 text-green-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-white">{{ t('boardExport') }}</h2>
        </div>

        <div class="space-y-4">
          <div>
            <label for="board-select" class="block mb-2 text-sm font-semibold text-gray-300">
              {{ t('tableBoard') }}
            </label>
            <Dropdown
              id="board-select"
              v-model="selectedBoard"
              :options="boardsData"
              optionLabel="name"
              optionValue="boards_Id"
              :placeholder="t('selectTableBoardPlaceholder')"
              class="w-full"
              :disabled="isLoading"
            >
            </Dropdown>
          </div>

          <div>
            <label
              for="opponent-board-select"
              class="block mb-2 text-sm font-semibold text-gray-300"
            >
              {{ t('opponentBoard') }}
            </label>
            <Dropdown
              id="opponent-board-select"
              v-model="selectedOpponentBoard"
              :options="boardsData"
              optionLabel="name"
              optionValue="boards_Id"
              :placeholder="t('selectOpponentBoardPlaceholder')"
              class="w-full"
              :disabled="isLoading"
            >
            </Dropdown>
          </div>

          <div
            v-if="selectedBoard && selectedOpponentBoard"
            class="bg-surface-800 border border-surface-600 rounded-lg p-4"
          >
            <div class="flex items-start gap-3">
              <div class="flex-1">
                <div class="flex items-center gap-2 mb-1">
                  <div class="bg-green-400/20 p-1 rounded">
                    <font-awesome-icon :icon="faChessBoard" class="text-green-400" />
                  </div>
                  <div>
                    <p class="text-sm text-gray-300 mb-2">{{ t('selectedBoards') }}</p>
                  </div>
                </div>
                <ul class="space-y-1 text-sm">
                  <li class="flex items-center gap-2 text-white">
                    <span class="w-2 h-2 bg-green-400 rounded-full"></span>
                    <span class="font-medium">{{ t('tableLabel') }}</span>
                    <span>{{
                      boardsData.find((b) => b.boards_Id === selectedBoard)?.name
                    }}</span>
                  </li>
                  <li class="flex items-center gap-2 text-white">
                    <span class="w-2 h-2 bg-green-400 rounded-full"></span>
                    <span class="font-medium">{{ t('opponentLabel') }}</span>
                    <span>{{
                      boardsData.find((b) => b.boards_Id === selectedOpponentBoard)?.name
                    }}</span>
                  </li>
                </ul>
              </div>
            </div>
          </div>

          <div class="flex justify-center pt-2">
            <Button
              @click="exportBoardsToPDF"
              :disabled="!isFormBoardsValid || isLoading"
              :loading="isLoading"
              severity="success"
              class="w-full sm:w-auto"
              size="large"
            >
              <template #icon>
                <font-awesome-icon :icon="faFileExport" class="h-4" />
              </template>
              <span class="ml-2">{{
                isLoading ? t('generating') : t('generateBoardPdf')
              }}</span>
            </Button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useToast } from 'vue-toastification'
import { faFileExport, faChessBoard } from '@fortawesome/free-solid-svg-icons'
import Dropdown from 'primevue/dropdown'
import Button from 'primevue/button'
import { useI18n } from 'vue-i18n'
import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'

const { t } = useI18n()

interface Deck {
  id: number
  title: string
}

interface Board {
  boards_Id: number
  name: string
}

const toast = useToast()

const decksData = ref<Deck[]>([])
const selectedDeck = ref<number | undefined>()

const boardsData = ref<Board[]>([])
const selectedBoard = ref<number | null>(null)
const selectedOpponentBoard = ref<number | null>(null)

const isLoading = ref(false)

const isFormBoardsValid = computed(
  () => typeof selectedBoard.value === 'number' && typeof selectedOpponentBoard.value === 'number',
)
const isFormDeckValid = computed(() => typeof selectedDeck.value === 'number')

const downloadFileFromResponse = (response: any, defaultFileName: string) => {
  const header = response.headers['content-disposition']
  let fileName = defaultFileName
  if (header) {
    let match = header.match(/filename\*=UTF-8''([^;]+)/) || header.match(/filename="?([^"]+)"?/)
    if (match && match[1]) {
      fileName = decodeURIComponent(match[1])
    }
  }
  const blob = new Blob([response.data], { type: 'application/pdf' })
  const url = window.URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = fileName
  document.body.appendChild(a)
  a.click()
  window.URL.revokeObjectURL(url)
  document.body.removeChild(a)
}

const exportDeckToPDF = async () => {
  if (!isFormDeckValid.value) {
    toast.warning(t('selectDeck'))
    return
  }
  isLoading.value = true
  try {
    const url = apiConfig.admin.export.cards(selectedDeck.value!)
    const response = await apiService.getFile(url)
    downloadFileFromResponse(response, 'DigitalWars - Karty.pdf')
  } catch (error: any) {
    toast.error(t('errorGeneratingCardPdf') + error)
    console.error('Błąd generowania PDF z kartami:', error.response?.data || error.message)
  } finally {
    isLoading.value = false
  }
}

const exportBoardsToPDF = async () => {
  if (!isFormBoardsValid.value) {
    toast.error(t('selectBothBoards'))
    return
  }

  isLoading.value = true
  try {
    const url = apiConfig.admin.export.boards(selectedBoard.value!, selectedOpponentBoard.value!)
    const response = await apiService.getFile(url)
    downloadFileFromResponse(response, 'DigitalWars - Plansze.pdf')
  } catch (error: any) {
    toast.error(t('errorGeneratingBoardPdf') + error)
    console.error('Błąd generowania PDF z planszami:', error.response?.data || error.message)
  } finally {
    isLoading.value = false
  }
}

const fetchBoardsFromAPI = async () => {
  try {
    const response = await apiService.get(apiConfig.boards.getAll)
    boardsData.value = response.data as Board[]
  } catch (error: any) {
    toast.error(t('errorFetchigBoards') + error)
  }
}

const fetchDecksFromAPI = async () => {
  try {
    const response = await apiService.get(apiConfig.admin.deck.getAll)
    decksData.value = response.data as Deck[]
  } catch (error: any) {
    toast.error(t('errorFetchingDecks') + error)
  }
}

onMounted(async () => {
  await Promise.all([fetchDecksFromAPI(), fetchBoardsFromAPI()])
})
</script>
