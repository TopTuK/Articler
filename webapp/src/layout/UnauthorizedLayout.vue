<script setup>
import HeaderView from '@/components/landing/HeaderView.vue';
import BaseLayout from './BaseLayout.vue'
</script>

<template>
    <BaseLayout>
        <template #header>
            <HeaderView />
        </template>
        
        <template #default>
            <router-view />
        </template>
    </BaseLayout>
</template>

