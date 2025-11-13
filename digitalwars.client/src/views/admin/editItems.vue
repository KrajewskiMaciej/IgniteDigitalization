<template>
  <div class="w-full flex">
    <div
      class="flex flex-col flex-1 justify-center items-center m-4 px-2 py-2 border-2 border-lgray-accent rounded-md bg-tertiary"
    >
      <h1 class="text-3xl font-nasalization text-white mt-5">Edycja Przedmiotów</h1>

      <div v-if="isLoadingDecks" class="text-center text-gray-400 mt-10">Ładowanie talii...</div>

      <form v-else class="w-full max-w-lg mt-4 flex flex-col items-center">
        <div class="w-full mb-4">
          <label class="block text-white mb-1">Wybierz talię:</label>
          <dropDown
            :items="decksData"
            v-model="selectedDeck"
            item-key="id"
            :display-format="(deck: Deck) => `#${deck.id} ${deck.title}`"
            placeholder="Wybierz talię..."
          />
        </div>

        <div v-if="isLoadingItems" class="text-center text-gray-400 mt-4">
          Ładowanie przedmiotów...
        </div>

        <div v-else-if="selectedDeck" class="w-full">
          <label class="block text-white mb-1">Wybierz przedmiot:</label>
          <dropDown
            :items="itemsData"
            v-model="selectedItem"
            item-key="id"
            :display-format="(item: Item) => `#${item.id} ${item.shortDesc}`"
            placeholder="Wybierz przedmiot..."
          />
        </div>

        <div v-if="selectedItem && currentItem" class="mt-6 space-y-4 text-white w-full">
          <div class="flex flex-col">
            <label for="title" class="mb-1 font-semibold">Tytuł przedmiotu:</label>
            <input
              id="title"
              v-model="currentItem.shortDesc"
              type="text"
              class="bg-primary border-2 border-lgray-accent rounded-md px-3 py-2 text-white w-full focus:border-accent focus:ring-accent"
            />
          </div>

          <div class="flex flex-col">
            <label for="description" class="mb-1 font-semibold">Opis przedmiotu:</label>
            <textarea
              id="description"
              v-model="currentItem.longDesc"
              rows="8"
              class="bg-primary border-2 border-lgray-accent rounded-md px-3 py-2 text-white resize-y w-full focus:border-accent focus:ring-accent"
            ></textarea>
          </div>
          <div class="flex justify-center w-full">
            <button
              type="button"
              @click="handleSave"
              :disabled="isSaving"
              class="bg-accent hover:bg-opacity-80 transition-colors duration-200 border-2 border-accent py-3 px-6 rounded-md mt-5 disabled:bg-gray-500 disabled:cursor-not-allowed"
            >
              <font-awesome-icon :icon="faSave" class="h-4 mr-2" />
              {{ isSaving ? 'Zapisywanie...' : 'Zapisz' }}
            </button>
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
import dropDown from '@/components/dropDown.vue'

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
  itemsData.value = [] // Wyczyść listę przed pobraniem nowych
  try {
    // WAŻNE: Upewnij się, że ten endpoint istnieje w Twoim pliku apiConfig
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
    // WAŻNE: Upewnij się, że ten endpoint istnieje w Twoim pliku apiConfig
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
  // Resetuj wybór przedmiotu przy zmianie talii
  selectedItem.value = undefined
  currentItem.value = null

  if (newDeckId !== undefined) {
    fetchItemsForDeck(newDeckId)
  } else {
    itemsData.value = [] // Wyczyść listę, jeśli żadna talia nie jest wybrana
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
