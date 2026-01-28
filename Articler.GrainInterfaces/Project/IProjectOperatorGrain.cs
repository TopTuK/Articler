using Articler.AppDomain.Models.Documents;
using Articler.AppDomain.Models.Project;
using Articler.AppDomain.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainInterfaces.Project
{
    public interface IProjectOperatorGrain : IGrainWithIntegerKey
    {
        Task<IProject?> GetProject(string userId, string projectId);

        Task<ICalculateTokenResult<IDocument>> AddProjectTextDocumentAsync(string userId, string projectId, string title, string text);
        Task<ICalculateTokenResult<IDocument>> AddProjectPdfDocumentAsync(string userId, string projectId, string title, string url);
    }
}
