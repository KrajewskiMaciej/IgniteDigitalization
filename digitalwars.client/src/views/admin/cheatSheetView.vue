<template>
  <div class="flex flex-col p-4 md:p-6 lg:p-8 gap-6">
    <div class="text-center mb-2">
      <h1 class="font-nasalization text-3xl md:text-4xl lg:text-5xl text-white mb-2">
        {{ t('supportMaterials') }}
      </h1>
      <p class="text-surface-400 text-sm md:text-base">{{ t('supportMaterialsDescription') }}</p>
    </div>

    <div class="max-w-6xl mx-auto w-full space-y-6">
      <div class="border border-surface-700 rounded-lg p-5 bg-secondary">
        <div
          class="flex justify-between items-center mb-5"
          :class="showFilesSection ? 'border-b border-surface-700 pb-4' : ''"
        >
          <div class="flex gap-3 items-center mt-1">
            <div class="bg-red-500/20 p-3 rounded-lg">
              <font-awesome-icon :icon="faFilePdf" class="h-6 text-red-500" />
            </div>
            <h2 class="text-xl md:text-2xl font-bold text-white">{{ t('pdfDocuments') }}</h2>
          </div>
          <div>
            <button>
              <font-awesome-icon
                :icon="showFilesSection ? faChevronUp : faChevronDown"
                class="h-5 text-surface-400 hover:text-primary-400 transition-colors duration-200"
                @click="showFilesSection = !showFilesSection"
              />
            </button>
          </div>
        </div>

        <div v-show="showFilesSection" class="space-y-4">
          <div>
            <label for="pdf-select" class="block mb-2.5 text-sm font-semibold text-gray-300">
              {{ t('selectDocument') }}
            </label>
            <Dropdown
              id="pdf-select"
              v-model="selectedPDF"
              :options="availablePDFs"
              optionLabel="name"
              optionValue="path"
              :placeholder="t('selectPdfPlaceholder')"
              class="w-full"
            >
              <template #value="slotProps">
                <div v-if="slotProps.value" class="flex items-center gap-2">
                  <font-awesome-icon :icon="faFilePdf" class="h-4 text-red-500" />
                  <span>{{ availablePDFs.find((p) => p.path === slotProps.value)?.name }}</span>
                </div>
                <span v-else>{{ slotProps.placeholder }}</span>
              </template>
              <template #option="slotProps">
                <div class="flex items-center gap-2">
                  <font-awesome-icon :icon="faFilePdf" class="h-4 text-red-500" />
                  <span>{{ slotProps.option.name }}</span>
                </div>
              </template>
            </Dropdown>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <Button @click="openPDF" :disabled="!selectedPDF" :label="t('preview')" class="w-full">
              <font-awesome-icon :icon="faEye" class="h-4 mr-2" />
              <span>{{ t('preview') }}</span>
            </Button>

            <Button
              @click="openPDFInNewTab"
              :disabled="!selectedPDF"
              severity="secondary"
              outlined
              class="w-full"
            >
              <font-awesome-icon :icon="faUpRightFromSquare" class="h-4 mr-2" />
              <span>{{ t('newTab') }}</span>
            </Button>
          </div>

          <div v-if="pdfWindows.length > 0" class="pt-4 border-t border-surface-700">
            <div class="flex items-center justify-between mb-3">
              <span class="text-sm font-semibold text-primary-400">
                {{ t('openDocuments') }} ({{ pdfWindows.length }}):
              </span>
              <Button
                @click="closeAllPDFs"
                severity="danger"
                outlined
                :label="t('closeAll')"
                text
                size="small"
                class="h-auto py-1"
              >
                <template #icon>
                  <font-awesome-icon :icon="faXmark" class="h-4" />
                </template>
              </Button>
            </div>
            <div class="space-y-2.5">
              <div
                v-for="(window, index) in pdfWindows"
                :key="index"
                class="flex items-center justify-between bg-secondary/70 p-3.5 rounded-lg border-2 border-surface-600 hover:border-primary-400 hover:bg-secondary transition-all duration-200"
              >
                <div class="flex items-center gap-3 flex-1 min-w-0">
                  <div class="bg-red-500/20 p-1.5 rounded">
                    <font-awesome-icon :icon="faFilePdf" class="h-4 text-red-500 flex-shrink-0" />
                  </div>
                  <span class="text-sm font-medium text-white truncate">
                    {{ availablePDFs.find((p) => p.path === window.path)?.name }}
                  </span>
                </div>
                <Button
                  @click="closePDF(index)"
                  icon="pi pi-times"
                  severity="danger"
                  text
                  rounded
                  size="small"
                  v-tooltip.top="t('close')"
                >
                  <template #icon>
                    <font-awesome-icon :icon="faMinus" class="h-4" />
                  </template>
                </Button>
              </div>
            </div>
          </div>

          <div
            v-else
            class="text-center py-12 border-2 border-dashed border-surface-700 rounded-xl bg-secondary/30"
          >
            <div
              class="bg-secondary/50 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-3"
            >
              <font-awesome-icon :icon="faFileCircleQuestion" class="h-10 text-surface-600" />
            </div>
            <p class="text-surface-400 text-sm font-medium">{{ t('noOpenDocuments') }}</p>
            <p class="text-gray-500 text-xs mt-1">{{ t('selectPdfInstruction') }}</p>
          </div>
        </div>
      </div>

      <div v-if="pdfWindows.length > 0">
        <div
          class="border-2 border-surface-700 rounded-xl overflow-hidden bg-secondary shadow-2xl"
        >
          <TabView v-model:activeIndex="activePDFIndex" class="pdf-tabs">
            <TabPanel v-for="(window, index) in pdfWindows" :key="index">
              <template #header>
                <div class="flex items-center gap-3 px-2">
                  <span class="font-medium">{{
                    availablePDFs.find((p) => p.path === window.path)?.name
                  }}</span>
                </div>
              </template>

              <div class="bg-white rounded-lg overflow-hidden">
                <iframe
                  :src="window.path"
                  class="w-full h-[70vh]"
                  title="PDF Document Viewer"
                ></iframe>
              </div>
            </TabPanel>
          </TabView>
        </div>
      </div>


    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import {
  faFilePdf,
  faEye,
  faUpRightFromSquare,
  faXmark,
  faFileCircleQuestion,
  faDiagramProject,
  faMinus,
  faChevronDown,
  faChevronUp,
  faMagnifyingGlassMinus,
  faMagnifyingGlassPlus,
  faDownload,
} from '@fortawesome/free-solid-svg-icons'
import Dropdown from 'primevue/dropdown'
import Button from 'primevue/button'
import TabView from 'primevue/tabview'
import TabPanel from 'primevue/tabpanel'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

