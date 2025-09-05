<template>
    <div class="flex flex-col sm:flex-row w-full gap-5 mt-3 md:mt-5">
      <!-- Opis dolny (pod planszą) -->
      <div class="flex-1">
        <label for="description-down" class="block mb-1 text-white">Opis dolny</label>
        <input 
          id="description-down"
          type="text" 
          :value="descriptionDown" 
          class="w-full px-3 py-2 border-2 border-lgray-accent rounded-md bg-transparent text-white"
          placeholder="Wprowadź opis dolny"
          @input="event => handleInput(event, 'down')" />
      </div>
      
      <!-- Opis lewy (po lewej stronie planszy) -->
      <div class="flex-1">
        <label for="description-left" class="block mb-1 text-white">Opis z lewej strony</label>
        <input 
          id="description-left"
          type="text" 
          :value="descriptionLeft" 
          class="w-full px-3 py-2 border-2 border-lgray-accent rounded-md bg-transparent text-white"
          placeholder="Wprowadź opis z lewej strony"
          @input="event => handleInput(event, 'left')" />
      </div>
    </div>
  </template>
  
<script setup lang="ts">
  defineProps({
    descriptionDown: {
      type: String,
      required: true
    },
    descriptionLeft: {
      type: String,
      required: true
    }
  });

  const emit = defineEmits(['update:descriptionDown', 'update:descriptionLeft', 'update']);

  /**
   * Obsługuje zdarzenie input dla obu pól tekstowych i emituje odpowiednie zdarzenia.
   * @param event - Zdarzenie input pochodzące z elementu <input>.
   * @param type - Określa, które pole zostało zmienione ('down' lub 'left').
   */
  const handleInput = (event: Event, type: 'down' | 'left') => {
    // Bezpieczne rzutowanie event.target na HTMLInputElement
    const target = event.target as HTMLInputElement;

    // Sprawdzenie typu i wywołanie odpowiedniego emita
    if (type === 'down') {
      emit('update:descriptionDown', target.value);
    } else {
      emit('update:descriptionLeft', target.value);
    }
    
    // Wspólne zdarzenie informujące o aktualizacji
    emit('update');
  };
</script>