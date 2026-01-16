using Articler.AppDomain.Factories.Auth;
using Articler.AppDomain.Models.Auth;
using Articler.GrainInterfaces.User;
using System.Security.Authentication;
using System.Security.Claims;

namespace Articler.WebApi.Services.Auth
{
    public class AuthenticationService : IAuthenticationService
    {
        private record AuthInfo
        {
            public string? Sub { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Email { get; set; }
        }

        private readonly ILogger<AuthenticationService> _logger;
        private readonly string _oidcSchemeName;

        private readonly IClusterClient _clusterClient;

        public AuthenticationService(
            ILogger<AuthenticationService> logger, 
            IConfiguration configuration,
            IClusterClient clusterClient)
        {
            _logger = logger;

            _oidcSchemeName = configuration["Auth:Oidc:Name"] ?? "oidc";
            _clusterClient = clusterClient;
        }

        private static AuthInfo GetOidcAuthInfo(IEnumerable<Claim> claims)
        {
            var sub = claims
                .FirstOrDefault(c => c.Type == "sub")?
                .Value;
            var name = claims
                .FirstOrDefault(c => c.Type == "name")?
                .Value;
            var email = claims
                .FirstOrDefault(c => c.Type == "email")?
                .Value;

            if (name != null)
            {
                var splitName = name.Split(' ');
                var firstName = splitName[0];
                var lastName = string.Empty;

                if (splitName.Length > 1)
                {
                    lastName = splitName[1].Trim();
                }

                return new()
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Sub = sub,
                };
            }

            return new()
            {
                Email = email,
                FirstName = sub ?? "Anonymous",
                LastName = string.Empty,
                Sub = sub,
            };
        }

        private AuthInfo GetAuthInfo(string schemeName, IEnumerable<Claim> claims)
        {
            _logger.LogInformation("AuthenticationService::GetAuthInfo: begin get user auth info. Scheme name={schemeName}",
                schemeName);

            if (schemeName == _oidcSchemeName)
            {
                return GetOidcAuthInfo(claims);
            }
            else
            {
                _logger.LogCritical("AuthenticationService::GetAuthInfo: Unknown scheme name \"{schemeName}\"", schemeName);
                throw new AuthenticationException($"Unknown scheme name \"{schemeName}\"");
            }
        }

        public async Task<IUserProfile> AuthenticateAsync(
            string schemeName,
            IEnumerable<Claim> claims, 
            IDictionary<string, string> metadata)
        {
            _logger.LogInformation("AuthenticationService::AuthenticateAsync: Start authenticating user with scheme \"{schemeName}\"",
                schemeName);

            var authInfo = GetAuthInfo(schemeName, claims);
            _logger.LogInformation("AuthenticationService::AuthenticateAsync: " +
                "UserInfo: {usrEmail} {usrFirstName} {usrLastName}",
                authInfo.Email, authInfo.FirstName, authInfo.LastName);

            if (authInfo.Email == null)
            {
                _logger.LogError("AuthenticationService::AuthenticateAsync: user email claim is null");
                throw new AuthenticationException("User email claim is null");
            }

            try
            {
                _logger.LogInformation("AuthenticationService::AuthenticateAsync: remote call IUserGrain:Authenticate. " +
                    "Email={email}, FirstName={firstName}, LastName={lastName}",
                    authInfo.Email, authInfo.FirstName, authInfo.LastName);
                var userGrain = _clusterClient.GetGrain<IUserGrain>(authInfo.Email);
                var user = await userGrain.Authenticate(
                    authInfo.Email,
                    authInfo.FirstName ?? "Writer", 
                    authInfo.LastName ?? string.Empty
                );

                _logger.LogInformation("AuthenticationService::AuthenticateAsync: got authenticated user. " +
                    "Email={email}, FirstName={firstName}, AccountType={accountType}",
                    user.Email, user.FirstName, user.AccountType);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("AuthenticationService::AuthenticateAsync: exception raised. Msg: {exMsg}", ex.Message);
                throw new AuthenticationException("Inner exception raised.", ex);
            }
        }

        public async Task<IUserProfile> GetUserAsync(string email)
        {
            _logger.LogInformation("AuthenticationService::GetUserAsync: start get user. " +
                "UserId={email}", email);

            try
            {
                var userGrain = _clusterClient.GetGrain<IUserGrain>(email);
                var user = await userGrain.GetUser();

                _logger.LogInformation("AuthenticationService::GetUserAsync: return user information. " +
                    "Email={email} FirstName={firstName} AccountType={accountType}",
                    user.Email, user.FirstName, user.AccountType);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("AuthenticationService::GetUserAsync: exception raised. Msg: {exMsg}", ex.Message);
                throw new AuthenticationException("Inner exception raised.", ex);
            }
        }

        public async Task<IUserProfile> UpdateUserAsync(string email, string firstName, string lastName)
        {
            _logger.LogInformation("AuthenticationService::UpdateUserAsync: start update user information. " +
                "UserId={email}, FirstName={firstName}, LastName={lastName}", email, firstName, lastName);

            if (string.IsNullOrEmpty(firstName))
            {
                _logger.LogCritical("AuthenticationService::UpdateUserAsync: first name can\'t be empty. " +
                    "UserId={email}, FirstName={firstName}, LastName={lastName}", email, firstName, lastName);
                throw new ArgumentException("first name can't be empty.", nameof(firstName));
            }

            try
            {
                var userGrain = _clusterClient.GetGrain<IUserGrain>(email);
                var user = await userGrain.UpdateUser(firstName, lastName);

                _logger.LogInformation("AuthenticationService::UpdateUserAsync: return user information. " +
                    "Email={email} FirstName={firstName} AccountType={accountType}",
                    user.Email, user.FirstName, user.AccountType);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("AuthenticationService::UpdateUserAsync: exception raised. Msg: {exMsg}", ex.Message);
                throw new AuthenticationException("Inner exception raised.", ex);
            }
        }
    }
}
