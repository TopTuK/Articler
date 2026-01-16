using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Models.Chat
{
    public interface IChatMessageResult
    {
        MessageResult Result { get; }
        IAgentChatMessageResponse? Response { get; }
    }
}
