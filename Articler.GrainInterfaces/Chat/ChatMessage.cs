using Articler.AppDomain.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainInterfaces.Chat
{
    [GenerateSerializer]
    [Alias("Articler.GrainInterfaces.Chat.ChatMessage")]
    public class GrainChatMessage : IGrainChatMessage
    {
        [Id(0)]
        public ChatRole Role { get; set; }
        [Id(1)]
        public string Content { get; set; } = string.Empty;
    }
}
