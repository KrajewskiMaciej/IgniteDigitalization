<template>
  <div class="flex flex-col h-full justify-center items-center text-center text-surface-500 p-4">
    <!-- Logo gry (skopiowane z mainView) -->
    <div class="mb-3 md:mb-4 lg:mb-6 inline-flex flex-col items-stretch">
            <h1 class="
              text-surface-500 font-bold font-inter animate-glow w-full text-center
              text-5xl sm:text-6xl md:text-7xl lg:text-8xl xl:text-[190px]
              mb-1 sm:mb-2 md:mb-3 lg:mb-5 xl:mb-7
            ">
              IGNITE
            </h1>
            <h1 class="
              text-surface-500 font-bold font-inter animate-glow w-full text-center
              text-3xl sm:text-4xl md:text-5xl lg:text-6xl xl:text-[78px]
              mb-1 sm:mb-2 md:mb-3 lg:mb-5 xl:mb-7
            ">
              DIGITALIZATION
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
/*Animacja świecenia się napisu DIGITAL WARS, #a78bfa jest kolor akcentu z configu tailwind*/
@keyframes glow {
  0%, 100% {
    color: #1e293b;
    text-shadow: none;
  }
  50% {
    color: var(--color-primary);
    text-shadow: 
      0 0 5px var(--color-primary), 
      0 0 10px var(--color-primary), 
      0 0 15px var(--color-primary),
      0 0 20px var(--color-primary);
  }
}

.animate-glow {
  animation-name: glow;
  animation-duration: 4s;
  animation-delay: 4s;
  animation-iteration-count: infinite;
}

@media (max-width: 640px) {
  @keyframes glow {
    0%, 100% {
      color: #1e293b;
      text-shadow: none;
    }
    50% {
      color: var(--color-primary);
      text-shadow: 
        0 0 3px var(--color-primary), 
        0 0 6px var(--color-primary), 
        0 0 9px var(--color-primary);
    }
  }
}
</style>