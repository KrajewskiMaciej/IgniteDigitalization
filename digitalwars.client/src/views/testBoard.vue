<template>
  <!-- Użyj klas podobnych do tych z admina -->
  <div
    class="relative flex-grow overflow-hidden w-[800px] h-[600px] mx-auto bg-gray-800 rounded-lg border border-gray-600"
  >
    <gameBoard
      :config="formData"
      :gameMode="true"
      :posX="posX"
      :posY="posY"
      :pawns="currentPawns"
      :pawnColor="'#0000ff'"
    />
  </div>

  <div class="flex flex-col items-center mt-4 gap-2">
    <div class="flex justify-center">
      <button
        @click="moveUp"
        class="w-12 h-12 bg-blue-600 hover:bg-blue-700 text-white rounded-lg flex items-center justify-center"
      >
        <font-awesome-icon :icon="faArrowUp" size="lg" />
      </button>
    </div>

    <div class="flex justify-center gap-2">
      <button
        @click="moveLeft"
        class="w-12 h-12 bg-blue-600 hover:bg-blue-700 text-white rounded-lg flex items-center justify-center"
      >
        <font-awesome-icon :icon="faArrowLeft" size="lg" />
      </button>

      <button
        @click="moveDown"
        class="w-12 h-12 bg-blue-600 hover:bg-blue-700 text-white rounded-lg flex items-center justify-center"
      >
        <font-awesome-icon :icon="faArrowDown" size="lg" />
      </button>

      <button
        @click="moveRight"
        class="w-12 h-12 bg-blue-600 hover:bg-blue-700 text-white rounded-lg flex items-center justify-center"
      >
        <font-awesome-icon :icon="faArrowRight" size="lg" />
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import gameBoard from '@/components/game/gameBoard.vue'
import { reactive, ref, computed } from 'vue'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import {
  faArrowRight,
  faArrowLeft,
  faArrowUp,
  faArrowDown,
} from '@fortawesome/free-solid-svg-icons'

// KROK 1: Zaimportuj interfejs, aby TypeScript pomagał w przyszłości
import type { BoardConfig } from '@/interfaces/types'

const posX = ref(0)
const posY = ref(0)


const currentPawns = computed(() => [
  {
    id: 1, // Unikalne ID
    x: posX.value,
    y: posY.value,
    color: '#0000ff', // Kolor pionka
    name: 'Mój Pionek'
  },
    {
    id: 2, // Unikalne ID
    x: posX.value,
    y: posY.value,
    color: '#00ffff', // Kolor pionka
    name: 'Mój Pionek'
  },
    {
    id: 3, // Unikalne ID
    x: posX.value,
    y: posY.value,
    color: '#000fff', // Kolor pionka
    name: 'Mój Pionek'
  }
])

// KROK 2: Użyj interfejsu i popraw nazwy pól oraz dodaj brakujące 'boardId'
const formData = reactive<BoardConfig>({
  boardId: 0, // <-- DODANE BRAKUJĄCE POLE
  name: 'Plansza podstawowa', // <-- ZMIENIONA WIELKOŚĆ LITER
  labelsUp: [
    'Podstawowa kordynacja',
    'Standaryzacja procesów',
    'Zintegrowane działania',
    'Pełna integracja strategiczna',
  ],
  labelsRight: ['Nowicjusz', 'Naśladowca', 'Innowator', 'Lider cyfrowy'],
  descriptionDown: 'Poziom integracji wew/zew',
  descriptionLeft: 'Zawansowanie Cyfrowe',
  rows: 8,
  cols: 8,
  cellColor: '#fefae0',
  borderColor: '#595959',
  borderColors: ['#008000', '#FFFF00', '#FFA500', '#FF0000'],
})

const moveLeft = () => {
  posX.value--
}

const moveRight = () => {
  posX.value++
}

const moveUp = () => {
  posY.value++
}

const moveDown = () => {
  posY.value--
}
</script>
