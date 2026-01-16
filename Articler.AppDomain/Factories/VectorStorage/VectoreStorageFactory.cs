using Articler.AppDomain.Models.VectorStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Factories.VectorStorage
{
    public static class VectoreStorageFactory
    {
        public class VectorStoreResult(string userId, Guid projectId, int chunkCount) : IVectorStoreResult
        {
            public string UserId { get; } = userId;
            public Guid ProjectId { get; } = projectId;
            public int ChunkCount { get; } = chunkCount;
        }

        public static IVectorStoreResult CreateVectorStoreResult(string userId, Guid projectId, int chunkCount)
        {
            return new VectorStoreResult(userId, projectId, chunkCount);
        }
    }
}
