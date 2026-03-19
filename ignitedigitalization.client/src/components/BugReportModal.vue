<template>
  <Dialog
    v-model:visible="isVisible"
    :header="t('bugReportTitle')"
    modal
    :draggable="false"
    :style="{ width: '500px', maxWidth: '95vw' }"
    :closable="!isSubmitting"
    class="bug-report-dialog"
  >
    <div v-if="!submitSuccess" class="flex flex-col gap-4">
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium text-surface-300">
          {{ t('bugReportEmail') }} <span class="text-red-400">*</span>
        </label>
        <InputText
          v-model="form.email"
          :placeholder="t('bugReportEmailPlaceholder')"
          :invalid="!!errors.email"
          :disabled="isSubmitting"
          type="email"
          class="w-full"
        />
        <small v-if="errors.email" class="text-red-400">{{ errors.email }}</small>
      </div>

      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium text-surface-300">
          {{ t('bugReportDescription') }} <span class="text-red-400">*</span>
        </label>
        <Textarea
          v-model="form.description"
          :placeholder="t('bugReportDescriptionPlaceholder')"
          :invalid="!!errors.description"
          :disabled="isSubmitting"
          rows="6"
          class="w-full resize-none"
        />
        <small v-if="errors.description" class="text-red-400">{{ errors.description }}</small>
      </div>

      <div v-if="submitError" class="rounded-md bg-red-900/30 px-4 py-3 text-sm text-red-300">
        {{ submitError }}
      </div>
    </div>

    <div v-else class="flex flex-col items-center gap-4 py-4 text-center">
      <div class="flex h-16 w-16 items-center justify-center rounded-full bg-primary-500/20">
        <font-awesome-icon :icon="faCircleCheck" class="h-8 w-8 text-primary-400" />
      </div>
      <p class="text-surface-200">{{ t('bugReportSuccess') }}</p>
    </div>

    <template #footer>
      <div v-if="!submitSuccess" class="flex justify-end gap-2">
        <Button
          :label="t('cancel')"
          outlined
          :disabled="isSubmitting"
          @click="close"
        />
        <Button
          :label="isSubmitting ? t('bugReportSubmitting') : t('bugReportSubmit')"
          :loading="isSubmitting"
          :disabled="isSubmitting"
          @click="submit"
        />
      </div>
      <div v-else class="flex justify-end">
        <Button :label="t('close')" @click="close" />
      </div>
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'
import { faCircleCheck } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import apiServices from '@/services/apiServices'
import apiConfig from '@/services/apiConfig'
import { useSettingsStore } from '@/stores/settingsStore'

const props = defineProps<{
  visible: boolean
}>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
}>()

const { t } = useI18n()
const settingsStore = useSettingsStore()

const isVisible = ref(props.visible)
const isSubmitting = ref(false)
const submitSuccess = ref(false)
const submitError = ref<string | null>(null)

const form = ref({ email: '', description: '' })
const errors = ref({ email: '', description: '' })

watch(
  () => props.visible,
  (val) => {
    isVisible.value = val
    if (val) resetState()
  },
)

watch(isVisible, (val) => {
  emit('update:visible', val)
})

function resetState() {
  form.value = { email: '', description: '' }
  errors.value = { email: '', description: '' }
  submitSuccess.value = false
  submitError.value = null
  isSubmitting.value = false
}

function validate(): boolean {
  errors.value = { email: '', description: '' }
  let valid = true

  if (!form.value.email.trim()) {
    errors.value.email = t('bugReportEmailRequired')
    valid = false
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.value.email)) {
    errors.value.email = t('bugReportEmailInvalid')
    valid = false
  }

  if (!form.value.description.trim()) {
    errors.value.description = t('bugReportDescriptionRequired')
    valid = false
  } else if (form.value.description.trim().length < 10) {
    errors.value.description = t('bugReportDescriptionTooShort')
    valid = false
  }

  return valid
}

async function submit() {
  if (!validate()) return

  isSubmitting.value = true
  submitError.value = null

  try {
    const lang = settingsStore.language ?? 'pl'
    await apiServices.post(apiConfig.bugReport.submit(lang), {
      reporterEmail: form.value.email.trim(),
      description: form.value.description.trim(),
    })
    submitSuccess.value = true
  } catch {
    submitError.value = t('bugReportError')
  } finally {
    isSubmitting.value = false
  }
}

function close() {
  isVisible.value = false
}
</script>
