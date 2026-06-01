<template>
  <nav
    :style="{ backgroundColor: navBgColor }"
    class="w-full py-2 px-4 flex justify-between items-center"
  >
    <!-- Logo -->
    <div>
      <RouterLink to="/">
        <img :src="logo" class="h-10" alt="IGNITE" />
      </RouterLink>
    </div>

    <!-- Nazwa drużyny na środku -->
    <div>
      <h1 class="text-lg font-semibold text-surface-0">{{ teamName }}</h1>
    </div>

    <div>
      <!-- Placeholder dla wyrównania -->
    </div>
  </nav>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import logo from '@/assets/logos/ignite_logo.svg'

const props = defineProps({
  teamName: {
    type: String,
    default: 'Test',
  },
  navBgColor: {
    type: String,
    default: '#141c2A',
  },
})

const navBgColor = computed(() => props.navBgColor)
const teamName = computed(() => props.teamName)
</script>
