using Articler.AppDomain.Models.Chat;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainInterfaces.Chat
{
    public interface IChatAgentGrain : IGrainWithGuidCompoundKey
    {
        Task CreateChatAgent(string title, string description, string text = "");
        Task DeleteChatAgent();
        Task<IEnumerable<IGrainChatMessage>> GetChatHistory();

        [ResponseTimeout("00:05:00")]
        Task<IAgentChatMessageResponse?> SendUserMessage(string message);
    }
}
