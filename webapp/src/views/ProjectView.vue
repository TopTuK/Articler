<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowLeft, AlertTriangle, Trash2 } from 'lucide-vue-next'
import { useToast } from 'vuestic-ui'
import useProjectService from '@/services/projectService'
import { useI18n } from 'vue-i18n'
import PostEditor from '@/components/common/PostEditor.vue'
import ChatView from '@/components/logged/project/ChatView.vue'
import DataSources from '@/components/logged/project/DataSources.vue'
import ConfirmDialog from '@/components/logged/project/ConfirmDialog.vue'

const route = useRoute()
const router = useRouter()
const { init: showToast } = useToast()
const projectService = useProjectService()
const { t } = useI18n()

const project = ref({
  id: route.params.id || '',
  title: '',
  description: '',
  status: 'in_progress',
})

const isLoading = ref(true)
const error = ref(null)

const isLoadingText = ref(false)
const textError = ref(null)
const isSavingText = ref(false)

const showDeleteDialog = ref(false)
const isDeleting = ref(false)

// Map backend State enum (1 = Unpublished, 2 = Published) to status string
const mapStateToStatus = (state) => {
  return state === 2 ? 'published' : 'in_progress'
}

const loadProject = async () => {
  if (!project.value.id) {
    error.value = new Error(t('project_view.errors.project_id_missing'))
    isLoading.value = false
    return
  }

  isLoading.value = true
  error.value = null

  try {
    const backendProject = await projectService.getProject(project.value.id)
    project.value = {
      id: backendProject.id,
      title: backendProject.title || t('project_view.defaults.unknown_project'),
      description: backendProject.description || '',
      status: mapStateToStatus(backendProject.state),
    }
    console.log('ProjectView::loadProject: Project loaded. ProjectId: ', project.value.id)
    
    // Load project text after project is loaded successfully
    loadProjectText()
  }
  catch (err) {
    console.error('ProjectView::loadProject: Exception: ', err)
    error.value = err
    showToast({
      message: t('project_view.toast.failed_to_load_project'),
      color: 'danger',
      position: 'top-right',
      duration: 3000,
    })
  }
  finally {
    isLoading.value = false
  }
}

const loadProjectText = async () => {
  if (!project.value.id) {
    return
  }

  isLoadingText.value = true
  textError.value = null

  try {
    const textData = await projectService.getProjectText(project.value.id)
    // API returns an object with a 'text' property
    postContent.value = textData.text || ''
    console.log('ProjectView::loadProjectText: Project text loaded successfully')
  }
  catch (err) {
    console.error('ProjectView::loadProjectText: Exception: ', err)
    textError.value = err
    // Don't show toast for text loading errors - allow user to continue editing
    // The error state can be used to show a message in the UI if needed
  }
  finally {
    isLoadingText.value = false
  }
}

onMounted(() => {
  loadProject()
})

const postContent = ref('')

const handleSavePost = async () => {
  if (!postContent.value.trim() || !project.value.id) return
  
  isSavingText.value = true
  
  try {
    await projectService.updateProjectText(project.value.id, postContent.value)
    console.log('ProjectView::handleSavePost: Project text saved successfully')
    showToast({
      message: t('project_view.toast.post_saved'),
      color: 'success',
      position: 'top-right',
      duration: 3000,
    })
  }
  catch (err) {
    console.error('ProjectView::handleSavePost: Exception: ', err)
    showToast({
      message: t('project_view.toast.failed_to_save_post'),
      color: 'danger',
      position: 'top-right',
      duration: 3000,
    })
  }
  finally {
    isSavingText.value = false
  }
}

const handleDeleteProject = () => {
  showDeleteDialog.value = true
}

const confirmDeleteProject = async () => {
  if (!project.value.id || isDeleting.value) return

  isDeleting.value = true

  try {
    await projectService.deleteProject(project.value.id)
    console.log('ProjectView::confirmDeleteProject: Project deleted successfully')
    showToast({
      message: t('project_view.toast.project_deleted'),
      color: 'success',
      position: 'top-right',
      duration: 3000,
    })
    // Redirect to projects list after successful deletion
    router.push('/projects')
  }
  catch (err) {
    console.error('ProjectView::confirmDeleteProject: Exception: ', err)
    showToast({
      message: t('project_view.toast.failed_to_delete_project'),
      color: 'danger',
      position: 'top-right',
      duration: 3000,
    })
    isDeleting.value = false
  }
}


</script>

