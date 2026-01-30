<script setup>
import { FileText, RefreshCw, ExternalLink } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'

const props = defineProps({
  modelValue: {
    type: String,
    default: '',
  },
  projectId: {
    type: String,
    default: '',
  },
  isLoadingText: {
    type: Boolean,
    default: false,
  },
  textError: {
    type: Object,
    default: null,
  },
  isSaving: {
    type: Boolean,
    default: false,
  },
})

const emit = defineEmits(['update:modelValue', 'save', 'refresh'])

const { t } = useI18n()

const handleSavePost = () => {
  if (!props.modelValue.trim()) return
  emit('save')
}

const handleRefresh = () => {
  emit('refresh')
}
</script>

<template>
  <div>
    <div class="rounded-lg border border-gray-800 bg-gray-900/50 backdrop-blur-sm h-full">
      <div class="border-b border-gray-800 p-4">
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-semibold text-white flex items-center gap-2">
            <FileText :size="20" class="text-purple-400" />
            {{ t('project_view.sections.post_content') }}
          </h2>
          <div class="flex items-center gap-2">
            <router-link
              v-if="projectId"
              :to="{ name: 'ProjectEditor', params: { id: projectId } }"
              class="inline-flex items-center justify-center gap-2 rounded-md border border-gray-700 bg-gray-800/50 px-3 py-2 text-sm font-medium text-white hover:bg-gray-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:pointer-events-none disabled:opacity-50"
              :title="t('project_editor.actions.open_editor_page')"
            >
              <ExternalLink :size="16" />
            </router-link>
            <button
              type="button"
              @click="handleRefresh"
              :disabled="isLoadingText"
              class="inline-flex items-center justify-center gap-2 rounded-md border border-gray-700 bg-gray-800/50 px-3 py-2 text-sm font-medium text-white hover:bg-gray-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:pointer-events-none disabled:opacity-50"
              :title="t('project_view.actions.refresh_text')"
            >
              <RefreshCw :size="16" :class="{ 'animate-spin': isLoadingText }" />
            </button>
          </div>
        </div>
      </div>
      <div class="p-4">
        <!-- Loading State -->
        <div v-if="isLoadingText" class="flex items-center justify-center min-h-[500px]">
          <div class="text-center space-y-4">
            <div class="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-purple-500 border-r-transparent"></div>
            <p class="text-sm text-gray-400">{{ t('project_view.loading_text') }}</p>
          </div>
        </div>
        
        <!-- Error State (non-blocking - allow editing) -->
        <div v-else-if="textError" class="mb-3 rounded-lg border border-yellow-500/20 bg-yellow-500/10 p-3">
          <p class="text-xs text-yellow-400">
            {{ t('project_view.errors.failed_to_load_text') }}
          </p>
        </div>
        
        <!-- Text Editor -->
        <textarea
          v-if="!isLoadingText"
          :placeholder="t('project_view.placeholders.post_editor')"
          :value="modelValue"
          @input="$emit('update:modelValue', $event.target.value)"
          class="flex min-h-[500px] w-full rounded-md border border-gray-700 bg-gray-900 px-3 py-2 text-sm text-white placeholder:text-gray-500 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 focus-visible:border-purple-500 disabled:cursor-not-allowed disabled:opacity-50 resize-none"
        />
        <div v-if="!isLoadingText" class="flex items-center justify-between mt-2">
          <p class="text-xs text-gray-400">
            {{ t('project_view.post.characters', { count: modelValue.length }) }}
          </p>
          <button
            type="button"
            @click="handleSavePost"
            :disabled="!modelValue.trim() || isSaving"
            class="inline-flex items-center justify-center gap-2 rounded-md bg-purple-600 px-4 py-2 text-sm font-medium text-white hover:bg-purple-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:pointer-events-none disabled:opacity-50"
          >
            <div
              v-if="isSaving"
              class="inline-block h-4 w-4 animate-spin rounded-full border-2 border-solid border-white border-r-transparent"
            ></div>
            {{ t('project_view.actions.save_post') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

