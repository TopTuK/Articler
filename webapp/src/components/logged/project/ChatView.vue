<script setup>
import { ref, onBeforeMount } from 'vue'
import { Send, RefreshCw } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vuestic-ui'
import useChatService from '@/services/chatService'

const props = defineProps({
  projectId: {
    type: String,
    required: true,
  },
})

const { t } = useI18n()
const { init: showToast } = useToast()
const chatService = useChatService()

const chatMessages = ref([])
const chatInput = ref('')
const isSending = ref(false)
const isLoadingHistory = ref(false)

// Helper to map AgentRole enum to string
const mapRoleToString = (role) => {
  // AgentRole enum: 0=User, 1=Assistant, 2=System
  switch (role) {
    case 0:
      return 'user'
    case 1:
      return 'assistant'
    case 2:
      return 'system'
    default:
      return 'unknown'
  }
}

const handleSendChat = async () => {
  if (!chatInput.value.trim() || isSending.value) return

  const userMessage = {
    id: Date.now().toString(),
    role: 'user',
    content: chatInput.value,
  }

  chatMessages.value.push(userMessage)
  const messageToSend = chatInput.value
  chatInput.value = ''
  isSending.value = true

  // Add a temporary "thinking" message to show the user that processing is happening
  const thinkingMessageId = (Date.now() + 1).toString()
  const thinkingMessage = {
    id: thinkingMessageId,
    role: 'assistant',
    content: t('project_view.chat.processing') || 'Processing your message...',
    isThinking: true,
  }
  chatMessages.value.push(thinkingMessage)

  try {
    const response = await chatService.sendChatMessage(props.projectId, messageToSend)
    
    // Remove the thinking message
    const thinkingIndex = chatMessages.value.findIndex(m => m.id === thinkingMessageId)
    if (thinkingIndex !== -1) {
      chatMessages.value.splice(thinkingIndex, 1)
    }
    
    // Handle the response from the API
    // Response structure: { result: number (0=Error, 1=Success, 2=NotEnoughTokens, 3=NotFound), response?: { role: number (0=User, 1=Assistant, 2=System), content: string, text: string } }
    if (response && response.result === 1 && response.response) {
      // Success - result === 1 (MessageResult.Success)
      const assistantMessage = {
        id: (Date.now() + 2).toString(),
        role: mapRoleToString(response.response.role),
        content: response.response.content || '',
      }
      chatMessages.value.push(assistantMessage)
    } else {
      // Error result - handle different error types
      let errorContent = t('project_view.chat.error_message') || 'Sorry, there was an error processing your message.'
      
      if (response) {
        if (response.result === 3) {
          // NotFound
          errorContent = t('project_view.chat.not_found') || 'Project not found.'
        } else if (response.result === 2) {
          // NotEnoughTokens
          errorContent = t('project_view.chat.not_enough_tokens') || 'Not enough tokens to process your message.'
        } else if (response.result === 0) {
          // Error
          errorContent = t('project_view.chat.error_message') || 'Sorry, there was an error processing your message.'
        }
      }
      
      const errorMessage = {
        id: (Date.now() + 2).toString(),
        role: 'assistant',
        content: errorContent,
      }
      chatMessages.value.push(errorMessage)
      showToast({
        message: t('project_view.toast.failed_to_process_chat_message') || 'Failed to process chat message',
        color: 'danger',
        position: 'top-right',
        duration: 3000,
      })
    }
  }
  catch (error) {
    console.error('ChatView::handleSendChat: Exception: ', error)
    
    // Remove the thinking message
    const thinkingIndex = chatMessages.value.findIndex(m => m.id === thinkingMessageId)
    if (thinkingIndex !== -1) {
      chatMessages.value.splice(thinkingIndex, 1)
    }
    
    // Handle different error types based on HTTP status or result code
    let errorContent = t('project_view.chat.error_message') || 'Sorry, there was an error processing your message.'
    let toastMessage = t('project_view.toast.failed_to_send_chat_message') || 'Failed to send chat message'
    
    if (error.response) {
      const status = error.response.status
      if (status === 404) {
        // NotFound
        errorContent = t('project_view.chat.not_found') || 'Project not found.'
        toastMessage = t('project_view.toast.project_not_found') || 'Project not found'
      } else if (status === 400) {
        // BadRequest - could be Error or NotEnoughTokens
        if (error.result === 2) {
          errorContent = t('project_view.chat.not_enough_tokens') || 'Not enough tokens to process your message.'
          toastMessage = t('project_view.toast.not_enough_tokens') || 'Not enough tokens'
        } else {
          errorContent = t('project_view.chat.error_message') || 'Sorry, there was an error processing your message.'
        }
      }
    } else if (error.result !== undefined) {
      // Error object has result property from chatService
      if (error.result === 3) {
        errorContent = t('project_view.chat.not_found') || 'Project not found.'
        toastMessage = t('project_view.toast.project_not_found') || 'Project not found'
      } else if (error.result === 2) {
        errorContent = t('project_view.chat.not_enough_tokens') || 'Not enough tokens to process your message.'
        toastMessage = t('project_view.toast.not_enough_tokens') || 'Not enough tokens'
      }
    }
    
    showToast({
      message: toastMessage,
      color: 'danger',
      position: 'top-right',
      duration: 3000,
    })
    
    // Add an error message to the chat
    const errorMessage = {
      id: (Date.now() + 2).toString(),
      role: 'assistant',
      content: errorContent,
    }
    chatMessages.value.push(errorMessage)
  }
  finally {
    isSending.value = false
  }
}

