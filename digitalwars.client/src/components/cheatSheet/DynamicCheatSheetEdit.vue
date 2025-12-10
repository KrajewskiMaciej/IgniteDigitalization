<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <!-- Nagłówek -->
    <div class="text-center">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-white mb-2">
        Enablery kart
      </h1>
      <p class="text-surface-400 text-sm md:text-base">
        Zobacz jak wygląda ścieżka gry danej talii kart i ją zedytuj
      </p>
    </div>

    <div class="max-w-6xl mx-auto w-full">
      <div class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl">
        <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
          <div class="bg-primary-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faLayerGroup" class="h-6 text-primary-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-white">Wybór talii</h2>
        </div>

        <div>
          <label for="deck-select" class="block mb-2 text-sm font-semibold text-gray-300">
            Wybierz talię kart:
          </label>
          <Dropdown
            id="deck-select"
            v-model="selectedDeckId"
            :options="decksData"
            optionLabel="title"
            optionValue="id"
            placeholder="Wybierz talię..."
            class="w-full"
            :disabled="isLoadingDecks"
          />
        </div>
      </div>
    </div>

    <div
      v-if="selectedDeckId"
      class="border border-surface-700 rounded-xl p-5 bg-gradient-to-br from-surface-900 to-surface-800 shadow-2xl"
    >
      <div class="flex items-center justify-between mb-5 pb-4 border-b border-surface-700">
        <div class="flex items-center gap-3">
          <div class="bg-primary-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faDiagramProject" class="h-6 text-primary-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-white">Drzewo decyzji</h2>
        </div>
        <div v-if="isEditMode">
          <span class="font-nasalization font-bold text-4xl text-primary-400">Tryb edycji</span>
        </div>
        <div class="flex gap-2 bg-surface-800 border border-surface-700 p-2 rounded-lg">
          <Button
            v-if="!isEditMode"
            @click="handleStartEditing"
            outlined
            rounded
            size="small"
            v-tooltip.top="t('editMode')"
          >
            <template #icon>
              <font-awesome-icon :icon="faPencil" class="h-4" />
            </template>
          </Button>
          <Button
            v-if="isEditMode"
            @click="handleSaveChanges"
            outlined
            rounded
            size="small"
            v-tooltip.top="t('saveChanges')"
          >
            <template #icon>
              <font-awesome-icon :icon="faCheck" class="h-4 text-green-500" />
            </template>
          </Button>
          <Button
            v-if="isEditMode"
            @click="handleRejectChanges"
            outlined
            rounded
            size="small"
            v-tooltip.top="t('rejectChanges')"
          >
            <template #icon>
              <font-awesome-icon :icon="faX" class="h-4 text-red-500" />
            </template>
          </Button>
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
          :class="{ 'edit-mode': isEditMode }"
          :nodes="nodes"
          :edges="edges"
          :node-types="nodeTypes"
          :nodes-connectable="isEditMode"
          :nodes-draggable="true"
          @nodes-initialized="layoutGraph(currentLayout)"
          @connect="onConnect"
          @edge-click="onEdgeClick"
        >
          <Background pattern-color="#4b5563" :gap="16" />
        </VueFlow>
      </div>
    </div>

    <div
      v-else
      class="max-w-6xl mx-auto w-full text-center py-12 border border-dashed border-surface-700 rounded-xl bg-surface-900/50"
    >
      <div
        class="bg-surface-800/50 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-3"
      >
        <font-awesome-icon :icon="faLayerGroup" class="h-10 text-surface-600" />
      </div>
      <p class="text-surface-400 text-sm font-medium">Wybierz talię aby zarządzać enablerami</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, nextTick, markRaw, shallowRef, watch } from 'vue'
import { useToast } from 'vue-toastification'
import apiServices from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'
import {
  type ICardTypes,
  type IEnablersMapResponse,
  type IDecisonCard,
  type IItemCard,
  type IPendingEnablerChange,
} from '@/types/Game'
import type { ICardNode, ICardEdge } from '@/types/Nodes'
import { Background } from '@vue-flow/background'
import { useI18n } from 'vue-i18n'
import CustomNode from './CustomNode.vue'
import Dropdown from 'primevue/dropdown'
import {
  faDiagramProject,
  faMagnifyingGlassPlus,
  faMagnifyingGlassMinus,
  faArrowsUpDown,
  faArrowsLeftRight,
  faExpand,
  faRotate,
  faLayerGroup,
  faPencil,
  faCheck,
  faX,
} from '@fortawesome/free-solid-svg-icons'
import Button from 'primevue/button'
import { VueFlow, MarkerType, useVueFlow } from '@vue-flow/core'
// @ts-ignore
import { useLayout } from '@/composables/useLayout'
import { useStorage } from '@vueuse/core'

