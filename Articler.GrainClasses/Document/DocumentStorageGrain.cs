using Articler.AppDomain.Models.Documents;
using Articler.AppDomain.Services.VectorStorage;
using Articler.GrainInterfaces.Document;
using Microsoft.Extensions.Logging;
using OpenAI;
using Qdrant.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Articler.GrainClasses.Document
{
    public class DocumentStorageGrain(
        ILogger<DocumentStorageGrain> logger,
        IVectorStorageService vectorStorageService)
        : Grain, IDocumentStorageGrain
    {
        private readonly ILogger<DocumentStorageGrain> _logger = logger;
        private readonly IVectorStorageService _vectorStorageService = vectorStorageService;

        public async Task<IDocument> AddTextDocument(string title, string text)
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
                var documentId = Guid.NewGuid();
                _logger.LogInformation("DocumentStorageGrain::AddTextDocument: created id of new document. " +
                    "GrainId={grainId} UserId={userId} DocumentId={documentId}", grainId, userId, documentId);
                var document = await _vectorStorageService.StoreTextAsync(userId, grainId, documentId, title, text);

                _logger.LogInformation("DocumentStorageGrain::AddTextDocument: successfully store document to vector db. " +
                    "GrainId={grainId} UserId={userId} DocumentId={documentId} DocumentType={documentType} DocumentTitile={documentTitle}",
                    grainId, userId, document.Id, document.DocumentType, document.Title);
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("DocumentStorageGrain::AddTextDocument: exception raised. " +
                    "GrainId={grainId} UserId={userId}. Message: {exMessage}", grainId, userId, ex.Message);
                throw;
            }
        }

        public async Task<IDocument> RemoveDocument(IDocument document)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("DocumentStorageGrain::RemoveDocument: start remove document. " +
                "GrainId={grainId} UserId={userId}, DocumentId={documentId}, DocumentTitle={documentTitle}",
                grainId, userId, document.Id, document.Title);

            try
            {
                //return document;
                var removedDocument = await _vectorStorageService.RemoveDocumentAsync(userId!, grainId, document.Id);
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
    }
}
