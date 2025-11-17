<template>
  <div class="w-full">
    <label for="board-selector" class="block mb-2 text-sm font-medium text-surface-0">
      {{ activeView === 'add' ? 'Wybierz szablon planszy' : 'Wybierz planszę do edycji' }}
    </label>
    <div class="flex gap-2 items-start">
      <Dropdown
        id="board-selector"
        :modelValue="modelValue"
        @update:modelValue="handleSelectionChange"
        :options="boards"
        optionLabel="name"
        optionValue="boardId"
        placeholder="Wybierz planszę"
        class="flex-1"
        :showClear="activeView === 'add'"
      >
        <template #option="slotProps">
          <div class="flex items-center">
            <span>{{ slotProps.option.name }}</span>
          </div>
        </template>
      </Dropdown>

      <Button
        v-if="modelValue !== undefined && modelValue !== null"
        severity="danger"
        outlined
        @click="$emit('deleteBoard')"
        class="flex-shrink-0"
      >
        <font-awesome-icon :icon="faTrash" class="h-4" />
      </Button>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { PropType } from 'vue'
import Dropdown from 'primevue/dropdown'
import { faTrash } from '@fortawesome/free-solid-svg-icons'
import Button from 'primevue/button'

interface Board {
  boardId: number
  name: string
}

defineProps({
  boards: {
    type: Array as PropType<Board[]>,
    required: true,
  },
  modelValue: {
    type: Number as PropType<number | null>,
    default: null,
  },
  activeView: {
    type: String as PropType<'add' | 'edit'>,
    required: true,
  },
})

const emit = defineEmits(['update:modelValue', 'deleteBoard'])

const handleSelectionChange = (value: number | null) => {
  emit('update:modelValue', value)
}
</script>
