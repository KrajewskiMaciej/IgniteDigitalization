<template>
  <div class="flex items-center justify-center mb-6">
    <h2 class="text-2xl font-nasalization">{{ t('accountSettings') }}</h2>
  </div>

  <div class="flex justify-center mb-6">
    <font-awesome-icon :icon="faCircleUser" class="text-6xl" />
  </div>

  <hr class="border-lgray-accent mb-4" />

  <form @submit.prevent="handleChangeAccountSettings">
    <div class="mb-4">
      <label for="username" class="font-bold ml-2 mb-2">{{ t('username') }}</label>
      <input
        id="username"
        v-model="usernameEdit"
        type="text"
        class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 text-white w-full"
      />
    </div>

    <hr class="border-lgray-accent mb-4" />

    <div class="mb-4">
      <label for="email" class="font-bold ml-2 mb-2">{{ t('email') }}</label>
      <input
        id="email"
        v-model="emailEdit"
        type="email"
        class="bg-tertiary border-2 border-lgray-accent rounded-md px-3 py-2 text-white w-full"
      />
    </div>

    <hr class="border-lgray-accent mb-4" />

    <button
      type="submit"
      class="relative w-full py-4 rounded-lg font-medium transition-all duration-300 overflow-hidden group text-white mb-5 bg-gradient-to-r from-primary-600 to-primary-700 hover:from-primary-500 hover:to-primary-600 shadow-lg shadow-primary-500/30 hover:shadow-primary-500/50"
    >
      <span class="relative z-10">{{ t('saveChanges') }}</span>
      <div
        class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-400/20 to-primary-400/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
      ></div>
    </button>
  </form>
</template>

<script setup lang="ts">
import { faCircleUser } from '@fortawesome/free-solid-svg-icons'
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const props = defineProps({
  username: {
    type: String,
    default: '',
  },
  email: {
    type: String,
    default: '',
  },
})

const usernameEdit = ref(props.username)
const emailEdit = ref(props.email)

watch(
  () => props.username,
  (val) => (usernameEdit.value = val),
)
watch(
  () => props.email,
  (val) => (emailEdit.value = val),
)

const handleChangeAccountSettings = async () => {
  // Tu będzie logika do zmiany ustawień konta
}
</script>
