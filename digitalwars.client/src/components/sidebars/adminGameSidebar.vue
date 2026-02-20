<template>
  <aside
    class="h-container bg-secondary text-white flex flex-col rounded-r-md border-t-2 border-r-2 border-solid border-lgray-accent"
    :class="isSideBarOpen ? 'w-64' : 'w-16'"
  >
    <div class="py-6 flex flex-row justify-between items-center px-4">
      <div></div>
      <h1 v-if="isSideBarOpen" class="text-xl font-bold text-white font-nasalization">
        DIGITAL WARS
      </h1>
      <div>
        <button @click="handleSidebar">
          <font-awesome-icon :icon="isSideBarOpen ? faArrowLeft : faArrowRight" class="h-4" />
        </button>
      </div>
    </div>

    <nav class="flex-1 px-2 py-2">
      <ul class="space-y-2">
        <li
          class="border-2 border-lgray-accent rounded-md hover:border-accent transition-colors duration-300 cursor-pointer"
        >
          <RouterLink
            :to="`/admin/game/${gameId}`"
            class="flex items-center gap-4 px-4 py-3 rounded-md"
            :class="isSideBarOpen ? '' : 'justify-center'"
          >
            <font-awesome-icon :icon="faTable" class="h-4 text-accent" />
            <span v-if="isSideBarOpen">{{ t('tables') }}</span>
          </RouterLink>
        </li>

        <li
          class="border-2 border-lgray-accent rounded-md hover:border-accent transition-colors duration-300 cursor-pointer"
        >
          <RouterLink
            :to="`/admin/game/market/${gameId}`"
            class="flex items-center gap-4 px-4 py-3 rounded-md"
            :class="isSideBarOpen ? '' : 'justify-center'"
          >
            <font-awesome-icon :icon="faUsers" class="h-4 text-accent" />
            <span v-if="isSideBarOpen">{{ t('marketManagementModule') }}</span>
          </RouterLink>
        </li>

        <li
          class="border-2 border-lgray-accent rounded-md hover:border-accent transition-colors duration-300 cursor-pointer"
        >
          <RouterLink
            :to="`/admin/game/${gameId}/table-management`"
            class="flex items-center gap-4 px-4 py-3 rounded-md"
            :class="isSideBarOpen ? '' : 'justify-center'"
          >
            <font-awesome-icon :icon="faUsersGear" class="h-4 text-accent" />
            <span v-if="isSideBarOpen">{{ t('tableManagementPanel') }}</span>
          </RouterLink>
        </li>

        <!-- <li
          class="border-2 border-lgray-accent rounded-md hover:border-accent transition-colors duration-300 cursor-pointer"
          @click="handleGameStats"
        >
          <div
            class="flex items-center px-4 py-3 rounded-md"
            :class="isSideBarOpen ? 'justify-between' : 'justify-center'"
          >
            <div>
              <font-awesome-icon
                :icon="faChartLine"
                class="h-4 text-accent"
                :class="isSideBarOpen ? 'mr-4' : 'mr-0'"
              />
              <span v-if="isSideBarOpen">{{ t('tableStatistics') }}</span>
            </div>
            <font-awesome-icon
              v-if="isSideBarOpen"
              :icon="isGameStatsDropdownOpen ? faArrowUp : faArrowDown"
              class="h-4"
            />
          </div>

          <div v-show="isGameStatsDropdownOpen" class="flex flex-col py-1 space-y-1">
            <RouterLink
              :to="{ path: `/admin/game/${gameId}/statistics`, query: { stat: 'positions' } }"
              class="px-4 py-2 hover:bg-secondary rounded-md transition-all duration-200"
              @click.stop
            >
              {{ t('finalPositions') }}
            </RouterLink>

            <RouterLink
              :to="{ path: `/admin/game/${gameId}/statistics`, query: { stat: 'success' } }"
              class="px-4 py-2 hover:bg-secondary rounded-md transition-all duration-200"
              @click.stop
            >
              {{ t('decisionEffectiveness') }}
            </RouterLink>

            <RouterLink
              :to="{ path: `/admin/game/${gameId}/statistics`, query: { stat: 'bits' } }"
              class="px-4 py-2 hover:bg-secondary rounded-md transition-all duration-200"
              @click.stop
            >
              {{ t('averageBitsPerRound') }}
            </RouterLink>

            <RouterLink
              :to="{ path: `/admin/game/${gameId}/statistics`, query: { stat: 'progress' } }"
              class="px-4 py-2 hover:bg-secondary rounded-md transition-all duration-200"
              @click.stop
            >
              {{ t('teamRankingOverRounds') }}
            </RouterLink>
          </div>
        </li> -->

        <li
          class="border-2 border-lgray-accent rounded-md hover:border-accent transition-colors duration-300 cursor-pointer"
        >
          <RouterLink
            :to="`/admin/game/${gameId}/dynamicCheatSheet`"
            class="flex items-center gap-4 px-4 py-3 rounded-md"
            :class="isSideBarOpen ? '' : 'justify-center'"
          >
            <font-awesome-icon :icon="faFile" class="h-4 text-accent" />
            <span v-if="isSideBarOpen">{{ t('dynamicGameMasterCheatSheet') }}</span>
          </RouterLink>
        </li>

        <li
          class="border-2 border-lgray-accent rounded-md hover:border-accent transition-colors duration-300 cursor-pointer"
        >
          <RouterLink
            to="/admin"
            class="flex items-center gap-4 px-4 py-3 rounded-md"
            :class="isSideBarOpen ? '' : 'justify-center'"
          >
            <font-awesome-icon :icon="faArrowLeft" class="h-4 text-accent" />
            <span v-if="isSideBarOpen">{{ t('adminPanel') }}</span>
          </RouterLink>
        </li>
      </ul>
    </nav>
  </aside>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'
import { ref } from 'vue'
import {
  faArrowDown,
  faArrowUp,
  faChartLine,
  faFile,
  faArrowLeft,
  faArrowRight,
  faUsers,
  faTable,
  faUsersGear,
} from '@fortawesome/free-solid-svg-icons'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const isSideBarOpen = ref(true)
const isGameManagerDropdownOpen = ref(false)
const isGameStatsDropdownOpen = ref(false)

const route = useRoute()
const gameId = route.params.gameId

const isGameTableManagerDropdownOpen = ref(false)

const handleGameTableManager = () => {
  isGameTableManagerDropdownOpen.value = !isGameTableManagerDropdownOpen.value
  if (!isSideBarOpen.value) isSideBarOpen.value = true
}

const handleSidebar = () => {
  isSideBarOpen.value = !isSideBarOpen.value
  isGameStatsDropdownOpen.value = false
  isGameManagerDropdownOpen.value = false
}

const handleGameStats = () => {
  isGameStatsDropdownOpen.value = !isGameStatsDropdownOpen.value
  if (!isSideBarOpen.value) isSideBarOpen.value = true
}

const handleGameManager = () => {
  isGameManagerDropdownOpen.value = !isGameManagerDropdownOpen.value
  if (!isSideBarOpen.value) isSideBarOpen.value = true
}
</script>
