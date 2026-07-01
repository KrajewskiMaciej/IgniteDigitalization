<template>
  <footer>

    <div
      class="relative flex items-center justify-center text-xs md:text-sm text-white w-full bg-secondary py-1 px-4 shadow-[0_-2px_4px_theme(colors.primary.400/0.5)]"
    >
      <span class="absolute left-4">{{ t('version') }}: {{ version }}</span>
      <RouterLink to="/">
        <img :src="logo" class="h-8" alt="Game Logo" />
      </RouterLink>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import logo from '@/assets/logos/Zasob_24x.jpg'

const { t } = useI18n()

const version = (import.meta.env.VITE_APP_VERSION as string) || 'dev';
</script>
