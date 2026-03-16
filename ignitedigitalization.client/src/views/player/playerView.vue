<template>
  <div
    class="flex flex-col h-screen bg-gradient-to-br from-surface-850 via-surface-900 to-surface-950 relative overflow-hidden"
  >
    <div
      class="absolute inset-0 bg-[radial-gradient(circle_at_30%_20%,_var(--tw-gradient-stops))] from-primary-900/15 via-transparent to-transparent pointer-events-none"
    ></div>

    <div class="relative z-10 flex flex-col h-full">
      <PlayerNavbar
        :team-name="gameData?.teamName || t('errorLoadingData')"
        :nav-bg-color="gameData?.teamColor || 'bg-secondary'"
      />

      <div v-if="gameStatusError" class="flex-1">
        <GameStatusDisplay :title="gameStatusError.title" :message="gameStatusError.message" />
      </div>

      <div v-else-if="gameData" class="flex-1 flex flex-col overflow-hidden">
        <div v-if="isMobile" class="flex flex-col h-full">
          <div
            class="flex gap-2 p-1 bg-secondary backdrop-blur-sm border-b border-surface-700 shadow-md"
          >
            <button
              @click="mobileView = 'cards'"
              class="flex-1 py-3 rounded-xl font-semibold text-sm transition-all duration-300 relative overflow-hidden group"
              :class="
                mobileView === 'cards'
                  ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                  : 'bg-secondary text-surface-300 hover:text-surface-0 border border-primary-500/30'
              "
            >
              <span class="relative z-10">{{ t('cards') }}</span>
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
                  : 'bg-secondary text-surface-300 hover:text-surface-0 border border-primary-500/30'
              "
            >
              <span class="relative z-10">{{ t('rivalBoard') }}</span>
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
                  : 'bg-secondary text-surface-300 hover:text-surface-0 border border-primary-500/30'
              "
            >
              <span class="relative z-10">{{ t('yourBoard') }}</span>
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
                  : 'bg-secondary text-surface-300 hover:text-surface-0 border border-primary-500/30'
              "
            >
              <span class="relative z-10">{{ t('menu') }}</span>
              <div
                v-if="mobileView !== 'menu'"
                class="absolute inset-0 bg-gradient-to-r from-primary-500/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
              ></div>
            </button>
          </div>

          <div class="flex-1 overflow-auto p-4">
            <div v-if="mobileView === 'cards'" class="h-full">
              <RouterView />

              <!-- Przełącznik trybu: QR / Lista kart -->
              <div class="flex gap-2 mb-4">
                <button
                  @click="cardMode = 'qr'"
                  class="flex-1 py-2 rounded-xl font-semibold text-sm transition-all duration-300"
                  :class="
                    cardMode === 'qr'
                      ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                      : 'bg-secondary text-surface-300 border border-primary-500/30'
                  "
                >
                  {{ t('switchToScanner') }}
                </button>
                <button
                  @click="cardMode = 'carousel'"
                  class="flex-1 py-2 rounded-xl font-semibold text-sm transition-all duration-300"
                  :class="
                    cardMode === 'carousel'
                      ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                      : 'bg-secondary text-surface-300 border border-primary-500/30'
                  "
                >
                  {{ t('switchToCardList') }}
                </button>
              </div>

              <!-- Tryb QR -->
              <QrCardScanner
                v-if="cardMode === 'qr'"
                :deck-id="gameData.deckId"
                :team-id="gameData.teamId"
                :game-id="gameData.gameId"
                :board-id="gameData.boardConfig?.boardId"
                :current-budget="currentGlobalBudget"
                :is-online-game="gameData.isOnline"
                :is-independent-team="gameData.isIndependent"
                @switch-to-carousel="cardMode = 'carousel'"
              />

              <!-- Tryb karuzelowy -->
              <template v-if="cardMode === 'carousel'">
                <div class="flex gap-2 my-4" v-if="cardCarouselRef?.hasItemCards">
                  <button
                    @click="showingDecisionCards = true"
                    class="flex-1 py-3 rounded-xl font-semibold transition-all duration-300"
                    :class="
                      showingDecisionCards
                        ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                        : 'bg-secondary text-surface-300 border border-primary-500/30'
                    "
                  >
                    {{ t('decisions') }}
                  </button>
                  <button
                    @click="showingDecisionCards = false"
                    class="flex-1 py-3 rounded-xl font-semibold transition-all duration-300"
                    :class="
                      !showingDecisionCards
                        ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                        : 'bg-secondary text-surface-300 border border-primary-500/30'
                    "
                  >
                    {{ t('items') }}
                  </button>
                </div>

                <Suspense>
                  <template #default>
                    <CardCarousel
                      ref="cardCarouselRef"
                      v-show="gameData && gameData.deckId"
                      :deck-id="gameData.deckId"
                      :team-id="gameData.teamId"
                      :game-id="gameData.gameId"
                      :board-id="gameData.boardConfig?.boardId"
                      :current-budget="currentGlobalBudget"
                      :showing-decision-cards="showingDecisionCards"
                      :is-online-game="gameData.isOnline"
                      :is-independent-team="gameData.isIndependent"
                    />
                  </template>
                  <template #fallback>
                    <div class="text-center text-surface-300">{{ t('loadingCards') }}</div>
                  </template>
                </Suspense>
              </template>
            </div>

            <div v-else-if="mobileView === 'board'" class="h-full">
              <GameBoard
                v-if="gameData?.rivalBoardConfig"
                :config="enemyformData"
                :gameMode="true"
                :pawns="enemypawns"
                :usePercentage="true"
              />
            </div>

            <div v-else-if="mobileView === 'market'" class="h-full">
              <CartesianBoard
                v-if="gameData?.boardConfig"
                :config="formData"
                :gameMode="true"
                :pawns="pawns"
              />
            </div>

            <div v-else-if="mobileView === 'menu'" class="h-full">
              <PlayerMenu
                ref="playerMenuRef"
                v-if="gameData"
                :game-id="gameData.gameId"
                :team-id="gameData.teamId"
                :current-phase-name="gameData.currentPhaseName ?? undefined"
                @budget-changed-in-menu="handleBudgetChangeFromMenu"
              />
            </div>
          </div>
        </div>

        <div v-else class="flex h-full relative">
          <div
            v-if="leftOpen"
            class="w-1/3 bg-secondary backdrop-blur-sm border-r border-surface-700 shadow-2xl overflow-auto p-4 transition-all duration-300"
          >
            <RouterView />
            <!-- <QuestionBox /> -->

            <!-- Przełącznik trybu: QR / Lista kart -->
            <div v-if="!isDesktop" class="flex gap-2 mb-4">
              <button
                @click="cardMode = 'qr'"
                class="flex-1 py-2 rounded-xl font-semibold text-sm transition-all duration-300"
                :class="
                  cardMode === 'qr'
                    ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                    : 'bg-secondary text-surface-300 border border-primary-500/30'
                "
              >
                {{ t('switchToScanner') }}
              </button>
              <button
                @click="cardMode = 'carousel'"
                class="flex-1 py-2 rounded-xl font-semibold text-sm transition-all duration-300"
                :class="
                  cardMode === 'carousel'
                    ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                    : 'bg-secondary text-surface-300 border border-primary-500/30'
                "
              >
                {{ t('switchToCardList') }}
              </button>
            </div>

            <!-- Tryb QR -->
            <QrCardScanner
              v-if="cardMode === 'qr' && !isDesktop"
              :deck-id="gameData.deckId"
              :team-id="gameData.teamId"
              :game-id="gameData.gameId"
              :board-id="gameData.boardConfig?.boardId"
              :current-budget="currentGlobalBudget"
              :is-online-game="gameData.isOnline"
              :is-independent-team="gameData.isIndependent"
              @switch-to-carousel="cardMode = 'carousel'"
            />

            <!-- Tryb karuzelowy -->
            <template v-if="cardMode === 'carousel' || isDesktop">
            <div class="flex gap-2 my-4" v-if="cardCarouselRef?.hasItemCards">
              <button
                @click="showingDecisionCards = true"
                class="flex-1 py-3 rounded-xl font-semibold transition-all duration-300"
                :class="
                  showingDecisionCards
                    ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                    : 'bg-secondary text-surface-300 border border-primary-500/30'
                "
              >
                {{ t('decisions') }}
              </button>
              <button
                @click="showingDecisionCards = false"
                class="flex-1 py-3 rounded-xl font-semibold transition-all duration-300"
                :class="
                  !showingDecisionCards
                    ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                    : 'bg-secondary text-surface-300 border border-primary-500/30'
                "
              >
                {{ t('items') }}
              </button>
            </div>

            <Suspense>
              <template #default>
                <CardCarousel
                  ref="cardCarouselRef"
                  v-if="gameData && gameData.deckId"
                  :deck-id="gameData.deckId"
                  :team-id="gameData.teamId"
                  :game-id="gameData.gameId"
                  :board-id="gameData.boardConfig?.boardId"
                  :current-budget="currentGlobalBudget"
                  :showing-decision-cards="showingDecisionCards"
                  :is-online-game="gameData.isOnline"
                  :is-independent-team="gameData.isIndependent"
                />
              </template>
              <template #fallback>
                <div class="text-center text-surface-300">{{ t('loadingCards') }}</div>
              </template>
            </Suspense>
            </template>
          </div>

          <div
            class="flex-1 flex flex-col bg-secondary backdrop-blur-sm shadow-xl p-4 transition-all duration-300"
          >
            <div class="flex justify-between items-center mb-6">
              <div class="flex gap-2">
                <button
                  @click="currentBoard = 'prep'"
                  class="px-6 py-3 rounded-xl font-semibold transition-all duration-300"
                  :class="
                    currentBoard === 'prep'
                      ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                      : 'bg-secondary text-surface-300 border border-primary-500/30'
                  "
                >
                  {{ t('rivalBoard') }}
                </button>
                <button
                  v-if="!isMarketPhaseOnly"
                  @click="currentBoard = 'market'"
                  class="px-6 py-3 rounded-xl font-semibold transition-all duration-300"
                  :class="
                    currentBoard === 'market'
                      ? 'bg-gradient-to-r from-primary-600 to-primary-700 text-surface-0 shadow-lg shadow-primary-500/50'
                      : 'bg-secondary text-surface-300 border border-primary-500/30'
                  "
                >
                  {{ t('yourBoard') }}
                </button>
              </div>

              <div class="flex gap-2">
                <button
                  @click="leftOpen = !leftOpen"
                  class="px-6 py-3 rounded-xl font-semibold bg-secondary text-surface-0 border border-primary-500/30 hover:border-primary-500/50 transition-all duration-300"
                >
                  {{ leftOpen ? t('hide') : t('show') }} {{ t('cards') }}
                </button>
                <button
                  @click="rightOpen = !rightOpen"
                  class="px-6 py-3 rounded-xl font-semibold bg-secondary text-surface-0 border border-primary-500/30 hover:border-primary-500/50 transition-all duration-300"
                >
                  {{ rightOpen ? t('hide') : t('show') }} {{ t('menu') }}
                </button>
              </div>
            </div>

            <div class="flex-1 overflow-auto">
              <!-- Plansza rynku: CartesianBoard (gameBoardCartesian) -->
              <CartesianBoard
                v-show="currentBoard === 'market' && gameData?.boardConfig"
                :config="formData"
                :gameMode="true"
                :pawns="pawns"
              />
              <!-- Plansza przygotowawcza: GameBoard (gameBoard) -->
              <GameBoard
                v-show="currentBoard === 'prep' && gameData?.rivalBoardConfig"
                :config="enemyformData"
                :gameMode="true"
                :pawns="enemypawns"
                :usePercentage="true"
              />
            </div>
          </div>

          <div
            v-if="rightOpen"
            class="w-1/4 bg-secondary backdrop-blur-sm border-l border-surface-700 shadow-2xl overflow-auto p-6 transition-all duration-300"
          >
            <PlayerMenu
              ref="playerMenuRef"
              v-show="currentPanel === 'menu' && gameData"
              :game-id="gameData.gameId"
              :team-id="gameData.teamId"
              :current-phase-name="gameData.currentPhaseName ?? undefined"
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
    <font-awesome-icon :icon="faCommentDots" class="h-10" />
  </div>
  <div
    v-if="showChat"
    class="fixed inset-0 z-50 bg-black bg-opacity-50 lg:flex lg:items-center lg:justify-start lg:pl-4"
  >
    <GameChat
      ref="chatRef"
      class="h-screen w-screen lg:h-3/4 lg:w-1/2 lg:max-w-2xl xl:w-1/5"
      @close-chat="showChat = false"
    />
  </div>

  <IndependentTeam
    @close="showIndependentTeamModal = false"
    :isVisible="showIndependentTeamModal"
    :teamName="gameData?.teamName!"
  />

  <NotIndependentTeam
    @close="showNotIndependentTeamModal = false"
    :isVisible="showNotIndependentTeamModal"
    :teamName="gameData?.teamName!"
  />

  <PhaseTwo
    @close="showPhaseTwoModal = false"
    :isVisible="showPhaseTwoModal"
    :teamName="gameData?.teamName!"
  />
