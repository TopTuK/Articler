<script setup>
import { ref, watch } from 'vue'
import { X, FileText } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false,
  },
  isUploading: {
    type: Boolean,
    default: false,
  },
})

const emit = defineEmits(['update:modelValue', 'submit', 'close'])

const { t } = useI18n()

const pdfUrlInput = ref('')
const pdfNameInput = ref('')

// Reset form when modal closes
watch(() => props.modelValue, (isOpen) => {
  if (!isOpen) {
    pdfUrlInput.value = ''
    pdfNameInput.value = ''
  }
})

const handleClose = () => {
  emit('update:modelValue', false)
  emit('close')
}

const handleBackdropClick = (e) => {
  if (e.target === e.currentTarget) {
    handleClose()
  }
}

const handleSubmit = () => {
  if (!pdfUrlInput.value.trim() || !pdfNameInput.value.trim() || props.isUploading) return
  emit('submit', {
    name: pdfNameInput.value.trim(),
    url: pdfUrlInput.value.trim(),
  })
}

const handleKeydown = (e) => {
  if (e.key === 'Escape') {
    handleClose()
  }
}

const isValidUrl = (url) => {
  try {
    new URL(url)
    return true
  } catch {
    return false
  }
}
</script>

<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition-opacity duration-300"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-opacity duration-300"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="modelValue"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/70 backdrop-blur-sm"
        @click="handleBackdropClick"
        @keydown="handleKeydown"
      >
        <Transition
          enter-active-class="transition-all duration-300"
          enter-from-class="opacity-0 scale-95 translate-y-4"
          enter-to-class="opacity-100 scale-100 translate-y-0"
          leave-active-class="transition-all duration-300"
          leave-from-class="opacity-100 scale-100 translate-y-0"
          leave-to-class="opacity-0 scale-95 translate-y-4"
        >
          <div
            v-if="modelValue"
            class="w-full max-w-2xl rounded-lg border border-gray-800 bg-gray-900/95 backdrop-blur-sm shadow-2xl flex flex-col overflow-hidden"
            @click.stop
          >
            <!-- Header -->
            <div class="flex items-center justify-between p-6 border-b border-gray-800">
              <h2 class="text-xl font-semibold text-white flex items-center gap-2">
                <FileText :size="20" class="text-red-400" />
                {{ t('project_view.actions.add_pdf_url') }}
              </h2>
              <button
                type="button"
                @click="handleClose"
                class="inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:text-white hover:bg-gray-800 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500"
                :disabled="isUploading"
              >
                <X :size="20" />
              </button>
            </div>

            <!-- Content -->
            <div class="flex-1 overflow-y-auto p-6 space-y-4">
              <div>
                <label
                  for="pdf-name-input"
                  class="block text-sm font-medium text-gray-300 mb-2"
                >
                  {{ t('project_view.placeholders.pdf_name') }}
                </label>
                <input
                  id="pdf-name-input"
                  type="text"
                  :placeholder="t('project_view.placeholders.pdf_name')"
                  v-model="pdfNameInput"
                  required
                  :disabled="isUploading"
                  class="flex w-full rounded-md border border-gray-700 bg-gray-800 px-4 py-2 text-sm text-white placeholder:text-gray-500 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 focus-visible:border-purple-500 disabled:cursor-not-allowed disabled:opacity-50"
                />
              </div>
              <div>
                <label
                  for="pdf-url-input"
                  class="block text-sm font-medium text-gray-300 mb-2"
                >
                  {{ t('project_view.placeholders.pdf_url') }}
                </label>
                <input
                  id="pdf-url-input"
                  type="url"
                  :placeholder="t('project_view.placeholders.pdf_url')"
                  v-model="pdfUrlInput"
                  required
                  :disabled="isUploading"
                  class="flex w-full rounded-md border border-gray-700 bg-gray-800 px-4 py-2 text-sm text-white placeholder:text-gray-500 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 focus-visible:border-purple-500 disabled:cursor-not-allowed disabled:opacity-50"
                />
                <p v-if="pdfUrlInput && !isValidUrl(pdfUrlInput)" class="mt-1 text-xs text-red-400">
                  {{ t('project_view.validation.invalid_url') }}
                </p>
              </div>
            </div>

            <!-- Footer -->
            <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-800 bg-gray-900/50">
              <button
                type="button"
                @click="handleClose"
                :disabled="isUploading"
                class="inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium text-gray-400 hover:text-white hover:bg-gray-800 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:pointer-events-none disabled:opacity-50"
              >
                {{ t('project_view.actions.cancel') }}
              </button>
              <button
                type="button"
                @click="handleSubmit"
                :disabled="!pdfUrlInput.trim() || !pdfNameInput.trim() || !isValidUrl(pdfUrlInput) || isUploading"
                class="inline-flex items-center justify-center rounded-md bg-purple-600 px-4 py-2 text-sm font-medium text-white hover:bg-purple-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:pointer-events-none disabled:opacity-50"
              >
                {{ isUploading ? t('project_view.actions.uploading') : t('project_view.actions.add_to_rag') }}
              </button>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>
