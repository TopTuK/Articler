import api from './apiService'

export default function useChatService() {
    const SEND_CHAT_MESSAGE_ACTION = "Chat/SendChatMessage"
    const GET_CHAT_HISTORY_ACTION = "Chat/GetChatHistory"

    const sendChatMessage = async (projectId, message, mode = 0) => {
        console.log('ChatService::sendChatMessage: Start send chat message. ProjectId: ', projectId, ' MessageLength: ', message?.length || 0, ' Mode: ', mode)

        try {
            const response = await api.post(SEND_CHAT_MESSAGE_ACTION, {
                projectId: projectId,
                message: message || '',
                mode: mode || 0
            })
            
            if (response.status === 200) {
                console.log('ChatService::sendChatMessage: Successfully sent chat message')
                // Response structure: { result: number (0=Error, 1=Success, 2=NotEnoughTokens, 3=NotFound), response?: { role: number, content: string, text: string } }
                return response.data
            } else {
                console.error('ChatService::sendChatMessage: Error sending chat message. Status:', response.status)
                throw new Error('ChatService::sendChatMessage: Error sending chat message. Status:', response.status)
            }
        }
        catch (error) {
            // Handle HTTP status codes from the controller
            if (error.response) {
                const status = error.response.status
                if (status === 404) {
                    // NotFound - project not found
                    console.error('ChatService::sendChatMessage: Project not found (404)')
                    error.result = 3 // MessageResult.NotFound
                } else if (status === 400) {
                    // BadRequest - Error or NotEnoughTokens
                    console.error('ChatService::sendChatMessage: Bad request (400)')
                    error.result = 0 // MessageResult.Error (default, could be NotEnoughTokens=2)
                }
            }
            console.error('ChatService::sendChatMessage: Exception: ', error)
            throw error
        }
    }

    const getChatHistory = async (projectId) => {
        console.log('ChatService::getChatHistory: Start get chat history. ProjectId: ', projectId)

        try {
            const response = await api.get(GET_CHAT_HISTORY_ACTION, {
                params: {
                    projectId: projectId
                }
            })
            
            if (response.status === 200) {
                console.log('ChatService::getChatHistory: Successfully retrieved chat history')
                // Backend returns an array of IChatMessage: [{role: number, content: string}, ...]
                return response.data
            } else {
                console.error('ChatService::getChatHistory: Error retrieving chat history. Status:', response.status)
                throw new Error(`ChatService::getChatHistory: Error retrieving chat history. Status: ${response.status}`)
            }
        }
        catch (error) {
            // Handle 404 (history not found) as empty array instead of error
            if (error.response && error.response.status === 404) {
                console.log('ChatService::getChatHistory: Chat history not found, returning empty array')
                return []
            }
            console.error('ChatService::getChatHistory: Exception: ', error)
            throw error
        }
    }

    return {
        sendChatMessage,
        getChatHistory,
    }
}