<template>
  <div class="min-h-screen bg-black">
    <main class="container mx-auto px-4 py-8 pt-24">
      <!-- Loading State (show only spinner, no page chrome) -->
      <div v-if="isLoading" class="min-h-[60vh] flex items-center justify-center">
        <div class="text-center space-y-4">
          <div class="inline-block h-10 w-10 animate-spin rounded-full border-4 border-solid border-purple-500 border-r-transparent"></div>
          <p class="text-gray-300">{{ t('project_view.loading') }}</p>
        </div>
      </div>

      <!-- Error State (show only error) -->
      <div v-else-if="error" class="min-h-[60vh] flex items-center justify-center">
        <div class="w-full max-w-lg">
          <div class="rounded-2xl border border-gray-800 bg-gradient-to-b from-gray-900/70 to-gray-950/60 backdrop-blur-sm shadow-[0_20px_60px_-20px_rgba(0,0,0,0.8)] overflow-hidden">
            <div class="p-6 sm:p-8">
              <div class="mx-auto mb-5 flex h-12 w-12 items-center justify-center rounded-xl border border-red-500/20 bg-red-500/10">
                <AlertTriangle :size="22" class="text-red-400" />
              </div>

              <div class="text-center space-y-2">
                <h1 class="text-xl sm:text-2xl font-semibold text-white">
                  {{ t('project_view.errors.failed_to_load_project_title') }}
                </h1>
                <p class="text-sm sm:text-base text-gray-400">
                  {{ error.message || t('project_view.errors.unknown_error_occurred') }}
                </p>
              </div>

              <div class="mt-6 flex flex-col sm:flex-row items-stretch sm:items-center justify-center gap-3">
            <router-link
              to="/projects"
              class="inline-flex items-center justify-center gap-2 rounded-lg border border-gray-700 bg-gray-800/40 px-4 py-2.5 text-sm font-semibold text-white hover:bg-gray-700/60 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500"
            >
              <ArrowLeft :size="16" class="text-gray-300" />
              {{ t('project_view.actions.back_to_projects') }}
            </router-link>
            <button
              type="button"
              @click="loadProject"
              class="inline-flex items-center justify-center gap-2 rounded-lg bg-purple-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-purple-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500"
            >
              {{ t('project_view.actions.retry') }}
            </button>
              </div>

              <div class="mt-6 rounded-xl border border-gray-800 bg-black/20 px-4 py-3">
                <p class="text-xs text-gray-500 text-center">
                  {{ t('project_view.defaults.unknown_project') }}
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Main Content (only after project is loaded successfully) -->
      <div v-else>
        <!-- Project Header (page-local; LoggedLayout already provides the global header) -->
        <div class="mb-8">
          <div class="flex items-center justify-between gap-6">
            <div class="flex items-center gap-4 min-w-0">
              <router-link
                to="/projects"
                class="text-gray-400 hover:text-white transition-colors shrink-0"
                :aria-label="t('project_view.actions.back_to_projects')"
              >
                <ArrowLeft :size="20" />
              </router-link>
              <div class="min-w-0 space-y-1">
                <h1 class="text-xl font-bold text-white truncate">{{ project.title || t('project_view.defaults.untitled_project') }}</h1>
                <p class="text-sm text-gray-400 truncate">{{ project.description || t('project_view.defaults.no_description') }}</p>
              </div>
            </div>
            <div class="flex items-center gap-3 shrink-0">
              <span
                class="inline-flex items-center rounded-full border border-purple-500/50 px-3 py-1 text-xs font-medium text-purple-400 capitalize"
              >
                {{ t(`project_card.status.${project.status}`) }}
              </span>
              <button
                type="button"
                @click="handleDeleteProject"
                :disabled="isDeleting"
                class="inline-flex items-center gap-2 rounded-lg border border-red-500/50 bg-red-500/10 px-3 py-1.5 text-xs font-medium text-red-400 hover:bg-red-500/20 hover:border-red-500/70 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                :aria-label="t('project_view.actions.delete_project')"
              >
                <Trash2 :size="14" />
                {{ t('project_view.actions.delete_project') }}
              </button>
            </div>
          </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <!-- Left Column - Chat & Data Sources -->
        <div class="space-y-6">
          <!-- Chat/Prompts Section -->
          <ChatView :project-id="project.id" />

          <!-- Data Sources Section -->
          <DataSources :project-id="project.id" />
        </div>

        <!-- Right Column - Post Editor -->
        <PostEditor
          v-model="postContent"
          :is-loading-text="isLoadingText"
          :text-error="textError"
          :is-saving="isSavingText"
          @save="handleSavePost"
          @refresh="loadProjectText"
        />
        </div>
      </div>

      <!-- Delete Project Confirmation Dialog -->
      <ConfirmDialog
        v-model="showDeleteDialog"
        :title="t('project_view.confirm.delete_project_title')"
        :message="t('project_view.confirm.delete_project_message')"
        :confirm-text="t('project_view.confirm.delete')"
        :cancel-text="t('project_view.confirm.cancel')"
        confirm-button-color="danger"
        @confirm="confirmDeleteProject"
      />
    </main>
  </div>
</template>

