using Articler.AppDomain.Factories.Chat;
using Articler.AppDomain.Models.Chat;
using Articler.AppDomain.Models.Project;
using Articler.GrainInterfaces.Chat;
using Articler.GrainInterfaces.Project;
using Articler.GrainInterfaces.User;

namespace Articler.WebApi.Services.Chat
{
    public class ChatService(ILogger<IChatService> logger, IClusterClient clusterClient) 
        : IChatService
    {
        private readonly ILogger<IChatService> _logger = logger;
        private readonly IClusterClient _clusterClient = clusterClient;

        public async Task<IEnumerable<IGrainChatMessage>?> GetChatHistoryAsync(string userId, string projectId)
        {
            _logger.LogInformation("ChatService::GetChatHistoryAsync: start get chat history. " +
                "UserId={userId} ProjectId={projectId}", userId, projectId);
            try
            {
                var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
                var project = await userGrain.GetProjectById(projectId);

                if (project == null)
                {
                    _logger.LogError("ChatService::GetChatHistoryAsync: project is not belong to user. " +
                        "UserId={userId} ProjectId={projectId}", userId, projectId);
                    return null;
                }

                var chatGrain = _clusterClient.GetGrain<IChatAgentGrain>(project.Id, userId);
                var history = await chatGrain.GetChatHistory();

                _logger.LogInformation("ChatService::GetChatHistoryAsync: return chat history. " +
                    "UserId={userId} ProjectId={projectId} MessageCount={messageCount}",
                    userId, projectId, history.Count());
                return history;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ChatService::GetChatHistoryAsync: Exception raised. " +
                    "UserId={userId} ProjectId={projectId}. Message: {exMsg}", userId, projectId, ex.Message);
                throw;
            }
        }

        public async Task<IChatMessageResult> SendChatMessageAsync(string userId, string projectId, string message)
        {
            _logger.LogInformation("ChatService::SendMessageAsync: get message from user. " +
                "UserId={userId}, ProjectId={projectId}, MessageLength={messageLength}",
                userId, projectId, message.Length);

            try
            {
                var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
                var project = await userGrain.GetProjectById(projectId);

                if (project == null)
                {
                    _logger.LogError("ChatService::SendMessageAsync: project is not belong to user. " +
                        "UserId={userId} ProjectId={projectId}", userId, projectId);
                    return ChatMessageResultFactory.CreateChatMessageResult(MessageResult.NotFound);
                }

                _logger.LogInformation("ChatService::SendMessageAsync: got user project. " +
                    "UserId={userId} ProjectId={projectId} ProjectTitle={projectTitle}",
                    userId, project.Id, project.Title);

                var chatGrain = _clusterClient.GetGrain<IChatAgentGrain>(project.Id, userId);
                var agentResponse = await chatGrain.SendUserMessage(message);
                if (agentResponse ==  null)
                {
                    _logger.LogError("ChatService::SendMessageAsync: agent response is null. " +
                        "UserId={userId}, ProjectId={projectId}}", userId, projectId);
                    return ChatMessageResultFactory.CreateChatMessageResult(MessageResult.Error);
                }

                if ((agentResponse.Text != null) && (agentResponse.Text.Length > 0))
                {
                    var projectGrain = _clusterClient.GetGrain<IProjectGrain>(project.Id, userId);
                    await projectGrain.SetProjectText(agentResponse.Text);
                }

                return ChatMessageResultFactory.CreateChatMessageResult(MessageResult.Success, agentResponse);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ChatService::SendMessageAsync: Exception raised. Msg: {exMsg}",
                    ex.Message);
                throw;
            }
        }
    }
}
