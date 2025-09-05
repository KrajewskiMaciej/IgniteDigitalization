<template>
    <div>
      <!-- Pole nazwy planszy -->
      <div class="mt-3 md:mt-5">
        <label for="board-name" class="block mb-1 text-white">Nazwa planszy</label>
        <input
          id="board-name"
          :value="name"
          type="text" 
          class="w-full px-2 py-1.5 md:px-3 md:py-2 border-2 border-lgray-accent rounded-md bg-transparent text-sm md:text-base text-white"
          placeholder="Wprowadź nazwę planszy"
          @input="handleInput" />
      </div>
  
      <!-- Informacje o wymiarach planszy -->
      <div class="flex flex-row w-full items-center justify-center gap-5 mt-8 mb-5">
        <div class="border-2 border-lgray-accent px-2 py-1.5 md:px-3 md:py-2 rounded-md w-60 text-center text-white">
          <span>Kolumny: {{ cols }}</span>
        </div>
        <div class="border-2 border-lgray-accent px-2 py-1.5 md:px-3 md:py-2 rounded-md w-60 text-center text-sm md:text-base text-white">
          <span>Rzędy: {{ rows }}</span>
        </div>
      </div>
    </div>
  </template>

<script setup lang="ts">
  defineProps({
    name: {
      type: String,
      required: true
    },
    cols: {
      type: Number,
      required: true
    },
    rows: {
      type: Number,
      required: true
    }
  });

  const emit = defineEmits(['update:name', 'update']);

  /**
   * Obsługuje zdarzenie input, aby bezpiecznie zaktualizować nazwę planszy.
   * @param event - Zdarzenie input pochodzące z elementu <input>.
   */
  const handleInput = (event: Event) => {
    // Bezpieczne rzutowanie event.target na HTMLInputElement
    const target = event.target as HTMLInputElement;
    
    // Emitowanie zdarzeń z nową wartością
    emit('update:name', target.value);
    emit('update');
  };
</script>