<template>
  <div>
    <div class="w-full flex justify-center mt-6">
      <div
        class="flex gap-4 bg-secondary/80 px-8 py-4 rounded-xl shadow-md border border-surface-700/60 backdrop-blur-sm"
      >
        <Button
          :label="t('decisionCards')"
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
          :label="t('items')"
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
          :label="t('processes')"
          @click="currentView = 'processes'"
          outlined
          size="large"
          :severity="currentView === 'processes' ? undefined : 'secondary'"
        >
          <template #icon>
            <font-awesome-icon :icon="faChessPawn" class="mr-2" />
          </template>
        </Button>

        <Button
          :label="t('enablers')"
          @click="currentView = 'enablers'"
          outlined
          size="large"
          :severity="currentView === 'enablers' ? undefined : 'secondary'"
        >
          <template #icon>
            <font-awesome-icon :icon="faLock" class="mr-2" />
          </template>
        </Button>
      </div>
    </div>

    <div class="mt-6">
      <EditDecisionCards v-model="selectedDeckId" v-if="currentView === 'decisions'" />
      <EditItems v-model="selectedDeckId" v-else-if="currentView === 'items'" />
      <EditProccesses v-model="selectedDeckId" v-else-if="currentView === 'processes'" />
      <DynamicCheatSheetEdit v-model="selectedDeckId" v-else-if="currentView === 'enablers'" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import EditDecisionCards from '@/components/game/EditDecisionCards.vue'
import EditItems from '@/components/game/EditItems.vue'
import EditProccesses from '@/components/game/EditProccesses.vue'
import Button from 'primevue/button'
import { faMicrochip, faChessPawn, faClone, faLock } from '@fortawesome/free-solid-svg-icons'
import DynamicCheatSheetEdit from '@/components/cheatSheet/DynamicCheatSheetEdit.vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const currentView = ref<'items' | 'decisions' | 'processes' | 'enablers'>('decisions')
const selectedDeckId = ref<number | undefined>(undefined)
</script>
