namespace Articler.WebApi.Models
{
    public class PdfDataSourceDocumentRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string PdfUrl { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"ProjectId={ProjectId} Title={Title} Url={PdfUrl}";
        }
    }
}
