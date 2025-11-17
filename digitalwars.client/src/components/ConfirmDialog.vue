<template>
  <ConfirmDialog :draggable="false">
    <template #container="{ message, acceptCallback, rejectCallback }">
      <div
        class="flex flex-col items-center rounded-md bg-surface-0 p-5 text-surface-700 dark:bg-surface-800 dark:text-surface-0/80"
      >
        <div
          class="-mt-14 flex h-24 w-24 items-center justify-center rounded-full bg-primary-500 text-surface-0/80 dark:border dark:border-surface-700 dark:bg-primary-400 dark:text-surface-700"
        >
          <font-awesome-icon :icon="faQuestion" class="h-12 w-12" />
        </div>
        <p class="mb-2 mt-4 text-2xl font-bold">{{ message.header }}</p>
        <p class="text-center">{{ message.message }}</p>
        <div class="mt-6 flex items-center gap-3">
          <Button @click="rejectCallback" :label="t('cancel')" outlined class="w-20" />
          <Button @click="acceptCallback" :label="t('accept')" class="w-20" />
        </div>
      </div>
    </template>
  </ConfirmDialog>
</template>

<script setup lang="ts">
import Button from 'primevue/button'
import ConfirmDialog from 'primevue/confirmdialog'
import { useI18n } from 'vue-i18n'
import { faQuestion } from '@fortawesome/free-solid-svg-icons'

const { t } = useI18n()
</script>
