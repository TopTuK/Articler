<script setup>
import LoggedLayout from './layout/LoggedLayout.vue'
import UnauthorizedLayout from './layout/UnauthorizedLayout.vue'
import { useAuthStore } from './stores/authStore'

const authStore = useAuthStore()
</script>

<template>
    <LoggedLayout v-if="authStore.isAuthenticated" />
    <UnauthorizedLayout v-else />
</template>
