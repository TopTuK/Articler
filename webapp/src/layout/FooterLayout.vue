<template>
    <footer class="relative bg-black border-t border-gray-800 overflow-hidden">
        <!-- Gradient border effect -->
        <div class="absolute top-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-purple-500/50 to-transparent"></div>
        
        <!-- Background glow effect -->
        <div class="absolute inset-0 bg-gradient-to-t from-purple-500/5 via-transparent to-transparent"></div>
        
        <div class="container mx-auto px-6 py-8 relative z-10">
            <div class="flex flex-col sm:flex-row items-center justify-between gap-4">
                <p class="text-gray-300 text-sm font-medium">
                    <span class="text-white">{{ $t('common.author_name') }}</span>
                    <span class="mx-2 text-gray-500">&copy;</span>
                    <span class="text-gray-400">{{ $t('common.created_date') }}</span>
                </p>
                
                <!-- Decorative element -->
                <div class="flex items-center gap-2">
                    <div class="w-1.5 h-1.5 rounded-full bg-purple-400 animate-pulse"></div>
                    <div class="w-1 h-1 rounded-full bg-blue-400 opacity-60"></div>
                    <div class="w-0.5 h-0.5 rounded-full bg-pink-400 opacity-40"></div>
                </div>
            </div>
        </div>
    </footer>
</template>

<style scoped>
@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}
</style>
