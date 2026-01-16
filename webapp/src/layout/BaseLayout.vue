<script setup>
import FooterLayout from './FooterLayout.vue';
</script>

<template>
    <div class="base-layout">
        <header class="header">
            <slot name="header">
                <!-- Header content -->
            </slot>
        </header>
        
        <main class="workspace">
            <slot>
                <!-- Workspace content -->
            </slot>
        </main>
        
        <footer>
            <footer-layout />
        </footer>
    </div>
</template>

<style scoped>
.base-layout {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
}

.workspace {
    flex: 1;
}
</style>