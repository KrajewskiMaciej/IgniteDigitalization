<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <!-- Nagłówek -->
    <div class="text-center">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-white mb-2"></h1>
      <p class="text-surface-400 text-sm md:text-base">Zarządzaj kartami w talii</p>
    </div>

    <div class="max-w-6xl mx-auto w-full space-y-6">
      <!-- Przycisk importu pliku -->
      <div class="text-center">
        <input
          type="file"
          accept=".xls,.xlsx"
          ref="fileInput"
          @change="handleFileChange"
          style="display: none"
        />
        <div class="flex gap-2 justify-center">
          <Button
            @click="triggerFileInput"
            severity="success"
            size="large"
            label="Wczytaj talię z pliku Excel"
          >
            <template #icon>
              <font-awesome-icon :icon="faFileExcel" class="mr-2" />
            </template>
          </Button>

          <Button @click="handleDownloadTemplate" size="large" label="Pobierz szablon kart">
            <template #icon>
              <font-awesome-icon :icon="faDownload" class="mr-2" />
            </template>
          </Button>
        </div>
      </div>

      <!-- Sekcja wyboru talii -->
      <div class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl">
        <!-- Nagłówek -->
        <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
          <div class="bg-primary-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faLayerGroup" class="h-6 text-primary-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-white">Wybór talii</h2>
        </div>

        <!-- Wybór talii -->
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

        <!-- Edycja nazwy -->
        <div
          v-if="deckName"
          class="mt-6 p-4 bg-surface-800 border border-surface-700 rounded-lg grid grid-cols-4 gap-4"
        >
          <!-- Pole inputa -->
          <div class="md:col-span-3 flex flex-col">
            <label for="deck-name" class="mb-2 text-sm font-semibold text-gray-300">
              Nazwa talii
            </label>

            <InputText
              id="deck-name"
              v-model="deckName"
              class="w-full"
              placeholder="Wpisz nową nazwę talii..."
            />
          </div>

          <!-- Przycisk -->
          <div class="flex items-end">
            <Button label="Zmień nazwę" class="w-full" @click="handleSaveDeckName" />
          </div>
        </div>
      </div>

      <!-- Grid z dwiema sekcjami -->
      <div v-if="selectedDeckId" class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Sekcja edycji karty -->
        <div class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl">
          <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
            <div class="bg-blue-500/20 p-3 rounded-lg">
              <font-awesome-icon :icon="faPenToSquare" class="h-6 text-blue-400" />
            </div>
            <h2 class="text-xl md:text-2xl font-bold text-white">Edycja karty</h2>
          </div>

          <div v-if="isLoadingCards" class="text-center py-8">
            <ProgressSpinner style="width: 3rem; height: 3rem" strokeWidth="4" />
            <p class="text-surface-400 mt-3">Ładowanie kart...</p>
          </div>

          <div v-else class="space-y-5">
            <div>
              <label for="card-select" class="block mb-2 text-sm font-semibold text-gray-300">
                Wybierz kartę:
              </label>
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
                  <div v-if="slotProps.value" class="flex items-center gap-2">
                    <span class="text-blue-400">#{{ slotProps.value }}</span>
                    <span>{{ cardsData.find((c) => c.id === slotProps.value)?.title }}</span>
                  </div>
                  <span v-else class="text-surface-400">{{ slotProps.placeholder }}</span>
                </template>
                <template #option="slotProps">
                  <div class="flex items-center gap-2">
                    <span class="text-blue-400">#{{ slotProps.option.id }}</span>
                    <span>{{ slotProps.option.title }}</span>
                  </div>
                </template>
              </Dropdown>
            </div>

            <form v-if="selectedCardId && currentCard" @submit.prevent="saveCard" class="space-y-5">
              <!-- Tytuł karty -->
              <div>
                <label for="title" class="block mb-2 text-sm font-semibold text-gray-300">
                  Tytuł karty:
                </label>
                <InputText
                  id="title"
                  v-model="currentCard.title"
                  placeholder="Wprowadź tytuł karty..."
                  class="w-full"
                />
              </div>

              <!-- Opis karty -->
              <div>
                <label for="description" class="block mb-2 text-sm font-semibold text-gray-300">
                  Opis karty:
                </label>
                <Textarea
                  id="description"
                  v-model="currentCard.description"
                  rows="12"
                  placeholder="Szczegółowy opis karty..."
                  class="w-full"
                />
              </div>

              <!-- Przycisk zapisu -->
              <div class="flex justify-center">
                <Button
                  type="submit"
                  :disabled="isSavingCard"
                  :loading="isSavingCard"
                  :label="isSavingCard ? 'Zapisywanie...' : 'Zapisz Kartę'"
                  size="large"
                  class="w-full"
                />
              </div>
            </form>
          </div>
        </div>

        <!-- Sekcja edycji feedbacku -->
        <div
          v-if="currentCard && selectedCardId"
          class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl"
        >
          <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
            <div class="bg-primary-500/20 p-3 rounded-lg">
              <font-awesome-icon :icon="faComment" class="h-6 text-primary-400" />
            </div>
            <h2 class="text-xl md:text-2xl font-bold text-white">Edycja feedbacku</h2>
          </div>

          <div class="space-y-5">
            <div>
              <label for="feedback-select" class="block mb-2 text-sm font-semibold text-gray-300">
                Wybierz feedback:
              </label>
              <Dropdown
                id="feedback-select"
                v-model="selectedFeedbackId"
                :options="feedbacksData"
                optionLabel="feedbacks_Long_Description"
                optionValue="feedbacks_Id"
                placeholder="Wybierz feedback..."
                class="w-full"
              >
                <template #value="slotProps">
                  <div v-if="slotProps.value" class="flex items-center gap-2">
                    <div
                      class="flex items-center justify-center w-6 h-6 rounded p-2"
                      :class="
                        feedbacksData.find((f) => f.feedbacks_Id === slotProps.value)?.status ===
                        'positive'
                          ? 'bg-green-500/20'
                          : 'bg-red-500/20'
                      "
                    >
                      <font-awesome-icon
                        :icon="
                          feedbacksData.find((f) => f.feedbacks_Id === slotProps.value)?.status ===
                          'positive'
                            ? faCircleCheck
                            : faCircleXmark
                        "
                        :class="
                          feedbacksData.find((f) => f.feedbacks_Id === slotProps.value)?.status ===
                          'positive'
                            ? 'text-green-400'
                            : 'text-red-400'
                        "
                      />
                    </div>
                    <span>
                      {{
                        feedbacksData
                          .find((f) => f.feedbacks_Id === slotProps.value)
                          ?.feedbacks_Long_Description.trim() === ''
                          ? 'Brak opisu'
                          : feedbacksData.find((f) => f.feedbacks_Id === slotProps.value)
                              ?.feedbacks_Long_Description
                      }}
                    </span>
                  </div>
                  <span v-else class="text-surface-400">{{ slotProps.placeholder }}</span>
                </template>
                <template #option="slotProps">
                  <div class="flex items-center gap-2">
                    <div
                      class="flex items-center justify-center w-6 h-6 rounded p-2"
                      :class="
                        slotProps.option.status === 'positive' ? 'bg-green-500/20' : 'bg-red-500/20'
                      "
                    >
                      <font-awesome-icon
                        :icon="
                          slotProps.option.status === 'positive' ? faCircleCheck : faCircleXmark
                        "
                        :class="
                          slotProps.option.status === 'positive' ? 'text-green-400' : 'text-red-400'
                        "
                      />
                    </div>
                    <span>{{
                      slotProps.option.feedbacks_Long_Description.trim() === ''
                        ? 'Brak opisu'
                        : truncateString(slotProps.option.feedbacks_Long_Description, 55)
                    }}</span>
                  </div>
                </template>
              </Dropdown>
            </div>

            <form
              v-if="selectedFeedbackId && selectedFeedback"
              @submit.prevent="saveFeedback"
              class="space-y-5"
            >
              <!-- Opis feedbacku -->
              <div>
                <label
                  for="feedbackDescription"
                  class="block mb-2 text-sm font-semibold text-gray-300"
                >
                  Opis feedbacku:
                </label>
                <Textarea
                  id="feedbackDescription"
                  v-model="selectedFeedback.feedbacks_Long_Description"
                  rows="8"
                  placeholder="Szczegółowy opis feedbacku..."
                  class="w-full"
                />
              </div>

              <!-- Przycisk zapisu -->
              <div class="flex justify-center">
                <Button
                  type="submit"
                  :disabled="isSavingFeedback"
                  :loading="isSavingFeedback"
                  :label="isSavingFeedback ? 'Zapisywanie...' : 'Zapisz Feedback'"
                  size="large"
                  class="w-full"
                />
              </div>
            </form>
          </div>
        </div>
      </div>

      <!-- Placeholder gdy brak wybranej talii -->
      <div
        v-if="!selectedDeckId"
        class="text-center py-12 border border-dashed border-surface-700 rounded-xl bg-surface-900/50"
      >
        <div
          class="bg-surface-800/50 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-3"
        >
          <font-awesome-icon :icon="faLayerGroup" class="h-10 text-surface-600" />
        </div>
        <p class="text-surface-400 text-sm font-medium">Wybierz talię aby zarządzać kartami</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {
  faPenToSquare,
  faFileExcel,
  faLayerGroup,
  faComment,
  faDownload,
  faCircleCheck,
  faCircleXmark,
} from '@fortawesome/free-solid-svg-icons'
import { ref, watch, onMounted, computed } from 'vue'
import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'
import { useToast } from 'vue-toastification'
import Dropdown from 'primevue/dropdown'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'
import ProgressSpinner from 'primevue/progressspinner'
import apiServices from '@/services/apiServices'
import type { IFeedback, IFeedbacksResponse } from '@/types/Feedbacks'
import { truncateString } from '@/composables/truncateString'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

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

