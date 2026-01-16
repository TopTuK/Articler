<script setup>
import VLogo from '@/components/common/VLogo.vue'
import { useI18n } from 'vue-i18n'
import { useRouter, useRoute } from 'vue-router'
import { computed } from 'vue'

const { t } = useI18n()
const router = useRouter()
const route = useRoute()

const isLoginPage = computed(() => route.name === 'Login')

const goToLogin = () => {
  router.push({ name: 'Login' })
}

const goToHome = () => {
  router.push({ name: 'Home' })
}

const goToSection = (sectionId) => {
  if (isLoginPage.value) {
    // If on login page, navigate to home first, then scroll to section
    router.push({ name: 'Home' }).then(() => {
      setTimeout(() => {
        const element = document.getElementById(sectionId)
        if (element) {
          element.scrollIntoView({ behavior: 'smooth' })
        }
      }, 100)
    })
  } else {
    // If on home page, just scroll to section
    const element = document.getElementById(sectionId)
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' })
    }
  }
}
</script>

<template>
    <nav class="fixed top-0 left-0 right-0 z-50 bg-black/90 backdrop-blur-xl border-b border-gray-800/50">
      <div class="container mx-auto px-6 h-16 flex items-center justify-between">
        <router-link to="/" class="flex items-center gap-2 cursor-pointer">
          <div class="w-9 h-9 rounded-lg gradient-primary flex items-center justify-center shadow-glow">
            <VLogo :size="20" />
          </div>
          <span class="font-display font-bold text-xl text-white">{{ t('common.app_name') }}</span>
        </router-link>

        <div class="hidden md:flex items-center gap-8">
          <a 
            @click.prevent="goToSection('features')" 
            class="text-gray-400 hover:text-white transition-colors text-sm font-medium cursor-pointer"
          >
            {{ t('landing.header.nav_features') }}
          </a>
          <a 
            @click.prevent="goToSection('how-it-works')" 
            class="text-gray-400 hover:text-white transition-colors text-sm font-medium cursor-pointer"
          >
            {{ t('landing.header.nav_how_it_works') }}
          </a>
          <a 
            @click.prevent="goToSection('sources')" 
            class="text-gray-400 hover:text-white transition-colors text-sm font-medium cursor-pointer"
          >
            {{ t('landing.header.nav_data_sources') }}
          </a>
        </div>

        <div class="flex items-center gap-3">
          <va-button 
            v-if="!isLoginPage"
            class="btn-variegated-secondary" 
            @click="goToLogin"
          >
            {{ t('landing.header.button_sign_in') }}
          </va-button>
          <va-button 
            v-if="!isLoginPage"
            class="btn-variegated-primary"
            @click="goToHome"
          >
            {{ t('landing.header.button_get_started') }}
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