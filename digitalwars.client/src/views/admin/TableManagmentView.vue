<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <!-- Nagłówek -->
    <div class="text-center">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-white mb-2">
        Zarządzanie Stołami
      </h1>
    </div>

    <div class="max-w-6xl mx-auto w-full space-y-6">
      <!-- Sekcja wyboru drużyny -->
      <div class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl">
        <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
          <div class="bg-primary-500/20 p-2.5 rounded-lg">
            <font-awesome-icon :icon="faUsers" class="h-6 text-primary-400" />
          </div>
          <h2 class="text-xl md:text-2xl font-bold text-white">Wybór drużyny</h2>
        </div>

        <div v-if="loading.teams" class="text-center py-8">
          <ProgressSpinner style="width: 3rem; height: 3rem" strokeWidth="4" />
          <p class="text-gray-400 mt-3">Ładowanie drużyn...</p>
        </div>

        <div v-else>
          <label for="team-select" class="block mb-2 text-sm font-semibold text-gray-300">
            Wybierz drużynę:
          </label>
          <Dropdown
            id="team-select"
            v-model="selectedTeamId"
            :options="teams"
            optionLabel="teamName"
            optionValue="teamId"
            placeholder="Wybierz drużynę..."
            class="w-full"
          >
            <template #option="slotProps">
              <div class="flex items-center justify-between w-full">
                <span>{{ slotProps.option.teamName }}</span>
                <span class="text-green-400">{{ slotProps.option.teamBud }} bitów</span>
              </div>
            </template>
          </Dropdown>
        </div>
      </div>

      <!-- Grid z dwiema sekcjami -->
      <div v-if="selectedTeamId" class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Sekcja zarządzania budżetem -->
        <div class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl">
          <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
            <div class="bg-green-500/20 p-2.5 rounded-lg">
              <font-awesome-icon :icon="faCoins" class="h-6 text-green-400" />
            </div>
            <h2 class="text-xl md:text-2xl font-bold text-white">Budżet drużyny</h2>
          </div>

          <div v-if="selectedTeam" class="space-y-5">
            <!-- Aktualny budżet -->
            <div class="bg-surface-800 rounded-lg p-4 border border-surface-700">
              <p class="text-sm text-gray-400 mb-1">Aktualny budżet:</p>
              <p class="text-3xl font-bold text-green-400">{{ selectedTeam.teamBud }} bitów</p>
            </div>

            <!-- Formularz edycji budżetu -->
            <form @submit.prevent="saveBudget" class="space-y-4">
              <div>
                <label for="budget-input" class="block mb-2 text-sm font-semibold text-gray-300">
                  Nowy budżet:
                </label>
                <InputNumber
                  id="budget-input"
                  v-model="budgetInputValue"
                  :min="20"
                  :step="5"
                  placeholder="Wprowadź nowy budżet..."
                  showButtons
                  class="w-full"
                  inputClass="text-center text-lg font-bold"
                />
              </div>

              <div class="flex justify-center">
                <Button
                  type="submit"
                  :disabled="isSavingBudget"
                  :loading="isSavingBudget"
                  :label="isSavingBudget ? 'Zapisywanie...' : 'Zapisz Budżet'"
                  size="large"
                  class="w-full"
                />
              </div>
            </form>
          </div>
        </div>

        <!-- Sekcja zarządzania kartami -->
        <div class="border border-surface-700 rounded-xl p-6 bg-surface-900 shadow-2xl">
          <div class="flex items-center gap-3 mb-5 pb-4 border-b border-surface-700">
            <div class="bg-blue-500/20 p-2.5 rounded-lg">
              <font-awesome-icon :icon="faLock" class="h-6 text-blue-400" />
            </div>
            <h2 class="text-xl md:text-2xl font-bold text-white">Odblokuj kartę</h2>
          </div>

          <div class="space-y-5">
            <div v-if="loading.cards" class="text-center py-8">
              <ProgressSpinner style="width: 3rem; height: 3rem" strokeWidth="4" />
              <p class="text-gray-400 mt-3">Ładowanie kart...</p>
            </div>

            <div v-else-if="decisionCards.length > 0">
              <label for="card-select" class="block mb-2 text-sm font-semibold text-gray-300">
                Wybierz kartę do odblokowania:
              </label>
              <Dropdown
                id="card-select"
                v-model="selectedCardId"
                :options="decisionCards"
                optionLabel="title"
                optionValue="id"
                placeholder="Wybierz kartę..."
                class="w-full"
              >
                <template #value="slotProps">
                  <div v-if="slotProps.value" class="flex items-center gap-2">
                    <span class="text-blue-400">#{{ slotProps.value }}</span>
                    <span>{{ decisionCards.find((c) => c.id === slotProps.value)?.title }}</span>
                  </div>
                  <span v-else class="text-gray-400">{{ slotProps.placeholder }}</span>
                </template>
                <template #option="slotProps">
                  <div class="flex items-center gap-2">
                    <span class="text-blue-400">#{{ slotProps.option.id }}</span>
                    <span>{{ slotProps.option.title }}</span>
                  </div>
                </template>
              </Dropdown>

              <!-- Opis karty -->
              <div
                v-if="selectedCard"
                class="mt-4 bg-surface-800 rounded-lg p-4 border border-surface-700"
              >
                <p class="text-sm text-gray-400 mb-2">Opis karty:</p>
                <p class="text-sm text-gray-300">{{ selectedCard.description }}</p>
              </div>

              <!-- Przycisk odblokowania -->
              <div class="flex justify-center mt-5">
                <Button
                  @click="handleUnlockCard"
                  :disabled="!selectedCardId || isUnlockingCard"
                  :loading="isUnlockingCard"
                  :label="isUnlockingCard ? 'Odblokowywanie...' : 'Odblokuj Kartę'"
                  size="large"
                  class="w-full"
                />
              </div>
            </div>

            <div v-else class="text-center py-8">
              <div
                class="bg-surface-800/50 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-3"
              >
                <font-awesome-icon :icon="faLock" class="h-8 text-surface-600" />
              </div>
              <p class="text-gray-400 text-sm">Nie znaleziono kart dla wybranej drużyny</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Placeholder gdy brak wybranej drużyny -->
      <div
        v-if="!selectedTeamId"
        class="text-center py-12 border border-dashed border-surface-700 rounded-xl bg-surface-900/50"
      >
        <div
          class="bg-surface-800/50 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-3"
        >
          <font-awesome-icon :icon="faUsers" class="h-10 text-surface-600" />
        </div>
        <p class="text-gray-400 text-sm font-medium">
          Wybierz drużynę aby zarządzać jej budżetem i kartami
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useToast } from 'vue-toastification'
import { faUsers, faCoins, faLock } from '@fortawesome/free-solid-svg-icons'
import Dropdown from 'primevue/dropdown'
import InputNumber from 'primevue/inputnumber'
import Button from 'primevue/button'
import ProgressSpinner from 'primevue/progressspinner'

