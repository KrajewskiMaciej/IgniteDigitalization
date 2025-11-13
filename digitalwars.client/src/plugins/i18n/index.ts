import type { DefaultLocaleMessageSchema } from 'vue-i18n'
import { createI18n } from 'vue-i18n'
import { en } from './en'
import { pl } from './pl'

const messages = {
  en,
  pl,
}

export const i18n = createI18n({
  legacy: false,
  locale: 'pl',
  fallbackLocale: 'en',
  messages,
})

type MessageSchema = typeof en & DefaultLocaleMessageSchema

declare module 'vue-i18n' {
  export interface DefineLocaleMessage extends MessageSchema {}
}
