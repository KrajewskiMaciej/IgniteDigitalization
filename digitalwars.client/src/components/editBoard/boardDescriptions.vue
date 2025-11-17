<template>
  <div class="flex flex-col lg:flex-row w-full gap-4 mt-3 md:mt-5">
    <!-- Opis dolny (pod planszą) -->
    <div class="flex-1">
      <label for="description-down" class="block mb-2 text-sm font-medium text-white">
        Opis dolny
      </label>
      <InputText
        id="description-down"
        :modelValue="descriptionDown"
        @update:modelValue="(value) => handleInput(value, 'down')"
        placeholder="Wprowadź opis dolny"
        class="w-full"
      />
    </div>

    <!-- Opis lewy (po lewej stronie planszy) -->
    <div class="flex-1">
      <label for="description-left" class="block mb-2 text-sm font-medium text-white">
        Opis z lewej strony
      </label>
      <InputText
        id="description-left"
        :modelValue="descriptionLeft"
        @update:modelValue="(value) => handleInput(value, 'left')"
        placeholder="Wprowadź opis z lewej strony"
        class="w-full"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import InputText from 'primevue/inputtext'

defineProps({
  descriptionDown: {
    type: String,
    required: true,
  },
  descriptionLeft: {
    type: String,
    required: true,
  },
})

const emit = defineEmits(['update:descriptionDown', 'update:descriptionLeft', 'update'])

const handleInput = (value: string, type: 'down' | 'left') => {
  if (type === 'down') {
    emit('update:descriptionDown', value)
  } else {
    emit('update:descriptionLeft', value)
  }

  emit('update')
}
</script>
