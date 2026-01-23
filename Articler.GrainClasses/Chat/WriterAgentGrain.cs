using Articler.AppDomain.Constants;
using Articler.AppDomain.Models.Chat;
using Articler.AppDomain.Services.VectorStorage;
using Articler.AppDomain.Settings;
using Articler.GrainInterfaces.Chat;
using Articler.GrainInterfaces.Document;
using Articler.GrainInterfaces.Project;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using ChatResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat;

namespace Articler.GrainClasses.Chat
{
    public class WriterAgentGrain : Grain, IWriterAgentGrain
    {
        private static readonly string WRITER_AGENT_INSTRUCTIONS = """
        You are a helpful text writer for different posts. 
        You MUST write text part of post if user asks you to write. Provide information what you have done according response rules.

        POST RULES:
        - Post MUST be in markdown format.
        - Never write in the post what you do or what you do not have enough context. Post text should have content relative to the topic of the post.
        - Use SearchDocuments tool if the request relates to user documents or you need specific context. If context is not enough write part of post with your knoweledge about the topic.
        
        LANGUAGE RULES: 
        - Text of post MUST be on language of project.

        RESPONSE RULES:
        - Never write that are not related to the topic of the post.
        - MUST: your responses must be in JSON format, contains 2 fields: "AgentReply" field there answer what you done and "PostText" field there should be update text of the post
        """;

        private readonly ILogger<WriterAgentGrain> _logger;

        private readonly IPersistentState<AgentHistoryState> _agentChatState;
        private readonly AIAgent _chatAgent;

        public WriterAgentGrain(
            ILogger<WriterAgentGrain> logger,
            [PersistentState(OrleansConstants.AgentChatHistoryStateName, OrleansConstants.AdoStorageProviderName)]
                IPersistentState<AgentHistoryState> agentHistoryState,
            [FromKeyedServices("DeepSeek")] OpenAIClient openAIClient,
            IOptionsSnapshot<OpenAIClientSettings> namedSettings)
        {
            _logger = logger;

            _agentChatState = agentHistoryState;

            var settings = namedSettings.Get(OpenAIClientSettings.DeepSeekOptions);

            var chatAgentScheme = AIJsonUtilities.CreateJsonSchema(typeof(AIChatAgentResponseFormat));
            var chatOptions = new ChatOptions
            {
                Instructions = WRITER_AGENT_INSTRUCTIONS,
                ResponseFormat = settings.Name switch
                {
                    OpenAIClientSettings.OpenAIOptions => ChatResponseFormat.ForJsonSchema(
                        schema: chatAgentScheme,
                        schemaName: "AgentReply",
                        schemaDescription: "Contains assistant reply and text of the post"
                    ),
                    _ => ChatResponseFormat.Json,
                }
            };

            var textSearchoptions = new TextSearchProviderOptions()
            {
                SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
                RecentMessageMemoryLimit = 0
            };

            _chatAgent = openAIClient
                .GetChatClient(settings.ChatModel)
                .CreateAIAgent(new ChatClientAgentOptions
                {
                    Name = "WriterAgent",
                    ChatOptions = chatOptions,
                    AIContextProviderFactory = ctx =>
                        new TextSearchProvider(
                            DocumentsSearchAdapter,
                            ctx.SerializedState,
                            ctx.JsonSerializerOptions,
                            textSearchoptions)
                });
        }

        public async Task<IEnumerable<TextSearchProvider.TextSearchResult>> DocumentsSearchAdapter(
            string query, CancellationToken cancellationToken)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("WriterAgentGrain::DocumentsSearchAdapter: start search information in user documents. " +
                "GrainId={grainId} UserId={userId} Query={query}", grainId, userId, query);

            var documentAgentGrain = GrainFactory.GetGrain<IDocumentAgentGrain>(grainId, userId!);
            var documents = await documentAgentGrain.SearchDocuments(query);

            _logger.LogInformation("WriterAgentGrain::DocumentsSearchAdapter: documents search result. " +
                "GrainId={grainId} UserId={userId} DocumentsCount={documentsCount}",
                grainId, userId, documents.Count());
            var result = documents
                .Select(doc => new TextSearchProvider.TextSearchResult()
                {
                    Text = doc
                })
                .ToList();
            return result;
        }

        public async Task Create(string firstMessage)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("WriterAgentGrain::Create: create writer agent. " +
                "GrainId={grainId} UserId={userId} FirstMessage={firstMessage}",
                grainId, userId, firstMessage);

            _agentChatState.State.FirstMessage = firstMessage;
            await _agentChatState.WriteStateAsync();

