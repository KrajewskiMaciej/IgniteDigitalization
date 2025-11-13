import type { DefaultLocaleMessageSchema } from 'vue-i18n'
import { createI18n } from 'vue-i18n'
import { en } from './en'
import { pl } from './pl'

const messages = {
  en,
  pl,
}

function getInitialLocale(): string {
  const saved = localStorage.getItem('language')
  if (saved && saved in messages) return saved

  const browser = navigator.language.split('-')[0]
  if (browser in messages) return browser

  return 'pl'
}

export const i18n = createI18n({
  legacy: false,
  locale: getInitialLocale(),
  fallbackLocale: 'en',
  messages,
})

type MessageSchema = typeof en & DefaultLocaleMessageSchema

declare module 'vue-i18n' {
  export interface DefineLocaleMessage extends MessageSchema {}
}
