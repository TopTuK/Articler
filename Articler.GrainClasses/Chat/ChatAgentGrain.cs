using Articler.AppDomain.Constants;
using Articler.AppDomain.Models.Chat;
using Articler.AppDomain.Services.VectorStorage;
using Articler.AppDomain.Settings;
using Articler.GrainClasses.Project;
using Articler.GrainInterfaces.Chat;
using Articler.GrainInterfaces.Project;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat;

namespace Articler.GrainClasses.Chat
{
    public class ChatAgentGrain : Grain, IChatAgentGrain
    {
        private static readonly string DOCUMENT_AGENT_INSTRUCTIONS = """
        You are a helpful assistant that reformulates questions to perform embeddings search.
        Your task is to reformulate the question taking into account the context of the chat.
        The reformulated question must always explicitly contain the subject of the question.
        You MUST reformulate the question in the SAME language as the user's question.
        Never add "in this chat", "in the context of this chat", "in the context of our conversation", "search for" or something like that in your answer.
        """;

        private static readonly string WRITER_AGENT_INSTRUCTIONS = """
        You are a helpful text writer and editor for different posts.
        Answer questions to chat OR  provide imoformation what you do and provide updated text of post.
        If you don't know context of post ask user to provide it even if user asks a question.
        If user asks you to write text when text of the post should be in markdown format.
        Use SearchDocuments tool if the request relates to user documents or need specific context.
        Answer user on his language.
        Update text of post on a language it initiale created or on language of user request.
        
        LANGUAGE RULE: You MUST ALWAYS answer in the SAME language as the user's question.
        TOOL USAGE RULES:
        - Use available tools only MAX 10 times.
        RESPONSE RULES:
        - Never answer questions that are not related to theme of the post.
        - Responses must be in JSON format, contains 2 fields: "AgentReply" field there answer what you done and "PostText" field there should be update text of the post
        """;

        private readonly ILogger<ChatAgentGrain> _logger;

        private readonly IPersistentState<ChatHistoryState> _chatHistoryState;
        private readonly AIAgent _chatAgent;
        private readonly AIAgent _documentAgent;
        private readonly IVectorStorageService _storageService;
        private readonly OpenAIClientSettings _clientSettings;

        public ChatAgentGrain(
            ILogger<ChatAgentGrain> logger,
            [PersistentState(OrleansConstants.ProjectChatHistoryStateName, OrleansConstants.AdoStorageProviderName)]
                IPersistentState<ChatHistoryState> chatHistoryState,
            [FromKeyedServices("DeepSeek")] OpenAIClient openAIClient,
            IOptionsSnapshot<OpenAIClientSettings> namedSettings,
            IVectorStorageService storageService)
        {
            _logger = logger;

            _chatHistoryState = chatHistoryState;

            var settings = namedSettings.Get(OpenAIClientSettings.DeepSeekOptions);
            _clientSettings = settings;

            _documentAgent = openAIClient
                .GetChatClient(settings.ChatModel)
                .CreateAIAgent(
                    instructions: DOCUMENT_AGENT_INSTRUCTIONS,
                    name: "DocumentSearcher",
                    description: "Creates query string with keywords and searches information in users documents",
                    tools: [AIFunctionFactory.Create(SearchDocumentAsync)]
                );

            var chatAgentScheme = AIJsonUtilities.CreateJsonSchema(typeof(AIChatAgentResponseFormat));
            var chatOptions = new ChatOptions
            {
                Instructions = WRITER_AGENT_INSTRUCTIONS,
                Tools = [_documentAgent.AsAIFunction()],
            };

            chatOptions.ResponseFormat = settings.Name switch
            {
                OpenAIClientSettings.OpenAIOptions => ChatResponseFormat.ForJsonSchema(
                    schema: chatAgentScheme,
                    schemaName: "AgentReply",
                    schemaDescription: "Schema contains assistant reply and text of the post (text can be empty)"
                ),
                _ => ChatResponseFormat.Json,
            };

            _chatAgent = openAIClient
                .GetChatClient(settings.ChatModel)
                .CreateAIAgent(new ChatClientAgentOptions
                {
                    Name = "WriterAgent",
                    ChatOptions = chatOptions
                });

            _storageService = storageService;
        }

