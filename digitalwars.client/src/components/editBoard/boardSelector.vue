<template>
  <div class="relative w-full">
    <label for="board-selector" class="block mb-2 text-sm font-medium text-white">Wybierz planszę do edycji</label>
    <select
      id="board-selector"
      :value="modelValue"
      @change="handleSelectionChange"
      class="bg-tertiary border-2 border-lgray-accent text-white text-sm rounded-lg focus:ring-accent focus:border-accent block w-full p-2.5"
    >
      <option :value="null" disabled>-- Wybierz planszę --</option>
      <!-- Pętla v-for teraz działa poprawnie, ponieważ 'boards' jest silnie typowane -->
      <option v-for="board in boards" :key="board.boardId" :value="board.boardId">
        {{ board.name }}
      </option>
    </select>
  </div>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';

// --- DEFINICJE INTERFEJSÓW ---
// Definiuje strukturę obiektu planszy, co rozwiązuje błędy 'board is of type unknown'
interface Board {
  boardId: number;
  name: string;
}

// --- PROPSY I EMITY ---
defineProps({
  // Poprawne typowanie propsa 'boards' przy użyciu PropType
  boards: {
    type: Array as PropType<Board[]>,
    required: true
  },
  modelValue: {
    type: Number as PropType<number | null>,
    default: null
  }
});

const emit = defineEmits(['update:modelValue']);

// --- FUNKCJE ---
/**
 * Obsługuje zmianę wybranej opcji w selektorze.
 * Rozwiązuje problemy z '$event.target' poprzez jawne rzutowanie typu.
 * @param event - Zdarzenie zmiany wywołane przez element <select>.
 */
const handleSelectionChange = (event: Event) => {
  // Rzutowanie 'event.target' na HTMLSelectElement, aby uzyskać dostęp do właściwości 'value'
  const target = event.target as HTMLSelectElement;

  if (target && target.value) {
    // Parsowanie wartości na liczbę i emitowanie zdarzenia
    const selectedId = parseInt(target.value, 10);
    emit('update:modelValue', selectedId);
  }
};
</script>