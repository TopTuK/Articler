using Articler.AppDomain.Models.Documents;

namespace Articler.WebApi.Services.DataSource
{
    public interface IDocumentService
    {
        Task<IEnumerable<IDocument>?> GetProjectDocumentsAsync(string userId, string projectId);
        Task<IDocument?> AddProjectTextDocumentAsync(string userId, string projectId, string title, string text);
        Task<IDocument?> AddProjectPdfDocumentAsync(string userId, string projectId, string title, string url);
        Task<IDocument?> RemoveProjectDocumentAsync(string userId, string projectId, string documentId);
    }
}
