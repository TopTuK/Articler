<script setup>
import VLogo from '@/components/common/VLogo.vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { nextTick } from 'vue'

const { t } = useI18n()
const router = useRouter()
const authStore = useAuthStore()

const goToProjects = () => {
  router.push({ name: 'Projects' })
}

const goToProfile = () => {
  router.push({ name: 'Profile' })
}

const handleLogout = async () => {
  authStore.logout()
  await nextTick()
  router.replace({ name: 'Home' })
}
</script>

<template>
  <nav class="fixed top-0 left-0 right-0 z-50 bg-black/90 backdrop-blur-xl border-b border-gray-800/50">
    <div class="container mx-auto px-6 h-16 flex items-center justify-between">
      <router-link to="/projects" class="flex items-center gap-2 cursor-pointer">
        <div class="w-9 h-9 rounded-lg gradient-primary flex items-center justify-center shadow-glow">
          <VLogo :size="20" />
        </div>
        <span class="font-display font-bold text-xl text-white">{{ t('common.app_name') }}</span>
      </router-link>

      <div class="flex items-center gap-3">
        <va-button 
          class="btn-variegated-secondary" 
          @click="goToProjects"
        >
          {{ t('common.project_button_title') }}
        </va-button>
        <va-button 
          class="btn-variegated-secondary" 
          @click="goToProfile"
        >
          {{ t('common.profile_button_title') }}
        </va-button>
        <va-button 
          class="btn-variegated-primary"
          @click="handleLogout"
        >
          {{ t('common.logout_button_title') }}
        </va-button>
      </div>
    </div>
  </nav>
</template>

<style scoped>
:deep(.btn-variegated-primary),
:deep(.btn-variegated-primary button),
:deep(.btn-variegated-primary .va-button__content) {
  background: #4f46e5 !important;
  border: none !important;
  color: white !important;
  font-weight: 600 !important;
  border-radius: 8px !important;
  transition: all 0.2s ease !important;
  box-shadow: 0 2px 4px rgba(79, 70, 229, 0.2) !important;
}

:deep(.btn-variegated-primary:hover),
:deep(.btn-variegated-primary:hover button),
:deep(.btn-variegated-primary:hover .va-button__content) {
  background: #4338ca !important;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.4) !important;
  transform: translateY(-1px) !important;
}

:deep(.btn-variegated-secondary),
:deep(.btn-variegated-secondary button),
:deep(.btn-variegated-secondary .va-button__content) {
  background: transparent !important;
  border: 1.5px solid #4b5563 !important;
  color: #e5e7eb !important;
  font-weight: 600 !important;
  border-radius: 8px !important;
  transition: all 0.2s ease !important;
}

:deep(.btn-variegated-secondary:hover),
:deep(.btn-variegated-secondary:hover button),
:deep(.btn-variegated-secondary:hover .va-button__content) {
  background: #374151 !important;
  border-color: #6b7280 !important;
  color: white !important;
  transform: translateY(-1px) !important;
}
</style>