using Articler.AppDomain.Models.Documents;
using Articler.AppDomain.Settings;
using Microsoft.Extensions.AI;
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
    // deepseek-embedding-v2
    // 1024
    public class DeepSeekVectorStorageService(
        ILogger<DeepSeekVectorStorageService> logger,
        VectorStore vectorStore,
        IOptions<QdrantSettings> vectorStorageSettings,
        OpenAIClient openAIClient,
        IOptions<OpenAIClientSettings> openAiSettings) : IVectorStorageService
    {
        private readonly ILogger<DeepSeekVectorStorageService> _logger = logger;

        private readonly QdrantSettings _vectorStorageSettings = vectorStorageSettings.Value;
        private readonly VectorStore _vectorStore = vectorStore;
        private readonly OpenAIClientSettings _embeddingSettings = openAiSettings.Value;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = openAIClient
            .GetEmbeddingClient(openAiSettings.Value.EmbeddingModel)
            .AsIEmbeddingGenerator();

        public Task EnsureCollectionExist(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IDocument?> RemoveDocumentAsync(string userId, Guid projectId, Guid documentId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> SearchDocumentsAsync(string query, string userId, Guid projectId, int top = 3, Guid? documentId = null, string? documentTitle = null)
        {
            throw new NotImplementedException();
        }

        public Task<IDocument> StoreTextAsync(string userId, Guid projectId, Guid documentId, string title, string text)
        {
            throw new NotImplementedException();
        }
    }
}
