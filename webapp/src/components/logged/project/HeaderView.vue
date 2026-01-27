<script setup>
import VLogo from '@/components/common/VLogo.vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useUserStore } from '@/stores/userStore'
import { nextTick } from 'vue'

const { t } = useI18n()
const router = useRouter()
const authStore = useAuthStore()
const userStore = useUserStore()

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
        <span class="text-white/80 font-medium px-3 py-1.5">
          Tokens: {{ userStore.userTokenCount }}
        </span>
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
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 50%, #4facfe 100%) !important;
  border: none !important;
  color: white !important;
  font-weight: 600 !important;
  border-radius: 8px !important;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1) !important;
  box-shadow: 0 4px 15px rgba(245, 87, 108, 0.4), 0 0 20px rgba(79, 172, 254, 0.3) !important;
  position: relative !important;
  overflow: hidden !important;
}

:deep(.btn-variegated-primary::before) {
  content: '' !important;
  position: absolute !important;
  top: 0 !important;
  left: -100% !important;
  width: 100% !important;
  height: 100% !important;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.3), transparent) !important;
  transition: left 0.6s ease !important;
}

:deep(.btn-variegated-primary:hover::before) {
  left: 100% !important;
}

:deep(.btn-variegated-primary:hover),
:deep(.btn-variegated-primary:hover button),
:deep(.btn-variegated-primary:hover .va-button__content) {
  background: linear-gradient(135deg, #fa709a 0%, #fee140 50%, #30cfd0 100%) !important;
  box-shadow: 0 8px 25px rgba(250, 112, 154, 0.5), 0 0 30px rgba(48, 207, 208, 0.4) !important;
  transform: translateY(-1px) !important;
}

:deep(.btn-variegated-secondary),
:deep(.btn-variegated-secondary button),
:deep(.btn-variegated-secondary .va-button__content) {
  background: transparent !important;
  border: 1.5px solid transparent !important;
  background-image: linear-gradient(rgba(0, 0, 0, 0.9), rgba(0, 0, 0, 0.9)), 
                    linear-gradient(135deg, #667eea 0%, #764ba2 50%, #f093fb 100%) !important;
  background-origin: border-box !important;
  background-clip: padding-box, border-box !important;
  color: #e0e7ff !important;
  font-weight: 600 !important;
  border-radius: 8px !important;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1) !important;
  position: relative !important;
}

:deep(.btn-variegated-secondary::before) {
  content: '' !important;
  position: absolute !important;
  inset: 0 !important;
  border-radius: 8px !important;
  padding: 1.5px !important;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 50%, #f093fb 100%) !important;
  -webkit-mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0) !important;
  mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0) !important;
  -webkit-mask-composite: xor !important;
  mask-composite: exclude !important;
  opacity: 0 !important;
  transition: opacity 0.3s ease !important;
}

:deep(.btn-variegated-secondary:hover),
:deep(.btn-variegated-secondary:hover button),
:deep(.btn-variegated-secondary:hover .va-button__content) {
  background-image: linear-gradient(135deg, rgba(102, 126, 234, 0.15) 0%, rgba(118, 75, 162, 0.15) 50%, rgba(240, 147, 251, 0.15) 100%), 
                    linear-gradient(135deg, #667eea 0%, #764ba2 50%, #f093fb 100%) !important;
  background-origin: border-box !important;
  background-clip: padding-box, border-box !important;
  border-color: transparent !important;
  color: white !important;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3), 0 0 20px rgba(240, 147, 251, 0.2) !important;
  transform: translateY(-1px) !important;
}
</style>