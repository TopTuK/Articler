namespace Articler.WebApi.Middlewares
{
    public class CheckUserPipeline
    {
        public void Configure(IApplicationBuilder builder)
        {
            builder.UseMiddleware<CheckUserMiddleware>();
        }
    }
}