        [Description("Search information in user documents")]
        public async Task<string> SearchDocumentAsync(
            [Description("User query string")] string userQuery,
            [Description("Document name or empty string if need to search in all documents")] List<string> documents)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ChatAgentGrain::SearchDocumentAsync: start search information in user documents. " +
                "GrainId={grainId} UserId={userId} UserQueryLength={queryLength} DocumentsCount={documentsCount}",
                grainId, userId, userQuery.Length, documents.Count);

            var vectorQuery = await _documentAgent.RunAsync("Convert user query text to query string with keywords. You should return only query string for search" +
                $"User query is \"{userQuery}\"");
            var ragDocuments = await _storageService.SearchDocumentsAsync(vectorQuery.Text, userId!, grainId);

            if (ragDocuments.Any())
            {
                var summaryDocument = await _documentAgent
                    .RunAsync("Summarize and generealize result of rag search in max 30 words" +
                        $"Text for processing is \"{string.Join('.', ragDocuments)}\"." +
                        "Return only result of summarization and generalization. Do not add something else");

                _logger.LogInformation("ChatAgentGrain::SearchDocumentAsync: return summary of rag search. " +
                    "GrainId={grainId} UserId={userId} SummaryLength={summaryLength}",
                    grainId, userId, summaryDocument.Text.Length);
                return summaryDocument.Text;
            }
            else
            {
                _logger.LogInformation("ChatAgentGrain::SearchDocumentAsync: rag search is empty. " +
                    "GrainId={grainId} UserId={userId}", grainId, userId);
                return string.Empty;
            }
        }

        public async Task CreateChatAgent(string title, string description, string text = "")
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ChatAgentGrain::CreateChatAgent: create message history for user. " +
                "GrainId={grainId} UserId={userId} Title={title} DescriptionLength={descriptionLength}",
                grainId, userId, title, description.Length);

            try
            {
                var messageText = string.IsNullOrWhiteSpace(description)
                    ? $"Assist me to write text. Titles is \"{title}\""
                    : $"Assist me to write text. Title is \"{title}\". Description of the text is \"{description}\"";

                var message = ChatMessageFactory.CreateUserMessage(messageText);
                _chatHistoryState.State.FirstMessage = message;
                await _chatHistoryState.WriteStateAsync();
                
                _logger.LogInformation("ChatAgentGrain::CreateChatAgent: added first initial message to state. " +
                    "GrainId={grainId} UserId={userId} Role={msgRole} TextLength={msgTextLength}",
                    grainId, userId, message.Role, message.Content.Length);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ChatAgentGrain::CreateChatAgent: exception raised. " +
                    "GrainId={grainId} UserId={userId} Title={title} DescriptionLength={descriptionLength}. Message={exMsg}",
                    grainId, userId, title, description.Length, ex.Message);
                throw;
            }
        }

        public async Task DeleteChatAgent()
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ChatAgentGrain::DeleteChatAgent: start delete history." +
                "GrainId={grainId} UserId={userId}", grainId, userId);

            await _chatHistoryState.ClearStateAsync();
        }

        public Task<IEnumerable<IGrainChatMessage>> GetChatHistory()
        {
            var grainId = this.GetPrimaryKey(out var userId);

            var history = _chatHistoryState.State.Messages;
            _logger.LogInformation("ChatAgentGrain::GetChatHistory: start get chat history. " +
                "GrainId={grainId} UserId={userId} MessageCount={messageCount}", grainId, userId, history.Count);
            
            return Task.FromResult(history.AsEnumerable());
        }

        public async Task<IAgentChatMessageResponse?> SendUserMessage(string message)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ChatAgentGrain::SendUserMessage: proccess user message. " +
                "GrainId={grainId} UserId={userId} MessageLength={messageLength}",
                grainId, userId, message.Length);

            var projectGrain = GrainFactory.GetGrain<IProjectGrain>(grainId, userId!);
            var project = await projectGrain.Get();
            var projectText = await projectGrain.GetProjectText();
            var projectDocuments = await projectGrain.GetDocuments();

            try
            {
                string promt = null!;
                var thread = _chatAgent.GetNewThread();
                string documentsPromt = projectDocuments.Any()
                    ? $"* Current documents: [{string.Join(',', projectDocuments.Select(d => $"(Id={d.Id};Title={d.Title})").ToList())}]"
                    : "* There is no any documents.";

                if (_chatHistoryState.State.Messages.Count == 0)
                {
                    promt = $"{_chatHistoryState.State.FirstMessage}." +
                        message + "Context is next:"+
                        $"* Current text of post=\"{projectText.Text}\"." +
                        documentsPromt;
                }
                else
                {
                    var jsonThread = JsonSerializer.Deserialize<JsonElement>(
                        _chatHistoryState.State.MessageHistory,
                        JsonSerializerOptions.Web);
                    thread = _chatAgent.DeserializeThread(jsonThread, JsonSerializerOptions.Web);
                    promt = $"{message}. Context is next:" +
                        $"Current text of the post=\"{projectText.Text}\"." +
                        documentsPromt;
                }

                var result = await _chatAgent.RunAsync(promt, thread);
                _logger.LogInformation("ChatAgentGrain::SendUserMessage: result text. " +
                    "GrainId={grainId} UserId={userId} AssistantText={assistantText}",
                    grainId, userId, result.Text);

                var messages = thread.GetService<IList<Microsoft.Extensions.AI.ChatMessage>>();
                if (messages != null)
                {
                    var msgs = messages
                        .Select(m =>
                        {
                            var role = AppDomain.Models.Chat.ChatRole.Unknown;
                            if (m.Role == Microsoft.Extensions.AI.ChatRole.Assistant)
                            {
                                role = AppDomain.Models.Chat.ChatRole.Assistant;
                            }
                            else if (m.Role == Microsoft.Extensions.AI.ChatRole.System)
                            {
                                role = AppDomain.Models.Chat.ChatRole.System;
                            }
                            else if (m.Role == Microsoft.Extensions.AI.ChatRole.User)
                            {
                                role = AppDomain.Models.Chat.ChatRole.User;
                            }
                            else if (m.Role == Microsoft.Extensions.AI.ChatRole.Tool)
                            {
                                role = AppDomain.Models.Chat.ChatRole.Tool;
                            }

                            return ChatMessageFactory.CreateMessage(role, m.Text);
                        })
                        .ToList();

                    _chatHistoryState.State.Messages = msgs;
                    var history = thread
                        .Serialize(JsonSerializerOptions.Web)
                        .GetRawText();
                    _chatHistoryState.State.MessageHistory = history;

                    await _chatHistoryState.WriteStateAsync();
                }

                if(result.TryDeserialize<AIChatAgentResponseFormat>(out var agentResponse))
                {
                    _logger.LogInformation("ChatAgentGrain::SendUserMessage: chat response. " +
                    "GrainId={grainId} UserId={userId} AssistantReply={assistantReply} TextLength={textLength}",
                        grainId, userId, agentResponse.AgentReply, agentResponse.PostText?.Length);
                    return ChatMessageFactory.CreateMessageResponse(
                        AppDomain.Models.Chat.ChatRole.Assistant,
                        agentResponse.AgentReply ?? string.Empty,
                        agentResponse.PostText ?? string.Empty);
                }
                else
                {
                    _logger.LogWarning("ChatAgentGrain::SendUserMessage: can\'t deserialize response.");
                    return ChatMessageFactory.CreateMessageResponse(
                        role: AppDomain.Models.Chat.ChatRole.Assistant,
                        message: result.Text,
                        text: string.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ChatAgentGrain::SendUserMessage: exception raised. MessageL {exMessage}", 
                    ex.Message);
                return null;
            }
        }
    }
}