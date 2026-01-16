using System.Security.Claims;

namespace Articler.WebApi.Services.Auth
{
    public interface IJwtSecurityService
    {
        string CreateJwtToken(IList<Claim> claims);
    }
}
