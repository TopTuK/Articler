using Articler.AppDomain.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainClasses.Chat
{
    [GenerateSerializer]
    [Alias("Articler.GrainClasses.Chat.GrainChatMessage")]
    public class GrainChatMessage
    {
        [Id(0)]
        public int Role { get; set; } = -1;
        [Id(1)]
        public string Message { get; set; } = string.Empty;
    }
}
