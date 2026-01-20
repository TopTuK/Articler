using Articler.AppDomain.Models.Chat;
using Articler.AppDomain.Models.Documents;
using Articler.AppDomain.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainInterfaces.Project
{
    public interface IProjectGrain : IGrainWithGuidCompoundKey
    {
        Task<IProject> Create(string title, string description, ProjectLanguage language);
        Task<IProject> Get();
        Task<IProject> Remove();

        Task<IProjectText> GetProjectText();
        Task<IProjectText> SetProjectText(string text);

        Task<IEnumerable<IDocument>> GetDocuments();
        Task<IDocument?> RemoveDocument(Guid documentId);
        Task<IDocument> AddTextDocument(string title, string text);
        Task<IDocument> AddPdfDocument(string title, string url);
    }
}