interface PdfInfo {
  name: string
  path: string
}

interface PdfWindow {
  path: string
}

const showFilesSection = ref<boolean>(true)

const availablePDFs: PdfInfo[] = [
  {
    name: 'Instrukcja główna',
    path: new URL('@/assets/documents/DIGITAL-WARS_instrukcja_final.pdf', import.meta.url).href,
  },
  {
    name: 'Dodatek 1',
    path: new URL('@/assets/documents/dodatek1.pdf', import.meta.url).href,
  },
  {
    name: 'Dodatek 2',
    path: new URL('@/assets/documents/dodatek2.pdf', import.meta.url).href,
  },
]

const selectedPDF = ref<string>(availablePDFs[0].path)
const pdfWindows = ref<PdfWindow[]>([])
const activePDFIndex = ref<number>(0)
const isZoomed = ref<boolean>(false)

function toggleImageSize(): void {
  isZoomed.value = !isZoomed.value
}

function openPDF(): void {
  if (selectedPDF.value && !pdfWindows.value.some((w) => w.path === selectedPDF.value)) {
    pdfWindows.value.push({ path: selectedPDF.value })
    activePDFIndex.value = pdfWindows.value.length - 1
  }
}

function closePDF(index: number): void {
  pdfWindows.value.splice(index, 1)
  if (activePDFIndex.value >= pdfWindows.value.length) {
    activePDFIndex.value = Math.max(0, pdfWindows.value.length - 1)
  }
}

function closeAllPDFs(): void {
  pdfWindows.value = []
  activePDFIndex.value = 0
}

function focusPDF(index: number): void {
  activePDFIndex.value = index
  const element = document.querySelector('.pdf-tabs')
  if (element) {
    element.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }
}

function openPDFInNewTab(): void {
  if (selectedPDF.value) {
    window.open(selectedPDF.value, '_blank')
  }
}
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 0.625rem;
  height: 0.625rem;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: rgb(var(--surface-800));
  border-radius: 0.5rem;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgb(var(--surface-600));
  border-radius: 0.5rem;
  transition: background 0.2s;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: rgb(var(--primary-400));
}
</style>
