<template>
  <div
    v-if="props.isVisible"
    class="fixed inset-0 flex items-center justify-center z-50 md:p-4 lg:p-6 xl:p-8"
  >
    <div
      class="absolute inset-0 bg-black/70 backdrop-blur-sm transition-opacity duration-300 md:block hidden"
      @click="closeModal"
    ></div>

    <div
      class="bg-gradient-to-br from-surface-850 to-surface-900 text-surface-0 relative z-50 transition-all duration-300 w-full h-full md:h-auto md:max-w-md lg:max-w-lg md:max-h-[95vh] overflow-y-auto custom-scrollbar md:rounded-2xl md:border md:border-primary-500/30 md:shadow-2xl md:shadow-primary-500/20"
      :class="
        props.isVisible
          ? 'md:scale-100 md:translate-y-0 opacity-100'
          : 'md:scale-95 md:translate-y-4 opacity-0'
      "
    >
      <button
        @click="closeModal"
        class="absolute top-4 right-4 z-20 w-10 h-10 flex items-center justify-center rounded-full text-surface-0 hover:text-primary-400 hover:bg-surface-700 backdrop-blur-sm transition-all duration-200 hover:shadow-lg hover:shadow-primary-500/30"
      >
        <font-awesome-icon :icon="faXmark" class="text-xl" />
      </button>

      <div class="px-4 sm:px-6 md:px-8 pt-16 pb-6 sm:pb-8">
        <div v-if="!isScanning" class="animate-fade w-full">
          <h1
            class="text-3xl sm:text-4xl font-bold mb-6 text-center text-surface-0 font-nasalization bg-clip-text"
          >
            {{ t('joinTheGame') }}
          </h1>
          <div
            class="h-[2px] bg-gradient-to-r from-transparent via-primary-500 to-transparent mb-6"
          ></div>

          <form @submit.prevent="validateAndJoin">
            <div class="mb-6">
              <input
                type="text"
                v-model="code"
                :placeholder="t('enterGameCodePlaceholder')"
                class="w-full px-4 py-4 bg-surface-800 border border-primary-500/30 rounded-xl text-surface-0 placeholder:text-surface-400 focus:outline-none focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20 transition-all duration-200"
                required
              />
            </div>
            <button
              type="submit"
              :disabled="isProcessing"
              class="relative w-full py-4 rounded-xl font-semibold transition-all duration-300 overflow-hidden group disabled:opacity-50 disabled:cursor-not-allowed bg-gradient-to-r from-primary-600 to-primary-700 hover:from-primary-500 hover:to-primary-600 shadow-lg shadow-primary-500/30 hover:shadow-primary-500/50"
            >
              <span class="relative z-10">{{
                isProcessing ? t('checking') : t('joinTheGame')
              }}</span>
              <div
                class="absolute inset-0 bg-gradient-to-r from-primary-400/0 via-primary-400/20 to-primary-400/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
              ></div>
            </button>
          </form>

          <div class="flex items-center gap-3 my-6">
            <div
              class="flex-1 h-px bg-gradient-to-r from-transparent via-primary-500/30 to-primary-500/30"
            ></div>
            <span class="text-surface-400 text-sm">lub</span>
            <div class="flex-1 h-px bg-gradient-to-r from-primary-500/30 to-transparent"></div>
          </div>

          <button
            type="button"
            @click="startScanning"
            class="relative w-full py-4 rounded-xl font-semibold transition-all duration-300 overflow-hidden group bg-surface-800 hover:bg-gradient-to-r hover:from-primary-600/20 hover:to-primary-700/20 border border-primary-500/30 hover:border-primary-500/50 hover:shadow-md hover:shadow-primary-500/20"
          >
            <span class="relative z-10 flex items-center justify-center gap-2">
              <font-awesome-icon :icon="faQrcode" />
              {{ t('scanQRCode') }}
            </span>
            <div
              class="absolute inset-0 bg-gradient-to-r from-primary-500/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
            ></div>
          </button>
        </div>

        <div v-else class="w-full animate-fade">
          <h2
            class="text-2xl sm:text-3xl font-bold mb-6 text-center bg-gradient-to-r from-primary-400 to-primary-600 bg-clip-text text-transparent"
          >
            {{ t('scanQRCode') }}
          </h2>

          <div
            class="mb-6 relative rounded-xl overflow-hidden border-2 border-primary-500/30 shadow-2xl shadow-primary-500/20"
          >
            <qrcode-stream
              :formats="['qr_code']"
              @detect="onDetect"
              @error="onScannerError"
              @camera-on="onCameraOn"
            >
              <div v-if="scanError" class="text-center p-6 bg-red-500/20 backdrop-blur-sm">
                <p class="text-red-400">{{ scanError }}</p>
              </div>
              <div
                v-if="!scanError && !cameraReady"
                class="text-center p-6 bg-surface-800/80 backdrop-blur-sm"
              >
                <p class="text-surface-300">{{ t('initializingCamera') }}</p>
              </div>
              <div class="absolute inset-0 pointer-events-none">
                <div
                  class="absolute top-4 left-4 w-8 h-8 border-t-2 border-l-2 border-primary-400"
                ></div>
                <div
                  class="absolute top-4 right-4 w-8 h-8 border-t-2 border-r-2 border-primary-400"
                ></div>
                <div
                  class="absolute bottom-4 left-4 w-8 h-8 border-b-2 border-l-2 border-primary-400"
                ></div>
                <div
                  class="absolute bottom-4 right-4 w-8 h-8 border-b-2 border-r-2 border-primary-400"
                ></div>
              </div>
            </qrcode-stream>
          </div>

          <button
            @click="isScanning = false"
            class="relative w-full py-4 rounded-xl font-semibold transition-all duration-300 overflow-hidden group bg-surface-800 hover:bg-gradient-to-r hover:from-primary-600/20 hover:to-primary-700/20 border border-primary-500/30 hover:border-primary-500/50"
          >
            <span class="relative z-10">{{ t('cancel') }}</span>
            <div
              class="absolute inset-0 bg-gradient-to-r from-primary-500/0 via-primary-500/10 to-primary-500/0 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700"
            ></div>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { faXmark, faQrcode } from '@fortawesome/free-solid-svg-icons'
