<template>
  <div>
    <div
      ref="nodeRef"
      @mouseenter="showToolTip = true"
      @mouseleave="showToolTip = false"
      class="border-2 border-surface-700 rounded-lg p-2 flex flex-col items-center animate-fade justify-center min-w-[50px] cursor-pointer hover:border-dashed hover:border-primary-400 transition-all duration-300 ease-in-out"
      :class="
        props.data.cardType === 'Decision'
          ? 'bg-blue-400'
          : props.data.cardType === 'Software'
            ? 'bg-green-400'
            : 'bg-orange-400'
      "
    >
      <div class="font-nasalization font-bold text-white text-lg">
        {{ data.label }}
      </div>
      <div v-if="data.tables && data.tables.length > 0" class="flex gap-1 flex-wrap justify-center">
        <div
          v-for="table in data.tables"
          :key="table.teamId"
          :style="{ backgroundColor: table.teamColor }"
          :title="table.teamName"
          class="h-2 w-2 rounded-full border border-surface-700"
        ></div>
      </div>

      <Handle
        type="target"
        :position="data.layoutDirection === 'TB' ? Position.Top : Position.Left"
        class="!bg-primary-400"
      />
      <Handle
        type="source"
        :position="data.layoutDirection === 'TB' ? Position.Bottom : Position.Right"
        class="!bg-primary-400"
      />
    </div>

    <Teleport to="body">
      <DecisionCard
        v-if="showToolTip"
        :card="props.data.card"
        :card-type="props.data.cardType"
        :node-ref="nodeRef"
        @close="showToolTip = false"
      />
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { Handle, Position } from '@vue-flow/core'
import { ref } from 'vue'
import DecisionCard from './DecisionCard.vue'

const props = defineProps<{
  id: string
  data: {
    label: string
    cardType: 'Decision' | 'Software' | 'Hardware'
    card: any
    tables: Array<{ teamId: number; teamColor: string; teamName: string }>
    layoutDirection: 'LR' | 'TB'
  }
}>()

const showToolTip = ref(false)
const nodeRef = ref<HTMLElement | null>(null)
</script>
