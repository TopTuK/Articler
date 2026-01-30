<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowLeft, FileText, Save } from 'lucide-vue-next'
import { useToast } from 'vuestic-ui'
import { storeToRefs } from 'pinia'
import useProjectService from '@/services/projectService'
import { useProjectStore } from '@/stores/projectStore'
import { useI18n } from 'vue-i18n'
import { MdEditor } from 'md-editor-v3'
import 'md-editor-v3/lib/style.css'

const route = useRoute()
const router = useRouter()
const { init: showToast } = useToast()
const projectService = useProjectService()
const projectStore = useProjectStore()
const { projectText } = storeToRefs(projectStore)
const { t } = useI18n()

const projectId = computed(() => route.params.id)
const project = ref({
  id: '',
  title: '',
  description: '',
})

const isLoading = ref(true)
const isLoadingText = ref(false)
const textError = ref(null)
const isSavingText = ref(false)
const editorId = 'project-editor-page'

const postContent = computed({
  get: () => projectText.value || '',
  set: (value) => {
    projectText.value = value
  },
})

const loadProject = async () => {
  if (!projectId.value) return

  isLoading.value = true
  try {
    const backendProject = await projectService.getProject(projectId.value)
    project.value = {
      id: backendProject.id,
      title: backendProject.title || t('project_view.defaults.unknown_project'),
      description: backendProject.description || '',
    }
    await loadProjectText()
  } catch (err) {
    console.error('ProjectEditorView::loadProject: Exception:', err)
    showToast({
      message: t('project_view.toast.failed_to_load_project'),
      color: 'danger',
      position: 'top-right',
      duration: 3000,
    })
    router.push('/projects')
  } finally {
    isLoading.value = false
  }
}

const loadProjectText = async () => {
  if (!projectId.value) return

  isLoadingText.value = true
  textError.value = null
  try {
    await projectStore.getProjectText(projectId.value)
  } catch (err) {
    console.error('ProjectEditorView::loadProjectText: Exception:', err)
    textError.value = err
  } finally {
    isLoadingText.value = false
  }
}

const handleSavePost = async () => {
  if (!projectText.value?.trim() || !projectId.value) return

  isSavingText.value = true
  try {
    await projectStore.saveProjectText(projectId.value)
    showToast({
      message: t('project_view.toast.post_saved'),
      color: 'success',
      position: 'top-right',
      duration: 3000,
    })
  } catch (err) {
    console.error('ProjectEditorView::handleSavePost: Exception:', err)
    showToast({
      message: t('project_view.toast.failed_to_save_post'),
      color: 'danger',
      position: 'top-right',
      duration: 3000,
    })
  } finally {
    isSavingText.value = false
  }
}

const goBack = () => {
  router.push({ name: 'Project', params: { id: projectId.value } })
}

onMounted(() => {
  loadProject()
})
</script>

<template>
  <div class="editor-page flex flex-col mt-16 h-full min-h-[calc(100vh-6rem)] bg-black">
    <!-- Loading State -->
    <div v-if="isLoading" class="flex-1 flex items-center justify-center">
      <div class="text-center space-y-4">
        <div class="inline-block h-10 w-10 animate-spin rounded-full border-4 border-solid border-purple-500 border-r-transparent"></div>
        <p class="text-gray-300">{{ t('project_view.loading') }}</p>
      </div>
    </div>

    <template v-else>
      <!-- Header -->
      <div class="flex-shrink-0 flex items-center justify-between gap-4 px-4 py-3 border-b border-gray-800 bg-gray-900/80 backdrop-blur-sm">
        <div class="flex items-center gap-4 min-w-0">
          <button
            type="button"
            @click="goBack"
            class="inline-flex items-center gap-2 rounded-lg text-gray-400 hover:text-white transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 shrink-0"
            :aria-label="t('project_editor.actions.back_to_project')"
          >
            <ArrowLeft :size="20" />
            <span class="hidden sm:inline">{{ t('project_editor.actions.back_to_project') }}</span>
          </button>
          <div class="min-w-0">
            <h1 class="text-lg font-semibold text-white truncate flex items-center gap-2">
              <FileText :size="18" class="text-purple-400 shrink-0" />
              {{ project.title || t('project_view.defaults.untitled_project') }}
            </h1>
            <p v-if="project.description" class="text-xs text-gray-500 truncate">{{ project.description }}</p>
          </div>
        </div>
        <div class="flex items-center gap-3 shrink-0">
          <p class="text-xs text-gray-400 hidden sm:block">
            {{ t('project_view.post.characters', { count: postContent.length }) }}
          </p>
          <button
            type="button"
            @click="handleSavePost"
            :disabled="!postContent.trim() || isSavingText"
            class="inline-flex items-center justify-center gap-2 rounded-md bg-purple-600 px-4 py-2 text-sm font-medium text-white hover:bg-purple-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <div
              v-if="isSavingText"
              class="inline-block h-4 w-4 animate-spin rounded-full border-2 border-solid border-white border-r-transparent"
            ></div>
            <Save v-else :size="16" />
            {{ isSavingText ? t('project_view.actions.saving') : t('project_view.actions.save_post') }}
          </button>
        </div>
      </div>

      <!-- Error State (non-blocking) -->
      <div v-if="textError" class="flex-shrink-0 mx-4 mt-2 rounded-lg border border-yellow-500/20 bg-yellow-500/10 p-3">
        <p class="text-xs text-yellow-400">
          {{ t('project_view.errors.failed_to_load_text') }}
        </p>
      </div>

      <!-- Editor -->
      <div class="flex-1 min-h-0 overflow-hidden p-4">
        <MdEditor
          :id="editorId"
          v-model="postContent"
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
          class="h-full"
        />
      </div>
    </template>
  </div>
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
