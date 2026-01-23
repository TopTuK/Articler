using Articler.AppDomain.Models.Chat;
using Articler.AppDomain.Models.Project;

namespace Articler.WebApi.Services.Chat
{
    public interface IChatService
    {
        Task<IEnumerable<IGrainChatMessage>?> GetChatHistoryAsync(string userId, string projectId);

        Task<IChatMessageResult> SendChatMessageAsync(string userId, string projectId, string message, ChatMode chatMode);
    }
}
