<template>
  <Button
    v-bind="filteredAttrs"
    :loading="isLoading"
    :disabled="isDisabled"
    @click="handleClick"
  >
    <template v-for="(_, name) in $slots" #[name]="slotProps">
      <slot :name="name" v-bind="slotProps ?? {}" />
    </template>
  </Button>
</template>

<script setup lang="ts">
import { ref, useAttrs, computed } from 'vue'
import Button from 'primevue/button'

defineOptions({ inheritAttrs: false })

const attrs = useAttrs()
const isRunning = ref(false)

const filteredAttrs = computed(() => {
  const { onClick, loading, disabled, ...rest } = attrs as Record<string, unknown>
  return rest
})

const isLoading = computed(() => isRunning.value || !!(attrs.loading as boolean | undefined))
const isDisabled = computed(() => isRunning.value || !!(attrs.disabled as boolean | undefined))

const handleClick = async (event: MouseEvent) => {
  if (isRunning.value) return
  const handler = attrs.onClick as ((...args: unknown[]) => unknown) | undefined
  if (!handler) return
  const result = handler(event)
  if (result instanceof Promise) {
    isRunning.value = true
    try {
      await result
    } finally {
      isRunning.value = false
    }
  }
}
</script>
