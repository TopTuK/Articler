<script setup>
import { watch } from 'vue'
import { AlertTriangle, X } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false,
  },
  title: {
    type: String,
    required: true,
  },
  message: {
    type: String,
    required: true,
  },
  confirmText: {
    type: String,
    default: null,
  },
  cancelText: {
    type: String,
    default: null,
  },
  confirmButtonColor: {
    type: String,
    default: 'danger',
  },
})

const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

const { t } = useI18n()

const handleClose = () => {
  emit('update:modelValue', false)
  emit('cancel')
}

const handleBackdropClick = (e) => {
  if (e.target === e.currentTarget) {
    handleClose()
  }
}

const handleConfirm = () => {
  emit('confirm')
  emit('update:modelValue', false)
}

const handleKeydown = (e) => {
  if (e.key === 'Escape') {
    handleClose()
  }
}

const getConfirmButtonClass = () => {
  const baseClass = 'inline-flex items-center justify-center gap-2 rounded-md px-4 py-2 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500'
  
  if (props.confirmButtonColor === 'danger') {
    return `${baseClass} bg-red-600 text-white hover:bg-red-700`
  }
  return `${baseClass} bg-purple-600 text-white hover:bg-purple-700`
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
            class="w-full max-w-md rounded-lg border border-gray-800 bg-gray-900/95 backdrop-blur-sm shadow-2xl flex flex-col overflow-hidden"
            @click.stop
          >
            <!-- Header -->
            <div class="flex items-center justify-between p-6 border-b border-gray-800">
              <h2 class="text-xl font-semibold text-white flex items-center gap-2">
                <AlertTriangle :size="20" class="text-red-400" />
                {{ title }}
              </h2>
              <button
                type="button"
                @click="handleClose"
                class="inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:text-white hover:bg-gray-800 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500"
              >
                <X :size="20" />
              </button>
            </div>

            <!-- Content -->
            <div class="p-6">
              <p class="text-sm text-gray-300 leading-relaxed">
                {{ message }}
              </p>
            </div>

            <!-- Footer -->
            <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-800">
              <button
                type="button"
                @click="handleClose"
                class="inline-flex items-center justify-center gap-2 rounded-md border border-gray-700 bg-gray-800/50 px-4 py-2 text-sm font-medium text-white hover:bg-gray-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500"
              >
                {{ cancelText || t('project_view.actions.cancel') }}
              </button>
              <button
                type="button"
                @click="handleConfirm"
                :class="getConfirmButtonClass()"
              >
                {{ confirmText || t('project_view.confirm.delete') }}
              </button>
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>
