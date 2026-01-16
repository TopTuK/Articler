using Articler.AppDomain.Models.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainInterfaces.Document
{
    public interface IDocumentStorageGrain : IGrainWithGuidCompoundKey
    {
        Task<IDocument> AddTextDocument(string title, string text);
        Task<IDocument> RemoveDocument(IDocument document);
    }
}
