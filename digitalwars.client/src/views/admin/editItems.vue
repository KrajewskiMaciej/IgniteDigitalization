<template>
  <div class="w-full flex">
    <div
      class="flex flex-col flex-1 items-center m-4 px-4 py-6 border-2 border-surface-700 rounded-lg bg-tertiary"
    >
      <h1 class="text-3xl font-nasalization text-white mb-6">Edycja Przedmiotów</h1>

      <div v-if="isLoadingDecks" class="text-center text-gray-400 mt-10">Ładowanie talii...</div>

      <form v-else class="w-full max-w-lg mt-4 flex flex-col items-center space-y-4">
        <div class="w-full">
          <label for="deck-select" class="block text-white mb-2 font-medium">Wybierz talię:</label>
          <Dropdown
            id="deck-select"
            v-model="selectedDeck"
            :options="decksData"
            optionLabel="title"
            optionValue="id"
            placeholder="Wybierz talię..."
            class="w-full"
          >
            <template #value="slotProps">
              <span v-if="slotProps.value">
                #{{ slotProps.value }} {{ decksData.find(d => d.id === slotProps.value)?.title }}
              </span>
              <span v-else>{{ slotProps.placeholder }}</span>
            </template>
            <template #option="slotProps">
              <span>#{{ slotProps.option.id }} {{ slotProps.option.title }}</span>
            </template>
          </Dropdown>
        </div>

        <div v-if="isLoadingItems" class="text-center text-gray-400 mt-4">
          Ładowanie przedmiotów...
        </div>

        <div v-else-if="selectedDeck" class="w-full">
          <label for="item-select" class="block text-white mb-2 font-medium">Wybierz przedmiot:</label>
          <Dropdown
            id="item-select"
            v-model="selectedItem"
            :options="itemsData"
            optionLabel="shortDesc"
            optionValue="id"
            placeholder="Wybierz przedmiot..."
            class="w-full"
          >
            <template #value="slotProps">
              <span v-if="slotProps.value">
                #{{ slotProps.value }} {{ itemsData.find(i => i.id === slotProps.value)?.shortDesc }}
              </span>
              <span v-else>{{ slotProps.placeholder }}</span>
            </template>
            <template #option="slotProps">
              <span>#{{ slotProps.option.id }} {{ slotProps.option.shortDesc }}</span>
            </template>
          </Dropdown>
        </div>

        <div v-if="selectedItem && currentItem" class="mt-6 space-y-4 w-full">
          <div class="flex flex-col">
            <label for="title" class="block text-white mb-2 font-medium">Tytuł przedmiotu:</label>
            <InputText
              id="title"
              v-model="currentItem.shortDesc"
              class="w-full"
            />
          </div>

          <div class="flex flex-col">
            <label for="description" class="block text-white mb-2 font-medium">Opis przedmiotu:</label>
            <Textarea
              id="description"
              v-model="currentItem.longDesc"
              rows="8"
              class="w-full"
            />
          </div>

          <div class="flex justify-center w-full px-4">
            <Button
              type="button"
              @click="handleSave"
              :disabled="isSaving"
              :loading="isSaving"
              :label="isSaving ? 'Zapisywanie...' : 'Zapisz'"
              class="mt-5 w-full"
            >
            </Button>
          </div>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useToast } from 'vue-toastification'
import { faSave } from '@fortawesome/free-solid-svg-icons'
import Dropdown from 'primevue/dropdown'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'

import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'

// --- Definicje interfejsów ---
interface Deck {
  id: number
  title: string
}
interface Item {
  id: number
  deckId: number
  shortDesc: string
  longDesc: string
  type: 'Hardware' | 'Software'
}

const toast = useToast()

const selectedDeck = ref<number | undefined>(undefined)
const selectedItem = ref<number | undefined>(undefined)

// --- Stan komponentu z jawnymi typami ---
const decksData = ref<Deck[]>([])
const itemsData = ref<Item[]>([])
const currentItem = ref<Item | null>(null)

const isLoadingDecks = ref(true)
const isLoadingItems = ref(false)
const isSaving = ref(false)

// --- Pobieranie danych z API ---
const fetchDecks = async () => {
  isLoadingDecks.value = true
  try {
    const response = await apiService.get(apiConfig.admin.deck.getAll)
    decksData.value = response.data as Deck[]
  } catch (error) {
    toast.error('Nie udało się pobrać listy talii.')
    console.error('Błąd pobierania talii:', error)
  } finally {
    isLoadingDecks.value = false
  }
}

const fetchItemsForDeck = async (deckId: number) => {
  isLoadingItems.value = true
  itemsData.value = []
  try {
    const response = await apiService.get(apiConfig.admin.deck.items(deckId))
    itemsData.value = response.data as Item[]
  } catch (error) {
    toast.error('Nie udało się pobrać listy przedmiotów dla wybranej talii.')
    console.error('Błąd pobierania przedmiotów:', error)
  } finally {
    isLoadingItems.value = false
  }
}

// --- Logika zapisu ---
const handleSave = async () => {
  if (!currentItem.value) {
    toast.warning('Brak przedmiotu do zapisania.')
    return
  }
  isSaving.value = true
  try {
    await apiService.put(apiConfig.admin.deck.updateItem(currentItem.value.id), currentItem.value)

    const index = itemsData.value.findIndex((item) => item.id === currentItem.value!.id)
    if (index !== -1) {
      itemsData.value[index] = { ...currentItem.value }
    }

    toast.success('Przedmiot został pomyślnie zaktualizowany.')
  } catch (error: unknown) {
    let errorMessage = 'Wystąpił nieoczekiwany błąd.'
    if (typeof error === 'object' && error !== null && 'response' in error) {
      const err = error as { response?: { data?: { message?: string } } }
      errorMessage = err.response?.data?.message || 'Wystąpił błąd serwera.'
    }
    toast.error(`Zapis nie powiódł się: ${errorMessage}`)
    console.error('Błąd zapisu przedmiotu:', error)
  } finally {
    isSaving.value = false
  }
}

// --- Obserwatorzy zmian ---
watch(selectedDeck, (newDeckId) => {
  selectedItem.value = undefined
  currentItem.value = null

  if (newDeckId !== undefined) {
    fetchItemsForDeck(newDeckId)
  } else {
    itemsData.value = []
  }
})

watch(selectedItem, (newValue) => {
  if (newValue !== undefined) {
    const item = itemsData.value.find((item) => item.id === newValue)
    if (item) {
      currentItem.value = { ...item }
    }
  } else {
    currentItem.value = null
  }
})

onMounted(async () => {
  await fetchDecks()
})
</script>