</template>

<script setup lang="ts">
import { reactive, ref, computed, watch, onMounted, onUnmounted } from 'vue'
import PlayerNavbar from '@/components/navbars/playerNavbar.vue'
import QuestionBox from '@/components/playerComponents/questionBox.vue'
import GameBoard from '@/components/game/gameBoard.vue'
import CartesianBoard from '@/components/game/gameBoardCartesian.vue'
import Footer from '@/components/footers/myFooter.vue'
import CardCarousel from '@/components/playerComponents/CardCarousel.vue'
import PlayerMenu from '@/components/playerComponents/playerMenu.vue'
import apiConfig from '@/services/apiConfig'
import apiServices from '@/services/apiServices'
import signalrService from '@/services/signalService'
import GameStatusDisplay from '@/components/playerComponents/gameStatusDisplay.vue'
import GameChat from '@/components/game/gameChat.vue'
import IndependentTeam from '@/components/game/IndependentTeam.vue'
import NotIndependentTeam from '@/components/game/NotIndependentTeam.vue'
import PhaseTwo from '@/components/game/PhaseTwo.vue'
import QrCardScanner from '@/components/playerComponents/QrCardScanner.vue'
import type { BoardConfig, GameData, Pawn, RawPawnData, GameStatusError } from '@/interfaces/types'
import { useBreakpoints } from '@vueuse/core'
import { faCommentDots } from '@fortawesome/free-solid-svg-icons'
import { onClickOutside } from '@vueuse/core'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const chatRef = ref<HTMLElement | null>(null)

