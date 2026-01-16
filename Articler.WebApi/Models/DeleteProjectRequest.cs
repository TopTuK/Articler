namespace Articler.WebApi.Models
{
    public class DeleteProjectRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public override string ToString()
        {
            return $"ProjectId={ProjectId}";
        }
    }
}
