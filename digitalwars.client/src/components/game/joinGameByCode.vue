<template>
  <div v-if="props.isVisible" class="fixed inset-0 flex items-center justify-center z-50">
    <div class="absolute inset-0 bg-black/70" @click="closeModal"></div>

    <div
      class="flex flex-col justify-start items-center bg-primary text-white rounded-lg w-96 relative z-10 border-2 border-accent p-6 sm:p-8 md:p-10 lg:p-12 animate-jump-in"
    >
      <button
        @click="closeModal"
        class="absolute top-2 right-2 w-8 h-8 flex items-center justify-center"
      >
        <font-awesome-icon
          :icon="faXmark"
          class="h-5 text-white hover:text-accent transition-all duration-100"
        />
      </button>

      <!-- Widok formularza -->
      <div v-if="!isScanning" class="animate-fade w-full">
        <h2 class="text-lg sm:text-xl md:text-2xl font-nasalization mb-2 sm:mb-3 text-center">
          Dołącz do gry
        </h2>
        <div class="w-full h-0.5 mb-1 sm:mb-2 md:mb-3 lg:mb-4 bg-accent"></div>
        <form @submit.prevent="validateAndJoin">
          <div class="mb-4">
            <input
              type="text"
              v-model="code"
              placeholder="Wprowadź kod gry..."
              class="w-full px-3 py-3 bg-tertiary border border-lgray-accent rounded-md text-white focus:outline-none focus:border-accent"
              required
            />
          </div>
          <button
            type="submit"
            :disabled="isProcessing"
            class="bg-tertiary hover:bg-accent/80 text-white w-full py-4 rounded-lg font-medium transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ isProcessing ? 'Sprawdzanie...' : 'Dołącz do gry' }}
          </button>
        </form>
        <div class="flex items-center gap-2 my-6">
          <div class="flex-1 h-px bg-lgray-accent"></div>
          <span class="text-gray-500 px-2">lub</span>
          <div class="flex-1 h-px bg-lgray-accent"></div>
        </div>
        <button
          type="button"
          @click="startScanning"
          class="bg-tertiary hover:bg-accent text-white w-full py-4 rounded-lg font-medium transition-all duration-300"
        >
          <font-awesome-icon :icon="faQrcode" class="mr-2" />
          Zeskanuj kod QR
        </button>
      </div>

      <!-- Widok skanera -->
      <div v-else class="w-full">
        <h2 class="text-lg sm:text-xl md:text-2xl font-nasalization mb-4 text-center">
          Skanuj kod QR
        </h2>
        <div class="mb-4 relative">
          <qrcode-stream
            :formats="['qr_code']"
            @detect="onDetect"
            @error="onScannerError"
            @camera-on="onCameraOn"
            class="rounded-lg overflow-hidden"
          >
            <div v-if="scanError" class="text-center p-4 bg-red-500/20">
              <p class="text-red-400">{{ scanError }}</p>
            </div>
            <div v-if="!scanError && !cameraReady" class="text-center p-4">
              <p>Inicjalizacja kamery...</p>
            </div>
            <div class="absolute inset-0 pointer-events-none">
              <!-- ... stylizacja ramki ... -->
            </div>
          </qrcode-stream>
        </div>
        <button
          @click="isScanning = false"
          class="bg-tertiary hover:bg-accent/80 text-white w-full py-3 rounded-lg font-medium transition-all duration-300"
        >
          Anuluj
        </button>
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

const processScanResult = (decodedText: string) => {
  // Ta funkcja nie jest już potrzebna, bo onDetect bezpośrednio wywołuje validateAndJoin
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
