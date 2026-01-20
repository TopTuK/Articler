<script setup>
import { ref, onMounted } from 'vue'
import { Upload, FileText, Type, Trash2, RefreshCw } from 'lucide-vue-next'
import { useToast } from 'vuestic-ui'
import { useI18n } from 'vue-i18n'
import useDataSourceService from '@/services/dataSourceService'
import AddTextDataSourceModal from './AddTextDataSourceModal.vue'
import AddPdfUrlModal from './AddPdfUrlModal.vue'
import ConfirmDialog from './ConfirmDialog.vue'

const props = defineProps({
  projectId: {
    type: String,
    required: true,
  },
})

const { t } = useI18n()
const { init: showToast } = useToast()
const dataSourceService = useDataSourceService()

const dataSources = ref([])
const showTextInputModal = ref(false)
const showPdfUrlModal = ref(false)
const isUploadingText = ref(false)
const isUploadingPdf = ref(false)
const isLoadingDocuments = ref(false)
const isRemovingDocument = ref(false)
const showConfirmDialog = ref(false)
const documentToDelete = ref(null)

const loadDocuments = async () => {
  if (isLoadingDocuments.value) return

  isLoadingDocuments.value = true
  
  try {
    const documents = await dataSourceService.getDocuments(props.projectId)
    
    // Map documents to the format expected by the component
    dataSources.value = (documents || []).map(doc => ({
      id: doc.id,
      name: doc.title,
      type: doc.documentType === 0 ? 'text' : 'unknown',
    }))
  } catch (error) {
    console.error('DataSources::loadDocuments: Exception: ', error)
    showToast({
      message: t('project_view.toast.failed_to_load_documents'),
      color: 'danger',
      position: 'top-right',
      duration: 3000,
      closeable: true,
    })
  } finally {
    isLoadingDocuments.value = false
  }
}

const handleAddText = async ({ name, text }) => {
  // Validate that title and text are not empty
  if (!text || !text.trim() || !name || !name.trim() || isUploadingText.value) {
    if (!name || !name.trim()) {
      showToast({
        message: t('project_view.toast.title_cannot_be_empty'),
        color: 'warning',
        position: 'top-right',
        duration: 3000,
        closeable: true,
      })
    } else if (!text || !text.trim()) {
      showToast({
        message: t('project_view.toast.text_cannot_be_empty'),
        color: 'warning',
        position: 'top-right',
        duration: 3000,
        closeable: true,
      })
    }
    return
  }

  isUploadingText.value = true
  
  try {
    await dataSourceService.addTextDocument(props.projectId, name, text)
    
    // Reload documents after successful upload
    await loadDocuments()
    
    showTextInputModal.value = false
    showToast({
      message: t('project_view.toast.text_added_to_rag'),
      color: 'success',
      position: 'top-right',
      duration: 3000,
      closeable: true,
    })
  } catch (error) {
    console.error('DataSources::handleAddText: Exception: ', error)
    showToast({
      message: t('project_view.toast.failed_to_upload_text'),
      color: 'danger',
      position: 'top-right',
      duration: 3000,
      closeable: true,
    })
  } finally {
    isUploadingText.value = false
  }
}

const handleAddPdfUrl = async ({ name, url }) => {
  // Validate that title and url are not empty
  if (!url || !url.trim() || !name || !name.trim() || isUploadingPdf.value) {
    if (!name || !name.trim()) {
      showToast({
        message: t('project_view.toast.title_cannot_be_empty'),
        color: 'warning',
        position: 'top-right',
        duration: 3000,
        closeable: true,
      })
    } else if (!url || !url.trim()) {
      showToast({
        message: t('project_view.toast.invalid_url'),
        color: 'warning',
        position: 'top-right',
        duration: 3000,
        closeable: true,
      })
    }
    return
  }

  isUploadingPdf.value = true
  
  try {
    await dataSourceService.addPdfDocument(props.projectId, name, url)
    
    // Reload documents after successful upload
    await loadDocuments()
    
    showPdfUrlModal.value = false
    showToast({
      message: t('project_view.toast.pdf_url_added'),
      color: 'success',
      position: 'top-right',
      duration: 3000,
      closeable: true,
    })
  } catch (error) {
    console.error('DataSources::handleAddPdfUrl: Exception: ', error)
    showToast({
      message: t('project_view.toast.failed_to_upload_pdf'),
      color: 'danger',
      position: 'top-right',
      duration: 3000,
      closeable: true,
    })
  } finally {
    isUploadingPdf.value = false
  }
}

const handleRemoveSource = (source) => {
  documentToDelete.value = source
  showConfirmDialog.value = true
}

