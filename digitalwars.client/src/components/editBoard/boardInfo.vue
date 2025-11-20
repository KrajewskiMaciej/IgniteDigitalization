<template>
  <div>
    <!-- Pole nazwy planszy -->
    <div class="mt-3 md:mt-5">
      <label for="board-name" class="block mb-2 text-sm font-medium text-white">
        Nazwa planszy
      </label>
      <InputText
        id="board-name"
        :modelValue="name"
        @update:modelValue="handleInput"
        placeholder="Wprowadź nazwę planszy"
        class="w-full"
      />
    </div>

    <!-- Informacje o wymiarach planszy -->
    <div class="flex flex-row w-full items-center justify-center gap-5 mt-8 mb-5">
      <div
        class="border border-surface-700 px-3 py-2 rounded-lg w-60 text-center text-white bg-surface-900"
      >
        <span class="font-medium"
          >Kolumny: <span class="text-primary-400">{{ cols }}</span></span
        >
      </div>
      <div
        class="border border-surface-700 px-3 py-2 rounded-lg w-60 text-center text-white bg-surface-900"
      >
        <span class="font-medium"
          >Rzędy: <span class="text-primary-400">{{ rows }}</span></span
        >
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import InputText from 'primevue/inputtext'

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
