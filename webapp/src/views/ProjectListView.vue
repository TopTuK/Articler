<script setup>
import { ref, computed, onBeforeMount } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { Search, Plus, FolderOpen, Sparkles, FileText } from 'lucide-vue-next'
import ProjectCard from '@/components/logged/project/ProjectCard.vue'
import useProjectService from '@/services/projectService'
import { formatRelativeTime } from '@/utils.js'

const { t } = useI18n()

const router = useRouter()
const projectService = useProjectService()

const isProjectsLoading = ref(true)
const projects = ref([])
const error = ref(null)

// Map backend State enum (1 = Unpublished, 2 = Published) to status string
const mapStateToStatus = (state) => {
  return state === 2 ? 'published' : 'unpublished'
}

// Map backend project to frontend format
const mapProject = (backendProject) => {
  return {
    id: backendProject.id,
    title: backendProject.title,
    description: backendProject.description,
    icon: 'blog', // Default icon for all projects
    status: mapStateToStatus(backendProject.state),
    createdDate: formatRelativeTime(backendProject.createdDate),
  }
}

const loadProjects = async () => {
  isProjectsLoading.value = true
  error.value = null
  try {
    const backendProjects = await projectService.getUserProjects()
    // Handle empty array or null/undefined
    if (Array.isArray(backendProjects)) {
      projects.value = backendProjects.map(mapProject)
      console.log('ProjectListView::loadProjects: projects count: ', projects.value.length)
    } else {
      projects.value = []
      console.log('ProjectListView::loadProjects: received non-array response')
    }
  }
  catch (err) {
    console.error('ProjectListView::loadProjects: Exception: ', err)
    error.value = err
    projects.value = []
  }
  finally {
    isProjectsLoading.value = false
  }
}

const navigateToNewProject = () => {
  router.push({ name: 'NewProject' })
}

const searchQuery = ref('')