const handleKeyDown = (e) => {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault()
    handleSendChat()
  }
}

const loadChatHistory = async (showSuccessToast = true) => {
  if (isLoadingHistory.value) return

  isLoadingHistory.value = true
  try {
    const history = await chatService.getChatHistory(props.projectId)
    
    // Handle the response - backend returns an array of IChatMessage: [{role: number, content: string}, ...]
    if (history && Array.isArray(history)) {
      // History is directly an array from the backend
      chatMessages.value = history.map((msg, index) => ({
        id: msg.id || `history-${index}-${Date.now()}`,
        role: mapRoleToString(msg.role),
        content: msg.content || '',
      }))
    } else {
      // If no history or unexpected format, set empty array
      chatMessages.value = []
    }
    
    if (showSuccessToast) {
      showToast({
        message: t('project_view.toast.chat_history_loaded') || 'Chat history loaded',
        color: 'success',
        position: 'top-right',
        duration: 2000,
      })
    }
  } catch (error) {
    console.error('ChatView::loadChatHistory: Exception: ', error)
    showToast({
      message: t('project_view.toast.failed_to_load_chat_history') || 'Failed to load chat history',
      color: 'danger',
      position: 'top-right',
      duration: 3000,
    })
  } finally {
    isLoadingHistory.value = false
  }
}

const handleRefreshHistory = async () => {
  await loadChatHistory(true)
}

// Load chat history when component mounts
onBeforeMount(() => {
  loadChatHistory(false)
})
</script>

<template>
  <div class="rounded-lg border border-gray-800 bg-gray-900/50 backdrop-blur-sm">
    <div class="border-b border-gray-800 p-4">
      <div class="flex items-center justify-between">
        <h2 class="text-lg font-semibold text-white flex items-center gap-2">
          <Send :size="20" class="text-purple-400" />
          {{ t('project_view.sections.ai_assistant') }}
        </h2>
        <button
          type="button"
          @click="handleRefreshHistory"
          :disabled="isLoadingHistory"
          class="inline-flex items-center justify-center rounded-md bg-gray-800 px-3 py-1.5 text-sm font-medium text-gray-300 hover:bg-gray-700 hover:text-white transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:pointer-events-none disabled:opacity-50"
          :title="t('project_view.chat.refresh_history') || 'Refresh chat history'"
        >
          <RefreshCw :size="16" :class="{ 'animate-spin': isLoadingHistory }" />
        </button>
      </div>
    </div>
    <div class="p-4 space-y-4">
      <!-- Chat Messages -->
      <div
        class="h-[300px] overflow-y-auto space-y-3 p-3 rounded-lg bg-gray-800/20 border border-gray-700/50"
      >
        <p
          v-if="chatMessages.length === 0"
          class="text-sm text-gray-400 text-center py-8"
        >
          {{ t('project_view.empty.chat_prompt') }}
        </p>
        <div
          v-for="message in chatMessages"
          :key="message.id"
          :class="[
            'p-3 rounded-lg',
            message.role === 'user'
              ? 'bg-purple-500/10 border border-purple-500/20 ml-8'
              : 'bg-gray-800/50 border border-gray-700 mr-8',
            message.isThinking ? 'opacity-70' : '',
          ]"
        >
          <p class="text-xs font-medium text-gray-400 mb-1 capitalize">
            {{ message.role }}
          </p>
          <p v-if="message.isThinking" class="text-sm text-gray-400 italic flex items-center gap-2">
            <span class="inline-block h-4 w-4 animate-spin rounded-full border-2 border-solid border-gray-400 border-r-transparent"></span>
            {{ message.content }}
          </p>
          <p v-else class="text-sm text-white">{{ message.content }}</p>
        </div>
      </div>

      <!-- Chat Input -->
      <div class="flex gap-2">
        <input
          type="text"
          :placeholder="t('project_view.placeholders.chat_input')"
          v-model="chatInput"
          @keydown="handleKeyDown"
          :disabled="isSending"
          class="flex h-10 w-full rounded-md border border-gray-700 bg-gray-900 px-3 py-2 text-sm text-white placeholder:text-gray-500 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 focus-visible:border-purple-500 disabled:cursor-not-allowed disabled:opacity-50"
        />
        <button
          type="button"
          @click="handleSendChat"
          :disabled="!chatInput.trim() || isSending"
          class="inline-flex items-center justify-center rounded-md bg-purple-600 px-3 py-2 text-sm font-medium text-white hover:bg-purple-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:pointer-events-none disabled:opacity-50"
        >
          <Send :size="16" />
        </button>
      </div>
    </div>
  </div>
</template>

