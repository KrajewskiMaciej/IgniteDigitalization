<template>
  <div class="flex flex-col min-h-screen bg-secondary">
    <AdminNavbar />

    <div class="flex flex-1 mt-2">
      <mySidebar class="flex-shrink-0" />

      <!--Content-->
      <div
        class="flex-1 ml-4 bg-secondary border-t border-l border-b border-surface-700 rounded-md flex flex-col"
      >
        <RouterView />
      </div>
    </div>
    <div class="mt-2">
      <Footer />
    </div>
  </div>
</template>

<script setup lang="ts">
import AdminNavbar from '@/components/navbars/adminNavbar.vue'
import mySidebar from '@/components/sidebars/adminGameSidebar.vue'
import Footer from '@/components/footers/adminFooter.vue'
</script>
