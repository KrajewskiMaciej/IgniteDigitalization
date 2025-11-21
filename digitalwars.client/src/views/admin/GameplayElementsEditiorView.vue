<template>
  <div>
    <div class="w-full flex justify-center mt-6">
      <div
        class="flex gap-4 bg-surface-800/80 px-8 py-4 rounded-xl shadow-md border border-surface-700/60 backdrop-blur-sm"
      >
        <Button
          :label="'Karty Decyzji'"
          @click="currentView = 'decisions'"
          outlined
          size="large"
          :severity="currentView === 'decisions' ? undefined : 'secondary'"
        >
          <template #icon>
            <font-awesome-icon :icon="faClone" class="mr-2" />
          </template>
        </Button>

        <Button
          :label="'Przedmioty'"
          @click="currentView = 'items'"
          outlined
          size="large"
          :severity="currentView === 'items' ? undefined : 'secondary'"
        >
          <template #icon>
            <font-awesome-icon :icon="faMicrochip" class="mr-2" />
          </template>
        </Button>

        <Button
          :label="'Procesy'"
          @click="currentView = 'processes'"
          outlined
          size="large"
          :severity="currentView === 'processes' ? undefined : 'secondary'"
        >
          <template #icon>
            <font-awesome-icon :icon="faChessPawn" class="mr-2" />
          </template>
        </Button>
      </div>
    </div>

    <div class="mt-6">
      <EditDecisionCards v-if="currentView === 'decisions'" />
      <EditItems v-else-if="currentView === 'items'" />
      <EditProccesses v-else-if="currentView === 'processes'" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import EditDecisionCards from '@/components/game/EditDecisionCards.vue'
import EditItems from '@/components/game/EditItems.vue'
import EditProccesses from '@/components/game/EditProccesses.vue'
import Button from 'primevue/button'
import { faMicrochip, faChessPawn, faClone } from '@fortawesome/free-solid-svg-icons'

const currentView = ref<'items' | 'decisions' | 'processes'>('decisions')
</script>
