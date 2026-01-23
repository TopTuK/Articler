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
        Task Create(string title, string description);
        Task Delete();
        Task<IEnumerable<IGrainChatMessage>> GetChatHistory();

        //[ResponseTimeout("00:05:00")]
        //Task<IAgentChatMessageResponse?> SendUserMessage(string message);

        [ResponseTimeout("00:10:00")]
        Task<IAgentChatMessageResponse?> SendUserMessage(string message, ChatMode mode);
    }
}
