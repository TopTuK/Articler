using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainClasses.Chat
{
    [GenerateSerializer]
    public class AgentHistoryState
    {
        [Id(0)]
        public string FirstMessage { get; set; } = string.Empty;
        [Id(1)]
        public string MessageHistory { get; set; } = string.Empty;
    }
}
