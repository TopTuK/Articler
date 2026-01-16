<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { ArrowLeft, Save, FileText } from 'lucide-vue-next'
import { useToast } from 'vuestic-ui'
import useProjectService from '@/services/projectService'

const router = useRouter()
const { t } = useI18n()
const { init: showToast } = useToast()
const { createProject } = useProjectService()

const title = ref('')
const description = ref('')
const isSubmitting = ref(false)

const handleSubmit = async (e) => {
  e.preventDefault()
  
  if (!title.value.trim()) {
    showToast({
      message: t('new_project.toast.title_required'),
      color: 'danger',
      position: 'top-right',
      duration: 3000,
    })
    return
  }

  if (isSubmitting.value) {
    return
  }

  isSubmitting.value = true

  try {
    await createProject(title.value.trim(), description.value.trim())
    
    showToast({
      message: t('new_project.toast.project_created'),
      color: 'success',
      position: 'top-right',
      duration: 3000,
    })
    // Note: Redirect is handled by createProject function in the service
  }
  catch (error) {
    console.error('NewProjectView::handleSubmit: Error creating project', error)
    showToast({
      message: error.response?.data || t('new_project.toast.project_creation_failed') || 'Failed to create project',
      color: 'danger',
      position: 'top-right',
      duration: 3000,
    })
  }
  finally {
    isSubmitting.value = false
  }
}

const handleCancel = () => {
  router.push({ name: 'Projects' })
}
</script>

<template>
  <div class="min-h-screen bg-black">
    <!-- Header -->
    <header class="border-b border-gray-800 bg-gray-900/80 backdrop-blur-xl">
      <div class="container mx-auto px-4 py-4">
        <div class="flex items-center justify-between">
          <router-link
            :to="{ name: 'Projects' }"
            class="flex items-center gap-2 text-gray-400 hover:text-white transition-colors"
          >
            <ArrowLeft :size="16" />
            <span>{{ t('new_project.back_to_projects') }}</span>
          </router-link>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="container mx-auto px-4 py-12">
      <div class="max-w-2xl mx-auto">
        <!-- Page Header -->
        <div class="flex items-center gap-4 mb-8">
          <div class="w-14 h-14 rounded-2xl bg-purple-500/20 border border-purple-500/30 flex items-center justify-center">
            <FileText :size="28" class="text-purple-400" />
          </div>
          <div>
            <h1 class="text-3xl font-bold text-white">{{ t('new_project.title') }}</h1>
            <p class="text-gray-400 mt-1">{{ t('new_project.subtitle') }}</p>
          </div>
        </div>

        <!-- Form -->
        <form @submit="handleSubmit" class="space-y-6">
          <div class="p-6 rounded-2xl border border-gray-800 bg-gray-900/50 backdrop-blur-sm space-y-6">
            <!-- Title Field -->
            <div class="space-y-2">
              <label for="title" class="text-white font-medium">
                {{ t('new_project.form.project_title_label') }}
              </label>
              <input
                id="title"
                type="text"
                :placeholder="t('new_project.form.project_title_placeholder')"
                v-model="title"
                class="flex h-12 w-full rounded-md border border-gray-700 bg-gray-900/50 backdrop-blur-sm px-4 text-lg text-white placeholder:text-gray-500 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 focus-visible:border-purple-500 disabled:cursor-not-allowed disabled:opacity-50"
              />
            </div>

            <!-- Description Field -->
            <div class="space-y-2">
              <label for="description" class="text-white font-medium">
                {{ t('new_project.form.description_label') }}
              </label>
              <textarea
                id="description"
                :placeholder="t('new_project.form.description_placeholder')"
                v-model="description"
                rows="5"
                class="flex w-full rounded-md border border-gray-700 bg-gray-900/50 backdrop-blur-sm px-4 py-3 text-sm text-white placeholder:text-gray-500 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 focus-visible:border-purple-500 disabled:cursor-not-allowed disabled:opacity-50 resize-none"
              />
              <p class="text-sm text-gray-400">
                {{ t('new_project.form.description_hint') }}
              </p>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex items-center justify-end gap-3">
            <button
              type="button"
              @click="handleCancel"
              class="inline-flex items-center justify-center rounded-md border border-gray-700 bg-transparent px-4 py-2 text-sm font-medium text-gray-300 hover:bg-gray-800 hover:text-white transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:pointer-events-none disabled:opacity-50"
            >
              {{ t('new_project.actions.cancel') }}
            </button>
            <button
              type="submit"
              :disabled="isSubmitting"
              class="inline-flex items-center justify-center gap-2 rounded-md bg-purple-600 px-4 py-2 text-sm font-medium text-white hover:bg-purple-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:pointer-events-none disabled:opacity-50"
            >
              <Save :size="16" />
              {{ isSubmitting ? t('new_project.actions.creating') || 'Creating...' : t('new_project.actions.create') }}
            </button>
          </div>
        </form>
      </div>
    </main>
  </div>
</template>

