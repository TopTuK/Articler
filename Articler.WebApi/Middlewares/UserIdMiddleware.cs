using System.Security.Claims;

namespace Articler.WebApi.Middlewares
{
    // Finds user email and puts it to context
    public class UserIdMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (true == context.User.Identity?.IsAuthenticated)
            {
                var userId = context.User.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Authorization is not found\"");
                    return;
                }

                context.Items["UserId"] = userId;
            }

            await next(context);
        }
    }
}
