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
    
    <!-- 2. Wybór Karty Decyzji (wyświetlany po wybraniu drużyny) -->
    <div v-if="selectedTeamId">
      <div v-if="loading.cards" class="text-center text-gray-400">Ładowanie kart decyzji...</div>
      
      <div v-else-if="decisionCards.length > 0" class="mb-4">
        <select v-model="selectedCardId" class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 w-full">
          <option :value="null">-- Wybierz kartę --</option>
          <option v-for="card in decisionCards" :key="card.id" :value="card.id">
            {{ card.id }} - {{ card.title }}
          </option>
        </select>
      </div>
      
      <!-- Komunikat, jeśli dla drużyny nie ma kart -->
      <div v-else class="text-center text-gray-400">Nie znaleziono kart dla wybranej drużyny.</div>
    </div>
    
    <!-- Przycisk "Odblokuj kartę" -->
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
  import { ref, reactive, onMounted, watch } from 'vue'
  import { useRoute } from 'vue-router'
  import { useToast } from 'vue-toastification'

  import apiService from '@/services/apiServices'
  import apiConfig from '@/services/apiConfig'

  interface Team {
    teamId: number;
    teamName: string;
    deckId: number;
  }

  interface DecisionCard {
  id: number;
  title: string;
  description: string;
}

interface CardsApiResponse {
  decisionCards: DecisionCard[];
  itemCards: any[];
}

  const route = useRoute();
  const toast = useToast();
  const gameId = Number(route.params.gameId);

  const teams = ref<Team[]>([]);
  const decisionCards = ref<DecisionCard[]>([]);
  const selectedTeamId = ref<number | null>(null);
  const selectedCardId = ref<number | null>(null);

  const loading = reactive({
    teams: true,
    // Karty nie ładują się na starcie, więc początkowy stan to false
    cards: false,
  });

  const fetchTeams = async () => {
    loading.teams = true;
    try {
      const response = await apiService.get(apiConfig.player.getTeamsManagement(gameId));
      teams.value = response.data as Team[];
    } catch (error) {
      toast.error("Nie udało się pobrać listy drużyn.");
      console.error(error);
    } finally {
      loading.teams = false;
    }
  };

const fetchDecisionCards = async (teamId: number) => {
    loading.cards = true;
    try {
        const selectedTeam = teams.value.find(team => team.teamId === teamId);

        if (!selectedTeam) {
            toast.error("Nie można odnaleźć wybranej drużyny.");
            loading.cards = false; // Zatrzymaj ładowanie
            return;
        }
        const response = await apiService.get<CardsApiResponse>(
            apiConfig.player.getDecisionCards(selectedTeam.deckId, gameId, selectedTeam.teamId)
        );

        // --- KLUCZOWA ZMIANA ---
        // Sprawdzamy, czy odpowiedź zawiera klucz 'decisionCards' i czy jest on tablicą
        if (response.data && Array.isArray(response.data.decisionCards)) {
            const sortedCards = response.data.decisionCards.sort((a, b) => a.id - b.id);
            decisionCards.value = sortedCards;
        } else {
            decisionCards.value = [];
            console.warn("Odpowiedź z API nie zawierała oczekiwanej tablicy 'decisionCards'.", response.data);
        }

    } catch (error) {
        toast.error("Nie udało się pobrać listy kart decyzji.");
        console.error("Błąd podczas pobierania kart decyzji:", error);
    } finally {
        loading.cards = false;
    }
};
  
  // NOWOŚĆ: Obserwator śledzący zmiany wybranej drużyny
  watch(selectedTeamId, (newTeamId) => {
    // Zawsze resetuj listę kart i wybór po zmianie drużyny
    decisionCards.value = [];
    selectedCardId.value = null;

    if (newTeamId !== null) {
      // Jeśli wybrano nową drużynę, pobierz jej karty
      fetchDecisionCards(newTeamId);
    } else {
      // Jeśli odznaczono drużynę, upewnij się, że wskaźnik ładowania jest wyłączony
      loading.cards = false;
    }
  });


  const handleCardAction = async () => {
    if (!selectedTeamId.value || !selectedCardId.value) {
      toast.warning("Proszę wybrać drużynę i kartę.");
      return;
    }

    const teamName = teams.value.find(t => t.teamId === selectedTeamId.value)?.teamName;
    const cardName = decisionCards.value.find(c => c.id === selectedCardId.value)?.title;

    try {
      const payload = {
        cardId: selectedCardId.value,
        teamId: selectedTeamId.value
      };
      const response = await apiService.post(apiConfig.player.unlockCard(gameId), payload);
      toast.success((response.data as { message: string }).message || `Pomyślnie odblokowano kartę "${cardName}" dla drużyny ${teamName}.`);
      
      selectedCardId.value = null;
    } catch (error: any) {
      if (error?.response?.status === 409) {
        toast.warning(error.response.data.message || "Ta karta jest już odblokowana.");
      } else {
        toast.error("Wystąpił błąd podczas odblokowywania karty.");
      }
      console.error("Błąd podczas akcji na karcie:", error);
    }
  };

  onMounted(() => {
    if (isNaN(gameId)) {
      toast.error("Nieprawidłowy identyfikator gry w adresie URL.");
      loading.teams = false;
      return;
    }
    // Pobieramy tylko drużyny na starcie
    fetchTeams();
  });
</script>