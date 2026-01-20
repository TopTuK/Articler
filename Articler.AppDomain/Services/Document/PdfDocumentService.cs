using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace Articler.AppDomain.Services.Document
{
    public class PdfDocumentService(
        ILogger<PdfDocumentService> logger,
        IHttpClientFactory httpClientFactory) : IPdfDocumentService
    {
        private readonly ILogger<PdfDocumentService> _logger = logger;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<string> DownloadAndParsePdfDocumentAsync(string url)
        {
            _logger.LogInformation("PdfDocumentService::DownloadAndParsePdfDocumentAsync: start downloading pdf document by url. " +
                "Url={url}", url);

            try
            {
                var client = _httpClientFactory.CreateClient();

                _logger.LogInformation("PdfDocumentService::DownloadAndParsePdfDocumentAsync: start get bytes with HttpClient. " +
                    "Url={url}", url);
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("PdfDocumentService::DownloadAndParsePdfDocumentAsync: got response by url. " +
                    "Url={url}", url);
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();

                _logger.LogInformation("PdfDocumentService::DownloadAndParsePdfDocumentAsync: got bytes by response. " +
                    "Url={url} BytesLength={byteLength}", url, pdfBytes.Length);
                using (var document = PdfDocument.Open(pdfBytes))
                {
                    var fullText = new StringBuilder();
                    foreach (var page in document.GetPages())
                    {
                        fullText.Append(page.Text);
                    }

                    _logger.LogInformation("PdfDocumentService::DownloadAndParsePdfDocumentAsync: extracted text from PDF. " +
                        "Url={url} TextLength={textLengt}", url, fullText.Length);
                    return fullText.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("PdfDocumentService::DownloadAndParsePdfDocumentAsync: exception raised. " +
                    "Url={url} Message={exMsg}", url, ex.Message);
                throw;
            }
        }
    }
}
