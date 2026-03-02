<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <!-- Nagłówek -->
    <div class="text-center">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-white mb-2">
        {{ t('economySettings') }}
      </h1>
      <p class="text-surface-400 text-sm md:text-base">
        {{ t('trainings') }} – {{ t('economySettings') }}
      </p>
    </div>

    <div class="max-w-4xl mx-auto w-full space-y-6">
      <!-- Import zasad z pliku -->
      <div class="text-center">
        <input
          type="file"
          accept=".xls,.xlsx"
          ref="fileInput"
          @change="handleFileImport"
          style="display: none"
        />
        <Button
          @click="() => fileInput?.click()"
          severity="success"
          size="large"
          :label="t('importEconomySettings')"
          :loading="isImporting"
        >
          <template #icon>
            <font-awesome-icon :icon="faFileExcel" class="mr-2" />
          </template>
        </Button>
      </div>

      <!-- Wybór szkolenia -->
      <div class="border border-surface-700 rounded-xl p-6 bg-secondary shadow-2xl">
        <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
          <div class="bg-primary-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faLayerGroup" class="h-6 text-primary-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-white">{{ t('training') }}</h2>
        </div>
        <Dropdown
          v-model="selectedDeckId"
          :options="decksData"
          optionLabel="title"
          optionValue="id"
          showClear
          :placeholder="t('selectTrainingPlaceholder')"
          class="w-full"
          :disabled="isLoadingDecks"
        />
      </div>

      <!-- Sekcja zasad ekonomii -->
      <div v-if="selectedDeckId && settings" class="space-y-6">
        <!-- Mapa 1 -->
        <div class="border border-surface-700 rounded-xl p-6 bg-secondary shadow-2xl">
          <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
            <div class="bg-blue-500/20 p-3 rounded-lg">
              <font-awesome-icon :icon="faCoins" class="h-6 text-blue-400" />
            </div>
            <h2 class="text-xl font-bold text-white">{{ t('map1Budget') }}</h2>
          </div>
          <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
            <div>
              <label class="block text-sm font-semibold text-gray-300 mb-1">
                Budżet startowy (BITS)
              </label>
              <InputNumber v-model="settings.map1_Starting_Budget" :min="0" :max="10000" showButtons class="w-full" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-gray-300 mb-1">
                Koszty obowiązkowe (BITS)
              </label>
              <InputNumber v-model="settings.map1_Mandatory_Cards_Cost" :min="0" :max="10000" showButtons class="w-full" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-gray-300 mb-1">
                Min. kart (cel)
              </label>
              <InputNumber v-model="settings.map1_Target_Cards_Min" :min="0" :max="100" showButtons class="w-full" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-gray-300 mb-1">
                Max. kart (cel)
              </label>
              <InputNumber v-model="settings.map1_Target_Cards_Max" :min="0" :max="100" showButtons class="w-full" />
            </div>
          </div>
        </div>

        <!-- Mapa 2 -->
        <div class="border border-surface-700 rounded-xl p-6 bg-secondary shadow-2xl">
          <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
            <div class="bg-green-500/20 p-3 rounded-lg">
              <font-awesome-icon :icon="faChartLine" class="h-6 text-green-400" />
            </div>
            <h2 class="text-xl font-bold text-white">{{ t('map2Budget') }}</h2>
          </div>
          <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
            <div>
              <label class="block text-sm font-semibold text-gray-300 mb-1">
                Budżet bazowy (BITS)
              </label>
              <InputNumber v-model="settings.map2_Base_Budget" :min="0" :max="10000" showButtons class="w-full" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-gray-300 mb-1">
                Maks. bonus PRE (BITS)
              </label>
              <InputNumber v-model="settings.map2_Prep_Bonus_Max_Bits" :min="0" :max="10000" showButtons class="w-full" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-gray-300 mb-1">
                Łączna liczba kart PRE
              </label>
              <InputNumber v-model="settings.map2_Prep_Cards_Total_Count" :min="1" :max="1000" showButtons class="w-full" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-gray-300 mb-1">
                Min. kart (cel)
              </label>
              <InputNumber v-model="settings.map2_Target_Cards_Min" :min="0" :max="100" showButtons class="w-full" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-gray-300 mb-1">
                Max. kart (cel)
              </label>
              <InputNumber v-model="settings.map2_Target_Cards_Max" :min="0" :max="100" showButtons class="w-full" />
            </div>
          </div>
        </div>

        <!-- Mnożnik przygotowania -->
        <div class="border border-surface-700 rounded-xl p-6 bg-secondary shadow-2xl">
          <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
            <div class="bg-yellow-500/20 p-3 rounded-lg">
              <font-awesome-icon :icon="faSliders" class="h-6 text-yellow-400" />
            </div>
            <h2 class="text-xl font-bold text-white">{{ t('prepMultiplier') }}</h2>
          </div>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-semibold text-gray-300 mb-1">
                Maksymalny mnożnik
              </label>
              <InputNumber
                v-model="settings.prepMultiplier_Max"
                :min="1"
                :max="10"
                :step="0.1"
                :minFractionDigits="1"
                :maxFractionDigits="2"
                showButtons
                class="w-full"
              />
            </div>
          </div>
          <p class="text-xs text-surface-400 mt-3">
            Formuła: MIN(Maks. mnożnik; 1 + zagrane_PRE / łącznych_PRE)
          </p>
        </div>

        <!-- Domyślne plansze -->
        <div class="border border-surface-700 rounded-xl p-6 bg-secondary shadow-2xl">
          <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
            <div class="bg-purple-500/20 p-3 rounded-lg">
              <font-awesome-icon :icon="faMap" class="h-6 text-purple-400" />
            </div>
            <h2 class="text-xl font-bold text-white">{{ t('defaultBoards') }}</h2>
          </div>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-semibold text-gray-300 mb-1">
                {{ t('defaultTeamsBoard') }} <span class="text-surface-500">(Mapa 1)</span>
              </label>
              <Dropdown
                v-model="selectedTeamBoardId"
                :options="boardsData"
                optionLabel="name"
                optionValue="boards_Id"
                showClear
                :placeholder="t('selectTeamBoardPlaceholder')"
                class="w-full"
              />
            </div>
            <div>
              <label class="block text-sm font-semibold text-gray-300 mb-1">
                {{ t('defaultRivalsBoard') }} <span class="text-surface-500">(Mapa 2)</span>
              </label>
              <Dropdown
                v-model="selectedRivalBoardId"
                :options="boardsData"
                optionLabel="name"
                optionValue="boards_Id"
                showClear
                :placeholder="t('selectRivalBoardPlaceholder')"
                class="w-full"
              />
            </div>
          </div>
        </div>

        <!-- Zapisz -->
        <div class="flex justify-end">
          <Button
            @click="saveSettings"
            :label="t('saveEconomySettings')"
            :loading="isSaving"
            size="large"
          >
            <template #icon>
              <font-awesome-icon :icon="faSave" class="mr-2" />
            </template>
          </Button>
        </div>
      </div>

      <!-- Brak szkolenia -->
      <div v-else-if="!selectedDeckId" class="text-center text-surface-400 py-10">
        <font-awesome-icon :icon="faLayerGroup" class="h-10 mb-3 text-surface-600" />
        <p>{{ t('selectTrainingFirst') }}</p>
      </div>

      <!-- Ładowanie -->
      <div v-else-if="isLoadingSettings" class="text-center text-surface-400 py-10">
        <p>Ładowanie zasad ekonomii...</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useToast } from 'vue-toastification'
