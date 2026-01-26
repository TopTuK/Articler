using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Models.Chat
{
    public interface IAgentChatMessageResponse : IGrainChatMessage
    {
        string Text { get; }
        //int RequestTokenCount { get; }
        //int ResponseTokenCount { get; }
        //int TotalTokenCount { get; }
    }
}
