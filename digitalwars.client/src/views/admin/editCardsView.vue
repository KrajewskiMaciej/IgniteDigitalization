<template>
    <div class="w-full flex">
        <!-- SEKCJA EDYCJI KART DECYZJI -->
        <div class="flex flex-col flex-1 justify-center items-center m-4 px-2 py-2 border-2 border-lgray-accent rounded-md bg-tertiary">
            <h1 class="text-3xl font-nasalization text-white mt-5">Edycja kart decyzji</h1>

            <input
                type="file"
                accept=".xls,.xlsx"
                ref="fileInput"
                @change="handleFileChange"
                style="display: none;"
            />

            <button
                @click="triggerFileInput"
                class="bg-green-500 border-2 border-green-700 py-3 px-6 rounded-md mt-5 text-white">
                <font-awesome-icon :icon="faFileExcel" class="h-4 mr-2"/>
                Wczytaj z pliku xls
            </button>

            <form class="w-full max-w-lg mt-4 flex flex-col items-center">
                <div class="w-full mb-4">
                    <label class="block text-white mb-1">Wybierz talię:</label>
                    <dropDown
                        :items="decksData"
                        v-model="selectedDeckId"
                        :item-key="'id'"
                        :display-format="(deck: Deck) => `#${deck.id} ${deck.title}`"
                        placeholder="Wybierz talię..."
                    />
                </div>

                <div v-if="selectedDeckId" class="w-full">
                    <label class="block text-white mb-1">Wybierz kartę:</label>
                    <dropDown
                        :items="cardsData"
                        v-model="selectedCardId"
                        :item-key="'id'"
                        :display-format="(card: Card) => `#${card.id} ${card.title}`"
                        placeholder="Wybierz kartę..."
                    />
                </div>

                <div v-if="selectedCardId && currentCard" class="mt-6 space-y-4 text-white w-full">
                    <div class="flex flex-col">
                        <label for="title" class="mb-1">Tytuł karty:</label>
                        <input
                            id="title"
                            v-model="currentCard.title"
                            type="text"
                            class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 text-white w-full"
                        />
                    </div>

                    <div class="flex flex-col">
                        <label for="description" class="mb-1">Opis karty:</label>
                        <textarea
                            id="description"
                            v-model="currentCard.description"
                            rows="8"
                            class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 text-white resize-none w-full"
                        ></textarea>
                    </div>
                    <div class="flex justify-center w-full">
                        <button
                            @click="saveCard"
                            type="button"
                            class="bg-accent border-2 border-accent py-3 px-6 rounded-md mt-5">
                            <font-awesome-icon :icon="faSave" class="h-4 mr-2"/>
                            Zapisz
                        </button>
                    </div>
                </div>
            </form>
        </div>

        <!-- SEKCJA EDYCJI FEEDBACKU -->
        <div v-if="currentCard && selectedCardId"
             class="flex flex-col flex-1 items-center m-4 px-2 py-2 border-2 border-lgray-accent rounded-md bg-tertiary">
            <h1 class="text-3xl font-nasalization text-white mt-5">Edycja feedbacku</h1>

            <form class="w-full max-w-lg mt-4 flex flex-col items-center">
                <div class="w-full mb-4">
                    <label class="block text-white mb-1 mt-2">Wybierz feedback:</label>
                    <dropDown
                        :items="feedbackData"
                        v-model="selectedFeedbackId"
                        :item-key="'id'"
                        :display-format="(feedback: Feedback) => `${feedback.status === 'P' ? '✅' : '❌'} ${feedback.longDescription.substring(0, 30)}...`"
                        placeholder="Wybierz feedback..."
                    />
                </div>

                <div v-if="selectedFeedbackId && currentFeedback" class="flex flex-col w-full">
                    <label for="feedbackDescription" class="block text-white mb-1">Opis feedbacku:</label>
                    <textarea
                        id="feedbackDescription"
                        v-model="currentFeedback.longDescription"
                        rows="8"
                        class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 text-white resize-none w-full"
                    >
                    </textarea>
                    <div class="flex justify-center w-full">
                        <button
                            @click="saveFeedback"
                            type="button"
                            class="bg-accent border-2 border-accent py-3 px-6 rounded-md mt-5 text-white">
                            <font-awesome-icon :icon="faSave" class="h-4 mr-2"/>
                            Zapisz
                        </button>
                    </div>
                </div>
            </form>
        </div>
    </div>
