using Articler.WebApi.Middlewares;
using Articler.WebApi.Models;
using Articler.WebApi.Services.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans.Runtime;

namespace Articler.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ChatController(
        ILogger<ChatController> logger,
        IChatService chatService
        ) : ControllerBase
    {
        private readonly ILogger<ChatController> _logger = logger;
        private readonly IChatService _chatService = chatService;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetChatHistory(string projectId)
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("ChatController::GetChatHistory: start get chat history. " +
                "UserId={userId}, ProjectId={projectId}", userId, projectId);

            try
            {
                var history = await _chatService.GetChatHistoryAsync(userId, projectId);
                if (history == null)
                {
                    _logger.LogError("ChatController::GetChatHistory: can\'t find history. " +
                        "UserId={userId}, ProjectId={projectId}", userId, projectId);
                    return NotFound();
                }

                _logger.LogInformation("ChatController::GetChatHistory: return chat history. " +
                    "UserId={userId}, ProjectId={projectId} MessageCount={messageCount}",
                    userId, projectId, history.Count());
                return new JsonResult(history);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ChatController::GetChatHistory: exception raised. " +
                    "UserId={userId}, ProjectId={projectId}. Message={exMsg}", userId, projectId, ex.Message);
                return BadRequest("Exception raised");
            }
        }

        [Authorize]
        [HttpPost]
        [MiddlewareFilter<CheckUserPipeline>]
        public async Task<IActionResult> SendChatMessage(ChatMessageRequest request)
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("ChatController::SendChatMessage: received chat messages. " +
                "UserId={userId}, Message=[{chatMessage}]", userId, request);

            try
            {
                var chatResponse = await _chatService.SendChatMessageAsync(userId, request.ProjectId, request.Message);

                _logger.LogInformation("ChatController::SendChatMessage: got chat response. " +
                    "UserId={userId} ProjectId={projectId}. ChatResult={chatResult}",
                    userId, request.ProjectId, chatResponse.Result);

                switch (chatResponse.Result)
                {
                    case AppDomain.Models.Chat.MessageResult.Success:
                        return new JsonResult(chatResponse);
                    case AppDomain.Models.Chat.MessageResult.NotFound:
                        return NotFound();
                    case AppDomain.Models.Chat.MessageResult.Error:
                    case AppDomain.Models.Chat.MessageResult.NotEnoughTokens:
                    default:
                        return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ChatController::SendChatMessage: exception raised. " +
                    "Message={exMsg}", ex.Message);
                return BadRequest("Exception raised");
            }
        }
    }
}
