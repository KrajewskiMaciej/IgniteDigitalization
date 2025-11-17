<template>
  <div class="w-full flex gap-4 p-4">
    <!-- SEKCJA EDYCJI KART -->
    <div
      class="flex flex-col flex-1 items-center px-4 py-6 border border-surface-700 rounded-lg bg-tertiary"
    >
      <h1 class="text-3xl font-nasalization text-white mb-6">Edycja kart</h1>

      <input
        type="file"
        accept=".xls,.xlsx"
        ref="fileInput"
        @change="handleFileChange"
        style="display: none"
      />

      <Button
        @click="triggerFileInput"
        severity="success"
        class="mb-6"
        size="large"
        label="Wczytaj talię z pliku xls"
      >
        <template #icon>
          <font-awesome-icon :icon="faFileExcel" class="mr-2" />
        </template>
      </Button>

      <div class="w-full max-w-lg space-y-4">
        <div class="flex flex-col">
          <label for="deck-select" class="block text-white mb-2 font-medium">Wybierz talię:</label>
          <Dropdown
            id="deck-select"
            v-model="selectedDeckId"
            :options="decksData"
            optionLabel="title"
            optionValue="id"
            placeholder="Wybierz talię..."
            class="w-full"
          >
            <template #value="slotProps">
              <span v-if="slotProps.value">
                #{{ slotProps.value }} {{ decksData.find((d) => d.id === slotProps.value)?.title }}
              </span>
              <span v-else>{{ slotProps.placeholder }}</span>
            </template>
            <template #option="slotProps">
              <span>#{{ slotProps.option.id }} {{ slotProps.option.title }}</span>
            </template>
          </Dropdown>
        </div>

        <div v-if="selectedDeckId" class="flex flex-col">
          <label for="card-select" class="block text-white mb-2 font-medium">Wybierz kartę:</label>
          <Dropdown
            id="card-select"
            v-model="selectedCardId"
            :options="cardsData"
            optionLabel="title"
            optionValue="id"
            placeholder="Wybierz kartę..."
            class="w-full"
          >
            <template #value="slotProps">
              <span v-if="slotProps.value">
                #{{ slotProps.value }} {{ cardsData.find((c) => c.id === slotProps.value)?.title }}
              </span>
              <span v-else>{{ slotProps.placeholder }}</span>
            </template>
            <template #option="slotProps">
              <span>#{{ slotProps.option.id }} {{ slotProps.option.title }}</span>
            </template>
          </Dropdown>
        </div>

        <div v-if="selectedCardId && currentCard" class="mt-6 space-y-4">
          <div class="flex flex-col">
            <label for="title" class="block text-white mb-2 font-medium">Tytuł karty:</label>
            <InputText id="title" v-model="currentCard.title" class="w-full" />
          </div>

          <div class="flex flex-col">
            <label for="description" class="block text-white mb-2 font-medium">Opis karty:</label>
            <Textarea id="description" v-model="currentCard.description" rows="12" class="w-full" />
          </div>

          <div class="flex justify-center w-full px-4">
            <Button @click="saveCard" class="mt-4 w-full" label="Zapisz"> </Button>
          </div>
        </div>
      </div>
    </div>

    <!-- SEKCJA EDYCJI FEEDBACKU -->
    <div
      v-if="currentCard && selectedCardId"
      class="flex flex-col flex-1 items-center px-4 py-6 border-2 border-surface-700 rounded-lg bg-tertiary"
    >
      <h1 class="text-3xl font-nasalization text-white mb-6">Edycja feedbacku</h1>

      <div class="w-full max-w-lg space-y-4">
        <div class="flex flex-col">
          <label for="feedback-select" class="block text-white mb-2 font-medium"
            >Wybierz feedback:</label
          >
          <Dropdown
            id="feedback-select"
            v-model="selectedFeedbackId"
            :options="feedbackData"
            optionLabel="longDescription"
            optionValue="id"
            placeholder="Wybierz feedback..."
            class="w-full"
          >
            <template #value="slotProps">
              <span v-if="slotProps.value">
                {{
                  feedbackData.find((f) => f.id === slotProps.value)?.status === 'P' ? '✅' : '❌'
                }}
                {{
                  feedbackData
                    .find((f) => f.id === slotProps.value)
                    ?.longDescription.substring(0, 30)
                }}...
              </span>
              <span v-else>{{ slotProps.placeholder }}</span>
            </template>
            <template #option="slotProps">
              <span>
                {{ slotProps.option.status === 'P' ? '✅' : '❌' }}
                {{ slotProps.option.longDescription.substring(0, 30) }}...
              </span>
            </template>
          </Dropdown>
        </div>

        <div v-if="selectedFeedbackId && currentFeedback" class="flex flex-col">
          <label for="feedbackDescription" class="block text-white mb-2 font-medium"
            >Opis feedbacku:</label
          >
          <Textarea
            id="feedbackDescription"
            v-model="currentFeedback.longDescription"
            rows="8"
            class="w-full"
          />

          <div class="flex justify-center w-full px-4">
            <Button @click="saveFeedback" class="mt-4 w-full" label="Zapisz"> </Button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { faSave, faFileExcel } from '@fortawesome/free-solid-svg-icons'
