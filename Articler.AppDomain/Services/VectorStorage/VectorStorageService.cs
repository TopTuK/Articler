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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Services.VectorStorage
{
    public class VectorStorageService: IVectorStorageService
    {
        private readonly ILogger<VectorStorageService> _logger;

        private readonly QdrantSettings _vectorStorageSettings;
        private readonly VectorStore _vectorStore;
        private readonly EmbeddingAgentSettings _embeddingSettings;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

        public VectorStorageService(
            ILogger<VectorStorageService> logger,
            VectorStore vectorStore,
            IOptions<QdrantSettings> vectorStorageSettings,
            [FromKeyedServices("OpenRouterEmbeddingClient")] OpenAIClient openAIClient,
            IOptionsSnapshot<EmbeddingAgentSettings> namedOpenAiSettings)
        {
            _logger = logger;

            _vectorStorageSettings = vectorStorageSettings.Value;
            _vectorStore = vectorStore;

            var opt = namedOpenAiSettings.Get(EmbeddingAgentSettings.OpenRouter);
            _embeddingSettings = opt;
            _embeddingGenerator = openAIClient
                .GetEmbeddingClient(opt.EmbeddingModel)
                .AsIEmbeddingGenerator();
        }

        public async Task EnsureCollectionExist(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("VectorStorageService::EnsureCollectionExist: ensuring collection exists. CollectionName={collectionName}",
                _vectorStorageSettings.CollectionName);

            try
            {
                // GetCollection will create the collection if it doesn't exist
                // Using DocumentStorageRecord ensures the collection schema matches the model
                var collection = _vectorStore.GetCollection<Guid, DocumentStorageRecord>(_vectorStorageSettings.CollectionName);
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

                var chunks = TextChunker
                    .ChunkText(text)
                    .ToList();
                if (chunks.Count == 0)
                {
                    _logger.LogError("VectorStorageService::StoreTextAsync: text chunks are empty. " +
                        "UserId={userId}, ProjectId={projectId}", userId, projectId);
                    throw new ArgumentException("Text chunks are empty.");
                }

                _logger.LogInformation("VectorStorageService::StoreTextAsync: generated [{chunkCount}] chunks. " +
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
                var collection = _vectorStore
                    .GetCollection<Guid, DocumentStorageRecord>(_vectorStorageSettings.CollectionName);

                // Create and store records for each chunk
                var records = new List<DocumentStorageRecord>();
                for (int i = 0; i < chunks.Count; i++)
                {
                    var embedding = chunkEmbeddings[i];

                    // Embedding<float> has Vector as ReadOnlyMemory<float>, so we can use it directly
                    var embeddingMemory = embedding.Vector;

                    // Generate unique Guid for each chunk
                    var chunkId = Guid.NewGuid();
                    var record = new DocumentStorageRecord
                    {
                        Id = chunkId,
                        UserId = userId,
                        ProjectId = projectId.ToString(),
                        DocumentId = documentId.ToString(),
                        Title = title,
                        TextChunk = chunks[i],
                        Embeddings = embeddingMemory,
                    };

                    /*
                    switch (_embeddingSettings.Name)
                    {
                        case OpenAIClientSettings.OpenAIOptions:
                            record.OpenAIEmbeddings = embeddingMemory;
                            record.DeepSeekEmbeddings = new float[1536];
                            break;
                        case OpenAIClientSettings.DeepSeekOptions:
                        case OpenAIClientSettings.OpenRouterOptions:
                            record.OpenAIEmbeddings = new float[1536];
                            record.DeepSeekEmbeddings = embeddingMemory;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(
                                nameof(_embeddingSettings.Name),
                                $"VectorStorageService::StoreTextAsync: unknown Embeddings name = {_embeddingSettings.Name}");
                    }
                    */

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

        public async Task<IDocument?> RemoveDocumentAsync(string userId, Guid projectId, Guid documentId)
        {
            _logger.LogInformation("VectorStorageService::RemoveDocumentAsync: start removing document. " +
                "UserId={userId}, ProjectId={projectId}, DocumentId={documentId}",
                userId, projectId, documentId);

            try
            {
                await EnsureCollectionExist();

                // Get the collection
                var collection = _vectorStore
                    .GetCollection<Guid, DocumentStorageRecord>(_vectorStorageSettings.CollectionName);

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
                System.Linq.Expressions.Expression<Func<DocumentStorageRecord, bool>> filterExpression = record =>
                    record.UserId == userId &&
                    record.ProjectId == projectIdString &&
                    record.DocumentId == documentIdString;
                var searchOptions = new VectorSearchOptions<DocumentStorageRecord>
                {
                    Filter = filterExpression,
                    VectorProperty = doc => doc.Embeddings,
                };
                /*
                var searchOptions = _embeddingSettings.Name switch
                {
                    OpenAIClientSettings.OpenAIOptions => new VectorSearchOptions<DocumentStorageRecord>
                    {
                        Filter = filterExpression,
                        VectorProperty = doc => doc.OpenAIEmbeddings,
                    },
                    OpenAIClientSettings.DeepSeekOptions => new VectorSearchOptions<DocumentStorageRecord>
                    {
                        Filter = filterExpression,
                        VectorProperty = doc => doc.DeepSeekEmbeddings,
                    },
                    OpenAIClientSettings.OpenRouterOptions => new VectorSearchOptions<DocumentStorageRecord>
                    {
                        Filter = filterExpression,
                        VectorProperty = doc => doc.DeepSeekEmbeddings,
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(_embeddingSettings.Name)),
                };
                */
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
                var collection = _vectorStore
                    .GetCollection<Guid, DocumentStorageRecord>(_vectorStorageSettings.CollectionName);

                // Perform vector search with filter
                // Using the collection to search for nearest vectors matching the query embedding
                // Convert ReadOnlyMemory<float> to array for search operation
                var queryVector = queryEmbedding.ToArray();

                // Convert Guid values to strings before building the filter expression
                // This avoids calling .ToString() inside the LINQ expression, which cannot be translated to Qdrant filters
                var projectIdString = projectId.ToString();
                var documentIdString = documentId?.ToString();

                // Build filter expression for metadata filtering
                /*
                System.Linq.Expressions.Expression<Func<DocumentStorageRecord, bool>> filterExpression = record =>
                    record.UserId == userId &&
                    record.ProjectId == projectIdString &&
                    (documentIdString == null || record.DocumentId == documentIdString) &&
                    (string.IsNullOrWhiteSpace(documentTitle) || record.Title == documentTitle);
                */
                System.Linq.Expressions.Expression<Func<DocumentStorageRecord, bool>> filterExpression = record =>
                    record.UserId == userId &&
                    record.ProjectId == projectIdString;
                var searchOptions = new VectorSearchOptions<DocumentStorageRecord>
                {
                    Filter = filterExpression,
                    VectorProperty = doc => doc.Embeddings,
                };
                /*
                var searchOptions = _embeddingSettings.Name switch
                {
                    OpenAIClientSettings.OpenAIOptions => new VectorSearchOptions<DocumentStorageRecord>
                    {
                        Filter = filterExpression,
                        VectorProperty = doc => doc.OpenAIEmbeddings,
                    },
                    OpenAIClientSettings.DeepSeekOptions => new VectorSearchOptions<DocumentStorageRecord>
                    {
                        Filter = filterExpression,
                        VectorProperty = doc => doc.DeepSeekEmbeddings,
                    },
                    OpenAIClientSettings.OpenRouterOptions => new VectorSearchOptions<DocumentStorageRecord>
                    {
                        Filter = filterExpression,
                        VectorProperty = doc => doc.DeepSeekEmbeddings,
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(_embeddingSettings.Name)),
                };
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
    }
}
