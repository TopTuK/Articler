using Articler.AppDomain.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainClasses.Chat
{
    [GenerateSerializer]
    public class ChatHistoryState
    {
        [Id(0)]
        public List<IGrainChatMessage> Messages { get; set; } = [];
        [Id(1)]
        public IGrainChatMessage? FirstMessage { get; set; } = null;
        [Id(3)]
        public string MessageHistory { get; set; } = string.Empty;
    }
}