import { reactive, ref, watch, onMounted } from 'vue'
import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'
import { useToast } from 'vue-toastification'
import Dropdown from 'primevue/dropdown'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'

// --- DEFINICJE INTERFEJSÓW ---
interface Deck {
  id: number
  title: string
}

interface Card {
  id: number
  deckId: number
  title: string
  description: string
}

interface Feedback {
  id: number
  longDescription: string
  status: 'P' | 'N'
}

// --- ZMIENNE REAKTYWNE ---
const toast = useToast()
const selectedDeckId = ref<number | undefined>(undefined)
const selectedCardId = ref<number | undefined>(undefined)
const selectedFeedbackId = ref<number | undefined>(undefined)

const fileInput = ref<HTMLInputElement | null>(null)

const decksData = reactive<Deck[]>([])
const cardsData = reactive<Card[]>([])
const feedbackData = reactive<Feedback[]>([
  { id: 1, longDescription: 'Przykładowy feedback negatywny dla tej karty.', status: 'N' },
  { id: 2, longDescription: 'Przykładowy feedback pozytywny dla tej karty.', status: 'P' },
])

const currentCard = ref<Card | null>(null)
const currentFeedback = ref<Feedback | null>(null)

// --- FUNKCJE ---
function triggerFileInput(): void {
  fileInput.value?.click()
}

async function handleFileChange(event: Event): Promise<void> {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]

  if (!file) return

  const formData = new FormData()
  formData.append('file', file)

  try {
    const response = await apiService.post(apiConfig.admin.deck.upload, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      withCredentials: true,
    })
    toast.success('Plik został pomyślnie wczytany i talia została utworzona.')
    await fetchDecks()
  } catch (error: any) {
    toast.error(`Błąd przy wysyłaniu pliku: ${error.response?.data?.message || error.message}`)
    console.error('Błąd przy wysyłaniu pliku:', error)
  }
}

async function saveCard(): Promise<void> {
  if (!currentCard.value) return
  // TODO: Implementacja logiki zapisu karty do API
  console.log('Zapisywanie karty:', currentCard.value)
  toast.success(`Zapisano kartę: ${currentCard.value.title}`)
}

async function saveFeedback(): Promise<void> {
  if (!currentFeedback.value) return
  // TODO: Implementacja logiki zapisu feedbacku do API
  console.log('Zapisywanie feedbacku:', currentFeedback.value)
  toast.success(`Zapisano feedback: ${currentFeedback.value.longDescription.substring(0, 30)}...`)
}

async function fetchDecks(): Promise<void> {
  try {
    const response = await apiService.get(apiConfig.admin.deck.getAll)
    decksData.length = 0
    decksData.push(...(response.data as Deck[]))
  } catch (error) {
    console.error('Błąd przy pobieraniu talii:', error)
    toast.error('Nie udało się pobrać dostępnych talii.')
  }
}

// --- WATCHERY ---
watch(selectedDeckId, async (newDeckId) => {
  selectedCardId.value = undefined
  currentCard.value = null

  if (!newDeckId) {
    cardsData.length = 0
    return
  }

  try {
    const url = apiConfig.admin.deck.cards(newDeckId)
    const response = await apiService.get(url)

    cardsData.length = 0
    cardsData.push(...(response.data as Card[]))
  } catch (error) {
    console.error('Błąd przy pobieraniu kart z talii:', error)
    toast.error('Nie udało się pobrać kart dla wybranej talii.')
    cardsData.length = 0
  }
})

watch(selectedCardId, (newCardId) => {
  selectedFeedbackId.value = undefined
  currentFeedback.value = null

  if (newCardId) {
    const card = cardsData.find((c) => c.id === newCardId)
    currentCard.value = card ? { ...card } : null
  } else {
    currentCard.value = null
  }
})

watch(selectedFeedbackId, (newFeedbackId) => {
  if (newFeedbackId) {
    const feedback = feedbackData.find((f) => f.id === newFeedbackId)
    currentFeedback.value = feedback ? { ...feedback } : null
  } else {
    currentFeedback.value = null
  }
})

// --- CYKL ŻYCIA KOMPONENTU ---
onMounted(fetchDecks)
</script>
