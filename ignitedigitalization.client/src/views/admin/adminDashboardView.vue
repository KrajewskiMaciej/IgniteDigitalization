<template>
  <div class="flex flex-col h-screen bg-tertiary">
    <AdminNavbar />

    <div class="flex flex-1 mt-2 overflow-hidden">
      <mySidebar class="flex-shrink-0" />

      <!--Content-->
      <div
        class="flex-1 ml-4 bg-secondary border-t border-l border-b border-surface-700 rounded-md flex flex-col overflow-auto"
      >
        <RouterView class="flex-1" />
      </div>
    </div>
    <div class="mt-2">
      <Footer />
    </div>
  </div>
</template>

<script setup lang="ts">
import AdminNavbar from '@/components/navbars/adminNavbar.vue'
import mySidebar from '@/components/sidebars/adminSidebar.vue'
import Footer from '@/components/footers/myFooter.vue'
import { RouterView } from 'vue-router'
</script>
