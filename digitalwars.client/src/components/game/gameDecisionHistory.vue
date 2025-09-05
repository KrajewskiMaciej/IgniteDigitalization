<template>
  <div>
    <h2>Zarządzaj historią decyzji</h2>
    <button @click="loadData">Ładowanie logów</button>

    <div style="margin-top: 20px;">
      <h3>Dodaj Decyzję</h3>
      <form @submit.prevent="createGamelog">
        <!-- POPRAWKA: Dodano modyfikator `.number` dla pól numerycznych -->
        <input v-model.number="newGamelog.TeamId" placeholder="TeamId" type="number" required />
        <input v-model.number="newGamelog.GameId" placeholder="GameId" type="number" required />
        <input v-model.number="newGamelog.CardId" placeholder="CardId" type="number" required />
        <input v-model.number="newGamelog.DeckId" placeholder="DeckId" type="number" />
        <input v-model="newGamelog.Date" placeholder="Date (YYYY-MM-DD)" type="text" />
        <input v-model.number="newGamelog.FeedbackId" placeholder="FeedbackId" type="number" />
        <input v-model.number="newGamelog.Cost" placeholder="Cost" type="number" />
        <input v-model="newGamelog.Status" placeholder="Status" type="text" />
        <button type="submit">Create</button>
      </form>
    </div>

    <div style="margin-top: 20px;">
      <h3>Usuń Decyzję</h3>
      <form @submit.prevent="deleteGamelog">
        <input v-model.number="deleteIds.TeamId" placeholder="TeamId" type="number" required />
        <input v-model.number="deleteIds.GameId" placeholder="GameId" type="number" required />
        <input v-model.number="deleteIds.CardId" placeholder="CardId" type="number" required />
        <button type="submit">Usuń</button>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

// --- DEFINICJE INTERFEJSÓW ---
interface Gamelog {
  TeamId: number;
  GameId: number;
  CardId: number;
  DeckId: number | null;
  Date: string;
  FeedbackId: number | null;
  Cost: number | null;
  Status: string;
}

// Interfejs dla stanu formularza, który może zawierać puste stringi lub liczby
interface GamelogFormState {
  TeamId: number | '';
  GameId: number | '';
  CardId: number | '';
  DeckId: number | '';
  Date: string;
  FeedbackId: number | '';
  Cost: number | '';
  Status: string;
}

interface DeleteFormState {
  TeamId: number | '';
  GameId: number | '';
  CardId: number | '';
}

// --- ZMIENNE REAKTYWNE Z TYPOWANIEM ---
const emit = defineEmits(['data-loaded']);

const gamelogs = ref<Gamelog[]>([
  { TeamId: 1, GameId: 100, CardId: 501, DeckId: 201, Date: '2025-04-01', FeedbackId: 301, Cost: 10, Status: 'Active' },
  { TeamId: 2, GameId: 101, CardId: 502, DeckId: 202, Date: '2025-04-02', FeedbackId: 302, Cost: 15, Status: 'Inactive' },
  { TeamId: 3, GameId: 102, CardId: 503, DeckId: 203, Date: '2025-04-03', FeedbackId: 303, Cost: 20, Status: 'Active' },
]);

const getInitialFormState = (): GamelogFormState => ({
  TeamId: '', GameId: '', CardId: '', DeckId: '', Date: '', FeedbackId: '', Cost: '', Status: '',
});

const getInitialDeleteState = (): DeleteFormState => ({
  TeamId: '', GameId: '', CardId: '',
});

const newGamelog = ref<GamelogFormState>(getInitialFormState());
const deleteIds = ref<DeleteFormState>(getInitialDeleteState());

// --- FUNKCJE ---
function loadData() {
  const selectedData = gamelogs.value.map(({ CardId, TeamId }) => ({ CardId, TeamId }));
  emit('data-loaded', { fullData: gamelogs.value, selectedData });
}

function createGamelog() {
  // POPRAWKA: Konwertujemy wartości formularza na poprawny typ `Gamelog`
  const logToAdd: Gamelog = {
    TeamId: Number(newGamelog.value.TeamId),
    GameId: Number(newGamelog.value.GameId),
    CardId: Number(newGamelog.value.CardId),
    DeckId: newGamelog.value.DeckId ? Number(newGamelog.value.DeckId) : null,
    Date: newGamelog.value.Date,
    FeedbackId: newGamelog.value.FeedbackId ? Number(newGamelog.value.FeedbackId) : null,
    Cost: newGamelog.value.Cost ? Number(newGamelog.value.Cost) : null,
    Status: newGamelog.value.Status,
  };
  gamelogs.value.push(logToAdd);
  loadData();
  newGamelog.value = getInitialFormState();
}

function deleteGamelog() {
  // POPRAWKA: Konwertujemy ID do usunięcia na liczby przed filtrowaniem
  const teamIdToDelete = Number(deleteIds.value.TeamId);
  const gameIdToDelete = Number(deleteIds.value.GameId);
  const cardIdToDelete = Number(deleteIds.value.CardId);

  gamelogs.value = gamelogs.value.filter(log =>
    // Używamy ścisłego porównania (===) po konwersji typów
    !(log.TeamId === teamIdToDelete && log.GameId === gameIdToDelete && log.CardId === cardIdToDelete)
  );
  loadData();
  deleteIds.value = getInitialDeleteState();
}
</script>

<style scoped>
form {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 10px;
}
input {
  padding: 5px;
}
button {
  width: fit-content;
  padding: 5px 10px;
}
</style>