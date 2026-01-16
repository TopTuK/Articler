using Articler.AppDomain.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainInterfaces.Chat
{
    public static class ChatMessageFactory
    {
        public static IGrainChatMessage CreateUserMessage(string message) => new GrainChatMessage()
        {
            Role = ChatRole.User,
            Content = message
        };

        public static IGrainChatMessage CreateAssistantMessage(string message) => new GrainChatMessage()
        {
            Role = ChatRole.Assistant,
            Content = message
        };

        public static IGrainChatMessage CreateSystemMessage(string message) => new GrainChatMessage()
        {
            Role = ChatRole.System,
            Content = message
        };

        public static IGrainChatMessage CreateMessage(ChatRole role, string message) => new GrainChatMessage()
        {
            Role = role,
            Content = message
        };

        public static IAgentChatMessageResponse CreateMessageResponse(ChatRole role, 
            string message, string text) => new ChatMessageResponse()
        {
            Role = role,
            Content = message,
            Text = text
        };
    }
}
