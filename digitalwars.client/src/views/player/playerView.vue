<template>
  <div
    class="flex flex-col h-screen bg-gradient-to-br from-surface-850 via-surface-900 to-surface-950 relative overflow-hidden"
  >
    <div
      class="absolute inset-0 bg-[radial-gradient(circle_at_30%_20%,_var(--tw-gradient-stops))] from-primary-900/15 via-transparent to-transparent pointer-events-none"
    ></div>

    <div class="relative z-10 flex flex-col h-full">
      <PlayerNavbar
        :team-name="gameData?.teamName || 'Błąd ładowania'"
        :nav-bg-color="gameData?.teamColor || 'bg-secondary'"
      />

      <div v-if="gameStatusError" class="flex-1">
        <GameStatusDisplay :title="gameStatusError.title" :message="gameStatusError.message" />
      </div>

      <div v-else-if="gameData" class="flex-1 flex flex-col overflow-hidden">
        <div v-if="isMobile" class="flex flex-col h-full">
          <div
            class="flex gap-2 p-1 bg-surface-850 backdrop-blur-sm border-b border-surface-700 shadow-md"
          >
            <button
              @click="mobileView = 'cards'"
              class="flex-1 py-3 rounded-xl font-semibold text-sm transition-all duration-300 relative overflow-hidden group"
              :class="
                mobileView === 'cards'
                  ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                  : 'bg-surface-800 text-surface-300 hover:text-surface-0 border border-primary-500/30'
              "
            >
              <span class="relative z-10">Karty</span>
              <div
                v-if="mobileView !== 'cards'"
                class="absolute inset-0 bg-gradient-to-r from-primary-500/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
              ></div>
            </button>

            <button
              @click="mobileView = 'board'"
              class="flex-1 py-3 rounded-xl font-semibold text-sm transition-all duration-300 relative overflow-hidden group"
              :class="
                mobileView === 'board'
                  ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                  : 'bg-surface-800 text-surface-300 hover:text-surface-0 border border-primary-500/30'
              "
            >
              <span class="relative z-10">Plansza</span>
              <div
                v-if="mobileView !== 'board'"
                class="absolute inset-0 bg-gradient-to-r from-primary-500/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
              ></div>
            </button>

            <button
              @click="mobileView = 'market'"
              class="flex-1 py-3 rounded-xl font-semibold text-sm transition-all duration-300 relative overflow-hidden group"
              :class="
                mobileView === 'market'
                  ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                  : 'bg-surface-800 text-surface-300 hover:text-surface-0 border border-primary-500/30'
              "
            >
              <span class="relative z-10">Rynek</span>
              <div
                v-if="mobileView !== 'market'"
                class="absolute inset-0 bg-gradient-to-r from-primary-500/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
              ></div>
            </button>

            <button
              @click="mobileView = 'menu'"
              class="flex-1 py-3 rounded-xl font-semibold text-sm transition-all duration-300 relative overflow-hidden group"
              :class="
                mobileView === 'menu'
                  ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                  : 'bg-surface-800 text-surface-300 hover:text-surface-0 border border-primary-500/30'
              "
            >
              <span class="relative z-10">Menu</span>
              <div
                v-if="mobileView !== 'menu'"
                class="absolute inset-0 bg-gradient-to-r from-primary-500/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
              ></div>
            </button>
          </div>

          <div class="flex-1 overflow-auto p-4">
            <div v-if="mobileView === 'cards'" class="h-full">
              <RouterView />
              <QuestionBox />

              <div class="flex gap-2 my-4">
                <button
                  @click="showingDecisionCards = true"
                  class="flex-1 py-3 rounded-xl font-semibold transition-all duration-300"
                  :class="
                    showingDecisionCards
                      ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                      : 'bg-surface-800 text-surface-300 border border-primary-500/30'
                  "
                >
                  Decyzje
                </button>
                <button
                  @click="showingDecisionCards = false"
                  class="flex-1 py-3 rounded-xl font-semibold transition-all duration-300"
                  :class="
                    !showingDecisionCards
                      ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                      : 'bg-surface-800 text-surface-300 border border-primary-500/30'
                  "
                >
                  Przedmioty
                </button>
              </div>

              <Suspense>
                <template #default>
                  <CardCarousel
                    v-show="gameData && gameData.deckId"
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
                  <div class="text-center text-surface-300">Ładowanie karuzeli kart...</div>
                </template>
              </Suspense>
            </div>

            <div v-else-if="mobileView === 'board'" class="h-full">
              <GameBoard
                v-if="gameData?.boardConfig"
                :config="formData"
                :gameMode="true"
                :pawns="pawns"
              />
            </div>

            <div v-else-if="mobileView === 'market'" class="h-full">
              <GameBoard
                v-if="gameData?.rivalBoardConfig"
                :config="enemyformData"
                :gameMode="true"
                :pawns="enemypawns"
              />
            </div>

            <div v-else-if="mobileView === 'menu'" class="h-full">
              <PlayerMenu
                ref="playerMenuRef"
                v-if="gameData"
                :game-id="gameData.gameId"
                :team-id="gameData.teamId"
                @budget-changed-in-menu="handleBudgetChangeFromMenu"
              />
            </div>
          </div>
        </div>

        <div v-else class="flex h-full relative">
          <div
            v-if="leftOpen"
            class="w-1/3 bg-surface-850 backdrop-blur-sm border-r border-surface-700 shadow-2xl overflow-auto p-4 transition-all duration-300"
          >
            <RouterView />
            <QuestionBox />

            <div class="flex gap-2 my-4">
              <button
                @click="showingDecisionCards = true"
                class="flex-1 py-3 rounded-xl font-semibold transition-all duration-300"
                :class="
                  showingDecisionCards
                    ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                    : 'bg-surface-800 text-surface-300 border border-primary-500/30'
                "
              >
                Decyzje
              </button>
              <button
                @click="showingDecisionCards = false"
                class="flex-1 py-3 rounded-xl font-semibold transition-all duration-300"
                :class="
                  !showingDecisionCards
                    ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                    : 'bg-surface-800 text-surface-300 border border-primary-500/30'
                "
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
                <div class="text-center text-surface-300">Ładowanie karuzeli kart...</div>
              </template>
            </Suspense>
          </div>

          <div
            class="flex-1 flex flex-col bg-surface-850 backdrop-blur-sm shadow-xl p-4 transition-all duration-300"
          >
            <div class="flex justify-between items-center mb-6">
              <div class="flex gap-2">
                <button
                  @click="currentBoard = 'player'"
                  class="px-6 py-3 rounded-xl font-semibold transition-all duration-300"
                  :class="
                    currentBoard === 'player'
                      ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                      : 'bg-surface-800 text-surface-300 border border-primary-500/30'
                  "
                >
                  Twoja plansza
                </button>
                <button
                  @click="currentBoard = 'market'"
                  class="px-6 py-3 rounded-xl font-semibold transition-all duration-300"
                  :class="
                    currentBoard === 'market'
                      ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                      : 'bg-surface-800 text-surface-300 border border-primary-500/30'
                  "
                >
                  Plansza rynku
                </button>
              </div>

              <div class="flex gap-2">
                <button
                  @click="leftOpen = !leftOpen"
                  class="px-6 py-3 rounded-xl font-semibold bg-surface-800 text-surface-0 border border-primary-500/30 hover:border-primary-500/50 transition-all duration-300"
                >
                  {{ leftOpen ? 'Ukryj' : 'Pokaż' }} karty
                </button>
                <button
                  @click="rightOpen = !rightOpen"
                  class="px-6 py-3 rounded-xl font-semibold bg-surface-800 text-surface-0 border border-primary-500/30 hover:border-primary-500/50 transition-all duration-300"
                >
                  {{ rightOpen ? 'Ukryj' : 'Pokaż' }} menu
                </button>
              </div>
            </div>

            <div class="flex-1 overflow-auto">
              <GameBoard
                v-show="currentBoard === 'player' && gameData?.boardConfig"
                :config="formData"
                :gameMode="true"
                :pawns="pawns"
              />
              <GameBoard
                v-show="currentBoard === 'market' && gameData?.rivalBoardConfig"
                :config="enemyformData"
                :gameMode="true"
                :pawns="enemypawns"
              />
            </div>
          </div>

          <div
            v-if="rightOpen"
            class="w-1/4 bg-surface-850 backdrop-blur-sm border-l border-surface-700 shadow-2xl overflow-auto p-6 transition-all duration-300"
          >
            <PlayerMenu
              ref="playerMenuRef"
              v-show="currentPanel === 'menu' && gameData"
              :game-id="gameData.gameId"
              :team-id="gameData.teamId"
              @budget-changed-in-menu="handleBudgetChangeFromMenu"
            />
          </div>
        </div>
      </div>

      <div class="mt-auto relative z-10">
        <Footer />
      </div>
    </div>
  </div>
  <div 
    class="bg-tertiary flex justify-center items-center px-2 py-2 fixed bottom-12 left-2 lg:left-6 h-14 w-14 z-50 rounded-full border cursor-pointer border-lgray-accent text-white transition-all duration-300 ease-in-out hover:shadow-lg hover:shadow-primary-400/60 hover:border-primary-400 hover:text-primary-400 hover:scale-110 hover:-translate-y-2"
    @click="showChat = true"
  >
    <font-awesome-icon :icon="faCommentDots" class="h-10"/>
  </div>
   <div v-if="showChat" class="fixed inset-0 z-50 bg-black bg-opacity-50 lg:flex lg:items-center lg:justify-start lg:pl-4">
        <GameChat
            ref="chatRef"
            class="
                h-screen w-screen
                lg:h-3/4 lg:w-1/2 lg:max-w-2xl xl:w-1/5
            "
            @close-chat="showChat = false"
        />
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
import signalrService from '@/services/signalService'
import GameStatusDisplay from '@/components/playerComponents/gameStatusDisplay.vue'
import GameChat from '@/components/game/gameChat.vue'
import type { BoardConfig, GameData, Pawn, RawPawnData, GameStatusError, } from '@/interfaces/types'
import { useBreakpoints } from '@vueuse/core'
import {
  faCommentDots,
} from '@fortawesome/free-solid-svg-icons'
import { onClickOutside } from '@vueuse/core';


