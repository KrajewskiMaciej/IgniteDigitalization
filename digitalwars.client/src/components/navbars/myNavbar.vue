<template>
  <nav
    class="w-full bg-secondary py-2 px-4 flex flex-row md:justify-between items-center shadow-[0_2px_4px_theme(colors.primary.400/0.5)]"
  >
    <div class="flex items-center w-full" :class="isMdOrLarger ? 'gap-2' : 'justify-between'">
      <img :src="logo" class="h-16" alt="ITM logo" />
      <img src="/DigitalWars-logo-circle.png" class="h-16" alt="Digital Wars logo" />
    </div>

    <!--Odnośniki do social medi-->
    <div v-if="isMdOrLarger" class="flex flex-row justify-around items-center gap-3 mr-5">
      <a href="https://www.linkedin.com/company/itm-ltd/about/" target="_blank"
        ><font-awesome-icon
          :icon="faLinkedin"
          class="text-3xl text-white hover:text-accent transition-all duration-300 ease-out"
      /></a>
      <a href="https://www.youtube.com/channel/UCMA8NVly_eqK114A6-fkjTw" target="_blank"
        ><font-awesome-icon
          :icon="faYoutube"
          class="text-3xl text-white hover:text-accent transition-all duration-300 ease-out"
      /></a>
      <a href="https://www.facebook.com/itm.olsztyn" target="_blank"
        ><font-awesome-icon
          :icon="faFacebook"
          class="text-3xl text-white hover:text-accent transition-all duration-300 ease-out"
      /></a>
    </div>
  </nav>
  <div v-if="!isMdOrLarger" class="flex justify-end mt-2 items-center gap-3 mr-5">
    <a href="https://www.linkedin.com/company/itm-ltd/about/" target="_blank"
      ><font-awesome-icon
        :icon="faLinkedin"
        class="text-4xl text-white hover:text-accent transition-all duration-300 ease-out"
    /></a>
    <a href="https://www.youtube.com/channel/UCMA8NVly_eqK114A6-fkjTw" target="_blank"
      ><font-awesome-icon
        :icon="faYoutube"
        class="text-4xl text-white hover:text-accent transition-all duration-300 ease-out"
    /></a>
    <a href="https://www.facebook.com/itm.olsztyn" target="_blank"
      ><font-awesome-icon
        :icon="faFacebook"
        class="text-4xl text-white hover:text-accent transition-all duration-300 ease-out"
    /></a>
  </div>
</template>

<script setup lang="ts">
import { faFacebook, faLinkedin, faYoutube } from '@fortawesome/free-brands-svg-icons'
import logo from '@/assets/logos/ITM_poziom_biale.png'
import { useBreakpoints } from '@vueuse/core'

const breakpoints = useBreakpoints({
  sm: 640,
  md: 768,
  lg: 1024,
  xl: 1280,
})

const isMdOrLarger = breakpoints.greaterOrEqual('md')
</script>
