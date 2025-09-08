<template>
    <div class="w-full">
        <div class="grid grid-cols-1 md:grid-cols-[55fr_45fr] gap-4">

            <div class="order-2 md:order-1 flex flex-col justify-start border-2 border-lgray-accent py-6 px-4 m-4 rounded-md text-white bg-tertiary">
                <div class="flex flex-row w-full items-center justify-center gap-5 flex-shrink-0">
                    <button
                        class="border-2 border-lgray-accent py-2 px-2 rounded-md w-60 text-center hover:border-accent transition-colors duration-300"
                        :class="{'border-accent': activeView === 'add'}"
                        @click="activeView = 'add'">
                        <font-awesome-icon :icon="faPlus" class="h-4 text-accent"/>
                        Dodaj nową planszę
                    </button>
                    <button
                        class="border-2 border-lgray-accent py-2 px-2 rounded-md w-60 text-center hover:border-accent transition-colors duration-300"
                        :class="{'border-accent': activeView === 'edit'}"
                        @click="activeView = 'edit'">
                        <font-awesome-icon :icon="faPenToSquare" class="h-4 text-accent"/>
                        Edytuj planszę
                    </button>
                </div>

                <div class="w-full">
                    <h1 class="mt-8 font-nasalization text-lg md:text-xl lg-text-2xl xl:test-3xl ">
                        {{ activeView === 'add' ? 'Dodaj nową planszę' : 'Edytuj planszę' }}
                    </h1>

                    <boardSelector
                        :boards="boardsForSelector"
                        v-model="selectedBoardId"
                        :activeView="activeView"
                        @delete="deleteBoard"
                    />

                    <form class="mt-4">
                        <boardInfo
                            v-model:name="formData.Name"
                            :cols="formData.LabelsUp.length * 2"
                            :rows="formData.LabelsRight.length * 2"
                            @update="validateDescriptions"
                        />

                        <boardColorSettings
                            v-model:cellColor="formData.CellColor"
                            v-model:borderColor="formData.BorderColor"
                            v-model:borderColors="formData.BorderColors"
                            @update="validateDescriptions"
                        />

                        <boardLabelsEditors
                            v-model:labelsUp="formData.LabelsUp"
                            v-model:labelsRight="formData.LabelsRight"
                            @update="validateDescriptions"
                        />

                        <boardDescriptions
                            v-model:descriptionDown="formData.DescriptionDown"
                            v-model:descriptionLeft="formData.DescriptionLeft"
                            @update="validateDescriptions"
                        />

                        <button
                            type="button"
                            class="bg-accent border-2 border-accent py-3 px-6 rounded-md mt-5 hover:bg-opacity-80 transition-all"
                            @click="saveBoard">
                            <font-awesome-icon :icon="faSave" class="h-4 mr-2" />
                            {{ activeView === 'add' ? 'Dodaj planszę' : 'Zapisz zmiany' }}
                        </button>
                    </form>
                </div>
            </div>

            <div class="order-1 md:order-2 border-2 border-lgray-accent py-6 px-8 m-4 rounded-md text-white bg-tertiary flex flex-col
                         md:sticky md:top-4 self-start md:max-h-[calc(100vh-2rem)]">
                
                <h2 class="text-xl mb-4 text-center flex-shrink-0">Podgląd planszy</h2>

                <div class="relative flex-grow min-h-0">
                    <myBoard
                        :config="previewConfig"
                    />
                </div>
            </div>

        </div>
    </div>
</template>

<script setup lang="ts">
import { faPlus, faPenToSquare, faSave } from '@fortawesome/free-solid-svg-icons';
import { ref, reactive, computed, onMounted, watch } from 'vue';
import { useToast } from 'vue-toastification';
import myBoard from '@/components/game/gameBoard.vue';
import boardSelector from '@/components/editBoard/boardSelector.vue';
import boardInfo from '@/components/editBoard/boardInfo.vue';
import boardColorSettings from '@/components/editBoard/boardColorSettings.vue';
import boardLabelsEditors from '@/components/editBoard/boardLabelsEditors.vue';
import boardDescriptions from '@/components/editBoard/boardDescriptions.vue';

import apiConfig from '@/services/apiConfig';
import apiService from '@/services/apiServices';