const filteredProjects = computed(() => {
  if (!projects.value || projects.value.length === 0) {
    return []
  }
  return projects.value.filter(
    (project) =>
      project.title.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      project.description.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
})

const projectStats = computed(() => {
  if (!projects.value || projects.value.length === 0) {
    return {
      total: 0,
      unpublished: 0,
      published: 0,
    }
  }
  return {
    total: projects.value.length,
    unpublished: projects.value.filter((p) => p.status === 'unpublished').length,
    published: projects.value.filter((p) => p.status === 'published').length,
  }
})

onBeforeMount(async () => {
  await loadProjects()
})
</script>

<template>
  <div class="min-h-screen bg-black pt-20">
    <div class="container mx-auto px-6 py-8 space-y-8">
      <!-- Page Title -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div class="space-y-2">
          <h1 class="text-3xl font-display font-bold text-white">{{ t('project_list.title') }}</h1>
          <p class="text-gray-400">{{ t('project_list.subtitle') }}</p>
        </div>
        <button
          @click="navigateToNewProject"
          class="inline-flex items-center justify-center gap-2 rounded-md bg-purple-600 px-4 py-2 text-sm font-medium text-white hover:bg-purple-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:pointer-events-none disabled:opacity-50 shadow-lg shadow-purple-500/20"
        >
          <Plus :size="16" />
          {{ t('project_list.new_project_button') }}
        </button>
      </div>

      <!-- Search and Stats -->
      <div class="flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
        <div class="relative w-full sm:w-80">
          <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            :placeholder="t('project_list.search_placeholder')"
            v-model="searchQuery"
            class="flex h-10 w-full rounded-md border border-gray-700 bg-gray-900/50 backdrop-blur-sm px-3 py-2 pl-10 text-sm text-white placeholder:text-gray-500 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 focus-visible:border-purple-500 disabled:cursor-not-allowed disabled:opacity-50"
          />
        </div>

        <!-- Project Stats -->
        <div class="flex items-center gap-6 text-sm">
          <div class="flex items-center gap-2">
            <span class="text-gray-400">{{ t('project_list.stats.total') }}</span>
            <span class="font-semibold text-white">{{ projectStats.total }}</span>
          </div>
          <div class="flex items-center gap-2">
            <span class="text-gray-400">{{ t('project_list.stats.unpublished') }}</span>
            <span class="font-semibold text-purple-400">{{ projectStats.unpublished }}</span>
          </div>
          <div class="flex items-center gap-2">
            <span class="text-gray-400">{{ t('project_list.stats.published') }}</span>
            <span class="font-semibold text-green-400">{{ projectStats.published }}</span>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="isProjectsLoading" class="text-center py-12">
        <p class="text-gray-400">{{ t('project_list.loading') }}</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="text-center py-12">
        <p class="text-red-400 mb-4">{{ t('project_list.error_message') }}</p>
        <button
          @click="loadProjects"
          class="inline-flex items-center justify-center gap-2 rounded-md bg-purple-600 px-4 py-2 text-sm font-medium text-white hover:bg-purple-700 transition-colors"
        >
          {{ t('project_list.retry_button') }}
        </button>
      </div>

      <!-- Projects Grid -->
      <div v-else-if="filteredProjects.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <ProjectCard
          v-for="project in filteredProjects"
          :key="project.id"
          :project="project"
        />
      </div>

      <!-- Empty State -->
      <div v-else class="relative">
        <!-- Background glow effect -->
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-96 h-96 rounded-full bg-gradient-to-r from-purple-500/10 via-blue-500/10 to-pink-500/10 blur-3xl animate-pulse"></div>
        
        <div class="relative z-10 flex flex-col items-center justify-center py-20 px-6">
          <!-- Icon container with gradient background -->
          <div class="relative mb-8">
            <div class="absolute inset-0 bg-gradient-to-r from-purple-500/20 to-blue-500/20 rounded-full blur-2xl animate-pulse"></div>
            <div class="relative w-32 h-32 rounded-full bg-gradient-to-br from-purple-500/20 to-blue-500/20 border border-purple-400/30 backdrop-blur-sm flex items-center justify-center shadow-2xl shadow-purple-500/20">
              <component 
                :is="searchQuery ? Search : FolderOpen" 
                :size="64" 
                class="text-purple-300 animate-bounce-slow"
              />
            </div>
            <!-- Floating decorative icons -->
            <div class="absolute -top-4 -right-4 w-8 h-8 rounded-lg bg-gradient-to-br from-pink-500/20 to-purple-500/20 border border-pink-400/30 backdrop-blur-sm flex items-center justify-center animate-float">
              <Sparkles :size="16" class="text-pink-300" />
            </div>
            <div class="absolute -bottom-4 -left-4 w-8 h-8 rounded-lg bg-gradient-to-br from-blue-500/20 to-cyan-500/20 border border-blue-400/30 backdrop-blur-sm flex items-center justify-center animate-float delay-200">
              <FileText :size="16" class="text-blue-300" />
            </div>
          </div>

          <!-- Text content -->
          <div class="text-center max-w-md space-y-4 mb-8">
            <h2 class="text-2xl font-display font-bold text-white">
              <span v-if="searchQuery" class="bg-gradient-to-r from-purple-400 to-blue-400 bg-clip-text text-transparent">
                {{ t('project_list.empty_search_title') }}
              </span>
              <span v-else class="bg-gradient-to-r from-purple-400 to-blue-400 bg-clip-text text-transparent">
                {{ t('project_list.empty_state_title') }}
              </span>
            </h2>
            <p class="text-gray-400 text-lg leading-relaxed">
              {{ searchQuery ? t('project_list.empty_search') : t('project_list.empty_state') }}
            </p>
          </div>

          <!-- Call to action button -->
          <button
            v-if="!searchQuery"
            @click="navigateToNewProject"
            class="group relative inline-flex items-center justify-center gap-2 rounded-lg bg-gradient-to-r from-purple-600 to-blue-600 px-6 py-3 text-sm font-medium text-white hover:from-purple-700 hover:to-blue-700 transition-all duration-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 shadow-lg shadow-purple-500/30 hover:shadow-xl hover:shadow-purple-500/40 hover:scale-105"
          >
            <Plus :size="18" class="group-hover:rotate-90 transition-transform duration-300" />
            {{ t('project_list.new_project_button') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>