import InputNumber from 'primevue/inputnumber'
import Dropdown from 'primevue/dropdown'
import Button from 'primevue/button'
import {
  faLayerGroup,
  faCoins,
  faChartLine,
  faSliders,
  faSave,
  faFileExcel,
  faMap,
} from '@fortawesome/free-solid-svg-icons'
import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const toast = useToast()

interface DeckListItem {
  id: number
  title: string
  defaultTeamsBoardId?: number | null
  defaultRivalsBoardId?: number | null
}
interface Board {
  boards_Id: number
  name: string
}
interface EconomySettings {
  map1_Starting_Budget: number
  map1_Mandatory_Cards_Cost: number
  map1_Target_Cards_Min: number
  map1_Target_Cards_Max: number
  map2_Base_Budget: number
  map2_Prep_Bonus_Max_Bits: number
  map2_Prep_Cards_Total_Count: number
  map2_Target_Cards_Min: number
  map2_Target_Cards_Max: number
  prepMultiplier_Max: number
}

const selectedDeckId = ref<number | undefined>(undefined)
const decksData = ref<DeckListItem[]>([])
const boardsData = ref<Board[]>([])
const selectedTeamBoardId = ref<number | null>(null)
const selectedRivalBoardId = ref<number | null>(null)
const settings = ref<EconomySettings | null>(null)
const isLoadingDecks = ref(false)
const isLoadingSettings = ref(false)
const isSaving = ref(false)
const isImporting = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)

