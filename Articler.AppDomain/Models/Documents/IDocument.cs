using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Models.Documents
{
    public interface IDocument
    {
        DocumentType DocumentType { get; }
        Guid Id { get; }
        string Title { get; }
    }
}
