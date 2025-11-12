<template>
  <div class="flex flex-col h-screen bg-primary relative overflow-hidden">
    <PlayerNavbar
      :team-name="gameData?.teamName || 'Błąd ładowania'"
      :nav-bg-color="gameData?.teamColor || 'bg-secondary'"
    />

    <!-- --- GŁÓWNA ZMIANA --- -->
    <!-- Widok błędu lub informacji o stanie gry -->
    <div v-if="gameStatusError" class="flex-1">
      <GameStatusDisplay
        :title="gameStatusError.title"
        :message="gameStatusError.message"
      />
    </div>

    <!-- Widok normalnej rozgrywki (jeśli nie ma błędu) -->
    <div v-else-if="gameData" class="flex flex-1 flex-row relative overflow-hidden">
      <!-- Lewy panel -->
      <Transition name="fade-slide" appear>
        <div
          v-if="leftOpen"
          class="absolute left-0 top-0 w-1/2 h-full bg-secondary border-r border-gray-400 shadow-lg z-40 overflow-auto p-4 transition-all duration-500 ease-in-out"
        >
          <RouterView />
          <QuestionBox />

          <!-- Przyciski przełączające -->
          <div class="flex justify-center space-x-2 my-4">
              <button
                  @click="showingDecisionCards = true"
                  :class="showingDecisionCards ? 'bg-blue-600 text-white' : 'bg-white text-black'"
                  class="px-5 py-2 rounded-md border font-semibold"
              >
                  Decyzje
              </button>
              <button
                  @click="showingDecisionCards = false"
                  :class="!showingDecisionCards ? 'bg-blue-600 text-white' : 'bg-white text-black'"
                  class="px-5 py-2 rounded-md border font-semibold"
              >
                  Przedmioty
              </button>
          </div>

          <Suspense>
              <template #default>
                  <CardCarousel
                      v-if="gameData && gameData.deckId" 
                      :deck-id="gameData.deckId" 
                      :team-id="gameData.teamId"
                      :game-id="gameData.gameId"
                      :board-id="gameData.boardConfig?.boardId"
                      :current-budget="currentGlobalBudget"
                      :showing-decision-cards="showingDecisionCards"
                      :is-online-game="gameData.IsOnline"
                      :is-independent-team="gameData.IsIndependent"
                      @card-action-completed="handleCardActionCompleted"
                  />
              </template>
              <template #fallback>
                  <div class="text-center text-white">Ładowanie karuzeli kart...</div>
              </template>
          </Suspense>
        </div>
      </Transition>

      <!-- Plansza -->
      <div
        class="transition-all duration-300 h-full bg-secondary border-2 border-lgray-accent rounded-md shadow-sm text-center p-4 pb-10 z-30"
        :class="[
          leftOpen ? 'w-1/2 ml-auto' : '',
          rightOpen ? 'w-1/2 mr-auto' : '',
          (!leftOpen && !rightOpen) ? 'w-1/2 mx-auto' : ''
        ]"
      >
        <div class="flex justify-center">
          <button
            @click="currentBoard = 'player'"
            :class="currentBoard === 'player' ? 'bg-black text-white' : 'bg-white text-black'"
            class="px-4 py-1 rounded-md border"
          >
            Twoja plansza
          </button>
          <button
            @click="currentBoard = 'market'"
            :class="currentBoard === 'market' ? 'bg-black text-white' : 'bg-white text-black'"
            class="px-4 py-1 rounded-md border"
          >
            Plansza rynku
          </button>
        </div>

        <div class="flex justify-between items-center">
          <button @click="showLeftPanel" class="bg-gray-800 text-white px-4 py-2 rounded-md">
            Panel kart
          </button>
          <button @click="showRightPanel" class="bg-gray-800 text-white px-4 py-2 rounded-md">
            Panel decyzji
          </button>
        </div>
        
        <GameBoard
          v-if="currentBoard === 'player' && gameData?.boardConfig"
          :config="formData"
          :gameMode="true"
          :pawns="pawns"
        />
        <GameBoard
          v-if="currentBoard === 'market' && gameData?.rivalBoardConfig"
          :config="enemyformData"
          :gameMode="true"
          :pawns="enemypawns"
        />
      </div>

      <!-- Prawy panel -->
      <div
        v-if="rightOpen"
        class="absolute right-0 top-0 w-1/2 h-full bg-secondary border-l border-gray-400 shadow-lg z-40 overflow-auto p-4"
      >
        <PlayerMenu 
          ref="playerMenuRef"
          v-if="currentPanel === 'menu' && gameData"
          :game-id="gameData.gameId"
          :team-id="gameData.teamId"
          @budget-changed-in-menu="handleBudgetChangeFromMenu"
        />
      </div>
    </div>

    <!-- Stopka -->
    <div class="mt-auto">
      <Footer />
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, watch, onMounted, onUnmounted } from 'vue'
import PlayerNavbar from '@/components/navbars/playerNavbar.vue'
import QuestionBox from '@/components/playerComponents/questionBox.vue'
import GameBoard from '@/components/game/gameBoard.vue'
import Footer from '@/components/footers/adminFooter.vue'
import CardCarousel from '@/components/playerComponents/CardCarousel.vue'
import PlayerMenu from '@/components/playerComponents/playerMenu.vue'
import { RouterView } from 'vue-router'
import apiConfig from '@/services/apiConfig'
import apiServices from '@/services/apiServices'
import signalrService from '@/services/signalService';
import GameStatusDisplay from '@/components/playerComponents/gameStatusDisplay.vue';
import type {BoardConfig, GameData, Pawn, RawPawnData, GameStatusError} from '@/interfaces/types'

