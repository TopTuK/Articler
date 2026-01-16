using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Models.VectorStorage
{
    internal record TextChunk
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string ChunkText { get; set; } = string.Empty;
        public int ChunkIndex { get; set; }
        public string OriginalTextHask {  get; set; } = string.Empty;
    }
}
