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
        [ResponseTimeout("00:10:00")]
        Task<ICalculateTokenResult<IDocument>> AddTextDocument(string title, string text);
        
        [ResponseTimeout("00:10:00")]
        Task<ICalculateTokenResult<IDocument>> AddPdfDocument(string title, string url);
        
        //[ResponseTimeout("00:10:00")]
        //Task<IDocument> RemoveDocument(IDocument document);

        [ResponseTimeout("00:10:00")]
        Task<IDocument?> RemoveDocument(string documentId);

        [ResponseTimeout("00:10:00")]
        Task<IDocument?> RemoveDocumentFromStorage(IDocument document);

        Task<IEnumerable<IDocument>> GetDocuments();
    }
}