interface Deck {
  id: number
  title: string
}

const { layout } = useLayout()
const { fitView, zoomIn, zoomOut } = useVueFlow()
const { t } = useI18n()
const toast = useToast()

const selectedDeckId = ref<number>()
const decksData = ref<Deck[]>([])
const isLoadingDecks = ref<boolean>(false)
const currentLayout = useStorage<'TB' | 'LR'>('prefferedLayour', 'TB')
const decisionCardsData = ref<IDecisonCard[]>([])
const itemsData = ref<IItemCard[]>([])
const isCreatingNodes = ref<boolean>(false)
const isCreatingEdges = ref<boolean>(false)
const isEditMode = ref<boolean>(false)

const nodeTypes = markRaw({
  decision: CustomNode,
} as any)

const nodes = ref<ICardNode[]>([])
const cardTypes = ref<ICardTypes[]>([])
const enablers = ref<Record<number, number[]>>({})
const edges = ref<ICardEdge[]>([])

const edgesCopy = shallowRef<ICardEdge[]>([])
const enablersCopy = ref<Record<number, number[]>>({})
const pendingEnablersChanges = ref<Map<number, IPendingEnablerChange>>(new Map())

const getCardsId = (cardId: number): number | undefined => {
  return cardTypes.value.find((card) => card.card_Id === cardId)?.cards_Id
}

const markCardAsChanged = (targetCardId: number) => {
  const cardsId = getCardsId(targetCardId)
  if (!cardsId) return

  pendingEnablersChanges.value.set(targetCardId, {
    cardId: targetCardId,
    cardsId,
    enablers: [...(enablers.value[targetCardId] || [])],
  })

  console.log('Pending changes:', Array.from(pendingEnablersChanges.value.values()))
}

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

const handleStartEditing = () => {
  isEditMode.value = true
  edgesCopy.value = JSON.parse(JSON.stringify(edges.value))
  enablersCopy.value = JSON.parse(JSON.stringify(enablers.value))
  pendingEnablersChanges.value.clear()
}

const handleSaveChanges = async () => {
  const changes = Array.from(pendingEnablersChanges.value.values())

  if (changes.length === 0) {
    isEditMode.value = false
    return
  }

  console.log('Jakie mam zmiany do zrobienia ?', changes);

  for (const change of changes) {
    const enablerCardsIds = change.enablers.map((cardId : number) => getCardsId(cardId)).filter((id): id is number => id !== undefined)

    try {
      await apiServices.put(apiConfig.admin.cheatsheet.editCardsEnablers(change.cardsId),  enablerCardsIds )
    } catch {
      toast.error(`Wystąpił błąd podczas edycji enablera karty ${change.cardId}`);
      return;
    }
  }

  pendingEnablersChanges.value.clear()
  isEditMode.value = false
}

const handleRejectChanges = () => {
  edges.value = JSON.parse(JSON.stringify(edgesCopy.value))
  enablers.value = JSON.parse(JSON.stringify(enablersCopy.value))
  pendingEnablersChanges.value.clear()
  isEditMode.value = false
}

const onConnect = (newConnection: any) => {
  if (!isEditMode.value) return

  const sourceCardId = Number(newConnection.source.replace('card-', ''))
  const targetCardId = Number(newConnection.target.replace('card-', ''))

  if (!enablers.value[targetCardId]) enablers.value[targetCardId] = []

  if (enablers.value[targetCardId].includes(sourceCardId)) return

  enablers.value[targetCardId].push(sourceCardId)
  markCardAsChanged(targetCardId)

  const edgeId = `e${newConnection.source}-${newConnection.target}`
  const newEdge: ICardEdge = {
    id: edgeId,
    source: newConnection.source,
    target: newConnection.target,
    markerEnd: {
      type: MarkerType.ArrowClosed,
      color: '#a78bfa',
    },
    style: {
      stroke: '#a78bfa',
      strokeWidth: 1.5,
    },
    class: 'flow-edge',
  }

  edges.value = [...edges.value, newEdge]
}