const chatRef = ref<HTMLElement | null>(null);

onClickOutside(chatRef, () => {
    showChat.value = false;
});

const breakpoints = useBreakpoints({
  mobile: 768,
})

const isMobile = breakpoints.smaller('mobile')

// --- PROPSY ---
const props = defineProps({
  teamToken: String,
})

// --- ZMIENNE STANU ---
const mobileView = ref('board')
const showingDecisionCards = ref(true)
const currentPanel = ref('menu')
const leftOpen = ref(false)
const rightOpen = ref(true)
const currentBoard = ref('player')
const showChat = ref(false);

const gameData = ref<GameData | null>(null)
const isLoading = ref(true)
const errorLoading = ref<string | null>(null)

const currentGlobalBudget = ref(0)
const pawns = ref<Pawn[]>([])
const enemypawns = ref<Pawn[]>([])

const playerMenuRef = ref<{ fetchGameLog: () => void; fetchTeamBud: () => void } | null>(null)

const createDefaultBoardConfig = (): BoardConfig => ({
  boardId: 0,
  name: 'Ładowanie...',
  labelsUp: [],
  labelsRight: [],
  descriptionDown: '',
  descriptionLeft: '',
  rows: 8,
  cols: 8,
  cellColor: '#fefae0',
  borderColor: '#595959',
  borderColors: [], // <-- POPRAWIONA NAZWA
})

