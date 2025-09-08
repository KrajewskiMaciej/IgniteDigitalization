<template>
    <div class="w-full flex">
        <div class="flex relative flex-col flex-1 justify-center items-center m-4 px-2 py-2 border-2 border-lgray-accent rounded-md bg-tertiary">
            <h1 class="text-3xl font-nasalization text-white mt-5">Eksport gry do PDF</h1>

            <form class="w-full max-w-lg mt-4 flex flex-col items-center">
                <!--Wybór talii kart-->
                <div class="w-full mb-4">
                    <label class="block text-white mb-1">Wybierz talię:</label>
                    <dropDown
                        :items="decksData"
                        v-model="selectedDeck"
                        :item-key="'id'"
                        :display-format="(deck: Deck) => `#${deck.id} ${deck.title}`"
                        item-label="title"
                        placeholder="Wybierz talię..."
                    />
                </div>
                <div class="flex justify-center w-full text-white">
                    <button
                        type="button"
                        @click="exportDeckToPDF"
                        :disabled="!isFormDeckValid || isLoading"
                        :class="isFormDeckValid ? 'bg-accent/70 hover:bg-accent' : 'bg-gray-500 cursor-not-allowed'"
                        class="py-3 px-6 rounded-md mt-5 mb-3 transition-all w-64 text-center"
                    >
                        <font-awesome-icon v-if="!isLoading" :icon="faFileExport" class="h-4 mr-2"/>
                        {{ isLoading ? 'Generowanie...' : 'Generuj PDF z Kartami' }}
                    </button>
                </div>

                <!--Wybór plansz-->
                <label class="block text-white mb-1 mt-6" >Wybierz plansze:</label>
                <div class="space-y-3 w-full border-2 border-lgray-accent rounded-md bg-primary p-4">
                    <!--Wybór Planszy Stołu-->
                    <div class="mb-1 sm-mb-2">
                      <label for="selectBoard" class="block font-bold text-left text-xs text-white mb-1">Wybierz planszę stołu</label>
                      <select 
                          id="selectBoard" 
                          required 
                          class="text-white bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 w-full"
                          :value="selectedBoard"
                          @change="selectedBoard = Number(($event.target as HTMLSelectElement).value)"
                      >
                          <option :value="null" disabled>Wybierz planszę stołu</option>
                          <option v-for="board in boardsData" :key="board.boards_Id" :value="board.boards_Id">{{ board.name }}</option>
                      </select>
                  </div>

                    <!-- Wybór Planszy konkurencji -->
                    <div class="mb-1 sm:mb-2">
                      <label for="selectOpponentBoard" class="block font-bold text-left text-xs text-white mb-1">Wybierz planszę konkurencji</label>
                      <select 
                          id="selectOpponentBoard" 
                          required 
                          class="text-white bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 w-full"
                          :value="selectedOpponentBoard"
                          @change="selectedOpponentBoard = Number(($event.target as HTMLSelectElement).value)"
                      >
                          <option :value="null" disabled>Wybierz planszę konkurencji</option>
                          <option v-for="board in boardsData" :key="board.boards_Id" :value="board.boards_Id">{{ board.name }}</option>
                      </select>
                  </div>
                </div>

                <div class="flex justify-center w-full text-white">
                    <button
                        type="button"
                        @click="exportBoardsToPDF"
                        :disabled="!isFormBoardsValid || isLoading"
                        :class="isFormBoardsValid ? 'bg-accent/70 hover:bg-accent' : 'bg-gray-500 cursor-not-allowed'"
                        class="py-3 px-6 rounded-md mt-5 mb-3 transition-all w-64 text-center">
                        <font-awesome-icon v-if="!isLoading" :icon="faFileExport" class="h-4 mr-2"/>
                        {{ isLoading ? 'Generowanie...' : 'Generuj PDF z planszami' }}
                    </button>
                </div>
            </form>

            <loadingSpinner v-if="isLoading" message="Generuję PDF..."/>
        </div>
    </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, computed, watch } from 'vue';
  import { useToast } from 'vue-toastification';
  import { faFileExport } from '@fortawesome/free-solid-svg-icons';

  import apiConfig from '@/services/apiConfig';
  import apiService from '@/services/apiServices';
  import dropDown from '@/components/dropDown.vue';
  import loadingSpinner from '@/components/loadingSpinner.vue';

  interface Deck { id: number; title: string; }
  interface Board { boards_Id: number; name: string; }

  const toast = useToast();

  const decksData = ref<Deck[]>([]);
  const selectedDeck = ref<number | undefined>();

  const boardsData = ref<Board[]>([]);
  const selectedBoard = ref<number | null>(null);
  const selectedOpponentBoard = ref<number | null>(null);

  const isLoading = ref(false);

  const isFormBoardsValid = computed(() => typeof selectedBoard.value === 'number' && typeof selectedOpponentBoard.value === 'number');
  const isFormDeckValid = computed(() => typeof selectedDeck.value === 'number');

