<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <!--Nagłówek-->
    <div class="text-center">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-white mb-2">
        {{ t('dynamicCheatSheet') }}
      </h1>
      <p class="text-surface-400 text-sm md:text-base">{{ t('lookHowTeamsAreDoing') }}</p>
    </div>

    <!--Ściąga-->
    <div
      class="border border-surface-700 rounded-xl p-5 bg-gradient-to-br from-surface-900 to-surface-800 shadow-2xl"
    >
      <div class="flex items-center justify-between mb-5 pb-4 border-b border-surface-700">
        <div class="flex items-center gap-3">
          <div class="bg-primary-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faDiagramProject" class="h-6 text-primary-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-white">Drzewo decyzji</h2>
        </div>
        <div class="flex gap-2 bg-surface-800 border border-surface-700 p-2 rounded-lg">
          <Button @click="zoomIn()" outlined rounded size="small" v-tooltip.top="t('zoomIn')">
            <template #icon>
              <font-awesome-icon :icon="faMagnifyingGlassPlus" class="h-4" />
            </template>
          </Button>
          <Button @click="zoomOut()" outlined rounded size="small" v-tooltip.top="t('zoomOut')">
            <template #icon>
              <font-awesome-icon :icon="faMagnifyingGlassMinus" class="h-4" />
            </template>
          </Button>
          <Button
            @click="changeLayout('LR')"
            outlined
            rounded
            size="small"
            v-tooltip.top="t('leftRightLayout')"
          >
            <template #icon>
              <font-awesome-icon :icon="faArrowsLeftRight" class="h-4" />
            </template>
          </Button>

          <Button
            @click="changeLayout('TB')"
            outlined
            rounded
            size="small"
            v-tooltip.top="t('topBottomLayout')"
          >
            <template #icon>
              <font-awesome-icon :icon="faArrowsUpDown" class="h-4" />
            </template>
          </Button>

          <Button @click="fitView()" outlined rounded size="small" v-tooltip.top="t('fitView')">
            <template #icon>
              <font-awesome-icon :icon="faExpand" class="h-4" />
            </template>
          </Button>

          <Button
            @click="layoutGraph(currentLayout)"
            outlined
            rounded
            size="small"
            v-tooltip.top="t('resetView')"
          >
            <template #icon>
              <font-awesome-icon :icon="faRotate" class="h-4" />
            </template>
          </Button>
        </div>
      </div>

      <div class="h-[60vh]">
        <VueFlow
          :nodes="nodes"
          :edges="edges"
          :node-types="nodeTypes"
          :nodes-connectable="false"
          :nodes-draggable="true"
          @nodes-initialized="layoutGraph(currentLayout)"
        >
          <Background pattern-color="#4b5563" :gap="16" />
          <Panel
            position="bottom-left"
            class="bg-surface-800/90 backdrop-blur-sm rounded-lg p-4 border border-surface-700 min-w-[200px]"
          >
            <div class="space-y-4">
              <div>
                <h3 class="text-white font-bold text-sm mb-2 flex items-center gap-2">
                  <font-awesome-icon :icon="faPeopleLine" class="text-xl text-primary-400"/>
                  {{ t('tables') }}
                </h3>
                <div class="flex flex-col gap-2">
                  <div 
                    v-for="table in tables" 
                    :key="table.teamId" 
                    class="flex gap-2 items-center hover:bg-surface-700/50 p-1 rounded transition-colors"
                  >
                    <div
                      :style="{ backgroundColor: table.teamColor }"
                      class="w-4 h-4 rounded-full ring-1 ring-surface-600"
                    ></div>
                    <span class="text-surface-200 text-sm">{{ table.teamName }}</span>
                  </div>
                </div>
              </div>

              <hr class="border-surface-700" />

              <div>
                <h3 class="text-white font-bold text-sm mb-2 flex items-center gap-2">
                   <font-awesome-icon :icon="faCircleInfo" class="text-xl text-primary-400"/>
                  Legenda
                </h3>
                <div class="flex flex-col gap-2">
                  <div class="flex gap-2 items-center">
                    <div
                      class="bg-blue-400 border-2 border-surface-700 h-6 w-6 rounded-md flex items-center justify-center font-nasalization font-bold text-white shadow-sm"
                    >
                      X
                    </div>
                    <span class="text-surface-200 text-sm">Karta Decyzji</span>
                  </div>
                  <div class="flex gap-2 items-center">
                    <div class="text-primary-400 text-lg">{{ `-->` }}</div>
                    <span class="text-surface-200 text-sm">Przejście między kartami</span>
                  </div>
                </div>
              </div>
            </div>
          </Panel>
        </VueFlow>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, nextTick, markRaw, shallowRef } from 'vue'
