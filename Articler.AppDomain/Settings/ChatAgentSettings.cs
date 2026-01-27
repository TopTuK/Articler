using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Settings
{
    public class ChatAgentSettings
    {
        public const string OpenAI = "OpenAIOptions";
        public const string DeepSeek = "DeepSeekOptions";

        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ChatModel { get; set; } = string.Empty;
    }
}
