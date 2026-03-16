import { defineStore } from 'pinia'
import { useStorage } from '@vueuse/core'
import { useI18n } from 'vue-i18n'
import { primevueEn } from '@/plugins/i18n/primevueEn'
import { primevuePl } from '@/plugins/i18n/primevuePl'
import { usePrimeVue } from 'primevue/config'
import { watch } from 'vue'

export const useSettingsStore = defineStore('settings', () => {
  const primeVue = usePrimeVue()
  const { locale } = useI18n()

  const language = useStorage<string>('language', locale.value)

  const setLanguage = (lang: string) => {
    language.value = lang
    locale.value = lang
    localStorage.setItem('language', lang)
  }

  watch(
    language,
    () => {
      primeVue.config.locale = language.value === 'pl' ? primevuePl : primevueEn
    },
    { immediate: true },
  )

  return {
    language,
    setLanguage,
  }
})
