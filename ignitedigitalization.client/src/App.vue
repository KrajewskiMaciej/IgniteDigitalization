<script setup lang="ts">
import { RouterView } from 'vue-router'
import ConfirmDialog from './components/ConfirmDialog.vue'
import BugReportButton from './components/BugReportButton.vue'
</script>

<template>
  <router-view />
  <ConfirmDialog />
  <BugReportButton />
</template>