// --- PROPSY ---
const props = defineProps({
  teamToken: String
});

// --- ZMIENNE STANU ---
const showingDecisionCards = ref(true);
const currentPanel = ref('menu');
const leftOpen = ref(false);
const rightOpen = ref(true);
const currentBoard = ref('player');

const gameData = ref<GameData | null>(null);
const isLoading = ref(true);
const errorLoading = ref<string | null>(null);

const currentGlobalBudget = ref(0);
const pawns = ref<Pawn[]>([]);
const enemypawns = ref<Pawn[]>([]);

const playerMenuRef = ref<{ fetchGameLog: () => void; fetchTeamBud: () => void; } | null>(null);

const createDefaultBoardConfig = (): BoardConfig => ({
  boardId: 0, name: 'Ładowanie...', labelsUp: [], labelsRight: [], descriptionDown: '',
  descriptionLeft: '', rows: 8, cols: 8, cellColor: '#fefae0', borderColor: '#595959', borderColors: [] // <-- POPRAWIONA NAZWA
});

const formData = reactive<BoardConfig>(createDefaultBoardConfig());
const enemyformData = reactive<BoardConfig>(createDefaultBoardConfig());

const gameStatusError = ref<GameStatusError | null>(null);

// --- FUNKCJE ---
const fetchGameDataByToken = async (token: string) => {
  if (!token) {
    gameStatusError.value = { title: "Błąd", message: "Brak tokena drużyny w adresie URL." };
    isLoading.value = false;
    return;
  }
  isLoading.value = true;
  gameStatusError.value = null; // Resetuj błąd przy każdym nowym ładowaniu
  
  try {
    const response = await apiServices.get<GameData>(apiConfig.player.getPlayerSessionDataByToken(token));
    gameData.value = response.data;
    
    currentGlobalBudget.value = gameData.value.teamBudget;

    Object.assign(formData, gameData.value.boardConfig);

    if (gameData.value.rivalBoardConfig) {
      Object.assign(enemyformData, gameData.value.rivalBoardConfig);
    } else {
      console.warn("Brak konfiguracji rivalBoardConfig.");
    }
    
    if (playerMenuRef.value) {
      playerMenuRef.value.fetchGameLog();
    }
    
    await fetchPawns();
    await fetchRivalPawns();

  } catch (err: any) {
    let title = "Wystąpił Błąd";
    let message = err.message || "Nie można załadować danych gry.";

    if (err.response) {
      const status = err.response.status;
      const data = err.response.data;

      if (status === 409 && data.errorCode) {
        switch (data.errorCode) {
          case 'GamePaused':
            title = "Gra Wstrzymana";
            message = data.message || "Gra jest obecnie wstrzymana. Skontaktuj się z Game Masterem.";
            break;
          case 'GameEnded':
            title = "Gra Zakończona";
            message = data.message || "Ta gra została już zakończona.";
            break;
        }
      } else{
          title = "Nie znaleziono Gry";
          message = data.message || "Nie znaleziono gry lub drużyny dla podanego tokena.";
      }
    }
    
    gameStatusError.value = { title, message };
    console.error("Błąd ładowania danych gry przez token:", err);
    
  } finally {
    isLoading.value = false;
  }
};

