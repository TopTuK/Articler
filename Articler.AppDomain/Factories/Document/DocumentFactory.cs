using Articler.AppDomain.Models.Documents;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Factories.Document
{
    public static class DocumentFactory
    {
        [GenerateSerializer]
        public class Document(DocumentType documentType, Guid id, string title) : IDocument
        {
            [Id(0)]
            public DocumentType DocumentType { get; } = documentType;

            [Id(1)]
            public Guid Id { get; } = id;

            [Id(2)]
            public string Title { get; } = title;
        }

        public static IDocument CreateDocument(DocumentType documentType, Guid id, string title)
        {
            return new Document(documentType, id, title);
        }
    }
}
