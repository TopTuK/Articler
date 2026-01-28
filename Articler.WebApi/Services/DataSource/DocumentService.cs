using Articler.AppDomain.Models.Documents;
using Articler.AppDomain.Models.Token;
using Articler.GrainInterfaces.Project;
using Articler.GrainInterfaces.User;
using Articler.WebApi.Models.TokenResult;

namespace Articler.WebApi.Services.DataSource
{
    public class DocumentService(ILogger<DocumentService> logger, IClusterClient clusterClient)
        : IDocumentService
    {
        private readonly ILogger<DocumentService> _logger = logger;
        private readonly IClusterClient _clusterClient = clusterClient;

        public async Task<ICalculateTokenResult<IDocument>> AddProjectPdfDocumentAsync(
            string userId, string projectId, string title, string url)
        {
            _logger.LogInformation("DocumentService::AddProjectPdfDocumentAsync: start add project PDF document. " +
                "UserId={userId} ProjectId={projectId} Title={title} URL={pdfUrl}",
                userId, projectId, title, url);

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(url))
            {
                _logger.LogError("DocumentService::AddProjectPdfDocumentAsync: title and url can\'t be empty. " +
                    "UserId={userId} ProjectId={projectId} Title={title} URL={pdfUrl}",
                    userId, projectId, title, url);
                return TokenResultFactory.CreateTokenResult<IDocument>(CalculateTokenStatus.InternalError);
            }

            try
            {
                var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
                var project = await userGrain.GetProjectById(projectId);

                if (project == null)
                {
                    _logger.LogError("DocumentService::AddProjectPdfDocumentAsync: can\'t get project. " +
                        "UserId={userId} ProjectId={projectId}", userId, projectId);
                    return TokenResultFactory.CreateTokenResult<IDocument>(CalculateTokenStatus.InternalError);
                }

                _logger.LogInformation("DocumentService::AddProjectPdfDocumentAsync: got user project. " +
                    "ProjectId={projectId} ProjectTitle={projectTitle}", project.Id, project.Title);

                var projectGrain = _clusterClient.GetGrain<IProjectGrain>(project.Id, userId);
                var documentTokenResult = await projectGrain.AddPdfDocument(title, url);

                if (documentTokenResult.Status != CalculateTokenStatus.Success)
                {
                    _logger.LogWarning("DocumentService::AddProjectPdfDocumentAsync: calculate token status is not success. " +
                        "UserId={userId}, ProjectId={projectId}, CalculateTokenStatus={tokenStatus}",
                        userId, projectId, documentTokenResult.Status);
                    return TokenResultFactory.CreateTokenResult<IDocument>(documentTokenResult.Status);
                }
                else if (documentTokenResult.Result == null)
                {
                    _logger.LogError("DocumentService::AddProjectPdfDocumentAsync: calculate token status but doocument is null. " +
                        "UserId={userId}, ProjectId={projectId}, CalculateTokenStatus={tokenStatus}",
                        userId, projectId, documentTokenResult.Status);
                    return TokenResultFactory.CreateTokenResult<IDocument>(CalculateTokenStatus.InternalError);
                }
                
                var document = documentTokenResult.Result;
                _logger.LogInformation("DocumentService::AddProjectPdfDocumentAsync: added PDF document. " +
                    "UserId={userId} ProjectId={projectId} DocumentId={documentId} DocumentTitle={documentTitle}",
                    userId, projectId, document.Id, document.Title);
                return documentTokenResult;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("DocumentService::AddProjectPdfDocumentAsync: exception raised." +
                    "UserId={userId} ProjectId={projectId} Message: {exMessage}", userId, projectId, ex.Message);
                return TokenResultFactory.CreateTokenResult<IDocument>(CalculateTokenStatus.ExceptionRaised);
            }
        }

        public async Task<ICalculateTokenResult<IDocument>> AddProjectTextDocumentAsync(string userId, string projectId, string title, string text)
        {
            _logger.LogInformation("DocumentService::AddProjectTextDocumentAsync: start add project text document. " +
                "UserId={userId} ProjectId={projectId} Title={title} TextLength={textLength}",
                userId, projectId, title, text.Length);

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(text))
            {
                _logger.LogError("DocumentService::AddProjectTextDocumentAsync: title and text can\'t be empty. " +
                    "UserId={userId} ProjectId={projectId} Title={title} TextLength={textLength}",
                    userId, projectId, title, text.Length);
                return TokenResultFactory.CreateTokenResult<IDocument>(CalculateTokenStatus.InternalError);
            }

