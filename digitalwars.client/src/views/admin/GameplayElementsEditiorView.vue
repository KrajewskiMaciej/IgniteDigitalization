<template>
  <div>
    <!-- Krok 1: wybór szkolenia -->
    <div v-if="!selectedDeckId" class="flex justify-center mt-12">
      <div
        class="bg-secondary/80 px-10 py-8 rounded-xl shadow-md border border-surface-700/60 backdrop-blur-sm w-full max-w-md"
      >
        <div class="flex items-center gap-3 mb-6 pb-4 border-b border-surface-700">
          <div class="bg-primary-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faLayerGroup" class="h-6 text-primary-400" />
          </div>
          <h2 class="text-xl font-bold text-white">{{ t('deckSelection') }}</h2>
        </div>

        <label class="block mb-2 text-sm font-semibold text-gray-300">
          {{ t('selectDeck') }}
        </label>
        <Dropdown
          v-model="selectedDeckId"
          :options="decksData"
          optionLabel="title"
          optionValue="id"
          :placeholder="t('selectDeckPlaceholder')"
          :loading="isLoadingDecks"
          class="w-full"
        />

        <div class="mt-6 pt-5 border-t border-surface-700">
          <p class="text-sm text-gray-400 mb-3">{{ t('orImportFromFile') }}</p>
          <input
            type="file"
            accept=".xls,.xlsx"
            ref="fileInput"
            @change="handleFileChange"
            style="display: none"
          />
          <div class="flex gap-2">
            <Button
              @click="triggerFileInput"
              severity="success"
              class="flex-1"
              :label="t('loadDeckFromExcel')"
            >
              <template #icon>
                <font-awesome-icon :icon="faFileExcel" class="mr-2" />
              </template>
            </Button>
            <Button
              @click="handleDownloadTemplate"
              severity="secondary"
              outlined
              :label="t('downloadCardTemplate')"
            >
              <template #icon>
                <font-awesome-icon :icon="faDownload" class="mr-2" />
              </template>
            </Button>
          </div>
        </div>
      </div>
    </div>

    <!-- Krok 2: zakładki edycji -->
    <template v-else>
      <div class="w-full flex justify-center mt-6">
        <div
          class="flex flex-wrap gap-4 bg-secondary/80 px-8 py-4 rounded-xl shadow-md border border-surface-700/60 backdrop-blur-sm"
        >
          <!-- Wybrane szkolenie + zmień -->
          <div class="flex items-center gap-3 pr-4 border-r border-surface-700">
            <span class="text-sm text-gray-300">{{ selectedDeckTitle }}</span>
            <Button
              :label="t('changeTraining')"
              size="small"
              severity="secondary"
              outlined
              @click="selectedDeckId = undefined"
            >
              <template #icon>
                <font-awesome-icon :icon="faArrowLeft" class="mr-1" />
              </template>
            </Button>
          </div>

          <Button
            :label="t('decisionCards')"
            @click="currentView = 'decisions'"
            outlined
            size="large"
            :severity="currentView === 'decisions' ? undefined : 'secondary'"
          >
            <template #icon>
              <font-awesome-icon :icon="faClone" class="mr-2" />
            </template>
          </Button>

          <Button
            :label="t('items')"
            @click="currentView = 'items'"
            outlined
            size="large"
            :severity="currentView === 'items' ? undefined : 'secondary'"
          >
            <template #icon>
              <font-awesome-icon :icon="faMicrochip" class="mr-2" />
            </template>
          </Button>

          <Button
            :label="t('processes')"
            @click="currentView = 'processes'"
            outlined
            size="large"
            :severity="currentView === 'processes' ? undefined : 'secondary'"
          >
            <template #icon>
              <font-awesome-icon :icon="faChessPawn" class="mr-2" />
            </template>
          </Button>

          <Button
            :label="t('enablers')"
            @click="currentView = 'enablers'"
            outlined
            size="large"
            :severity="currentView === 'enablers' ? undefined : 'secondary'"
          >
            <template #icon>
              <font-awesome-icon :icon="faLock" class="mr-2" />
            </template>
          </Button>

          <Button
            :label="t('economySettings')"
            @click="currentView = 'economy'"
            outlined
            size="large"
            :severity="currentView === 'economy' ? undefined : 'secondary'"
          >
            <template #icon>
              <font-awesome-icon :icon="faCoins" class="mr-2" />
            </template>
          </Button>
        </div>
      </div>

      <div class="mt-6">
        <EditDecisionCards v-model="selectedDeckId" v-if="currentView === 'decisions'" />
        <EditItems v-model="selectedDeckId" v-else-if="currentView === 'items'" />
        <EditProccesses v-model="selectedDeckId" v-else-if="currentView === 'processes'" />
        <DynamicCheatSheetEdit v-model="selectedDeckId" v-else-if="currentView === 'enablers'" />
        <DeckEconomySettings v-model="selectedDeckId" v-else-if="currentView === 'economy'" />
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import EditDecisionCards from '@/components/game/EditDecisionCards.vue'
import EditItems from '@/components/game/EditItems.vue'
import EditProccesses from '@/components/game/EditProccesses.vue'
import DeckEconomySettings from '@/components/game/DeckEconomySettings.vue'
import Button from 'primevue/button'
import Dropdown from 'primevue/dropdown'
import {
  faMicrochip,
  faChessPawn,
  faClone,
  faLock,
  faCoins,
  faLayerGroup,
  faArrowLeft,
  faFileExcel,
  faDownload,
} from '@fortawesome/free-solid-svg-icons'
import DynamicCheatSheetEdit from '@/components/cheatSheet/DynamicCheatSheetEdit.vue'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vue-toastification'
import apiService from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'

const { t } = useI18n()
const toast = useToast()

interface Deck {
  id: number
  title: string
}

const currentView = ref<'items' | 'decisions' | 'processes' | 'enablers' | 'economy'>('decisions')
const selectedDeckId = ref<number | undefined>(undefined)
const decksData = ref<Deck[]>([])
const isLoadingDecks = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)

const selectedDeckTitle = computed(
  () => decksData.value.find((d) => d.id === selectedDeckId.value)?.title ?? '',
)

async function fetchDecks() {
  isLoadingDecks.value = true
  try {
    const response = await apiService.get(apiConfig.admin.deck.getAll)
    decksData.value = response.data as Deck[]
  } finally {
    isLoadingDecks.value = false
  }
}

function triggerFileInput(): void {
  fileInput.value?.click()
}

async function handleFileChange(event: Event): Promise<void> {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  const formData = new FormData()
  formData.append('file', file)

  try {
    await apiService.post(apiConfig.admin.deck.upload, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      withCredentials: true,
    })
    toast.success(t('fileSuccessfullyUploadedAndDeckCreated'))
    await fetchDecks()
  } catch (error: any) {
    toast.error(t('errorUploadingDeckFile') + error.message)
  }
}

const handleDownloadTemplate = async () => {
  try {
    const response = await apiService.getFile(apiConfig.admin.deck.getCardsTemplate)
    const file = response.data
    const url = window.URL.createObjectURL(file)
    const link = document.createElement('a')
    link.href = url
    link.download = 'DigitalWars_SzablonKart.xlsx'
    link.click()
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('Błąd przy pobieraniu szablonu kart:', error)
    toast.error(t('errorDownloadingCardsTemplate') + error)
  }
}

onMounted(fetchDecks)
</script>
