<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <!-- Nagłówek -->
    <div class="text-center">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-white mb-2">
        Edycja Procesów
      </h1>
      <p class="text-surface-400 text-sm md:text-base">Zarządzaj procesami w talii kart</p>
    </div>

    <div class="max-w-6xl mx-auto w-full space-y-6">
      <!-- Sekcja wyboru talii -->
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
            v-model="selectedDeck"
            :options="decksData"
            optionLabel="title"
            optionValue="id"
            placeholder="Wybierz talię..."
            class="w-full"
          >
          </Dropdown>
        </div>
      </div>

      <!-- Sekcja wyboru procesu -->
      <div
        v-if="selectedDeck"
        class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl"
      >
        <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
          <div class="bg-primary-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faChessPawn" class="h-6 text-primary-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-white">Procesy</h2>
        </div>

        <div>
          <label for="process-select" class="block mb-2 text-sm font-semibold text-gray-300">
            Wybierz proces:
          </label>

          <div class="flex gap-2">
            <Dropdown
              id="process-select"
              v-model="selectedProcess"
              :options="processesData"
              optionLabel="processDesc"
              optionValue="processId"
              placeholder="Wybierz proces..."
              class="flex-1"
            >
              <template #value="slotProps">
                <div v-if="slotProps.value" class="flex items-center gap-2">
                  <div
                    class="w-4 h-4 rounded-full flex-shrink-0 border border-surface-600"
                    :style="`background-color: ${
                      processesData.find((p) => p.processId === slotProps.value)?.processColor ||
                      '#6B7280'
                    }`"
                  ></div>
                  <span>{{
                    processesData.find((p) => p.processId === slotProps.value)?.processDesc
                  }}</span>
                </div>
                <span v-else class="text-surface-400">{{ slotProps.placeholder }}</span>
              </template>
              <template #option="slotProps">
                <div class="flex items-center gap-2">
                  <div
                    class="w-4 h-4 rounded-full flex-shrink-0 border border-surface-600"
                    :style="`background-color: ${slotProps.option.processColor || '#6B7280'}`"
                  ></div>
                  <span>{{ slotProps.option.processDesc }}</span>
                </div>
              </template>
            </Dropdown>

            <Button
              type="button"
              @click="addNewProcess"
              severity="success"
              rounded
              v-tooltip.top="'Dodaj nowy proces'"
            >
              <template #icon>
                <font-awesome-icon :icon="faPlus" class="h-4" />
              </template>
            </Button>

            <Button
              type="button"
              @click="deleteSelectedProcess"
              :disabled="!selectedProcess"
              severity="danger"
              rounded
              v-tooltip.top="'Usuń wybrany proces'"
            >
              <template #icon>
                <font-awesome-icon :icon="faTrash" class="h-4" />
              </template>
            </Button>
          </div>
        </div>
      </div>

      <!-- Sekcja edycji procesu -->
      <div
        v-if="selectedProcess || isAddingNewProcess"
        class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl"
      >
        <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
          <div class="bg-blue-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faPenToSquare" class="h-6 text-blue-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-white">
            {{ isAddingNewProcess ? 'Nowy proces' : 'Edycja procesu' }}
          </h2>
        </div>

        <form @submit.prevent="saveProcessChanges" class="space-y-5">
          <!-- Skrót procesu -->
          <div>
            <label for="process-short" class="block mb-2 text-sm font-semibold text-gray-300">
              Skrót procesu:
            </label>
            <InputText
              id="process-short"
              v-model="editedProcess.processDesc"
              placeholder="Wprowadź skrót proces..."
              class="w-full"
              :minlength="2"
              :maxlength="25"
            />
          </div>

          <!-- Opis procesu -->
          <div>
            <label for="process-long" class="block mb-2 text-sm font-semibold text-gray-300">
              Opis procesu:
            </label>
            <Textarea
              id="process-long"
              v-model="editedProcess.processLongDesc"
              rows="3"
              :maxlength="75"
              placeholder="Szczegółowy opis procesu..."
              class="w-full"
            />
          </div>

          <!-- Kolor procesu -->
          <div>
            <div>
              <div class="flex flex-col items-center justify-center gap-4">
                <input
                  ref="colorPicker"
                  v-model="editedProcess.processColor"
                  type="color"
                  class="absolute opacity-0 pointer-events-none"
                />

                <div class="text-center">
                  <label class="block mb-2 text-sm font-semibold text-gray-300"
                    >Kolor procesu:</label
                  >
                  <p class="text-primary-400 text-sm mb-3">(Kliknij na pionek aby zmienić kolor)</p>
                </div>

                <div
                  class="bg-white rounded-lg inline-block border border-surface-700 hover:border-primary-400 transition-colors cursor-pointer"
                  @click="openColorPicker"
                >
                  <pawnPreview :pawnColor="editedProcess.processColor" class="w-32 h-32" />
                </div>

                <div class="flex items-center gap-3">
                  <span class="text-sm text-surface-400">Aktualny kolor:</span>
                  <span class="text-white text-sm">{{
                    editedProcess.processColor.toUpperCase()
                  }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Przycisk zapisu -->
          <div class="flex justify-center px-4">
            <Button
              type="submit"
              :disabled="!isProcessValid"
              :label="isAddingNewProcess ? 'Dodaj Nowy Proces' : 'Zapisz Zmiany'"
              size="large"
              class="w-full"
            >
            </Button>
          </div>
        </form>
      </div>

      <!-- Placeholder gdy brak wybranej talii -->
      <div
        v-if="!selectedDeck"
        class="text-center py-12 border border-dashed border-surface-700 rounded-xl bg-surface-900/50"
      >
        <div
          class="bg-surface-800/50 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-3"
        >
          <font-awesome-icon :icon="faLayerGroup" class="h-10 text-surface-600" />
        </div>
        <p class="text-surface-400 text-sm font-medium">Wybierz talię aby zarządzać procesami</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'
import { useToast } from 'vue-toastification'
import {
  faTrash,
  faPlus,
  faLayerGroup,
  faPenToSquare,
  faChessPawn,
} from '@fortawesome/free-solid-svg-icons'
import Dropdown from 'primevue/dropdown'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'
import { useConfirm } from 'primevue/useconfirm'
import apiConfig from '@/services/apiConfig'
import apiService from '@/services/apiServices'
import pawnPreview from '@/components/game/PawnPreview.vue'

const toast = useToast()
const confirm = useConfirm()

// --- INTERFACES ---
interface Deck {
  id: number
  title: string
}

interface Process {
  processId: number
  deckId: number
  processDesc: string
  processLongDesc: string
  processColor: string
}

// --- REACTIVE DATA ---
const selectedDeck = ref<number | null>(null)
const decksData = ref<Deck[]>([])

const processesData = ref<Process[]>([])
const selectedProcess = ref<number | null>(null)
const isAddingNewProcess = ref(false)
const editedProcess = ref({
  processDesc: '',
  processLongDesc: '',
  processColor: '#6B7280',
})

const colorPicker = ref<HTMLInputElement | null>(null)

// --- FUNCTIONS ---
const openColorPicker = () => {
  colorPicker.value?.click()
}

const addNewProcess = () => {
  if (!selectedDeck.value) {
    toast.error('Najpierw wybierz talię kart')
    return
  }

  editedProcess.value = {
    processDesc: '',
    processLongDesc: '',
    processColor: '#6B7280',
  }
  selectedProcess.value = null
  isAddingNewProcess.value = true
}

const deleteSelectedProcess = async () => {
  if (!selectedProcess.value) return

  const processId = selectedProcess.value
  const processToDelete = processesData.value.find((process) => process.processId === processId)

  if (!processToDelete) {
    toast.error('Nie znaleziono procesu do usunięcia')
    return
  }

  confirm.require({
    message: `Czy na pewno chcesz usunąć proces "${processToDelete.processDesc}"?`,
    header: 'Potwierdzenie usunięcia',
    rejectLabel: 'Anuluj',
    acceptLabel: 'Usuń',
    accept: async () => {
      try {
        // API call to delete process
        processesData.value = processesData.value.filter(
          (process) => process.processId !== processId,
        )
        selectedProcess.value = null
        editedProcess.value = {
          processDesc: '',
          processLongDesc: '',
          processColor: '#6B7280',
        }

        toast.success('Proces został pomyślnie usunięty')
      } catch (error) {
        console.error('Błąd przy usuwaniu procesu:', error)
        toast.error('Błąd podczas usuwania procesu')
      }
    },
  })
}

const saveProcessChanges = async () => {
  if (!isProcessValid.value) {
    toast.error('Proszę wprowadzić poprawne dane procesu')
    return
  }

  try {
    if (isAddingNewProcess.value) {
      // API call to add process
      const newProcess: Process = {
        ...editedProcess.value,
        processId: Date.now(),
        deckId: selectedDeck.value!,
      }
      processesData.value.push(newProcess)

      toast.success('Nowy proces został dodany')
    } else {
      // API call to update process
      const index = processesData.value.findIndex(
        (process) => process.processId === selectedProcess.value,
      )
      if (index !== -1) {
        processesData.value[index] = {
          ...editedProcess.value,
          processId: selectedProcess.value!,
          deckId: selectedDeck.value!,
        }
      }

      toast.success('Proces został zaktualizowany')
    }

    isAddingNewProcess.value = false
    selectedProcess.value = null
    editedProcess.value = {
      processDesc: '',
      processLongDesc: '',
      processColor: '#6B7280',
    }
  } catch (error) {
    console.error('Błąd przy zapisie procesu:', error)
    toast.error('Błąd podczas zapisywania procesu')
  }
}

// --- COMPUTED ---
const isProcessValid = computed(() => {
  return (
    editedProcess.value.processDesc.length >= 2 &&
    editedProcess.value.processDesc.length <= 25 &&
    editedProcess.value.processLongDesc.length >= 8 &&
    editedProcess.value.processLongDesc.length <= 75 &&
    editedProcess.value.processColor.trim() !== ''
  )
})

// --- WATCHERS ---
watch(selectedDeck, async (newDeck) => {
  if (newDeck) {
    processesData.value = []
    selectedProcess.value = null
    isAddingNewProcess.value = false

    try {
      const response = await apiService.get<Process[]>(apiConfig.processes.getByDeck(newDeck))
      processesData.value = response.data
    } catch (error) {
      console.error('Błąd przy pobieraniu procesów:', error)
      toast.error('Błąd podczas pobierania procesów')
    }
  }
})

watch(selectedProcess, (newProcess) => {
  if (newProcess && !isAddingNewProcess.value) {
    const process = processesData.value.find((p) => p.processId === newProcess)
    if (process) {
      editedProcess.value = { ...process }
    }
  } else if (newProcess && isAddingNewProcess.value) {
    isAddingNewProcess.value = false
    const process = processesData.value.find((p) => p.processId === newProcess)
    if (process) {
      editedProcess.value = { ...process }
    }
  } else if (!newProcess && !isAddingNewProcess.value) {
    editedProcess.value = {
      processDesc: '',
      processLongDesc: '',
      processColor: '#6B7280',
    }
  }
})

// --- LIFECYCLE ---
onMounted(async () => {
  try {
    const response = await apiService.get<Deck[]>(apiConfig.admin.deck.getAll)
    decksData.value = response.data
  } catch (error) {
    console.error('Błąd przy pobieraniu talii:', error)
    toast.error('Błąd podczas pobierania talii kart')
  }
})
</script>