const formData = reactive<BoardConfig>(createDefaultBoardConfig())
const enemyformData = reactive<BoardConfig>(createDefaultBoardConfig())

const gameStatusError = ref<GameStatusError | null>(null)

// --- FUNKCJE ---
const fetchGameDataByToken = async (token: string) => {
  if (!token) {
    gameStatusError.value = { title: 'Błąd', message: 'Brak tokena drużyny w adresie URL.' }
    isLoading.value = false
    return
  }
  isLoading.value = true
  gameStatusError.value = null // Resetuj błąd przy każdym nowym ładowaniu

  try {
    const response = await apiServices.get<GameData>(
      apiConfig.player.getPlayerSessionDataByToken(token),
    )
    gameData.value = response.data

    currentGlobalBudget.value = gameData.value.teamBudget

    Object.assign(formData, gameData.value.boardConfig)

    if (gameData.value.rivalBoardConfig) {
      Object.assign(enemyformData, gameData.value.rivalBoardConfig)
    } else {
      console.warn('Brak konfiguracji rivalBoardConfig.')
    }

    if (playerMenuRef.value) {
      playerMenuRef.value.fetchGameLog()
    }

    await fetchPawns()
    await fetchRivalPawns()
  } catch (err: any) {
    let title = 'Wystąpił Błąd'
    let message = err.message || 'Nie można załadować danych gry.'

    if (err.response) {
      const status = err.response.status
      const data = err.response.data

      if (status === 409 && data.errorCode) {
        switch (data.errorCode) {
          case 'GamePaused':
            title = 'Gra Wstrzymana'
            message = data.message || 'Gra jest obecnie wstrzymana. Skontaktuj się z Game Masterem.'
            break
          case 'GameEnded':
            title = 'Gra Zakończona'
            message = data.message || 'Ta gra została już zakończona.'
            break
        }
      } else {
        title = 'Nie znaleziono Gry'
        message = data.message || 'Nie znaleziono gry lub drużyny dla podanego tokena.'
      }
    }

    gameStatusError.value = { title, message }
    console.error('Błąd ładowania danych gry przez token:', err)
  } finally {
    isLoading.value = false
  }
}

