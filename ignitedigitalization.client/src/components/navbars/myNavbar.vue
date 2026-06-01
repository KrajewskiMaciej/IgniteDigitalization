<template>
  <nav
    class="w-full bg-secondary py-2 px-4 flex flex-row md:justify-between items-center shadow-[0_2px_4px_theme(colors.primary.400/0.5)]"
  >
    <div class="flex items-center w-full" :class="isMdOrLarger ? '' : 'justify-center'">
      <img @click="onClickIgniteDigitalizationLogo" :src="logo" class="h-10" alt="IGNITE" />
    </div>
  </nav>
</template>

<script setup lang="ts">
import logo from '@/assets/logos/ignite_logo.svg'
import { useBreakpoints } from '@vueuse/core'
import { ref } from 'vue'

const clickCount = ref<number>(0)

const emit = defineEmits<{
  (e: 'open-video'): void
}>()

const onClickIgniteDigitalizationLogo = () => {
  clickCount.value += 1

  if (clickCount.value === 10) {
    clickCount.value = 0
    emit('open-video')
  }

  setTimeout(() => {
    clickCount.value = 0
  }, 5000)
}

const breakpoints = useBreakpoints({
  sm: 640,
  md: 768,
  lg: 1024,
  xl: 1280,
})

const isMdOrLarger = breakpoints.greaterOrEqual('md')
</script>
