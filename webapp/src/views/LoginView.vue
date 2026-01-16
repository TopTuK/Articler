<script setup>
import { ArrowLeft } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const handleSignIn = () => {
  // TODO: Implement PM Moscow Club authentication
  console.log('Sign in with PM Moscow Club clicked')
  window.location.href = "/auth/SignInPmi";
}
</script>

<template>
  <div class="relative min-h-screen bg-black flex flex-col overflow-hidden">
    <!-- Background effects -->
    <div class="absolute inset-0 gradient-hero opacity-50" />
    <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] gradient-glow animate-pulse-glow opacity-30" />

    <!-- Back link -->
    <div class="relative z-10 p-6">
      <router-link
        to="/"
        class="inline-flex items-center gap-2 text-gray-400 hover:text-white transition-colors"
      >
        <ArrowLeft :size="16" />
        <span>{{ t('login.back_to_home') }}</span>
      </router-link>
    </div>

    <!-- Main content -->
    <div class="relative z-10 flex-1 flex items-center justify-center px-6 py-12">
      <div class="w-full max-w-md space-y-8 text-center animate-fade-in-up">
        <!-- Logo/Brand -->
        <div class="space-y-4">
          <h1 class="text-4xl sm:text-5xl font-display font-bold text-white">
            <span class="text-gradient">{{ t('common.app_name') }}</span>
          </h1>
          <p class="text-gray-300 text-lg">
            {{ t('login.subtitle') }}
          </p>
        </div>

        <!-- Sign in card -->
        <div class="glass-card-enhanced rounded-2xl p-8 space-y-6">
          <div class="space-y-2">
            <h2 class="text-xl font-semibold text-white">{{ t('login.welcome_back') }}</h2>
            <p class="text-sm text-gray-400">
              {{ t('login.continue_with_account') }}
            </p>
          </div>

          <va-button
            @click="handleSignIn"
            size="large"
            class="w-full btn-hero"
          >
            {{ t('login.sign_in_button') }}
          </va-button>
        </div>

        <!-- Footer text -->
        <p class="text-xs text-gray-500">
          {{ t('login.terms_notice') }}
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.glass-card-enhanced {
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.15);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3), 0 0 20px rgba(139, 92, 246, 0.1);
}

:deep(.btn-hero),
:deep(.btn-hero button),
:deep(.btn-hero .va-button__content) {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 25%, #f093fb 50%, #4facfe 75%, #00f2fe 100%) !important;
  background-size: 200% 200% !important;
  animation: gradient-shift 3s ease infinite !important;
  border: none !important;
  color: white !important;
  font-weight: 600 !important;
  padding: 0.75rem 1.5rem !important;
  box-shadow: 0 4px 20px rgba(102, 126, 234, 0.4), 0 0 30px rgba(102, 126, 234, 0.2) !important;
  transition: all 0.3s ease !important;
  width: 100% !important;
}

:deep(.btn-hero:hover),
:deep(.btn-hero:hover button),
:deep(.btn-hero:hover .va-button__content) {
  transform: translateY(-2px) !important;
  box-shadow: 0 6px 30px rgba(102, 126, 234, 0.6), 0 0 40px rgba(102, 126, 234, 0.3) !important;
}

@keyframes gradient-shift {
  0% {
    background-position: 0% 50%;
  }
  50% {
    background-position: 100% 50%;
  }
  100% {
    background-position: 0% 50%;
  }
}
</style>

