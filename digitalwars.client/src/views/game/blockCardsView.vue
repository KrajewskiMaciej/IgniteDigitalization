<template>
  <div class="max-w-xl mx-auto bg-secondary text-white p-6 rounded shadow-md mt-10">
    <h2 class="text-2xl font-bold mb-4 text-center text-lime-400">Zarządzanie kartami</h2>

    <!-- 1. Wybór Drużyny -->
    <div v-if="loading.teams" class="text-center text-gray-400">Ładowanie drużyn...</div>
    <div v-else class="mb-4">
      <select v-model="selectedTeamId" class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 w-full">
        <option :value="null">-- Wybierz drużynę --</option>
        <option v-for="team in teams" :key="team.teamId" :value="team.teamId">
          {{ team.teamName }}
        </option>
      </select>
    </div>
    
    <!-- 2. Wybór Karty Decyzji -->
    <div v-if="loading.cards" class="text-center text-gray-400">Ładowanie kart decyzji...</div>
    <div v-else-if="selectedTeamId" class="mb-4">
      <select v-model="selectedCardId" class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 w-full">
        <option :value="null">-- Wybierz kartę --</option>
        <option v-for="card in decisionCards" :key="card.cardId" :value="card.cardId">
          {{ card.cardId }} - {{ card.cardName }}
        </option>
      </select>
    </div>
    
    <!-- Przycisk "Odblokuj kartę", który pojawia się po dokonaniu obu wyborów -->
    <div class="text-center mt-6">
      <button
        v-if="selectedTeamId && selectedCardId"
        @click="handleCardAction"
        class="px-6 py-2 rounded font-bold bg-red-500 hover:bg-red-600 text-white transition-colors duration-200"
      >
        Odblokuj kartę
      </button>
      <p v-else class="text-sm text-gray-400">Wybierz drużynę i kartę, aby kontynuować.</p>
    </div>

  </div>
</template>

<script setup lang="ts">
  import { ref, reactive, onMounted } from 'vue'
  import { useRoute } from 'vue-router'
  import { useToast } from 'vue-toastification'

  import apiService from '@/services/apiServices'
  import apiConfig from '@/services/apiConfig'

  // --- Definicje interfejsów dla danych z API ---
  interface Team {
    teamId: number;
    teamName: string;
  }

  interface DecisionCard {
    cardId: number;
    cardName: string;
  }

  const route = useRoute();
  const toast = useToast();
  // FIX: Parametry z routera są stringami, konwertujemy na liczbę i sprawdzamy poprawność
  const gameId = Number(route.params.gameId);

  // --- Stan komponentu z jawnymi typami ---
  const teams = ref<Team[]>([]);
  const decisionCards = ref<DecisionCard[]>([]);

  const selectedTeamId = ref<number | null>(null);
  const selectedCardId = ref<number | null>(null);

  const loading = reactive({
    teams: true,
    cards: true,
  });

  // --- Funkcje do pobierania danych z API ---
  const fetchTeams = async () => {
    loading.teams = true;
    try {
      const response = await apiService.get(apiConfig.games.getTeamsManagement(gameId));
      // FIX: Rzutowanie typu na odpowiedni interfejs
      teams.value = response.data as Team[];
    } catch (error) {
      toast.error("Nie udało się pobrać listy drużyn.");
      console.error(error);
    } finally {
      loading.teams = false;
    }
  };

  const fetchDecisionCards = async () => {
    loading.cards = true;
    try {
      const response = await apiService.get(apiConfig.games.getDecisionCards(gameId));
      // FIX: Jawne typowanie parametrów w funkcji sort
      (response.data as DecisionCard[]).sort((a: DecisionCard, b: DecisionCard) => a.cardId - b.cardId);
      decisionCards.value = response.data as DecisionCard[];
    } catch (error) {
      toast.error("Nie udało się pobrać listy kart decyzji.");
      console.error(error);
    } finally {
      loading.cards = false;
    }
  };

  const handleCardAction = async () => {
    if (!selectedTeamId.value || !selectedCardId.value) {
      toast.warning("Proszę wybrać drużynę i kartę.");
      return;
    }

    // Bezpieczne pobieranie nazw dzięki zdefiniowanym typom
    const teamName = teams.value.find(t => t.teamId === selectedTeamId.value)?.teamName;
    const cardName = decisionCards.value.find(c => c.cardId === selectedCardId.value)?.cardName;

    try {
      const payload = {
        cardId: selectedCardId.value,
        teamId: selectedTeamId.value
      };

      const response = await apiService.post(apiConfig.games.unlockCard(gameId), payload);

      toast.success((response.data as { message: string }).message || `Pomyślnie odblokowano kartę "${cardName}" dla drużyny ${teamName}.`);
      
      // Resetowanie wyborów po pomyślnej akcji
      selectedCardId.value = null;

    } catch (error: any) { // FIX: Jawne otypowanie błędu
      // Bezpieczny dostęp do właściwości błędu
      if (error?.response?.status === 409) {
        toast.warning(error.response.data.message || "Ta karta jest już odblokowana.");
      } else {
        toast.error("Wystąpił błąd podczas odblokowywania karty.");
      }
      console.error("Błąd podczas akcji na karcie:", error);
    }
  };

  onMounted(() => {
    // Sprawdzamy, czy gameId jest poprawną liczbą przed wykonaniem zapytań
    if (isNaN(gameId)) {
      toast.error("Nieprawidłowy identyfikator gry w adresie URL.");
      loading.teams = false;
      loading.cards = false;
      return;
    }
    fetchTeams();
    fetchDecisionCards();
  });
</script>