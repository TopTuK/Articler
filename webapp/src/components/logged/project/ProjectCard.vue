<script setup>
import { FileText, BookOpen, Code, Newspaper, PenTool, Lightbulb } from 'lucide-vue-next'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'

const router = useRouter()
const { t } = useI18n()

const props = defineProps({
  project: {
    type: Object,
    required: true,
  },
})

const navigateToProject = (projectId, event) => {
  router.push({ name: 'Project', params: { id: projectId } })
}

const projectIcons = {
  article: FileText,
  blog: BookOpen,
  technical: Code,
  news: Newspaper,
  creative: PenTool,
  idea: Lightbulb,
}

const projectStatus = {
  draft: { labelKey: 'draft', variant: 'secondary' },
  'in-progress': { labelKey: 'in_progress', variant: 'default' },
  completed: { labelKey: 'completed', variant: 'outline' },
  published: { labelKey: 'published', variant: 'default' },
  unpublished: { labelKey: 'unpublished', variant: 'secondary' },
}

const getStatusLabel = (status) => {
  const statusConfig = projectStatus[status]
  if (!statusConfig) return status
  return t(`project_card.status.${statusConfig.labelKey}`)
}

const getStatusVariant = (status) => {
  const statusConfig = projectStatus[status]
  if (!statusConfig) return 'bg-gray-800 text-gray-300 border-gray-700'
  
  const variantMap = {
    default: 'bg-purple-600 text-white border-purple-500',
    secondary: 'bg-gray-800 text-gray-300 border-gray-700',
    outline: 'border border-gray-700 bg-transparent text-gray-300',
    destructive: 'bg-red-600 text-white border-red-500',
  }
  
  return variantMap[statusConfig.variant] || variantMap.secondary
}
</script>

<template>
  <div
    @click="navigateToProject(project.id, $event)"
    :class="[
      'rounded-lg border border-gray-800 bg-gray-900/50 backdrop-blur-sm p-6 text-white shadow-lg transition-all duration-300',
      'hover:border-purple-500/50 hover:shadow-xl hover:shadow-purple-500/10 cursor-pointer group'
    ]"
  >
    <div class="flex flex-col space-y-1.5 pb-3">
      <div class="flex items-start justify-between gap-4">
        <div class="w-10 h-10 rounded-lg bg-purple-500/20 flex items-center justify-center group-hover:bg-purple-500/30 transition-colors">
          <component :is="projectIcons[project.icon]" class="w-5 h-5 text-purple-400" />
        </div>
        <div class="flex items-center gap-2">
          <span
            :class="[
              'inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-purple-500 shrink-0',
              getStatusVariant(project.status)
            ]"
          >
            {{ getStatusLabel(project.status) }}
          </span>
        </div>
      </div>
      <div class="mt-3">
        <h3 class="text-lg font-semibold leading-none tracking-tight text-white group-hover:text-purple-400 transition-colors">
          {{ project.title }}
        </h3>
      </div>
    </div>
    <div class="space-y-4 pt-0">
      <div>
        <p class="text-sm text-gray-300 line-clamp-2">
          {{ project.description }}
        </p>
      </div>
      <p class="text-xs text-gray-500">
        {{ t('project_card.created') }} {{ project.createdDate }}
      </p>
    </div>
  </div>
</template>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>

