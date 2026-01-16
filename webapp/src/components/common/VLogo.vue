<script setup>
defineProps({
    size: {
        type: [String, Number],
        default: 20,
    },
})
</script>

<template>
    <svg
        :width="size"
        :height="size"
        viewBox="0 0 24 24"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        class="text-primary-foreground"
    >
        <!-- Document base -->
        <path
            d="M14 2H6C5.46957 2 4.96086 2.21071 4.58579 2.58579C4.21071 2.96086 4 3.46957 4 4V20C4 20.5304 4.21071 21.0391 4.58579 21.4142C4.96086 21.7893 5.46957 22 6 22H18C18.5304 22 19.0391 21.7893 19.4142 21.4142C19.7893 21.0391 20 20.5304 20 20V8L14 2Z"
            stroke="currentColor"
            stroke-width="1.5"
            stroke-linecap="round"
            stroke-linejoin="round"
            fill="none"
        />
        <path
            d="M14 2V8H20"
            stroke="currentColor"
            stroke-width="1.5"
            stroke-linecap="round"
            stroke-linejoin="round"
            fill="none"
        />
        <!-- Text lines -->
        <path
            d="M8 12H16"
            stroke="currentColor"
            stroke-width="1.5"
            stroke-linecap="round"
        />
        <path
            d="M8 16H14"
            stroke="currentColor"
            stroke-width="1.5"
            stroke-linecap="round"
        />
        <!-- AI sparkles/circuit nodes -->
        <circle cx="18" cy="4" r="1.5" fill="currentColor" opacity="0.8" />
        <circle cx="20" cy="6" r="1" fill="currentColor" opacity="0.6" />
        <path
            d="M17.5 4.5L19.5 5.5"
            stroke="currentColor"
            stroke-width="0.8"
            opacity="0.5"
        />
    </svg>
</template>

