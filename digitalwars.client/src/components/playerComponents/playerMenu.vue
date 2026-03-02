<template>
  <div class="h-full w-full max-w-md mx-auto flex flex-col">
    <!-- Góra: bity + etap -->
    <div class="flex justify-between items-center mb-2">
      <div class="text-xl font-bold text-green-600">
        {{ t('bits') }}: {{ currentBudget }}
      </div>
      <div class="text-md font-semibold text-primary-400">{{ phaseDisplayLabel }}</div>
    </div>

    <div
      class="h-[2px] bg-gradient-to-r from-transparent via-primary-500 to-transparent mb-4 mt-4 sm:mb-8"
    ></div>

    <!-- Tabela decyzji -->
    <div class="flex flex-col flex-grow pt-3 overflow-hidden">
      <h2 class="text-lg font-semibold mb-2 text-white">{{ t('decisions') }}</h2>

      <!-- Lista -->
      <div class="overflow-y-auto custom-scrollbar pr-2 flex-grow">
        <div v-if="isLoading" class="text-center text-gray-500">
          {{ t('loadingDecisionHistory') }}
        </div>
        <div v-else-if="error" class="text-center text-red-500">{{ error }}</div>
        <div v-else-if="gameLogEntries.length === 0" class="text-center text-surface-400">
          {{ t('noDecisionHistory') }}
        </div>
        <ul v-else class="space-y-3 text-sm">
          <li v-for="(decision, index) in gameLogEntries" :key="index">
            <div
              v-if="decision.isEventNotification"
              class="border-2 border-blue-400 rounded-lg p-3 bg-blue-900/60 text-center"
            >
              <h4 class="font-bold text-blue-300 text-sm mb-1">{{ t('newEvent') }}</h4>
              <p class="text-white text-xs leading-relaxed">{{ decision.description }}</p>
            </div>

            <div
              v-else
              class="border-2 bg-gray-800 text-white text-left p-3 rounded-lg shadow-md space-y-2 relative"
              :class="
                decision.eventApplied ? 'border-purple-400 bg-purple-900/20' : 'border-gray-600'
              "
            >
              <div
                v-if="decision.eventApplied"
                class="absolute -top-2 -right-2 px-3 py-1 bg-purple-600 text-white text-xs font-bold rounded-full shadow-lg"
              >
                EVENT
              </div>

              <div class="font-semibold text-sm">
                <span class="text-surface-400">{{ t('cardId') }} {{ decision.cardId }}</span>
                <span class="mx-1">→</span>
                <span class="text-white">{{ decision.choice }}</span>
              </div>

              <div class="border-t border-gray-600 pt-2">
                <div class="flex items-center gap-2 mb-1">
                  <span class="text-surface-400 text-xs">{{ t('result') }}:</span>
                  <span
                    class="font-bold text-sm"
                    :class="{
                      'text-green-400': decision.result === 'Pozytywny',
                      'text-red-400': decision.result === 'Negatywny',
                    }"
                  >
                    {{ decision.result }}
                  </span>
                </div>
                <p class="text-xs text-gray-300 leading-relaxed">
                  {{ decision.description }}
                </p>
                <p v-if="decision.hint" class="text-xs text-gray-300 leading-relaxed">
                  {{ t('hint') }}: {{ decision.hint }}
                </p>
              </div>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import apiConfig from '@/services/apiConfig'
import apiServices from '@/services/apiServices'
import { ref, watch, onMounted, onUnmounted, computed } from 'vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

// --- DEFINICJE INTERFEJSÓW ---
// Interfejs dla danych przychodzących z API
interface ApiLogEntry {
  isEventNotification: boolean
  eventDescription?: string
  cardTitle?: string
  cardId: number
  status: boolean
  feedbackDescription?: string
  enablerDescription?: string
  cost: number
  gameEventId: number | null
}

// Interfejs dla danych przetworzonych, używanych w szablonie
interface ProcessedLogEntry {
  isEventNotification: boolean
  description: string
  hint?: string
  choice: string
  cardId: number
  result: 'Pozytywny' | 'Negatywny'
  eventApplied: boolean
}

const gameLogEntries = ref<ProcessedLogEntry[]>([])
const currentBudget = ref(0)
const isLoading = ref(true)
const error = ref<string | null>(null)

const props = defineProps({
  gameId: Number,
  teamId: Number,
  currentPhaseName: String,
})

const phaseDisplayLabel = computed(() => {
  if (props.currentPhaseName === 'Rynkowa') return `${t('stage')}: 2 - ${t('phaseMarket')}`
  return `${t('stage')}: 1 - ${t('phasePrep')}`
})

const emit = defineEmits(['budget-changed-in-menu'])

const hasRequiredIds = computed(() => props.gameId != null && props.teamId != null)

async function fetchData() {
  if (!hasRequiredIds.value) return

  isLoading.value = true
  error.value = null

  try {
    const [historyResponse, budgetResponse] = await Promise.all([
      apiServices.post<ApiLogEntry[]>(apiConfig.player.getPlayerHistory, {
        gameId: props.gameId,
        teamId: props.teamId,
      }),
      apiServices.get<{ budget: number }>(apiConfig.player.getCurrency, { teamId: props.teamId }),
    ])

    if (Array.isArray(historyResponse.data)) {
      gameLogEntries.value = historyResponse.data.map((log: ApiLogEntry): ProcessedLogEntry => {
        if (log.isEventNotification) {
          return {
            isEventNotification: true,
            description: log.eventDescription || 'Aktywowano nowe wydarzenie.',
            choice: '',
            cardId: 0,
            result: 'Pozytywny',
            eventApplied: false,
          }
        }
        return {
          isEventNotification: false,
          choice: log.cardTitle || `Karta ID: ${log.cardId}`,
          cardId: log.cardId,
          result: log.status ? 'Pozytywny' : 'Negatywny',
          description:
            log.feedbackDescription ||
            (log.cost !== undefined ? `Koszt: ${log.cost}` : 'Brak opisu'),
            hint: log.enablerDescription || undefined,
          eventApplied: log.gameEventId != null,
        }
      })
    }
    currentBudget.value = budgetResponse.data.budget
    emit('budget-changed-in-menu', currentBudget.value)
  } catch (err: any) {
    console.error('Błąd podczas pobierania danych panelu gracza:', err)
    error.value = 'Nie udało się załadować danych.'
  } finally {
    isLoading.value = false
  }
}

const handleFetchBudget = async () => {
  try {
    const budgetResponse = await apiServices.get<{ budget: number }>(apiConfig.player.getCurrency, {
      teamId: props.teamId,
    })
    currentBudget.value = budgetResponse.data.budget
    emit('budget-changed-in-menu', currentBudget.value)
  } catch (err) {
    console.error('Błąd podczas pobierania budżetu:', err)
  }
}

defineExpose({
  fetchGameLog: fetchData,
  fetchTeamBud: fetchData,
  handleFetchBudget,
})

watch(
  hasRequiredIds,
  (hasIds) => {
    if (hasIds) {
      fetchData()
    }
  },
  { immediate: true },
)
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
