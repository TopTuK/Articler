using Articler.AppDomain.Models.Auth;
using System.Security.Claims;

namespace Articler.WebApi.Services.Auth
{
    public interface IAuthenticationService
    {
        Task<IUserProfile> AuthenticateAsync(string schemeName, 
            IEnumerable<Claim> claims, IDictionary<string, string> metadata);

        Task<IUserProfile> GetUserAsync(string email);
        Task<IUserProfile> UpdateUserAsync(string email, string firstName, string lastName);
    }
}
