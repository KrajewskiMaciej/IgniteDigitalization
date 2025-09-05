<template>
  <div class="p-6 text-white">
    <h2 class="text-2xl font-bold mb-4 text-center text-lime-400">Zarządzanie budżetem drużyn</h2>

    <!-- Wybór drużyny -->
    <div v-if="!loading" class="mb-6 text-center max-w-md mx-auto">
      <label for="team-select" class="block mb-2 font-semibold">Wybierz drużynę:</label>
      <select
        id="team-select"
        v-model="selectedTeamId"
        class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 w-full"
      >
        <option :value="null">-- Wybierz drużynę --</option>
        <option v-for="team in teams" :key="team.teamId" :value="team.teamId">
          {{ team.teamName }} (Aktualny budżet: {{ team.teamBud }})
        </option>
      </select>
    </div>

    <!-- Edycja budżetu -->
    <div v-if="selectedTeam" class="max-w-md mx-auto">
      <div class="mb-4 flex items-center justify-center gap-4 bg-secondary p-4 rounded-lg shadow-inner">
        <label for="budget-input" class="font-semibold">Nowy budżet:</label>
        <input
          id="budget-input"
          v-model.number="budgetInputValue"
          type="number"
          class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 w-40 text-center text-lg font-bold"
        />
        <button
          class="px-5 py-2 bg-lime-500 hover:bg-lime-600 text-black font-bold rounded transition-colors duration-200"
          @click="saveBudget"
        >
          Zapisz
        </button>
      </div>
    </div>

    <!-- Komunikaty -->
    <div v-if="loading" class="text-center text-gray-400">Ładowanie drużyn...</div>
    <div v-else-if="!teams.length" class="text-center text-gray-400">Nie znaleziono drużyn dla tej gry.</div>
    <div v-else-if="!selectedTeam" class="text-center text-gray-400">Wybierz drużynę, aby edytować jej budżet.</div>
  </div>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted, watch } from 'vue'
  import { useRoute } from 'vue-router'
  import { useToast } from 'vue-toastification'

  import apiService from '@/services/apiServices'
  import apiConfig from '@/services/apiConfig'

  // FIX: Zdefiniowano interfejs dla obiektu Team
  interface Team {
    teamId: number;
    teamName: string;
    teamBud: number;
  }

  const toast = useToast()
  const route = useRoute()

  // FIX: Konwersja parametru z URL na liczbę
  const gameId = Number(route.params.gameId);

  // FIX: Jawne typowanie dla zmiennych stanu
  const teams = ref<Team[]>([]);
  const selectedTeamId = ref<number | null>(null);
  const loading = ref<boolean>(true);
  const budgetInputValue = ref<number>(0);

  // FIX: Jawne typowanie dla wartości obliczeniowej
  const selectedTeam = computed<Team | undefined>(() => {
    if (!selectedTeamId.value) return undefined;
    return teams.value.find(t => t.teamId === selectedTeamId.value);
  });

  // Obserwator do aktualizacji pola input, gdy zmienia się wybrana drużyna
  watch(selectedTeam, (newTeam) => {
    budgetInputValue.value = newTeam ? newTeam.teamBud : 0;
  });

  const fetchTeams = async () => {
    loading.value = true;
    try {
      const response = await apiService.get(apiConfig.games.getTeamsManagement(gameId));
      // FIX: Rzutowanie typu danych z odpowiedzi API na zdefiniowany interfejs
      teams.value = response.data as Team[];
    } catch (error) {
      toast.error('Nie udało się wczytać listy drużyn z serwera.');
      console.error("Błąd podczas pobierania drużyn:", error);
    } finally {
      loading.value = false;
    }
  }

  const saveBudget = async () => {
    if (!selectedTeam.value) {
      toast.warning('Najpierw wybierz drużynę.');
      return;
    }

    const { teamId, teamName } = selectedTeam.value;

    try {
      // Endpoint powinien przyjmować ID drużyny i nowy budżet
      await apiService.put(apiConfig.games.updateTeamBudget(teamId), { newBudget: budgetInputValue.value });
      toast.success(`Zapisano nowy budżet dla drużyny "${teamName}".`);
      
      // Odśwież dane, aby zobaczyć zmiany
      await fetchTeams();

    } catch (error: any) { // FIX: Jawne otypowanie błędu
      toast.error(`Błąd podczas zapisywania budżetu dla "${teamName}".`);
      console.error("Błąd podczas aktualizacji budżetu:", error);
    }
  }

  onMounted(() => {
    // FIX: Walidacja, czy gameId jest poprawną liczbą
    if (isNaN(gameId)) {
      toast.error("Błąd: Nieprawidłowy ID gry w adresie URL!");
      loading.value = false;
      return;
    }
    fetchTeams();
  })
</script>