</template>

<script setup lang="ts">
import { faSave, faFileExcel } from '@fortawesome/free-solid-svg-icons';
import dropDown from '@/components/dropDown.vue';
import { reactive, ref, watch, onMounted } from 'vue';
import apiConfig from '@/services/apiConfig.js';
import apiService from '@/services/apiServices.js';

// --- DEFINICJE INTERFEJSÓW ---
interface Deck {
  id: number;
  title: string;
}

interface Card {
  id: number;
  deckId: number;
  title: string;
  description: string;
}

interface Feedback {
  id: number;
  longDescription: string;
  status: 'P' | 'N';
}

// --- ZMIENNE REAKTYWNE ---
const selectedDeckId = ref<number | undefined>(undefined);
const selectedCardId = ref<number | undefined>(undefined);
const selectedFeedbackId = ref<number | undefined>(undefined);

const fileInput = ref<HTMLInputElement | null>(null);

const decksData = reactive<Deck[]>([]);
const cardsData = reactive<Card[]>([]);
const feedbackData = reactive<Feedback[]>([
  { id: 1, longDescription: 'Przykładowy feedback negatywny dla tej karty.', status: 'N' },
  { id: 2, longDescription: 'Przykładowy feedback pozytywny dla tej karty.', status: 'P' },
]);

const currentCard = ref<Card | null>(null);
const currentFeedback = ref<Feedback | null>(null);


// --- FUNKCJE ---
function triggerFileInput(): void {
  fileInput.value?.click();
}

async function handleFileChange(event: Event): Promise<void> {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0];

  if (!file) return;

  const formData = new FormData();
  formData.append("file", file);

  try {
    const response = await apiService.post(apiConfig.admin.deck.upload, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      withCredentials: true
    });
    console.log("Plik został pomyślnie wysłany:", response.data);
    await fetchDecks();
  } catch (error) {
    console.error("Błąd przy wysyłaniu pliku:", error);
  }
}

async function saveCard(): Promise<void> {
  if (!currentCard.value) return;
  console.log("Zapisywanie karty:", currentCard.value);
  alert(`Zapisano kartę: ${currentCard.value.title}`);
}

async function saveFeedback(): Promise<void> {
  if (!currentFeedback.value) return;
  console.log("Zapisywanie feedbacku:", currentFeedback.value);
  alert(`Zapisano feedback: ${currentFeedback.value.longDescription.substring(0, 30)}...`);
}

async function fetchDecks(): Promise<void> {
  try {
    const response = await apiService.get(apiConfig.admin.deck.getAll);
    decksData.length = 0;
    // POPRAWKA: Dodajemy asercję typu (as Deck[]), aby poinformować TypeScript, że spodziewamy się tablicy.
    decksData.push(...(response.data as Deck[]));
  } catch (error) {
    console.error("Błąd przy pobieraniu talii:", error);
  }
}

// --- WATCHERY ---
watch(selectedDeckId, async (newDeckId) => {
  selectedCardId.value = undefined;
  currentCard.value = null;

  if (!newDeckId) {
    cardsData.length = 0;
    return;
  }

  try {
    const url = apiConfig.admin.deck.decisions(newDeckId);
    const response = await apiService.get(url);
    
    cardsData.length = 0;
    // POPRAWKA: Dodajemy asercję typu (as Card[]), aby poinformować TypeScript, że spodziewamy się tablicy.
    cardsData.push(...(response.data as Card[]));
  } catch (error) {
    console.error("Błąd przy pobieraniu kart z talii:", error);
    cardsData.length = 0;
  }
});

watch(selectedCardId, (newCardId) => {
  selectedFeedbackId.value = undefined;
  currentFeedback.value = null;

  if (newCardId) {
    const card = cardsData.find(c => c.id === newCardId);
    currentCard.value = card ? { ...card } : null;
  } else {
    currentCard.value = null;
  }
});

watch(selectedFeedbackId, (newFeedbackId) => {
  if (newFeedbackId) {
    const feedback = feedbackData.find(f => f.id === newFeedbackId);
    currentFeedback.value = feedback ? { ...feedback } : null;
  } else {
    currentFeedback.value = null;
  }
});

// --- CYKL ŻYCIA KOMPONENTU ---
onMounted(fetchDecks);

</script>
