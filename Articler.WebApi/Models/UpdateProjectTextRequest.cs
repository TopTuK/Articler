namespace Articler.WebApi.Models
{
    public class UpdateProjectTextRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"ProjectId={ProjectId} New TextLength={Text.Length}";
        }
    }
}
