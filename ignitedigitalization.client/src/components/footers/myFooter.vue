<template>
  <footer>
    <div
      class="flex justify-center items-center text-xs md:text-sm text-white w-full bg-secondary py-2 shadow-[0_-2px_4px_theme(colors.primary.400/0.5)]"
    >
      <div>
        <RouterLink to="/">
          <img :src="logo" class="h-8" alt="Game Logo" />
        </RouterLink>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import logo from '@/assets/logos/Zasob_24x.png'

const { t } = useI18n()
</script>
