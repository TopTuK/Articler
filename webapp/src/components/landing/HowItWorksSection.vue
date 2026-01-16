<script setup>
import { Upload, Settings2, PenTool } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { computed } from 'vue'

const { t } = useI18n()

const steps = computed(() => [
  {
    icon: Upload,
    number: t('landing.how_it_works.steps.upload.number'),
    title: t('landing.how_it_works.steps.upload.title'),
    description: t('landing.how_it_works.steps.upload.description'),
    gradient: "from-purple-500/20 to-purple-600/20",
    iconColor: "text-purple-400",
    glowColor: "rgba(168, 85, 247, 0.3)",
  },
  {
    icon: Settings2,
    number: t('landing.how_it_works.steps.configure.number'),
    title: t('landing.how_it_works.steps.configure.title'),
    description: t('landing.how_it_works.steps.configure.description'),
    gradient: "from-blue-500/20 to-blue-600/20",
    iconColor: "text-blue-400",
    glowColor: "rgba(59, 130, 246, 0.3)",
  },
  {
    icon: PenTool,
    number: t('landing.how_it_works.steps.generate.number'),
    title: t('landing.how_it_works.steps.generate.title'),
    description: t('landing.how_it_works.steps.generate.description'),
    gradient: "from-pink-500/20 to-pink-600/20",
    iconColor: "text-pink-400",
    glowColor: "rgba(236, 72, 153, 0.3)",
  },
])
</script>

<template>
  <section id="how-it-works" class="py-24 relative bg-black overflow-hidden">
    <!-- Background effects -->
    <div class="absolute inset-0 bg-gradient-to-b from-transparent via-purple-500/5 to-transparent" />
    <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-gradient-radial from-purple-500/10 via-transparent to-transparent rounded-full blur-3xl" />

    <div class="container mx-auto px-6 relative z-10">
      <!-- Section header -->
      <div class="text-center max-w-2xl mx-auto mb-20 header-animate">
        <span class="text-purple-300 font-semibold text-sm uppercase tracking-wider mb-4 block">{{ t('landing.how_it_works.section_label') }}</span>
        <h2 class="font-display font-bold text-4xl sm:text-5xl mb-6 text-white">
          {{ t('landing.how_it_works.title_part1') }}
          <span class="text-gradient"> {{ t('landing.how_it_works.title_part2') }}</span>
        </h2>
        <p class="text-gray-300 text-lg leading-relaxed">
          {{ t('landing.how_it_works.description') }}
        </p>
      </div>

      <!-- Steps -->
      <div class="grid lg:grid-cols-3 gap-8 lg:gap-8 relative">
        <!-- Animated connector line for desktop -->
        <div class="hidden lg:block absolute top-24 left-[16.66%] right-[16.66%] h-0.5">
          <div class="absolute inset-0 bg-gradient-to-r from-transparent via-purple-500/40 to-transparent connector-line" />
        </div>

        <div
          v-for="(step, index) in steps"
          :key="step.number"
          class="relative"
        >
          <div
            class="step-card p-8 lg:p-10 text-center lg:text-left relative overflow-hidden group cursor-pointer"
            :style="{ animationDelay: `${index * 150}ms` }"
          >
            <!-- Large number background with glow -->
            <span class="absolute -top-6 -right-4 font-display font-bold text-[140px] text-white/[0.02] leading-none select-none group-hover:text-white/[0.04] transition-colors duration-500">
              {{ step.number }}
            </span>

            <!-- Gradient overlay on hover -->
            <div class="absolute inset-0 bg-gradient-to-br opacity-0 group-hover:opacity-100 transition-opacity duration-500"
                 :class="`${step.gradient}`" />

            <div class="relative z-10">
              <!-- Icon wrapper with enhanced styling -->
              <div class="step-icon-wrapper mb-6">
                <div
                  class="w-20 h-20 rounded-2xl flex items-center justify-center mx-auto lg:mx-0 group-hover:scale-110 transition-all duration-300 relative"
                  :class="`bg-gradient-to-br ${step.gradient}`"
                >
                  <!-- Glow effect -->
                  <div
                    class="absolute inset-0 rounded-2xl blur-xl opacity-0 group-hover:opacity-100 transition-opacity duration-300"
                    :style="{ backgroundColor: step.glowColor }"
                  />
                  <component
                    :is="step.icon"
                    :class="`w-10 h-10 ${step.iconColor} relative z-10`"
                  />
                </div>
              </div>

              <!-- Step number badge -->
              <div class="inline-flex items-center px-3 py-1 rounded-full bg-purple-500/10 border border-purple-400/20 mb-4">
                <span class="text-purple-300 font-display font-bold text-xs uppercase tracking-wider">
                  {{ t('landing.how_it_works.step_label') }} {{ step.number }}
                </span>
              </div>

              <!-- Title -->
              <h3 class="font-display font-semibold text-2xl lg:text-3xl mb-4 text-white group-hover:text-transparent group-hover:bg-clip-text group-hover:bg-gradient-to-r group-hover:from-purple-400 group-hover:to-blue-400 transition-all duration-300">
                {{ step.title }}
              </h3>

              <!-- Description -->
              <p class="text-gray-400 leading-relaxed group-hover:text-gray-300 transition-colors duration-300">
                {{ step.description }}
              </p>
            </div>

            <!-- Shine effect on hover -->
            <div class="absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-500 pointer-events-none">
              <div class="absolute inset-0 bg-gradient-to-r from-transparent via-white/5 to-transparent -skew-x-12 translate-x-[-200%] group-hover:translate-x-[200%] transition-transform duration-1000" />
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.step-card {
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  animation: fade-in-up 0.8s ease-out forwards;
  opacity: 0;
  position: relative;
  overflow: hidden;
}

.step-card::before {
  content: '';
  position: absolute;
  inset: 0;
  border-radius: 20px;
  padding: 1px;
  background: linear-gradient(135deg, rgba(139, 92, 246, 0.3), rgba(59, 130, 246, 0.3), rgba(236, 72, 153, 0.3));
  -webkit-mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0);
  -webkit-mask-composite: xor;
  mask-composite: exclude;
  opacity: 0;
  transition: opacity 0.4s ease;
}

.step-card:hover {
  background: rgba(255, 255, 255, 0.08);
  border-color: rgba(139, 92, 246, 0.4);
  transform: translateY(-8px);
  box-shadow: 
    0 20px 60px rgba(0, 0, 0, 0.5),
    0 0 40px rgba(139, 92, 246, 0.2),
    inset 0 1px 0 rgba(255, 255, 255, 0.1);
}

.step-card:hover::before {
  opacity: 1;
}

.step-icon-wrapper {
  position: relative;
}

.step-icon-wrapper::after {
  content: '';
  position: absolute;
  inset: -8px;
  border-radius: 20px;
  background: linear-gradient(135deg, rgba(139, 92, 246, 0.2), rgba(59, 130, 246, 0.2));
  opacity: 0;
  transition: opacity 0.4s ease;
  z-index: -1;
  filter: blur(12px);
}

.step-card:hover .step-icon-wrapper::after {
  opacity: 1;
}

.connector-line {
  animation: pulse-glow 2s ease-in-out infinite;
}

@keyframes fade-in-up {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes pulse-glow {
  0%, 100% {
    opacity: 0.3;
  }
  50% {
    opacity: 0.6;
  }
}

.bg-gradient-radial {
  background: radial-gradient(circle, var(--tw-gradient-stops));
}

.header-animate {
  animation: fade-in-up 0.8s ease-out forwards;
  opacity: 0;
}
</style>