onClickOutside(chatRef, () => {
  showChat.value = false
})

const breakpoints = useBreakpoints({
  mobile: 768,
  desktop: 1280,
})

const isMobile = breakpoints.smaller('mobile')
const isDesktop = breakpoints.greaterOrEqual('desktop')

// --- PROPSY ---
const props = defineProps({
  teamToken: String,
})

// --- ZMIENNE STANU ---
const mobileView = ref('cards')
const showingDecisionCards = ref(true)
const currentPanel = ref('menu')
const leftOpen = ref(true)
const rightOpen = ref(true)
const currentBoard = ref('market') // 'market' = CartesianBoard (plansza rynku), 'prep' = GameBoard (plansza przygotowawcza)
const showChat = ref(false)
const cardMode = ref<'qr' | 'carousel'>('qr')
const showIndependentTeamModal = ref<boolean>(false)
const showNotIndependentTeamModal = ref<boolean>(false)
const showPhaseTwoModal = ref<boolean>(false)
const cardCarouselRef = ref<InstanceType<typeof CardCarousel> | null>(null)

import { fillBoardConfig } from '@/composables/BoardHelpers'

const gameData = ref<GameData | null>(null)
const isLoading = ref(true)
const errorLoading = ref<string | null>(null)

const currentGlobalBudget = ref(0)
const pawns = ref<Pawn[]>([])
const enemypawns = ref<Pawn[]>([])

