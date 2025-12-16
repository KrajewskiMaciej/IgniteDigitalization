<template>
  <div
    class="group relative w-full border rounded-lg bg-surface-850 p-3 flex flex-col gap-2 text-white shadow-lg hover:shadow-xl hover:-translate-y-2 transition-all duration-300 cursor-pointer"
    :style="{ borderColor: props.color }"
    @dblclick="router.push(`/admin/game/${props.gameId}/${table.id}`)"
  >
    <!-- Header z nazwą i ID -->
    <div
      class="flex flex-row items-center justify-between pb-2 border-b"
      :style="{ borderColor: props.color + '30' }"
    >
      <div class="flex items-center min-w-0 flex-1 mr-2 overflow-hidden">
        <div class="min-w-0 flex-1">
          <h3 class="text-base font-bold break-words truncate" :style="{ color: props.color }">
            {{ table.name || t('tableWithoutName') }}
          </h3>
        </div>
      </div>
    </div>

    <!-- Przyciski kontrolne -->
    <div class="flex gap-2">
      <button
        @click.stop="openQrForTeam"
        class="flex-1 flex items-center justify-center gap-1.5 border bg-surface-800 py-1.5 px-2 rounded-md hover:scale-105 transition-all duration-300 text-xs font-medium"
        :style="{ borderColor: props.color + '40' }"
      >
        <font-awesome-icon :icon="faQrcode" class="h-3.5" :style="{ color: props.color }" />
        <span>{{ t('showQRCode') }}</span>
      </button>
    </div>

    <!-- Przycisk otwarcia stołu -->
    <RouterLink
      :to="`/admin/game/${props.gameId}/${table.id}`"
      @dblclick.stop
      class="w-full flex items-center justify-center gap-2 py-2 px-3 rounded-md font-semibold text-sm transition-all duration-300 hover:opacity-90 hover:scale-105"
      :style="{ backgroundColor: props.color, color: '#000' }"
    >
      <font-awesome-icon :icon="faMagnifyingGlass" class="h-3.5" />
      <span>{{ t('openTable') }}</span>
    </RouterLink>
  </div>

  <!-- Teleport Modal QR do body -->
  <Teleport to="body">
    <div
      v-if="showQr"
      class="fixed inset-0 flex items-center justify-center z-[9999] p-4"
      @click="showQr = false"
    >
      <div class="absolute inset-0 bg-black/60 backdrop-blur-sm"></div>
      <div
        class="bg-surface-800 text-white rounded-xl relative z-10 border p-8 w-full max-w-md shadow-2xl"
        :style="{ borderColor: props.color }"
        @click.stop
      >
        <!-- Przycisk zamknięcia -->
        <button
          @click="showQr = false"
          class="absolute top-4 right-4 w-8 h-8 flex items-center justify-center rounded-full hover:bg-white/10 transition-colors group"
        >
          <font-awesome-icon
            :icon="faXmark"
            class="h-5 text-white group-hover:text-primary-400 transition-colors"
          />
        </button>

        <!-- Nazwa drużyny -->
        <div class="mb-6">
          <h2 class="text-3xl font-bold text-center mb-2" :style="{ color: props.color }">
            {{ table.name || t('table') }}
          </h2>
          <div
            class="h-1 w-24 mx-auto rounded-full"
            :style="{ backgroundColor: props.color }"
          ></div>
        </div>

        <!-- QR Code -->
        <div class="bg-white p-6 rounded-xl shadow-inner mb-6">
          <div class="flex justify-center">
            <qrcode-vue :value="props.gameUrl" :size="qrSize" />
          </div>
        </div>

        <!-- Token -->
        <div class="text-center space-y-3">
          <div class="inline-block px-4 py-2 rounded-lg border border-surface-700">
            <p class="text-xs text-surface-400 mb-1">{{ t('teamToken') }}</p>
            <p class="text-6xl font-bold tracking-wider">
              {{ table.token }}
            </p>
          </div>

          <p class="text-xs text-surface-400 pt-2">{{ t('scanQrCodeToJoinTable') }}</p>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { faMagnifyingGlass, faQrcode, faXmark } from '@fortawesome/free-solid-svg-icons'
import { RouterLink, useRouter } from 'vue-router'
import QrcodeVue from 'qrcode.vue'
import type { PropType } from 'vue'
import { useI18n } from 'vue-i18n'


const { t } = useI18n();

// --- DEFINICJE INTERFEJSÓW ---
interface Table {
  id: number
  name?: string
  token: string
}

// --- PROPSY I ROUTER ---
const props = defineProps({
  gameId: {
    type: Number,
    required: true,
  },
  table: {
    type: Object as PropType<Table>,
    required: true,
  },
  color: {
    type: String,
    required: true,
  },
  token: {
    type: String,
    required: true,
  },
  gameUrl: {
    type: String,
    required: true,
  },
})

const router = useRouter()

// --- LOGIKA KODU QR ---
const showQr = ref(false)

const qrSize = computed(() => {
  if (typeof window === 'undefined') return 300

  const width = window.innerWidth
  if (width < 640) return 200
  if (width < 768) return 250
  if (width < 1024) return 300
  return 350
})

function openQrForTeam() {
  showQr.value = true
}
</script>
