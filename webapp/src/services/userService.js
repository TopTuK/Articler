import api from './apiService'

export default function useUserService() {
    const GET_USER_ACTION = "User/GetUser"
    const UPDATE_USER_ACTION = "User/UpdateUser"

    // Map AccountType enum (0=None, 1=Free, 2=Trial, 3=Paid) to string
    const mapAccountType = (accountType) => {
        if (typeof accountType === 'string') {
            // Already a string, return as is
            return accountType
        }
        // Map enum number to string
        switch (accountType) {
            case 0:
                return 'None'
            case 1:
                return 'Free'
            case 2:
                return 'Trial'
            case 3:
                return 'Paid'
            default:
                return 'Free'
        }
    }

    const getUserProfile = async () => {
        console.log('UserService::getUserProfile: Start get user profile')

        try {
            const response = await api.get(GET_USER_ACTION)
            
            if (response.status === 200 && response.data) {
                const userData = response.data
                console.log('UserService::getUserProfile: Successfully got user profile', userData)
                
                // Map the response to match expected format
                return {
                    email: userData.email || '',
                    firstName: userData.firstName || '',
                    lastName: userData.lastName || '',
                    accountType: mapAccountType(userData.accountType),
                    createdDate: userData.createdDate ? new Date(userData.createdDate) : new Date(),
                }
            } else {
                console.error('UserService::getUserProfile: Invalid response', response)
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
        console.log('UserService::updateUserProfile: Start update user profile. firstName: ', firstName, ' lastName: ', lastName)

        try {
            const response = await api.post(UPDATE_USER_ACTION, {
                firstName: firstName,
                lastName: lastName,
            })
            
            if (response.status === 200 && response.data) {
                const userData = response.data
                console.log('UserService::updateUserProfile: Successfully updated user profile', userData)
                
                // Map the response to match expected format
                return {
                    email: userData.email || '',
                    firstName: userData.firstName || '',
                    lastName: userData.lastName || '',
                    accountType: mapAccountType(userData.accountType),
                    createdDate: userData.createdDate ? new Date(userData.createdDate) : new Date(),
                }
            } else {
                console.error('UserService::updateUserProfile: Invalid response', response)
                throw new Error('Invalid response from server')
            }
        }
        catch (error) {
            console.error('UserService::updateUserProfile: Exception: ', error)
            
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
        getUserProfile,
        updateUserProfile,
    }
}

