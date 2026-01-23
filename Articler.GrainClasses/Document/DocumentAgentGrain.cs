using Articler.AppDomain.Services.VectorStorage;
using Articler.AppDomain.Settings;
using Articler.GrainInterfaces.Document;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainClasses.Document
{
    public class DocumentAgentGrain : Grain, IDocumentAgentGrain
    {
        private static string DOCUMENT_AGENT_INSTRUCTIONS = """
        You are a assistant who reformulates questions to perform embeddings search.
        Your task is to reformulate the question taking into account the context of the chat.
        The reformulated question must always explicitly contain the subject of the question.
        You MUST reformulate the question in the SAME language as the user's question.
        Never add "in this chat", "in the context of this chat", "in the context of our conversation", "search for" or something like that in your answer.
        """;

        private readonly ILogger _logger;

        private readonly IVectorStorageService _storageService;
        private readonly AIAgent _documentAgent;

        public DocumentAgentGrain(
            ILogger<DocumentAgentGrain> logger,
            IVectorStorageService storageService,
            [FromKeyedServices("DeepSeek")] OpenAIClient openAIClient,
            IOptionsSnapshot<OpenAIClientSettings> namedSettings)
        {
            _logger = logger;
            _storageService = storageService;

            var settings = namedSettings.Get(OpenAIClientSettings.DeepSeekOptions);
            _documentAgent = openAIClient
                .GetChatClient(settings.ChatModel)
                .CreateAIAgent(
                    instructions: DOCUMENT_AGENT_INSTRUCTIONS,
                    name: "DocumentSearcher",
                    description: "Search information in documents"
                );
        }

        public async Task<IEnumerable<string>> SearchDocuments(string query)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("DocumentAgentGrain::SearchDocuments: start search documents by query. " +
                "GrainId={grainId} UserId={userId}, Query={query}", grainId, userId, query);

            var searchQuery = await _documentAgent.RunAsync(
                "Convert query to new query string with keywords for searching in vector database. You MUST return only new query string for search. " +
                $"Query=\"{query}\"");

            if (string.IsNullOrWhiteSpace(searchQuery.Text))
            {
                _logger.LogWarning("DocumentAgentGrain::SearchDocuments: search query is null or whitespace." +
                    "GrainId={grainId} UserId={userId}, Query={query}", grainId, userId, query);
                return [];
            }

            _logger.LogInformation("DocumentAgentGrain::SearchDocuments: start search documents by search query. " +
                "GrainId={grainId} UserId={userId} SearchQuery={searchQuery}",
                grainId, userId, searchQuery);
            var documents = await _storageService.SearchDocumentsAsync(searchQuery.Text, userId!, grainId);

            if (documents.Any())
            {
                _logger.LogInformation("DocumentAgentGrain::SearchDocuments: return documents. " +
                    "GrainId={grainId} UserId={userId} DocumentsCount={documentsCount}",
                    grainId, userId, documents.Count());
                return documents;
            }
            else
            {
                _logger.LogWarning("DocumentAgentGrain::SearchDocuments: documents are not found. " +
                    "GrainId={grainId} UserId={userId} SearchQuery={searchQuery}",
                    grainId, userId, searchQuery);
                return [];
            }
        }
    }
}
