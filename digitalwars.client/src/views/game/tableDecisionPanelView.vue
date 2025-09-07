<template>
  <div class="flex flex-col md:flex-row bg-secondary p-4 rounded-md text-white w-full max-w-7xl mx-auto space-y-6 md:space-y-0 md:space-x-6">
    <!-- Lewa kolumna: Akcje Stołu -->
    <div class="flex-1 p-4 bg-secondary rounded-md min-h-[500px]">
      <h2 class="text-xl font-bold mb-4 text-center">
        Wybierz {{ actionMode === 'cards' ? 'decyzję' : 'przedmiot' }}
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

      <!-- Selectory kart i przedmiotów -->
      <select
        v-if="actionMode === 'cards'"
        v-model="selectedCardId"
        class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 w-full mb-2"
      >
        <option :value="null">-- Wybierz kartę --</option>
        <option v-for="card in cards" :key="card.id" :value="card.id">{{ card.id }} - {{ card.title }}</option>
      </select>
      <select
        v-else
        v-model="selectedItemId"
        class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 w-full mb-2"
      >
        <option :value="null">-- Wybierz przedmiot --</option>
        <option v-for="item in items" :key="item.id" :value="item.id">{{ item.id }} - {{ item.title }}</option>
      </select>

      <!-- Opisy i koszty -->
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
      <p v-if="teamData" class="text-center text-sm mt-1">
        Twoje bity: <span class="font-semibold">{{ teamData.teamBud }}</span>
      </p>

      <!-- Przyciski akcji -->
      <div class="flex justify-center mt-4">
        <button
          v-if="actionMode === 'cards'"
          :disabled="!selectedCardId" 
          @click="playCard"
          class="px-4 py-2 bg-lime-500 text-black font-bold rounded hover:bg-lime-600 disabled:opacity-50"
        >
          Zagraj kartę
        </button>
        <button
          v-else
          :disabled="!selectedItemId"
          @click="giveItem"
          class="px-4 py-2 bg-cyan-500 text-black font-bold rounded hover:bg-cyan-600 disabled:opacity-50"
        >
          Użyj przedmiot
        </button>
      </div>
      
       <div v-if="showOwnBoard" class="w-full flex justify-center mt-6">
        <GameBoard
          :config="formData"
          :game-mode="true"
          :pawns="pawns"
        />
      </div>
    </div>

    <!-- Prawa kolumna: Panel Decyzji -->
    <div class="flex-1 p-4 bg-secondary rounded-md min-h-[500px]">
       <div class="flex items-center justify-between mb-4">
        <h2 class="text-xl font-bold text-center">🕒 Panel decyzji</h2>
        <div class="flex justify-center gap-2">
          <button
            @click="decisionMode = 'pending'"
            :class="['px-4 py-2 rounded font-semibold transition', decisionMode === 'pending' ? 'bg-accent text-black shadow' : 'bg-gray-700 text-gray-300 hover:bg-gray-600']"
          >
            Do zatwierdzenia
          </button>
          <button
            @click="decisionMode = 'history'"
            :class="['px-4 py-2 rounded font-semibold transition', decisionMode === 'history' ? 'bg-accent text-black shadow' : 'bg-gray-700 text-gray-300 hover:bg-gray-600']"
          >
            Historia decyzji
          </button>
        </div>
      </div>

     <div v-if="decisionMode === 'pending'" class="overflow-y-auto scroll-smooth max-h-[650px] pr-2 space-y-3 border border-lgray-accent rounded-md shadow-inner bg-secondary-dark p-2">
        <div v-if="loading.pending" class="text-center text-gray-400">Ładowanie sugestii...</div>
        <div v-else-if="pendingDecisions.length === 0" class="text-center text-gray-400 p-4">
          Brak decyzji do zatwierdzenia.
        </div>
        <div v-for="entry in pendingDecisions" :key="entry.logId" class="p-2 rounded border border-yellow-500 bg-secondary relative">
          <p><strong>{{ entry.tableName }}</strong> sugeruje:</p>
          <p class="font-semibold text-lg">{{ entry.cardTitle }}</p>
          <p class="text-xs text-gray-400 mt-1">Zasugerowano: {{ formatDate(entry.timestamp) }}</p>
          <div class="flex justify-end space-x-2 mt-2">
            <button @click="approveDecision(entry.logId)" class="px-2 py-1 bg-green-500 text-sm text-black rounded hover:bg-green-600">Zatwierdź</button>
            <button @click="rejectDecision(entry.logId)" class="px-2 py-1 bg-red-500 text-sm text-white rounded hover:bg-red-600">Odrzuć</button>
          </div>
        </div>
     </div>
      <div v-else class="space-y-4 max-h-[650px] overflow-y-auto scroll-smooth">
        <div v-if="loading.history" class="text-center text-gray-400">Ładowanie historii...</div>
        <div v-else-if="decisions.length === 0" class="text-center text-gray-400 p-4">Brak decyzji w historii dla tej gry.</div>
        <div v-for="(entry, index) in decisions" :key="index" class="mb-2">
          <div v-if="entry.isEventNotification" class="border border-blue-500 rounded p-3 bg-blue-900/50 text-center">
            <h3 class="font-bold text-lg text-blue-300">Nowe Wydarzenie</h3>
            <p class="text-white mt-1">{{ entry.feedbackDescription }}</p>
            <p class="text-xs text-gray-400 mt-2">Aktywowano: {{ formatDate(entry.timestamp) }}</p>
          </div>
          <div v-else class="border border-gray-600 rounded p-3 bg-secondary relative">
            <div v-if="entry.eventAppliedId" class="absolute top-1 right-2 px-2 py-0.5 bg-purple-600 text-white text-xs font-bold rounded-full">EVENT</div>
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
  import { ref, reactive, computed, onMounted, onUnmounted, watch } from 'vue';
  import { useToast } from 'vue-toastification';
  import GameBoard from '@/components/game/gameBoard.vue';
  import apiConfig from '@/services/apiConfig';
  import apiServices from '@/services/apiServices';
  import signalService from '@/services/signalService';

  // --- INTERFEJSY ---
  interface SessionData {
    teamId: number;
    teamName: string;
    teamBud: number;
    deckId: number;
    boardConfig: BoardConfig & { boardId: number };
  }
  interface AvailableCardsResponse {
    decisionCards: Card[];
    itemCards: Item[];
  }
  interface TeamData {
    teamId: number;
    teamName: string;
    teamBud: number;
    deckId: number;
    boardId: number;
  }
  interface Card { id: number; title: string; description: string; cost?: number; enablers?: unknown[]; }
  interface Item { id: number; title: string; description: string; cost?: number; }
  interface DecisionLog { isEventNotification: boolean; timestamp: string; feedbackDescription: string; cardId?: number; cardTitle?: string; tableId?: number; tableName?: string; result?: 'Pozytywny' | 'Negatywny'; eventAppliedId?: number | null; }
  interface PendingDecision { logId: number; cardId: number; cardTitle: string; tableId: number; tableName: string; timestamp: string; }
  interface Pawn { id: number; x: number; y: number; color: string; name: string; }
  interface BoardConfig { name: string; labelsUp: string[]; labelsRight: string[]; descriptionDown: string; descriptionLeft: string; Rows: number; Cols: number; cellColor: string; borderColor: string; borderColors: string[]; }
  interface RawHistoryLog { isEventNotification: boolean; eventDescription: string; timestamp: string; cardId: number; cardTitle: string; teamId: number; teamName: string; feedbackDescription: string; status: boolean; gameEventId: number | null; }
  interface RawPendingLog { logId: number; cardId: number; cardTitle: string; teamId: number; teamName: string; timestamp: string; }
  interface RawPawn { gpId: number; posX: string; posY: string; color: string; name: string; }

  const props = defineProps({
    gameId: { type: [Number, String], required: true },
    teamId: { type: [Number, String], required: true }
  });

  const toast = useToast();

  const loading = reactive({ teamData: true, cards: true, items: true, history: true, pending: true });
  const teamData = ref<TeamData | null>(null);
  const cards = ref<Card[]>([]);
  const items = ref<Item[]>([]);
  const decisions = ref<DecisionLog[]>([]);
  const pendingDecisions = ref<PendingDecision[]>([]);
  const pawns = ref<Pawn[]>([]);
  
  const actionMode = ref<'cards' | 'items'>('cards');
  const decisionMode = ref<'pending' | 'history'>('history');
  const selectedCardId = ref<number | null>(null);
  const selectedItemId = ref<number | null>(null);
  const showOwnBoard = ref(true);

  const formData = reactive<BoardConfig>({
    name: 'Plansza podstawowa', labelsUp: [], labelsRight: [], descriptionDown: '', descriptionLeft: '',
    Rows: 8, Cols: 8, cellColor: '#fefae0', borderColor: '#595959', borderColors: []
  });

  const selectedCard = computed<Card | undefined>(() => cards.value.find(c => c.id === selectedCardId.value));
  const selectedItem = computed<Item | undefined>(() => items.value.find(i => i.id === selectedItemId.value));

  // --- POBIERANIE DANYCH ---
  const fetchAllDataForTeam = async () => {
    const gameIdNum = Number(props.gameId);
    const teamIdNum = Number(props.teamId);
    if (isNaN(gameIdNum) || isNaN(teamIdNum)) return;

    Object.keys(loading).forEach(k => loading[k as keyof typeof loading] = true);

    try {
      // Wywołanie jest teraz zgodne z poprawioną definicją w apiConfig.ts
      const response = await apiServices.get<SessionData>(apiConfig.player.getTeamInfo(gameIdNum, teamIdNum));
      const sessionData = response.data;

      if (sessionData.boardConfig) {
        Object.assign(formData, sessionData.boardConfig);
      }
      
      teamData.value = {
        teamId: sessionData.teamId,
        teamName: sessionData.teamName,
        teamBud: sessionData.teamBud,
        deckId: sessionData.deckId,
        boardId: sessionData.boardConfig?.boardId,
      };

      await Promise.all([
        fetchAvailableCardsAndItems(),
        fetchPawns(),
        fetchDecisionHistory(),
        fetchPendingDecisions()
      ]);

    } catch (error) {
      toast.error("Wystąpił błąd podczas ładowania kluczowych danych drużyny.");
      console.error("Błąd w fetchAllDataForTeam:", error);
    } finally {
      Object.keys(loading).forEach(k => loading[k as keyof typeof loading] = false);
    }
  };
  
  const fetchAvailableCardsAndItems = async () => {
    if (!teamData.value?.deckId) return;
    try {
      const response = await apiServices.get<AvailableCardsResponse>(apiConfig.player.getCards(teamData.value.deckId), {
        params: { gameId: props.gameId, teamId: props.teamId }
      });
      cards.value = response.data.decisionCards || [];
      items.value = response.data.itemCards || [];
    } catch (error) { toast.error("Błąd pobierania kart i przedmiotów."); }
  };
  
  const fetchDecisionHistory = async () => {
    try {
      const response = await apiServices.post<RawHistoryLog[]>(apiConfig.player.getPlayerHistory, { gameId: props.gameId, teamId: props.teamId });
      const logs = response.data;
      if (Array.isArray(logs)) {
        decisions.value = logs.map(log => log.isEventNotification
          ? { isEventNotification: true, feedbackDescription: log.eventDescription || "Aktywowano nowe wydarzenie.", timestamp: log.timestamp }
          : { isEventNotification: false, cardId: log.cardId, cardTitle: log.cardTitle, tableId: log.teamId, tableName: log.teamName, timestamp: log.timestamp, feedbackDescription: log.feedbackDescription, result: log.status ? 'Pozytywny' : 'Negatywny', eventAppliedId: log.gameEventId }
        );
      }
    } catch (error) { toast.error("Błąd ładowania historii decyzji."); }
  };

  const fetchPendingDecisions = async () => {
    try {
      const response = await apiServices.get<RawPendingLog[]>(apiConfig.games.getPendingLogs(Number(props.gameId)));
      pendingDecisions.value = (response.data)
        .filter(log => log.teamId === Number(props.teamId))
        .map(log => ({
          logId: log.logId,
          cardId: log.cardId,
          cardTitle: log.cardTitle,
          tableId: log.teamId,
          tableName: log.teamName,
          timestamp: log.timestamp
        }));
    } catch (error) { toast.error("Błąd pobierania sugestii."); }
  };

  const fetchPawns = async () => {
    if (!teamData.value?.boardId) return;
    try {
      const response = await apiServices.get<RawPawn[]>(apiConfig.player.getPawns, { params: { gameId: props.gameId, teamId: props.teamId, boardId: teamData.value.boardId } });
      pawns.value = (response.data).map(p => ({ id: p.gpId, x: Number(p.posX), y: Number(p.posY), color: p.color, name: p.name }));
    } catch (err) { console.error("Błąd pobierania pionków:", err); }
  };

  // --- AKCJE UŻYTKOWNIKA ---
  const executeCardOrItemAction = async (isCard: boolean) => {
    const entity = isCard ? selectedCard.value : selectedItem.value;
    const team = teamData.value;

    // POPRAWKA: Usunięto błąd "Object is possibly 'undefined'"
    // Ten warunek `if` jest wystarczającym zabezpieczeniem (type guard) dla TypeScript
    if (!entity || !team) {
      return;
    }
    if (team.teamBud < (entity.cost || 0)) {
      toast.error(`Brak wystarczającej liczby bitów!`);
      return;
    }

    // Rozdzielono logikę na `if`, aby ułatwić TypeScriptowi analizę typów
    let wasSuccess = true;
    if (isCard) {
      // W tym bloku TypeScript wie, że `entity` to `Card`
      const cardEntity = entity as Card;
      wasSuccess = !(cardEntity.enablers && cardEntity.enablers.length > 0);
    }
    
    const endpoint = wasSuccess ? apiConfig.player.playCardSuccess(entity.id) : apiConfig.player.playCardFailure(entity.id);
    const payload = { gameId: Number(props.gameId), teamId: team.teamId, deckId: team.deckId, boardId: team.boardId, cost: entity.cost || 0, ForceExecution: false };

    try {
      const response = await apiServices.post<{ message: string }>(endpoint, payload);
      toast.success(response.data.message || 'Akcja przetworzona pomyślnie.');
      await fetchAllDataForTeam(); // Odśwież wszystko
    } catch (error: any) {
      toast.error(error.response?.data?.message || `Wystąpił błąd podczas akcji.`);
      console.error(error);
    }
  };

  const playCard = () => executeCardOrItemAction(true);
  const giveItem = () => executeCardOrItemAction(false);

  const approveDecision = async (logId: number) => {
    try {
      await apiServices.post(apiConfig.games.approveLog(logId), {});
      toast.success("Sugestia została zatwierdzona!");
      await Promise.all([fetchPendingDecisions(), fetchDecisionHistory()]);
    } catch (error) { toast.error("Wystąpił błąd podczas zatwierdzania sugestii."); }
  };

  const rejectDecision = async (logId: number) => {
    try {
      await apiServices.delete(apiConfig.games.rejectLog(logId));
      toast.info("Sugestia została odrzucona.");
      await fetchPendingDecisions();
    } catch (error) { toast.error("Wystąpił błąd podczas odrzucania sugestii."); }
  };

  const formatDate = (timestamp: string) => new Date(timestamp).toLocaleString('pl-PL');
  
  watch(() => props.teamId, (newId) => {
    if (newId) {
      fetchAllDataForTeam();
      selectedCardId.value = null;
      selectedItemId.value = null;
    }
  }, { immediate: true });

  onMounted(async () => {
    const gameIdNum = Number(props.gameId);
    if (isNaN(gameIdNum)) {
      toast.error("Błąd krytyczny: Brak lub nieprawidłowe ID gry!");
      return;
    }

    signalService.connection.on("HistoryUpdated", () => fetchAllDataForTeam());
    signalService.connection.on("PendingUpdated", () => fetchPendingDecisions());
    signalService.connection.on("BoardUpdated", () => fetchPawns());

    try {
      await signalService.start();
      await signalService.joinGameRoom(gameIdNum);
      console.log(`Pomyślnie dołączono do pokoju SignalR dla gry: ${gameIdNum}`);
    } catch (err) {
      console.error("Błąd połączenia SignalR: ", err);
      toast.error("Nie udało się połączyć z serwerem czasu rzeczywistego.");
    }
  });

  onUnmounted(() => {
    if (props.gameId) {
      signalService.leaveGameRoom(Number(props.gameId));
    }
  });
</script>