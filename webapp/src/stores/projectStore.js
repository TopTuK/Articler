import { defineStore } from 'pinia'
import { ref } from 'vue' 
import api from '@/services/apiService'

export const useProjectStore = defineStore('project', () => {
    const GET_PROJECT_TEXT_ACTION = "Project/GetProjectText"
    const SAVE_PROJECT_TEXT_ACTION = "Project/UpdateProjectText"

    const projectText = ref('')

    const getProjectText = async (projectId) => {
        console.log('ProjectStore::getProjectText: Start get project text. ProjectId: ', projectId)

        try {
            const params = {    
                projectId: projectId
            }

            const response = await api.get(GET_PROJECT_TEXT_ACTION, { params: params })
            if (response.status === 200) {
                console.log('ProjectStore::getProjectText: Successfully got project text')
                // API returns an object with a 'text' property
                projectText.value = response.data?.text || response.data?.Text || ''
            } else {
                console.error('ProjectStore::getProjectText: Error getting project. Status:', response.status)
                throw new Error('ProjectStore::getProjectText: Error getting project. Status:', response.status)
            }
        }
        catch (error) {
            console.error('ProjectStore::getProjectText: Exception: ', error)
            throw error
        }
    }

    const saveProjectText = async (projectId) => {
        console.log('ProjectStore::updateProjectText: Start update project text. ProjectId: ', projectId)

        try {
            const response = await api.post(SAVE_PROJECT_TEXT_ACTION, {
                projectId: projectId,
                text: projectText.value || ''
            })
            
            if (response.status === 200) {
                console.log('ProjectStore::updateProjectText: Successfully updated project text')
                return response.data
            } else {
                console.error('ProjectStore::updateProjectText: Error updating project text. Status:', response.status)
                throw new Error('ProjectStore::updateProjectText: Error updating project text. Status:', response.status)
            }
        }
        catch (error) {
            console.error('ProjectStore::updateProjectText: Exception: ', error)
            throw error
        }
    }

    return {
        projectText,
        getProjectText, saveProjectText,
    }
})