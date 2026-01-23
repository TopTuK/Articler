using Articler.AppDomain.Models.Chat;

namespace Articler.WebApi.Models
{
    public class ChatMessageRequest
    {
        public string ProjectId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ChatMode Mode { get; set; } = ChatMode.Write;

        public override string ToString()
        {
            return $"ProjectId={ProjectId} Mode={Mode} MessageLength={Message.Length}";
        }
    }
}