const downloadFileFromResponse = (response: any, defaultFileName: string) => {
  const header = response.headers['content-disposition'];
  let fileName = defaultFileName; // Używamy przekazanej nazwy jako domyślnej
  if (header) {
    let match = header.match(/filename\*=UTF-8''([^;]+)/) || header.match(/filename="?([^"]+)"?/);
    if (match && match[1]) {
      fileName = decodeURIComponent(match[1]);
    }
  }
  const blob = new Blob([response.data], { type: 'application/pdf' });
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  window.URL.revokeObjectURL(url);
  document.body.removeChild(a);
};


 const exportDeckToPDF = async () => {
  if (!isFormDeckValid.value) {
    toast.error("Proszę wybrać talię kart.");
    return;
  }
  isLoading.value = true;
  try {
    const url = apiConfig.admin.export.cards(selectedDeck.value!);
    const response = await apiService.getFile(url);
    // ZMIANA 2: Przekazano odpowiednią domyślną nazwę pliku
    downloadFileFromResponse(response, "DigitalWars - Karty.pdf");
    toast.success("PDF z kartami został pomyślnie wygenerowany.");
  } catch (error: any) {
    toast.error("Wystąpił błąd podczas generowania PDF z kartami.");
    console.error("Błąd generowania PDF z kartami:", error.response?.data || error.message);
  } finally {
    isLoading.value = false;
  }
};

const exportBoardsToPDF = async () => {
  if (!isFormBoardsValid.value) {
    toast.error("Proszę wybrać obie plansze przed wygenerowaniem PDF.");
    return;
  }

  console.log("Selected Board ID:", selectedBoard.value);
  console.log("Selected Opponent Board ID:", selectedOpponentBoard.value);

  isLoading.value = true;
  try {
    const url = apiConfig.admin.export.boards(selectedBoard.value!, selectedOpponentBoard.value!);
    const response = await apiService.getFile(url);
    // ZMIANA 3: Przekazano odpowiednią domyślną nazwę pliku
    downloadFileFromResponse(response, "DigitalWars - Plansze.pdf");
    toast.success("PDF z planszami został pomyślnie wygenerowany.");
  } catch (error: any) {
    toast.error("Wystąpił błąd podczas generowania PDF z planszami.");
    console.error("Błąd generowania PDF z planszami:", error.response?.data || error.message);
  } finally {
    isLoading.value = false;
  }
};

  const fetchBoardsFromAPI = async () => {
    try {
      const response = await apiService.get(apiConfig.boards.getAll);
      console.log("Dane plansz otrzymane z API:", response.data);
      boardsData.value = response.data as Board[];
    } catch (error: any) {
      toast.error(`Nie udało się pobrać plansz: ${error.message}`);
    }
  };

  const fetchDecksFromAPI = async () => {
    try {
      const response = await apiService.get(apiConfig.admin.deck.getAll);
      decksData.value = response.data as Deck[];
    } catch (error: any) {
      toast.error(`Nie udało się pobrać talii kart: ${error.message}`);
    }
  };

  onMounted(async () => {
    await Promise.all([fetchDecksFromAPI(), fetchBoardsFromAPI()]);
  });
</script>

<style scoped>
    .custom-scrollbar::-webkit-scrollbar {
    width: 0.6rem;
    }
    .custom-scrollbar::-webkit-scrollbar-track {
    background: transparent;
    margin: 0.5rem 0.3rem;
    }
    .custom-scrollbar::-webkit-scrollbar-thumb {
    background: #a78bfa;
    border-radius: 0.25rem;
    border: 0.1rem solid transparent;
    background-clip: content-box;
    }
</style>