            try
            {
                var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
                var project = await userGrain.GetProjectById(projectId);

                if (project == null)
                {
                    _logger.LogError("DocumentService::AddProjectTextDocumentAsync: can\'t get project. " +
                        "UserId={userId} ProjectId={projectId}", userId, projectId);
                    return TokenResultFactory.CreateTokenResult<IDocument>(CalculateTokenStatus.InternalError);
                }

                _logger.LogInformation("DocumentService::AddProjectTextDocumentAsync: got user project. " +
                    "ProjectId={projectId} ProjectTitle={projectTitle}",
                    project.Id, project.Title);

                var projectGrain = _clusterClient.GetGrain<IProjectGrain>(project.Id, userId);
                var documentTokenResult = await projectGrain.AddTextDocument(title, text);

                if (documentTokenResult.Status != CalculateTokenStatus.Success)
                {
                    _logger.LogWarning("DocumentService::AddProjectTextDocumentAsync: calculate token status is not success. " +
                        "UserId={userId}, ProjectId={projectId}, CalculateTokenStatus={tokenStatus}",
                        userId, projectId, documentTokenResult.Status);
                    return TokenResultFactory.CreateTokenResult<IDocument>(documentTokenResult.Status);
                }
                else if (documentTokenResult.Result == null)
                {
                    _logger.LogError("DocumentService::AddProjectTextDocumentAsync: calculate token status but doocument is null. " +
                        "UserId={userId}, ProjectId={projectId}, CalculateTokenStatus={tokenStatus}",
                        userId, projectId, documentTokenResult.Status);
                    return TokenResultFactory.CreateTokenResult<IDocument>(CalculateTokenStatus.InternalError);
                }

                var document = documentTokenResult.Result;
                _logger.LogInformation("DocumentService::AddProjectTextDocumentAsync: added text document. " +
                    "UserId={userId} ProjectId={projectId} DocumentId={documentId} DocumentTitle={documentTitle}",
                    userId, projectId, document.Id, document.Title);
                return documentTokenResult;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("DocumentService::AddProjectTextDocumentAsync: exception raised." +
                    "UserId={userId} ProjectId={projectId} Message: {exMessage}", userId, projectId, ex.Message);
                return TokenResultFactory.CreateTokenResult<IDocument>(CalculateTokenStatus.ExceptionRaised);
            }
        }

        public async Task<IEnumerable<IDocument>?> GetProjectDocumentsAsync(string userId, string projectId)
        {
            _logger.LogInformation("DocumentService::GetProjectDocumentsAsync: start get project documents. " +
                "UserId={userId} ProjectId={projectId}", userId, projectId);

            try
            {
                var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
                var project = await userGrain.GetProjectById(projectId);

                if (project == null)
                {
                    _logger.LogError("DocumentService::GetProjectDocumentsAsync: can\'t get project. " +
                        "UserId={userId} ProjectId={projectId}", userId, projectId);
                    return null;
                }

                _logger.LogInformation("DocumentService::GetProjectDocumentsAsync: got user project. " +
                    "ProjectId={projectId} ProjectTitle={projectTitle}",
                    project.Id, project.Title);

                var projectGrain = _clusterClient.GetGrain<IProjectGrain>(project.Id, userId);
                var documents = await projectGrain.GetDocuments();

                _logger.LogInformation("DocumentService::GetProjectDocumentsAsync: return project documents. " +
                    "UserId={userId} ProjectId={projectId} DocumentsCount={documentsCount}",
                    userId, projectId, documents.Count());
                return documents;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("DocumentService::GetProjectDocumentsAsync: exception raised." +
                    "UserId={userId} ProjectId={projectId} Message: {exMessage}", userId, projectId, ex.Message);
                throw;
            }
        }

        public async Task<IDocument?> RemoveProjectDocumentAsync(string userId, 
            string projectId, string documentId)
        {
            _logger.LogInformation("DocumentService::RemoveProjectDocumentAsync: start remove project document. " +
                "UserId={userId} ProjectId={projectId} DocumentId={documentId}", userId, projectId, documentId);

            if(!Guid.TryParse(documentId, out var docId))
            {
                _logger.LogError("DocumentService::RemoveProjectDocumentAsync: can\'t convert document guid. " +
                    "UserId={userId} ProjectId={projectId} DocumentId={documentId}", userId, projectId, documentId);
                return null;
            }

            try
            {
                var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
                var project = await userGrain.GetProjectById(projectId);

                if (project == null)
                {
                    _logger.LogError("DocumentService::AddProjectTextDocumentAsync: can\'t get project. " +
                        "UserId={userId} ProjectId={projectId}", userId, projectId);
                    return null;
                }

                _logger.LogInformation("DocumentService::GetProjectDocumentsAsync: got user project. " +
                    "ProjectId={projectId} ProjectTitle={projectTitle}", project.Id, project.Title);

                var projectGrain = _clusterClient.GetGrain<IProjectGrain>(project.Id, userId);
                var document = await projectGrain.RemoveDocument(docId);

                _logger.LogInformation("DocumentService::GetProjectDocumentsAsync: remove document result. " +
                    "UserId={userId} ProjectId={projectId} DocumentId={documentId}. Result={removeResult} ", 
                    userId, projectId, documentId, (document != null));
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("DocumentService::RemoveProjectDocumentAsync: exception raised." +
                    "UserId={userId} ProjectId={projectId} DocumentId={documentId} Message: {exMessage}", 
                    userId, projectId, documentId, ex.Message);
                throw;
            }
        }
    }
}