            _logger.LogInformation("WriterAgentGrain::Create: writer agent created. " +
                "GrainId={grainId} UserId={userId}", grainId, userId);
        }

        public async Task Delete()
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("WriterAgentGrain::Delete: start delete state of writer agent. " +
                "GrainId={grainId} UserId={userId}", grainId, userId);

            await _agentChatState.ClearStateAsync();

            _logger.LogInformation("WriterAgentGrain::Delete: deleted state of writer agent. " +
                "GrainId={grainId} UserId={userId}", grainId, userId);
        }

        public async Task<IAgentChatMessageResponse?> SendMessage(string message)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("WriterAgentGrain::SendMessage: received user message. Start agent work. " +
                "GrainId={grainId} UserId={userId}, MessageLength={messageLength}", grainId, userId, message.Length);

            var projectGrain = GrainFactory.GetGrain<IProjectGrain>(grainId, userId!);
            var project = await projectGrain.Get();
            var projectText = await projectGrain.GetProjectText();
            var projectDocuments = await projectGrain.GetDocuments();

            try
            {
                string promt = null!;
                var thread = _chatAgent.GetNewThread();
                var documentsPromt = projectDocuments.Any()
                    ? $"* Current documents: [{string.Join(',', projectDocuments.Select(doc => $"(id={doc.Id};Title={doc.Title})").ToList())}]"
                    : " There is no any documents.";

                // Project language
                var language = project.Language switch
                {
                    AppDomain.Models.Project.ProjectLanguage.Russian => "Russian",
                    AppDomain.Models.Project.ProjectLanguage.English => "English",
                    _ => throw new ArgumentOutOfRangeException(nameof(project.Language), "language of the project is not supported")
                };

                // Start conversation
                if (string.IsNullOrEmpty(_agentChatState.State.MessageHistory))
                {
                    // First user message
                    promt = $"{_agentChatState.State.FirstMessage}. {message}. " +
                        "Current context of the post:" +
                        $"* ProjectId=\"{project.Id}\"" +
                        $"* UserId=\"{userId}\"" +
                        $"* Language of the post=\"{language}\"" +
                        $"* Current text of the post=\"{projectText.Text}\"." +
                        documentsPromt;
                }
                else
                {
                    // Conversation exists
                    var jsonThread = JsonSerializer.Deserialize<JsonElement>(
                        _agentChatState.State.MessageHistory,
                        JsonSerializerOptions.Web);
                    thread = _chatAgent.DeserializeThread(jsonThread, JsonSerializerOptions.Web);
                    promt = $"{message}. Current context of the post:" +
                        $"* ProjectId=\"{project.Id}\"" +
                        $"* UserId=\"{userId}\"" +
                        $"* Language of the post=\"{language}\"" +
                        $"* Current text of the post=\"{projectText.Text}\"." +
                        documentsPromt;
                }

                _logger.LogInformation("WriterAgentGrain::SendMessage: send promt to respond agent. " +
                    "GrainId={grainId} UserId={userId} PromtLength={promtLength}",
                    grainId, userId, promt.Length);
                var chatResult = await _chatAgent.RunAsync(promt, thread);

                _logger.LogInformation("WriterAgentGrain::SendMessage: received agent response. " +
                    "GrainId={grainId} UserId={userId} ResultTextLength={textLength}",
                    grainId, userId, chatResult.Text.Length);
                var history = thread
                    .Serialize(JsonSerializerOptions.Web)
                    .GetRawText();
                _agentChatState.State.MessageHistory = history;
                await _agentChatState.WriteStateAsync();

                if (chatResult.TryDeserialize<AIChatAgentResponseFormat>(out var response))
                {
                    _logger.LogInformation("WriterAgentGrain::SendMessage: deserealized chat response to JSON. " +
                        "GrainId={grainId} UserId={userId} AssistantText={assistantText} PostTextLength={postLength}",
                        grainId, userId, response.AgentReply, response.PostText?.Length);
                    return ChatMessageFactory.CreateMessageResponse(
                        AppDomain.Models.Chat.ChatRole.Assistant,
                        response.AgentReply ?? "Unknown response",
                        response.PostText ?? "ERROR!");
                }
                else
                {
                    _logger.LogWarning("WriterAgentGrain::SendMessage: can\'t deserealized chat response to JSON. " +
                        "GrainId={grainId} UserId={userId}", grainId, userId);
                    return ChatMessageFactory.CreateMessageResponse(
                        AppDomain.Models.Chat.ChatRole.Assistant,
                        message: chatResult.Text,
                        text: string.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("WriterAgentGrain::SendMessage: exception raised. " +
                    "GrainId={grainId} UserId={userId} Message={exMessage}", grainId, userId, ex.Message);
                return null;
            }
        }
    }
}
