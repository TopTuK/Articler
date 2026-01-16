using Articler.AppDomain.Factories.Document;
using Articler.AppDomain.Helpers;
using Articler.AppDomain.Models.Documents;
using Articler.AppDomain.Models.VectorStorage;
using Articler.AppDomain.Settings;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using OpenAI;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Services.VectorStorage
{
    public class _OpenAIVectorStorageService(
        ILogger<_OpenAIVectorStorageService> logger,
        VectorStore vectorStore,
        IOptions<QdrantSettings> vectorStorageSettings,
        OpenAIClient openAIClient,
        IOptions<OpenAIClientSettings> openAiSettings) : IVectorStorageService
    {
        private readonly ILogger<_OpenAIVectorStorageService> _logger = logger;

        private readonly QdrantSettings _vectorStorageSettings = vectorStorageSettings.Value;
        private readonly VectorStore _vectorStore = vectorStore;
        private readonly OpenAIClientSettings _embeddingSettings = openAiSettings.Value;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = openAIClient
            .GetEmbeddingClient(openAiSettings.Value.EmbeddingModel)
            .AsIEmbeddingGenerator();

        public async Task EnsureCollectionExist(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("VectorStorageService::EnsureCollectionExist: ensuring collection exists. CollectionName={collectionName}",
                _vectorStorageSettings.OpenAICollectionName);

            try
            {
                // GetCollection will create the collection if it doesn't exist
                // Using DocumentStorageRecord ensures the collection schema matches the model
                var collection = _vectorStore.GetCollection<Guid, OpenAIDocumentStorageRecord>(_vectorStorageSettings.OpenAICollectionName);
                await collection.EnsureCollectionExistsAsync(cancellationToken);

                _logger.LogInformation("VectorStorageService::EnsureCollectionExist: collection ensured. CollectionName={collectionName}",
                    _vectorStorageSettings.OpenAICollectionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VectorStorageService::EnsureCollectionExist: failed to ensure collection exists. CollectionName={collectionName}",
                    _vectorStorageSettings.OpenAICollectionName);
                throw;
            }
        }

        public async Task<IDocument> StoreTextAsync(
            string userId, Guid projectId,
            Guid documentId, string title, string text)
        {
            _logger.LogInformation("VectorStorageService::StoreTextAsync: start storing document to vector storage. " +
                "UserId={userId}, ProjectId={projectId}, Title={title}, TextLength={textLength}",
                userId, projectId, title, text.Length);

            try
            {
                await EnsureCollectionExist();

                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogError("VectorStorageService::StoreTextAsync: text is null or empty. " +
                        "UserId={userId}, ProjectId={projectId}", userId, projectId);
                    throw new ArgumentException("Text is null or empty.");
                }

                var chunks = TextChunker.ChunkText(text).ToList();
                if (chunks.Count == 0)
                {
                    _logger.LogError("VectorStorageService::StoreTextAsync: text chunks are empty. " +
                        "UserId={userId}, ProjectId={projectId}", userId, projectId);
                    throw new ArgumentException("Text chunks are empty.");
                }

                _logger.LogInformation("VectorStorageService::StoreTextAsync: generated {chunkCount} chunks. " +
                    "UserId={userId}, ProjectId={projectId}", chunks.Count, userId, projectId);

                // Generate embeddings for all chunks
                var chunkEmbeddings = await _embeddingGenerator.GenerateAsync(chunks);
                _logger.LogInformation("VectorStorageService::StoreTextAsync: generated {chunkEmbeddingsCount} embeddings chunks. " +
                    "UserId={userId}, ProjectId={projectId}", chunkEmbeddings.Count, userId, projectId);

                if (chunkEmbeddings.Count != chunks.Count)
                {
                    _logger.LogCritical("VectorStorageService::StoreTextAsync: Embedding count ({chunkEmbeddingsCount}) does not match chunk count ({chunksCount})",
                        chunkEmbeddings.Count, chunks.Count);
                    throw new InvalidOperationException(
                        $"Embedding count ({chunkEmbeddings.Count}) does not match chunk count ({chunks.Count})");
                }

                // Get the collection
                var collection = _vectorStore.GetCollection<Guid, OpenAIDocumentStorageRecord>(_vectorStorageSettings.OpenAICollectionName);

                // Create and store records for each chunk
                var records = new List<OpenAIDocumentStorageRecord>();
                for (int i = 0; i < chunks.Count; i++)
                {
                    var embedding = chunkEmbeddings[i];

                    // Embedding<float> has Vector as ReadOnlyMemory<float>, so we can use it directly
                    var embeddingMemory = embedding.Vector;

                    // Generate unique Guid for each chunk
                    var chunkId = Guid.NewGuid();
                    var record = new OpenAIDocumentStorageRecord
                    {
                        Id = chunkId,
                        UserId = userId,
                        ProjectId = projectId.ToString(),
                        DocumentId = documentId.ToString(),
                        Title = title,
                        TextChunk = chunks[i],
                        OpenAIEmbeddings = embeddingMemory,
                    };

                    records.Add(record);
                }

                // Upsert all records to the collection
                await collection.UpsertAsync(records);

                _logger.LogInformation("VectorStorageService::StoreTextAsync: successfully stored {recordCount} records. UserId={userId}, ProjectId={projectId}",
                    records.Count, userId, projectId);
                return DocumentFactory.CreateDocument(DocumentType.Text, documentId, title);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "VectorStorageService::StoreTextAsync: failed to store text. UserId={userId}, ProjectId={projectId}",
                    userId, projectId);
                throw;
            }
        }

        public async Task<IEnumerable<string>> SearchDocumentsAsync(string query, string userId, Guid projectId, 
            int top = 3, Guid? documentId = null, string? documentTitle = null)
        {
            _logger.LogInformation("VectorStorageService::SearchDocumentsAsync: start search data." +
                "UserId={userId}, ProjectId={projectId}, QueryLength={queryLength}, " +
                "DocumentId={documentId} DocumentTitle={documentTitle}",
                userId, projectId, query.Length, documentId, documentTitle);

            try
            {
                await EnsureCollectionExist();

                if (string.IsNullOrWhiteSpace(query))
                {
                    _logger.LogWarning("VectorStorageService::SearchDocumentsAsync: Query text is null or empty. " +
                        "UserId={userId}, ProjectId={projectId}", userId, projectId);
                    return [];
                }

                var queryEmbedding = await _embeddingGenerator.GenerateVectorAsync(query);
                _logger.LogInformation("VectorStorageService::SearchDocumentsAsync: generated query embedding. " +
                    "QueryEmbeddingLength={queryEmbeddingLength}", queryEmbedding.Length);

                // Get the collection
                var collection = _vectorStore.GetCollection<Guid, OpenAIDocumentStorageRecord>(_vectorStorageSettings.OpenAICollectionName);

                // Perform vector search with filter
                // Using the collection to search for nearest vectors matching the query embedding
                // Convert ReadOnlyMemory<float> to array for search operation
                var queryVector = queryEmbedding.ToArray();

                // Convert Guid values to strings before building the filter expression
                // This avoids calling .ToString() inside the LINQ expression, which cannot be translated to Qdrant filters
                var projectIdString = projectId.ToString();
                var documentIdString = documentId?.ToString();

                // Build filter expression for metadata filtering
                System.Linq.Expressions.Expression<Func<OpenAIDocumentStorageRecord, bool>> filterExpression = record =>
                    record.UserId == userId &&
                    record.ProjectId == projectIdString;

                var searchOptions = _embeddingSettings.Name switch
                {
                    "OpenAI" => new VectorSearchOptions<OpenAIDocumentStorageRecord>
                    {
                        Filter = filterExpression,
                        VectorProperty = doc => doc.OpenAIEmbeddings,
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(_embeddingSettings.Name)),
                };

                /*
                System.Linq.Expressions.Expression<Func<DocumentStorageRecord, bool>> filterExpression = record =>
                    record.UserId == userId &&
                    record.ProjectId == projectIdString &&
                    (documentIdString == null || record.DocumentId == documentIdString) &&
                    (string.IsNullOrWhiteSpace(documentTitle) || record.Title == documentTitle);
                */
                _logger.LogInformation("VectorStorageService::SearchDocumentsAsync: applying filter. " +
                    "UserId={userId}, ProjectId={projectId}, DocumentId={documentId}, Title={title}",
                    userId, projectId, documentId, documentTitle);

                // Enumerate results to execute the search but don't return anything
                var documents = new List<string>();
                await foreach (var result in collection.SearchAsync(queryVector, top, searchOptions))
                {
                    _logger.LogInformation("VectorStorageService::SearchDocumentsAsync: SCORE={score} TextLength={textLength}",
                        result.Score, result.Record.TextChunk.Length);
                    documents.Add(result.Record.TextChunk);
                }

                _logger.LogInformation("VectorStorageService::SearchDocumentsAsync: return documents. " +
                    "UserId={userId} ProjectId={projetId} DocumentsCount={documentsCount}",
                    userId, projectId, documents.Count);
                return documents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VectorStorageService::SearchDocumentsAsync: failed to search text. " +
                    "UserId={userId}, ProjectId={projectId}", userId, projectId);
                return [];
            }
        }

        public async Task<IDocument?> RemoveDocumentAsync(string userId, Guid projectId, Guid documentId)
        {
            _logger.LogInformation("VectorStorageService::RemoveDocumentAsync: start removing document. " +
                "UserId={userId}, ProjectId={projectId}, DocumentId={documentId}",
                userId, projectId, documentId);

            try
            {
                await EnsureCollectionExist();

                // Get the collection
                var collection = _vectorStore.GetCollection<Guid, OpenAIDocumentStorageRecord>(_vectorStorageSettings.OpenAICollectionName);

                // Convert Guid values to strings before building the filter expression
                // This avoids calling .ToString() inside the LINQ expression, which cannot be translated to Qdrant filters
                var projectIdString = projectId.ToString();
                var documentIdString = documentId.ToString();

                // Find all records matching the filter to get their IDs and the document title
                var recordIds = new List<Guid>();
                string? documentTitle = null;

                // Use a dummy vector for search (we only need the filter to work)
                // We'll use a zero vector of the correct dimension (1536 based on the model)
                var dummyVector = new float[_embeddingSettings.EmbeddingModelDimension];

                // Build filter expression to find all records for this document
                System.Linq.Expressions.Expression<Func<OpenAIDocumentStorageRecord, bool>> filterExpression = record =>
                    record.UserId == userId &&
                    record.ProjectId == projectIdString &&
                    record.DocumentId == documentIdString;
                var searchOptions = _embeddingSettings.Name switch
                {
                    "OpenAI" => new VectorSearchOptions<OpenAIDocumentStorageRecord>
                    {
                        Filter = filterExpression,
                        VectorProperty = doc => doc.OpenAIEmbeddings,
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(_embeddingSettings.Name)),
                };

                _logger.LogInformation("VectorStorageService::RemoveDocumentAsync: searching for records to delete. " +
                    "UserId={userId}, ProjectId={projectId}, DocumentId={documentId}",
                    userId, projectId, documentId);

                // Search with a large limit to get all matching records
                await foreach (var result in collection.SearchAsync(dummyVector, int.MaxValue, searchOptions))
                {
                    if (result.Record != null)
                    {
                        recordIds.Add(result.Record.Id);
                        // Capture the title from the first record found
                        if (string.IsNullOrEmpty(documentTitle))
                        {
                            documentTitle = result.Record.Title;
                        }
                    }
                }

                if (recordIds.Count == 0)
                {
                    _logger.LogWarning("VectorStorageService::RemoveDocumentAsync: no records found to delete. " +
                        "UserId={userId}, ProjectId={projectId}, DocumentId={documentId}",
                        userId, projectId, documentId);
                    // Return a document with empty title if not found
                    return null;
                }

                _logger.LogInformation("VectorStorageService::RemoveDocumentAsync: found {recordCount} records to delete. " +
                    "UserId={userId}, ProjectId={projectId}, DocumentId={documentId}",
                    recordIds.Count, userId, projectId, documentId);

                // Delete all matching records by their IDs
                await collection.DeleteAsync(recordIds);

                _logger.LogInformation("VectorStorageService::RemoveDocumentAsync: successfully deleted {recordCount} records. " +
                    "UserId={userId}, ProjectId={projectId}, DocumentId={documentId}",
                    recordIds.Count, userId, projectId, documentId);

                // Return the document with the title we found
                return DocumentFactory.CreateDocument(DocumentType.Text, documentId, documentTitle ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VectorStorageService::RemoveDocumentAsync: failed to remove document. " +
                    "UserId={userId}, ProjectId={projectId}, DocumentId={documentId}",
                    userId, projectId, documentId);
                throw;
            }
        }
    }
}
