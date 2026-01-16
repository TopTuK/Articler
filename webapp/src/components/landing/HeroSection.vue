<script setup>
import { ArrowRight, FileText, Brain, Zap } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'

const { t } = useI18n()
const router = useRouter()

const navigateToLogin = () => {
  router.push({ name: 'Login' })
}
</script>

<template>
  <section class="relative min-h-screen flex items-center justify-center pt-16 overflow-hidden bg-black">
    <!-- Background effects -->
    <div class="absolute inset-0 gradient-hero" />
    <div class="absolute top-1/4 left-1/2 -translate-x-1/2 w-[800px] h-[800px] gradient-glow animate-pulse-glow" />
    
    <!-- Floating elements -->
    <div class="absolute top-32 left-[15%] w-16 h-16 glass-card-enhanced flex items-center justify-center animate-float opacity-80">
      <FileText :size="32" class="text-purple-400" />
    </div>
    <div class="absolute top-48 right-[20%] w-14 h-14 glass-card-enhanced flex items-center justify-center animate-float delay-200 opacity-80">
      <Brain :size="28" class="text-blue-400" />
    </div>
    <div class="absolute bottom-32 left-[25%] w-12 h-12 glass-card-enhanced flex items-center justify-center animate-float delay-400 opacity-80">
      <Zap :size="24" class="text-pink-400" />
    </div>

    <div class="container mx-auto px-6 relative z-10">
      <div class="max-w-4xl mx-auto text-center">
        <!-- Badge -->
        <div class="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-gradient-to-r from-purple-500/20 to-blue-500/20 border border-purple-400/40 backdrop-blur-sm mb-8 animate-fade-in-up opacity-0 shadow-lg shadow-purple-500/20">
          <Zap :size="16" class="text-purple-300" />
          <span class="text-sm font-medium text-purple-200">{{ t('landing.hero.badge') }}</span>
        </div>

        <!-- Headline -->
        <h1 class="font-display font-bold text-5xl sm:text-6xl lg:text-7xl leading-tight mb-6 animate-fade-in-up opacity-0 delay-100 text-white">
          {{ t('landing.hero.headline_part1') }}
          <span class="block text-gradient">{{ t('landing.hero.headline_part2') }}</span>
        </h1>

        <!-- Subheadline -->
        <p class="text-lg sm:text-xl text-gray-300 max-w-2xl mx-auto mb-10 leading-relaxed animate-fade-in-up opacity-0 delay-200">
          {{ t('landing.hero.subheadline') }}
        </p>

        <!-- CTA Buttons -->
        <div class="flex flex-col sm:flex-row items-center justify-center gap-4 mb-16 animate-fade-in-up opacity-0 delay-300">
          <va-button class="btn-hero" size="large" @click="navigateToLogin">
            <span class="flex items-center gap-2">
              {{ t('landing.hero.cta_start') }}
              <ArrowRight :size="20" />
            </span>
          </va-button>
        </div>

        <!-- Stats -->
        <div class="flex flex-wrap items-center justify-center gap-8 sm:gap-12 animate-fade-in-up opacity-0 delay-400">
          <div class="text-center">
            <div class="font-display font-bold text-4xl text-white mb-1">10K+</div>
            <div class="text-sm text-gray-400">{{ t('landing.hero.stats_writers') }}</div>
          </div>
          <div class="w-px h-12 bg-gradient-to-b from-transparent via-gray-600 to-transparent hidden sm:block" />
          <div class="text-center">
            <div class="font-display font-bold text-4xl text-white mb-1">500K+</div>
            <div class="text-sm text-gray-400">{{ t('landing.hero.stats_articles') }}</div>
          </div>
          <div class="w-px h-12 bg-gradient-to-b from-transparent via-gray-600 to-transparent hidden sm:block" />
          <div class="text-center">
            <div class="font-display font-bold text-4xl text-white mb-1">99%</div>
            <div class="text-sm text-gray-400">{{ t('landing.hero.stats_satisfaction') }}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Bottom gradient fade -->
    <div class="absolute bottom-0 left-0 right-0 h-32 bg-gradient-to-t from-black to-transparent" />
  </section>
</template>

<style scoped>
.glass-card-enhanced {
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-radius: 12px;
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
}

:deep(.btn-hero:hover),
:deep(.btn-hero:hover button),
:deep(.btn-hero:hover .va-button__content) {
  transform: translateY(-2px) !important;
  box-shadow: 0 6px 30px rgba(102, 126, 234, 0.6), 0 0 40px rgba(102, 126, 234, 0.3) !important;
}
</style>

