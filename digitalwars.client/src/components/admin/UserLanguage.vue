<template>
  <div class="h-full w-full">
    <div class="flex items-center justify-center mb-6">
      <h2 class="text-2xl font-nasalization">{{ t('accountLanguage') }}</h2>
    </div>
    <hr class="border-lgray-accent mb-4" />

    <Dropdown
      v-model="selectedLanguage"
      :options="languages"
      optionLabel="name"
      optionValue="code"
      class="w-full"
    >
      <template #value="slotProps">
        <div v-if="slotProps.value" class="flex gap-2 items-center">
          <span :class="getLanguageIcon(slotProps.value)"></span>
          <span>{{ getLanguageName(slotProps.value) }}</span>
        </div>
        <span v-else>
          {{ slotProps.placeholder }}
        </span>
      </template>

      <template #option="slotProps">
        <div class="flex gap-2 items-center">
          <span :class="slotProps.option.icon"></span>
          <span>{{ slotProps.option.name }}</span>
        </div>
      </template>
    </Dropdown>

    <button
      type="button"
      @click="saveLanguage"
      class="relative w-full py-4 mt-6 rounded-lg font-medium transition-all duration-300 overflow-hidden group text-white mb-5 bg-gradient-to-r from-primary-600 to-primary-700 hover:from-primary-500 hover:to-primary-600 shadow-lg shadow-primary-500/30 hover:shadow-primary-500/50"
    >
      <span class="relative z-10">{{ t('saveChanges') }}</span>
      <div
        class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-400/20 to-primary-400/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
      ></div>
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import Dropdown from 'primevue/dropdown'
import { useSettingsStore } from '@/stores/settingsStore'

const settingsStore = useSettingsStore()
const { t, locale } = useI18n()

const languages = [
  { name: 'English', code: 'en', icon: 'fi fi-gb' },
  { name: 'Polski', code: 'pl', icon: 'fi fi-pl' },
]

const selectedLanguage = ref(settingsStore.language || locale.value)

const getLanguageIcon = (code: string) => {
  return languages.find((l) => l.code === code)?.icon || ''
}

const getLanguageName = (code: string) => {
  return languages.find((l) => l.code === code)?.name || code
}

const saveLanguage = () => {
  settingsStore.language = selectedLanguage.value

  locale.value = selectedLanguage.value
}

onMounted(() => {
  if (!selectedLanguage.value) {
    selectedLanguage.value = 'pl'
  }
})
</script>