const fetchDecks = async () => {
  isLoadingDecks.value = true
  try {
    const response = await apiService.get<Array<{ id: number; title: string; defaultTeamsBoardId?: number | null; defaultRivalsBoardId?: number | null }>>(
      apiConfig.admin.deck.getAll,
    )
    decksData.value = response.data.map((d) => ({
      id: d.id,
      title: d.title,
      defaultTeamsBoardId: d.defaultTeamsBoardId,
      defaultRivalsBoardId: d.defaultRivalsBoardId,
    }))
  } catch {
    toast.error(t('errorFetchingDecks'))
  } finally {
    isLoadingDecks.value = false
  }
}

const fetchBoards = async () => {
  try {
    const response = await apiService.get<Board[]>(apiConfig.boards.getAll)
    boardsData.value = response.data
  } catch {
    // brak plansz – nie blokujemy
  }
}

const fetchSettings = async (deckId: number) => {
  isLoadingSettings.value = true
  settings.value = null
  try {
    const response = await apiService.get<EconomySettings>(
      apiConfig.admin.deck.getEconomy(deckId),
    )
    settings.value = response.data
  } catch {
    toast.error(t('errorSavingEconomySettings'))
  } finally {
    isLoadingSettings.value = false
  }
}

const saveSettings = async () => {
  if (!selectedDeckId.value || !settings.value) return
  isSaving.value = true
  try {
    const deck = decksData.value.find((d) => d.id === selectedDeckId.value)
    await Promise.all([
      apiService.put(apiConfig.admin.deck.updateEconomy(selectedDeckId.value), settings.value),
      apiService.put(apiConfig.admin.deck.updateDeckName, {
        Decks_Id: selectedDeckId.value,
        Decks_Name: deck?.title ?? '',
        Default_Teams_Boards_Id: selectedTeamBoardId.value,
        Default_Rivals_Boards_Id: selectedRivalBoardId.value,
      }),
    ])
    // Odśwież dane szkolenia w liście (zaktualizowane plansze)
    const idx = decksData.value.findIndex((d) => d.id === selectedDeckId.value)
    if (idx !== -1) {
      decksData.value[idx].defaultTeamsBoardId = selectedTeamBoardId.value
      decksData.value[idx].defaultRivalsBoardId = selectedRivalBoardId.value
    }
    toast.success(t('economySettingsSaved'))
  } catch {
    toast.error(t('errorSavingEconomySettings'))
  } finally {
    isSaving.value = false
  }
}

const handleFileImport = async (event: Event) => {
  const file = (event.target as HTMLInputElement).files?.[0]
  if (!file || !selectedDeckId.value) {
    toast.warning(t('selectTrainingFirst'))
    return
  }
  isImporting.value = true
  const formData = new FormData()
  formData.append('file', file)
  try {
    await apiService.post(apiConfig.admin.deck.importEconomy(selectedDeckId.value), formData)
    toast.success(t('economySettingsImported'))
    await fetchSettings(selectedDeckId.value)
  } catch {
    toast.error(t('errorImportingEconomySettings'))
  } finally {
    isImporting.value = false
    if (fileInput.value) fileInput.value.value = ''
  }
}

watch(selectedDeckId, (newId) => {
  if (newId) {
    fetchSettings(newId)
    // Ustaw plansze z danych szkolenia
    const deck = decksData.value.find((d) => d.id === newId)
    selectedTeamBoardId.value = deck?.defaultTeamsBoardId ?? null
    selectedRivalBoardId.value = deck?.defaultRivalsBoardId ?? null
  } else {
    settings.value = null
    selectedTeamBoardId.value = null
    selectedRivalBoardId.value = null
  }
})

fetchDecks()
fetchBoards()
</script>
