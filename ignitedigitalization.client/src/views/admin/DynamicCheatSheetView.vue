<template>
  <DynamicCheatSheet :game-id="Number(props.gameId)" />
</template>

<script setup lang="ts">
import DynamicCheatSheet from '@/components/cheatSheet/DynamicCheatSheet.vue'

interface IProps {
  gameId: string
}

const props = defineProps<IProps>()
</script>