const handleCardActionCompleted = async (eventPayload: { success: boolean, newBudget?: number }) => {
  if (eventPayload.success && gameData.value) {
    if (typeof eventPayload.newBudget === 'number') {
      currentGlobalBudget.value = eventPayload.newBudget;
    }
    if (playerMenuRef.value) {
      playerMenuRef.value.fetchGameLog();
      playerMenuRef.value.fetchTeamBud();
    }
    await fetchPawns();
    await fetchRivalPawns();
  }
};

const handleBudgetChangeFromMenu = (newBudgetFromMenu: number) => {
  currentGlobalBudget.value = newBudgetFromMenu;
};

const showLeftPanel = () => { rightOpen.value = false; leftOpen.value = true; };
const showRightPanel = () => { leftOpen.value = false; rightOpen.value = true; };

const fetchPawns = async () => {
  if (!gameData.value?.gameId || !gameData.value.boardConfig?.boardId) return;
  try {
    const response = await apiServices.get<RawPawnData[]>(
      apiConfig.player.getPawns(gameData.value.gameId, gameData.value.teamId, gameData.value.boardConfig.boardId)
    );
    pawns.value = response.data.map(p => ({
      id: p.gpId!,
      x: Number(p.posX),
      y: Number(p.posY),
      color: p.color!,
      name: p.name!
    }));
  } catch (err: any) {
    console.error("Błąd pobierania pionków:", err);
  }
};

const fetchRivalPawns = async () => {
  if (!gameData.value?.gameId || !gameData.value.rivalBoardConfig?.boardId) return;
  try {
    const response = await apiServices.get<RawPawnData[]>(
      apiConfig.player.getRivalPawns(gameData.value.gameId, gameData.value.rivalBoardConfig.boardId)
    );
    enemypawns.value = response.data.map(p => ({
      id: p.teamId!,
      x: Number(p.posX),
      y: Number(p.posY),
      color: p.teamColor!,
      name: p.teamName!
    }));
  } catch (err: any) {
    console.error("Błąd pobierania pionków rynku:", err);
  }
};

// --- LOGIKA SIGNALR ---
const onBoardUpdate = (data: any) => {
  console.log("SignalR: Otrzymano 'BoardUpdated'. Odświeżam stan planszy.", data);
  fetchPawns();
  fetchRivalPawns();
};

const onHistoryUpdate = () => {
  console.log("SignalR: Otrzymano 'HistoryUpdated'. Odświeżam historię.");
  if (playerMenuRef.value) {
    playerMenuRef.value.fetchGameLog();
  }
};

let isSignalRInitialized = false;

watch(() => props.teamToken, async (newToken) => {
  if (!newToken) return;
  await fetchGameDataByToken(newToken);
  if (gameData.value?.gameId && !isSignalRInitialized) {
    isSignalRInitialized = true;
    try {
      await signalrService.start();
      await signalrService.joinGameRoom(String(gameData.value.gameId));
      console.log(`SignalR: Połączono i dołączono do pokoju gry ${gameData.value.gameId}`);
      signalrService.connection.on("BoardUpdated", onBoardUpdate);
      signalrService.connection.on("HistoryUpdated", onHistoryUpdate);
    } catch (err) {
      console.error("Błąd połączenia SignalR w playerView: ", err);
    }
  }
}, { immediate: true });

onMounted(() => {
  console.log("PlayerView zamontowany.");
});

onUnmounted(() => {
  if (gameData.value?.gameId) {
    console.log(`SignalR: Opuszczanie pokoju gry ${gameData.value.gameId}`);
    signalrService.leaveGameRoom(String(gameData.value.gameId));
    signalrService.connection.off("BoardUpdated", onBoardUpdate);
    signalrService.connection.off("HistoryUpdated", onHistoryUpdate);
  }
});
</script>

<style scoped>
  .fade-slide-enter-active,
  .fade-slide-leave-active {
    transition: all 0.5s ease;
  }
  .fade-slide-enter-from {
    opacity: 0;
    transform: translateX(-20px);
  }
  .fade-slide-leave-to {
    opacity: 0;
    transform: translateX(-20px);
  }
</style>