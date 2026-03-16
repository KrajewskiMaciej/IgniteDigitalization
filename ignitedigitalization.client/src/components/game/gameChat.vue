<template>
  <div
    class="bg-tertiary border border-lgray-accent lg:rounded-lg overflow-hidden relative animate-fade-up flex flex-col h-full"
  >
    <!--Górny pasek-->
    <div class="bg-accent flex-shrink-0 border-b-tertiary">
      <div class="flex justify-center items-center p-4">
        <img :src="logo" class="h-12" alt="ITM logo" />
      </div>
      <font-awesome-icon
        :icon="faXmark"
        class="absolute right-2 top-2 h-6 text-surface-0 hover:text-surface-800 transition duration-300 ease-in-out cursor-pointer"
        @click="emit('closeChat')"
      />
    </div>

    <!--Wiadomości-->
    <div class="flex-1 overflow-y-auto p-4 space-y-6 custom-scrollbar">
      <div class="flex justify-start">
        <div
          class="bg-secondary text-white rounded-2xl rounded-bl-md px-4 py-2 max-w-xs break-words"
        >
          {{ t('chatWelcomeMessage', { cost: 10 }) }}
        </div>
      </div>

      <div
        v-for="message in messages"
        :key="message.id"
        class="mb-3"
        :class="message.isUser ? 'flex justify-end' : 'flex justify-start'"
      >
        <div
          class="text-white rounded-2xl px-4 py-3 max-w-sm whitespace-pre-wrap leading-relaxed"
          :class="message.isUser ? 'bg-accent rounded-br-md' : 'bg-gray-600 rounded-bl-md'"
          style="word-break: break-all; overflow-wrap: anywhere; hyphens: auto"
        >
          {{ message.text }}
        </div>
      </div>
    </div>

    <!--Input na wiadomości-->
    <div class="flex items-end p-4 flex-shrink-0">
      <textarea
        :placeholder="t('chatPlaceholder')"
        v-model="message"
        rows="1"
        class="flex-1 border border-lgray-accent bg-tertiary rounded-2xl px-2 py-2 text-white resize-none transition-all duration-300 focus:border-accent focus:ring-1 focus:ring-accent focus:outline-none min-h-[40px] max-h-40 overflow-y-hidden custom-scrollbar"
        @input="autoResize"
        @keydown.enter.prevent="sendMessage"
      />
      <div
        class="ml-2 h-10 w-10 rounded-full border border-lgray-accent bg-tertiary text-white flex items-center justify-center cursor-pointer hover:border-accent hover:text-accent transition-all duration-300 ease-in-out"
        @click="sendMessage"
      >
        <font-awesome-icon :icon="faPaperPlane" class="h-4" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { faXmark, faPaperPlane } from '@fortawesome/free-solid-svg-icons'
import logo from '@/assets/logos/Zasob_14x.png'
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

//Input użytkownika
const message = ref('')

//Wszystkie wiadomości
const messages = ref([])

const autoResize = (event) => {
  const textarea = event.target
  const scrollTop = textarea.scrollTop
  const cursorPosition = textarea.selectionStart

  const oldHeight = textarea.style.height
  textarea.style.height = 'auto'
  const newHeight = textarea.scrollHeight

  textarea.style.height = oldHeight

  if (newHeight <= 120) {
    textarea.style.height = Math.max(newHeight, 40) + 'px'
    textarea.style.overflowY = 'hidden'
  } else {
    textarea.style.height = '120px'
    textarea.style.overflowY = 'scroll'
  }

  textarea.scrollTop = scrollTop
  textarea.setSelectionRange(cursorPosition, cursorPosition)
}

// Funkcja wysyłania wiadomości
const sendMessage = () => {
  //Nie da się wysłać pustej wiadomości
  if (!message.value.trim()) return

  const userMessage = {
    id: Date.now(), // Na razie generuje id tak, poźniej to z backendu będzie
    text: message.value,
    isUser: true,
    timestamp: new Date(),
  }

  messages.value.push(userMessage)

  message.value = ''

  // Na razie odpowiedź zawsze taka sama i jest opóźnienie lekkie dodane żeby zasymulować odpowiedź od AI
  setTimeout(() => {
    const aiMessage = {
      id: Date.now() + 1,
      text: t('chatAiPlaceholder'),
      isUser: false,
      timestamp: new Date(),
    }
    messages.value.push(aiMessage)
  }, 1000)
}

const emit = defineEmits(['closeChat'])
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 0.6rem;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
  margin: 0.5rem 0.3rem;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: theme('colors.secondary');
  border-radius: 0.25rem;
  border: 0.1rem solid transparent;
  background-clip: content-box;
}
</style>