import { useToast } from 'vue-toastification'
import apiServices from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'
import {
  type IGameResponse,
  type ITeamManagmentResponse,
  type ICardTypes,
  type IEnablersMapResponse,
  type IDecisonCard,
  type IItemCard,
} from '@/types/Game'
import type { ICardNode, ICardEdge } from '@/types/Nodes'
import { Background } from '@vue-flow/background'
import { useI18n } from 'vue-i18n'
import CustomNode from './CustomNode.vue'

import {
  faDiagramProject,
  faMagnifyingGlassPlus,
  faMagnifyingGlassMinus,
  faArrowsUpDown,
  faArrowsLeftRight,
  faExpand,
  faRotate,
  faPeopleLine,
  faCircleInfo
} from '@fortawesome/free-solid-svg-icons'
import Button from 'primevue/button'
import { VueFlow, MarkerType, useVueFlow, Panel } from '@vue-flow/core'
// @ts-ignore
import { useLayout } from '@/composables/useLayout'
import { useStorage } from '@vueuse/core'
const { layout } = useLayout()
const { fitView, zoomIn, zoomOut } = useVueFlow()

const { t } = useI18n()

interface IProps {
  gameId: number
}

const props = defineProps<IProps>();
const toast = useToast();

const gameId = ref<number>(props.gameId);
const tables = ref<ITeamManagmentResponse[]>([]);
const deckId = ref<number>();
const latestEntry = ref<number>();
const currentLayout = useStorage<'TB' | 'LR'>('prefferedLayour', 'TB');
const decisionCardsData = ref<IDecisonCard[]>([]);
const itemsData = ref<IItemCard[]>([]);
const tableEntries = ref<Record<number, Array<{teamId: number, teamColor: string, teamName: string}>>>({});

const nodeTypes = markRaw({
  decision: CustomNode,
} as any)

//Dane do ściągi
const nodes = ref<ICardNode[]>([])
const cardTypes = ref<ICardTypes[]>([])
const enablers = ref<Record<number, number[]>>()
const edges = ref<ICardEdge[]>()

async function layoutGraph(direction: 'LR' | 'TB') {
  nodes.value = nodes.value.map((node) => ({
    ...node,
    data: {
      ...node.data,
      layoutDirection: direction,
    },
  }))

  nodes.value = layout(nodes.value, edges.value, direction)

  nextTick(() => {
    fitView()
  })
}

const changeLayout = (newLayout: 'LR' | 'TB') => {
  layoutGraph(newLayout)
  currentLayout.value = newLayout
}

const createNodesFromCards = (cardTypes: ICardTypes[]) => {
  const decisionCards = cardTypes.filter((card) => card.cardType === 'Decision')

  const decisionCardsMap = new Map(
    decisionCardsData.value.map((decision) => [decision.id, decision]),
  )

  nodes.value = decisionCards.map((card) => ({
    id: `card-${card.card_Id}`,
    type: 'decision',
    position: { x: 0, y: 0 },
    data: {
      label: card.card_Id.toString(),
      cardType: card.cardType,
      tables: tableEntries.value[card.card_Id] ?? [],
      card: decisionCardsMap.get(card.card_Id)!,
      layoutDirection: currentLayout.value,
    },
  }))
}

const createEdgesFromEnablers = (
  enablersMap: Record<number, number[]>,
  cardTypes: ICardTypes[],
) => {
  const decisionCardsIds = cardTypes
    .filter((card) => card.cardType === 'Decision')
    .map((card) => card.card_Id)

  edges.value = Object.entries(enablersMap).flatMap(([targetCardId, enablerCardIds]) => {
    const targetId: number = Number(targetCardId)

    if (!decisionCardsIds.includes(targetId)) return []

    return enablerCardIds
      .filter((sourceCardId) => decisionCardsIds.includes(sourceCardId))
      .map((sourceCardId) => ({
        id: `e${sourceCardId}-${targetCardId}`,
        source: `card-${sourceCardId}`,
        target: `card-${targetCardId}`,
        markerEnd: {
          type: MarkerType.ArrowClosed,
          color: '#a78bfa',
        },
        style: {
          stroke: '#a78bfa',
          strokeWidth: 1.5,
        },
        class: 'flow-edge',
      }))
  })
  console.log('Nowo powstałe krawędzie:', edges.value)
}

