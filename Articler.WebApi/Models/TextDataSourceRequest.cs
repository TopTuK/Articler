namespace Articler.WebApi.Models
{
    public class TextDataSourceRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"ProjectId={ProjectId} Title={Title} New TextLength={Text.Length}";
        }
    }
}
