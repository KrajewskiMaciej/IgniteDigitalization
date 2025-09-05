<template>
  <div class="flex flex-col md:flex-row bg-secondary p-4 rounded-md text-white w-full max-w-7xl mx-auto space-y-6 md:space-y-0 md:space-x-6">
    <!-- Lewa kolumna -->
    <div class="flex-1 p-4 bg-secondary rounded-md min-h-[500px]">
      <h2 class="text-xl font-bold mb-4 text-center">
        Wybierz stół i {{ actionMode === 'cards' ? 'decyzję' : 'przedmiot' }}
      </h2>

      <!-- Przełącznik kart/przedmiotów -->
      <div class="flex justify-center gap-4 mb-4">
        <button
          @click="actionMode = 'cards'"
          :class="actionMode === 'cards' ? 'bg-accent text-black shadow' : 'bg-gray-700 text-gray-300 hover:bg-gray-600'"
          class="px-4 py-1 rounded font-semibold transition"
        >
          Decyzje
        </button>
        <button
          @click="actionMode = 'items'"
          :class="actionMode === 'items' ? 'bg-accent text-black shadow' : 'bg-gray-700 text-gray-300 hover:bg-gray-600'"
          class="px-4 py-1 rounded font-semibold transition"
        >
          Przedmioty
        </button>
      </div>

      <select
        v-if="!teamId"
        v-model="selectedTableId"
        class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 w-full mb-2"
      >
        <option disabled value="">-- Wybierz stół --</option>
        <option v-for="team in tables" :key="team.teamId" :value="team.teamId">{{ team.teamName }}</option>
      </select>

      <select
        v-if="actionMode === 'cards'"
        v-model="selectedCardId"
        class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 w-full mb-2"
      >
        <option disabled value="">-- Wybierz kartę --</option>
        <option v-for="card in cards" :key="card.id" :value="card.id">{{ card.id }} - {{ card.title }}</option>
      </select>

      <select
        v-else
        v-model="selectedItemId"
        class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 w-full mb-2"
      >
        <option disabled value="">-- Wybierz przedmiot --</option>
        <option v-for="item in items" :key="item.id" :value="item.id">{{ item.id }} - {{ item.title }}</option>
      </select>

      <p v-if="actionMode === 'cards' && selectedCard" class="text-sm text-center text-gray-300 mt-2 italic">
        {{ selectedCard.description }}
      </p>
      <p v-if="actionMode === 'items' && selectedItem" class="text-sm text-center text-gray-300 mt-2 italic">
        {{ selectedItem.description }}
      </p>

      <p v-if="actionMode === 'cards' && selectedCard" class="text-center text-sm mt-2">
        Koszt karty: <span class="font-semibold">{{ selectedCard?.cost || 0 }} bitów</span>
      </p>
      <p v-if="actionMode === 'items' && selectedItem" class="text-center text-sm mt-2">
        Koszt przedmiotu: <span class="font-semibold">{{ selectedItem?.cost || 0 }} bitów</span>
      </p>

      <p v-if="selectedTableId" class="text-center text-sm mt-1">
        Bity {{ selectedTeam?.teamName }}: <span class="font-semibold">{{ currentBits }}</span>
      </p>
      <div class="flex justify-center mt-4">
        <button
          v-if="actionMode === 'cards'"
          :disabled="!selectedCardId || !selectedTableId" 
          @click="playCard"
          class="px-4 py-2 bg-lime-500 text-black font-bold rounded hover:bg-lime-600 disabled:opacity-50"
        >
          Zagraj kartę
        </button>

        <button
          v-else
          :disabled="!selectedItemId || !selectedTableId"
          @click="giveItem"
          class="px-4 py-2 bg-cyan-500 text-black font-bold rounded hover:bg-cyan-600 disabled:opacity-50"
        >
          Użyj przedmiot
        </button>
      </div>

      <!-- Wybór i zatwierdzenie zdarzenia (tylko w widoku ogólnym) -->
      <div v-if="!teamId" class="mt-6">
        <label class="block text-lg font-bold mb-2">Wybierz zdarzenie losowe:</label>

        <select
          v-model="selectedPendingEventIndex"
          class="bg-tertiary text-base border border-gray-500 rounded px-3 py-2 w-full mb-2"
        >
          <option v-for="(event, index) in availableEvents" :key="index" :value="event.eventId">
              {{ event.shortDesc }}
          </option>
        </select>

        <p v-if="selectedEvent && selectedEvent.eventId" class="text-sm text-center text-gray-300 mt-2 italic mb-4">
          {{ selectedEvent.longDesc }}
        </p>

        <div class="flex justify-center">
          <button
            @click="applySelectedEvent"
            class="px-4 py-1 bg-blue-500 text-sm font-semibold text-white rounded hover:bg-blue-600"
          >
            Zastosuj
          </button>
        </div>
      </div>

      <!-- Trzy przełączniki -->
      <div class="mt-6 mb-4 flex flex-wrap gap-6 justify-center items-center">
        <div class="flex items-center gap-2">
          <label class="relative w-16 h-8 rounded-full cursor-pointer block transition-colors duration-300"
                :class="showMenu ? 'bg-accent' : 'bg-primary'">
            <input type="checkbox" class="sr-only" v-model="showMenu" />
            <span class="w-6 h-6 bg-white absolute left-1 top-1 rounded-full transition-transform duration-300"
                  :class="{ 'translate-x-8': showMenu }"></span>
          </label>
          <span class="text-sm">Menu</span>
        </div>

        <div class="flex items-center gap-2">
          <label class="relative w-16 h-8 rounded-full cursor-pointer block transition-colors duration-300"
                :class="showOwnBoard ? 'bg-accent' : 'bg-primary'">
            <input type="checkbox" class="sr-only" v-model="showOwnBoard" />
            <span class="w-6 h-6 bg-white absolute left-1 top-1 rounded-full transition-transform duration-300"
                  :class="{ 'translate-x-8': showOwnBoard }"></span>
          </label>
          <span class="text-sm">Twoja plansza</span>
        </div>

        <div class="flex items-center gap-2">
          <label class="relative w-16 h-8 rounded-full cursor-pointer block transition-colors duration-300"
                :class="showOpponentsBoard ? 'bg-accent' : 'bg-primary'">
            <input type="checkbox" class="sr-only" v-model="showOpponentsBoard" />
            <span class="w-6 h-6 bg-white absolute left-1 top-1 rounded-full transition-transform duration-300"
                  :class="{ 'translate-x-8': showOpponentsBoard }"></span>
          </label>
          <span class="text-sm">Plansza rywali</span>
        </div>
      </div>

      <!-- GameBoard -->
      <div class="w-full flex justify-center">
        <GameBoard
          :config="enemyformData"
          :game-mode="false"
          :pawns="enemypawns"
        />
      </div>

    </div>

    <!-- Prawa kolumna: decyzje do zatwierdzenia -->
    <div class="flex-1 p-4 bg-secondary rounded-md min-h-[500px]">
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-xl font-bold text-center">🕒 Panel decyzji</h2>
        <div class="flex justify-center gap-2 mb-4">
          <button
            @click="decisionMode = 'pending'"
            :class="[
              'px-4 py-2 rounded font-semibold transition',
              decisionMode === 'pending' ? 'bg-accent text-black shadow' : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
            ]"
          >
            Do zatwierdzenia
          </button>

          <button
            @click="decisionMode = 'history'"
            :class="[
              'px-4 py-2 rounded font-semibold transition',
              decisionMode === 'history' ? 'bg-accent text-black shadow' : 'bg-gray-700 text-gray-300 hover:bg-gray-600'
            ]"
          >
            Historia decyzji
          </button>
        </div>
      </div>

      <!-- Decyzje do zatwierdzenia -->
     <div v-if="decisionMode === 'pending'" class="overflow-y-auto scroll-smooth max-h-[650px] pr-2 space-y-3 border border-lgray-accent rounded-md shadow-inner bg-secondary-dark p-2">
        <div v-if="loadingPending" class="text-center text-gray-400">Ładowanie sugestii...</div>
        <div v-else-if="pendingDecisions.length === 0" class="text-center text-gray-400 p-4">
          Brak decyzji do zatwierdzenia.
        </div>

        <div
          v-for="entry in pendingDecisions"
          :key="entry.logId"
          class="p-2 rounded border border-yellow-500 bg-secondary relative"
        >
          <p><strong>{{ entry.tableName }}</strong> sugeruje:</p>
          <p class="font-semibold text-lg">{{ entry.cardTitle }}</p>
          <p class="text-xs text-gray-400 mt-1">Zasugerowano: {{ formatDate(entry.timestamp) }}</p>
          <div class="flex justify-end space-x-2 mt-2">
            <button @click="approveDecision(entry.logId)" class="px-2 py-1 bg-green-500 text-sm text-black rounded hover:bg-green-600">Zatwierdź</button>
            <button @click="rejectDecision(entry.logId)" class="px-2 py-1 bg-red-500 text-sm text-white rounded hover:bg-red-600">Odrzuć</button>
          </div>
        </div>
     </div>

      <!-- Historia decyzji -->
      <div v-else class="space-y-4 max-h-[650px] overflow-y-auto scroll-smooth">
        <div v-if="loadingHistory" class="text-center text-gray-400">Ładowanie historii...</div>
        <div v-else-if="decisions.length === 0" class="text-center text-gray-400">Brak decyzji w historii dla tej gry.</div>

        <div
          v-for="(entry, index) in decisions"
          :key="index"
          class="mb-2" 
        >
          <!-- Specjalny wygląd dla powiadomienia o evencie -->
          <div v-if="entry.isEventNotification" class="border border-blue-500 rounded p-3 bg-blue-900/50 text-center">
            <h3 class="font-bold text-lg text-blue-300">Nowe Wydarzenie</h3>
            <p class="text-white mt-1">{{ entry.feedbackDescription }}</p>
            <p class="text-xs text-gray-400 mt-2">Aktywowano: {{ formatDate(entry.timestamp) }}</p>
          </div>

          <!-- Normalny wygląd dla zagrania karty -->
          <div v-else class="border border-gray-600 rounded p-3 bg-secondary relative">
            <div v-if="entry.eventAppliedId" class="absolute top-1 right-2 px-2 py-0.5 bg-purple-600 text-white text-xs font-bold rounded-full">
              EVENT
            </div>
            <p><strong>{{ entry.tableName }}</strong> – Karta ID: {{ entry.cardId }}</p>
            <p :class="entry.result === 'Pozytywny' ? 'text-green-400' : 'text-red-400'">
              <strong>Wynik:</strong> {{ entry.result }}
            </p>
            <p class="text-sm mt-1">Zagrano kartę: <span class="font-semibold">{{ entry.cardTitle }}</span></p>
            <p class="text-sm mt-1">{{ entry.feedbackDescription || 'Brak opisu feedbacku.' }}</p>
            <p class="text-xs text-gray-400 mt-1">Zagrano: {{ formatDate(entry.timestamp) }}</p>
          </div>
        </div> 
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, reactive, computed, onMounted, onUnmounted, watch } from 'vue'
  import { useToast } from 'vue-toastification'
  import GameBoard from '@/components/game/gameBoard.vue'
  import apiConfig from '@/services/apiConfig';
  import apiServices from '@/services/apiServices';
  import signalService from '@/services/signalService';

  // --- DEFINICJE TYPÓW DLA DANYCH Z API ---
  interface Team {
    teamId: number;
    teamName: string;
    teamBud: number;
    deckId: number;
    boardId: number;
  }

  interface Card {
    id: number;
    title: string;
    description: string;
    cost?: number;
    enablers?: unknown[]; // Zmieniono z 'any[]' na 'unknown[]' dla bezpieczeństwa typów
  }

  interface Item {
    id: number;
    title: string;
    description: string;
    cost?: number;
  }

  interface DecisionLog {
    isEventNotification: boolean;
    timestamp: string;
    feedbackDescription: string;
    cardId?: number;
    cardTitle?: string;
    tableId?: number;
    tableName?: string;
    result?: 'Pozytywny' | 'Negatywny';
    eventAppliedId?: number | null;
  }

  interface PendingDecision {
    logId: number;
    cardId: number;
    cardTitle: string;
    tableId: number;
    tableName: string;
    timestamp: string;
  }

  interface GameEvent {
    eventId: number | null;
    shortDesc: string;
    longDesc: string;
  }

  interface Pawn {
    id: number;
    x: number;
    y: number;
    color: string;
    name: string;
  }

  // Interfejsy dla "surowych" danych z API
  interface RawApiLog {
    logId: number;
    gameEventId: number | null;
    teamId: number | null;
    timestamp: string;
    cardId: number;
    cardTitle: string;
    teamName: string;
    feedbackDescription: string;
    status: boolean;
  }

  interface RawPendingLog {
    logId: number;
    cardId: number;
    cardTitle: string;
    teamId: number;
    teamName: string;
    timestamp: string;
  }
  
  interface RawPawnData {
    teamId: number;
    posX: string | number;
    posY: string | number;
    teamColor: string;
    teamName: string;
  }

  interface RivalBoardConfig {
    boardId: number;
    name: string;
    labelsUp: string[];
    labelsRight: string[];
    descriptionDown: string;
    descriptionLeft: string;
    rows: number;
    cols: number;
    cellColor: string;
    borderColor: string;
    borderColors: string[];
  }

  const props = defineProps({
    gameId: Number,
    teamId: Number
  });

  const toast = useToast()
  const gameId = props.gameId;
  const teamId = computed(() => props.teamId);

  const loading = reactive({
    teams: true,
    cards: true,
    items: true,
  });

  const decisions = ref<DecisionLog[]>([])
  const loadingHistory = ref(true);
  const showMenu = ref(true)
  const showOwnBoard = ref(true)
  const showOpponentsBoard = ref(true)
  const selectedCardId = ref<number | null>(null)
  const selectedTableId = ref<number | null>(null)
  const selectedPendingEventIndex = ref<number | null>(null)
  const tables = ref<Team[]>([])
  const cards = ref<Card[]>([])
  const availableEvents = ref<GameEvent[]>([]);
  const enemyformData = reactive({
    Id: 1,
    Name: 'Plansza podstawowa',
    LabelsUp: ['Podstawowa kordynacja', 'Standaryzacja procesów', 'Zintegrowane działania', 'Pełna integracja strategiczna'],
    LabelsRight: ['Nowicjusz', 'Naśladowca', 'Innowator', 'Lider cyfrowy'],
    DescriptionDown: 'Poziom integracji wew/zew',
    DescriptionLeft: 'Zawansowanie Cyfrowe',
    Rows: 8,
    Cols: 8,
    CellColor: '#f0f0f0',
    BorderColor: '#595959',
    BorderColors: ['#008000', '#FFFF00', '#FFA500', '#FF0000'],
    boardId: 0
  });
  const pendingDecisions = ref<PendingDecision[]>([]);
  const loadingPending = ref(true);

  const selectedCard = computed<Card | undefined>(() => cards.value.find(c => c.id === selectedCardId.value));
  const selectedTeam = computed<Team | undefined>(() => tables.value.find(t => t.teamId === selectedTableId.value));
  const currentBits = computed(() => selectedTeam.value ? selectedTeam.value.teamBud : 0);
  const selectedEvent = computed<GameEvent | undefined>(() => {
    const selectedId = selectedPendingEventIndex.value;
    return availableEvents.value.find(e => e.eventId === selectedId);
  });

  const historyVersion = ref<string | null>(null);
  const pendingVersion = ref<string | null>(null);

  let hasLoadedOnce = false;

  watch(selectedTableId, (newTeamId) => {
    selectedCardId.value = null;
    selectedItemId.value = null;
    if (newTeamId) {
      fetchAvailableCardsForTeam();
      fetchRivalPawns();
    } else {
      cards.value = [];
      items.value = [];
    }
  });

  watch(() => props.teamId, async () => {
    await fetchPendingDecisions();
    await fetchDecisionHistory();
  });

  const fetchDecisionHistory = async () => {
    if (!gameId) {
      toast.error("Brak ID gry w adresie URL.");
      return;
    }
    if (!hasLoadedOnce) loadingHistory.value = true;

    try {
      await fetchGameEvents();
      const payload = { gameId: gameId };
      const response = await apiServices.post(apiConfig.player.getLogs, payload);
      const responseData = response.data as RawApiLog[];

      let mappedLogs: DecisionLog[] = responseData.map((log: RawApiLog) => {
        const isEvent = log.gameEventId && !log.teamId;
        if (isEvent) {
          const eventDetails = availableEvents.value.find(e => e.eventId === log.gameEventId);
          return {
            isEventNotification: true,
            timestamp: log.timestamp,
            feedbackDescription: eventDetails ? eventDetails.longDesc : `Aktywowano wydarzenie (ID: ${log.gameEventId})`
          };
        } else {
          return {
            isEventNotification: false,
            cardId: log.cardId,
            cardTitle: log.cardTitle,
            tableId: log.teamId || 0,
            tableName: log.teamName,
            timestamp: log.timestamp,
            feedbackDescription: log.feedbackDescription,
            result: log.status ? 'Pozytywny' : 'Negatywny',
            eventAppliedId: log.gameEventId
          };
        }
      });

      if (teamId.value) {
        const id = teamId.value;
        mappedLogs = mappedLogs.filter(entry =>
          entry.isEventNotification || entry.tableId === id
        );
      }
      decisions.value = mappedLogs;

      const versionResponse = await apiServices.get(apiConfig.games.getHistoryVersion(gameId));
      historyVersion.value = (versionResponse.data as { version: string }).version;
    } catch (error) {
      toast.error("Wystąpił błąd podczas ładowania historii decyzji.");
      console.error("Błąd ładowania historii:", error);
    } finally {
      if (!hasLoadedOnce) loadingHistory.value = false;
      hasLoadedOnce = true;
    }
  };

  const decisionMode = ref('history')
  const actionMode = ref('cards')
  const items = ref<Item[]>([])
  const selectedItemId = ref<number | null>(null)
  const selectedItem = computed<Item | undefined>(() => items.value.find(i => i.id === selectedItemId.value));

  async function applySelectedEvent() {
    const eventId = selectedPendingEventIndex.value;
    if (!eventId || !gameId) return;

    try {
      const payload = { eventId: eventId };
      const response = await apiServices.post(apiConfig.games.applyEvent(gameId), payload);
      toast.success((response.data as { message: string }).message || "Zdarzenie zostało aktywowane!");
      fetchDecisionHistory();
    } catch (error) {
      toast.error("Wystąpił błąd podczas aktywacji zdarzenia.");
      console.error("Błąd applySelectedEvent:", error);
    }
  }

  async function playCard() {
    const card = selectedCard.value;
    const team = selectedTeam.value;
    if (!card || !team) return;
    if (team.teamBud < (card.cost || 0)) {
      toast.error(`Drużyna ${team.teamName} ma za mało bitów!`);
      return;
    }

    const hasEnablers = card.enablers && Array.isArray(card.enablers) && card.enablers.length > 0;
    const wasSuccess = !hasEnablers;
    const endpoint = wasSuccess ? apiConfig.player.playCardSuccess(card.id) : apiConfig.player.playCardFailure(card.id);

    const payload = {
      gameId: gameId,
      teamId: team.teamId,
      deckId: team.deckId,
      boardId: team.boardId,
      cost: card.cost || 0,
      ForceExecution: true
    };

    try {
      const response = await apiServices.post(endpoint, payload);
      const resultText = wasSuccess ? "Sukces" : "Porażka";
      toast.success(`${(response.data as { message: string }).message || 'Akcja przetworzona.'} Wynik: ${resultText}`);
      await fetchTeams();
      await fetchAvailableCardsForTeam();
    } catch (error) {
      toast.error("Wystąpił błąd podczas zagrywania karty.");
      console.error(error);
    }
  }

  async function giveItem() {
    const item = selectedItem.value;
    const team = selectedTeam.value;
    if (!item || !team) return;
    if (team.teamBud < (item.cost || 0)) {
      toast.error(`Drużyna ${team.teamName} ma za mało bitów!`);
      return;
    }

    const payload = {
      gameId: gameId,
      teamId: team.teamId,
      deckId: team.deckId,
      boardId: team.boardId,
      cost: item.cost || 0,
      ForceExecution: true
    };

    try {
      const response = await apiServices.post(apiConfig.player.playCardSuccess(item.id), payload);
      toast.success((response.data as { message: string }).message || `Użyto przedmiot dla ${team.teamName}.`);
      await fetchTeams();
      await fetchAvailableCardsForTeam();
    } catch (error) {
      toast.error("Wystąpił błąd podczas używania przedmiotu.");
      console.error(error);
    }
  }

  function formatDate(timestamp: string) {
    const date = new Date(timestamp)
    return date.toLocaleString('pl-PL')
  }

  let hasLoadedPendingOnce = false;

  const fetchPendingDecisions = async () => {
    if (!gameId) return;
    if (!hasLoadedPendingOnce) loadingPending.value = true;

    try {
      const response = await apiServices.get(apiConfig.games.getPendingLogs(gameId));
      let data = response.data as RawPendingLog[];
      if (teamId.value) {
        data = data.filter((log: RawPendingLog) => log.teamId === teamId.value);
      }

      pendingDecisions.value = data.map((log: RawPendingLog) => ({
        logId: log.logId,
        cardId: log.cardId,
        cardTitle: log.cardTitle,
        tableId: log.teamId,
        tableName: log.teamName,
        timestamp: log.timestamp,
      }));
      
      const versionResponse = await apiServices.get(apiConfig.games.getPendingVersion(gameId));
      pendingVersion.value = (versionResponse.data as { version: string }).version;
    } catch (error) {
      toast.error("Błąd podczas pobierania sugestii do zatwierdzenia.");
      console.error(error);
    } finally {
      if (!hasLoadedPendingOnce) loadingPending.value = false;
      hasLoadedPendingOnce = true;
    }
  };

  const approveDecision = async (logId: number) => {
    try {
      await apiServices.post(apiConfig.games.approveLog(logId), {});
      toast.success("Sugestia została zatwierdzona!");
      fetchPendingDecisions();
      fetchDecisionHistory();
    } catch (error) {
      toast.error("Wystąpił błąd podczas zatwierdzania sugestii.");
      console.error(error);
    }
  };

  const rejectDecision = async (logId: number) => {
    try {
      await apiServices.delete(apiConfig.games.rejectLog(logId));
      toast.info("Sugestia została odrzucona.");
      fetchPendingDecisions();
    } catch (error) {
      toast.error("Wystąpił błąd podczas odrzucania sugestii.");
      console.error(error);
    }
  };

  const fetchTeams = async () => {
    if (!gameId) return;
    try {
      const response = await apiServices.get(apiConfig.games.getTeamsManagement(gameId));
      tables.value = response.data as Team[];
    } catch (error) {
      toast.error("Błąd pobierania drużyn.");
      console.error(error);
    }
  };

  const fetchAvailableCardsForTeam = async () => {
    if (!selectedTableId.value) {
      cards.value = [];
      items.value = [];
      return;
    }
    const team = selectedTeam.value;
    if (!team || !team.deckId) {
      toast.error("Wybrana drużyna nie ma przypisanej talii.");
      return;
    }
    loading.cards = true;
    loading.items = true;
    try {
      const response = await apiServices.get(apiConfig.player.getCards(team.deckId), {
        params: { gameId: gameId, teamId: team.teamId }
      });
      const data = response.data as { decisionCards: Card[], itemCards: Item[] };
      if (data && data.decisionCards && data.itemCards) {
        cards.value = data.decisionCards;
        items.value = data.itemCards;
      } else {
        toast.error("Otrzymano nieprawidłowe dane kart z serwera.");
      }
    } catch (error) {
      toast.error("Wystąpił błąd podczas pobierania dostępnych kart.");
      console.error(error);
    } finally {
      loading.cards = false;
      loading.items = false;
    }
  };

  const fetchGameEvents = async () => {
    try {
      const response = await apiServices.get(apiConfig.games.getGameEvents);
      availableEvents.value = [
        { eventId: null, shortDesc: "Brak zdarzenia", longDesc: "" },
        ...(response.data as GameEvent[])
      ];
    } catch (error) {
      toast.error("Nie udało się pobrać listy zdarzeń losowych.");
      console.error("Błąd pobierania zdarzeń:", error);
    }
  };

  const fetchRivalBoard = async () => {
    if (props.gameId == null) return;
    
    try {
      const response = await apiServices.get(apiConfig.games.getGameData, { params: { gameId: props.gameId } });
      const data = response.data as { rivalBoardConfig: RivalBoardConfig };

      if (data.rivalBoardConfig) {
        const config = data.rivalBoardConfig;
        enemyformData.Id = config.boardId;
        enemyformData.Name = config.name;
        enemyformData.LabelsUp = config.labelsUp;
        enemyformData.LabelsRight = config.labelsRight;
        enemyformData.DescriptionDown = config.descriptionDown;
        enemyformData.DescriptionLeft = config.descriptionLeft;
        enemyformData.Rows = config.rows;
        enemyformData.Cols = config.cols;
        enemyformData.CellColor = config.cellColor;
        enemyformData.BorderColor = config.borderColor;
        enemyformData.BorderColors = config.borderColors;
        enemyformData.boardId = config.boardId; 
        await fetchRivalPawns();
      }
    } catch (error) {
      toast.error("Wystąpił błąd podczas ładowania danych planszy rywala.");
      console.error("Błąd w fetchRivalBoard:", error);
    }
  };

  const enemypawns = ref<Pawn[]>([]);
  const fetchRivalPawns = async () => {
    try {
      const response = await apiServices.get(apiConfig.player.getRivalPawns, {
        params: { gameId: gameId, boardId: enemyformData.Id }
      });
      enemypawns.value = (response.data as RawPawnData[]).map((p: RawPawnData) => ({
        id: p.teamId,
        x: Number(p.posX),
        y: Number(p.posY),
        color: p.teamColor,
        name: p.teamName
      }));
    } catch (err) {
      console.error("Błąd pobierania pionków rywala:", err);
    }
  };

  onMounted(async () => {
    if (!gameId) return;

    await Promise.all([
        fetchDecisionHistory(),
        fetchPendingDecisions(),
        fetchGameEvents(),
        fetchRivalBoard(),
    ]);

    try {
      await signalService.start();
      await signalService.joinGameRoom(gameId);
      console.log("Połączono z SignalR i dołączono do pokoju gry.");

      signalService.connection.on("HistoryUpdated", () => {
        console.log("Otrzymano powiadomienie: Historia się zmieniła. Odświeżam...");
        fetchDecisionHistory();
        fetchTeams();
      });
      signalService.connection.on("PendingUpdated", () => {
        console.log("Otrzymano powiadomienie: Sugestie się zmieniły. Odświeżam...");
        fetchPendingDecisions();
      });
      signalService.connection.on("BoardUpdated", () => {
        console.log("SignalR: BoardUpdated – odświeżam pionki");
        fetchRivalPawns();
      });
    } catch (err) {
      console.error("Błąd połączenia SignalR: ", err);
    }

    await fetchTeams();
    if (teamId.value) {
      selectedTableId.value = teamId.value;
      await fetchAvailableCardsForTeam();
    }
  });

  onUnmounted(() => {
    if (gameId) {
        signalService.leaveGameRoom(gameId);
    }
  });
</script>