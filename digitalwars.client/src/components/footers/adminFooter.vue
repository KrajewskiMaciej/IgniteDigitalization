<template>
  <footer>
    <div
      class="bg-secondary text-center text-white w-full text-xs py-2 border-t border-surface-700"
    >
      Copyright
      <font-awesome-icon :icon="faCopyright" class="h-3 text-white" />
      {{ new Date().getFullYear() }}
      ITM Software House. {{ t('allRightsReserved') }}
    </div>
  </footer>
</template>

<script setup lang="ts">
import { faCopyright } from '@fortawesome/free-solid-svg-icons'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
</script>
