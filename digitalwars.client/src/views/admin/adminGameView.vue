<template>
    <div class="w-full text-white">
        <div class="m-4 px-2 py-2 border-2 border-lgray-accent rounded-md bg-tertiary">
            <h1 class="text-center text-white font-nasalization text-3xl mt-2 mb-4">Stoły</h1>
            <tableButtons />
            <hr class="my-4 border-lgray-accent"/>
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
                  <div class="text-center text-gray-300 mt-4 xl:col-span-3">
                    Brak aktywnej gry lub nieprawidłowy identyfikator gry.
                  </div>
                </template>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import tableCard from '@/components/game/tableCard.vue';
import { ref, onMounted } from 'vue';
// BŁĄD TS2307: Jeśli TypeScript nie może znaleźć tego modułu, upewnij się, że masz zainstalowany `vue-router`.
// Uruchom w terminalu: npm install vue-router
import { useRoute } from 'vue-router';
import apiServices from '@/services/apiServices';
import apiConfig from '@/services/apiConfig';
import tableButtons from '@/components/admin/tableAdminButtons.vue';

// --- DEFINICJE INTERFEJSÓW ---
interface Table {
  id: number;
  color: string;
  token: string;
  // Dodaj inne właściwości, które może zwracać API, np. name
  name?: string; 
}

// --- POBIERANIE DANYCH Z TRASY ---
const route = useRoute();
// Konwertujemy parametr z trasy na liczbę. Jeśli parametr nie istnieje, wynikiem będzie NaN.
const gameId = Number(route.params.gameId);

// Pobranie origin URL do generowania linków
const origin = typeof window !== 'undefined' ? window.location.origin : '';

// --- ZMIENNE REAKTYWNE ---
// POPRAWKA BŁĘDU `never`: Jawnie typujemy tablicę `tables` za pomocą interfejsu `Table`
const tables = ref<Table[]>([]);

// --- FUNKCJE ---
const getTeams = async () => {
  // Sprawdzamy, czy gameId jest prawidłową liczbą przed wykonaniem zapytania
  if (isNaN(gameId)) {
    console.error('Nieprawidłowy gameId, przerywam pobieranie drużyn.');
    tables.value = []; // Czyścimy tablicę, jeśli ID jest niepoprawne
    return;
  }
  
  try {
    // Dodajemy typ generyczny do zapytania, aby TypeScript wiedział, czego się spodziewać
    const response = await apiServices.get<Table[]>(apiConfig.admin.games.getTeams(gameId));
    tables.value = response.data;
  } catch (error) {
    console.error('Błąd przy pobieraniu drużyn:', error);
  }
};
        
// --- CYKL ŻYCIA KOMPONENTU ---
onMounted(() => {
  getTeams();
});
</script>