const onEdgeClick = (event: { edge: { id: string; source: string; target: string } }) => {
  if (!isEditMode.value) return

  const sourceCardId = Number(event.edge.source.replace('card-', ''))
  const targetCardId = Number(event.edge.target.replace('card-', ''))

  if (enablers.value[targetCardId]) {
    enablers.value[targetCardId] = enablers.value[targetCardId].filter((id) => id !== sourceCardId)
  }

  markCardAsChanged(targetCardId)

  edges.value = edges.value.filter((e) => e.id !== event.edge.id)
}

const createNodesFromCards = (cardTypes: ICardTypes[]) => {
  try {
    isCreatingNodes.value = true
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
        card: decisionCardsMap.get(card.card_Id)!,
        layoutDirection: currentLayout.value,
      },
    }))
  } catch {
    toast.error('Błąd podczas generowania węzłów')
  } finally {
    isCreatingNodes.value = false
  }
}

const createEdgesFromEnablers = (
  enablersMap: Record<number, number[]>,
  cardTypes: ICardTypes[],
) => {
  try {
    isCreatingEdges.value = true
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
  } catch {
    toast.error('Błąd podczas generowania krawędzi pomiędzy węzłami')
  } finally {
    isCreatingEdges.value = false
  }
}

async function fetchDecks(): Promise<void> {
  isLoadingDecks.value = true
  try {
    const response = await apiServices.get(apiConfig.admin.deck.getAll)
    decksData.value = response.data as Deck[]
  } catch (error) {
    console.error('Błąd przy pobieraniu talii:', error)
    toast.error('Nie udało się pobrać dostępnych talii.')
  } finally {
    isLoadingDecks.value = false
  }
}

const fetchEnablersMap = async (deckId: number) => {
  try {
    const response = await apiServices.get<IEnablersMapResponse>(
      apiConfig.admin.cheatsheet.getMap(deckId),
    )

    enablers.value = response.data.enablersMap
    cardTypes.value = response.data.cardTypes
  } catch {
    toast.error('Wystąpił błąd podczas pobierania enablerów')
  }
}

const fetchDecisionCards = async (deckId: number) => {
  try {
    const response = await apiServices.get<IDecisonCard[]>(apiConfig.admin.deck.cards(deckId))
    decisionCardsData.value = response.data
  } catch {
    toast.error('Błąd podczas pobierania kart decyzji')
  }
}

const fetchItems = async (deckId: number) => {
  try {
    const response = await apiServices.get<IItemCard[]>(apiConfig.admin.deck.items(deckId))
    itemsData.value = response.data
  } catch {
    toast.error('Błąd podczas pobierania przedmiotów')
  }
}

watch(selectedDeckId, async (newSelectedDeckId) => {
  if (newSelectedDeckId) {
    await Promise.all([
      fetchDecisionCards(newSelectedDeckId),
      fetchItems(newSelectedDeckId),
      fetchEnablersMap(newSelectedDeckId),
    ])

    createNodesFromCards(cardTypes.value)
    createEdgesFromEnablers(enablers.value, cardTypes.value)
    layoutGraph(currentLayout.value)
  }
})

onMounted(async () => {
  fetchDecks()
})
</script>

<style>
.flow-edge path.vue-flow__edge-path {
  stroke: #a78bfa;
  stroke-width: 1;
  filter: drop-shadow(0 0 4px rgba(167, 139, 250, 0.5));
  stroke-dasharray: 8 8;
  animation: flow 1s linear infinite;
  stroke-linecap: round;
}

@keyframes flow {
  to {
    stroke-dashoffset: -16;
  }
}

.vue-flow.edit-mode .flow-edge path.vue-flow__edge-path {
  animation: none;
  stroke-dasharray: none;
}

.vue-flow.edit-mode .vue-flow__edge {
  cursor: pointer;
}

.vue-flow.edit-mode .vue-flow__edge:hover path.vue-flow__edge-path {
  stroke: red !important;
}

.vue-flow.edit-mode .vue-flow__handle {
  opacity: 1;
  background: #22c55e;
  width: 12px;
  height: 12px;
}
</style>
