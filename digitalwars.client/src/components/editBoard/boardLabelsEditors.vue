<template>
  <div class="flex flex-col lg:flex-row w-full gap-4 mt-3 md:mt-5">
    <!-- Etykiety górne -->
    <div class="flex-1">
      <label class="block mb-2 text-sm font-medium text-white">{{ t('labelsTop') }}</label>

      <!-- Lista istniejących etykiet górnych -->
      <div class="border-2 border-surface-700 px-3 py-3 rounded-lg mb-4 bg-surface-900">
        <div class="flex flex-col gap-2">
          <div
            v-for="(label, index) in labelsUp"
            :key="index"
            class="flex items-center border-2 border-surface-600 rounded-lg p-2 gap-2 bg-surface-900 hover:border-primary-400 transition-colors duration-200"
          >
            <InputText
              :modelValue="label"
              @update:modelValue="(value) => updateLabelUp(index, value)"
              class="flex-1"
              :placeholder="t('label')"
            />
            <!-- Przycisk usuwania etykiety -->
            <Button
              type="button"
              @click="removeLabelUp(index)"
              :disabled="labelsUp.length <= 1"
              severity="danger"
              text
              rounded
              size="small"
              class="flex-shrink-0"
            >
              <font-awesome-icon :icon="faMinus" class="h-3.5" />
            </Button>
          </div>
        </div>
      </div>

      <!-- Formularz dodawania nowej etykiety górnej -->
      <div class="flex items-center gap-2">
        <InputText
          v-model="newLabelUp"
          @keyup.enter="addLabelUp"
          class="flex-1"
          :placeholder="t('newLabelTopPlaceholder')"
        />
        <Button type="button" @click="addLabelUp" class="flex-shrink-0" label="Dodaj">
          <template #icon>
            <font-awesome-icon :icon="faPlus" class="h-4 mr-2" />
          </template>
        </Button>
      </div>
    </div>

    <!-- Etykiety prawe -->
    <div class="flex-1">
      <label class="block mb-2 text-sm font-medium text-white">{{ t('labelsRight') }}</label>

      <!-- Lista istniejących etykiet prawych -->
      <div class="border-2 border-surface-700 px-3 py-3 rounded-lg mb-4 bg-surface-900">
        <div class="flex flex-col gap-2">
          <div
            v-for="(label, index) in labelsRight"
            :key="index"
            class="flex items-center border-2 border-surface-600 rounded-lg p-2 gap-2 bg-surface-900 hover:border-primary-400 transition-colors duration-200"
          >
            <InputText
              :modelValue="label"
              @update:modelValue="(value) => updateLabelRight(index, value)"
              class="flex-1"
              :placeholder="t('label')"
            />
            <!-- Przycisk usuwania etykiety -->
            <Button
              type="button"
              @click="removeLabelRight(index)"
              :disabled="labelsRight.length <= 1"
              severity="danger"
              text
              rounded
              size="small"
              class="flex-shrink-0"
            >
              <font-awesome-icon :icon="faMinus" class="h-3.5" />
            </Button>
          </div>
        </div>
      </div>

      <!-- Formularz dodawania nowej etykiety prawej -->
      <div class="flex items-center gap-2">
        <InputText
          v-model="newLabelRight"
          @keyup.enter="addLabelRight"
          class="flex-1"
          :placeholder="t('newLabelRightPlaceholder')"
        />
        <Button type="button" @click="addLabelRight" class="flex-shrink-0" label="Dodaj">
          <template #icon>
            <font-awesome-icon :icon="faPlus" class="h-4 mr-2" />
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
import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const toast = useToast()
const newLabelUp = ref('')
const newLabelRight = ref('')

// --- PROPSY Z POPRAWNYM TYPOWANIEM ---
const props = defineProps({
  labelsUp: {
    type: Array as PropType<string[]>,
    required: true,
  },
  labelsRight: {
    type: Array as PropType<string[]>,
    required: true,
  },
})

const emit = defineEmits(['update:labelsUp', 'update:labelsRight', 'update'])

// --- FUNKCJE DLA ETYKIET GÓRNYCH ---
const addLabelUp = () => {
  if (newLabelUp.value.trim()) {
    const updatedLabels = [...props.labelsUp, newLabelUp.value]
    emit('update:labelsUp', updatedLabels)
    newLabelUp.value = ''
    emit('update')
  } else {
    toast.warning(t('labelCannotBeEmpty'))
  }
}

const removeLabelUp = (index: number) => {
  if (props.labelsUp.length > 1) {
    const updatedLabels = [...props.labelsUp]
    updatedLabels.splice(index, 1)
    emit('update:labelsUp', updatedLabels)
    emit('update')
  } else {
    toast.warning(t('atLeastOneUpLabel'))
  }
}

const updateLabelUp = (index: number, value: string | undefined) => {
  const updatedLabels = [...props.labelsUp]
  updatedLabels[index] = value ?? ''
  emit('update:labelsUp', updatedLabels)
  emit('update')
}

// --- FUNKCJE DLA ETYKIET PRAWYCH ---
const addLabelRight = () => {
  if (newLabelRight.value.trim()) {
    const updatedLabels = [...props.labelsRight, newLabelRight.value]
    emit('update:labelsRight', updatedLabels)
    newLabelRight.value = ''
    emit('update')
  } else {
    toast.warning(t('labelCannotBeEmpty'))
  }
}

const removeLabelRight = (index: number) => {
  if (props.labelsRight.length > 1) {
    const updatedLabels = [...props.labelsRight]
    updatedLabels.splice(index, 1)
    emit('update:labelsRight', updatedLabels)
    emit('update')
  } else {
    toast.warning(t('atLeastObeOneRightLabel'))
  }
}

const updateLabelRight = (index: number, value: string | undefined) => {
  const updatedLabels = [...props.labelsRight]
  updatedLabels[index] = value ?? ''
  emit('update:labelsRight', updatedLabels)
  emit('update')
}
</script>
