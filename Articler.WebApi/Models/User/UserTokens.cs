namespace Articler.WebApi.Models.User
{
    public class UserTokens
    {
        public string Email { get; set; } = string.Empty;
        public int TokenCount { get; set; } = 0;
    }
}
