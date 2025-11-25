<template>
  <div v-if="isVisible" class="fixed inset-0 flex items-center justify-center z-50">
    <!-- Tło -->
    <div class="absolute inset-0 bg-black/70" @click="handleClose"></div>

    <!-- Modal -->
    <div
      class="bg-surface-800 z-20 text-white relative border border-surface-700 animate-jump-in w-full h-full flex flex-col justify-center p-6 sm:w-[90vw] sm:max-w-lg sm:h-auto sm:max-h-[90vh] sm:rounded-lg sm:p-8"
    >
      <!-- Treść główna -->
      <div class="flex flex-col justify-center items-center text-center px-2">
        <h1 class="text-white font-nasalization text-xl md:text-2xl mb-4">
          Samodzielne decyzje drużyny
        </h1>

        <p class="text-base text-gray-200">
          Drużyna
          <span class="text-primary-400 font-semibold">{{ teamName }}</span>
          może podejmować
          <span class="text-green-400 font-bold">samodzielne decyzje</span>.
        </p>

        <p class="font-bold text-surface-200 mt-2 text-sm md:text-base">
          Decyzje tej drużyny będą wykonywane bez akceptacji Game Mastera.
        </p>
      </div>

      <hr class="my-6 border-surface-700" />

      <!-- Progres -->
      <div class="space-y-2 px-2">
        <div class="flex items-center justify-between text-xs text-surface-400">
          <span>Okno zamknie się automatycznie</span>
          <span>za {{ secondsLeft }} s</span>
        </div>

        <div class="w-full h-2 bg-surface-900 rounded-full overflow-hidden">
          <div
            class="h-full bg-primary-400 transition-all duration-100"
            :style="{ width: progressPercent + '%' }"
          ></div>
        </div>
      </div>

      <!-- Przyciski -->
      <div class="mt-6 flex justify-center px-2">
        <Button type="button" class="px-6 w-full sm:w-auto" @click="handleClose" label="OK" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onUnmounted } from 'vue'
import Button from 'primevue/button'

const props = defineProps<{
  isVisible: boolean
  teamName: string
}>()

const emit = defineEmits<{
  (e: 'close'): void
}>()

const TOTAL_TIME_MS = 10_000
const remainingTime = ref(TOTAL_TIME_MS)
let intervalId: number | null = null

const clearTimer = () => {
  if (intervalId !== null) {
    clearInterval(intervalId)
    intervalId = null
  }
}

const startTimer = () => {
  clearTimer()
  remainingTime.value = TOTAL_TIME_MS

  intervalId = window.setInterval(() => {
    remainingTime.value -= 100
    if (remainingTime.value <= 0) {
      remainingTime.value = 0
      clearTimer()
      handleClose()
    }
  }, 100)
}

const handleClose = () => {
  clearTimer()
  emit('close')
}

const progressPercent = computed(() => {
  return (remainingTime.value / TOTAL_TIME_MS) * 100
})

const secondsLeft = computed(() => {
  return Math.ceil(remainingTime.value / 1000)
})

watch(
  () => props.isVisible,
  (visible) => {
    if (visible) {
      startTimer()
    } else {
      clearTimer()
    }
  },
  { immediate: true },
)

onUnmounted(() => {
  clearTimer()
})
</script>
