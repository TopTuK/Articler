using Articler.AppDomain.Constants;
using Articler.AppDomain.Models.Chat;
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
    public class ResponderAgentGrain : Grain, IResponderAgentGrain
    {
        private static readonly string AGENT_INSTRUCTIONS = """
        You are a helpful assistant who answers user's questions about topic of the post. You should advise user what next to write in the post.
        You MUST NOT update text of the post. You should only answer the user's question!

        RESPONSE RULES:
        - Never answer questions that are not related to the topic of the post.
        - Use SearchDocuments tool if the request relates to user documents or you need specific context.
        - Responses MUST BE in JSON format, contains 2 fields: "AgentReply" field there should be your answer and "PostText" field which should be empty string.
        
        LANGUAGE RULES: 
        - You MUST ALWAYS answer in the SAME language as the user's question.
        """;

        private readonly ILogger<ResponderAgentGrain> _logger;

        private readonly IPersistentState<AgentHistoryState> _agentHistoryState;
        private readonly AIAgent _chatAgent;

        public ResponderAgentGrain(
            ILogger<ResponderAgentGrain> logger,
            [PersistentState(OrleansConstants.AgentChatHistoryStateName, OrleansConstants.AdoStorageProviderName)]
                IPersistentState<AgentHistoryState> agentHistoryState,
            [FromKeyedServices("DeepSeekChatClient")] OpenAIClient openAIClient,
            IOptionsSnapshot<ChatAgentSettings> namedSettings)
        {
            _logger = logger;

            _agentHistoryState = agentHistoryState;

            var settings = namedSettings.Get(ChatAgentSettings.DeepSeek);
            var chatAgentScheme = AIJsonUtilities.CreateJsonSchema(typeof(AIChatAgentResponseFormat));
            var chatOptions = new ChatOptions
            {
                Instructions = AGENT_INSTRUCTIONS,
                ResponseFormat = settings.Name switch
                {
                    ChatAgentSettings.OpenAI => ChatResponseFormat.ForJsonSchema(
                        schema: chatAgentScheme,
                        schemaName: "AgentReply",
                        schemaDescription: "Schema contains assistant reply and text of the post (text can be empty)"
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
                    Name = "RespondAgent",
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
            _logger.LogInformation("ResponderAgentGrain::DocumentsSearchAdapter: start search information in users documents. " +
                "GrainId={grainId} UserId={userId} Query={query}", grainId, userId, query);

            var documentAgentGrain = GrainFactory.GetGrain<IDocumentAgentGrain>(grainId, userId!);
            var documents = await documentAgentGrain.SearchDocuments(query);

            _logger.LogInformation("ResponderAgentGrain::DocumentsSearchAdapter: documents search result. " +
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
            _logger.LogInformation("ResponderAgentGrain::Create: create respond agent. " +
                "GrainId={grainId} UserId={userId} FirstMessage={firstMessage}",
                grainId, userId, firstMessage);

            _agentHistoryState.State.FirstMessage = firstMessage;
            await _agentHistoryState.WriteStateAsync();

            _logger.LogInformation("ResponderAgentGrain::Create: respond agent created. " +
                "GrainId={grainId} UserId={userId}", grainId, userId);
        }

        public async Task Delete()
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ResponderAgentGrain::Delete: start delete state of respond agent. " +
                "GrainId={grainId} UserId={userId}", grainId, userId);

            await _agentHistoryState.ClearStateAsync();

            _logger.LogInformation("ResponderAgentGrain::Delete: deleted state of respond agent. " +
                "GrainId={grainId} UserId={userId}", grainId, userId);
        }

        public async Task<IAgentChatMessageResponse?> SendMessage(string message)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ResponderAgentGrain::SendMessage: received user message. Start agent work. " +
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
                if (string.IsNullOrEmpty(_agentHistoryState.State.MessageHistory))
                {
                    // First user message
                    promt = $"{_agentHistoryState.State.FirstMessage}. {message}. " +
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
                        _agentHistoryState.State.MessageHistory,
                        JsonSerializerOptions.Web);
                    thread = _chatAgent.DeserializeThread(jsonThread, JsonSerializerOptions.Web);
                    promt = $"{message}. Current context of the post:" +
                        $"* ProjectId=\"{project.Id}\"" +
                        $"* UserId=\"{userId}\"" +
                        $"* Language of the post=\"{language}\"" +
                        $"* Current text of the post=\"{projectText.Text}\"." +
                        documentsPromt;
                }

                _logger.LogInformation("ResponderAgentGrain::SendMessage: send promt to respond agent. " +
                    "GrainId={grainId} UserId={userId} PromtLength={promtLength}",
                    grainId, userId, promt.Length);
                var chatResult = await _chatAgent.RunAsync(promt, thread);

                _logger.LogInformation("ResponderAgentGrain::SendMessage: received agent response. " +
                    "GrainId={grainId} UserId={userId} ResultTextLength={textLength} ",
                    grainId, userId, chatResult.Text.Length);
                _logger.LogInformation("ResponderAgentGrain::SendMessage: token usage. " +
                    "GrainId={grainId} UserId={userId}" +
                    "InTokenCount={inputTokensCount}, OutTokenCount={outTokenCount} TotalToken={totalToken}",
                    grainId, userId, chatResult.Usage?.InputTokenCount, chatResult.Usage?.OutputTokenCount,
                    chatResult.Usage?.TotalTokenCount);

                var history = thread
                    .Serialize(JsonSerializerOptions.Web)
                    .GetRawText();
                _agentHistoryState.State.MessageHistory = history;
                await _agentHistoryState.WriteStateAsync();

                if (chatResult.TryDeserialize<AIChatAgentResponseFormat>(out var response))
                {
                    _logger.LogInformation("ResponderAgentGrain::SendMessage: deserealized chat response to JSON. " +
                        "GrainId={grainId} UserId={userId} AssistantText={assistantText} PostTextLength={postLength}",
                        grainId, userId, response.AgentReply, response.PostText?.Length);
                    return ChatMessageFactory.CreateMessageResponse(
                        AppDomain.Models.Chat.ChatRole.Assistant,
                        response.AgentReply ?? "Unknown response",
                        response.PostText ?? string.Empty);
                }
                else
                {
                    _logger.LogWarning("ResponderAgentGrain::SendMessage: can\'t deserealized chat response to JSON. " +
                        "GrainId={grainId} UserId={userId}", grainId, userId);
                    return ChatMessageFactory.CreateMessageResponse(
                        AppDomain.Models.Chat.ChatRole.Assistant,
                        message: chatResult.Text,
                        text: string.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ResponderAgentGrain::SendMessage: exception raised. " +
                    "GrainId={grainId} UserId={userId} Message={exMessage}", grainId, userId, ex.Message);
                return null;
            }
        }
    }
}
