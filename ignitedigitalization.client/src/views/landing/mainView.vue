<template>
  <div>
    <!-- Zmieniono na surface-950 dla najgłębszego tła -->
    <div class="relative flex flex-col h-screen w-screen overflow-hidden bg-secondary">
      <div class="relative z-20 flex flex-col h-full">
        <Navbar/>

        <div class="flex-1 flex flex-col justify-center items-center text-center">

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

          <div class="absolute inset-0 flex flex-col justify-end items-center pb-16">
            <!-- Kontener przycisków: zastąpiono surface-850 przez surface-900 -->
            <div
              class="flex items-center text-white bg-gradient-to-r from-surface-900/90 to-surface-800/90 border border-primary-500/40 shadow-2xl px-2 py-2 rounded-full gap-1.5 backdrop-blur-sm"
            >
              <!-- Przycisk Game Master -->
              <div
                @click="handleGameMasterClick"
                class="flex items-center px-8 py-3.5 rounded-full cursor-pointer transition-all duration-300 ease-out relative overflow-hidden group bg-secondary hover:bg-primary-600 hover:shadow-lg hover:shadow-primary-500/40 hover:scale-[1.02]"
              >
                <span class="mr-2.5 relative z-20 font-semibold">{{ t('gameMaster') }}</span>
                <font-awesome-icon :icon="faUserGear" class="h-4 w-4 relative z-20" />
                <!-- Efekt błysku (Glint) -->
                <div
                  class="absolute inset-0 bg-gradient-to-r from-white/0 via-white/10 to-white/0 translate-x-[-200%] group-hover:translate-x-[200%] transition-transform duration-700"
                />
              </div>

              <!-- Przycisk Gracz -->
              <div
                @click="showJoinByCode = true"
                class="flex items-center px-8 py-3.5 rounded-full cursor-pointer transition-all duration-300 ease-out relative overflow-hidden group bg-secondary hover:bg-primary-600 hover:shadow-lg hover:shadow-primary-500/40 hover:scale-[1.02]"
              >
                <span class="mr-2.5 relative z-20 font-semibold">{{ t('player') }}</span>
                <font-awesome-icon :icon="faUser" class="h-4 w-4 relative z-20" />
                <div
                  class="absolute inset-0 bg-gradient-to-r from-white/0 via-white/10 to-white/0 translate-x-[-200%] group-hover:translate-x-[200%] transition-transform duration-700"
                />
              </div>
            </div>
          </div>
        </div>

        <Footer />
      </div>

      <!-- Komponenty modalne -->
      <NewAuth v-if="showAuthModal" @close="showAuthModal = false" />
      <joinByCode :is-visible="showJoinByCode" @close="showJoinByCode = false" />
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, shallowRef } from 'vue'
import LoginRegister from '@/components/auth/loginRegister.vue'
import joinByCode from '@/components/game/joinGameByCode.vue'
import Footer from '@/components/footers/myFooter.vue'
import Navbar from '@/components/navbars/myNavbar.vue'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'
import { faUser, faUserGear } from '@fortawesome/free-solid-svg-icons'
import { useI18n } from 'vue-i18n'
import { useCountdown } from '@vueuse/core'
import loginRegister from '@/components/auth/loginRegister.vue'
import NewAuth from '@/components/auth/NewAuth.vue'

const { t } = useI18n()
const authStore = useAuthStore()

const showAuthModal = ref<boolean>(false)
const showJoinByCode = ref<boolean>(false)

onMounted(() => {
  if (sessionStorage.getItem('showLoginAfterRedirect') === 'true') {
    sessionStorage.removeItem('showLoginAfterRedirect')
    showAuthModal.value = true
  }
})

const handleGameMasterClick = () => {
  if (authStore.isAuthenticated === true) {
    router.push('/admin')
  } else {
    showAuthModal.value = true
  }
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