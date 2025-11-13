<template>
  <div class="flex flex-col h-full justify-center items-center text-center text-white p-4">
    <!-- Logo gry (skopiowane z mainView) -->
    <div class="mb-8">
      <h1 class="text-white font-bold tracking-wider font-nasalization animate-glow text-6xl mb-5">
        DIGITAL
      </h1>
      <h1 class="text-white font-bold tracking-wider font-nasalization animate-glow text-6xl mb-5">
        WARS
      </h1>
    </div>

    <!-- Komunikat błędu -->
    <div class="bg-red-900/50 border border-red-500 rounded-lg p-6 max-w-md w-full">
      <h2 class="text-2xl font-semibold mb-4 text-red-300">{{ title }}</h2>
      <p class="text-lg">{{ message }}</p>
    </div>

    <!-- Przycisk powrotu -->
    <button
      @click="goHome"
      class="mt-8 bg-tertiary hover:bg-accent text-white px-8 py-3 rounded-full font-medium transition-all duration-300"
    >
      Powrót na stronę główną
    </button>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'

defineProps({
  title: {
    type: String,
    required: true,
  },
  message: {
    type: String,
    required: true,
  },
})

const router = useRouter()
const goHome = () => {
  router.push('/')
}
</script>

<style scoped>
/* Możesz skopiować tutaj style animacji 'glow' z mainView.vue */
@keyframes glow {
  0%,
  100% {
    color: white;
    text-shadow: none;
  }
  50% {
    color: #a78bfa;
    text-shadow:
      0 0 5px #a78bfa,
      0 0 10px #a78bfa,
      0 0 15px #a78bfa;
  }
}
.animate-glow {
  animation: glow 4s infinite;
}
</style>
