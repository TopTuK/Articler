namespace Articler.WebApi.Models
{
    public class RemoveDocumentRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string DocumentId { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"ProjectId={ProjectId} DocumentId={DocumentId}";
        }
    }
}
