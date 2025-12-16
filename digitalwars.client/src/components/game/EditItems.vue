<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <!-- Nagłówek -->
    <div class="text-center">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-white mb-2">
        {{ t('editItems') }}
      </h1>
      <p class="text-surface-400 text-sm md:text-base">{{ t('manageItemsInTheDeck') }}</p>
    </div>

    <div class="max-w-6xl mx-auto w-full space-y-6">
      <!-- Sekcja wyboru talii -->
      <div class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl">
        <!-- Nagłówek -->
        <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
          <div class="bg-primary-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faLayerGroup" class="h-6 text-primary-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-white">{{ t('deckSelection') }}</h2>
        </div>

        <!-- Wybór talii -->
        <div>
          <label for="deck-select" class="block mb-2 text-sm font-semibold text-gray-300">
            {{ t('selectDeck') }}
          </label>
          <Dropdown
            id="deck-select"
            v-model="selectedDeckId"
            :options="decksData"
            optionLabel="title"
            optionValue="id"
            :placeholder="t('selectDeckPlaceholder')"
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
              {{ t('deckName') }}
            </label>

            <InputText
              id="deck-name"
              v-model="deckName"
              class="w-full"
              :placeholder="t('deckNamePlaceholder')"
            />
          </div>

          <!-- Przycisk -->
          <div class="flex items-end">
            <Button :label="t('changeName')" class="w-full" @click="handleSaveDeckName" />
          </div>
        </div>
      </div>

      <!-- Sekcja wyboru przedmiotu -->
      <div
        v-if="selectedDeckId"
        class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl"
      >
        <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
          <div class="bg-green-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faMicrochip" class="h-6 text-green-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-white">{{ t('items') }}</h2>
        </div>

        <div v-if="isLoadingItems" class="text-center py-8">
          <ProgressSpinner style="width: 3rem; height: 3rem" strokeWidth="4" />
          <p class="text-surface-400 mt-3">{{ t('loadingItems') }}</p>
        </div>

        <div v-else>
          <label for="item-select" class="block mb-2 text-sm font-semibold text-gray-300">
            {{ t('selectItem') }}:
          </label>
          <Dropdown
            id="item-select"
            v-model="selectedItem"
            :options="itemsData"
            optionLabel="shortDesc"
            optionValue="id"
            :placeholder="t('selectItemPlaceholder')"
            class="w-full"
          >
            <template #value="slotProps">
              <div v-if="slotProps.value" class="flex items-center gap-2">
                <span class="text-green-400">#{{ slotProps.value }}</span>
                <span>{{ itemsData.find((i) => i.id === slotProps.value)?.shortDesc }}</span>
              </div>
              <span v-else class="text-surface-400">{{ slotProps.placeholder }}</span>
            </template>
            <template #option="slotProps">
              <div class="flex items-center gap-2">
                <span class="text-green-400">#{{ slotProps.option.id }}</span>
                <span>{{ slotProps.option.shortDesc }}</span>
              </div>
            </template>
          </Dropdown>
        </div>
      </div>

      <!-- Sekcja edycji przedmiotu -->
      <div
        v-if="selectedItem && currentItem"
        class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl"
      >
        <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
          <div class="bg-blue-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faPenToSquare" class="h-6 text-blue-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-white">{{ t('itemsEdition') }}</h2>
        </div>

        <form @submit.prevent="handleSaveItem" class="space-y-5">
          <!-- Tytuł przedmiotu -->
          <div>
            <label for="item-title" class="block mb-2 text-sm font-semibold text-gray-300">
              {{ t('itemName') }}
            </label>
            <InputText
              id="item-title"
              v-model="currentItem.shortDesc"
              :placeholder="t('itemNamePlaceholder')"
              class="w-full"
            />
          </div>

          <!-- Opis przedmiotu -->
          <div>
            <label for="item-description" class="block mb-2 text-sm font-semibold text-gray-300">
              {{ t('itemDescription') }}
            </label>
            <Textarea
              id="item-description"
              v-model="currentItem.longDesc"
              rows="8"
              :placeholder="t('itemDescriptionPlaceholder')"
              class="w-full"
            />
          </div>

          <!-- Przycisk zapisu -->
          <div class="flex justify-center px-4">
            <Button
              type="submit"
              :disabled="isSaving"
              :loading="isSaving"
              :label="isSaving ? t('saving') : t('saveChanges')"
              size="large"
              class="w-full"
            >
            </Button>
          </div>
        </form>
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
        <p class="text-surface-400 text-sm font-medium">{{ t('selectDeckToEditItems') }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useToast } from 'vue-toastification'
import { faLayerGroup, faMicrochip, faPenToSquare } from '@fortawesome/free-solid-svg-icons'
import Dropdown from 'primevue/dropdown'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'
import ProgressSpinner from 'primevue/progressspinner'
import { useI18n } from 'vue-i18n'
import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'

const { t } = useI18n()

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

const selectedDeckId = ref<number | undefined>(undefined)
const selectedItem = ref<number | undefined>(undefined)

// --- Stan komponentu z jawnymi typami ---
const decksData = ref<Deck[]>([])
const itemsData = ref<Item[]>([])
const currentItem = ref<Item | null>(null)
const deckName = ref<string>('')

const isLoadingDecks = ref(true)
const isLoadingItems = ref(false)
const isSaving = ref(false)

const handleSaveDeckName = async () => {
  if (deckName.value.trim() === '') {
    toast.warning(t('deckNameCannotBeEmpty'))
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
    toast.error(t('errorSavingDeckName') + error)
  }
}

// --- Pobieranie danych z API ---
const fetchDecks = async () => {
  isLoadingDecks.value = true
  try {
    const response = await apiService.get(apiConfig.admin.deck.getAll)
    decksData.value = response.data as Deck[]
  } catch (error) {
    toast.error(t('errorFetchingDecks') + error)
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
    toast.error(t('errorFetchingItems') + error)
    console.error('Błąd pobierania przedmiotów:', error)
  } finally {
    isLoadingItems.value = false
  }
}

// --- Logika zapisu ---
const handleSaveItem = async () => {
  if (!currentItem.value) {
    toast.warning(t('noItemSelected'))
    return
  }
  isSaving.value = true
  try {
    console.log(currentItem.value.id, 'ID')
    await apiService.put(apiConfig.admin.deck.updateItem(currentItem.value.id), {
      cardId: currentItem.value.id,
      shortDesc: currentItem.value.shortDesc,
      longDesc: currentItem.value.longDesc,
    })

    const index = itemsData.value.findIndex((item) => item.id === currentItem.value!.id)
    if (index !== -1) {
      itemsData.value[index] = { ...currentItem.value }
    }
  } catch (error: unknown) {
    let errorMessage = 'Wystąpił nieoczekiwany błąd.'
    if (typeof error === 'object' && error !== null && 'response' in error) {
      const err = error as { response?: { data?: { message?: string } } }
      errorMessage = err.response?.data?.message || 'Wystąpił błąd serwera.'
    }
    toast.error(t('errorSavingItem') + ' ' + errorMessage)
  } finally {
    isSaving.value = false
  }
}

// --- Obserwatorzy zmian ---
watch(selectedDeckId, (newDeckId) => {
  selectedItem.value = undefined
  currentItem.value = null

  if (newDeckId !== undefined) {
    fetchItemsForDeck(newDeckId)
  } else {
    itemsData.value = []
  }
  const deck = decksData.value.find((deck) => deck.id === newDeckId)
  if (deck) {
    deckName.value = deck.title
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
