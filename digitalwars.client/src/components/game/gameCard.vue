<template>
  <div
    class="group relative w-full border-2 rounded-lg bg-surface-850 p-3 flex flex-col gap-2 text-white shadow-lg hover:shadow-xl hover:-translate-y-2 transition-all duration-300 cursor-pointer"
    :style="{ borderColor: props.color }"
    @dblclick="router.push(`/admin/game/market/${game.id}`)"
  >
    <!-- Header z nazwą i statusem -->
    <div
      class="flex flex-row items-center justify-between pb-2 border-b"
      :style="{ borderColor: props.color + '30' }"
    >
      <div class="flex items-center min-w-0 flex-1 mr-2 overflow-hidden">
        <div class="min-w-0 flex-1">
          <h3 class="text-base font-bold break-words truncate" :style="{ color: props.color }">
            {{ game.name }}
          </h3>
          <div class="flex gap-1.5 items-center mt-1">
            <div class="rounded-full w-2 h-2" :class="getStatus(game.status).color"></div>
            <span class="text-xs text-gray-400">{{ getStatus(game.status).text }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Przyciski kontrolne -->
    <div class="flex gap-2">
      <button
        v-if="game.status === 'During' || game.status === 'Paused'"
        @click.stop="handleStatusChange(game.id, game.status === 'During' ? 'Paused' : 'During')"
        class="flex-1 flex items-center justify-center gap-1.5 border bg-surface-800 py-1.5 px-2 rounded-md hover:scale-105 transition-all duration-300 text-xs font-medium"
        :style="{ borderColor: props.color + '40' }"
      >
        <font-awesome-icon
          :icon="game.status === 'During' ? faCircleStop : faCirclePlay"
          class="h-3.5"
          :style="{ color: props.color }"
        />
        <span>{{ game.status === 'During' ? 'Wstrzymaj' : 'Wznów' }}</span>
      </button>

      <button
        v-if="game.status !== 'End'"
        @click.stop="handleEndGame(game.id)"
        class="flex-1 flex items-center justify-center gap-1.5 border border-red-500 bg-surface-800 py-1.5 px-2 rounded-md hover:border-red-500 hover:bg-red-500/10 hover:scale-105 transition-all duration-300 text-xs font-medium"
      >
        <font-awesome-icon :icon="faPowerOff" class="h-3.5 text-red-500" />
        <span class="text-red-500">Zakończ</span>
      </button>
    </div>

    <!-- Przycisk otwarcia gry -->
    <RouterLink
      :to="`/admin/game/${game.id}`"
      @dblclick.stop
      class="w-full flex items-center justify-center gap-2 py-2 px-3 rounded-md font-semibold text-sm transition-all duration-300 hover:opacity-90 hover:scale-105"
      :style="{ backgroundColor: props.color, color: '#000' }"
    >
      <font-awesome-icon :icon="faMagnifyingGlass" class="h-3.5" />
      <span>Otwórz grę</span>
    </RouterLink>
  </div>
</template>

<script setup lang="ts">
import {
  faCircleStop,
  faMagnifyingGlass,
  faCirclePlay,
  faPowerOff,
} from '@fortawesome/free-solid-svg-icons'
import { RouterLink, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'

const router = useRouter()
const confirm = useConfirm()

const props = defineProps({
  game: {
    type: Object,
    required: true,
  },
  color: {
    type: String,
    required: true,
  },
})

const emit = defineEmits(['update-status'])

const handleStatusChange = (gameId: any, newStatus: any) => {
  const action = newStatus === 'Paused' ? 'wstrzymać' : 'wznowić'

  confirm.require({
    header: `${newStatus === 'Paused' ? 'Wstrzymaj' : 'Wznów'} grę`,
    message: `Czy na pewno chcesz ${action} grę "${props.game.name}"?`,
    accept: () => {
      emit('update-status', { gameId, newStatus })
    },
    reject: () => {
      // Anulowano
    },
  })
}

const handleEndGame = (gameId: any) => {
  confirm.require({
    header: 'Zakończ grę',
    message: `Czy na pewno chcesz ZAKOŃCZYĆ grę "${props.game.name}"? Tej operacji NIE MOŻNA cofnąć.`,
    accept: () => {
      emit('update-status', { gameId, newStatus: 'End' })
    },
    reject: () => {
      // Anulowano
    },
  })
}

const getStatus = (status: string) => {
  switch (status) {
    case 'During':
      return {
        text: 'W trakcie',
        color: 'bg-green-500',
      }
    case 'Paused':
      return {
        text: 'Wstrzymana',
        color: 'bg-yellow-500',
      }
    case 'End':
      return {
        text: 'Zakończona',
        color: 'bg-red-500',
      }
    default:
      return {
        text: status,
        color: 'bg-gray-500',
      }
  }
}
</script>
