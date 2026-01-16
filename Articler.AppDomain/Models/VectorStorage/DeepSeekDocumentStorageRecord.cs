using Microsoft.Extensions.VectorData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Models.VectorStorage
{
    public sealed class DeepSeekDocumentStorageRecord
    {
        // Unique key for the stored chunk (Guid is required by Qdrant)
        [VectorStoreKey]
        public required Guid Id { get; init; }

        [VectorStoreData(IsIndexed = true)]
        public required string UserId { get; init; }

        [VectorStoreData(IsIndexed = true)]
        public required string ProjectId { get; init; }

        [VectorStoreData(IsIndexed = true)]
        public required string DocumentId { get; init; }

        [VectorStoreData(IsFullTextIndexed = true)]
        public required string Title { get; init; }

        [VectorStoreData(IsFullTextIndexed = true)]
        public required string TextChunk { get; init; }

        [VectorStoreVector(Dimensions: 768, DistanceFunction = DistanceFunction.CosineDistance)]
        public ReadOnlyMemory<float> DeepSeekEmbeddings { get; set; }
    }
}
