<template>
  <footer>
    <div
      class="text-center text-white w-full bg-surface-850 text-xs py-2 border-t-2 border-lgray-accent"
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
