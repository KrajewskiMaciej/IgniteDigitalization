<template>
  <div class="w-full text-surface-500">
    <div class="m-4 px-2 py-2">
      <h1
        class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-surface-500 mb-2 text-center mt-2"
      >
        {{ t('tables') }}
      </h1>
      <tableButtons />
      <hr class="my-4 border-lgray-accent" />
      <div class="grid grid-cols-1 xl:grid-cols-3 gap-4 mt-6">
        <!-- Sprawdzamy, czy gameId jest poprawną liczbą -->
        <template v-if="!isNaN(gameId)">
          <tableCard
            v-for="table in tables"
            :key="table.id"
            :table="table"
            :gameId="gameId"
            :color="table.color"
            :token="table.token"
            :gameUrl="`${origin}/player/${table.token}`"
          />
        </template>
        <template v-else>
          <div class="text-center text-surface-300 mt-4 xl:col-span-3">
            {{ t('noActiveGameOrInvalidGameId') }}
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import tableCard from '@/components/game/tableCard.vue'
import { ref, onMounted } from 'vue'
// BŁĄD TS2307: Jeśli TypeScript nie może znaleźć tego modułu, upewnij się, że masz zainstalowany `vue-router`.
// Uruchom w terminalu: npm install vue-router
import { useRoute } from 'vue-router'
import apiServices from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'
import tableButtons from '@/components/admin/tableAdminButtons.vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

// --- DEFINICJE INTERFEJSÓW ---
interface Table {
  id: number
  color: string
  token: string
  // Dodaj inne właściwości, które może zwracać API, np. name
  name?: string
}

// --- POBIERANIE DANYCH Z TRASY ---
const route = useRoute()
// Konwertujemy parametr z trasy na liczbę. Jeśli parametr nie istnieje, wynikiem będzie NaN.
const gameId = Number(route.params.gameId)

// Pobranie origin URL do generowania linków
const origin = typeof window !== 'undefined' ? window.location.origin : ''

// --- ZMIENNE REAKTYWNE ---
// POPRAWKA BŁĘDU `never`: Jawnie typujemy tablicę `tables` za pomocą interfejsu `Table`
const tables = ref<Table[]>([])

// --- FUNKCJE ---
const getTeams = async () => {
  // Sprawdzamy, czy gameId jest prawidłową liczbą przed wykonaniem zapytania
  if (isNaN(gameId)) {
    console.error('Nieprawidłowy gameId, przerywam pobieranie drużyn.')
    tables.value = [] // Czyścimy tablicę, jeśli ID jest niepoprawne
    return
  }

  try {
    // Dodajemy typ generyczny do zapytania, aby TypeScript wiedział, czego się spodziewać
    const response = await apiServices.get<Table[]>(apiConfig.admin.games.getTeams(gameId))
    tables.value = response.data
  } catch (error) {
    console.error('Błąd przy pobieraniu drużyn:', error)
  }
}

// --- CYKL ŻYCIA KOMPONENTU ---
onMounted(() => {
  getTeams()
})
</script>