const isMarketPhaseOnly = computed(() => gameData.value?.currentPhaseName !== 'Rynkowa')

const playerMenuRef = ref<{
  fetchGameLog: () => void
  fetchTeamBud: () => void
  handleFetchBudget: () => void
} | null>(null)

const createDefaultBoardConfig = (): BoardConfig => ({
  boardId: 0,
  name: t('loadingBoardName'),
  labelsUp: [],
  labelsRight: [],
  descriptionDown: '',
  descriptionLeft: '',
  rows: 8,
  cols: 8,
  cellColor: '#fefae0',
  borderColor: '#595959',
  borderColors: [], // <-- POPRAWIONA NAZWA
  cellsDescriptions: '',
})

const formData = reactive<BoardConfig>(createDefaultBoardConfig())
const enemyformData = reactive<BoardConfig>(createDefaultBoardConfig())
const gameStatusError = ref<GameStatusError | null>(null)

// --- FUNKCJE ---
const fetchGameDataByToken = async (token: string, skipIndependenceModal = false) => {
  if (!token) {
    gameStatusError.value = { title: t('errorLoadingData'), message: t('missingTeamToken') }
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

    // Ustaw domyślną planszę zależnie od fazy
    // isMarketPhaseOnly = true gdy faza NIE jest Rynkową (czyli faza przygotowawcza)
    currentBoard.value = isMarketPhaseOnly.value ? 'prep' : 'market'

    if (!skipIndependenceModal) {
      if (gameData.value.isIndependent) {
        showIndependentTeamModal.value = true
      } else {
        showNotIndependentTeamModal.value = true
      }
    }

    currentGlobalBudget.value = gameData.value.teamBudget

    const apiBoardConfig = gameData.value.boardConfig

    fillBoardConfig(formData, apiBoardConfig)

    if (gameData.value.rivalBoardConfig) {
      fillBoardConfig(enemyformData, gameData.value.rivalBoardConfig)
    } else {
      console.warn('Brak konfiguracji rivalBoardConfig.')
    }

    if (playerMenuRef.value) {
      playerMenuRef.value.fetchGameLog()
    }

    await fetchPawns()
    await fetchRivalPawns()
  } catch (err: any) {
    let title = t('errorLoadingData')
    let message = err.message || t('cannotLoadGameData')

    if (err.response) {
      const status = err.response.status
      const data = err.response.data

      if (status === 409 && data.errorCode) {
        switch (data.errorCode) {
          case 'GamePaused':
            title = t('gamePausedTitle')
            message = data.message || t('gamePausedMessage')
            break
          case 'GameEnded':
            title = t('gameEndedTitle')
            message = data.message || t('gameEndedMessage')
            break
        }
      } else {
        title = t('gameNotFoundTitle')
        message = data.message || t('gameNotFoundMessage')
      }
    }

    gameStatusError.value = { title, message }
    console.error('Błąd ładowania danych gry przez token:', err)
  } finally {
    isLoading.value = false
  }
}

