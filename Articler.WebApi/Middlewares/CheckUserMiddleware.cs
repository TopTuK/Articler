using Articler.AppDomain.Models.Auth;
using Articler.GrainInterfaces.User;

namespace Articler.WebApi.Middlewares
{
    public class CheckUserMiddleware(
            ILogger<CheckUserMiddleware> logger,
            IClusterClient clusterClient
        ) : IMiddleware
    {
        private readonly ILogger<CheckUserMiddleware> _logger = logger;
        private readonly IClusterClient _clusterClient = clusterClient;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var userId = context.Items["UserId"]?.ToString();

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("CheckUserMiddleware::InvokeAsync: userId is null or empty");

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                var errorResponse = new { error = "UserId is required" };
                await context.Response.WriteAsJsonAsync(errorResponse);

                return;
            }

            var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
            var profile = await userGrain.GetUser();

            var accountTypes = new AccountType[]
            {
                AccountType.Paid,
                AccountType.Trial,
                AccountType.Super,
            };

            if (!accountTypes.Contains(profile.AccountType))
            {
                _logger.LogError("CheckUserMiddleware::InvokeAsync: user can\'t execute action. " +
                    "UserId={userId}, AccountType={accountType}", userId, profile.AccountType);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                var errorResponse = new { error = "UserId is required" };
                await context.Response.WriteAsJsonAsync(errorResponse);

                return;
            }

            await next(context);
        }
    }
}
