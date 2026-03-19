<template>
  <div class="fixed bottom-5 right-5 z-50">
    <div
      class="bg-primary-500 flex justify-center items-center px-2 py-2 h-14 w-14 rounded-full border cursor-pointer border-lgray-accent text-white transition-all duration-300 ease-in-out hover:shadow-lg hover:shadow-primary-400/60 hover:border-primary-400 hover:text-primary-400 hover:scale-110 hover:-translate-y-2"
      :title="t('bugReportButtonTitle')"
      @click="isModalVisible = true"
    >
      <font-awesome-icon :icon="faBug" class="h-7" />
    </div>
    <BugReportModal v-model:visible="isModalVisible" />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { faBug } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import BugReportModal from './BugReportModal.vue'

const { t } = useI18n()
const isModalVisible = ref(false)
</script>
