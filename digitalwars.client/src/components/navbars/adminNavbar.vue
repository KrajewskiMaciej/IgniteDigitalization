<template>
  <nav
    class="w-full bg-secondary py-3 px-6 flex flex-row justify-between items-center border-b-2 border-lgray-accent"
  >
    <div>
      <RouterLink to="/">
        <img :src="logo" class="h-12" alt="ITM logo" />
      </RouterLink>
    </div>
    <div class="flex justify-center items-center mr-5">
      <div @click="toggleDropdown" class="cursor-pointer">
        <font-awesome-icon
          :icon="faCircleUser"
          class="h-8 text-white hover:text-accent transition-all duration-300"
        />
      </div>
      <div
        class="text-white ml-3 hidden sm:block cursor-pointer hover:text-accent"
        @click="toggleDropdown"
      >
        {{ username }}
      </div>
    </div>

    <div
      v-show="isDropdownOpen"
      class="flex flex-col items-center absolute top-16 right-16 w-48 bg-secondary rounded-md shadow-lg py-1 z-50 border-solid border-2 border-lgray-accent"
      ref="dropdownMenu"
    >
      <button
        @click="isVisible = true"
        class="flex items-center w-full px-4 py-2 text-sm text-white hover:text-gray-500"
      >
        <span>Ustawienia konta</span>
        <font-awesome-icon :icon="faGear" class="ml-2" />
      </button>
      <hr class="border-lgray-accent w-[90%]" />
      <button
        @click="logout"
        class="flex items-center w-full px-4 py-2 text-sm text-white hover:text-red-600"
      >
        <span>Wyloguj się</span>
        <font-awesome-icon :icon="faRightFromBracket" class="ml-2" />
      </button>
    </div>
  </nav>

  <adminSettings
    :isVisible="isVisible"
    @close="isVisible = false"
    :username="username"
    :email="email"
  />
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import logo from '@/assets/logos/ITM_poziom_biale.png'
// BŁĄD TS2307: Jeśli TS nie widzi modułu, upewnij się, że masz go zainstalowanego:
// npm install @fortawesome/free-solid-svg-icons
import { faCircleUser, faRightFromBracket, faGear } from '@fortawesome/free-solid-svg-icons'
// BŁĄD TS2307: Upewnij się, że masz zainstalowany `vue-router`:
// npm install vue-router
import { useRouter, RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import adminSettings from '../admin/adminSettings.vue'
import apiServices from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'

// --- DEFINICJE INTERFEJSÓW ---
interface User {
  name: string
  email: string
}

// --- ZMIENNE REAKTYWNE I KONFIGURACJA ---
const authStore = useAuthStore()
const router = useRouter()

const isDropdownOpen = ref(false)
// POPRAWKA: Jawne typowanie refa dla elementu DOM
const dropdownMenu = ref<HTMLDivElement | null>(null)
const username = ref('')
const email = ref('')
const isVisible = ref(false)

// --- FUNKCJE ---
const toggleDropdown = () => {
  isDropdownOpen.value = !isDropdownOpen.value
}

const handleClickOutside = (event: MouseEvent) => {
  // POPRAWKA: Zmieniono rzutowanie typu z `Node` na `Element`
  const target = event.target as Element

  if (
    dropdownMenu.value &&
    !dropdownMenu.value.contains(target) &&
    !target.closest('.cursor-pointer')
  ) {
    isDropdownOpen.value = false
  }
}

const logout = async () => {
  try {
    // POPRAWKA: Metoda POST wymaga drugiego argumentu (ciała żądania), nawet jeśli jest puste
    await apiServices.post(apiConfig.auth.logout, {})
    authStore.setAuthenticated(false)
    router.push('/')
    isDropdownOpen.value = false
  } catch (error) {
    console.error('Błąd podczas wylogowywania:', error)
  }
}

const fetchUser = async () => {
  try {
    // ULEPSZENIE: Dodano typ generyczny do wywołania API dla lepszego bezpieczeństwa
    const res = await apiServices.get<User>(apiConfig.auth.me)
    username.value = res.data.name
    email.value = res.data.email
  } catch (error) {
    console.error('Nie udało się pobrać danych użytkownika', error)
  }
}

// --- CYKL ŻYCIA KOMPONENTU ---
onMounted(() => {
  document.addEventListener('click', handleClickOutside)
  fetchUser()
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>
