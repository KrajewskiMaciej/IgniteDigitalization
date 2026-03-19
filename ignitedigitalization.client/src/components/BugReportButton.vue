<template>
  <div class="fixed bottom-5 right-5 z-50">
    <Button
      :title="t('bugReportButtonTitle')"
      rounded
      severity="secondary"
      class="shadow-lg"
      @click="isModalVisible = true"
    >
      <template #icon>
        <font-awesome-icon :icon="faBug" class="mr-2 h-4 w-4" />
      </template>
      <span class="text-sm">{{ t('bugReportButtonTitle') }}</span>
    </Button>
    <BugReportModal v-model:visible="isModalVisible" />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import Button from 'primevue/button'
import { faBug } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import BugReportModal from './BugReportModal.vue'

const { t } = useI18n()
const isModalVisible = ref(false)
</script>