import apiService from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'

// --- DEFINICJE INTERFEJSÓW ---
interface Team {
  teamId: number
  teamName: string
  teamBud: number
  deckId: number
}

interface DecisionCard {
  id: number
  title: string
  description: string
}

interface CardsApiResponse {
  decisionCards: DecisionCard[]
  itemCards: any[]
}

// --- ZMIENNE REAKTYWNE ---
const toast = useToast()
const route = useRoute()
const gameId = Number(route.params.gameId)

const teams = ref<Team[]>([])
const decisionCards = ref<DecisionCard[]>([])
const selectedTeamId = ref<number | null>(null)
const selectedCardId = ref<number | null>(null)
const budgetInputValue = ref<number>(0)

const loading = reactive({
  teams: true,
  cards: false,
})

const isSavingBudget = ref(false)
const isUnlockingCard = ref(false)

// --- COMPUTED ---
const selectedTeam = computed<Team | undefined>(() => {
  if (!selectedTeamId.value) return undefined
  return teams.value.find((t) => t.teamId === selectedTeamId.value)
})

const selectedCard = computed<DecisionCard | undefined>(() => {
  if (!selectedCardId.value) return undefined
  return decisionCards.value.find((c) => c.id === selectedCardId.value)
})

// --- FUNKCJE ---
const fetchTeams = async () => {
  loading.teams = true
  try {
    const response = await apiService.get(apiConfig.player.getTeamsManagement(gameId))
    teams.value = response.data as Team[]
  } catch (error) {
    toast.error('Nie udało się pobrać listy drużyn.')
    console.error('Błąd podczas pobierania drużyn:', error)
  } finally {
    loading.teams = false
  }
}

