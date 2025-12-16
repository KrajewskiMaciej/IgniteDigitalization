<template>
  <div>
    <!-- Sekcja wyboru kolorów podstawowych -->
    <div class="mt-3 md:mt-5">
      <label class="block mb-3 text-sm font-medium text-white">{{ t('basicColors') }}</label>
      <div class="flex flex-col sm:flex-row w-full gap-4">
        <!-- Kolor wypełnienia komórki -->
        <div class="border border-surface-700 py-3 px-4 rounded-lg flex-1 bg-surface-900">
          <label for="cell-color" class="block mb-2 text-sm text-surface-200-300">{{
            t('cellColor')
          }}</label>
          <div class="flex items-center gap-3">
            <input
              id="cell-color"
              :value="cellColor"
              type="color"
              class="w-14 h-14 p-0 bg-transparent cursor-pointer flex-shrink-0"
              @input="(event) => handleColorInput(event, 'cell')"
            />
            <div class="flex flex-col flex-1 min-w-0">
              <span class="text-xs text-surface-200-400">{{ t('hexValue') }}:</span>
              <span class="font-mono text-sm text-white truncate">{{ cellColor }}</span>
            </div>
          </div>
        </div>

        <!-- Kolor obramowania komórki -->
        <div class="border border-surface-700 py-3 px-4 rounded-lg flex-1 bg-surface-900">
          <label for="border-color" class="block mb-2 text-sm text-surface-200-300">{{
            t('borderColor')
          }}</label>
          <div class="flex items-center gap-3">
            <input
              id="border-color"
              :value="borderColor"
              type="color"
              class="w-14 h-14 p-0 bg-transparent cursor-pointer flex-shrink-0"
              @input="(event) => handleColorInput(event, 'border')"
            />
            <div class="flex flex-col flex-1 min-w-0">
              <span class="text-xs text-surface-200-400">{{ t('hexValue') }}:</span>
              <span class="font-mono text-sm text-white truncate">{{ borderColor }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Sekcja kolorów stref na planszy -->
    <div class="mt-8">
      <label class="block mb-3 text-sm font-medium text-white">{{ t('borderColors') }}</label>

      <!-- Lista istniejących kolorów stref -->
      <div class="border border-surface-700 p-3 rounded-lg mb-4 bg-surface-900">
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
          <div
            v-for="(color, index) in borderColors"
            :key="index"
            class="flex items-center border border-surface-600 rounded-lg p-2 gap-3 bg-surface-900 hover:border-primary-400 transition-colors duration-200"
          >
            <!-- Podgląd i selektor koloru -->
            <input
              type="color"
              :value="color"
              :title="`Strefa ${index + 1}: ${color}`"
              class="w-10 h-10 p-0 bg-transparent cursor-pointer flex-shrink-0"
              @input="(event) => handleBorderColorUpdate(event, index)"
            />
            <div class="flex flex-col flex-1 min-w-0">
              <span class="text-xs text-surface-200-400">{{ t('zone') }} {{ index + 1 }}</span>
              <span class="font-mono text-xs text-white truncate">{{ color }}</span>
            </div>
            <!-- Przycisk usuwania -->
            <Button
              type="button"
              @click="removeColor(index)"
              :disabled="borderColors.length <= 1"
              severity="danger"
              text
              rounded
              size="small"
              class="flex-shrink-0"
              :title="`Usuń strefę ${index + 1}`"
            >
              <font-awesome-icon :icon="faMinus" class="h-3.5" />
            </Button>
          </div>
        </div>
      </div>

      <!-- Formularz dodawania nowego koloru -->
      <div
        class="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 border border-surface-700 p-4 rounded-lg bg-surface-900"
      >
        <div class="flex items-center gap-3 flex-1">
          <input
            type="color"
            v-model="newColor"
            class="w-14 h-14 p-0 bg-transparent cursor-pointer flex-shrink-0"
          />
          <div class="flex flex-col flex-1 min-w-0">
            <span class="text-xs text-surface-200-400">{{ t('newColor') }}</span>
            <span class="font-mono text-sm text-white truncate">{{ newColor }}</span>
          </div>
        </div>
        <Button type="button" @click="addColor" class="sm:w-auto" :label="t('addColor')">
          <template #icon>
            <font-awesome-icon :icon="faPlus" class="mr-1" />
          </template>
        </Button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { PropType } from 'vue'
import { faPlus, faMinus } from '@fortawesome/free-solid-svg-icons'
import { useToast } from 'vue-toastification'
import Button from 'primevue/button'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const toast = useToast()
const newColor = ref('#aabbcc')

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
    type: Array as PropType<string[]>,
    required: true,
  },
})

const emit = defineEmits([
  'update:cellColor',
  'update:borderColor',
  'update:borderColors',
  'update',
])

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

const addColor = () => {
  const updatedColors = [...props.borderColors, newColor.value]
  emit('update:borderColors', updatedColors)
  emit('update')
}

const removeColor = (index: number) => {
  if (props.borderColors.length > 1) {
    const updatedColors = [...props.borderColors]
    updatedColors.splice(index, 1)
    emit('update:borderColors', updatedColors)
    emit('update')
  } else {
    toast.warning(t('selectAtLeastOneColor'))
  }
}

const updateBorderColor = (index: number, value: string) => {
  const updatedColors = [...props.borderColors]
  updatedColors[index] = value
  emit('update:borderColors', updatedColors)
  emit('update')
}
</script>
