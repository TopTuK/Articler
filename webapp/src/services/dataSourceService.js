import api from './apiService'

export default function useDataSourceService() {
    const GET_DOCUMENTS_ACTION = "Document/GetProjectDocuments"
    const REMOVE_DOCUMENT_ACTION = "Document/RemoveDocument"
    const UPLOAD_TEXT_DATA_ACTION = "Document/AddTextDocument"
    const UPLOAD_PDF_DATA_ACTION = "Document/AddPdfDocument"

    const getDocuments = async (projectId) => {
        console.log('DataSourceService::getDocuments: Start get documents. ProjectId: ', projectId)

        try {
            const response = await api.get(GET_DOCUMENTS_ACTION, {
                params: {
                    projectId: projectId
                }
            })
            
            if (response.status === 200) {
                console.log('DataSourceService::getDocuments: Successfully retrieved documents')
                return response.data
            } else {
                console.error('DataSourceService::getDocuments: Error getting documents. Status:', response.status)
                throw new Error('DataSourceService::getDocuments: Error getting documents. Status:', response.status)
            }
        }
        catch (error) {
            console.error('DataSourceService::getDocuments: Exception: ', error)
            throw error
        }
    }

    const addTextDocument = async (projectId, title, text) => {
        console.log('DataSourceService::addTextDocument: Start add text document. ProjectId: ', projectId, ' Title: ', title, ' TextLength: ', text?.length || 0)

        // Validate that title and text are not empty
        if (!title || !title.trim()) {
            throw new Error('DataSourceService::addTextDocument: Title cannot be empty')
        }
        if (!text || !text.trim()) {
            throw new Error('DataSourceService::addTextDocument: Text cannot be empty')
        }

        try {
            const response = await api.post(UPLOAD_TEXT_DATA_ACTION, {
                ProjectId: projectId,
                Title: title.trim(),
                Text: text.trim()
            })
            
            if (response.status === 200) {
                console.log('DataSourceService::addTextDocument: Successfully added text document')
                return response.data
            } else {
                console.error('DataSourceService::addTextDocument: Error adding text document. Status:', response.status)
                throw new Error('DataSourceService::addTextDocument: Error adding text document. Status:', response.status)
            }
        }
        catch (error) {
            console.error('DataSourceService::addTextDocument: Exception: ', error)
            throw error
        }
    }

    const addPdfDocument = async (projectId, title, pdfUrl) => {
        console.log('DataSourceService::addPdfDocument: Start add pdf document. ProjectId: ', projectId, ' Title: ', title)

        // Validate that title and text are not empty
        if (!title || !title.trim()) {
            throw new Error('DataSourceService::addPdfDocument: Title cannot be empty')
        }
        if (!pdfUrl || !pdfUrl.trim()) {
            throw new Error('DataSourceService::addPdfDocument: Url cannot be empty')
        }

        try {
            const response = await api.post(UPLOAD_PDF_DATA_ACTION, {
                ProjectId: projectId,
                Title: title.trim(),
                PdfUrl: pdfUrl.trim()
            })
            
            if (response.status === 200) {
                console.log('DataSourceService::addPdfDocument: Successfully added pdf document')
                return response.data
            } else {
                console.error('DataSourceService::addPdfDocument: Error adding pdf document. Status:', response.status)
                throw new Error('DataSourceService::addPdfDocument: Error adding pdf document. Status:', response.status)
            }
        }
        catch (error) {
            console.error('DataSourceService::addPdfDocument: Exception: ', error)
            throw error
        }
    }

    const removeDocument = async (projectId, documentId) => {
        console.log('DataSourceService::removeDocument: Start remove document. ProjectId: ', projectId, ' DocumentId: ', documentId)

        // Validate that projectId and documentId are not empty
        if (!projectId || !projectId.trim()) {
            throw new Error('DataSourceService::removeDocument: ProjectId cannot be empty')
        }
        if (!documentId || !documentId.trim()) {
            throw new Error('DataSourceService::removeDocument: DocumentId cannot be empty')
        }

        try {
            const response = await api.post(REMOVE_DOCUMENT_ACTION, {
                ProjectId: projectId.trim(),
                DocumentId: documentId.trim()
            })
            
            if (response.status === 200) {
                console.log('DataSourceService::removeDocument: Successfully removed document')
                return response.data
            } else {
                console.error('DataSourceService::removeDocument: Error removing document. Status:', response.status)
                throw new Error('DataSourceService::removeDocument: Error removing document. Status:', response.status)
            }
        }
        catch (error) {
            console.error('DataSourceService::removeDocument: Exception: ', error)
            throw error
        }
    }

    return {
        getDocuments,
        addTextDocument, addPdfDocument,
        removeDocument,
    }
}