// --- DEFINICJE INTERFEJSÓW ---
interface Board {
  boards_Id: number;
  name: string;
  labels_Up: string;
  labels_Right: string;
  description_Down: string;
  description_Left: string;
  rows: number;
  cols: number;
  cell_Color: string;
  border_Color: string;
  borders_Colors: string;
}

interface FormData {
  BoardId: number;
  Name: string;
  LabelsUp: string[];
  LabelsRight: string[];
  DescriptionDown: string;
  DescriptionLeft: string;
  Rows: number;
  Cols: number;
  CellColor: string;
  BorderColor: string;
  BorderColors: string[];
}

interface PreviewConfig {
    Name: string;
    LabelsUp: string[];
    LabelsRight: string[];
    DescriptionDown: string;
    DescriptionLeft: string;
    Rows: number;
    Cols: number;
    CellColor: string;
    BorderColor: string;
    BorderColors: string[];
}

// --- ZMIENNE REAKTYWNE Z TYPOWANIEM ---
const selectedBoardId = ref<number | undefined>(undefined);
const activeView = ref<'add' | 'edit'>('add');
const toast = useToast();

const data = reactive<{ boards: Board[] }>({
  boards: []
});

const getDefaultFormData = (): FormData => ({
  BoardId: 0,
  Name: 'Nowa plansza',
  LabelsUp: ['Etykieta 1', 'Etykieta 2', 'Etykieta 3', 'Etykieta 4'],
  LabelsRight: ['Etykieta A', 'Etykieta B', 'Etykieta C', 'Etykieta D'],
  DescriptionDown: 'Opis dolny',
  DescriptionLeft: 'Opis lewy',
  Rows: 8,
  Cols: 8,
  CellColor: '#ffffff',
  BorderColor: '#000000',
  BorderColors: ['#008000', '#FFFF00', '#FFA500', '#FF0000']
});

const formData = reactive<FormData>(getDefaultFormData());

// --- WŁAŚCIWOŚCI OBLICZENIOWE ---
// POPRAWKA: Tworzy nową tablicę z poprawną nazwą klucza 'boardId' dla komponentu boardSelector
const boardsForSelector = computed(() => {
  return data.boards.map(board => ({
    boardId: board.boards_Id,
    name: board.name
  }));
});

// --- WATCHERY ---
watch(() => formData.LabelsUp, (newLabels) => {
  if (Array.isArray(newLabels)) {
    formData.Cols = newLabels.length * 2;
  }
}, { deep: true });

watch(() => formData.LabelsRight, (newLabels) => {
  if (Array.isArray(newLabels)) {
    formData.Rows = newLabels.length * 2;
  }
}, { deep: true });

watch(selectedBoardId, () => loadSelectedBoard());

watch(activeView, (newView) => {
  resetForm();
  if (newView === 'edit' && data.boards.length === 0) {
    toast.info('Brak plansz do edycji. Dodaj nową planszę.');
    activeView.value = 'add';
  }
});

// --- FUNKCJE POMOCNICZE ---
const stringToArray = (str: string): string[] => {
  if (typeof str !== 'string' || !str) return [];
  return str.split(';').map(item => item.trim()).filter(item => item);
};

const arrayToString = (arr: string[]): string => {
  if (!arr || !Array.isArray(arr)) return '';
  return arr.join(';');
};

// --- LOGIKA BIZNESOWA ---
const fetchBoardsFromAPI = async () => {
  try {
    const response = await apiService.get<Board[]>(apiConfig.boards.getAll);
    data.boards = response.data;
    if (data.boards.length === 0 && activeView.value === 'edit') {
      toast.info("Brak plansz do edycji, przełączam na dodawanie.");
      activeView.value = 'add';
    }
  } catch (error: any) {
    console.error('Błąd pobierania plansz:', error.response?.data || error.message);
    toast.error(`Nie udało się pobrać plansz: ${error.response?.data?.title || error.message}`);
  }
};

const resetForm = () => {
  Object.assign(formData, getDefaultFormData());
  selectedBoardId.value = undefined;
};

