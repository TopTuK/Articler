import api from './apiService'
import { useRouter } from 'vue-router'

export default function useProjectService() {
    const CREATE_PROJECT_ACTION = "Project/Create"
    const GET_USER_PROJECTS_ACTION = "Project/GetProjects"
    const GET_USER_PROJECT_ACTION = "Project/GetProject"
    const UPDATE_PROJECT_ACTION = "Project/UpdateProject"
    const DELETE_PROJECT_ACTION = "Project/DeleteProject"

    const router = useRouter()

    const createProject = async (title, description) => {
        console.log('ProjectService::createProject: Start create project. title: ', title, ' description: ', description)

        try {
            const response = await api.post(CREATE_PROJECT_ACTION, {
                title: title,
                description: description || ''
            })
            
            const project = response.data
            console.log('ProjectService::createProject: Project created successfully. ProjectId: ', project.id)
            
            // Redirect to project page on success
            router.push({ name: 'Project', params: { id: project.id } })
            
            return project
        }
        catch (error) {
            console.error('ProjectService::createProject: Exception: ', error)
            throw error
        }
    }

    const getUserProjects = async () => {
        console.log('ProjectService::getUserProjects: Start get user projects')

        try {
            const response = await api.get(GET_USER_PROJECTS_ACTION)
            return response.data
        }
        catch (error) {
            console.error('ProjectService::getUserProjects: Exception: ', error)
            throw error
        }
    }

    const getProject = async (projectId) => {
        console.log('ProjectService::getProject: Start get project. ProjectId: ', projectId)

        try {
            const params = {    
                projectId: projectId
            }

            const response = await api.get(GET_USER_PROJECT_ACTION, { params: params })
            if (response.status === 200) {
                console.log('ProjectService::getProject: Successfully got project')
                return response.data
            } else {
                console.error('ProjectService::getProject: Error getting project. Status:', response.status)
                throw new Error('ProjectService::getProject: Error getting project. Status:', response.status)
            }
        }
        catch (error) {
            console.error('ProjectService::getProject: Exception: ', error)
            throw error
        }
    }

    const updateProject = async (projectId, title, description) => {
        console.log('ProjectService::updateProject: Start update project. id: ', projectId, ' title: ', title, ' description: ', description)

        try {
            const response = await api.put(UPDATE_PROJECT_ACTION, {
                id: projectId,
                title: title,
                description: description,
            })
            return response.data
        }
        catch (error) {
            console.error('ProjectService::updateProject: Exception: ', error)
            throw error
        }
    }

    const deleteProject = async (projectId) => {
        console.log('ProjectService::deleteProject: Start delete project. ProjectId: ', projectId)

        try {
            const response = await api.post(DELETE_PROJECT_ACTION, {
                projectId: projectId
            })
            
            if (response.status === 200) {
                console.log('ProjectService::deleteProject: Successfully deleted project. ProjectId: ', projectId)
                return response.data
            } else {
                console.error('ProjectService::deleteProject: Error deleting project. Status:', response.status)
                throw new Error('ProjectService::deleteProject: Error deleting project. Status:', response.status)
            }
        }
        catch (error) {
            console.error('ProjectService::deleteProject: Exception: ', error)
            throw error
        }
    } 

    return {
        createProject,
        getUserProjects,
        getProject,
        updateProject,
        deleteProject,
    }
}