<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <!-- Nagłówek -->
    <div class="text-center">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-surface-500 mb-2">
        {{ t('editProcesses') }}
      </h1>
      <p class="text-surface-400 text-sm md:text-base">{{ t('manageProcessesInTheDeck') }}</p>
    </div>

    <div class="max-w-6xl mx-auto w-full space-y-6">
      <!-- Sekcja wyboru procesu -->
      <div
        v-if="selectedDeck"
        class="border border-surface-700 rounded-xl p-6 bg-secondary shadow-2xl"
      >
        <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
          <div class="bg-primary-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faChessPawn" class="h-6 text-primary-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-surface-500">{{ t('processes') }}</h2>
        </div>

        <div>
          <label for="process-select" class="block mb-2 text-sm font-semibold text-surface-300">
            {{ t('selectProcess') }}
          </label>

          <div class="flex gap-2">
            <Dropdown
              id="process-select"
              v-model="selectedProcess"
              :options="processesData"
              optionLabel="processDesc"
              optionValue="processId"
              :placeholder="t('selectProcessPlaceholder')"
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
              v-tooltip.top="t('addNewProcess')"
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
              v-tooltip.top="t('deleteProcess')"
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
        class="border border-surface-700 rounded-xl p-6 bg-secondary shadow-2xl"
      >
        <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
          <div class="bg-blue-500/20 p-3 rounded-lg">
            <font-awesome-icon :icon="faPenToSquare" class="h-6 text-blue-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-surface-500">
            {{ isAddingNewProcess ? t('newProcess') : t('processEdition') }}
          </h2>
        </div>

        <form @submit.prevent="saveProcessChanges" class="space-y-5">
          <!-- Skrót procesu -->
          <div>
            <label for="process-short" class="block mb-2 text-sm font-semibold text-surface-300">
              {{ t('processName') }}
            </label>
            <InputText
              id="process-short"
              v-model="editedProcess.processDesc"
              :placeholder="t('processNamePlaceholder')"
              class="w-full"
              :minlength="2"
              :maxlength="25"
            />
          </div>

          <!-- Opis procesu -->
          <div>
            <label for="process-long" class="block mb-2 text-sm font-semibold text-surface-300">
              {{ t('processDescription') }}
            </label>
            <Textarea
              id="process-long"
              v-model="editedProcess.processLongDesc"
              rows="3"
              :maxlength="75"
              :placeholder="t('processDescriptionPlaceholder')"
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
                  <label class="block mb-2 text-sm font-semibold text-surface-300">{{
                    t('processColor')
                  }}</label>
                  <p class="text-primary-400 text-sm mb-3">
                    {{ isAddingNewProcess ? t('clickToSelectColor') : t('clickToEditColor') }}
                  </p>
                </div>

                <div
                  class="bg-white rounded-lg inline-block border border-surface-700 hover:border-primary-400 transition-colors cursor-pointer"
                  @click="openColorPicker"
                >
                  <pawnPreview :pawnColor="editedProcess.processColor" class="w-32 h-32" />
                </div>

                <div class="flex items-center gap-3">
                  <span class="text-sm text-surface-400">{{ t('selectedColor') }}</span>
                  <span class="text-surface-500 text-sm">{{
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
              :label="isAddingNewProcess ? t('addNewProcess') : t('saveChanges')"
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
        class="text-center py-12 border border-dashed border-surface-700 rounded-xl bg-secondary/50"
      >
        <div
          class="bg-secondary/50 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-3"
        >
          <font-awesome-icon :icon="faLayerGroup" class="h-10 text-surface-600" />
        </div>
        <p class="text-surface-400 text-sm font-medium">{{ t('selectDeckToEditProcesses') }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
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
import apiServices from '@/services/apiServices'
import pawnPreview from '@/components/game/PawnPreview.vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const toast = useToast()
const confirm = useConfirm()

interface IProcessResponse {
  message: string
  processId: number
}

// --- INTERFACES ---
interface Process {
  processId: number
  deckId: number
  processDesc: string
  processLongDesc: string
  processColor: string
}

// --- REACTIVE DATA ---
const selectedDeck = defineModel<number | undefined>()

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

  confirm.require({
    message: `${t('processDeletionConfirmation')} "${processToDelete?.processDesc}"`,
    header: t('processDeletion'),
    accept: async () => {
      try {
        await apiServices.delete(apiConfig.processes.deleteProcess(selectedProcess.value!))
        processesData.value = processesData.value.filter(
          (process) => process.processId !== processId,
        )
        selectedProcess.value = null
        editedProcess.value = {
          processDesc: '',
          processLongDesc: '',
          processColor: '#6B7280',
        }
      } catch (error) {
        console.error('Błąd przy usuwaniu procesu:', error)
        toast.error(t('errorDeletingProcess') + error)
      }
    },
  })
}

const saveProcessChanges = async () => {
  if (!isProcessValid.value) {
    toast.error(t('invalidProcessData'))
    return
  }

  try {
    if (isAddingNewProcess.value) {
      const response = await apiServices.post<IProcessResponse>(apiConfig.processes.addProcess, {
        deck_Id: selectedDeck.value,
        process_Desc: editedProcess.value.processDesc,
        process_Long_Desc: editedProcess.value.processLongDesc,
        process_Color: editedProcess.value.processColor,
        process_Weight: 0.15, //Na razie na sztywno przypisana waga może później będziemy obsługiwać
      })

      const newProcess: Process = {
        ...editedProcess.value,
        processId: response.data.processId,
        deckId: selectedDeck.value!,
      }
      processesData.value.push(newProcess)

      selectedProcess.value = response.data.processId

      toast.success(t('newProcessAdded'))
    } else {
      // API call to update process
      const index = processesData.value.findIndex(
        (process) => process.processId === selectedProcess.value,
      )

      const response = await apiServices.put(
        apiConfig.processes.editProcess(selectedProcess.value!),
        {
          process_Desc: editedProcess.value.processDesc,
          process_Long_Desc: editedProcess.value.processLongDesc,
          process_Color: editedProcess.value.processColor,
        },
      )

      if (index !== -1) {
        processesData.value[index] = {
          ...editedProcess.value,
          processId: selectedProcess.value!,
          deckId: selectedDeck.value!,
        }
      }
    }

    isAddingNewProcess.value = false
  } catch (error) {
    console.error('Błąd przy zapisie procesu:', error)
    toast.error(t('errorSavingProcess') + error)
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
      const response = await apiServices.get<Process[]>(apiConfig.processes.getByDeck(newDeck))
      processesData.value = response.data
    } catch (error) {
      console.error('Błąd przy pobieraniu procesów:', error)
      toast.error(t('errorFetchingProcesses') + error)
    }
  }
}, { immediate: true })

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

</script>
