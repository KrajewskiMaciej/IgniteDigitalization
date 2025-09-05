<template>
  <div class="w-full max-w-xl mx-auto mt-10">
    <div v-if="loading" class="text-center text-white">Ładowanie kart...</div>
    <div v-else-if="fetchError" class="text-center text-red-500">Błąd ładowania kart: {{ fetchError }}</div>
    <div v-else-if="!cards || cards.length === 0" class="text-center text-white">Brak dostępnych kart.</div>
    
    <!-- Warunek v-else, aby uniknąć renderowania, gdy karty są puste -->
    <div v-else class="relative">
      <div
        @mousedown="startHold"
        @mouseup="cancelHold"
        @mouseleave="cancelHold"
        :class="[
          'w-full h-64 bg-gradient-to-br from-lime-400 via-lime-500 to-lime-600 rounded-2xl shadow-2xl text-white transition-transform duration-200 ease-in-out',
          cardClicked ? 'scale-105' : 'scale-100'
        ]"
      >
        <div class="flex h-full">
          <!-- Lewy przycisk -->
          <button
            @click.stop="prevCard"
            class="w-12 flex items-center justify-center hover:bg-black/20 rounded-l-2xl transition"
            aria-label="Poprzednia karta"
            :disabled="cards.length <= 1"
          >
            <!-- POPRAWKA: Użyto encji HTML, aby uniknąć błędu parsowania -->
            <p>&lt;</p>
          </button>

          <!-- Środek karty -->
          <div v-if="cards[currentIndex]" class="flex-1 flex flex-col items-center justify-center px-6 text-center">
            <h2 class="text-2xl font-bold mb-2">
              {{ cards[currentIndex].title }}
            </h2>
            <p v-if="showDescriptions" class="text-base text-white/90 italic">
              {{ cards[currentIndex].description }}
            </p>
            <p class="text-xs mt-2">ID: {{ cards[currentIndex].id }}</p>
          </div>

          <!-- Prawy przycisk -->
          <button
            @click.stop="nextCard"
            class="w-12 flex items-center justify-center hover:bg-black/20 rounded-r-2xl transition"
            aria-label="Następna karta"
            :disabled="cards.length <= 1"
          >
            <!-- POPRAWKA: Użyto encji HTML -->
            <p>&gt;</p>
          </button>
        </div>
      </div>

      <!-- Wskaźniki (zawijane) -->
      <div v-if="cards.length > 0 && cards[currentIndex]" class="flex flex-wrap justify-center gap-2 mt-6">
        <button
          v-for="card in cards"
          :key="card.id"
          @click="goToCardByDisplayOrder(card.displayOrder)"
          class="w-8 h-8 text-sm font-medium rounded-full flex items-center justify-center transition-all duration-300 border-2"
          :class="card.displayOrder === cards[currentIndex].displayOrder
            ? 'bg-primary text-white border-white scale-110'
            : 'bg-gray-200 text-gray-700 border-gray-400 hover:bg-gray-300'"
        >
          {{ card.id }}
        </button>
      </div>

      <!-- Przyciski akcji -->
      <div class="flex justify-center mt-6">
        <button
          @click="sendCardSelection"
          class="bg-black text-white px-6 py-2 rounded-full shadow-md hover:bg-gray-800 transition-colors duration-200"
        >
          Wybierz kartę
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
// Upewnij się, że ta ścieżka jest poprawna dla Twojego projektu
import apiClient from '@/services/apiServices'; 

// --- DEFINICJE INTERFEJSÓW ---
interface Card {
  id: number;
  title: string;
  description: string;
  displayOrder: number;
  cost: number;
  enablers: any[]; // Można doprecyzować typ, jeśli jest znany
}

// --- PROPS ---
const props = defineProps({
  deckId: Number,
  gameId: Number,
  teamId: Number,
  boardId: Number,
  gameProcessId: Number,
  currentBudget: {
    type: Number,
    default: 0
  },
  showDescriptions: {
    type: Boolean,
    default: false
  }
});