const loadSelectedBoard = () => {
  if (!selectedBoardId.value) {
    resetForm();
    return;
  }
  
  const selectedBoard = data.boards.find(board => board.boards_Id === selectedBoardId.value);
  if (!selectedBoard) {
    toast.error('Nie znaleziono wybranej planszy.');
    return;
  }

  formData.BoardId = selectedBoard.boards_Id;
  formData.Name = selectedBoard.name;
  formData.LabelsUp = stringToArray(selectedBoard.labels_Up);
  formData.LabelsRight = stringToArray(selectedBoard.labels_Right);
  formData.DescriptionDown = selectedBoard.description_Down;
  formData.DescriptionLeft = selectedBoard.description_Left;
  formData.Rows = selectedBoard.rows;
  formData.Cols = selectedBoard.cols;
  formData.CellColor = selectedBoard.cell_Color;
  formData.BorderColor = selectedBoard.border_Color;
  formData.BorderColors = stringToArray(selectedBoard.borders_Colors);

  toast.success(`Załadowano planszę: ${formData.Name}`);
};

const saveBoard = async () => {
  try {
    if (!formData.Name.trim()) {
      toast.error('Nazwa planszy jest wymagana!');
      return;
    }
    if (formData.LabelsUp.some(label => !label.trim()) || formData.LabelsRight.some(label => !label.trim())) {
      toast.error('Wszystkie etykiety muszą być wypełnione!');
      return;
    }
    
    const payload = {
      Name: formData.Name,
      Labels_Up: arrayToString(formData.LabelsUp),
      Labels_Right: arrayToString(formData.LabelsRight),
      Description_Down: formData.DescriptionDown,
      Description_Left: formData.DescriptionLeft,
      Rows: formData.Rows,
      Cols: formData.Cols,
      Cell_Color: formData.CellColor,
      Border_Color: formData.BorderColor,
      Borders_Colors: arrayToString(formData.BorderColors)
    };

    if (activeView.value === 'add') {
      const response = await apiService.post<Board>(apiConfig.boards.create, payload);
      toast.success(`Plansza "${response.data.name}" dodana pomyślnie!`);
      await fetchBoardsFromAPI();
      resetForm();
    } else {
      if (!selectedBoardId.value) {
        toast.warning('Wybierz planszę do edycji!');
        return;
      }
      const response = await apiService.put<Board>(apiConfig.boards.update(selectedBoardId.value), payload);
      toast.success(`Plansza "${response.data.name}" zaktualizowana pomyślnie!`);
      await fetchBoardsFromAPI();
    }
  } catch (error: any) {
    console.error('Błąd podczas zapisywania planszy:', error.response?.data || error.message);
    const errorMessage = error.response?.data?.title || error.response?.data || error.message;
    toast.error(`Błąd zapisu: ${errorMessage}`);
  }
};

const deleteBoard = async () => {
  if (!selectedBoardId.value) {
    toast.warning('Nie wybrano planszy do usunięcia!');
    return;
  }
  
  const boardToDelete = data.boards.find(b => b.boards_Id === selectedBoardId.value);
  const boardName = boardToDelete ? boardToDelete.name : "wybrana plansza";

  if (confirm(`Czy na pewno chcesz usunąć planszę "${boardName}"?`)) {
    try {
      await apiService.delete(apiConfig.boards.delete(selectedBoardId.value));
      toast.success('Plansza została usunięta pomyślnie!');
      
      await fetchBoardsFromAPI();
      resetForm();
      if (data.boards.length === 0) {
        activeView.value = 'add';
      }
    } catch (error: any) {
      console.error('Błąd podczas usuwania planszy:', error.response?.data || error.message);
      const errorMessage = error.response?.data?.title || error.response?.data || error.message;
      toast.error(`Błąd usuwania: ${errorMessage}`);
    }
  }
};

const validateDescriptions = () => {
  if (!formData.DescriptionDown?.trim()) {
    formData.DescriptionDown = 'Opis dolny';
    toast.warning("Opis dolny nie może być pusty. Ustawiono wartość domyślną.");
  }
  if (!formData.DescriptionLeft?.trim()) {
    formData.DescriptionLeft = 'Opis lewy';
    toast.warning("Opis lewy nie może być pusty. Ustawiono wartość domyślną.");
  }
};

// --- COMPUTED & LIFECYCLE ---
const previewConfig = computed<PreviewConfig>(() => {
  return {
    Name: formData.Name,
    LabelsUp: formData.LabelsUp,
    LabelsRight: formData.LabelsRight,
    DescriptionDown: formData.DescriptionDown,
    DescriptionLeft: formData.DescriptionLeft,
    Rows: formData.Rows,
    Cols: formData.Cols,
    CellColor: formData.CellColor,
    BorderColor: formData.BorderColor,
    BorderColors: formData.BorderColors
  };
});

onMounted(fetchBoardsFromAPI);
</script>

