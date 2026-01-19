<script setup>
import { ref, watch } from 'vue'
import { X, FileText, Save } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { MdEditor } from 'md-editor-v3'
import 'md-editor-v3/lib/style.css'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false,
  },
  postText: {
    type: String,
    default: '',
  },
  isSaving: {
    type: Boolean,
    default: false,
  },
})

const emit = defineEmits(['update:modelValue', 'save', 'close'])

const { t } = useI18n()

const editorText = ref('')
const editorId = 'post-editor-modal'

// Sync editorText with postText when modal opens
watch(() => props.modelValue, (isOpen) => {
  if (isOpen) {
    editorText.value = props.postText || ''
  }
})

// Watch for external changes to postText
watch(() => props.postText, (newText) => {
  if (props.modelValue) {
    editorText.value = newText || ''
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

const handleSave = () => {
  if (props.isSaving) return
  emit('save', editorText.value)
}

const handleKeydown = (e) => {
  if (e.key === 'Escape') {
    handleClose()
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
            class="w-full max-w-6xl max-h-[90vh] rounded-lg border border-gray-800 bg-gray-900/95 backdrop-blur-sm shadow-2xl flex flex-col overflow-hidden"
            @click.stop
          >
            <!-- Header -->
            <div class="flex items-center justify-between p-6 border-b border-gray-800">
              <h2 class="text-xl font-semibold text-white flex items-center gap-2">
                <FileText :size="20" class="text-purple-400" />
                {{ t('project_view.sections.post_content') }}
              </h2>
              <button
                type="button"
                @click="handleClose"
                :disabled="isSaving"
                class="inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:text-white hover:bg-gray-800 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <X :size="20" />
              </button>
            </div>

            <!-- Content -->
            <div class="flex-1 overflow-hidden p-6 flex flex-col">
              <div class="flex-1 min-h-[500px]">
                <MdEditor
                  :id="editorId"
                  v-model="editorText"
                  theme="dark"
                  previewTheme="github"
                  :toolbars="[
                    'bold',
                    'underline',
                    'italic',
                    '-',
                    'title',
                    'strikeThrough',
                    'sub',
                    'sup',
                    'quote',
                    'unorderedList',
                    'orderedList',
                    'task',
                    '-',
                    'codeRow',
                    'code',
                    'link',
                    'image',
                    'table',
                    '-',
                    'revoke',
                    'next',
                    'save',
                    '=',
                    'pageFullscreen',
                    'fullscreen',
                    'preview',
                    'catalog'
                  ]"
                  language="en-US"
                />
              </div>
            </div>

            <!-- Footer -->
            <div class="flex items-center justify-between gap-3 p-6 border-t border-gray-800 bg-gray-900/50">
              <p class="text-xs text-gray-400">
                {{ t('project_view.post.characters', { count: editorText.length }) }}
              </p>
              <div class="flex items-center gap-3">
                <button
                  type="button"
                  @click="handleClose"
                  :disabled="isSaving"
                  class="inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium text-gray-400 hover:text-white hover:bg-gray-800 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:pointer-events-none disabled:opacity-50"
                >
                  {{ t('project_view.actions.cancel') }}
                </button>
                <button
                  type="button"
                  @click="handleSave"
                  :disabled="!editorText.trim() || isSaving"
                  class="inline-flex items-center justify-center gap-2 rounded-md bg-purple-600 px-4 py-2 text-sm font-medium text-white hover:bg-purple-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:pointer-events-none disabled:opacity-50"
                >
                  <div
                    v-if="isSaving"
                    class="inline-block h-4 w-4 animate-spin rounded-full border-2 border-solid border-white border-r-transparent"
                  ></div>
                  <Save v-else :size="16" />
                  {{ isSaving ? t('project_view.actions.saving') : t('project_view.actions.save_post') }}
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
/* Ensure the editor takes full height */
:deep(.md-editor) {
  height: 100%;
  display: flex;
  flex-direction: column;
}

:deep(.md-editor-content) {
  flex: 1;
  overflow: hidden;
}
</style>
