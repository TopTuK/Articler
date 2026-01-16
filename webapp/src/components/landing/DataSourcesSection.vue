<script setup>
import { FileText, File, FileCode, Link, Database, MessageSquare } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { computed } from 'vue'

const { t } = useI18n()

const sources = computed(() => [
  { icon: FileText, label: t('landing.data_sources.sources.pdf.label'), description: t('landing.data_sources.sources.pdf.description'), color: 'purple' },
  { icon: File, label: t('landing.data_sources.sources.text.label'), description: t('landing.data_sources.sources.text.description'), color: 'blue' },
  { icon: FileCode, label: t('landing.data_sources.sources.markdown.label'), description: t('landing.data_sources.sources.markdown.description'), color: 'pink' },
  { icon: Link, label: t('landing.data_sources.sources.web_urls.label'), description: t('landing.data_sources.sources.web_urls.description'), color: 'cyan' },
  { icon: Database, label: t('landing.data_sources.sources.databases.label'), description: t('landing.data_sources.sources.databases.description'), color: 'indigo' },
  { icon: MessageSquare, label: t('landing.data_sources.sources.direct_input.label'), description: t('landing.data_sources.sources.direct_input.description'), color: 'violet' },
])

const getOrbitPosition = (index) => {
  const angle = (index * 60) * (Math.PI / 180)
  const radius = 140
  const x = Math.cos(angle) * radius
  const y = Math.sin(angle) * radius
  return { x, y }
}

const getLineEndPosition = (angle) => {
  return {
    x: `${50 + 35 * Math.cos((angle * Math.PI) / 180)}%`,
    y: `${50 + 35 * Math.sin((angle * Math.PI) / 180)}%`,
  }
}

const getIconColorClass = (color) => {
  const colors = {
    purple: 'text-purple-400',
    blue: 'text-blue-400',
    pink: 'text-pink-400',
    cyan: 'text-cyan-400',
    indigo: 'text-indigo-400',
    violet: 'text-violet-400',
  }
  return colors[color] || 'text-primary'
}

const getIconBgClass = (color) => {
  const colors = {
    purple: 'bg-gradient-to-br from-purple-500/20 to-purple-600/20',
    blue: 'bg-gradient-to-br from-blue-500/20 to-blue-600/20',
    pink: 'bg-gradient-to-br from-pink-500/20 to-pink-600/20',
    cyan: 'bg-gradient-to-br from-cyan-500/20 to-cyan-600/20',
    indigo: 'bg-gradient-to-br from-indigo-500/20 to-indigo-600/20',
    violet: 'bg-gradient-to-br from-violet-500/20 to-violet-600/20',
  }
  return colors[color] || 'bg-primary/10'
}
</script>

