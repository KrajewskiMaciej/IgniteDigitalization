<template>
  <div class="h-full w-full max-w-md mx-auto bg-yellow rounded-xl shadow p-4 flex flex-col">
    <!-- Góra: bity + etap -->
    <div class="flex justify-between items-center mb-2">
      <div class="text-xl font-bold text-green-600">Bity: {{ currentBudget }}</div>
      <div class="text-md font-semibold text-gray-700">Etap:</div>
    </div>

    <!-- Tabela decyzji -->
    <div class="flex flex-col flex-grow border-t pt-3 overflow-hidden">
      <h2 class="text-lg font-semibold mb-2 text-white">Decyzje</h2>

      <!-- Lista -->
      <div class="overflow-y-auto pr-2 flex-grow">
        <div v-if="isLoading" class="text-center text-gray-500">Ładowanie historii...</div>
        <div v-else-if="error" class="text-center text-red-500">{{ error }}</div>
        <div v-else-if="gameLogEntries.length === 0" class="text-center text-gray-400">Brak historii decyzji.</div>
        <ul v-else class="space-y-2 text-sm">
          <li v-for="(decision, index) in gameLogEntries" :key="index">
            <!-- Widok dla powiadomienia o evencie -->
            <div v-if="decision.isEventNotification" class="border border-blue-500 rounded p-2 bg-blue-900/50 text-center text-xs">
              <h4 class="font-bold text-blue-300">Nowe Wydarzenie</h4>
              <p class="text-white mt-1">{{ decision.description }}</p>
            </div>
          
            <!-- Widok dla normalnego logu zagrania karty -->
            <div v-else
              class="border bg-primary text-white text-left p-2 rounded shadow-sm space-y-1 text-xs leading-tight relative"
              :class="{ 'border-purple-500': decision.eventApplied }"
            >
              <div v-if="decision.eventApplied" class="absolute top-1 right-1 px-2 py-0.5 bg-purple-600 text-white text-xs font-bold rounded-full">
                EVENT
              </div>
            
              <div><strong>Karta {{ decision.cardId }}</strong> → {{ decision.choice }}</div>
              <div class="border-t border-gray-500 w-full my-1"></div>
              <div>
                Wynik:
                <span
                  class="font-semibold"
                  :class="{
                    'text-green-400': decision.result === 'Pozytywny',
                    'text-red-400': decision.result === 'Negatywny'
                  }"
                >
                  {{ decision.result }}
                </span>
              </div>
              <p class="text-xs text-white">
                {{ decision.description }}
              </p>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>
  
<script setup lang="ts">
import apiConfig from '@/services/apiConfig';
import apiServices from '@/services/apiServices';
import { ref, watch, onMounted, onUnmounted, computed } from 'vue';
import signalrService from '@/services/signalService';

// --- DEFINICJE INTERFEJSÓW ---
// Interfejs dla danych przychodzących z API
interface ApiLogEntry {
  isEventNotification: boolean;
  eventDescription?: string;
  cardTitle?: string;
  cardId: number;
  status: boolean;
  feedbackDescription?: string;
  cost: number;
  gameEventId: number | null;
}

// Interfejs dla danych przetworzonych, używanych w szablonie
interface ProcessedLogEntry {
  isEventNotification: boolean;
  description: string;
  choice: string;
  cardId: number;
  result: 'Pozytywny' | 'Negatywny';
  eventApplied: boolean;
}

// --- Reaktywne referencje i Props ---
// POPRAWKA: Jawne typowanie tablicy `gameLogEntries`
const gameLogEntries = ref<ProcessedLogEntry[]>([]);
const currentBudget = ref(0);
const isLoading = ref(true);
// POPRAWKA: Zmieniono typ `error`, aby akceptował string lub null
const error = ref<string | null>(null);

const props = defineProps({
  gameId: Number,
  teamId: Number
});

const emit = defineEmits(['budget-changed-in-menu']);

// --- Computed Properties ---
const hasRequiredIds = computed(() => props.gameId != null && props.teamId != null);

// --- Metody pobierania danych ---
async function fetchData() {
  if (!hasRequiredIds.value) return;

  isLoading.value = true;
  error.value = null;

  try {
    // POPRAWKA: Dodano typy generyczne do wywołań API
    const [historyResponse, budgetResponse] = await Promise.all([
      apiServices.post<ApiLogEntry[]>(apiConfig.player.getPlayerHistory, { gameId: props.gameId, teamId: props.teamId }),
      apiServices.get<{ teamBud: number }>(apiConfig.player.getCurrency, { teamId: props.teamId } )
    ]);

    if (Array.isArray(historyResponse.data)) {
      // POPRAWKA: Dodano typ do parametru `log`
      gameLogEntries.value = historyResponse.data.map((log: ApiLogEntry): ProcessedLogEntry => {
        if (log.isEventNotification) {
          return {
            isEventNotification: true,
            description: log.eventDescription || "Aktywowano nowe wydarzenie.",
            // Wypełniamy pozostałe pola, aby pasowały do interfejsu
            choice: '',
            cardId: 0, 
            result: 'Pozytywny', 
            eventApplied: false
          };
        }
        return {
          isEventNotification: false,
          choice: log.cardTitle || `Karta ID: ${log.cardId}`,
          cardId: log.cardId,
          result: log.status ? 'Pozytywny' : 'Negatywny',
          description: log.feedbackDescription || `Koszt: ${log.cost}`,
          eventApplied: log.gameEventId != null
        };
      });
    }

    currentBudget.value = budgetResponse.data.teamBud;
    emit('budget-changed-in-menu', currentBudget.value);

  } catch (err: any) { // POPRAWKA: Dodano typ `any` do błędu
    console.error("Błąd podczas pobierania danych panelu gracza:", err);
    error.value = "Nie udało się załadować danych.";
  } finally {
    isLoading.value = false;
  }
}

defineExpose({
  fetchGameLog: fetchData,
  fetchTeamBud: fetchData
});

// --- Logika cyklu życia i SignalR ---
const handleHistoryUpdate = () => {
  console.log("PlayerMenu: Otrzymano 'HistoryUpdated'. Odświeżam dane.");
  fetchData();
};

watch(hasRequiredIds, (hasIds) => {
  if (hasIds) {
    fetchData();
  }
}, { immediate: true });

onMounted(() => {
  if (signalrService.connection) {
    signalrService.connection.on("HistoryUpdated", handleHistoryUpdate);
    console.log("PlayerMenu: Zarejestrowano listener 'HistoryUpdated'.");
  } else {
    console.warn("PlayerMenu: Połączenie SignalR nie było aktywne podczas montowania komponentu.");
  }
});

onUnmounted(() => {
  if (signalrService.connection) {
    signalrService.connection.off("HistoryUpdated", handleHistoryUpdate);
    console.log("PlayerMenu: Usunięto listener 'HistoryUpdated'.");
  }
});
</script>