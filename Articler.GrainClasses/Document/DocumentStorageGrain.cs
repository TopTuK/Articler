using Articler.AppDomain.Factories.Token;
using Articler.AppDomain.Models.Documents;
using Articler.AppDomain.Models.Token;
using Articler.AppDomain.Services.Document;
using Articler.AppDomain.Services.VectorStorage;
using Articler.GrainInterfaces.Document;
using Articler.GrainInterfaces.Project;
using Articler.GrainInterfaces.User;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Articler.GrainClasses.Document
{
    public class DocumentStorageGrain(
        ILogger<DocumentStorageGrain> logger,
        IVectorStorageService vectorStorageService,
        IPdfDocumentService pdfDocumentService)
        : Grain, IDocumentStorageGrain
    {
        private readonly ILogger<DocumentStorageGrain> _logger = logger;
        private readonly IVectorStorageService _vectorStorageService = vectorStorageService;
        private readonly IPdfDocumentService _pdfDocumentService = pdfDocumentService;

        public async Task<IEnumerable<IDocument>> GetDocuments()
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("DocumentStorageGrain::GetDocuments: start get project documents. " +
                "GrainId={grainId}, UserId={userId}", grainId, userId);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogCritical("DocumentStorageGrain::GetDocuments: userId is null or empty." +
                    "GrainId={grainId}", grainId);
                throw new Exception("UserId is null or empty.");
            }

            var projectGrain = GrainFactory.GetGrain<IProjectGrain>(grainId, userId);
            var documents = await projectGrain.GetDocuments();

            _logger.LogInformation("DocumentStorageGrain::GetDocuments: return project documents. " +
                "GrainId={grainId} UserId={userId} DocumentsCount={documentsCount}",
                grainId, userId, documents.Count());
            return documents;
        }

        public async Task<ICalculateTokenResult<IDocument>> AddTextDocument(string title, string text)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("DocumentStorageGrain::AddTextDocument: start add user text document. " +
                "GrainId={grainId} UserId={userId}, Title={title}, TextLength={textLength}",
                grainId, userId, title, text.Length);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogCritical("DocumentStorageGrain::AddTextDocument: userId is null or empty." +
                    "GrainId={grainId}", grainId);
                throw new Exception("UserId is null or empty.");
            }

            try
            {
                var userGrain = GrainFactory.GetGrain<IUserGrain>(userId);
                var calculateTokenResult = await userGrain.CalculateEmbeddingsTokens(text);

                if (calculateTokenResult.Status != CalculateTokenStatus.Success)
                {
                    _logger.LogWarning("DocumentStorageGrain::AddTextDocument: user don\'t have enough tokens. " +
                        "GrainId={grainId}, UserId={userId}, TextLength={textLength} MaxToken={maxToken}",
                        grainId, userId, text.Length, calculateTokenResult.MaxTokenCount);
                    return CalculateTokenResultFactory<IDocument>.CreateTokenResult(calculateTokenResult.Status);
                }

                var documentId = Guid.NewGuid();
                _logger.LogInformation("DocumentStorageGrain::AddTextDocument: created id of new document. " +
                    "GrainId={grainId} UserId={userId} DocumentId={documentId}", grainId, userId, documentId);
                var document = await _vectorStorageService.StoreTextAsync(userId, grainId, documentId, title, text);

                _logger.LogInformation("DocumentStorageGrain::AddTextDocument: store document to vector storage. " +
                    "GrainId={grainId} UserId={userId} DocumentId={documentId} DocumentTitle={documentTitle}",
                    grainId, userId, document.Id, document.Title);
                
                var projectGrain = GrainFactory.GetGrain<IProjectGrain>(grainId, userId);
                document = await projectGrain.AddDocument(document);

                _logger.LogInformation("DocumentStorageGrain::AddTextDocument: successfully store document to vector db. " +
                    "GrainId={grainId} UserId={userId} DocumentId={documentId} DocumentType={documentType} DocumentTitle={documentTitle}",
                    grainId, userId, document.Id, document.DocumentType, document.Title);
                return CalculateTokenResultFactory<IDocument>.CreateTokenResult(calculateTokenResult.Status, document);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("DocumentStorageGrain::AddTextDocument: exception raised. " +
                    "GrainId={grainId} UserId={userId}. Message: {exMessage}", grainId, userId, ex.Message);
                throw;
            }
        }

        public async Task<ICalculateTokenResult<IDocument>> AddPdfDocument(string title, string url)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("DocumentStorageGrain::AddPdfDocument: start add user PDF document. " +
                "GrainId={grainId} UserId={userId}, Title={title}, URL={pdfUrl}",
                grainId, userId, title, url);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogCritical("DocumentStorageGrain::AddPdfDocument: userId is null or empty." +
                    "GrainId={grainId}", grainId);
                throw new Exception("UserId is null or empty.");
            }

            try
            {
                // First we should download and parse PDF
                _logger.LogInformation("DocumentStorageGrain::AddPdfDocument: start downloading pdf and parsing text. " +
                    "GrainId={grainId} UserId={userId}, URL={pdfUrl}", grainId, userId, url);
                var text = await _pdfDocumentService.DownloadAndParsePdfDocumentAsync(url);

                var userGrain = GrainFactory.GetGrain<IUserGrain>(userId);
                var calculateTokenResult = await userGrain.CalculateEmbeddingsTokens(text);

                if (calculateTokenResult.Status != CalculateTokenStatus.Success)
                {
                    _logger.LogWarning("DocumentStorageGrain::AddPdfDocument: user don\'t have enough tokens. " +
                        "GrainId={grainId}, UserId={userId}, TextLength={textLength}, MaxToken={maxToken}",
                        grainId, userId, text.Length, calculateTokenResult.MaxTokenCount);
                    return CalculateTokenResultFactory<IDocument>.CreateTokenResult(calculateTokenResult.Status);
                }

                _logger.LogInformation("DocumentStorageGrain::AddPdfDocument: parsed PDF document from URL. " +
                    "GrainId={grainId}, UserId={userId}, URL={pdfUrl}, TextLengt={textLength}",
                    grainId, userId, url, text.Length);

                // Save document to vector DB
                var documentId = Guid.NewGuid();
                _logger.LogInformation("DocumentStorageGrain::AddPdfDocument: created id of new document. " +
                    "GrainId={grainId} UserId={userId} DocumentId={documentId}", grainId, userId, documentId);
                var document = await _vectorStorageService.StoreTextAsync(userId, grainId, documentId, title, text);

                _logger.LogInformation("DocumentStorageGrain::AddTextDocument: store document to vector storage. " +
                    "GrainId={grainId} UserId={userId} DocumentId={documentId} DocumentTitle={documentTitle}",
                    grainId, userId, document.Id, document.Title);

                var projectGrain = GrainFactory.GetGrain<IProjectGrain>(grainId, userId);
                document = await projectGrain.AddDocument(document);

                _logger.LogInformation("DocumentStorageGrain::AddPdfDocument: successfully store document to vector db. " +
                    "GrainId={grainId} UserId={userId} DocumentId={documentId} DocumentType={documentType} DocumentTitile={documentTitle}",
                    grainId, userId, document.Id, document.DocumentType, document.Title);
                return CalculateTokenResultFactory<IDocument>.CreateTokenResult(calculateTokenResult.Status, document);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("DocumentStorageGrain::AddPdfDocument: exception raised. " +
                    "GrainId={grainId} UserId={userId}. Message: {exMessage}", grainId, userId, ex.Message);
                throw;
            }
        }

        private async Task<IDocument> RemoveDocumentAsync(IDocument document, Guid grainId, string userId)
        {
            _logger.LogInformation("DocumentStorageGrain::RemoveDocument: start remove document. " +
                "GrainId={grainId} UserId={userId}, DocumentId={documentId}, DocumentTitle={documentTitle}",
                grainId, userId, document.Id, document.Title);

            try
            {
                var removedDocument = await _vectorStorageService.RemoveDocumentAsync(userId, grainId, document.Id);
                if (removedDocument == null)
                {
                    _logger.LogWarning("DocumentStorageGrain::RemoveDocument: can\'t find document to remove. " +
                        "GrainId={grainId} UserId={userId}, DocumentId={documentId}, DocumentTitle={documentTitle}",
                        grainId, userId, document.Id, document.Title);
                    return document;
                }

                _logger.LogInformation("DocumentStorageGrain::RemoveDocument: removed document from vector db. " +
                    "GrainId={grainId} UserId={userId}, DocumentId={documentId}, DocumentTitle={documentTitle}",
                    grainId, userId, removedDocument.Id, removedDocument.Title);
                return removedDocument;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("DocumentStorageGrain::RemoveDocument: exception raised. " +
                    "GrainId={grainId} UserId={userId}. Message: {exMessage}", grainId, userId, ex.Message);
                throw;
            }
        }

        public async Task<IDocument?> RemoveDocument(string documentId)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("DocumentStorageGrain::RemoveDocument: start remove document. " +
                "GrainId={grainId} UserId={userId}, DocumentId={documentId}", grainId, userId, documentId);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogCritical("DocumentStorageGrain::RemoveDocument: userId is null or empty." +
                    "GrainId={grainId}", grainId);
                throw new Exception("UserId is null or empty.");
            }

            try
            {
                var projectGrain = GrainFactory.GetGrain<IProjectGrain>(grainId, userId);
                var document = await projectGrain.GetDocumentById(documentId);

                if (document == null)
                {
                    _logger.LogWarning("DocumentStorageGrain::RemoveDocument: document isn\'t found. " +
                        "GrainId={grainId} UserId={userId}, DocumentId={documentId}", grainId, userId, documentId);
                    return null;
                }

                document = await projectGrain.RemoveDocument(document.Id);
                if (document == null)
                {
                    _logger.LogError("DocumentStorageGrain::RemoveDocument: document isn\'t found. " +
                        "GrainId={grainId} UserId={userId}, DocumentId={documentId}", grainId, userId, documentId);
                    return null;
                }
                _logger.LogInformation("DocumentStorageGrain::RemoveDocument: removed document from project. " +
                    "GrainId={grainId} UserId={userId}, DocumentId={documentId}", grainId, userId, document.Id);

                document = await RemoveDocumentAsync(document, grainId, userId);
                _logger.LogInformation("DocumentStorageGrain::RemoveDocument: removed document from vector storage. " +
                    "GrainId={grainId} UserId={userId}, DocumentId={documentId}", grainId, userId, document.Id);

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("DocumentStorageGrain::RemoveDocument: exception raised. " +
                    "GrainId={grainId} UserId={userId}, DocumentId={documentId}. Message={exMessage}",
                    grainId, userId, documentId, ex.Message);
                return null;
            }
        }

        public async Task<IDocument?> RemoveDocumentFromStorage(IDocument document)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("DocumentStorageGrain::RemoveDocumentFromStorage: start remove document from storage. " +
                "GrainId={grainId} UserId={userId}, DocumentId={documentId}", grainId, userId, document.Id);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogCritical("DocumentStorageGrain::RemoveDocumentFromStorage: userId is null or empty." +
                    "GrainId={grainId}", grainId);
                throw new Exception("UserId is null or empty.");
            }

            var doc = await RemoveDocumentAsync(document, grainId, userId);

            _logger.LogInformation("DocumentStorageGrain::RemoveDocumentFromStorage: removing document from storage result. " +
                "GrainId={grainId} UserId={userId}, Success={isDocumentRevmoded}", grainId, userId, (doc != null));
            return doc;
        }
    }
}
