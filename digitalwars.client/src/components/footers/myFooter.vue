<template>
  <footer>
    <div
      class="flex justify-center items-center text-xs md:text-sm text-white w-full bg-secondary py-2 shadow-[0_-2px_4px_theme(colors.primary.400/0.5)]"
    >
      Copyright
      <font-awesome-icon :icon="faCopyright" class="h-3 text-white" />
      {{ new Date().getFullYear() }}
      ITM Software House. All rights reserved.
    </div>
  </footer>
</template>

<script setup lang="ts">
import { faCopyright } from '@fortawesome/free-solid-svg-icons'
</script>
