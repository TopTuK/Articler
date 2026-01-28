using Articler.AppDomain.Models.Documents;
using Articler.AppDomain.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainInterfaces.Document
{
    public interface IDocumentStorageGrain : IGrainWithGuidCompoundKey
    {
        Task<ICalculateTokenResult<IDocument>> AddTextDocument(string title, string text);
        Task<ICalculateTokenResult<IDocument>> AddPdfDocument(string title, string url);
        Task<IDocument> RemoveDocument(IDocument document);
    }
}