const confirmDelete = async () => {
  if (!documentToDelete.value || isRemovingDocument.value) return

  isRemovingDocument.value = true

  try {
    await dataSourceService.removeDocument(props.projectId, documentToDelete.value.id)
    
    // Reload documents after successful removal
    await loadDocuments()
    
    showToast({
      message: t('project_view.toast.source_removed'),
      color: 'success',
      position: 'top-right',
      duration: 3000,
      closeable: true,
    })
  } catch (error) {
    console.error('DataSources::handleRemoveSource: Exception: ', error)
    showToast({
      message: t('project_view.toast.failed_to_remove_document'),
      color: 'danger',
      position: 'top-right',
      duration: 3000,
      closeable: true,
    })
  } finally {
    isRemovingDocument.value = false
    documentToDelete.value = null
  }
}

const getSourceIcon = (type) => {
  switch (type) {
    case 'pdf':
      return FileText
    default:
      return Type
  }
}

onMounted(() => {
  loadDocuments()
})
</script>

<template>
  <div class="rounded-lg border border-gray-800 bg-gray-900/50 backdrop-blur-sm">
    <div class="border-b border-gray-800 p-4">
      <div class="flex items-center justify-between">
        <h2 class="text-lg font-semibold text-white flex items-center gap-2">
          <Upload :size="20" class="text-purple-400" />
          {{ t('project_view.sections.data_sources') }}
        </h2>
        <button
          type="button"
          @click="loadDocuments"
          :disabled="isLoadingDocuments"
          class="inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:text-purple-400 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          :title="t('project_view.actions.refresh')"
        >
          <RefreshCw 
            :size="18" 
            :class="{ 'animate-spin': isLoadingDocuments }"
          />
        </button>
      </div>
    </div>
    <div class="p-4 space-y-4">
      <!-- Action Buttons -->
      <div class="flex flex-wrap gap-2">
        <button
          type="button"
          @click="showTextInputModal = true"
          class="inline-flex items-center justify-center gap-2 rounded-md border border-gray-700 bg-gray-800/50 px-3 py-2 text-sm font-medium text-white hover:bg-gray-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500"
        >
          <Type :size="16" />
          {{ t('project_view.actions.add_text') }}
        </button>
        <button
          type="button"
          @click="showPdfUrlModal = true"
          class="inline-flex items-center justify-center gap-2 rounded-md border border-gray-700 bg-gray-800/50 px-3 py-2 text-sm font-medium text-white hover:bg-gray-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500"
        >
          <FileText :size="16" />
          {{ t('project_view.actions.add_pdf_url') }}
        </button>
      </div>

      <!-- Text Input Modal -->
      <AddTextDataSourceModal
        v-model="showTextInputModal"
        :is-uploading="isUploadingText"
        @submit="handleAddText"
      />

      <!-- PDF URL Modal -->
      <AddPdfUrlModal
        v-model="showPdfUrlModal"
        :is-uploading="isUploadingPdf"
        @submit="handleAddPdfUrl"
      />

      <!-- Confirm Delete Dialog -->
      <ConfirmDialog
        v-model="showConfirmDialog"
        :title="t('project_view.confirm.delete_document_title')"
        :message="t('project_view.confirm.delete_document_message')"
        :confirm-text="t('project_view.confirm.delete')"
        :cancel-text="t('project_view.confirm.cancel')"
        confirm-button-color="danger"
        @confirm="confirmDelete"
      />

      <!-- Sources List -->
      <div class="space-y-2">
        <p
          v-if="dataSources.length === 0"
          class="text-sm text-gray-400 text-center py-4"
        >
          {{ t('project_view.empty.no_data_sources') }}
        </p>
        <div
          v-for="source in dataSources"
          :key="source.id"
          class="flex items-center justify-between p-3 rounded-lg bg-gray-800/20 border border-gray-700/50 hover:border-gray-700 transition-colors"
        >
          <div class="flex items-center gap-3">
            <component
              :is="getSourceIcon(source.type)"
              :size="16"
              :class="{
                'text-red-400': source.type === 'pdf',
                'text-green-400': source.type === 'text',
              }"
            />
            <div>
              <p class="text-sm font-medium text-white">{{ source.name }}</p>
              <p v-if="source.url" class="text-xs text-gray-400 truncate max-w-xs">{{ source.url }}</p>
            </div>
          </div>
          <button
            type="button"
            @click="handleRemoveSource(source)"
            :disabled="isRemovingDocument"
            class="inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:text-red-400 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <Trash2 :size="16" />
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

