namespace Articler.WebApi.Middlewares
{
    public class UserAuthMiddleware(IConfiguration configuration) : IMiddleware
    {
        private readonly string _cookieName = configuration["Auth:AuthDefaultSchemeName"] ?? "cookie";

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var token = context.Request.Cookies[_cookieName];
            if (!string.IsNullOrEmpty(token))
            {
#pragma warning disable ASP0019 // Suggest using IHeaderDictionary.Append or the indexer
                context.Request
                    .Headers
                    .Add("Authorization", "Bearer " + token);
#pragma warning restore ASP0019 // Suggest using IHeaderDictionary.Append or the indexer
            }

            return next(context);
        }
    }
}
