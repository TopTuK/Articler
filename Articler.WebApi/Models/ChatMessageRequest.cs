namespace Articler.WebApi.Models
{
    public class ChatMessageRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"ProjectId={ProjectId} MessageLength={Message.Length}";
        }
    }
}
