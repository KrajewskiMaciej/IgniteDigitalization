<template>
<div>
    <div class="relative flex flex-col h-screen w-screen overflow-hidden bg-surface-900">
      <div class="relative z-10 flex flex-col h-full">
        <Navbar @open-video="handleShowVideo" />

        <div class="flex-1 relative">
          <AnimatedScene class="absolute inset-0" />

          <div class="absolute inset-0 flex flex-col justify-end items-center pb-16">
            <div
              class="flex items-center text-white bg-gradient-to-r from-surface-850/90 to-surface-800/90 border border-primary-500 shadow-2xl px-2 py-2 rounded-full gap-1.5 backdrop-blur-sm"
            >
              <div
                @click="handleGameMasterClick"
                class="flex items-center px-8 py-3.5 rounded-full cursor-pointer transition-all duration-300 ease-out relative overflow-hidden group bg-surface-800/50 hover:bg-gradient-to-r hover:from-primary-400 hover:to-primary-500 hover:shadow-lg hover:shadow-primary-500/50 hover:scale-[1.02]"
              >
                <span class="mr-2.5 relative z-10">{{ t('gameMaster') }}</span>
                <font-awesome-icon :icon="faUserGear" class="h-4 w-4 relative z-10" />
                <div
                  class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
                />
              </div>

              <div
                @click="showJoinByCode = true"
                class="flex items-center px-8 py-3.5 rounded-full cursor-pointer transition-all duration-300 ease-out relative overflow-hidden group bg-surface-800/50 hover:bg-gradient-to-r hover:from-primary-400 hover:to-primary-500 hover:shadow-lg hover:shadow-primary-500/50 hover:scale-[1.02]"
              >
                <span class="mr-2.5 relative z-10">{{ t('player') }}</span>
                <font-awesome-icon :icon="faUser" class="h-4 w-4 relative z-10" />
                <div
                  class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
                />
              </div>
            </div>
          </div>
        </div>

        <Footer />
      </div>

      <LoginRegister :is-visible="showAuthModal" @close="showAuthModal = false" />
      <joinByCode :is-visible="showJoinByCode" @close="showJoinByCode = false" />
    </div>
      <Teleport v-if="showVideo" to="body">
      <div 
        class="fixed inset-0 w-screen h-screen z-50 flex items-center justify-center bg-black/95 backdrop-blur-sm p-4"
      >
        <video 
          :autoplay="showVideo" 
          class="w-full h-full rounded-lg disco-shadow"
          @click.stop
        >
          <source src="/easteregss/gandalf.mp4" type="video/mp4" />
        </video>
      </div>
    </Teleport>
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
import AnimatedScene from '@/components/animations/AnimatedScene.vue'
import { useCountdown} from '@vueuse/core'

const { t } = useI18n()
const authStore = useAuthStore()

const showAuthModal = ref<boolean>(false)
const showJoinByCode = ref<boolean>(false)
const showVideo = ref<boolean>(false);

const countdownSeconds = shallowRef<number>(17);


const { start } = useCountdown(countdownSeconds, {
  onComplete() {
    showVideo.value = false;
  },
})

const handleShowVideo = () => {
  showVideo.value = true;
  start(countdownSeconds);
}

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
    box-shadow: 0 0 30px 10px rgba(239, 68, 68, 0.7);
  }
  16% {
    box-shadow: 0 0 30px 10px rgba(249, 115, 22, 0.7);
  }
  33% {
    box-shadow: 0 0 30px 10px rgba(234, 179, 8, 0.7);
  }
  50% {
    box-shadow: 0 0 30px 10px rgba(34, 197, 94, 0.7);
  }
  66% {
    box-shadow: 0 0 30px 10px rgba(59, 130, 246, 0.7);
  }
  83% {
    box-shadow: 0 0 30px 10px rgba(168, 85, 247, 0.7);
  }
  100% {
    box-shadow: 0 0 30px 10px rgba(239, 68, 68, 0.7);
  }
}
</style>
