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
              :label="t('change')"
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
} from '@fortawesome/free-solid-svg-icons'
import DynamicCheatSheetEdit from '@/components/cheatSheet/DynamicCheatSheetEdit.vue'
import { useI18n } from 'vue-i18n'
import apiService from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'

const { t } = useI18n()

interface Deck {
  id: number
  title: string
}

const currentView = ref<'items' | 'decisions' | 'processes' | 'enablers' | 'economy'>('decisions')
const selectedDeckId = ref<number | undefined>(undefined)
const decksData = ref<Deck[]>([])
const isLoadingDecks = ref(false)

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

onMounted(fetchDecks)
</script>
