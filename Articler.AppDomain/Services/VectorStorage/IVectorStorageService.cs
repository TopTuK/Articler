using Articler.AppDomain.Models.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Services.VectorStorage
{
    public interface IVectorStorageService
    {
        Task EnsureCollectionExist(CancellationToken cancellationToken = default);
        Task<IDocument> StoreTextAsync(string userId, Guid projectId, Guid documentId, string title, string text);
        Task<IEnumerable<string>> SearchDocumentsAsync(string query, string userId, Guid projectId,
            int top = 3, Guid? documentId = null, string? documentTitle = null);
        Task<IDocument?> RemoveDocumentAsync(string userId, Guid projectId, Guid documentId);
    }
}
