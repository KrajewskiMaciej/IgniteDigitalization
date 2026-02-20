<template>
  <div>
    <!-- Zmieniono na surface-950 dla najgłębszego tła -->
    <div class="relative flex flex-col h-screen w-screen overflow-hidden bg-secondary">
      <div class="relative z-20 flex flex-col h-full">
        <Navbar/>

        <div class="flex-1 relative">
          <AnimatedScene class="absolute inset-0" />

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
.disco-shadow {
  animation: disco 0.5s ease-in-out infinite;
}

@keyframes disco {
  0% {
    box-shadow: 0 0 60px 20px rgba(239, 68, 68, 0.7);
  }
  16% {
    box-shadow: 0 0 60px 20px rgba(249, 115, 22, 0.7);
  }
  33% {
    box-shadow: 0 0 60px 20px rgba(234, 179, 8, 0.7);
  }
  50% {
    box-shadow: 0 0 60px 20px rgba(34, 197, 94, 0.7);
  }
  66% {
    box-shadow: 0 0 60px 20px rgba(59, 160, 246, 0.7);
  }
  83% {
    box-shadow: 0 0 60px 20px rgba(168, 85, 247, 0.7);
  }
  200% {
    box-shadow: 0 0 60px 20px rgba(239, 68, 68, 0.7);
  }
}
</style>