const fetchDecisionCards = async (teamId: number) => {
  loading.cards = true
  try {
    const selectedTeam = teams.value.find((team) => team.teamId === teamId)

    if (!selectedTeam) {
      toast.error('Nie można odnaleźć wybranej drużyny.')
      loading.cards = false
      return
    }

    const response = await apiService.get<CardsApiResponse>(
      apiConfig.player.getDecisionCards(selectedTeam.deckId, gameId, selectedTeam.teamId),
    )

    if (response.data && Array.isArray(response.data.decisionCards)) {
      const sortedCards = response.data.decisionCards.sort((a, b) => a.id - b.id)
      decisionCards.value = sortedCards
    } else {
      decisionCards.value = []
      console.warn(
        "Odpowiedź z API nie zawierała oczekiwanej tablicy 'decisionCards'.",
        response.data,
      )
    }
  } catch (error) {
    toast.error('Nie udało się pobrać listy kart decyzji.')
    console.error('Błąd podczas pobierania kart decyzji:', error)
  } finally {
    loading.cards = false
  }
}

const saveBudget = async () => {
  if (!selectedTeam.value) {
    toast.warning('Najpierw wybierz drużynę.')
    return
  }

  const { teamId, teamName } = selectedTeam.value

  isSavingBudget.value = true
  try {
    await apiService.put(apiConfig.player.updateTeamBudget(gameId, teamId), {
      newBudget: budgetInputValue.value,
    })
    toast.success(`Zapisano nowy budżet dla drużyny "${teamName}".`)
    await fetchTeams()
  } catch (error: any) {
    toast.error(`Błąd podczas zapisywania budżetu dla "${teamName}".`)
    console.error('Błąd podczas aktualizacji budżetu:', error)
  } finally {
    isSavingBudget.value = false
  }
}

const handleUnlockCard = async () => {
  if (!selectedTeamId.value || !selectedCardId.value) {
    toast.warning('Proszę wybrać drużynę i kartę.')
    return
  }

  const teamName = teams.value.find((t) => t.teamId === selectedTeamId.value)?.teamName
  const cardName = decisionCards.value.find((c) => c.id === selectedCardId.value)?.title

  isUnlockingCard.value = true
  try {
    const payload = {
      cardId: selectedCardId.value,
      teamId: selectedTeamId.value,
    }
    const response = await apiService.post(apiConfig.player.unlockCard(gameId), payload)
    toast.success(
      (response.data as { message: string }).message ||
        `Pomyślnie odblokowano kartę "${cardName}" dla drużyny ${teamName}.`,
    )
    selectedCardId.value = null
  } catch (error: any) {
    if (error?.response?.status === 409) {
      toast.warning(error.response.data.message || 'Ta karta jest już odblokowana.')
    } else {
      toast.error('Wystąpił błąd podczas odblokowywania karty.')
    }
    console.error('Błąd podczas akcji na karcie:', error)
  } finally {
    isUnlockingCard.value = false
  }
}

// --- WATCHERY ---
watch(selectedTeam, (newTeam) => {
  budgetInputValue.value = newTeam ? newTeam.teamBud : 0
})

watch(selectedTeamId, (newTeamId) => {
  decisionCards.value = []
  selectedCardId.value = null

  if (newTeamId !== null) {
    fetchDecisionCards(newTeamId)
  } else {
    loading.cards = false
  }
})

// --- CYKL ŻYCIA KOMPONENTU ---
onMounted(() => {
  if (isNaN(gameId)) {
    toast.error('Błąd: Nieprawidłowy ID gry w adresie URL!')
    loading.teams = false
    return
  }
  fetchTeams()
})
</script>
