<script setup>
import { ref, computed, onBeforeMount } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { ArrowLeft, User, Mail, Calendar, Crown, Edit, Save, X, Coins } from 'lucide-vue-next'
import useUserService from '@/services/userService'

const router = useRouter()
const { t } = useI18n()
const userService = useUserService()

const profile = ref({
  email: '',
  firstName: '',
  lastName: '',
  accountType: 'Free',
  tokenCount: 0,
  createdDate: null,
})

const isEditing = ref(false)
const isLoading = ref(true)
const isSaving = ref(false)
const error = ref(null)

const editFirstName = ref('')
const editLastName = ref('')

const initials = computed(() => {
  if (!profile.value.firstName || !profile.value.lastName) return '??'
  return `${profile.value.firstName[0]}${profile.value.lastName[0]}`.toUpperCase()
})

const getAccountBadgeVariant = (type) => {
  switch (type) {
    case 'Paid':
      return 'bg-purple-600 text-white border-purple-500'
    case 'Trial':
      return 'bg-gray-800 text-gray-300 border-gray-700'
    case 'Free':
      return 'border border-gray-700 bg-transparent text-gray-300'
    default:
      return 'border border-gray-700 bg-transparent text-gray-300'
  }
}

const getAccountIcon = (type) => {
  return type === 'Paid' ? Crown : null
}

const getAccountTypeLabel = (type) => {
  const typeKey = type?.toLowerCase() || 'free'
  return t(`profile_view.account_types.${typeKey}`) || type
}

const formattedDate = computed(() => {
  if (!profile.value.createdDate) return ''
  const date = new Date(profile.value.createdDate)
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
})

const loadProfile = async () => {
  isLoading.value = true
  error.value = null
  try {
    const userProfile = await userService.getUserProfile()
    profile.value = {
      email: userProfile.email,
      firstName: userProfile.firstName,
      lastName: userProfile.lastName,
      accountType: userProfile.accountType,
      tokenCount: userProfile.tokenCount,
      createdDate: userProfile.createdDate,
    }
  } catch (err) {
    console.error('ProfileView::loadProfile: Exception: ', err)
    error.value = err.message || err.toString() || t('profile_view.errors.failed_to_load')
  } finally {
    isLoading.value = false
  }
}

const startEditing = () => {
  editFirstName.value = profile.value.firstName
  editLastName.value = profile.value.lastName
  isEditing.value = true
}

const cancelEditing = () => {
  editFirstName.value = ''
  editLastName.value = ''
  isEditing.value = false
  error.value = null
}

const saveProfile = async () => {
  if (!editFirstName.value.trim()) {
    error.value = t('profile_view.errors.first_name_required')
    return
  }

  isSaving.value = true
  error.value = null
  try {
    const updatedProfile = await userService.updateUserProfile(
      editFirstName.value.trim(),
      editLastName.value.trim()
    )
    profile.value = {
      email: updatedProfile.email,
      firstName: updatedProfile.firstName,
      lastName: updatedProfile.lastName,
      accountType: updatedProfile.accountType,
      tokenCount: updatedProfile.tokenCount,
      createdDate: updatedProfile.createdDate,
    }
    isEditing.value = false
  } catch (err) {
    console.error('ProfileView::saveProfile: Exception: ', err)
    error.value = err.message || t('profile_view.errors.failed_to_update')
  } finally {
    isSaving.value = false
  }
}

const goBack = () => {
  router.push({ name: 'Projects' })
}

onBeforeMount(async () => {
  await loadProfile()
})
</script>

