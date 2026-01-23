using Articler.AppDomain.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainInterfaces.Chat
{
    public interface IAgentGrain : IGrainWithGuidCompoundKey
    {
        Task Create(string firstMessage);
        Task Delete();

        [ResponseTimeout("00:10:00")]
        Task<IAgentChatMessageResponse?> SendMessage(string message);
    }
}
