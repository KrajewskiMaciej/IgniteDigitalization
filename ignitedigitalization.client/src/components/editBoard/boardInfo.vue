<template>
  <div>
    <!-- Pole nazwy planszy -->
    <div class="mt-3 md:mt-5">
      <label for="board-name" class="block mb-2 text-sm font-medium text-surface-500">
        {{ t('boardName') }}
      </label>
      <InputText
        id="board-name"
        :modelValue="name"
        @update:modelValue="handleInput"
        :placeholder="t('boardNamePlaceholder')"
        class="w-full"
      />
    </div>

    <!-- Informacje o wymiarach planszy -->
    <div class="flex flex-row w-full items-center justify-center gap-5 mt-8 mb-5">
      <div
        class="border border-surface-700 px-3 py-2 rounded-lg w-60 text-center text-white bg-secondary"
      >
        <span class="font-medium"
          >{{ t('columns') }}: <span class="text-primary-400">{{ cols }}</span></span
        >
      </div>
      <div
        class="border border-surface-700 px-3 py-2 rounded-lg w-60 text-center text-white bg-secondary"
      >
        <span class="font-medium"
          >{{ t('rows') }}: <span class="text-primary-400">{{ rows }}</span></span
        >
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import InputText from 'primevue/inputtext'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

defineProps({
  name: {
    type: String,
    required: true,
  },
  cols: {
    type: Number,
    required: true,
  },
  rows: {
    type: Number,
    required: true,
  },
})

const emit = defineEmits(['update:name', 'update'])

const handleInput = (value: string | undefined) => {
  emit('update:name', value)
  emit('update')
}
</script>
