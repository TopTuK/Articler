using Articler.AppDomain.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Factories.Chat
{
    public static class ChatMessageResultFactory
    {
        private class ChatMessageResult : IChatMessageResult
        {
            public MessageResult Result { get; init; }
            public IAgentChatMessageResponse? Response { get; init; }
        }

        public static IChatMessageResult CreateChatMessageResult(
            MessageResult result, IAgentChatMessageResponse? response = null) => new ChatMessageResult
            { 
                Result = result, 
                Response = response 
            };
    }
}
