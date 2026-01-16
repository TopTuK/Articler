using Articler.AppDomain.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainInterfaces.Chat
{
    [GenerateSerializer]
    [Alias("Articler.GrainInterfaces.Chat.ChatMessageReply")]
    public class ChatMessageResponse : GrainChatMessage, IAgentChatMessageResponse
    {
        [Id(2)]
        public string Text {  get; set; } = string.Empty;
    }
}
