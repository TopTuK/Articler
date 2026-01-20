using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Services.Document
{
    public interface IPdfDocumentService
    {
        Task<string> DownloadAndParsePdfDocumentAsync(string url);
    }
}
