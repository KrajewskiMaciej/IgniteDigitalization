<template>
    <div class="w-full text-white">
        <div class="m-4 px-2 py-2 border-2 border-lgray-accent rounded-md bg-tertiary">
            <h1 class="text-center text-white font-nasalization text-3xl mt-2 mb-4">Stoły w grze</h1>

            <div v-if="loadingTables" class="text-center py-4">Ładowanie stołów...</div>
            <div v-else class="grid grid-cols-1 xl:grid-cols-3 gap-4 mt-6">
                <!-- <tableCard
                    :key="table.id"
                    :game = game
                    :table = table
                    :color="getTableColor(table.id)"
                /> -->
            </div>
        </div>
    </div>

</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import { useToast } from 'vue-toastification';
  import tableCard from '@/components/game/tableCard.vue';

  import apiConfig from '@/services/apiConfig.js';
  import apiService from '@/services/apiServices.js';

  const toast = useToast();

  const loadingTables = ref(false);
  const fetchError = ref<string | null>(null); // Ulepszone typowanie dla błędu

  // --- POPRAWKA: Dodano typ 'number' do parametru 'tableId' ---
  const getTableColor = (tableId: number) => {
    // Tutaj twoja logika
  };

  // Pobieranie stołów
  const fetchTables = async () => {
    // Tutaj twoja logika
  };

  // --- POPRAWKA: Otypowano cały obiekt przekazywany do funkcji ---
  const handleUpdateTableStatus = async ({ tableId, newStatus }: { tableId: number; newStatus: string }) => {
    // Tutaj twoja logika
  };

  onMounted(() => {
    fetchTables();
  });
</script>
