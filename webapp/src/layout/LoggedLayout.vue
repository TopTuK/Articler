<script setup>
import BaseLayout from './BaseLayout.vue'
import HeaderView from '@/components/logged/project/HeaderView.vue'
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

