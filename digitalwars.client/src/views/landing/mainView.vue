<template>
  <!--Jest to główny widok, który użytkownik widzi po przejsciu na stronę gry-->

  <div
    class="bg-gradient-to-br from-surface-800 via-surface-850 to-surface-900 flex flex-col h-screen w-screen"
  >
    <!--navbar-->
    <Navbar />

    <div class="flex-1 flex flex-col justify-center items-center text-center">
      <div class="mb-3 md:mb-4 lg:mb-6 xl:mb-8">
        <h1
          class="text-white font-bold tracking-wider font-nasalization animate-glow text-5xl lg:text-6xl xl:text-7xl mb-1 sm:mb-2 md:mb-3 lg:mb-5 xl:mb-7"
        >
          DIGITAL
        </h1>
        <h1
          class="text-white font-bold tracking-wider font-nasalization animate-glow text-5xl lg:text-6xl xl:text-7xl mb-1 sm:mb-2 md:mb-3 lg:mb-5 xl:mb-7"
        >
          WARS
        </h1>
      </div>

      <!--Switch Game Master / Player -->
      <div
        class="flex items-center text-white bg-gradient-to-r from-surface-850 to-surface-800 border border-primary-500 shadow-2xl px-2 py-2 rounded-full mb-11 gap-1.5 backdrop-blur-sm"
      >
        <div
          @click="currentView = 'game master'"
          class="flex items-center px-8 py-3.5 rounded-full cursor-pointer transition-all duration-300 ease-out relative overflow-hidden group"
          :class="
            currentView === 'game master'
              ? 'bg-gradient-to-r from-primary-400 to-primary-500 font-semibold shadow-lg shadow-primary-500/50 scale-[1.02]'
              : 'bg-surface-800/50 hover:bg-gradient-to-r hover:from-primary-400/20 hover:to-primary-500/20 hover:shadow-md hover:shadow-primary-500/20'
          "
        >
          <span class="mr-2.5 relative z-10">{{ t('gameMaster') }}</span>
          <font-awesome-icon :icon="faUserGear" class="h-4 w-4 relative z-10" />
          <div
            v-if="currentView !== 'game master'"
            class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
          />
        </div>

        <div
          @click="currentView = 'player'"
          class="flex items-center px-8 py-3.5 rounded-full cursor-pointer transition-all duration-300 ease-out relative overflow-hidden group"
          :class="
            currentView === 'player'
              ? 'bg-gradient-to-r from-primary-400 to-primary-500 font-semibold shadow-lg shadow-primary-500/50 scale-[1.02]'
              : 'bg-surface-800/50 hover:bg-gradient-to-r hover:from-primary-400/20 hover:to-primary-500/20 hover:shadow-md hover:shadow-primary-500/20'
          "
        >
          <span class="mr-2.5 relative z-10">{{ t('player') }}</span>
          <font-awesome-icon :icon="faUser" class="h-4 w-4 relative z-10" />
          <div
            v-if="currentView !== 'player'"
            class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
          />
        </div>
      </div>

      <!--Przycisk do logowania/rejestraci oraz do dołączenia do gry przez kod-->
      <div
        class="w-full space-y-3 sm:space-y-4 max-w-xs sm:max-w-sm md:max-w-md lg:max-w-lg px-2 sm:px-4 md:px-0"
      >
        <Button
          v-if="currentView === 'game master'"
          @click="handleLoginClick"
          :label="t('createGameAsGM')"
          outlined
          rounded
          raised
          class="w-full text-surface-0 py-2"
          :pt="{ label: { class: 'text-lg font-bold' } }"
        />
        <Button
          v-if="currentView === 'player'"
          @click="showJoinByCode = true"
          :label="t('joinGameAsPlayer')"
          outlined
          rounded
          raised
          class="w-full text-surface-0 py-2"
          :pt="{ label: { class: 'text-lg font-bold' } }"
        />
      </div>

      <!--Formularz logowania/rejestracji-->
      <LoginRegister :is-visible="showAuthModal" @close="showAuthModal = false" />

      <joinByCode :is-visible="showJoinByCode" @close="showJoinByCode = false" />
    </div>
    <Footer />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import LoginRegister from '@/components/auth/loginRegister.vue'
import joinByCode from '@/components/game/joinGameByCode.vue'
import Footer from '@/components/footers/myFooter.vue'
import Navbar from '@/components/navbars/myNavbar.vue'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'
import { faUser, faUserGear } from '@fortawesome/free-solid-svg-icons'
import { useI18n } from 'vue-i18n'
import Button from 'primevue/button'

const { t } = useI18n()
const authStore = useAuthStore()

//Jest to zmienna od której zależy czy formularz logowania/rejestracji jest wyświetlony
const showAuthModal = ref<boolean>(false)
const showJoinByCode = ref<boolean>(false)

const currentView = ref<'game master' | 'player'>('game master')

onMounted(() => {
  if (sessionStorage.getItem('showLoginAfterRedirect') === 'true') {
    sessionStorage.removeItem('showLoginAfterRedirect')
    showAuthModal.value = true
  }
})

const handleLoginClick = () => {
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
      0 0 15px #a78bfa,
      0 0 20px #a78bfa;
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
    0%,
    100% {
      color: white;
      text-shadow: none;
    }
    50% {
      color: #a78bfa;
      text-shadow:
        0 0 3px #a78bfa,
        0 0 6px #a78bfa,
        0 0 9px #a78bfa;
    }
  }
}
</style>