const handleBudgetChangeFromMenu = (newBudgetFromMenu: number) => {
  currentGlobalBudget.value = newBudgetFromMenu
}

// Gdy faza się zmienia, przełącz planszę odpowiednio do fazy
watch(isMarketPhaseOnly, (isPrepPhase) => {
  currentBoard.value = isPrepPhase ? 'prep' : 'market'
  mobileView.value = 'market'
})

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
      maxX: Number(p.maxPosX) || 1,
      maxY: Number(p.maxPosY) || 1,
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
      maxX: Number(p.maxPosX) || 1,
      maxY: Number(p.maxPosY) || 1,
    }))
  } catch (err: any) {
    console.error('Błąd pobierania pionków rynku:', err)
  }
}

// --- LOGIKA SIGNALR ---
const onBoardUpdate = (data: any) => {
  fetchPawns()
  fetchRivalPawns()
}

const onHistoryUpdate = () => {
  if (playerMenuRef.value) {
    playerMenuRef.value.fetchGameLog()
    playerMenuRef.value.fetchTeamBud()
  }
}

const onPendingUpdate = () => {
  if (cardCarouselRef.value) {
    cardCarouselRef.value.fetchCards()
  }
}

const onBudgetUpdate = () => {
  if (playerMenuRef.value) {
    playerMenuRef.value.handleFetchBudget()
  }
}

const onPhaseUpdate = async () => {
  if (props.teamToken) {
    await fetchGameDataByToken(props.teamToken, true)
    currentBoard.value = 'market'
    mobileView.value = 'market'
    showPhaseTwoModal.value = true
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
        await signalrService.joinGameRoomAsPlayer(
          String(gameData.value.gameId),
          String(gameData.value.teamId),
        )
        signalrService.connection.on('BoardUpdated', onBoardUpdate)
        signalrService.connection.on('HistoryUpdated', onHistoryUpdate)
        signalrService.connection.on('PendingUpdated', onPendingUpdate)
        signalrService.connection.on('BudgetUpdated', onBudgetUpdate)
        signalrService.connection.on('PhaseUpdated', onPhaseUpdate)
      } catch (err) {
        console.error('Błąd połączenia SignalR w playerView: ', err)
      }
    }
  },
  { immediate: true },
)


onUnmounted(() => {
  if (gameData.value?.gameId) {
    signalrService.leaveGameRoomAsPlayer(
      String(gameData.value.gameId),
      String(gameData.value.teamId),
    )
    signalrService.connection.off('BoardUpdated', onBoardUpdate)
    signalrService.connection.off('HistoryUpdated', onHistoryUpdate)
    signalrService.connection.off('PendingUpdated', onPendingUpdate)
    signalrService.connection.off('BudgetUpdated', onBudgetUpdate)
    signalrService.connection.off('PhaseUpdated', onPhaseUpdate)
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