<template>
  <div class="min-h-screen bg-black pt-20">
    <!-- Page Header with Back Button and Edit Button -->
    <div class="container mx-auto px-4 py-6 max-w-2xl">
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center gap-4">
          <button
            @click="goBack"
            class="inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:text-white hover:bg-gray-800 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500"
          >
            <ArrowLeft class="h-5 w-5" />
          </button>
          <h1 class="text-xl font-semibold text-white">{{ t('profile_view.title') }}</h1>
        </div>
        <button
          v-if="!isEditing && !isLoading && profile.email"
          @click="startEditing"
          class="inline-flex items-center justify-center gap-2 rounded-md border border-gray-700 bg-transparent px-4 py-2 text-sm font-medium text-gray-300 hover:bg-gray-800 hover:text-white transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500"
        >
          <Edit class="h-4 w-4" />
          {{ t('profile_view.actions.edit_profile') }}
        </button>
      </div>
    </div>

    <main class="container mx-auto px-4 pb-8 max-w-2xl">
      <!-- Loading State -->
      <div v-if="isLoading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-8 w-8 border-4 border-purple-500 border-t-transparent mb-4"></div>
        <p class="text-gray-400">{{ t('profile_view.loading.title') }}</p>
        <p class="text-gray-500 text-sm mt-2">{{ t('profile_view.loading.subtitle') }}</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error && !isEditing" class="text-center py-12">
        <div class="max-w-md mx-auto p-4 rounded-lg bg-red-500/10 border border-red-500/20">
          <p class="text-red-400 mb-4">{{ error }}</p>
          <button
            @click="loadProfile"
            :disabled="isLoading"
            class="inline-flex items-center justify-center gap-2 rounded-md bg-purple-600 px-4 py-2 text-sm font-medium text-white hover:bg-purple-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ isLoading ? t('profile_view.loading.title') : t('profile_view.actions.retry') }}
          </button>
        </div>
      </div>

      <!-- Profile Card -->
      <div
        v-else
        class="rounded-lg border border-gray-800 bg-gray-900/50 backdrop-blur-sm shadow-lg"
      >
        <div class="text-center pb-2 pt-8 px-6">
          <div class="flex justify-center mb-4">
            <div class="h-24 w-24 rounded-full border-4 border-purple-500/20 bg-gradient-to-br from-purple-500/20 to-blue-500/20 flex items-center justify-center">
              <span class="text-2xl font-bold text-purple-300">
                {{ initials }}
              </span>
            </div>
          </div>
          <h2 class="text-2xl font-semibold text-white mb-2">
            {{ profile.firstName }} {{ profile.lastName }}
          </h2>
          <div class="flex justify-center mt-2">
            <span
              :class="[
                'inline-flex items-center rounded-full border px-3 py-1 text-xs font-semibold',
                getAccountBadgeVariant(profile.accountType)
              ]"
            >
              <component
                v-if="getAccountIcon(profile.accountType)"
                :is="getAccountIcon(profile.accountType)"
                class="h-3 w-3 mr-1"
              />
              {{ getAccountTypeLabel(profile.accountType) }} {{ t('profile_view.account_types.account') }}
            </span>
          </div>
        </div>

        <div class="space-y-4 pt-6 pb-6 px-6">
          <!-- Error Message -->
          <div v-if="error && isEditing" class="p-3 rounded-md bg-red-500/10 border border-red-500/20">
            <p class="text-sm text-red-400">{{ error }}</p>
          </div>

          <!-- Profile Fields -->
          <div class="space-y-4">
            <div class="flex items-center gap-4 p-4 rounded-lg bg-gray-800/50">
              <div class="h-10 w-10 rounded-full bg-purple-500/10 flex items-center justify-center shrink-0">
                <User class="h-5 w-5 text-purple-400" />
              </div>
              <div class="flex-1">
                <p class="text-sm text-gray-400">{{ t('profile_view.fields.full_name') }}</p>
                <template v-if="isEditing">
                  <div class="space-y-2 mt-1">
                    <input
                      v-model="editFirstName"
                      type="text"
                      :placeholder="t('profile_view.fields.first_name_placeholder')"
                      class="w-full px-3 py-2 rounded-md border border-gray-700 bg-gray-800 text-white placeholder:text-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500"
                    />
                    <input
                      v-model="editLastName"
                      type="text"
                      :placeholder="t('profile_view.fields.last_name_placeholder')"
                      class="w-full px-3 py-2 rounded-md border border-gray-700 bg-gray-800 text-white placeholder:text-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500"
                    />
                  </div>
                </template>
                <p v-else class="font-medium text-white">
                  {{ profile.firstName }} {{ profile.lastName }}
                </p>
              </div>
            </div>

            <div class="flex items-center gap-4 p-4 rounded-lg bg-gray-800/50">
              <div class="h-10 w-10 rounded-full bg-purple-500/10 flex items-center justify-center shrink-0">
                <Mail class="h-5 w-5 text-purple-400" />
              </div>
              <div class="flex-1">
                <p class="text-sm text-gray-400">{{ t('profile_view.fields.email_address') }}</p>
                <p class="font-medium text-white">{{ profile.email }}</p>
              </div>
            </div>

            <div class="flex items-center gap-4 p-4 rounded-lg bg-gray-800/50">
              <div class="h-10 w-10 rounded-full bg-purple-500/10 flex items-center justify-center shrink-0">
                <Crown class="h-5 w-5 text-purple-400" />
              </div>
              <div class="flex-1">
                <p class="text-sm text-gray-400">{{ t('profile_view.fields.account_type') }}</p>
                <p class="font-medium text-white">{{ getAccountTypeLabel(profile.accountType) }}</p>
              </div>
            </div>

            <div class="flex items-center gap-4 p-4 rounded-lg bg-gray-800/50">
              <div class="h-10 w-10 rounded-full bg-purple-500/10 flex items-center justify-center shrink-0">
                <Calendar class="h-5 w-5 text-purple-400" />
              </div>
              <div class="flex-1">
                <p class="text-sm text-gray-400">{{ t('profile_view.fields.member_since') }}</p>
                <p class="font-medium text-white">{{ formattedDate }}</p>
              </div>
            </div>

            <div class="flex items-center gap-4 p-4 rounded-lg bg-gray-800/50">
              <div class="h-10 w-10 rounded-full bg-purple-500/10 flex items-center justify-center shrink-0">
                <Coins class="h-5 w-5 text-purple-400" />
              </div>
              <div class="flex-1">
                <p class="text-sm text-gray-400">{{ t('profile_view.fields.token_count') }}</p>
                <p class="font-medium text-white">{{ profile.tokenCount.toLocaleString() }}</p>
              </div>
            </div>
          </div>

          <!-- Edit Actions -->
          <div v-if="isEditing" class="flex items-center justify-end gap-3 pt-4 border-t border-gray-800">
            <button
              @click="cancelEditing"
              :disabled="isSaving"
              class="inline-flex items-center justify-center gap-2 rounded-md border border-gray-700 bg-transparent px-4 py-2 text-sm font-medium text-gray-300 hover:bg-gray-800 hover:text-white transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <X class="h-4 w-4" />
              {{ t('profile_view.actions.cancel') }}
            </button>
            <button
              @click="saveProfile"
              :disabled="isSaving || !editFirstName.trim()"
              class="inline-flex items-center justify-center gap-2 rounded-md bg-purple-600 px-4 py-2 text-sm font-medium text-white hover:bg-purple-700 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <Save class="h-4 w-4" />
              {{ isSaving ? t('profile_view.actions.saving') : t('profile_view.actions.save') }}
            </button>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

