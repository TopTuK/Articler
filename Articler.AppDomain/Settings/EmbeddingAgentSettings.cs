using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Settings
{
    public class EmbeddingAgentSettings
    {
        public const string OpenRouter = "OpenRouterOption";

        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string EmbeddingModel { get; set; } = string.Empty;
        public string ChatModel { get; set; } = string.Empty;
        public int EmbeddingModelDimension { get; set; } = -1;
    }
}