// --- ZMIENNE REAKTYWNE Z TYPOWANIEM ---
// POPRAWKA: Jawne typowanie tablicy `cards` za pomocą interfejsu `Card`
const cards = ref<Card[]>([]);
const currentIndex = ref(0);
const loading = ref(true);
const fetchError = ref<string | null>(null);
const cardClicked = ref(false);
// POPRAWKA: Jawne typowanie dla zmiennej przechowującej timer
let holdTimeout: ReturnType<typeof setTimeout> | null = null;

const emit = defineEmits(['card-action-completed']);

const deckIdToFetch = computed(() => props.deckId);

// --- FUNKCJE ---
async function fetchCards() {
  if (!deckIdToFetch.value) {
    loading.value = false;
    return;
  }

  loading.value = true;
  fetchError.value = null;

  try {
    const response = await apiClient.get<Card[]>(`/player/deck/${deckIdToFetch.value}/unified-cards`, {
      params: { gameId: props.gameId, teamId: props.teamId }
    });

    if (response && response.data && Array.isArray(response.data)) {
      // POPRAWKA: Dodano typy do parametrów funkcji sortującej
      const sortedCards = response.data.sort((a: Card, b: Card) => a.displayOrder - b.displayOrder);
      cards.value = sortedCards;
      currentIndex.value = 0;
    } else {
      cards.value = [];
    }
  } catch (error: any) { // POPRAWKA: Dodano typ `any` do błędu w bloku catch
    cards.value = [];
    fetchError.value = error.response?.data?.message || error.message || 'Nieznany błąd';
    console.error("[CardCarousel] Błąd pobierania kart:", error);
  } finally {
    loading.value = false;
  }
}

const startHold = () => {
  holdTimeout = setTimeout(() => {
    cardClicked.value = true;
  }, 500);
};

const cancelHold = () => {
  if (holdTimeout) {
    clearTimeout(holdTimeout);
  }
  cardClicked.value = false;
};

const nextCard = () => {
  if (cards.value.length === 0) return;
  currentIndex.value = (currentIndex.value + 1) % cards.value.length;
};

const prevCard = () => {
  if (cards.value.length === 0) return;
  currentIndex.value = (currentIndex.value - 1 + cards.value.length) % cards.value.length;
};

// POPRAWKA: Dodano typ `number` do parametru
const goToCardByDisplayOrder = (displayOrder: number) => {
  const targetIndex = cards.value.findIndex(card => card.displayOrder === displayOrder);
  if (targetIndex !== -1) {
    currentIndex.value = targetIndex;
  }
};

const sendCardSelection = async () => {
  if (cards.value.length === 0 || !cards.value[currentIndex.value]) return;

  const selectedCardData = cards.value[currentIndex.value];
  const hasEnablers = selectedCardData.enablers && Array.isArray(selectedCardData.enablers) && selectedCardData.enablers.length > 0;

  const propsToSend = {
    gameId: props.gameId,
    teamId: props.teamId,
    deckId: props.deckId,
    boardId: props.boardId,
    gameProcessId: props.gameProcessId,
    cost: selectedCardData.cost
  };

  const hasSufficientBudget = props.currentBudget >= propsToSend.cost;

  try {
    if (!hasEnablers && hasSufficientBudget) {
      await apiClient.post(`/player/success/${selectedCardData.id}`, propsToSend);
    } else {
      await apiClient.post(`/player/failure/${selectedCardData.id}`, propsToSend);
    }
    // Po udanej akcji, odśwież karty i emituj zdarzenie
    await fetchCards();
    emit('card-action-completed', { success: true });
  } catch (error: any) {
    console.error("Błąd podczas wysyłania wyboru karty:", error);
    emit('card-action-completed', { success: false, error: error.message });
  }
};

// --- WATCHER ---
watch(() => props.deckId, (newDeckId, oldDeckId) => {
  if (newDeckId && newDeckId !== oldDeckId) {
    fetchCards();
  }
}, { immediate: true });
</script>