<template>
  <section id="sources" class="py-24 relative overflow-hidden bg-black">
    <!-- Background effects -->
    <div class="absolute inset-0 gradient-hero opacity-50" />
    <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] gradient-glow animate-pulse-glow opacity-30" />
    
    <div class="container mx-auto px-6 relative z-10">
      <div class="grid lg:grid-cols-2 gap-16 items-center">
        <!-- Left content -->
        <div class="animate-fade-in-up opacity-0" style="animation-delay: 0.1s">
          <span class="text-purple-300 font-semibold text-sm uppercase tracking-wider mb-4 block">{{ t('landing.data_sources.section_label') }}</span>
          <h2 class="font-display font-bold text-4xl sm:text-5xl mb-6 text-white">
            {{ t('landing.data_sources.title_part1') }}
            <span class="text-gradient"> {{ t('landing.data_sources.title_part2') }}</span>
          </h2>
          <p class="text-gray-300 text-lg mb-8 leading-relaxed">
            {{ t('landing.data_sources.description') }}
          </p>

          <div class="grid grid-cols-2 gap-4">
            <div
              v-for="(source, index) in sources"
              :key="source.label"
              class="source-card flex items-start gap-3 p-5 group cursor-pointer"
              :style="{ animationDelay: `${index * 100 + 200}ms` }"
            >
              <div class="source-icon-wrapper">
                <div :class="`w-12 h-12 rounded-xl flex items-center justify-center shrink-0 group-hover:scale-110 transition-all duration-300 ${getIconBgClass(source.color)}`">
                  <component :is="source.icon" :class="`w-6 h-6 ${getIconColorClass(source.color)}`" />
                </div>
              </div>
              <div class="flex-1">
                <div class="font-semibold text-white text-sm mb-1 group-hover:text-transparent group-hover:bg-clip-text group-hover:bg-gradient-to-r group-hover:from-purple-400 group-hover:to-blue-400 transition-all duration-300">
                  {{ source.label }}
                </div>
                <div class="text-xs text-gray-400 group-hover:text-gray-300 transition-colors duration-300">
                  {{ source.description }}
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Right visual -->
        <div class="relative animate-fade-in-up opacity-0" style="animation-delay: 0.3s">
          <!-- Central hub -->
          <div class="relative w-full aspect-square max-w-md mx-auto">
            <!-- Enhanced glow effect -->
            <div class="absolute inset-0 gradient-glow scale-150 opacity-60" />
            <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-96 h-96 rounded-full bg-gradient-to-r from-purple-500/10 via-blue-500/10 to-pink-500/10 blur-3xl animate-pulse-glow" />

            <!-- Center circle -->
            <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-32 h-32 rounded-full gradient-primary shadow-glow flex items-center justify-center z-10 animate-pulse-glow">
              <span class="font-display font-bold text-2xl text-white">AI</span>
            </div>

            <!-- Orbiting sources -->
            <div
              v-for="(source, index) in sources"
              :key="source.label"
              class="orbit-card absolute top-1/2 left-1/2 w-16 h-16 glass-card-enhanced flex items-center justify-center animate-float group"
              :style="{
                '--orbit-x': `${getOrbitPosition(index).x}px`,
                '--orbit-y': `${getOrbitPosition(index).y}px`,
                transform: `translate(calc(-50% + ${getOrbitPosition(index).x}px), calc(-50% + ${getOrbitPosition(index).y}px))`,
                animationDelay: `${index * 200}ms`,
              }"
            >
              <component :is="source.icon" :class="`w-7 h-7 ${getIconColorClass(source.color)} group-hover:scale-110 transition-transform duration-300`" />
              <div :class="`absolute inset-0 rounded-xl ${getIconBgClass(source.color)} opacity-0 group-hover:opacity-30 transition-opacity duration-300 blur-md`" />
            </div>

            <!-- Enhanced connection lines -->
            <svg class="absolute inset-0 w-full h-full" style="transform: rotate(-30deg)">
              <line
                v-for="(angle, i) in [0, 60, 120, 180, 240, 300]"
                :key="i"
                x1="50%"
                y1="50%"
                :x2="getLineEndPosition(angle).x"
                :y2="getLineEndPosition(angle).y"
                stroke="url(#gradient-line)"
                stroke-width="2"
                stroke-opacity="0.4"
                stroke-dasharray="4 4"
                class="animate-pulse"
              />
              <defs>
                <linearGradient id="gradient-line" x1="0%" y1="0%" x2="100%" y2="100%">
                  <stop offset="0%" style="stop-color:#667eea;stop-opacity:0.6" />
                  <stop offset="50%" style="stop-color:#764ba2;stop-opacity:0.4" />
                  <stop offset="100%" style="stop-color:#f093fb;stop-opacity:0.6" />
                </linearGradient>
              </defs>
            </svg>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.source-card {
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 16px;
  transition: all 0.3s ease;
  animation: fade-in-up 0.8s ease-out forwards;
  opacity: 0;
}

.source-card:hover {
  background: rgba(255, 255, 255, 0.08);
  border-color: rgba(139, 92, 246, 0.4);
  transform: translateY(-4px);
  box-shadow: 
    0 10px 40px rgba(0, 0, 0, 0.4),
    0 0 30px rgba(139, 92, 246, 0.15),
    inset 0 1px 0 rgba(255, 255, 255, 0.1);
}

.source-icon-wrapper {
  position: relative;
}

.source-icon-wrapper::before {
  content: '';
  position: absolute;
  inset: -4px;
  border-radius: 16px;
  background: linear-gradient(135deg, rgba(139, 92, 246, 0.2), rgba(59, 130, 246, 0.2));
  opacity: 0;
  transition: opacity 0.3s ease;
  z-index: -1;
}

.source-card:hover .source-icon-wrapper::before {
  opacity: 1;
}

.glass-card-enhanced {
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-radius: 12px;
  box-shadow: 
    0 8px 32px rgba(0, 0, 0, 0.3),
    0 0 20px rgba(139, 92, 246, 0.1);
  transition: all 0.3s ease;
}

.orbit-card {
  transition: all 0.3s ease;
}

.orbit-card:hover {
  background: rgba(255, 255, 255, 0.12);
  border-color: rgba(139, 92, 246, 0.4);
  box-shadow: 
    0 8px 32px rgba(0, 0, 0, 0.4),
    0 0 30px rgba(139, 92, 246, 0.3);
  z-index: 20;
  transform: translate(calc(-50% + var(--orbit-x, 0px)), calc(-50% + var(--orbit-y, 0px))) scale(1.15) !important;
}

@keyframes fade-in-up {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>

