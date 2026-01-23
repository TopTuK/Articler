using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainInterfaces.Document
{
    public interface IDocumentAgentGrain : IGrainWithGuidCompoundKey
    {
        Task<IEnumerable<string>> SearchDocuments(string query);
    }
}
