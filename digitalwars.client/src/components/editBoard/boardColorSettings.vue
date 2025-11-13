<template>
  <div>
    <!-- Sekcja wyboru kolorów podstawowych -->
    <div class="flex flex-col sm:flex-row w-full items-stretch justify-center gap-5 mt-3 md:mt-5">
      <!-- Kolor wypełnienia komórki -->
      <div
        class="border-2 border-lgray-accent py-2 px-2 rounded-md flex-1 text-center flex flex-col justify-between"
      >
        <label for="cell-color" class="block mb-1 text-white">Kolor komórki</label>
        <div>
          <input
            id="cell-color"
            :value="cellColor"
            type="color"
            class="w-16 h-12 p-0 border-none rounded-md bg-transparent cursor-pointer mx-auto"
            @input="(event) => handleColorInput(event, 'cell')"
          />
          <div class="text-sm mt-2 text-gray-400">{{ cellColor }}</div>
        </div>
      </div>

      <!-- Kolor obramowania komórki -->
      <div
        class="border-2 border-lgray-accent py-2 px-2 rounded-md flex-1 text-center flex flex-col justify-between"
      >
        <label for="border-color" class="block mb-1 text-white">Kolor obramowania komórki</label>
        <div>
          <input
            id="border-color"
            :value="borderColor"
            type="color"
            class="w-16 h-12 p-0 border-none rounded-md bg-transparent cursor-pointer mx-auto"
            @input="(event) => handleColorInput(event, 'border')"
          />
          <div class="text-sm mt-2 text-gray-400">{{ borderColor }}</div>
        </div>
      </div>
    </div>

    <!-- Sekcja kolorów stref na planszy -->
    <div class="mt-8 md:mt-10">
      <label class="block mb-1 text-white">Kolory granic planszy</label>

      <!-- Lista istniejących kolorów stref -->
      <div class="border-2 border-lgray-accent px-2 py-2 rounded-md mb-3">
        <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-2">
          <div
            v-for="(color, index) in borderColors"
            :key="index"
            class="flex items-center border border-gray-600 rounded p-2 gap-2"
          >
            <!-- Podgląd i selektor koloru w jednym -->
            <input
              type="color"
              :value="color"
              class="w-10 h-10 p-0 border-none rounded-md bg-transparent cursor-pointer"
              @input="(event) => handleBorderColorUpdate(event, index)"
            />
            <span class="text-gray-400 font-mono text-sm">{{ color }}</span>
            <!-- Przycisk usuwania koloru -->
            <button
              type="button"
              @click="removeColor(index)"
              class="ml-auto p-1 text-red-500 hover:text-red-400 rounded transition-colors"
              :disabled="borderColors.length <= 1"
              title="Usuń kolor"
            >
              <font-awesome-icon :icon="faTrash" class="h-3" />
            </button>
          </div>
        </div>
      </div>

      <!-- Formularz dodawania nowego koloru -->
      <div class="flex items-center justify-center gap-3">
        <input
          type="color"
          v-model="newColor"
          class="w-12 h-12 p-0 border-none rounded-md bg-transparent cursor-pointer"
        />
        <button
          type="button"
          @click="addColor"
          class="px-3 py-2 rounded-md border-2 border-lgray-accent hover:border-accent transition-colors duration-300 text-white"
        >
          <font-awesome-icon :icon="faPlus" class="h-4 text-accent mr-2" />
          Dodaj kolor
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { PropType } from 'vue' // Import 'type'
import { faPlus, faTrash } from '@fortawesome/free-solid-svg-icons'
import { useToast } from 'vue-toastification'

const toast = useToast()
const newColor = ref('#aabbcc')

// --- POPRAWNIE OTYPOWANE PROPSY ---
const props = defineProps({
  cellColor: {
    type: String,
    required: true,
  },
  borderColor: {
    type: String,
    required: true,
  },
  borderColors: {
    type: Array as PropType<string[]>, // Jawne typowanie tablicy stringów
    required: true,
  },
})

const emit = defineEmits([
  'update:cellColor',
  'update:borderColor',
  'update:borderColors',
  'update',
])

// --- BEZPIECZNE FUNKCJE OBSŁUGI ZDARZEŃ ---
const handleColorInput = (event: Event, type: 'cell' | 'border') => {
  const target = event.target as HTMLInputElement
  if (type === 'cell') {
    emit('update:cellColor', target.value)
  } else {
    emit('update:borderColor', target.value)
  }
  emit('update')
}

const handleBorderColorUpdate = (event: Event, index: number) => {
  const target = event.target as HTMLInputElement
  updateBorderColor(index, target.value)
}

// --- FUNKCJE Z POPRAWNYM TYPOWANIEM PARAMETRÓW ---
const addColor = () => {
  const updatedColors = [...props.borderColors, newColor.value]
  emit('update:borderColors', updatedColors)
  emit('update')
}

const removeColor = (index: number) => {
  // Jawne typowanie: number
  if (props.borderColors.length > 1) {
    const updatedColors = [...props.borderColors]
    updatedColors.splice(index, 1)
    emit('update:borderColors', updatedColors)
    emit('update')
  } else {
    toast.warning('Musi istnieć co najmniej jeden kolor granicy!')
  }
}

const updateBorderColor = (index: number, value: string) => {
  // Jawne typowanie: number, string
  const updatedColors = [...props.borderColors]
  updatedColors[index] = value
  emit('update:borderColors', updatedColors)
  emit('update')
}
</script>
