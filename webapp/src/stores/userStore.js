import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/apiService'

export const useUserStore = defineStore('userStore', () => {
    const GET_USER_ACTION = "User/GetUser"

    const userProfile = ref(null)
    const userTokenCount = ref(0)

    const getUserProfile = async () => {
        console.log('UserStore::getUserProfile: Start get user profile')

        try {
            const response = await api.get(GET_USER_ACTION)

            if (response.status === 200 && response.data) {
                const userData = response.data
                console.log('UserStore::getUserProfile: Successfully got user profile')
                
                // Map the response to match expected format
                const user = {
                    email: userData.email || '',
                    firstName: userData.firstName || '',
                    lastName: userData.lastName || '',
                    accountType: userData.accountType,
                    tokenCount: userData.tokenCount || 0,
                    createdDate: userData.createdDate ? new Date(userData.createdDate) : new Date(),
                }
                userProfile.value = userProfile
                userTokenCount.value = userData.tokenCount || 0

                return user
            } else {
                console.error('UserStore::getUserProfile: Invalid response', response)
                throw new Error('Invalid response from server')
            }
        }
        catch (error) {
            console.error('UserService::getUserProfile: Exception: ', error)
            
            // Handle specific error cases
            if (error.response) {
                // Server responded with error status
                if (error.response.status === 404) {
                    throw new Error('User profile not found')
                } else if (error.response.status === 400) {
                    throw new Error('Bad request. Please try again later.')
                } else if (error.response.status === 401) {
                    throw new Error('Unauthorized. Please log in again.')
                } else {
                    throw new Error(`Server error: ${error.response.status}`)
                }
            } else if (error.request) {
                // Request was made but no response received
                throw new Error('Network error. Please check your connection.')
            } else {
                // Something else happened
                throw new Error(error.message || 'Failed to load user profile')
            }
        }
    }

    const updateUserProfile = async (firstName, lastName) => {
        console.log('UserStore::updateUserProfile: Start update user profile. firstName: ', firstName, ' lastName: ', lastName)

        try {
            const response = await api.post(UPDATE_USER_ACTION, {
                firstName: firstName,
                lastName: lastName,
            })
            
            if (response.status === 200 && response.data) {
                const userData = response.data
                console.log('UserStore::updateUserProfile: Successfully updated user profile', userData)
                
                // Map the response to match expected format
                return {
                    email: userData.email || '',
                    firstName: userData.firstName || '',
                    lastName: userData.lastName || '',
                    accountType: mapAccountType(userData.accountType),
                    tokenCount: userData.tokenCount || 0,
                    createdDate: userData.createdDate ? new Date(userData.createdDate) : new Date(),
                }
            } else {
                console.error('UserService::updateUserProfile: Invalid response', response)
                throw new Error('Invalid response from server')
            }
        }
        catch (error) {
            console.error('UserStore::updateUserProfile: Exception: ', error)
            
            // Handle specific error cases
            if (error.response) {
                // Server responded with error status
                if (error.response.status === 404) {
                    throw new Error('User profile not found')
                } else if (error.response.status === 400) {
                    const errorMessage = error.response.data || 'Bad request. Please try again later.'
                    throw new Error(errorMessage)
                } else if (error.response.status === 401) {
                    throw new Error('Unauthorized. Please log in again.')
                } else {
                    throw new Error(`Server error: ${error.response.status}`)
                }
            } else if (error.request) {
                // Request was made but no response received
                throw new Error('Network error. Please check your connection.')
            } else {
                // Something else happened
                throw new Error(error.message || 'Failed to update user profile')
            }
        }
    }

    return {
        getUserProfile, updateUserProfile,
        userTokenCount,
    }
})