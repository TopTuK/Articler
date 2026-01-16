using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Settings
{
    public class QdrantSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string CollectionName { get; set; } = string.Empty;
        public string OpenAICollectionName { get; set; } = string.Empty;
        public string DeepSeekCollectionName { get; set; } = string.Empty;
    }
}
