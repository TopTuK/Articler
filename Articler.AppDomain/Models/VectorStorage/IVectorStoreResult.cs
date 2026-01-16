using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Models.VectorStorage
{
    public interface IVectorStoreResult
    {
        string UserId { get; }
        Guid ProjectId { get; }
        int ChunkCount { get; }
    }
}