const handleCardActionCompleted = async (eventPayload: {
  success: boolean
  newBudget?: number
}) => {
  if (eventPayload.success && gameData.value) {
    if (typeof eventPayload.newBudget === 'number') {
      currentGlobalBudget.value = eventPayload.newBudget
    }
    if (playerMenuRef.value) {
      playerMenuRef.value.fetchGameLog()
      playerMenuRef.value.fetchTeamBud()
    }
    await fetchPawns()
    await fetchRivalPawns()
  }
}

const handleBudgetChangeFromMenu = (newBudgetFromMenu: number) => {
  currentGlobalBudget.value = newBudgetFromMenu
}

const showLeftPanel = () => {
  rightOpen.value = false
  leftOpen.value = true
}
const showRightPanel = () => {
  leftOpen.value = false
  rightOpen.value = true
}

const fetchPawns = async () => {
  if (!gameData.value?.gameId || !gameData.value.boardConfig?.boardId) return
  try {
    const response = await apiServices.get<RawPawnData[]>(
      apiConfig.player.getPawns(
        gameData.value.gameId,
        gameData.value.teamId,
        gameData.value.boardConfig.boardId,
      ),
    )
    pawns.value = response.data.map((p) => ({
      id: p.gpId!,
      x: Number(p.posX),
      y: Number(p.posY),
      color: p.color!,
      name: p.name!,
    }))
  } catch (err: any) {
    console.error('Błąd pobierania pionków:', err)
  }
}

const fetchRivalPawns = async () => {
  if (!gameData.value?.gameId || !gameData.value.rivalBoardConfig?.boardId) return
  try {
    const response = await apiServices.get<RawPawnData[]>(
      apiConfig.player.getRivalPawns(
        gameData.value.gameId,
        gameData.value.rivalBoardConfig.boardId,
      ),
    )
    enemypawns.value = response.data.map((p) => ({
      id: p.teamId!,
      x: Number(p.posX),
      y: Number(p.posY),
      color: p.teamColor!,
      name: p.teamName!,
    }))
  } catch (err: any) {
    console.error('Błąd pobierania pionków rynku:', err)
  }
}

// --- LOGIKA SIGNALR ---
const onBoardUpdate = (data: any) => {
  console.log("SignalR: Otrzymano 'BoardUpdated'. Odświeżam stan planszy.", data)
  fetchPawns()
  fetchRivalPawns()
}

const onHistoryUpdate = () => {
  console.log("SignalR: Otrzymano 'HistoryUpdated'. Odświeżam historię.")
  if (playerMenuRef.value) {
    playerMenuRef.value.fetchGameLog()
  }
}

let isSignalRInitialized = false

watch(
  () => props.teamToken,
  async (newToken) => {
    if (!newToken) return
    await fetchGameDataByToken(newToken)
    if (gameData.value?.gameId && !isSignalRInitialized) {
      isSignalRInitialized = true
      try {
        await signalrService.start()
        await signalrService.joinGameRoom(String(gameData.value.gameId))
        console.log(`SignalR: Połączono i dołączono do pokoju gry ${gameData.value.gameId}`)
        signalrService.connection.on('BoardUpdated', onBoardUpdate)
        signalrService.connection.on('HistoryUpdated', onHistoryUpdate)
      } catch (err) {
        console.error('Błąd połączenia SignalR w playerView: ', err)
      }
    }
  },
  { immediate: true },
)

onMounted(() => {
  console.log('PlayerView zamontowany.')
})

onUnmounted(() => {
  if (gameData.value?.gameId) {
    console.log(`SignalR: Opuszczanie pokoju gry ${gameData.value.gameId}`)
    signalrService.leaveGameRoom(String(gameData.value.gameId))
    signalrService.connection.off('BoardUpdated', onBoardUpdate)
    signalrService.connection.off('HistoryUpdated', onHistoryUpdate)
  }
})
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