// --- ZMIENNE REAKTYWNE ---
const toast = useToast()
const selectedDeckId = ref<number | undefined>(undefined)
const selectedCardId = ref<number | undefined>(undefined)
const selectedFeedbackId = ref<number | undefined>(undefined)
const feedbacksData = ref<IFeedback[]>([])

const fileInput = ref<HTMLInputElement | null>(null)

const decksData = ref<Deck[]>([])
const deckName = ref<string>('')
const cardsData = ref<Card[]>([])

const currentCard = ref<Card | null>(null)
const selectedFeedback = computed<IFeedback | null>(() => {
  if (!selectedFeedbackId.value) return null
  return feedbacksData.value.find((f) => f.feedbacks_Id === selectedFeedbackId.value) ?? null
})

const isLoadingDecks = ref(true)
const isLoadingCards = ref(false)
const isSavingCard = ref(false)
const isSavingFeedback = ref(false)

// --- FUNKCJE ---
function triggerFileInput(): void {
  fileInput.value?.click()
}

const fetchFeedbacks = async () => {
  if (!currentCard.value?.id) return

  try {
    const response = await apiService.get<IFeedbacksResponse>(
      apiConfig.admin.deck.getFeedbacks(currentCard.value.id),
    )

    const data = response.data

    console.log('Pobrane procesy:', data)

    const mapped: IFeedback[] = [
      data.negativeFeedback ? { ...data.negativeFeedback, status: 'negative' as const } : undefined,

      data.positiveFeedback ? { ...data.positiveFeedback, status: 'positive' as const } : undefined,
    ].filter((f): f is IFeedback => f !== undefined)

    feedbacksData.value = mapped
  } catch (error) {
    toast.error('Wystąpił błąd podczas pobierania feedbacków dla karty')
    console.error('Błąd pobierania feedbacków:', error)
  }
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

const handleDownloadTemplate = async () => {
  try {
    const response = await apiServices.getFile(apiConfig.admin.deck.getCardsTemplate)
    console.log('Co otrzymałem w odpowiedzi ?', response.data)

    const file = response.data

    const url = window.URL.createObjectURL(file)

    const link = document.createElement('a')
    link.href = url
    link.download = 'DigitalWars_SzablonKart.xlsx'
    link.click()

    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('Błąd przy pobieraniu szablonu kart:', error)
    toast.error('Nie udało się pobrać szablonu kart.')
  }
}

const handleSaveDeckName = async () => {
  if (deckName.value.trim() === '') {
    toast.warning('Nazwa talii kart nie może być pusta')
    return
  }
  try {
    const response = await apiService.put(apiConfig.admin.deck.updateDeckName, {
      decks_Id: selectedDeckId.value,
      decks_Name: deckName.value,
    })

    console.log('Odpowiedź:', response)
    const deck = decksData.value.find((deck) => deck.id === selectedDeckId.value)
    if (deck) {
      deck.title = deckName.value
    }
  } catch (error) {
    toast.error('Błąd podczas aktualizacji nazwy talii')
  }
}

async function saveCard(): Promise<void> {
  if (!currentCard.value) return
  isSavingCard.value = true
  try {
    const response = await apiService.put(apiConfig.admin.deck.updateCard, {
      cardId: currentCard.value.id,
      shortDesc: currentCard.value.title,
      longDesc: currentCard.value.description,
    })

    const cardToUpdated = cardsData.value.find((card) => card.id === currentCard.value?.id)

    if (cardToUpdated) {
      cardToUpdated.title = currentCard.value.title
      cardToUpdated.description = currentCard.value.description
    }
  } catch (error) {
    toast.error('Nie udało się zapisać karty')
    console.error('Błąd zapisu karty:', error)
  } finally {
    isSavingCard.value = false
  }
}

async function saveFeedback(): Promise<void> {
  if (!selectedFeedback.value) return
  isSavingFeedback.value = true
  try {
    console.log('Feedbacki:', feedbacksData.value)
    const negativeDescription = feedbacksData.value[0].feedbacks_Long_Description
    const positiveDescription = feedbacksData.value[1].feedbacks_Long_Description
    apiServices.put(apiConfig.admin.deck.updateFeedbacks(selectedCardId.value!), {
      positiveDescription: positiveDescription,
      negativeDescription: negativeDescription,
    })
  } catch (error) {
    toast.error('Nie udało się zapisać feedbacku')
    console.error('Błąd zapisu feedbacku:', error)
  } finally {
    isSavingFeedback.value = false
  }
}

async function fetchDecks(): Promise<void> {
  isLoadingDecks.value = true
  try {
    const response = await apiService.get(apiConfig.admin.deck.getAll)
    decksData.value = response.data as Deck[]
  } catch (error) {
    console.error('Błąd przy pobieraniu talii:', error)
    toast.error('Nie udało się pobrać dostępnych talii.')
  } finally {
    isLoadingDecks.value = false
  }
}

// --- WATCHERY ---
watch(selectedDeckId, async (newDeckId) => {
  selectedCardId.value = undefined
  currentCard.value = null

  if (!newDeckId) {
    cardsData.value = []
    return
  }

  isLoadingCards.value = true
  try {
    const url = apiConfig.admin.deck.cards(newDeckId)
    const response = await apiService.get(url)
    cardsData.value = response.data as Card[]
  } catch (error) {
    console.error('Błąd przy pobieraniu kart z talii:', error)
    toast.error('Nie udało się pobrać kart dla wybranej talii.')
    cardsData.value = []
  } finally {
    isLoadingCards.value = false
  }

  const deck = decksData.value.find((deck) => deck.id === newDeckId)
  console.log('Znaleziona talia kart:', deck)
  if (deck) {
    deckName.value = deck.title
  }
})

watch(selectedCardId, (newCardId) => {
  selectedFeedbackId.value = undefined

  if (newCardId) {
    const card = cardsData.value.find((c) => c.id === newCardId)
    currentCard.value = card ? { ...card } : null
  } else {
    currentCard.value = null
  }

  fetchFeedbacks()
})

// --- CYKL ŻYCIA KOMPONENTU ---
onMounted(fetchDecks)
</script>
