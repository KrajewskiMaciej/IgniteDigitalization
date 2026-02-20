<template>
  <div
    v-if="position"
    class="fixed z-[9999] min-w-[300px] max-w-[500px] shadow-2xl animate-fade"
    :style="{
      top: `${position.y}px`,
      left: `${position.x}px`,
      transform: 'translate(-50%, -100%) translateY(-8px)',
    }"
  >
    <div class="bg-secondary rounded-t-lg px-4 py-3 border-b border-surface-200 text-center">
      <span class="font-bold text-surface-900">{{ card.title }}</span>
    </div>
    <div
      class="rounded-b-lg px-4 py-3 text-white flex flex-col text-center"
      :class="
        cardType === 'Decision'
          ? 'bg-blue-400'
          : cardType === 'Software'
            ? 'bg-green-400'
            : 'bg-orange-400'
      "
    >
      <span class="text-sm">{{ card.description }}</span>
      <span class="text-xs mt-2 opacity-70">ID karty: {{ card.id }}</span>
    </div>
    <div
      class="absolute -bottom-2 left-1/2 -translate-x-1/2 w-0 h-0 border-l-8 border-r-8 border-t-8 border-transparent"
      :class="
        cardType === 'Decision'
          ? 'border-t-blue-400'
          : cardType === 'Software'
            ? 'border-t-green-400'
            : 'border-t-orange-400'
      "
    ></div>
  </div>
</template>

<script setup lang="ts">
import type { IDecisonCard } from '@/types/Game'
import { computed } from 'vue'

const props = defineProps<{
  card: IDecisonCard
  cardType: 'Decision' | 'Software' | 'Hardware'
  nodeRef: HTMLElement | null
}>()

const position = computed(() => {
  if (!props.nodeRef) return null

  const rect = props.nodeRef.getBoundingClientRect()
  return {
    x: rect.left + rect.width / 2,
    y: rect.top,
  }
})
</script>
