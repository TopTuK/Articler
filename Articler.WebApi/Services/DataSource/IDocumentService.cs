using Articler.AppDomain.Models.Documents;
using Articler.AppDomain.Models.Token;

namespace Articler.WebApi.Services.DataSource
{
    public interface IDocumentService
    {
        Task<IEnumerable<IDocument>?> GetProjectDocumentsAsync(string userId, string projectId);
        Task<IDocument?> RemoveProjectDocumentAsync(string userId, string projectId, string documentId);

        Task<ICalculateTokenResult<IDocument>> AddProjectTextDocumentAsync(string userId, string projectId, string title, string text);
        Task<ICalculateTokenResult<IDocument>> AddProjectPdfDocumentAsync(string userId, string projectId, string title, string url);
    }
}
