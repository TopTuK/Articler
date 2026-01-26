using Articler.AppDomain.Constants;
using Articler.AppDomain.Models.Chat;
using Articler.GrainInterfaces.Chat;
using Microsoft.Extensions.Logging;

namespace Articler.GrainClasses.Chat
{
    public class ChatAgentGrain : Grain, IChatAgentGrain
    {
        private readonly ILogger<ChatAgentGrain> _logger;

        private readonly IPersistentState<ChatHistoryState> _chatHistoryState;

        public ChatAgentGrain(
            ILogger<ChatAgentGrain> logger,
            [PersistentState(OrleansConstants.ProjectChatHistoryStateName, OrleansConstants.AdoStorageProviderName)]
                IPersistentState<ChatHistoryState> chatHistoryState)
        {
            _logger = logger;

            _chatHistoryState = chatHistoryState;
        }

        public async Task Create(string title, string description)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ChatAgentGrain::Create: create message history for project. " +
                "GrainId={grainId} UserId={userId} Title={title} DescriptionLength={descriptionLength}",
                grainId, userId, title, description.Length);

            try
            {
                var messageText = string.IsNullOrWhiteSpace(description)
                    ? $"Help me to write post. Title and topic of the post are \"{title}\""
                    : $"Help me to write post. Title of the post is \"{title}\". Topic of the post is \"{description}\"";

                // Create writer agent
                var writerAgentGrain = GrainFactory.GetGrain<IWriterAgentGrain>(grainId, userId!);
                await writerAgentGrain.Create(messageText);

                // Create responder agent
                var respondAgentGrain = GrainFactory.GetGrain<IResponderAgentGrain>(grainId, userId!);
                await respondAgentGrain.Create(messageText);

                var message = ChatMessageFactory.CreateUserMessage(messageText);
                _chatHistoryState.State.FirstMessage = message;
                await _chatHistoryState.WriteStateAsync();
                
                _logger.LogInformation("ChatAgentGrain::Create: added first initial message to state. " +
                    "GrainId={grainId} UserId={userId} Role={msgRole} TextLength={msgTextLength}",
                    grainId, userId, message.Role, message.Content.Length);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ChatAgentGrain::Create: exception raised. " +
                    "GrainId={grainId} UserId={userId} Title={title} DescriptionLength={descriptionLength}. " +
                    "Message={exMsg}",
                    grainId, userId, title, description.Length, ex.Message);
                throw;
            }
        }

        public async Task Delete()
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ChatAgentGrain::Delete: start delete history." +
                "GrainId={grainId} UserId={userId}", grainId, userId);

            var writerAgentGrain = GrainFactory.GetGrain<IWriterAgentGrain>(grainId, userId!);
            await writerAgentGrain.Delete();

            var respondAgentGrain = GrainFactory.GetGrain<IResponderAgentGrain>(grainId, userId!);
            await respondAgentGrain.Delete();

            await _chatHistoryState.ClearStateAsync();

            _logger.LogInformation("ChatAgentGrain::Delete: end delete history." +
                "GrainId={grainId} UserId={userId}", grainId, userId);
        }

        public Task<IEnumerable<IGrainChatMessage>> GetChatHistory()
        {
            var grainId = this.GetPrimaryKey(out var userId);

            var history = _chatHistoryState.State.Messages;
            _logger.LogInformation("ChatAgentGrain::GetChatHistory: start get chat history. " +
                "GrainId={grainId} UserId={userId} MessageCount={messageCount}", grainId, userId, history.Count);            
            return Task.FromResult(history.AsEnumerable());
        }

        public async Task<IAgentChatMessageResponse?> SendUserMessage(string message, ChatMode mode)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ChatAgentGrain::SendUserMessage: received user message. " +
                "GrainId={grainId} UserId={userId} MessageLength={messageLength} Mode={chatMode}",
                grainId, userId, message.Length, mode);

            var userMessage = ChatMessageFactory.CreateUserMessage(message);

            IAgentChatMessageResponse? agentResponse = null;
            switch (mode)
            {
                case ChatMode.Write:
                    var writerAgentGrain = GrainFactory.GetGrain<IWriterAgentGrain>(grainId, userId!);
                    agentResponse = await writerAgentGrain.SendMessage(message);
                    break;
                case ChatMode.Ask:
                    var respondAgentGrain = GrainFactory.GetGrain<IResponderAgentGrain>(grainId, userId!);
                    agentResponse = await respondAgentGrain.SendMessage(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), "SendUserMessage: Unknown ChatMode");
            }

            if (agentResponse == null)
            {
                _logger.LogError("ChatAgentGrain::SendUserMessage: agent response is null. " +
                    "GrainId={grainId} UserId={userId} MessageLength={messageLength} Mode={chatMode}",
                    grainId, userId, message.Length, mode);
                return agentResponse;
            }

            _logger.LogInformation("ChatAgentGrain::SendUserMessage: received assistant response. " +
                "GrainId={grainId} UserId={userId} AssistantMessage={assistantMessage}",
                grainId, userId, agentResponse.Content);
            var assistantMessage = ChatMessageFactory.CreateAssistantMessage(agentResponse.Content);

            _chatHistoryState.State
                .Messages
                .AddRange([userMessage, assistantMessage]);
            await _chatHistoryState.WriteStateAsync();

            return agentResponse;
        }
    }
}