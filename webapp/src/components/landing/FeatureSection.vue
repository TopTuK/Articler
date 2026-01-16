<script setup>
import { Brain, Layers, Wand2, Globe, Lock, Gauge } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { computed } from 'vue'

const { t } = useI18n()

const features = computed(() => [
  {
    icon: Brain,
    title: t('landing.features.items.ai_learns.title'),
    description: t('landing.features.items.ai_learns.description'),
  },
  {
    icon: Layers,
    title: t('landing.features.items.knowledge_stacking.title'),
    description: t('landing.features.items.knowledge_stacking.description'),
  },
  {
    icon: Wand2,
    title: t('landing.features.items.smart_generation.title'),
    description: t('landing.features.items.smart_generation.description'),
  },
  {
    icon: Globe,
    title: t('landing.features.items.multi_format.title'),
    description: t('landing.features.items.multi_format.description'),
  },
  {
    icon: Lock,
    title: t('landing.features.items.private_secure.title'),
    description: t('landing.features.items.private_secure.description'),
  },
  {
    icon: Gauge,
    title: t('landing.features.items.lightning_fast.title'),
    description: t('landing.features.items.lightning_fast.description'),
  },
])

const getIconBgClass = (index) => {
  const classes = [
    'bg-gradient-to-br from-purple-500/20 to-purple-600/20',
    'bg-gradient-to-br from-blue-500/20 to-blue-600/20',
    'bg-gradient-to-br from-pink-500/20 to-pink-600/20',
    'bg-gradient-to-br from-cyan-500/20 to-cyan-600/20',
    'bg-gradient-to-br from-indigo-500/20 to-indigo-600/20',
    'bg-gradient-to-br from-violet-500/20 to-violet-600/20',
  ]
  return classes[index % classes.length]
}

const getIconColorClass = (index) => {
  const classes = [
    'text-purple-400',
    'text-blue-400',
    'text-pink-400',
    'text-cyan-400',
    'text-indigo-400',
    'text-violet-400',
  ]
  return classes[index % classes.length]
}
</script>

<template>
  <section id="features" class="py-24 relative bg-black">
    <div class="container mx-auto px-6">
      <!-- Section header -->
      <div class="text-center max-w-2xl mx-auto mb-16">
        <span class="text-purple-300 font-semibold text-sm uppercase tracking-wider mb-4 block">{{ t('landing.features.section_label') }}</span>
        <h2 class="font-display font-bold text-4xl sm:text-5xl mb-6 text-white">
          {{ t('landing.features.title_part1') }}
          <span class="text-gradient"> {{ t('landing.features.title_part2') }}</span>
        </h2>
        <p class="text-gray-300 text-lg">
          {{ t('landing.features.description') }}
        </p>
      </div>

      <!-- Features grid -->
      <div class="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="(feature, index) in features"
          :key="index"
          class="feature-card p-8 group cursor-pointer"
          :style="{ animationDelay: `${index * 100}ms` }"
        >
          <div class="feature-icon-wrapper mb-6">
            <div :class="`w-14 h-14 rounded-xl flex items-center justify-center group-hover:scale-110 transition-all duration-300 ${getIconBgClass(index)}`">
              <component :is="feature.icon" :class="`w-7 h-7 ${getIconColorClass(index)}`" />
            </div>
          </div>
          <h3 class="font-display font-semibold text-xl mb-3 text-white group-hover:text-transparent group-hover:bg-clip-text group-hover:bg-gradient-to-r group-hover:from-purple-400 group-hover:to-blue-400 transition-all duration-300">
            {{ feature.title }}
          </h3>
          <p class="text-gray-400 leading-relaxed group-hover:text-gray-300 transition-colors duration-300">
            {{ feature.description }}
          </p>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.feature-card {
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 16px;
  transition: all 0.3s ease;
  animation: fade-in-up 0.8s ease-out forwards;
  opacity: 0;
}

.feature-card:hover {
  background: rgba(255, 255, 255, 0.08);
  border-color: rgba(139, 92, 246, 0.4);
  transform: translateY(-4px);
  box-shadow: 
    0 10px 40px rgba(0, 0, 0, 0.4),
    0 0 30px rgba(139, 92, 246, 0.15),
    inset 0 1px 0 rgba(255, 255, 255, 0.1);
}

.feature-icon-wrapper {
  position: relative;
}

.feature-icon-wrapper::before {
  content: '';
  position: absolute;
  inset: -4px;
  border-radius: 16px;
  background: linear-gradient(135deg, rgba(139, 92, 246, 0.2), rgba(59, 130, 246, 0.2));
  opacity: 0;
  transition: opacity 0.3s ease;
  z-index: -1;
}

.feature-card:hover .feature-icon-wrapper::before {
  opacity: 1;
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