//Pobieranie id talii kart
const fetchDeckId = async () => {
  try {
    const response = await apiServices.get<IGameResponse>(apiConfig.games.getById(gameId.value))
    deckId.value = response.data.deckId
  } catch {
    toast.error('Wystąpił błąd podczas pobierania id talii kart')
  }
}

//Pobieranie informacji o drużynach w grze
const fetchTemasInfo = async () => {
  try {
    const response = await apiServices.get<ITeamManagmentResponse[]>(
      apiConfig.player.getTeamsManagement(gameId.value),
    )

    tables.value = response.data
    console.log('Stoły:', tables.value);
  } catch {
    toast.error(`Wystąpił błąd podczas pobierania informacji o drużynach w grze: ${gameId.value}`)
  }
}

//Mapa eneblerów i typy kart
const fetchEnablersMap = async () => {
  try {
    const response = await apiServices.get<IEnablersMapResponse>(
      apiConfig.admin.cheatsheet.getMap(deckId.value!),
    )

    enablers.value = response.data.enablersMap
    cardTypes.value = response.data.cardTypes

    console.log(response.data, 'response')
  } catch {
    toast.error('Wystąpił błąd podczas pobierania enablerów')
  }
}

//Ostatnia zagrana karta z sukcesem przez każdą drużynę 
const fetchAllTeamsEntries = async () => {
  const entriesPromises = tables.value.map(async (table) => {
    try {
      const response = await apiServices.get<number>(
        apiConfig.admin.cheatsheet.getEntreis(gameId.value, table.teamId),
      )
      return {
        cardsId: response.data,
        teamId: table.teamId,
        teamColor: table.teamColor,
        teamName: table.teamName
      }
    } catch {
      console.error(`Błąd podczas pobierania entries dla zespołu ${table.teamId}`)
      return null
    }
  })

  const results = await Promise.all(entriesPromises)

  console.log('Rezultaty:', results)
  
  results.forEach(result => {
    if (!result) return

    const card = cardTypes.value.find(card => card.cards_Id === result.cardsId)
    
    if (card) {
      const cardId = card.card_Id
      
      if (!tableEntries.value[cardId]) {
        tableEntries.value[cardId] = []
      }
      
      tableEntries.value[cardId].push({
        teamId: result.teamId,
        teamColor: result.teamColor,
        teamName: result.teamName
      })
    }
  })

  console.log('Team entries:', tableEntries.value)
}

const fetchDecisionCards = async () => {
  try {
    const response = await apiServices.get<IDecisonCard[]>(
      apiConfig.admin.deck.cards(deckId.value!),
    )

    decisionCardsData.value = response.data

    console.log('Karty decyzji:', decisionCardsData.value)
  } catch {
    toast.error('Błąd podczas pobierania kart decyzji')
  }
}

const fetchItems = async () => {
  try {
    const response = await apiServices.get<IItemCard[]>(apiConfig.admin.deck.items(deckId.value!))

    itemsData.value = response.data

    console.log('Pobrane karty przedmiotów:', itemsData.value)
  } catch {
    toast.error('Błąd podczas pobierania przedmiotów')
  }
}

onMounted(async () => {
  await Promise.all([fetchDeckId(), fetchTemasInfo()])

  await Promise.all([fetchEnablersMap(), fetchDecisionCards(), fetchItems(), fetchAllTeamsEntries()])

  createNodesFromCards(cardTypes.value)
  createEdgesFromEnablers(enablers.value!, cardTypes.value)
})
</script>

<style>
.flow-edge path.vue-flow__edge-path {
  stroke: #a78bfa;
  stroke-width: 1;
  filter: drop-shadow(0 0 4px rgba(167, 139, 250, 0.5));
}

.flow-edge path.vue-flow__edge-path {
  stroke-dasharray: 8 8;
  animation: flow 1s linear infinite;
  stroke-linecap: round;
}

@keyframes flow {
  to {
    stroke-dashoffset: -16;
  }
}
</style>