import { QrcodeStream } from 'vue-qrcode-reader'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import apiServices from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

interface DetectedBarcode {
  rawValue: string
}

const router = useRouter()
const toast = useToast()

const isScanning = ref(false)
const scanError = ref('')
const cameraReady = ref(false)
const code = ref('')
const isProcessing = ref(false)
const lastScannedCode = ref('')

const props = defineProps({
  isVisible: { type: Boolean, default: false },
})
const emit = defineEmits(['close'])

const closeModal = () => {
  emit('close')
  isScanning.value = false
  scanError.value = ''
}

const validateAndJoin = async () => {
  if (!code.value || isProcessing.value) return
  isProcessing.value = true
  try {
    // Krok 1: Wywołaj nowy endpoint walidacyjny
    await apiServices.get(apiConfig.player.validateToken(code.value))

    // Krok 2: Jeśli walidacja się powiodła, przekieruj do widoku gracza
    router.push(`/player/${code.value}`)
    closeModal()
  } catch (error: any) {
    // Krok 3: Jeśli walidacja się nie powiodła, wyświetl błąd
    const errorMessage = error.response?.data?.message || 'Wystąpił nieznany błąd.'
    toast.error(errorMessage)
  } finally {
    isProcessing.value = false
  }
}

const startScanning = () => {
  isScanning.value = true
  scanError.value = ''
  lastScannedCode.value = ''
  cameraReady.value = false
}

const onDetect = (detectedCodes: DetectedBarcode[]) => {
  if (isProcessing.value || detectedCodes.length === 0) return
  const decodedText = detectedCodes[0].rawValue

  if (decodedText && decodedText !== lastScannedCode.value) {
    isProcessing.value = true
    lastScannedCode.value = decodedText
    code.value = decodedText
    validateAndJoin() // Użyj tej samej logiki walidacji
  }
}

const onCameraOn = () => {
  cameraReady.value = true
}

const onScannerError = (error: Error) => {
  let errorMessage = 'Błąd kamery'
  if (error.name === 'NotAllowedError') {
    errorMessage = 'Brak dostępu do kamery. Sprawdź uprawnienia przeglądarki.'
  } else if (error.name === 'NotFoundError') {
    errorMessage = 'Nie znaleziono kamery w urządzeniu.'
  } else if (error.name === 'NotReadableError') {
    errorMessage = 'Kamera jest obecnie używana przez inną aplikację.'
  }
  scanError.value = errorMessage
  console.error('Błąd skanera QR:', error)
}
</script>
